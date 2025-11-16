// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides metadata for assemblies in a specified universe.
    /// </summary>
    /// <remarks>
    /// This class uses <see cref="MetadataLoadContext"/> to load assemblies for metadata purposes only.
    /// <para>
    /// Callers must pass absolute file paths to assemblies that should be resolvable (e.g., core/runtime
    /// assemblies, the target assembly, and any third-party dependencies or reference assemblies).
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public sealed class MetadataUniverse : IDisposable
    {
        private readonly MetadataLoadContext mlc;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataUniverse"/> class.
        /// </summary>
        /// <param name="assemblyPaths">The assembly file paths to be used by the resolver.</param>
        /// <param name="includeTrustedPlatformAssemblies">Indicates whether to include trusted platform assemblies.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assemblyPaths"/> is <see langword="null"/>.</exception>
        public MetadataUniverse(IEnumerable<string> assemblyPaths, bool includeTrustedPlatformAssemblies = true)
        {
            if (assemblyPaths is null)
                throw new ArgumentNullException(nameof(assemblyPaths));

            var coreAssembly = typeof(object).Assembly;
            var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                coreAssembly.Location,
            };

            if (includeTrustedPlatformAssemblies && AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string tpa)
            {
                var trusted = tpa.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
                candidates.UnionWith(trusted.Where(File.Exists));
            }

            candidates.UnionWith(assemblyPaths.Select(Path.GetFullPath).Where(File.Exists));

            var resolver = new PathAssemblyResolver(candidates);
            mlc = new MetadataLoadContext(resolver, coreAssembly.GetName().Name);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataUniverse"/> class with probe folders.
        /// </summary>
        /// <param name="assemblyPaths">The assembly file paths to be used by the resolver.</param>
        /// <param name="probeFolders">Additional folders to search for assemblies on-demand.</param>
        /// <param name="includeTrustedPlatformAssemblies">Indicates whether to include trusted platform assemblies.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assemblyPaths"/> or <paramref name="probeFolders"/> is <see langword="null"/>.</exception>
        private MetadataUniverse(IEnumerable<string> assemblyPaths, IEnumerable<string> probeFolders, bool includeTrustedPlatformAssemblies)
        {
            if (assemblyPaths is null)
                throw new ArgumentNullException(nameof(assemblyPaths));
            if (probeFolders is null)
                throw new ArgumentNullException(nameof(probeFolders));

            var coreAssembly = typeof(object).Assembly;
            var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                coreAssembly.Location,
            };

            if (includeTrustedPlatformAssemblies && AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string tpa)
            {
                var trusted = tpa.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
                candidates.UnionWith(trusted.Where(File.Exists));
            }

            candidates.UnionWith(assemblyPaths.Select(Path.GetFullPath).Where(File.Exists));

            var resolver = new FolderAssemblyResolver(candidates, probeFolders);
            mlc = new MetadataLoadContext(resolver, coreAssembly.GetName().Name);
        }

        /// <summary>
        /// Creates a <see cref="MetadataUniverse"/> by scanning folders for assemblies.
        /// </summary>
        /// <param name="probeFolders">Folders to scan for <c>*.dll</c> recursively.</param>
        /// <param name="includeTrustedPlatformAssemblies">Indicates whether to include trusted platform assemblies.</param>
        /// <returns>A new instance of <see cref="MetadataUniverse"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="probeFolders"/> is <see langword="null"/>.</exception>
        public static MetadataUniverse FromProbeFolders(IEnumerable<string> probeFolders, bool includeTrustedPlatformAssemblies = true)
        {
            if (probeFolders is null)
                throw new ArgumentNullException(nameof(probeFolders));

            var searchFolders = new HashSet<string>(probeFolders.Where(Directory.Exists), StringComparer.OrdinalIgnoreCase);

            var nugetPackages = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
            if (!string.IsNullOrEmpty(nugetPackages) && Directory.Exists(nugetPackages))
                searchFolders.Add(nugetPackages);

            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrEmpty(userProfile))
            {
                var defaultCache = Path.Combine(userProfile, ".nuget", "packages");
                if (Directory.Exists(defaultCache))
                    searchFolders.Add(defaultCache);
            }

            var files = probeFolders
                .Where(Directory.Exists)
                .SelectMany(dir => Directory.EnumerateFiles(dir, "*.dll", SearchOption.AllDirectories));

            return new MetadataUniverse(files, searchFolders, includeTrustedPlatformAssemblies);
        }

        /// <summary>
        /// Loads an assembly from the specified path (metadata-only).
        /// </summary>
        /// <param name="assemblyPath">The absolute or relative path to the assembly to load.</param>
        /// <returns>The loaded assembly.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="assemblyPath"/> is <see langword="null"/> or empty.</exception>
        public Assembly LoadFromPath(string assemblyPath)
        {
            if (string.IsNullOrEmpty(assemblyPath))
                throw new ArgumentException($"'{nameof(assemblyPath)}' cannot be null or empty.", nameof(assemblyPath));

            var fullPath = Path.GetFullPath(assemblyPath);
            return mlc.LoadFromAssemblyPath(fullPath);
        }

        /// <summary>
        /// Releases resources used by the <see cref="MetadataUniverse"/>.
        /// </summary>
        public void Dispose() => mlc.Dispose();

        /// <summary>
        /// A custom assembly resolver that searches in specified directories.
        /// </summary>
        private sealed class FolderAssemblyResolver : MetadataAssemblyResolver
        {
            private static readonly Version ZeroVersion = new(0, 0, 0, 0);

            private readonly PathAssemblyResolver pathResolver;
            private readonly List<string> probeFolders;
            private readonly ConcurrentDictionary<string, Assembly?> cache = new(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// Initializes a new instance of the <see cref="FolderAssemblyResolver"/> class.
            /// </summary>
            /// <param name="assemblyPaths">The assembly file paths to be used by the resolver.</param>
            /// <param name="searchFolders">Additional folders to search for assemblies on-demand.</param>
            public FolderAssemblyResolver(IEnumerable<string> assemblyPaths, IEnumerable<string> searchFolders)
            {
                pathResolver = new PathAssemblyResolver(assemblyPaths);
                probeFolders = [.. searchFolders];
            }

            /// <summary>
            /// Resolves an assembly by its name.
            /// </summary>
            /// <param name="context">The metadata load context.</param>
            /// <param name="assemblyName">The assembly name to resolve.</param>
            /// <returns>The resolved assembly, or <see langword="null"/> if not found.</returns>
            public override Assembly? Resolve(MetadataLoadContext context, AssemblyName assemblyName)
            {
                var name = assemblyName.Name;
                if (name is null)
                    return null;

                return cache.GetOrAdd(name, _ => pathResolver.Resolve(context, assemblyName)
                                              ?? FindInAppDomain(assemblyName)
                                              ?? FindAssemblyInProbeFolders(context, assemblyName));
            }

            /// <summary>
            /// Searches for an assembly in the current AppDomain.
            /// </summary>
            /// <param name="assemblyName">The assembly name to find.</param>
            /// <returns>The found assembly, or <see langword="null"/> if not found.</returns>
            private static Assembly? FindInAppDomain(AssemblyName assemblyName)
            {
                return AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(assemblyName, a.GetName()));
            }

            /// <summary>
            /// Searches for an assembly in the probe folders.
            /// </summary>
            /// <param name="context">The metadata load context.</param>
            /// <param name="assemblyName">The assembly name to find.</param>
            /// <returns>The found assembly, or <see langword="null"/> if not found.</returns>
            private Assembly? FindAssemblyInProbeFolders(MetadataLoadContext context, AssemblyName assemblyName)
            {
                var fileName = assemblyName.Name! + ".dll";
                var candidates = new SortedList<Version, string>(Comparer<Version>.Create((a, b) => b.CompareTo(a)));

                foreach (var folder in probeFolders)
                {
                    var candidatePaths = Directory.EnumerateFiles(folder, fileName, SearchOption.AllDirectories);
                    foreach (var path in candidatePaths)
                    {
                        try
                        {
                            var foundName = AssemblyName.GetAssemblyName(path);
                            if (AssemblyName.ReferenceMatchesDefinition(assemblyName, foundName))
                            {
                                var version = foundName.Version ?? ZeroVersion;
                                candidates.TryAdd(version, path);
                            }
                        }
                        catch
                        {
                            // Skip invalid assemblies
                        }
                    }
                }

                if (candidates.Count == 0)
                    return null;

                // Prioritize exact version match
                if (assemblyName.Version is not null && candidates.TryGetValue(assemblyName.Version, out var exactPath))
                    return context.LoadFromAssemblyPath(exactPath);

                // Fall back to highest compatible version
                return context.LoadFromAssemblyPath(candidates.Values[0]);
            }
        }
    }
}

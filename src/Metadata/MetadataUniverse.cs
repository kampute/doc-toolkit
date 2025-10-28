// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System;
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

            var files = probeFolders
                .Where(Directory.Exists)
                .SelectMany(static f => Directory.EnumerateFiles(f, "*.dll", SearchOption.AllDirectories));

            return new MetadataUniverse(files, includeTrustedPlatformAssemblies);
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
            var fullName = AssemblyName.GetAssemblyName(fullPath).FullName!;

            try
            {
                return mlc.LoadFromAssemblyName(fullName);
            }
            catch (FileNotFoundException)
            {
                return mlc.LoadFromAssemblyPath(fullPath);
            }
            catch (FileLoadException)
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == fullName);
                if (assembly is not null)
                    return assembly;

                throw;
            }
        }

        /// <summary>
        /// Releases resources used by the <see cref="MetadataUniverse"/>.
        /// </summary>
        public void Dispose() => mlc.Dispose();
    }
}

// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    [TestFixture]
    public class MetadataUniverseTests
    {
        private string testDirectory = null!;
        private string testAssemblyPath = null!;
        private List<string> systemAssemblyPaths = null!;

        private static List<string> DiscoverUnloadedSystemAssemblies(int maxAssemblies)
        {
            var assemblyPaths = new List<string>();
            var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
            foreach (var path in Directory.GetFiles(runtimeDir, "System.*.dll"))
            {
                var name = Path.GetFileNameWithoutExtension(path);
                if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == name))
                {
                    assemblyPaths.Add(path);
                    if (assemblyPaths.Count >= maxAssemblies)
                        break;
                }
            }
            return assemblyPaths;
        }

        [SetUp]
        public void SetUp()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(testDirectory);

            // Get system assemblies that are NOT loaded in the current AppDomain
            systemAssemblyPaths = DiscoverUnloadedSystemAssemblies(3);

            if (systemAssemblyPaths.Count != 0)
            {
                // Copy a system assembly to test directory for probe folder tests
                testAssemblyPath = Path.Combine(testDirectory, Path.GetFileName(systemAssemblyPaths[0]));
                File.Copy(systemAssemblyPaths[0], testAssemblyPath, true);
            }
            else
            {
                // Fallback: use the current test assembly
                var currentAssembly = typeof(MetadataUniverseTests).Assembly;
                testAssemblyPath = Path.Combine(testDirectory, Path.GetFileName(currentAssembly.Location));
                File.Copy(currentAssembly.Location, testAssemblyPath, true);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(testDirectory))
            {
                try
                {
                    Directory.Delete(testDirectory, true);
                }
                catch
                {
                    // Best effort cleanup
                }
            }
        }

        [Test]
        public void Constructor_WithNullAssemblyPaths_ThrowsArgumentNullException()
        {
            Assert.That(static () => new MetadataUniverse(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("assemblyPaths"));
        }

        [Test]
        public void Constructor_WithEmptyAssemblyPaths_CreatesInstance()
        {
            var assemblyPaths = Array.Empty<string>();

            using var universe = new MetadataUniverse(assemblyPaths);

            Assert.That(universe, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNonExistentPaths_FiltersThemOut()
        {
            var assemblyPaths = new[]
            {
                testAssemblyPath,
                Path.Combine(testDirectory, "nonexistent.dll")
            };

            using var universe = new MetadataUniverse(assemblyPaths);
            var assembly = universe.LoadFromPath(testAssemblyPath);

            Assert.That(assembly, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithRelativePaths_ConvertsToAbsolute()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var relativePath = Path.GetRelativePath(currentDir, testAssemblyPath);
            var assemblyPaths = new[] { relativePath };

            using var universe = new MetadataUniverse(assemblyPaths);
            var assembly = universe.LoadFromPath(testAssemblyPath);

            Assert.That(assembly, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithIncludeTrustedPlatformAssembliesFalse_CannotResolveReferencedPlatformAssemblies()
        {
            // The test assembly references System.Runtime, which is a trusted platform assembly but not the core assembly.
            var assemblyPath = typeof(MetadataUniverseTests).Assembly.Location;
            using var universe = new MetadataUniverse([assemblyPath], includeTrustedPlatformAssemblies: false);
            var type = universe.LoadFromPath(assemblyPath).GetType(typeof(MetadataUniverseTests).FullName!)!;

            Assert.That(() => type.BaseType, Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void Constructor_WithIncludeTrustedPlatformAssembliesTrue_ResolvesReferencedPlatformAssemblies()
        {
            var assemblyPath = typeof(MetadataUniverseTests).Assembly.Location;
            using var universe = new MetadataUniverse([assemblyPath], includeTrustedPlatformAssemblies: true);
            var type = universe.LoadFromPath(assemblyPath).GetType(typeof(MetadataUniverseTests).FullName!)!;

            Assert.That(type.BaseType?.FullName, Is.EqualTo(typeof(object).FullName));
        }

        [Test]
        public void LoadFromPath_WithNullPath_ThrowsArgumentException()
        {
            using var universe = new MetadataUniverse([testAssemblyPath]);

            Assert.That(() => universe.LoadFromPath(null!), Throws.ArgumentException
                .With.Message.Contain("cannot be null or empty")
                .And.Property("ParamName").EqualTo("assemblyPath"));
        }

        [Test]
        public void LoadFromPath_WithEmptyPath_ThrowsArgumentException()
        {
            using var universe = new MetadataUniverse([testAssemblyPath]);

            Assert.That(() => universe.LoadFromPath(string.Empty), Throws.ArgumentException
                .With.Message.Contain("cannot be null or empty")
                .And.Property("ParamName").EqualTo("assemblyPath"));
        }

        [Test]
        public void LoadFromPath_WithValidPath_LoadsAssemblyFromThatFile()
        {
            using var universe = new MetadataUniverse([testAssemblyPath]);

            var assembly = universe.LoadFromPath(testAssemblyPath);

            Assert.That(assembly.Location, Is.EqualTo(Path.GetFullPath(testAssemblyPath)).IgnoreCase);
        }

        [Test]
        public void LoadFromPath_WithRelativePath_LoadsAssembly()
        {
            var currentDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(testDirectory);

            try
            {
                using var universe = new MetadataUniverse([testAssemblyPath]);
                var relativePath = Path.GetFileName(testAssemblyPath);

                var assembly = universe.LoadFromPath(relativePath);

                Assert.That(assembly, Is.Not.Null);
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDir);
            }
        }

        [Test]
        public void LoadFromPath_CalledMultipleTimes_LoadsSameAssembly()
        {
            using var universe = new MetadataUniverse([testAssemblyPath]);

            var assembly1 = universe.LoadFromPath(testAssemblyPath);
            var assembly2 = universe.LoadFromPath(testAssemblyPath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(assembly1, Is.Not.Null);
                Assert.That(assembly2, Is.Not.Null);
            }

            Assert.That(assembly1.FullName, Is.EqualTo(assembly2.FullName));
        }

        [Test]
        public void LoadFromPath_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            using var universe = new MetadataUniverse([testAssemblyPath]);
            var nonExistentPath = Path.Combine(testDirectory, "nonexistent.dll");

            Assert.That(() => universe.LoadFromPath(nonExistentPath), Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            var universe = new MetadataUniverse([testAssemblyPath]);

            universe.Dispose();

            Assert.That(universe.Dispose, Throws.Nothing);
        }

        [Test]
        public void Dispose_RemovesMetadataOfLoadedAssembliesFromAvailableAssemblies()
        {
            var universe = new MetadataUniverse([testAssemblyPath]);
            var assemblyMetadata = universe.LoadFromPath(testAssemblyPath).GetMetadata();
            Assert.That(MetadataProvider.AvailableAssemblies, Does.Contain(assemblyMetadata), "Precondition: metadata is cached while the universe is alive.");

            universe.Dispose();

            Assert.That(MetadataProvider.AvailableAssemblies, Does.Not.Contain(assemblyMetadata));
        }

        [Test]
        public void Dispose_AfterDispose_LoadFromPathThrowsObjectDisposedException()
        {
            var universe = new MetadataUniverse([testAssemblyPath]);
            universe.Dispose();

            Assert.That(() => universe.LoadFromPath(testAssemblyPath), Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void FromProbeFolders_WithNullProbeFolders_ThrowsArgumentNullException()
        {
            Assert.That(static () => MetadataUniverse.FromProbeFolders(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("probeFolders"));
        }

        [Test]
        public void FromProbeFolders_WithValidFolders_CreatesInstance()
        {
            var probeFolders = new[] { testDirectory };

            using var universe = MetadataUniverse.FromProbeFolders(probeFolders);

            Assert.That(universe, Is.Not.Null);
        }

        [Test]
        public void FromProbeFolders_WithEmptyFolders_CreatesInstance()
        {
            var probeFolders = Array.Empty<string>();

            using var universe = MetadataUniverse.FromProbeFolders(probeFolders);

            Assert.That(universe, Is.Not.Null);
        }

        [Test]
        public void FromProbeFolders_WithNonExistentFolders_FiltersThemOut()
        {
            var probeFolders = new[]
            {
                testDirectory,
                Path.Combine(testDirectory, "nonexistent")
            };

            using var universe = MetadataUniverse.FromProbeFolders(probeFolders);

            Assert.That(universe, Is.Not.Null);
        }

        [Test]
        public void FromProbeFolders_ScansRecursively_FindsNestedAssemblies()
        {
            // Create a nested directory structure
            var subDirectory = Path.Combine(testDirectory, "subfolder", "nested");
            Directory.CreateDirectory(subDirectory);

            // Copy assembly to nested location
            var nestedAssemblyPath = Path.Combine(subDirectory, Path.GetFileName(testAssemblyPath));
            File.Copy(testAssemblyPath, nestedAssemblyPath, true);

            // FromProbeFolders scans recursively and includes found DLLs in the initial assembly paths
            using var universe = MetadataUniverse.FromProbeFolders([testDirectory]);
            var assembly = universe.LoadFromPath(nestedAssemblyPath);

            Assert.That(assembly, Is.Not.Null);
        }

        [Test]
        public void FromProbeFolders_WithIncludeTrustedPlatformAssembliesFalse_CreatesInstance()
        {
            // Whether platform assemblies stay unresolvable cannot be observed here: the probe folders include the local NuGet
            // cache, and the resolver falls back to the assemblies loaded in the application domain.
            var probeFolders = new[] { testDirectory };

            using var universe = MetadataUniverse.FromProbeFolders(probeFolders, includeTrustedPlatformAssemblies: false);

            Assert.That(universe.LoadFromPath(testAssemblyPath).Location, Is.EqualTo(Path.GetFullPath(testAssemblyPath)).IgnoreCase);
        }

        [Test]
        public void MetadataUniverse_LoadsAssemblyMetadataOnly()
        {
            using var universe = new MetadataUniverse([testAssemblyPath]);

            var assembly = universe.LoadFromPath(testAssemblyPath);

            Assert.That(assembly.ReflectionOnly, Is.True);
        }

        [Test]
        public void MetadataUniverse_LoadedAssembly_HasIdentityOfFile()
        {
            using var universe = new MetadataUniverse([testAssemblyPath]);
            var expected = AssemblyName.GetAssemblyName(testAssemblyPath);

            var actual = universe.LoadFromPath(testAssemblyPath).GetName();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(actual.Name, Is.EqualTo(expected.Name));
                Assert.That(actual.Version, Is.EqualTo(expected.Version));
            }
        }

        [Test]
        public void MetadataUniverse_WithMultipleAssemblies_LoadsAll()
        {
            if (systemAssemblyPaths.Count < 2)
            {
                Assert.Inconclusive("Insufficient system assemblies available for this test.");
                return;
            }

            var assemblyPaths = new[] { systemAssemblyPaths[0], systemAssemblyPaths[1] };

            using var universe = new MetadataUniverse(assemblyPaths);

            var assembly1 = universe.LoadFromPath(systemAssemblyPaths[0]);
            var assembly2 = universe.LoadFromPath(systemAssemblyPaths[1]);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(assembly1, Is.Not.Null);
                Assert.That(assembly2, Is.Not.Null);
            }

            Assert.That(assembly1.GetName().Name, Is.Not.EqualTo(assembly2.GetName().Name));
        }

        [Test]
        public void MetadataUniverse_ResolvesSystemAssemblies()
        {
            using var universe = new MetadataUniverse([testAssemblyPath]);
            var assembly = universe.LoadFromPath(testAssemblyPath);

            // Resolving base types requires loading the assemblies the loaded assembly references.
            Assert.That(() => assembly.GetTypes().Select(static t => t.BaseType).ToList(), Throws.Nothing);
        }
    }
}

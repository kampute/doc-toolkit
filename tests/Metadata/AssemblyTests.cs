// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using NUnit.Framework.Internal;
    using System.Linq;

    [TestFixture]
    public class AssemblyTests
    {
        [Test]
        public void Name_HasExpectedValue()
        {
            var testAssembly = typeof(AssemblyTests).Assembly;
            var assemblyMetadata = testAssembly.GetMetadata();

            Assert.That(assemblyMetadata.Name, Is.EqualTo(testAssembly.GetName().Name));
        }

        [Test]
        public void Identity_HasExpectedValue()
        {
            var testAssembly = typeof(AssemblyTests).Assembly;
            var assemblyMetadata = testAssembly.GetMetadata();

            var expected = testAssembly.GetName();
            var actual = assemblyMetadata.Identity;

            Assert.That(actual, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(actual.Name, Is.EqualTo(expected.Name));
                Assert.That(actual.Version, Is.EqualTo(expected.Version));
                Assert.That(actual.CultureName, Is.EqualTo(expected.CultureName));
                Assert.That(actual.GetPublicKeyToken(), Is.EqualTo(expected.GetPublicKeyToken()));
            }
        }

        [Test]
        public void Modules_HasExpectedCollection()
        {
            var testAssembly = typeof(AssemblyTests).Assembly;
            var assemblyMetadata = testAssembly.GetMetadata();

            Assert.That(assemblyMetadata.Modules, Is.Not.Empty);
        }

        [Test]
        public void ReferencedAssemblies_HasExpectedCollection()
        {
            var testAssembly = typeof(AssemblyTests).Assembly;
            var assemblyMetadata = testAssembly.GetMetadata();

            var systemRuntime = assemblyMetadata.ReferencedAssemblies.FirstOrDefault(static a => a.Name == "System.Runtime");
            Assert.That(systemRuntime, Is.Not.Null);
        }

        [Test]
        public void Namespaces_HasExpectedCollection()
        {
            var testAssembly = typeof(AssemblyTests).Assembly;
            var assemblyMetadata = testAssembly.GetMetadata();

            Assert.That(assemblyMetadata.Namespaces.Select(static ns => ns.Key), Does.Contain(typeof(AssemblyTests).Namespace));
        }

        [Test]
        public void ExportedTypes_HasExpectedCollection()
        {
            var testAssembly = typeof(AssemblyTests).Assembly;
            var assemblyMetadata = testAssembly.GetMetadata();

            Assert.That(assemblyMetadata.ExportedTypes.Select(static a => a.Name), Does.Contain(nameof(AssemblyTests)));
            Assert.That(assemblyMetadata.ExportedTypes.Select(static a => a.Name), Does.Not.Contain(nameof(MockHelper)));
        }

        [Test]
        public void Attributes_HasExpectedCollection()
        {
            var testAssembly = typeof(AssemblyTests).Assembly;
            var assemblyMetadata = testAssembly.GetMetadata();

            Assert.That(assemblyMetadata.Attributes, Is.Not.Empty);
        }

        [Test]
        public void Represents_WithSameAssembly_ReturnsTrue()
        {
            var testAssembly = typeof(AssemblyTests).Assembly;
            var assemblyMetadata = testAssembly.GetMetadata();

            Assert.That(assemblyMetadata.Represents(testAssembly), Is.True);
        }

        [Test]
        public void Represents_WithDifferentAssembly_ReturnsFalse()
        {
            var testAssembly = typeof(AssemblyTests).Assembly;
            var assemblyMetadata = testAssembly.GetMetadata();

            Assert.That(assemblyMetadata.Represents(typeof(object).Assembly), Is.False);
        }

        [Test]
        public void TryGetType_WithExportedByAssembly_ReturnsTrueAndTypeMetadata()
        {
            var testAssembly = typeof(AssemblyTests).Assembly;
            var assemblyMetadata = testAssembly.GetMetadata();

            var typeName = typeof(AssemblyTests).FullName;
            var found = assemblyMetadata.TryGetType(typeName!, out var typeMetadata);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(found, Is.True);
                Assert.That(typeMetadata?.Name, Is.EqualTo(nameof(AssemblyTests)));
            }
        }

        [Test]
        public void TryGetType_WithTypeNotExportedByAssembly_ReturnsFalseAndNull()
        {
            var testAssembly = typeof(AssemblyTests).Assembly;
            var assemblyMetadata = testAssembly.GetMetadata();

            var found = assemblyMetadata.TryGetType("NonExistentType", out var typeMetadata);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(found, Is.False);
                Assert.That(typeMetadata, Is.Null);
            }
        }
    }
}

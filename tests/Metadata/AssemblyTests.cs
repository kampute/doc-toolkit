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
    using System.Linq;
    using System.Reflection;

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
        public void Modules_ContainsManifestModule()
        {
            var testAssembly = typeof(AssemblyTests).Assembly;
            var assemblyMetadata = testAssembly.GetMetadata();

            Assert.That(assemblyMetadata.Modules, Does.Contain(testAssembly.ManifestModule));
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
        public void Namespaces_AreOrderedByNameWithTypesOrderedByFullName()
        {
            var assemblyMetadata = typeof(AssemblyTests).Assembly.GetMetadata();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(assemblyMetadata.Namespaces.Keys, Is.Ordered.Using((IComparer<string>)StringComparer.Ordinal));
                foreach (var (ns, types) in assemblyMetadata.Namespaces)
                    Assert.That(types.Select(static t => t.FullName), Is.Ordered.Using((IComparer<string>)StringComparer.Ordinal), ns);
            }
        }

        [Test]
        public void ExportedTypes_AreOrderedByFullName()
        {
            var assemblyMetadata = typeof(AssemblyTests).Assembly.GetMetadata();

            Assert.That(assemblyMetadata.ExportedTypes.Select(static t => t.FullName), Is.Ordered.Using((IComparer<string>)StringComparer.Ordinal));
        }

        [Test]
        public void Attributes_MapsAssemblyAttributesByNameIgnoringCase()
        {
            var assembly = typeof(MetadataProvider).Assembly;
            var assemblyMetadata = assembly.GetMetadata();

            var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()!.Title;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(assemblyMetadata.Attributes["Title"], Is.EqualTo(title));
                Assert.That(assemblyMetadata.Attributes["title"], Is.EqualTo(title));
                Assert.That(assemblyMetadata.Attributes.Keys, Has.None.EqualTo("Metadata"));
            }
        }

        [Test]
        public void Attributes_MapsAssemblyMetadataAttributesByKey()
        {
            var assembly = typeof(MetadataProvider).Assembly;
            var assemblyMetadata = assembly.GetMetadata();

            var repositoryUrl = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(static a => a.Key == "RepositoryUrl").Value;

            Assert.That(assemblyMetadata.Attributes["RepositoryUrl"], Is.EqualTo(repositoryUrl));
        }

        [TestCase(typeof(object), ExpectedResult = true)]
        [TestCase(typeof(AssemblyTests), ExpectedResult = false)]
        public bool IsStronglyNamed_ReflectsPublicKey(Type typeInAssembly)
        {
            return typeInAssembly.Assembly.GetMetadata().IsStronglyNamed;
        }

        [Test]
        public void IsReflectionOnly_WithRuntimeAssembly_ReturnsFalse()
        {
            var assemblyMetadata = typeof(AssemblyTests).Assembly.GetMetadata();

            Assert.That(assemblyMetadata.IsReflectionOnly, Is.False);
        }

        [Test]
        public void IsReflectionOnly_WithMetadataLoadContextAssembly_ReturnsTrue()
        {
            var assemblyPath = typeof(AssemblyTests).Assembly.Location;
            using var universe = new MetadataUniverse([assemblyPath]);

            var assemblyMetadata = universe.LoadFromPath(assemblyPath).GetMetadata();

            Assert.That(assemblyMetadata.IsReflectionOnly, Is.True);
        }

        [Test]
        public void ExtensionMethods_ContainsClassicAndBlockExtensionMethods()
        {
            var assemblyMetadata = typeof(AssemblyTests).Assembly.GetMetadata();

            string[] expectedNames =
            [
                nameof(Acme.SampleExtensions.ClassicExtensionMethodForClass),
                nameof(Acme.SampleExtensions.ClassicExtensionMethodForStruct),
                nameof(Acme.SampleExtensions.ClassicExtensionMethodForInterface),
                nameof(Acme.SampleExtensions.ClassicExtensionMethod),
                nameof(Acme.SampleExtensions.InstanceExtensionMethodForClass),
                nameof(Acme.SampleExtensions.StaticExtensionMethodForClass),
                nameof(Acme.SampleExtensions.GenericExtensionMethodForClass),
                nameof(Acme.SampleExtensions.InstanceExtensionMethodForStruct),
                nameof(Acme.SampleExtensions.StaticExtensionMethodForStruct),
                nameof(Acme.SampleExtensions.GenericExtensionMethodForStruct),
                nameof(Acme.SampleExtensions.InstanceExtensionMethodForInterface),
                nameof(Acme.SampleExtensions.StaticExtensionMethodForInterface),
                nameof(Acme.SampleExtensions.GenericExtensionMethodForInterface),
                nameof(Acme.SampleExtensions.InstanceExtensionMethod),
                nameof(Acme.SampleExtensions.StaticExtensionMethod),
                nameof(Acme.SampleExtensions.GenericExtensionMethod),
            ];

            Assert.That(assemblyMetadata.ExtensionMethods.Select(static m => m.Name), Is.EquivalentTo(expectedNames));
        }

        [Test]
        public void ExtensionProperties_ContainsBlockExtensionProperties()
        {
            var assemblyMetadata = typeof(AssemblyTests).Assembly.GetMetadata();

            string[] expectedNames =
            [
                "InstanceExtensionPropertyForClass",
                "StaticExtensionPropertyForClass",
                "InstanceExtensionPropertyForStruct",
                "StaticExtensionPropertyForStruct",
                "InstanceExtensionPropertyForInterface",
                "StaticExtensionPropertyForInterface",
                "InstanceExtensionProperty",
                "StaticExtensionProperty",
                "FullExtensionProperty",
            ];

            Assert.That(assemblyMetadata.ExtensionProperties.Select(static p => p.Name), Is.EquivalentTo(expectedNames));
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
        public void TryGetType_WithNullName_ThrowsArgumentNullException()
        {
            var assemblyMetadata = typeof(AssemblyTests).Assembly.GetMetadata();

            Assert.That(() => assemblyMetadata.TryGetType(null!, out _), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("fullName"));
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

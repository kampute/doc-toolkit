// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.Linq;

    [TestFixture]
    public class InterfaceTypeTests
    {
        [TestCase(typeof(IDisposable), nameof(IDisposable))]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface))]
        [TestCase(typeof(Acme.ISampleGenericInterface<>), "ISampleGenericInterface`1")]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), "IInnerGenericInterface`2")]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface), "IDeepInnerGenericInterface")]
        public void ImplementsInterfaceType(Type interfaceType, string expectedName)
        {
            var metadata = interfaceType.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IInterfaceType>());
            Assert.That(metadata.Name, Is.EqualTo(expectedName));
        }

        [Test]
        public void GenericTypeDefinition_HasCorrectGenericMetadata()
        {
            var metadata = typeof(System.Collections.Generic.IEnumerable<>).GetMetadata<IInterfaceType>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsGenericTypeDefinition, Is.True);
                Assert.That(metadata.TypeParameters, Has.Count.EqualTo(1));
                Assert.That(metadata.TypeParameters[0].Name, Is.EqualTo("T"));

                Assert.That(metadata.IsConstructedGenericType, Is.False);
                Assert.That(metadata.TypeArguments, Is.Empty);
                Assert.That(metadata.GenericTypeDefinition, Is.Null);
            }
        }

        [Test]
        public void ConstructedGenericType_HasCorrectGenericMetadata()
        {
            var metadata = typeof(System.Collections.Generic.IEnumerable<int>).GetMetadata<IInterfaceType>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsGenericTypeDefinition, Is.False);
                Assert.That(metadata.TypeParameters, Is.Empty);

                Assert.That(metadata.IsConstructedGenericType, Is.True);
                Assert.That(metadata.TypeArguments, Has.Count.EqualTo(1));
                Assert.That(metadata.TypeArguments[0].Name, Is.EqualTo(nameof(Int32)));
                Assert.That(metadata.GenericTypeDefinition, Is.Not.Null);
            }
        }

        [TestCase(typeof(IDisposable), 0, 0)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>), 0, 1)]
        [TestCase(typeof(Acme.ISampleGenericInterface<object>), 0, 1)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), 1, 2)]
        [TestCase(typeof(Acme.ISampleGenericInterface<object>.IInnerGenericInterface<int, string>), 1, 2)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface), 3, 0)]
        [TestCase(typeof(Acme.ISampleGenericInterface<object>.IInnerGenericInterface<int, string>.IDeepInnerGenericInterface), 3, 0)]
        public void OwnGenericParameterRange_HasExpectedValue(Type interfaceType, int expectedOffset = 0, int expectedCount = 0)
        {
            var metadata = interfaceType.GetMetadata<IGenericCapableType>();

            Assert.That(metadata.OwnGenericParameterRange, Is.EqualTo((expectedOffset, expectedCount)));
        }

        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface),
            nameof(Acme.ISampleInterface),
            nameof(System.Collections.IEnumerable),
            "IEnumerable`1"
        )]
        [TestCase(typeof(Acme.ISampleDirectAndIndirectExtendedInterface),
            nameof(System.Collections.IEnumerable),
            "IReadOnlyCollection`1", // IReadOnlyCollection<int>
            "IEnumerable`1", // IEnumerable<int>
            "IEnumerable`1" // IEnumerable<string>
        )]
        public void Interfaces_HasExpectedValue(Type interfaceType, params string[] expectedNames)
        {
            var metadata = interfaceType.GetMetadata<IInterfaceType>();

            Assert.That(metadata.Interfaces.Select(i => i.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.ISampleInterface),
            nameof(Acme.ISampleInterface.InterfaceField)
        )]
        public void Fields_HasExpectedValue(Type interfaceType, params string[] expectedNames)
        {
            var metadata = interfaceType.GetMetadata<IInterfaceType>();

            Assert.That(metadata.Fields.Select(e => e.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.ISampleInterface),
            nameof(Acme.ISampleInterface.InterfaceMethod),
            nameof(Acme.ISampleInterface.InterfaceGenericMethod),
            nameof(Acme.ISampleInterface.InterfaceMethodWithInParam),
            nameof(Acme.ISampleInterface.InterfaceMethodWithOutParam),
            nameof(Acme.ISampleInterface.InterfaceMethodWithRefParam),
            nameof(Acme.ISampleInterface.InterfaceStaticMethod),
            nameof(Acme.ISampleInterface.InterfaceStaticDefaultMethod)
        )]
        public void Methods_HasExpectedValue(Type interfaceType, params string[] expectedNames)
        {
            var metadata = interfaceType.GetMetadata<IInterfaceType>();

            Assert.That(metadata.Methods.Select(m => m.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.ISampleInterface),
            nameof(Acme.ISampleInterface.InterfaceProperty)
        )]
        public void Properties_HasExpectedValue(Type interfaceType, params string[] expectedNames)
        {
            var metadata = interfaceType.GetMetadata<IInterfaceType>();

            Assert.That(metadata.Properties.Select(p => p.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.ISampleInterface),
            nameof(Acme.ISampleInterface.InterfaceEvent)
        )]
        public void Events_HasExpectedValue(Type interfaceType, params string[] expectedNames)
        {
            var metadata = interfaceType.GetMetadata<IInterfaceType>();

            Assert.That(metadata.Events.Select(e => e.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.ISampleInterface),
            "True",
            "False"
        )]
        public void Operators_HasExpectedValue(Type interfaceType, params string[] expectedNames)
        {
            var metadata = interfaceType.GetMetadata<IInterfaceType>();

            Assert.That(metadata.Operators.Select(e => e.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.ISampleGenericInterface<>),
            typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>)
        )]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>),
            typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface)
        )]
        public void NestedTypes_HasExpectedValue(Type structType, params Type[] expectedTypes)
        {
            var metadata = structType.GetMetadata<IInterfaceType>();
            var expectedNestedTypes = expectedTypes.Select(t => t.GetMetadata());

            Assert.That(metadata.NestedTypes, Is.EquivalentTo(expectedNestedTypes));
        }

        [TestCase(typeof(IDisposable), ExpectedResult = "T:System.IDisposable")]
        [TestCase(typeof(System.Collections.Generic.IEnumerable<>), ExpectedResult = "T:System.Collections.Generic.IEnumerable`1")]
        [TestCase(typeof(Acme.ISampleGenericInterface<>), ExpectedResult = "T:Acme.ISampleGenericInterface`1")]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), ExpectedResult = "T:Acme.ISampleGenericInterface`1.IInnerGenericInterface`2")]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface), ExpectedResult = "T:Acme.ISampleGenericInterface`1.IInnerGenericInterface`2.IDeepInnerGenericInterface")]
        public string CodeReference_HasExpectedValue(Type interfaceType)
        {
            var metadata = interfaceType.GetMetadata<IInterfaceType>();

            return metadata.CodeReference;
        }

        [TestCase(typeof(Acme.ISampleGenericInterface<>))]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>),
            typeof(Acme.ISampleGenericInterface<>)
        )]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface),
            typeof(Acme.ISampleGenericInterface<>),
            typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>)
        )]
        public void DeclaringTypeHierarchy_HasExpectedValue(Type type, params Type[] expectedTypes)
        {
            var metadata = type.GetMetadata<IInterfaceType>();
            var expectedDeclaringTypes = expectedTypes.Select(t => t.GetMetadata());

            Assert.That(metadata.DeclaringTypeHierarchy, Is.EqualTo(expectedDeclaringTypes));
        }

        [TestCase(typeof(Acme.ISampleInterface))]
        [TestCase(typeof(Acme.ISampleExtendedGenericInterface<,,>))]
        public void BaseTypeHierarchy_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IInterfaceType>();

            Assert.That(metadata.BaseTypeHierarchy, Is.Empty);
        }

        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface),
            nameof(Acme.ISampleInterface),
            "IEnumerable`1" // IEnumerable<V>
        )]
        [TestCase(typeof(Acme.ISampleExtendedGenericInterface<,,>),
            nameof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface)
        )]
        [TestCase(typeof(Acme.ISampleExtendedConstructedGenericInterface),
            "ISampleExtendedGenericInterface`3" // ISampleExtendedGenericInterface<object, int, string>
        )]
        [TestCase(typeof(Acme.ISampleDirectExtendedConstructedGenericInterface),
            nameof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface)
        )]
        [TestCase(typeof(Acme.ISampleDirectAndIndirectExtendedInterface),
            "IReadOnlyCollection`1", // IReadOnlyCollection<int>
            "IEnumerable`1" // IEnumerable<string>
        )]
        public void ImplementedInterfaces_HasExpectedValue(Type type, params string[] expectedNames)
        {
            var metadata = type.GetMetadata<IInterfaceType>();

            Assert.That(metadata.ImplementedInterfaces.Select(i => i.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.ISampleInterface),
            typeof(Acme.SampleMethods),
            typeof(Acme.SampleProperties),
            typeof(Acme.SampleEvents),
            typeof(Acme.SampleOperators),
            typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            typeof(Acme.SampleDerivedGenericClass<,,>),
            typeof(Acme.SampleDerivedConstructedGenericClass),
            typeof(Acme.SampleDirectDerivedConstructedGenericClass),
            typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface),
            typeof(Acme.ISampleExtendedGenericInterface<,,>),
            typeof(Acme.ISampleExtendedConstructedGenericInterface),
            typeof(Acme.ISampleDirectExtendedConstructedGenericInterface)
        )]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface),
            typeof(Acme.ISampleExtendedGenericInterface<,,>),
            typeof(Acme.ISampleExtendedConstructedGenericInterface),
            typeof(Acme.ISampleDirectExtendedConstructedGenericInterface)
        )]
        public void ImplementingTypes_HasExpectedValue(Type type, params Type[] expectedTypes)
        {
            var metadata = type.GetMetadata<IInterfaceType>();
            var expectedImplementingTypes = expectedTypes.Select(t => t.GetMetadata());

            Assert.That(metadata.ImplementingTypes, Is.EquivalentTo(expectedImplementingTypes));
        }

        [TestCase(typeof(System.Collections.IEnumerable), typeof(System.Collections.Generic.IEnumerable<>), ExpectedResult = false)]
        [TestCase(typeof(System.Collections.IEnumerable), typeof(System.Collections.Generic.IEnumerable<int>), ExpectedResult = false)]
        [TestCase(typeof(System.Collections.IEnumerable), typeof(System.Collections.Generic.IEnumerator<>), ExpectedResult = false)]
        [TestCase(typeof(System.Collections.Generic.IEnumerable<>), typeof(System.Collections.Generic.IEnumerable<int>), ExpectedResult = true)]
        [TestCase(typeof(System.Collections.Generic.IEnumerable<int>), typeof(System.Collections.Generic.IEnumerable<>), ExpectedResult = false)]
        [TestCase(typeof(System.Collections.Generic.IEnumerable<int>), typeof(System.Collections.Generic.IEnumerable<int>), ExpectedResult = true)]
        [TestCase(typeof(System.Collections.Generic.IEnumerable<int>), typeof(System.Collections.Generic.IEnumerable<string>), ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleInterface), typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface), ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleGenericInterface<object>), typeof(Acme.ISampleGenericInterface<System.IO.MemoryStream>), ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>), typeof(Acme.ISampleGenericInterface<System.IO.MemoryStream>), ExpectedResult = true)]
        [TestCase(typeof(Acme.ISampleGenericInterface<System.IO.MemoryStream>), typeof(Acme.ISampleGenericInterface<object>), ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>), typeof(Acme.ISampleGenericInterface<>), ExpectedResult = true)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), ExpectedResult = true)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), typeof(Acme.ISampleGenericInterface<object>.IInnerGenericInterface<int, string>), ExpectedResult = true)]
        [TestCase(typeof(Acme.ISampleGenericInterface<object>.IInnerGenericInterface<int, string>), typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), ExpectedResult = false)]
        public bool IsSubstitutableBy_ReturnsExpectedResult(Type targetType, Type sourceType)
        {
            var targetMetadata = targetType.GetMetadata<IInterfaceType>();
            var sourceMetadata = sourceType.GetMetadata();

            return targetMetadata.IsSubstitutableBy(sourceMetadata);
        }

        [TestCase(typeof(Acme.ISampleGenericInterface<>),
            "InstanceExtensionPropertyForInterface",
            "StaticExtensionPropertyForInterface"
        )]
        public void ExtensionProperties_HasExpectedValue(Type type, params string[] expectedNames)
        {
            var extendedType = type.GetMetadata<IInterfaceType>();

            Assert.That(extendedType.ExtensionProperties.Select(m => m.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.ISampleGenericInterface<>),
            nameof(Acme.SampleExtensions.ClassicExtensionMethodForInterface),
            nameof(Acme.SampleExtensions.InstanceExtensionMethodForInterface),
            nameof(Acme.SampleExtensions.StaticExtensionMethodForInterface),
            nameof(Acme.SampleExtensions.GenericExtensionMethodForInterface)
        )]
        public void ExtensionMethods_HasExpectedValue(Type type, params string[] expectedNames)
        {
            var extendedType = type.GetMetadata<IInterfaceType>();

            Assert.That(extendedType.ExtensionMethods.Select(m => m.Name), Is.EquivalentTo(expectedNames));
        }
    }
}

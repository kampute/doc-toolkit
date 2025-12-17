// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System;
    using System.Linq;

    [TestFixture]
    public class StructTypeTests
    {
        [TestCase(typeof(DateTime), nameof(DateTime))]
        [TestCase(typeof(System.Collections.Generic.KeyValuePair<,>), "KeyValuePair`2")]
        [TestCase(typeof(System.Collections.Generic.KeyValuePair<string, int>), "KeyValuePair`2")]
        [TestCase(typeof(Acme.SampleGenericStruct<>), "SampleGenericStruct`1")]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>), "InnerGenericStruct`2")]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct), "DeepInnerGenericStruct")]
        public void ImplementsStructType(Type structType, string expectedName)
        {
            var metadata = structType.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IStructType>());
            Assert.That(metadata.Name, Is.EqualTo(expectedName));
        }

        [TestCase(typeof(ReadOnlySpan<>), true)]
        [TestCase(typeof(Acme.SampleGenericStruct<>), false)]
        public void IsReadonly_HasExpectedValue(Type structType, bool isReadOnly)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.IsReadOnly, Is.EqualTo(isReadOnly));
        }

        [TestCase(typeof(ReadOnlySpan<>), true)]
        [TestCase(typeof(Acme.SampleGenericStruct<>), false)]
        public void IsRef_HasExpectedValue(Type structType, bool isRef)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.IsRefLike, Is.EqualTo(isRef));
        }

        [Test]
        public void GenericTypeDefinition_HasCorrectGenericMetadata()
        {
            var metadata = typeof(System.Collections.Generic.KeyValuePair<,>).GetMetadata<IStructType>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsGenericTypeDefinition, Is.True);
                Assert.That(metadata.TypeParameters, Has.Count.EqualTo(2));
                Assert.That(metadata.TypeParameters[0].Name, Is.EqualTo("TKey"));
                Assert.That(metadata.TypeParameters[1].Name, Is.EqualTo("TValue"));

                Assert.That(metadata.IsConstructedGenericType, Is.False);
                Assert.That(metadata.TypeArguments, Is.Empty);
                Assert.That(metadata.GenericTypeDefinition, Is.Null);
            }
        }

        [Test]
        public void ConstructedGenericType_HasCorrectGenericMetadata()
        {
            var metadata = typeof(System.Collections.Generic.KeyValuePair<int, string>).GetMetadata<IStructType>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsGenericTypeDefinition, Is.False);
                Assert.That(metadata.TypeParameters, Is.Empty);

                Assert.That(metadata.IsConstructedGenericType, Is.True);
                Assert.That(metadata.TypeArguments, Has.Count.EqualTo(2));
                Assert.That(metadata.TypeArguments[0].Name, Is.EqualTo("Int32"));
                Assert.That(metadata.TypeArguments[1].Name, Is.EqualTo("String"));
                Assert.That(metadata.GenericTypeDefinition, Is.Not.Null);
            }
        }

        [TestCase(typeof(DateTime), 0, 0)]
        [TestCase(typeof(System.Collections.Generic.KeyValuePair<,>), 0, 2)]
        [TestCase(typeof(System.Collections.Generic.KeyValuePair<string, int>), 0, 2)]
        [TestCase(typeof(Acme.SampleGenericStruct<>), 0, 1)]
        [TestCase(typeof(Acme.SampleGenericStruct<System.IO.Stream>), 0, 1)]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>), 1, 2)]
        [TestCase(typeof(Acme.SampleGenericStruct<System.IO.Stream>.InnerGenericStruct<int, string>), 1, 2)]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct), 3, 0)]
        [TestCase(typeof(Acme.SampleGenericStruct<System.IO.Stream>.InnerGenericStruct<int, string>.DeepInnerGenericStruct), 3, 0)]
        public void OwnGenericParameterRange_HasExpectedValue(Type structType, int expectedOffset, int expectedCount)
        {
            var metadata = structType.GetMetadata<IGenericCapableType>();

            Assert.That(metadata.OwnGenericParameterRange, Is.EqualTo((expectedOffset, expectedCount)));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            nameof(Acme.ISampleInterface),
            nameof(System.Collections.IEnumerable),
            "IEnumerable`1" // IEnumerable<V>
        )]
        public void Interfaces_HasExpectedValue(Type structType, params string[] expectedNames)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.Interfaces.Select(static i => i.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct), ExpectedResult = 1)]
        public int Constructors_HasExpectedValue(Type structType)
        {
            var metadata = structType.GetMetadata<IStructType>();

            return metadata.Constructors.Count;
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            nameof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct.Field)
        )]
        public void Fields_HasExpectedValue(Type structType, params string[] expectedNames)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.Fields.Select(static f => f.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            nameof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct.Method),
            nameof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct.GenericMethod)
        )]
        public void Methods_HasExpectedValue(Type structType, params string[] expectedNames)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.Methods.Select(static m => m.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            nameof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct.Property)
        )]
        public void Properties_HasExpectedValue(Type structType, params string[] expectedNames)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.Properties.Select(static p => p.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            nameof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct.Event)
        )]
        public void Events_HasExpectedValue(Type structType, params string[] expectedNames)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.Events.Select(static e => e.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            "Implicit"
        )]
        public void Operators_HasExpectedValue(Type structType, params string[] expectedNames)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.Operators.Select(static o => o.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            nameof(Acme.ISampleInterface.InterfaceMethod),
            nameof(System.Collections.Generic.IEnumerable<>.GetEnumerator),
            nameof(System.Collections.IEnumerable.GetEnumerator)
        )]
        public void ExplicitInterfaceMethods_HasExpectedValue(Type structType, params string[] expectedMethodNames)
        {
            var metadata = structType.GetMetadata<IStructType>();

            var shortNames = metadata.ExplicitInterfaceMethods.Select(static m => m.Name.SubstringAfterLast('.'));

            Assert.That(shortNames, Is.EquivalentTo(expectedMethodNames));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            nameof(Acme.ISampleInterface.InterfaceProperty)
        )]
        public void ExplicitInterfaceProperties_HasExpectedValue(Type structType, params string[] expectedPropertyNames)
        {
            var metadata = structType.GetMetadata<IStructType>();

            var shortNames = metadata.ExplicitInterfaceProperties.Select(static p => p.Name.SubstringAfterLast('.'));

            Assert.That(shortNames, Is.EquivalentTo(expectedPropertyNames));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            nameof(Acme.ISampleInterface.InterfaceEvent)
        )]
        public void ExplicitInterfaceEvents_HasExpectedValue(Type structType, params string[] expectedEventNames)
        {
            var metadata = structType.GetMetadata<IStructType>();

            var shortNames = metadata.ExplicitInterfaceEvents.Select(static e => e.Name.SubstringAfterLast('.'));

            Assert.That(shortNames, Is.EquivalentTo(expectedEventNames));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            "False"
        )]
        public void ExplicitInterfaceOperators_HasExpectedValue(Type classType, params string[] expectedNames)
        {
            var metadata = classType.GetMetadata<IStructType>();

            var shortNames = metadata.ExplicitInterfaceOperators.Select(static e => e.Name.SubstringAfterLast('.'));

            Assert.That(shortNames, Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleFields))]
        [TestCase(typeof(Acme.SampleGenericStruct<>),
            typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>)
        )]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>),
            typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct)
        )]
        public void NestedTypes_HasExpectedValue(Type structType, params Type[] expectedTypes)
        {
            var metadata = structType.GetMetadata<IStructType>();
            var expectedNestedTypes = expectedTypes.Select(static t => t.GetMetadata());

            Assert.That(metadata.NestedTypes, Is.EquivalentTo(expectedNestedTypes));
        }

        [TestCase(typeof(DateTime), "T:System.DateTime")]
        [TestCase(typeof(Nullable<>), "T:System.Nullable`1")]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct), "T:Acme.SampleGenericStruct`1.InnerGenericStruct`2.DeepInnerGenericStruct")]
        public void CodeReference_HasExpectedValue(Type structType, string expected)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.CodeReference, Is.EqualTo(expected));
        }

        [TestCase(typeof(DateTime))]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>),
            typeof(Acme.SampleGenericStruct<>)
        )]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            typeof(Acme.SampleGenericStruct<>),
            typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>)
        )]
        public void DeclaringTypeHierarchy_HasExpectedValue(Type type, params Type[] expectedTypes)
        {
            var metadata = type.GetMetadata<IStructType>();
            var expectedDeclaringTypes = expectedTypes.Select(static t => t.GetMetadata());

            Assert.That(metadata.DeclaringTypeHierarchy, Is.EqualTo(expectedDeclaringTypes));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            typeof(object),
            typeof(ValueType)
        )]
        public void BaseTypeHierarchy_HasExpectedValue(Type type, params Type[] expectedTypes)
        {
            var metadata = type.GetMetadata<IStructType>();
            var expectedBaseTypes = expectedTypes.Select(static t => t.GetMetadata<IClassType>());

            Assert.That(metadata.BaseTypeHierarchy, Is.EqualTo(expectedBaseTypes));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct),
            nameof(Acme.ISampleInterface),
            "IEnumerable`1" // IEnumerable<V>
        )]
        public void ImplementedInterfaces_HasExpectedValue(Type type, params string[] expectedNames)
        {
            var metadata = type.GetMetadata<IStructType>();

            Assert.That(metadata.ImplementedInterfaces.Select(static i => i.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>),
            "InstanceExtensionPropertyForStruct",
            "StaticExtensionPropertyForStruct"
        )]
        public void ExtensionProperties_HasExpectedValue(Type type, params string[] expectedNames)
        {
            var extendedType = type.GetMetadata<IStructType>();

            Assert.That(extendedType.ExtensionProperties.Select(static m => m.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericStruct<>),
            nameof(Acme.SampleExtensions.ClassicExtensionMethodForStruct),
            nameof(Acme.SampleExtensions.InstanceExtensionMethodForStruct),
            nameof(Acme.SampleExtensions.StaticExtensionMethodForStruct),
            nameof(Acme.SampleExtensions.GenericExtensionMethodForStruct)
        )]
        public void ExtensionMethods_HasExpectedValue(Type type, params string[] expectedNames)
        {
            var extendedType = type.GetMetadata<IStructType>();

            Assert.That(extendedType.ExtensionMethods.Select(static m => m.Name), Is.EquivalentTo(expectedNames));
        }
    }
}

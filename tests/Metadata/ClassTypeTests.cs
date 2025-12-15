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
    public class ClassTypeTests
    {
        [TestCase(typeof(string), nameof(String))]
        [TestCase(typeof(System.Collections.Generic.List<>), "List`1")]
        [TestCase(typeof(System.Collections.Generic.List<string>), "List`1")]
        [TestCase(typeof(Acme.SampleGenericClass<>), "SampleGenericClass`1")]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), "InnerGenericClass`2")]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), "DeepInnerGenericClass")]
        public void ImplementsClassType(Type classType, string expectedName)
        {
            var metadata = classType.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IClassType>());
            Assert.That(metadata.Name, Is.EqualTo(expectedName));
        }

        [TestCase(typeof(System.IO.Stream), true)]
        [TestCase(typeof(System.Collections.Generic.List<>), false)]
        public void IsAbstract_HasExpectedValue(Type classType, bool isAbstract)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.IsAbstract, Is.EqualTo(isAbstract));
        }

        [TestCase(typeof(string), true)]
        [TestCase(typeof(System.Collections.Generic.List<>), false)]
        public void IsSealed_HasExpectedValue(Type classType, bool isSealed)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.IsSealed, Is.EqualTo(isSealed));
        }

        [TestCase(typeof(string), false)]
        [TestCase(typeof(Enumerable), true)]
        public void IsStatic_HasExpectedValue(Type classType, bool isStatic)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.IsStatic, Is.EqualTo(isStatic));
        }

        [TestCase(typeof(Acme.SampleDerivedGenericClass<,,>), typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass))]
        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass), typeof(Acme.SampleDerivedGenericClass<object, int, string>))]
        [TestCase(typeof(System.IO.MemoryStream), typeof(System.IO.Stream))]
        [TestCase(typeof(System.IO.Stream), typeof(MarshalByRefObject))]
        [TestCase(typeof(MarshalByRefObject), typeof(object))]
        [TestCase(typeof(object), null)]
        public void BaseType_HasExpectedValue(Type classType, Type? baseType)
        {
            var metadata = classType.GetMetadata<IClassType>();
            var baseMetadata = baseType?.GetMetadata();

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.BaseType, Is.SameAs(baseMetadata));
        }

        [Test]
        public void GenericTypeDefinition_HasCorrectGenericMetadata()
        {
            var metadata = typeof(System.Collections.Generic.List<>).GetMetadata<IClassType>();

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
        public void ConstructedGenericTypes_HasCorrectGenericMetadata()
        {
            var metadata = typeof(System.Collections.Generic.List<int>).GetMetadata<IClassType>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsGenericTypeDefinition, Is.False);
                Assert.That(metadata.TypeParameters, Is.Empty);

                Assert.That(metadata.IsConstructedGenericType, Is.True);
                Assert.That(metadata.TypeArguments, Has.Count.EqualTo(1));
                Assert.That(metadata.TypeArguments[0].Name, Is.EqualTo("Int32"));
                Assert.That(metadata.GenericTypeDefinition, Is.Not.Null);
            }
        }

        [TestCase(typeof(string), 0, 0)]
        [TestCase(typeof(System.Collections.Generic.List<>), 0, 1)]
        [TestCase(typeof(System.Collections.Generic.List<int>), 0, 1)]
        [TestCase(typeof(System.Collections.Generic.Dictionary<,>), 0, 2)]
        [TestCase(typeof(System.Collections.Generic.Dictionary<string, int>), 0, 2)]
        [TestCase(typeof(Acme.SampleGenericClass<>), 0, 1)]
        [TestCase(typeof(Acme.SampleGenericClass<object>), 0, 1)]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), 1, 2)]
        [TestCase(typeof(Acme.SampleGenericClass<object>.InnerGenericClass<int, string>), 1, 2)]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), 3, 0)]
        [TestCase(typeof(Acme.SampleGenericClass<object>.InnerGenericClass<int, string>.DeepInnerGenericClass), 3, 0)]
        public void OwnGenericParameterRange_HasExpectedValue(Type classType, int expectedOffset, int expectedCount)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.OwnGenericParameterRange, Is.EqualTo((expectedOffset, expectedCount)));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            nameof(Acme.ISampleInterface),
            nameof(System.Collections.IEnumerable),
            "IEnumerable`1" // IEnumerable<V>
        )]
        public void Interfaces_HasExpectedValue(Type classType, params string[] expectedNames)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.Interfaces.Select(i => i.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), ExpectedResult = 1)]
        public int Constructors_HasExpectedValue(Type classType)
        {
            var metadata = classType.GetMetadata<IClassType>();

            return metadata.Constructors.Count;
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.Field)
        )]
        public void Fields_HasExpectedValue(Type classType, params string[] expectedNames)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.Fields.Select(f => f.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.Method),
            nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.GenericMethod)
        )]
        [TestCase(typeof(Acme.SampleExtensions),
            nameof(Acme.SampleExtensions.ClassicExtensionMethodForClass),
            nameof(Acme.SampleExtensions.InstanceExtensionMethodForClass),
            nameof(Acme.SampleExtensions.StaticExtensionMethodForClass),
            nameof(Acme.SampleExtensions.GenericExtensionMethodForClass),
            nameof(Acme.SampleExtensions.ClassicExtensionMethodForStruct),
            nameof(Acme.SampleExtensions.InstanceExtensionMethodForStruct),
            nameof(Acme.SampleExtensions.StaticExtensionMethodForStruct),
            nameof(Acme.SampleExtensions.GenericExtensionMethodForStruct),
            nameof(Acme.SampleExtensions.ClassicExtensionMethodForInterface),
            nameof(Acme.SampleExtensions.InstanceExtensionMethodForInterface),
            nameof(Acme.SampleExtensions.StaticExtensionMethodForInterface),
            nameof(Acme.SampleExtensions.GenericExtensionMethodForInterface),
            nameof(Acme.SampleExtensions.ClassicExtensionMethod),
            nameof(Acme.SampleExtensions.InstanceExtensionMethod),
            nameof(Acme.SampleExtensions.StaticExtensionMethod),
            nameof(Acme.SampleExtensions.GenericExtensionMethod),
            nameof(Acme.SampleExtensions.NonExtensionMethod)
        )]
        public void Methods_HasExpectedValue(Type classType, params string[] expectedNames)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.Methods.Select(m => m.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.Property)
        )]
        [TestCase(typeof(Acme.SampleExtensions),
            "InstanceExtensionPropertyForClass",
            "StaticExtensionPropertyForClass",
            "InstanceExtensionPropertyForStruct",
            "StaticExtensionPropertyForStruct",
            "InstanceExtensionPropertyForInterface",
            "StaticExtensionPropertyForInterface",
            "InstanceExtensionProperty",
            "StaticExtensionProperty",
            "FullExtensionProperty"
        )]
        public void Properties_HasExpectedValue(Type classType, params string[] expectedNames)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.Properties.Select(p => p.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.Event)
        )]
        public void Events_HasExpectedValue(Type classType, params string[] expectedNames)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.Events.Select(e => e.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            "Implicit"
        )]
        public void Operators_HasExpectedValue(Type classType, params string[] expectedNames)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.Operators.Select(o => o.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            nameof(Acme.ISampleInterface.InterfaceMethod),
            nameof(System.Collections.Generic.IEnumerable<>.GetEnumerator),
            nameof(System.Collections.IEnumerable.GetEnumerator)
        )]
        public void ExplicitInterfaceMethods_HasExpectedValue(Type classType, params string[] expectedNames)
        {
            var metadata = classType.GetMetadata<IClassType>();

            var shortNames = metadata.ExplicitInterfaceMethods.Select(m => m.Name.SubstringAfterLast('.'));

            Assert.That(shortNames, Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            nameof(Acme.ISampleInterface.InterfaceProperty)
        )]
        public void ExplicitInterfaceProperties_HasExpectedValue(Type classType, params string[] expectedNames)
        {
            var metadata = classType.GetMetadata<IClassType>();

            var shortNames = metadata.ExplicitInterfaceProperties.Select(p => p.Name.SubstringAfterLast('.'));

            Assert.That(shortNames, Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            nameof(Acme.ISampleInterface.InterfaceEvent)
        )]
        public void ExplicitInterfaceEvents_HasExpectedValue(Type classType, params string[] expectedNames)
        {
            var metadata = classType.GetMetadata<IClassType>();

            var shortNames = metadata.ExplicitInterfaceEvents.Select(e => e.Name.SubstringAfterLast('.'));

            Assert.That(shortNames, Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            "False"
        )]
        public void ExplicitInterfaceOperators_HasExpectedValue(Type classType, params string[] expectedNames)
        {
            var metadata = classType.GetMetadata<IClassType>();

            var shortNames = metadata.ExplicitInterfaceOperators.Select(e => e.Name.SubstringAfterLast('.'));

            Assert.That(shortNames, Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>),
            typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>)
        )]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>),
            typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass)
        )]
        public void NestedTypes_HasExpectedValue(Type classType, params Type[] expectedTypes)
        {
            var metadata = classType.GetMetadata<IClassType>();
            var expectedNestedTypes = expectedTypes.Select(t => t.GetMetadata());

            Assert.That(metadata.NestedTypes, Is.EquivalentTo(expectedNestedTypes));
        }

        [TestCase(typeof(string), ExpectedResult = "T:System.String")]
        [TestCase(typeof(System.Collections.Generic.List<>), ExpectedResult = "T:System.Collections.Generic.List`1")]
        [TestCase(typeof(Acme.SampleGenericClass<>), ExpectedResult = "T:Acme.SampleGenericClass`1")]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), ExpectedResult = "T:Acme.SampleGenericClass`1.InnerGenericClass`2")]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), ExpectedResult = "T:Acme.SampleGenericClass`1.InnerGenericClass`2.DeepInnerGenericClass")]
        public string CodeReference_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IClassType>();

            return metadata.CodeReference;
        }

        [TestCase(typeof(Acme.SampleGenericClass<>))]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>),
            typeof(Acme.SampleGenericClass<>)
        )]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            typeof(Acme.SampleGenericClass<>),
            typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>)
        )]
        public void DeclaringTypeHierarchy_HasExpectedValue(Type type, params Type[] expectedTypes)
        {
            var metadata = type.GetMetadata<IClassType>();
            var expectedDeclaringTypes = expectedTypes.Select(t => t.GetMetadata());

            Assert.That(metadata.DeclaringTypeHierarchy, Is.EqualTo(expectedDeclaringTypes));
        }

        [TestCase(typeof(object))]
        [TestCase(typeof(Acme.SampleGenericClass<>),
            typeof(object)
        )]
        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass),
            typeof(object),
            typeof(Acme.SampleGenericClass<object>.InnerGenericClass<int, string>.DeepInnerGenericClass),
            typeof(Acme.SampleDerivedGenericClass<object, int, string>)
        )]
        public void BaseTypeHierarchy_HasExpectedValue(Type type, params Type[] expectedTypes)
        {
            var metadata = type.GetMetadata<IClassType>();
            var expectedBaseTypes = expectedTypes.Select(t => t.GetMetadata<IClassType>());

            Assert.That(metadata.BaseTypeHierarchy, Is.EqualTo(expectedBaseTypes));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            nameof(Acme.ISampleInterface),
            "IEnumerable`1" // IEnumerable<V>
        )]
        [TestCase(typeof(Acme.SampleDerivedGenericClass<,,>))]
        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass))]
        [TestCase(typeof(Acme.SampleDirectDerivedConstructedGenericClass))]
        public void ImplementedInterfaces_HasExpectedValue(Type type, params string[] expectedNames)
        {
            var metadata = type.GetMetadata<IClassType>();

            Assert.That(metadata.ImplementedInterfaces.Select(i => i.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>), typeof(Acme.SampleGenericClass<>), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericClass<>), typeof(Acme.SampleGenericClass<object>), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleGenericClass<object>), typeof(Acme.SampleGenericClass<>), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleGenericClass<object>), typeof(Acme.SampleGenericClass<object>), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericClass<object>), typeof(Acme.SampleGenericClass<string>), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleGenericClass<string>), typeof(Acme.SampleGenericClass<object>), ExpectedResult = false)]
        public bool IsAssignableFrom_ReturnsExpectedResult(Type targetType, Type sourceType)
        {
            var targetMetadata = targetType.GetMetadata<IClassType>();
            var sourceMetadata = sourceType.GetMetadata();

            return targetMetadata.IsAssignableFrom(sourceMetadata);
        }

        [TestCase(typeof(Acme.SampleGenericClass<>), typeof(Acme.SampleGenericClass<>), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericClass<>), typeof(Acme.SampleGenericClass<object>), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericClass<object>), typeof(Acme.SampleGenericClass<>), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleGenericClass<object>), typeof(Acme.SampleGenericClass<object>), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericClass<object>), typeof(Acme.SampleGenericClass<string>), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleGenericClass<string>), typeof(Acme.SampleGenericClass<object>), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleGenericClass<>), typeof(Acme.SampleGenericStruct<>), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), typeof(Acme.SampleGenericClass<object>.InnerGenericClass<int, string>), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericClass<object>.InnerGenericClass<int, string>), typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), ExpectedResult = false)]
        public bool IsSubstitutableBy_ReturnsExpectedResult(Type targetType, Type sourceType)
        {
            var targetMetadata = targetType.GetMetadata<IClassType>();
            var sourceMetadata = sourceType.GetMetadata();

            return targetMetadata.IsSubstitutableBy(sourceMetadata);
        }

        [TestCase(typeof(Acme.SampleGenericClass<>),
            "InstanceExtensionPropertyForClass",
            "StaticExtensionPropertyForClass"
        )]
        public void ExtensionProperties_HasExpectedValue(Type type, params string[] expectedNames)
        {
            var extendedType = type.GetMetadata<IClassType>();

            Assert.That(extendedType.ExtensionProperties.Select(m => m.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>),
            nameof(Acme.SampleExtensions.ClassicExtensionMethodForClass),
            nameof(Acme.SampleExtensions.InstanceExtensionMethodForClass),
            nameof(Acme.SampleExtensions.StaticExtensionMethodForClass),
            nameof(Acme.SampleExtensions.GenericExtensionMethodForClass)
        )]
        public void ExtensionMethods_HasExpectedValue(Type type, params string[] expectedNames)
        {
            var extendedType = type.GetMetadata<IClassType>();

            Assert.That(extendedType.ExtensionMethods.Select(m => m.Name), Is.EquivalentTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>), typeof(object), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleDerivedGenericClass<,,>), typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass), typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass), typeof(Acme.SampleGenericClass<object>.InnerGenericClass<int, string>.DeepInnerGenericClass), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass), typeof(Acme.SampleDerivedGenericClass<object, int, string>), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleDirectDerivedConstructedGenericClass), typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleDirectDerivedConstructedGenericClass), typeof(Acme.SampleGenericClass<object>.InnerGenericClass<int, string>.DeepInnerGenericClass), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleDirectDerivedConstructedGenericClass), typeof(Acme.SampleDerivedGenericClass<object, int, string>), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleGenericClass<>), typeof(Acme.SampleDerivedConstructedGenericClass), ExpectedResult = false)]
        public bool IsSubclassOf_ReturnsExpectedResult(Type type, Type superType)
        {
            var metadata = type.GetMetadata<IClassType>();
            var baseMetadata = superType.GetMetadata<IClassType>();

            return metadata.IsSubclassOf(baseMetadata);
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass),
            typeof(Acme.SampleDerivedGenericClass<,,>),
            typeof(Acme.SampleDirectDerivedConstructedGenericClass)
        )]
        public void DerivedTypes_WithNoGenericBaseClass_ReturnsExpectedValue(Type type, params Type[] expectedTypes)
        {
            var metadata = type.GetMetadata<IClassType>();
            var expectedDerivedTypes = expectedTypes.Select(t => t.GetMetadata<IClassType>());

            Assert.That(metadata.DerivedTypes, Is.EquivalentTo(expectedDerivedTypes));
        }
    }
}
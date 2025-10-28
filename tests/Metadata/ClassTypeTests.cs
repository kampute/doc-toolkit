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
    public class ClassTypeTests
    {
        [TestCase(typeof(string), nameof(String))]
        [TestCase(typeof(System.Collections.Generic.List<>), "List`1")]
        [TestCase(typeof(System.Collections.Generic.List<string>), "List`1")]
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
        [TestCase(typeof(System.Linq.Enumerable), true)]
        public void IsStatic_HasExpectedValue(Type classType, bool isStatic)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.IsStatic, Is.EqualTo(isStatic));
        }

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
        [TestCase(typeof(Acme.MyList<int>), 0, 1)]
        [TestCase(typeof(Acme.MyList<>.Helper<,>), 1, 2)]
        [TestCase(typeof(Acme.MyList<int>.Helper<bool, string>), 1, 2)]
        public void OwnGenericParameterRange_HasExpectedValue(Type classType, int expectedOffset, int expectedCount)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.OwnGenericParameterRange, Is.EqualTo((expectedOffset, expectedCount)));
        }

        [TestCase(typeof(Acme.UseList))]
        public void Interfaces_HasExpectedValue(Type classType)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.Interfaces, Is.Not.Empty);
        }

        [TestCase(typeof(Acme.Widget))]
        public void Fields_HasExpectedValue(Type classType)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.Fields, Is.Not.Empty);
            Assert.That(metadata.Fields.Where(f => !f.IsVisible), Is.Empty);
        }

        [TestCase(typeof(Acme.Widget))]
        public void Constructors_HasExpectedValue(Type classType)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.Constructors, Is.Not.Empty);
            Assert.That(metadata.Constructors.Where(c => !c.IsVisible), Is.Empty);
        }

        [TestCase(typeof(Acme.Widget))]
        public void Methods_HasExpectedValue(Type classType)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.Methods, Is.Not.Empty);
            Assert.That(metadata.Methods.Where(m => !m.IsVisible), Is.Empty);
        }

        [TestCase(typeof(Acme.Widget))]
        public void Properties_HasExpectedValue(Type classType)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.Properties, Is.Not.Empty);
            Assert.That(metadata.Properties.Where(p => !p.IsVisible), Is.Empty);
        }

        [TestCase(typeof(Acme.Widget))]
        public void Events_HasExpectedValue(Type classType)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.Events, Is.Not.Empty);
            Assert.That(metadata.Events.Where(e => !e.IsVisible), Is.Empty);
        }

        [TestCase(typeof(Acme.Widget))]
        public void Operators_HasExpectedValue(Type classType)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.Operators, Is.Not.Empty);
            Assert.That(metadata.Operators.Where(o => !o.IsVisible), Is.Empty);
        }

        [TestCase(typeof(Acme.Widget))]
        public void NestedTypes_HasExpectedValue(Type classType)
        {
            var metadata = classType.GetMetadata<IClassType>();

            Assert.That(metadata.NestedTypes.Select(static x => x.Name), Is.EquivalentTo([
                nameof(Acme.Widget.NestedClass),
                nameof(Acme.Widget.NestedDerivedClass),
                nameof(Acme.Widget.Direction),
                nameof(Acme.Widget.Del),
                "IMenuItem`1"
            ]));
        }

        [TestCase(typeof(string), ExpectedResult = "T:System.String")]
        [TestCase(typeof(System.Collections.Generic.List<>), ExpectedResult = "T:System.Collections.Generic.List`1")]
        [TestCase(typeof(Acme.Widget), ExpectedResult = "T:Acme.Widget")]
        [TestCase(typeof(Acme.Widget.NestedClass), ExpectedResult = "T:Acme.Widget.NestedClass")]
        [TestCase(typeof(Acme.MyList<>), ExpectedResult = "T:Acme.MyList`1")]
        [TestCase(typeof(Acme.MyList<>.Helper<,>), ExpectedResult = "T:Acme.MyList`1.Helper`2")]
        public string CodeReference_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IClassType>();

            return metadata.CodeReference;
        }

        [TestCase(typeof(TestTypes), ExpectedResult = new string[] { })]
        [TestCase(typeof(TestTypes.TestNestedClass), ExpectedResult = new[] { nameof(TestTypes) })]
        [TestCase(typeof(TestTypes.TestNestedClass.InnerClass), ExpectedResult = new[] { nameof(TestTypes), nameof(TestTypes.TestNestedClass) })]
        [TestCase(typeof(TestTypes.TestNestedClass.InnerClass.DeepestClass), ExpectedResult = new[] { nameof(TestTypes), nameof(TestTypes.TestNestedClass), nameof(TestTypes.TestNestedClass.InnerClass) })]
        public string[] DeclaringTypeHierarchy_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IClassType>();

            return [.. metadata.DeclaringTypeHierarchy.Select(t => t.Name)];
        }

        [TestCase(typeof(object), ExpectedResult = new string[] { })]
        [TestCase(typeof(TestTypes.TestBaseClass), ExpectedResult = new[] { nameof(Object) })]
        [TestCase(typeof(TestTypes.TestDerivedClass), ExpectedResult = new[] { nameof(Object), nameof(TestTypes.TestBaseClass) })]
        [TestCase(typeof(TestTypes.TestGrandChildClass), ExpectedResult = new[] { nameof(Object), nameof(TestTypes.TestBaseClass), nameof(TestTypes.TestDerivedClass) })]
        public string[] BaseTypeHierarchy_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IClassType>();

            return [.. metadata.BaseTypeHierarchy.Select(t => t.Name)];
        }

        [TestCase(typeof(TestTypes.TestExtensionTarget), ExpectedResult = new[] { nameof(TestTypes.ExtensionMethod) })]
        [TestCase(typeof(TestTypes.TestBaseClass), ExpectedResult = new string[] { })]
        public string[] ExtensionMethods_HasExpectedValue(Type type)
        {
            var extendedType = type.GetMetadata<IClassType>();

            return [.. extendedType.ExtensionMethods.Select(m => m.Name)];
        }

        [TestCase(typeof(TestTypes.TestBaseClass), ExpectedResult = new[] { nameof(TestTypes.ITestInterface) })]
        [TestCase(typeof(TestTypes.TestDerivedClass), ExpectedResult = new[] { nameof(TestTypes.IExtendedTestInterface) })]
        public string[] ImplementedInterfaces_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IClassType>();

            return [.. metadata.ImplementedInterfaces.Select(i => i.Name)];
        }

        [TestCase(typeof(TestTypes.TestBaseClass), ExpectedResult = new[] {
            nameof(TestTypes.TestDerivedClass)                // Directly inherits TestBaseClass
        })]
        [TestCase(typeof(TestTypes.GenericBaseClass<>), ExpectedResult = new[] {
            nameof(TestTypes.ConstructedGenericDerivedClass), // Inherits GenericBaseClass<string>
            "GenericDerivedClass`1"                           // Directly inherits GenericBaseClass<T>
        })]
        public string[] DerivedTypes_WithNoGenericBaseClass_ReturnsExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IClassType>();

            return [.. metadata.DerivedTypes.Select(t => t.Name).OrderBy(n => n)];
        }

        [TestCase(typeof(TestTypes.TestBaseClass), typeof(TestTypes.TestBaseClass), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.TestBaseClass), typeof(TestTypes.TestDerivedClass), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.TestDerivedClass), typeof(TestTypes.TestBaseClass), ExpectedResult = false)]
        [TestCase(typeof(TestTypes.TestDerivedClass), typeof(TestTypes.ITestInterface), ExpectedResult = false)]
        [TestCase(typeof(TestTypes.GenericBaseClass<>), typeof(TestTypes.GenericDerivedClass<>), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.GenericBaseClass<int>), typeof(TestTypes.ConstructedGenericDerivedClass), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.GenericDerivedClass<>), typeof(TestTypes.GenericBaseClass<>), ExpectedResult = false)]
        [TestCase(typeof(TestTypes.GenericBaseClass<int>), typeof(TestTypes.GenericBaseClass<long>), ExpectedResult = false)]
        public bool IsAssignableFrom_ReturnsExpectedResult(Type targetType, Type sourceType)
        {
            var targetMetadata = targetType.GetMetadata<IClassType>();
            var sourceMetadata = sourceType.GetMetadata();

            return targetMetadata.IsAssignableFrom(sourceMetadata);
        }

        [TestCase(typeof(TestTypes.TestDerivedClass), typeof(TestTypes.TestBaseClass), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.TestGrandChildClass), typeof(TestTypes.TestDerivedClass), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.TestGrandChildClass), typeof(TestTypes.TestBaseClass), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.TestGrandChildClass), typeof(object), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.TestBaseClass), typeof(TestTypes.TestDerivedClass), ExpectedResult = false)]
        [TestCase(typeof(TestTypes.GenericDerivedClass<>), typeof(TestTypes.GenericBaseClass<>), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.ConstructedGenericDerivedClass), typeof(TestTypes.GenericBaseClass<int>), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.ConstructedGenericDerivedClass), typeof(TestTypes.GenericBaseClass<>), ExpectedResult = false)]
        [TestCase(typeof(TestTypes.TestDerivedClass), typeof(TestTypes.TestDerivedClass), ExpectedResult = false)]
        public bool IsSubclassOf_ReturnsExpectedResult(Type type, Type superType)
        {
            var metadata = type.GetMetadata<IClassType>();
            var baseMetadata = superType.GetMetadata<IClassType>();

            return metadata.IsSubclassOf(baseMetadata);
        }
    }
}

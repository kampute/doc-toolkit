// Copyright (C) 2025 Kampute
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

    [TestFixture]
    public class StructTypeTests
    {
        [TestCase(typeof(DateTime), nameof(DateTime))]
        [TestCase(typeof(KeyValuePair<,>), "KeyValuePair`2")]
        [TestCase(typeof(KeyValuePair<string, int>), "KeyValuePair`2")]
        [TestCase(typeof(TestTypes.TestValueType), nameof(TestTypes.TestValueType))]
        public void ImplementsStructType(Type structType, string expectedName)
        {
            var metadata = structType.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IStructType>());
            Assert.That(metadata.Name, Is.EqualTo(expectedName));
        }

        [TestCase(typeof(ReadOnlySpan<>), true)]
        [TestCase(typeof(TestTypes.TestValueType), false)]
        public void IsReadonly_HasExpectedValue(Type structType, bool isReadOnly)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.IsReadOnly, Is.EqualTo(isReadOnly));
        }

        [TestCase(typeof(ReadOnlySpan<>), true)]
        [TestCase(typeof(TestTypes.TestValueType), false)]
        public void IsRef_HasExpectedValue(Type structType, bool isRef)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.IsRef, Is.EqualTo(isRef));
        }

        [Test]
        public void GenericTypeDefinition_HasCorrectGenericMetadata()
        {
            var metadata = typeof(KeyValuePair<,>).GetMetadata<IStructType>();

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
            var metadata = typeof(KeyValuePair<int, string>).GetMetadata<IStructType>();

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
        [TestCase(typeof(KeyValuePair<,>), 0, 2)]
        [TestCase(typeof(KeyValuePair<string, int>), 0, 2)]
        [TestCase(typeof(ReadOnlySpan<byte>), 0, 1)]
        [TestCase(typeof(TestTypes.TestValueType), 0, 0)]
        public void OwnGenericParameterRange_HasExpectedValue(Type structType, int expectedOffset, int expectedCount)
        {
            var metadata = structType.GetMetadata<IGenericCapableType>();

            Assert.That(metadata.OwnGenericParameterRange, Is.EqualTo((expectedOffset, expectedCount)));
        }

        [TestCase(typeof(TestTypes.TestValueType))]
        public void Interfaces_HasExpectedValue(Type structType)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.Interfaces, Is.Not.Empty);
        }

        [TestCase(typeof(TestTypes.TestValueType))]
        public void Fields_HasExpectedValue(Type structType)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.Fields, Is.Not.Empty);
            Assert.That(metadata.Fields.Where(f => !f.IsVisible), Is.Empty);
        }

        [TestCase(typeof(TestTypes.TestValueType))]
        public void Constructors_HasExpectedValue(Type structType)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.Constructors, Is.Not.Empty);
            Assert.That(metadata.Constructors.Where(c => !c.IsVisible), Is.Empty);
        }

        [TestCase(typeof(TestTypes.TestValueType))]
        public void Methods_HasExpectedValue(Type structType)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.Methods, Is.Not.Empty);
            Assert.That(metadata.Methods.Where(m => !m.IsVisible), Is.Empty);
        }

        [TestCase(typeof(TestTypes.TestValueType))]
        public void Properties_HasExpectedValue(Type structType)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.Properties, Is.Not.Empty);
            Assert.That(metadata.Properties.Where(p => !p.IsVisible), Is.Empty);
        }

        [TestCase(typeof(TestTypes.TestValueType))]
        public void Events_HasExpectedValue(Type structType)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.Events, Is.Not.Empty);
            Assert.That(metadata.Events.Where(e => !e.IsVisible), Is.Empty);
        }

        [TestCase(typeof(TestTypes.TestValueType))]
        public void Operators_HasExpectedValue(Type structType)
        {
            var metadata = structType.GetMetadata<IStructType>();

            Assert.That(metadata.Operators, Is.Not.Empty);
            Assert.That(metadata.Operators.Where(o => !o.IsVisible), Is.Empty);
        }

        [TestCase(typeof(TestTypes.TestValueType.NestedValueType), ExpectedResult = new string[] { })]
        [TestCase(typeof(TestTypes.TestValueType), ExpectedResult = new[] { nameof(TestTypes.TestValueType.NestedValueType) })]
        public string[] NestedTypes_HasExpectedValue(Type structType)
        {
            var metadata = structType.GetMetadata<IStructType>();

            return [.. metadata.NestedTypes.Select(static t => t.Name).OrderBy(n => n)];
        }

        [TestCase(typeof(DateTime), ExpectedResult = "T:System.DateTime")]
        [TestCase(typeof(Nullable<>), ExpectedResult = "T:System.Nullable`1")]
        [TestCase(typeof(TestTypes.TestValueType), ExpectedResult = "T:Kampute.DocToolkit.Test.TestTypes.TestValueType")]
        public string CodeReference_HasExpectedValue(Type structType)
        {
            var metadata = structType.GetMetadata<IStructType>();

            return metadata.CodeReference;
        }

        [TestCase(typeof(DateTime), ExpectedResult = new string[] { })]
        [TestCase(typeof(TestTypes.TestValueType), ExpectedResult = new[] { nameof(TestTypes) })]
        [TestCase(typeof(TestTypes.TestValueType.NestedValueType), ExpectedResult = new[] { nameof(TestTypes), nameof(TestTypes.TestValueType) })]
        public string[] DeclaringTypeHierarchy_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IStructType>();

            return [.. metadata.DeclaringTypeHierarchy.Select(t => t.Name)];
        }

        [TestCase(typeof(DateTime), ExpectedResult = new[] { nameof(Object), nameof(ValueType) })]
        public string[] BaseTypeHierarchy_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IStructType>();

            return [.. metadata.BaseTypeHierarchy.Select(t => t.Name)];
        }

        [TestCase(typeof(TestTypes.TestValueType), ExpectedResult = new[] { nameof(TestTypes.ITestInterface) })]
        [TestCase(typeof(TestTypes.TestValueType.NestedValueType), ExpectedResult = new string[] { })]
        public string[] ImplementedInterfaces_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IStructType>();

            return [.. metadata.ImplementedInterfaces.Select(i => i.Name).OrderBy(n => n)];
        }

        [TestCase(typeof(TestTypes.TestValueType), typeof(TestTypes.TestValueType), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.TestValueType), typeof(TestTypes.TestValueType.NestedValueType), ExpectedResult = false)]
        [TestCase(typeof(TestTypes.TestValueType), typeof(TestTypes.ITestInterface), ExpectedResult = false)]
        [TestCase(typeof(TestTypes.TestValueType), typeof(TestTypes.TestBaseClass), ExpectedResult = false)]
        public bool IsAssignableFrom_ReturnsExpectedResult(Type targetType, Type sourceType)
        {
            var targetMetadata = targetType.GetMetadata<IStructType>();
            var sourceMetadata = sourceType.GetMetadata();

            return targetMetadata.IsAssignableFrom(sourceMetadata);
        }
    }
}

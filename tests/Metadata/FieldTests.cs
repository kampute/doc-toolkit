// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.Reflection;

    [TestFixture]
    public class FieldTests
    {
        private readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        [TestCase(nameof(Acme.Widget.message))]
        [TestCase(nameof(Acme.Widget.defaultDirection))]
        [TestCase(nameof(Acme.Widget.PI))]
        [TestCase(nameof(Acme.Widget.monthlyAverage))]
        [TestCase(nameof(Acme.Widget.array1))]
        [TestCase(nameof(Acme.Widget.array2))]
        [TestCase(nameof(Acme.Widget.pCount))]
        [TestCase(nameof(Acme.Widget.ppValues))]
        public void ImplementsField(string fieldName)
        {
            var fieldInfo = typeof(Acme.Widget).GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IField>());
            Assert.That(metadata.Name, Is.EqualTo(fieldName));
        }

        [TestCase(nameof(Acme.Widget.message), ExpectedResult = "String")]
        [TestCase(nameof(Acme.Widget.defaultDirection), ExpectedResult = "Direction")]
        [TestCase(nameof(Acme.Widget.PI), ExpectedResult = "Double")]
        [TestCase(nameof(Acme.Widget.monthlyAverage), ExpectedResult = "Double")]
        [TestCase(nameof(Acme.Widget.array1), ExpectedResult = "Int64[]")]
        [TestCase(nameof(Acme.Widget.array2), ExpectedResult = "Widget[,]")]
        [TestCase(nameof(Acme.Widget.pCount), ExpectedResult = "Int32*")]
        [TestCase(nameof(Acme.Widget.ppValues), ExpectedResult = "Single**")]
        public string FieldType_HasExpectedValue(string fieldName)
        {
            var fieldInfo = typeof(Acme.Widget).GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Type.Name;
        }

        [TestCase(nameof(Acme.Widget.pCount), ExpectedResult = true)]
        [TestCase(nameof(Acme.Widget.message), ExpectedResult = false)]
        public bool IsUnsafe_HasExpectedValue(string fieldName)
        {
            var fieldInfo = typeof(Acme.Widget).GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsUnsafe;
        }

        [TestCase(nameof(Acme.Widget.monthlyAverage), ExpectedResult = true)]
        [TestCase(nameof(Acme.Widget.message), ExpectedResult = false)]
        public bool IsReadOnly_HasExpectedValue(string fieldName)
        {
            var fieldInfo = typeof(Acme.Widget).GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsReadOnly;
        }

        [TestCase("valid", ExpectedResult = true)]
        [TestCase(nameof(Acme.Widget.message), ExpectedResult = false)]
        public bool IsVolatile_HasExpectedValue(string fieldName)
        {
            var fieldInfo = typeof(Acme.Widget).GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsVolatile;
        }

        [TestCase(nameof(Acme.ValueType.buffer), ExpectedResult = true)]
        [TestCase(nameof(Acme.ValueType.total), ExpectedResult = false)]
        public bool IsFixedSizeBuffer_HasExpectedValue(string fieldName)
        {
            var fieldInfo = typeof(Acme.ValueType).GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsFixedSizeBuffer;
        }

        [TestCase(nameof(Acme.Widget.PI), ExpectedResult = true)]
        [TestCase(nameof(Acme.Widget.message), ExpectedResult = false)]
        public bool IsLiteral_HasExpectedValue(string fieldName)
        {
            var fieldInfo = typeof(Acme.Widget).GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsLiteral;
        }

        [TestCase(nameof(Acme.Widget.PI), ExpectedResult = 3.14159)]
        [TestCase(nameof(Acme.Widget.message), ExpectedResult = null)]
        public object? LiteralValue_HasExpectedValue(string fieldName)
        {
            var fieldInfo = typeof(Acme.Widget).GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.LiteralValue;
        }

        [TestCase(nameof(Acme.Widget.message), ExpectedResult = MemberVisibility.Public)]
        [TestCase(nameof(Acme.Widget.defaultDirection), ExpectedResult = MemberVisibility.Internal)]
        [TestCase(nameof(Acme.Widget.monthlyAverage), ExpectedResult = MemberVisibility.ProtectedInternal)]
        [TestCase(nameof(Acme.Widget.PI), ExpectedResult = MemberVisibility.Public)]
        [TestCase("valid", ExpectedResult = MemberVisibility.Private)]
        public MemberVisibility Visibility_HasExpectedValue(string fieldName)
        {
            var fieldInfo = typeof(Acme.Widget).GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Visibility;
        }

        [TestCase(nameof(Acme.Widget.defaultDirection), ExpectedResult = true)]
        [TestCase(nameof(Acme.Widget.PI), ExpectedResult = true)]
        [TestCase(nameof(Acme.Widget.message), ExpectedResult = false)]
        [TestCase(nameof(Acme.Widget.monthlyAverage), ExpectedResult = false)]
        public bool IsStatic_HasExpectedValue(string fieldName)
        {
            var fieldInfo = typeof(Acme.Widget).GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsStatic;
        }

        [TestCase(nameof(Acme.Widget.message), ExpectedResult = true)]
        [TestCase(nameof(Acme.Widget.PI), ExpectedResult = true)]
        [TestCase(nameof(Acme.Widget.defaultDirection), ExpectedResult = false)]
        [TestCase(nameof(Acme.Widget.monthlyAverage), ExpectedResult = true)]
        public bool IsVisible_HasExpectedValue(string fieldName)
        {
            var fieldInfo = typeof(Acme.Widget).GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsVisible;
        }

        [TestCase(nameof(Acme.Widget.message), ExpectedResult = false)]
        public bool IsSpecialName_HasExpectedValue(string fieldName)
        {
            var fieldInfo = typeof(Acme.Widget).GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsSpecialName;
        }

        [TestCase(nameof(Acme.Widget.message))]
        public void DeclaringType_HasExpectedType(string fieldName)
        {
            var fieldInfo = typeof(Acme.Widget).GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.DeclaringType, Is.Not.Null);
            Assert.That(metadata.DeclaringType.Name, Is.EqualTo(nameof(Acme.Widget)));
        }

        [TestCase(typeof(Acme.ValueType), nameof(Acme.ValueType.total), ExpectedResult = "F:Acme.ValueType.total")]
        [TestCase(typeof(Acme.Widget.NestedClass), nameof(Acme.Widget.NestedClass.value), ExpectedResult = "F:Acme.Widget.NestedClass.value")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.message), ExpectedResult = "F:Acme.Widget.message")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.defaultDirection), ExpectedResult = "F:Acme.Widget.defaultDirection")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.PI), ExpectedResult = "F:Acme.Widget.PI")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.monthlyAverage), ExpectedResult = "F:Acme.Widget.monthlyAverage")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.array1), ExpectedResult = "F:Acme.Widget.array1")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.array2), ExpectedResult = "F:Acme.Widget.array2")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.pCount), ExpectedResult = "F:Acme.Widget.pCount")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.ppValues), ExpectedResult = "F:Acme.Widget.ppValues")]
        public string CodeReference_HasExpectedValue(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CodeReference;
        }

        [Test]
        public void TryGetFixedSizeBufferInfo_ReturnsExpectedValues()
        {
            var fieldInfo = typeof(Acme.ValueType).GetField(nameof(Acme.ValueType.buffer), bindingFlags);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            var result = metadata.TryGetFixedSizeBufferInfo(out var elementType, out var length);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(elementType, Is.Not.Null);
            }
            using (Assert.EnterMultipleScope())
            {
                Assert.That(elementType.Name, Is.EqualTo("Int32"));
                Assert.That(length, Is.EqualTo(10));
            }
        }
    }
}

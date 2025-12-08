// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class FieldTests
    {
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField))]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField))]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField))]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.FixedBuffer))]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ComplexField))]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.InternalField))]
        public void ImplementsField(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.InstanceOf<IField>());
            Assert.That(metadata.Name, Is.EqualTo(fieldName));
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = nameof(String))]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField), ExpectedResult = nameof(Int32))]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField), ExpectedResult = nameof(Int32))]
        public string FieldType_HasExpectedValue(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Type.Name;
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.FixedBuffer), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ComplexField), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = false)]
        public bool IsUnsafe_HasExpectedValue(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsUnsafe;
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = false)]
        public bool IsReadOnly_HasExpectedValue(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsReadOnly;
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.InternalField), ExpectedResult = false)]
        public bool IsVolatile_HasExpectedValue(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsVolatile;
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.FixedBuffer), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ArrayField), ExpectedResult = false)]
        public bool IsFixedSizeBuffer_HasExpectedValue(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsFixedSizeBuffer;
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField), ExpectedResult = false)]
        public bool IsLiteral_HasExpectedValue(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsLiteral;
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = "Constant")]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField), ExpectedResult = null)]
        public object? LiteralValue_HasExpectedValue(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.LiteralValue;
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.InternalField), ExpectedResult = MemberVisibility.Internal)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField), ExpectedResult = MemberVisibility.Public)]
        public MemberVisibility Visibility_HasExpectedValue(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Visibility;
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.InternalField), ExpectedResult = false)]
        public bool IsStatic_HasExpectedValue(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsStatic;
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.InternalField), ExpectedResult = false)]
        public bool IsVisible_HasExpectedValue(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsVisible;
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = false)]
        public bool IsSpecialName_HasExpectedValue(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsSpecialName;
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField))]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField))]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ComplexField))]
        public void DeclaringType_HasExpectedType(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.DeclaringType.Represents(declaringType), Is.True);
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = "F:Acme.SampleFields.ConstField")]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ComplexField), ExpectedResult = "F:Acme.SampleFields.ComplexField")]
        public string CodeReference_HasExpectedValue(Type declaringType, string fieldName)
        {
            var fieldInfo = declaringType.GetField(fieldName, Acme.Bindings.AllDeclared);
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CodeReference;
        }

        [Test]
        public void TryGetFixedSizeBufferInfo_ReturnsExpectedValues()
        {
            var fieldInfo = typeof(Acme.SampleFields).GetField(nameof(Acme.SampleFields.FixedBuffer), Acme.Bindings.AllDeclared);
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
                Assert.That(elementType.Name, Is.EqualTo(nameof(Byte)));
                Assert.That(length, Is.EqualTo(16));
            }
        }
    }
}

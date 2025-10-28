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
    public class PrimitiveTypeTests
    {
        [TestCase(typeof(bool), "Boolean")]
        [TestCase(typeof(byte), "Byte")]
        [TestCase(typeof(sbyte), "SByte")]
        [TestCase(typeof(char), "Char")]
        [TestCase(typeof(double), "Double")]
        [TestCase(typeof(float), "Single")]
        [TestCase(typeof(int), "Int32")]
        [TestCase(typeof(uint), "UInt32")]
        [TestCase(typeof(long), "Int64")]
        [TestCase(typeof(ulong), "UInt64")]
        [TestCase(typeof(short), "Int16")]
        [TestCase(typeof(ushort), "UInt16")]
        public void ImplementsPrimitveType(Type type, string expectedName)
        {
            var metadata = type.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IPrimitiveType>());
            Assert.That(metadata.Name, Is.EqualTo(expectedName));
        }

        [TestCase(typeof(int), nameof(int.MaxValue))]
        [TestCase(typeof(int), nameof(int.MinValue))]
        public void Fields_HasExpectedValue(Type type, string fieldName)
        {
            var metadata = type.GetMetadata<IPrimitiveType>();

            var field = metadata.Fields.FirstOrDefault(f => f.Name == fieldName);
            Assert.That(field, Is.Not.Null);
        }

        [TestCase(typeof(int), nameof(int.ToString))]
        public void Methods_HasExpectedValue(Type type, string methodName)
        {
            var metadata = type.GetMetadata<IPrimitiveType>();

            var method = metadata.Methods.FirstOrDefault(m => m.Name == methodName);
            Assert.That(method, Is.Not.Null);
        }

        [TestCase(typeof(bool), ExpectedResult = "T:System.Boolean")]
        [TestCase(typeof(byte), ExpectedResult = "T:System.Byte")]
        [TestCase(typeof(sbyte), ExpectedResult = "T:System.SByte")]
        [TestCase(typeof(char), ExpectedResult = "T:System.Char")]
        [TestCase(typeof(double), ExpectedResult = "T:System.Double")]
        [TestCase(typeof(float), ExpectedResult = "T:System.Single")]
        [TestCase(typeof(int), ExpectedResult = "T:System.Int32")]
        [TestCase(typeof(uint), ExpectedResult = "T:System.UInt32")]
        [TestCase(typeof(long), ExpectedResult = "T:System.Int64")]
        [TestCase(typeof(ulong), ExpectedResult = "T:System.UInt64")]
        [TestCase(typeof(short), ExpectedResult = "T:System.Int16")]
        [TestCase(typeof(ushort), ExpectedResult = "T:System.UInt16")]
        public string CodeReference_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IPrimitiveType>();

            return metadata.CodeReference;
        }

        [TestCase(typeof(int), typeof(int), ExpectedResult = true)]
        [TestCase(typeof(int), typeof(long), ExpectedResult = false)]
        [TestCase(typeof(bool), typeof(bool), ExpectedResult = true)]
        [TestCase(typeof(bool), typeof(int), ExpectedResult = false)]
        public bool IsAssignableFrom_ReturnsExpectedResult(Type targetType, Type sourceType)
        {
            var targetMetadata = targetType.GetMetadata<IPrimitiveType>();
            var sourceMetadata = sourceType.GetMetadata();

            return targetMetadata.IsAssignableFrom(sourceMetadata);
        }
    }
}

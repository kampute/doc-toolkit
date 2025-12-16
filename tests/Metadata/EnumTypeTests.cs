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
    public class EnumTypeTests
    {
        [TestCase(typeof(DayOfWeek), nameof(DayOfWeek))]
        [TestCase(typeof(AttributeTargets), nameof(AttributeTargets))]
        public void ImplementsEnumType(Type enumType, string expectedName)
        {
            var metadata = enumType.GetMetadata();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata, Is.InstanceOf<IEnumType>());
                Assert.That(metadata.Name, Is.EqualTo(expectedName));
                Assert.That(metadata.BaseType?.FullName, Is.EqualTo("System.Enum"));
            }
        }

        [TestCase(typeof(DayOfWeek), ExpectedResult = "System.Int32")]
        [TestCase(typeof(AttributeTargets), ExpectedResult = "System.Int32")]
        public string UnderlayingType_HasExpectedValue(Type enumType)
        {
            var metadata = enumType.GetMetadata<IEnumType>();

            return metadata.UnderlyingType.FullName;
        }

        [TestCase(typeof(DayOfWeek), ExpectedResult = false)]
        [TestCase(typeof(AttributeTargets), ExpectedResult = true)]
        public bool IsEnumFlag_HasExpectedValue(Type enumType)
        {
            var metadata = enumType.GetMetadata<IEnumType>();

            return metadata.IsFlagsEnum;
        }

        [TestCase(typeof(DayOfWeek))]
        [TestCase(typeof(AttributeTargets))]
        public void Fields_HasEpectedValue(Type enumType)
        {
            var metadata = enumType.GetMetadata<IEnumType>();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.Fields.Select(static f => f.Name), Is.EquivalentTo(Enum.GetNames(enumType)));
        }

        [TestCase(typeof(DayOfWeek), ExpectedResult = "T:System.DayOfWeek")]
        [TestCase(typeof(AttributeTargets), ExpectedResult = "T:System.AttributeTargets")]
        public string CodeReference_HasExpectedValue(Type enumType)
        {
            var metadata = enumType.GetMetadata<IEnumType>();

            return metadata.CodeReference;
        }

        [TestCase(typeof(DayOfWeek),
            typeof(object),
            typeof(ValueType),
            typeof(Enum)
        )]
        public void BaseTypeHierarchy_HasExpectedValue(Type type, params Type[] expectedTypes)
        {
            var metadata = type.GetMetadata<IEnumType>();
            var expectedBaseTypes = expectedTypes.Select(static t => t.GetMetadata<IClassType>());

            Assert.That(metadata.BaseTypeHierarchy, Is.EqualTo(expectedBaseTypes));
        }

        [TestCase(typeof(DayOfWeek), DayOfWeek.Sunday, ExpectedResult = "Sunday")]
        [TestCase(typeof(DayOfWeek), DayOfWeek.Monday, ExpectedResult = "Monday")]
        [TestCase(typeof(DayOfWeek), DayOfWeek.Saturday, ExpectedResult = "Saturday")]
        [TestCase(typeof(AttributeTargets), AttributeTargets.Assembly, ExpectedResult = "Assembly")]
        [TestCase(typeof(AttributeTargets), AttributeTargets.Class, ExpectedResult = "Class")]
        public string? GetEnumName_WithValidEnumValue_ReturnsExpectedName(Type enumType, object value)
        {
            var metadata = enumType.GetMetadata<IEnumType>();

            return metadata.GetEnumName(value);
        }

        [TestCase(typeof(DayOfWeek), 0, ExpectedResult = "Sunday")]
        [TestCase(typeof(DayOfWeek), 1, ExpectedResult = "Monday")]
        [TestCase(typeof(DayOfWeek), 6, ExpectedResult = "Saturday")]
        [TestCase(typeof(AttributeTargets), 1, ExpectedResult = "Assembly")]
        [TestCase(typeof(AttributeTargets), 4, ExpectedResult = "Class")]
        public string? GetEnumName_WithValidIntegerValue_ReturnsExpectedName(Type enumType, int value)
        {
            var metadata = enumType.GetMetadata<IEnumType>();

            return metadata.GetEnumName(value);
        }

        [TestCase(typeof(DayOfWeek), (byte)0, ExpectedResult = "Sunday")]
        [TestCase(typeof(DayOfWeek), (short)1, ExpectedResult = "Monday")]
        [TestCase(typeof(DayOfWeek), (long)2, ExpectedResult = "Tuesday")]
        [TestCase(typeof(AttributeTargets), (byte)1, ExpectedResult = "Assembly")]
        [TestCase(typeof(AttributeTargets), (short)4, ExpectedResult = "Class")]
        public string? GetEnumName_WithDifferentNumericTypes_ReturnsExpectedName(Type enumType, object value)
        {
            var metadata = enumType.GetMetadata<IEnumType>();

            return metadata.GetEnumName(value);
        }

        [TestCase(typeof(DayOfWeek), 99)]
        [TestCase(typeof(DayOfWeek), -1)]
        [TestCase(typeof(AttributeTargets), 999)]
        public void GetEnumName_WithInvalidValue_ReturnsNull(Type enumType, object value)
        {
            var metadata = enumType.GetMetadata<IEnumType>();

            var result = metadata.GetEnumName(value);
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetEnumName_WithFlagsEnumCombinedValue_ReturnsNull()
        {
            var metadata = typeof(AttributeTargets).GetMetadata<IEnumType>();

            var combinedFlags = (int)(AttributeTargets.Class | AttributeTargets.Method);
            var result = metadata.GetEnumName(combinedFlags);

            Assert.That(result, Is.Null);
        }

        [TestCase(typeof(DayOfWeek), typeof(DayOfWeek), ExpectedResult = true)]
        [TestCase(typeof(DayOfWeek), typeof(AttributeTargets), ExpectedResult = false)]
        [TestCase(typeof(DayOfWeek), typeof(int), ExpectedResult = true)]
        public bool IsAssignableFrom_ReturnsExpectedResult(Type targetType, Type sourceType)
        {
            var targetMetadata = targetType.GetMetadata<IEnumType>();
            var sourceMetadata = sourceType.GetMetadata();

            return targetMetadata.IsAssignableFrom(sourceMetadata);
        }
    }
}

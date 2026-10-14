// Copyright (C) Kampute
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
    public class CompositeTypeTests
    {
        [TestCase(typeof(Acme.SampleRecord), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleDerivedRecord), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericRecord<>), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericRecord<int>), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleNominalRecord), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleRecordStruct), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleRecordWithCustomDeconstruct), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleConstructors), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleFields), ExpectedResult = false)]
        [TestCase(typeof(string), ExpectedResult = false)]
        [TestCase(typeof(DateTime), ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleInterface), ExpectedResult = false)]
        public bool IsRecord_ReturnsExpectedResult(Type type)
        {
            return type.GetMetadata<ICompositeType>().IsRecord;
        }

        [TestCase(typeof(Acme.SampleRecord), ExpectedResult = "X, Y")]
        [TestCase(typeof(Acme.SampleDerivedRecord), ExpectedResult = "X, Y, Z")]
        [TestCase(typeof(Acme.SampleGenericRecord<>), ExpectedResult = "Value")]
        [TestCase(typeof(Acme.SampleGenericRecord<int>), ExpectedResult = "Value")]
        [TestCase(typeof(Acme.SampleRecordStruct), ExpectedResult = "A, B")]
        [TestCase(typeof(Acme.SampleNominalRecord), ExpectedResult = null)]
        [TestCase(typeof(Acme.SampleRecordWithCustomDeconstruct), ExpectedResult = null)]
        [TestCase(typeof(Acme.SampleConstructors), ExpectedResult = null)]
        public string? PrimaryConstructor_ReturnsConstructorWithRecordParameters(Type type)
        {
            var primaryConstructor = type.GetMetadata<ICompositeType>().PrimaryConstructor;

            return primaryConstructor is null ? null : string.Join(", ", primaryConstructor.Parameters.Select(static p => p.Name));
        }
    }
}

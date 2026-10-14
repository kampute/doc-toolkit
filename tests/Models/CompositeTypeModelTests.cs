// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Models
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.XmlDoc;
    using NUnit.Framework;
    using System;
    using System.Linq;

    [TestFixture]
    public class CompositeTypeModelTests
    {
        [TestCase(typeof(Acme.SampleRecord), ExpectedResult = ".ctor, Sum, X, Y")]
        [TestCase(typeof(Acme.SampleDerivedRecord), ExpectedResult = ".ctor, Z")]
        [TestCase(typeof(Acme.SampleNominalRecord), ExpectedResult = "Equals, GetHashCode, Name, ToString")]
        [TestCase(typeof(Acme.SampleRecordStruct), ExpectedResult = ".ctor, A, B")]
        [TestCase(typeof(Acme.SampleRecordWithCustomDeconstruct), ExpectedResult = ".ctor, A, Deconstruct")]
        public string Members_OfRecord_IncludeOnlyUserAuthoredMembers(Type recordType)
        {
            // An empty repository leaves every member undocumented, so an implicit default constructor is subject to omission.
            using var context = new DocumentationContext
            (
                new CSharp(),
                MockHelper.CreateAddressProvider(),
                new XmlDocProvider(new XmlDocRepository()),
                new HtmlFormat(),
                [recordType.Assembly.GetMetadata()],
                []
            );

            var model = (CompositeTypeModel)context.FindMember(recordType.GetMetadata())!;

            return string.Join(", ", model.Members.Select(static m => m.Metadata.Name).Order(StringComparer.Ordinal));
        }
    }
}

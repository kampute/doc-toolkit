// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc
{
    using Kampute.DocToolkit.XmlDoc;
    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class XmlDocProviderTests
    {
        private const string XmlDoc =
          @"<?xml version='1.0' encoding='utf-8'?>
            <doc>
                <members>
                    <member name='T:System.NamespaceDoc'>
                        <summary>Contains system components.</summary>
                    </member>
                    <member name='M:System.Object.ToString'>
                        <summary>Returns a string that represents the current object.</summary>
                    </member>
                    <member name='M:System.String.ToString'>
                        <inheritdoc/>
                    </member>
                    <member name='M:System.Int32.ToString'>
                        <inheritdoc cref='M:System.Decimal.ToString'/>
                    </member>
                    <member name='M:System.Double.ToString'>
                        <inheritdoc path='/doc/members/member[@name=""M:System.Decimal.ToString""]/*'/>
                    </member>
                    <member name='M:System.Decimal.ToString'>
                        <include file='included.xml' path='/doc/members/member[@name=""M:System.Decimal.ToString""]/*'/>
                    </member>
                </members>
            </doc>";

        private const string IncludedXmlDoc =
          @"<?xml version='1.0' encoding='utf-8'?>
            <doc>
                <members>
                    <member name='M:System.Decimal.ToString'>
                        <summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
                    </member>
                </members>
            </doc>";

        private static readonly string dir = Path.GetTempPath();
        private static readonly string mainXmlFilePath = Path.Combine(dir, "doc.xml");
        private static readonly string includedXmlFilePath = Path.Combine(dir, "included.xml");
        private XmlDocProvider xmlDocProvider = null!;

        [SetUp]
        public void SetUp()
        {
            File.WriteAllText(mainXmlFilePath, XmlDoc);
            File.WriteAllText(includedXmlFilePath, IncludedXmlDoc);

            xmlDocProvider = new XmlDocProvider();
            xmlDocProvider.ImportFile(mainXmlFilePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(mainXmlFilePath))
                File.Delete(mainXmlFilePath);
            if (File.Exists(includedXmlFilePath))
                File.Delete(includedXmlFilePath);
        }

        [Test]
        public void HasDocumentation_ReturnsTrue()
        {
            Assert.That(xmlDocProvider.HasDocumentation, Is.True);
        }

        [TestCase("M:System.Object.ToString", ExpectedResult = "Returns a string that represents the current object.")] // Direct summary
        [TestCase("M:System.String.ToString", ExpectedResult = "Returns a string that represents the current object.")] // Inherited summary
        [TestCase("M:System.Int32.ToString", ExpectedResult = "Converts the numeric value of this instance to its equivalent string representation.")] // Inherited cref summary
        [TestCase("M:System.Double.ToString", ExpectedResult = "Converts the numeric value of this instance to its equivalent string representation.")] // Inherited path summary
        [TestCase("M:System.Decimal.ToString", ExpectedResult = "Converts the numeric value of this instance to its equivalent string representation.")] // Included summary
        [TestCase("N:System", ExpectedResult = null)]
        public string? TryGetDoc_ReturnsCorrectMemberDoc(string cref)
        {
            return xmlDocProvider.TryGetDoc(cref, out var memberDoc) ? memberDoc.Summary.ToString() : null;
        }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.XmlDoc;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Linq;

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
            xmlDocProvider.ImportFile(Path.ChangeExtension(GetType().Assembly.Location, ".xml"));
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
            return xmlDocProvider.TryGetDoc(cref, out var memberDoc) ? memberDoc.Summary.ToString().Trim() : null;
        }

        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.NonExtensionMethod), ExpectedResult = "A non-extension method to verify correct classification.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.ClassicExtensionMethod), ExpectedResult = "A classic extension method.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethod), ExpectedResult = "An instance extension method.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethod), ExpectedResult = "A static extension method.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.GenericExtensionMethod), ExpectedResult = "A generic extension method.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.ClassicExtensionMethodForClass), ExpectedResult = "A classic extension method for generic class.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethodForClass), ExpectedResult = "An instance extension method for generic class.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethodForClass), ExpectedResult = "A static extension method for generic class.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.GenericExtensionMethodForClass), ExpectedResult = "A generic extension method for generic class.")]
        public string? TryGetMemberDoc_ForMethod_ReturnsCorrectMemberDoc(Type type, string methodName)
        {
            var container = type.GetMetadata<IClassType>();
            Assert.That(container, Is.Not.Null);

            var method = container.Methods.FirstOrDefault(m => m.Name == methodName);
            Assert.That(method, Is.Not.Null);

            return xmlDocProvider.TryGetMemberDoc(method, out var doc) ? doc.Summary.ToString().Trim() : null;
        }

        [TestCase(typeof(Acme.SampleExtensions), "InstanceExtensionProperty", ExpectedResult = "An instance extension property.")]
        [TestCase(typeof(Acme.SampleExtensions), "StaticExtensionProperty", ExpectedResult = "A static extension property.")]
        [TestCase(typeof(Acme.SampleExtensions), "InstanceExtensionPropertyForClass", ExpectedResult = "An instance extension property for generic class.")]
        [TestCase(typeof(Acme.SampleExtensions), "StaticExtensionPropertyForClass", ExpectedResult = "A static extension property for generic class.")]
        public string? TryGetMemberDoc_ForProperty_ReturnsCorrectMemberDoc(Type type, string propertyName)
        {
            var container = type.GetMetadata<IClassType>();
            Assert.That(container, Is.Not.Null);

            var property = container.Properties.FirstOrDefault(m => m.Name == propertyName);
            Assert.That(property, Is.Not.Null);

            return xmlDocProvider.TryGetMemberDoc(property, out var doc) ? doc.Summary.ToString().Trim() : null;
        }
    }
}

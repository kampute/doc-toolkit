// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Xslt
{
    using Kampute.DocToolkit.Xslt;
    using NUnit.Framework;
    using System.IO;
    using System.Xml;
    using System.Xml.Xsl;

    [TestFixture]
    public class XsltToolsTests
    {
        private const string StyleSheet = """
            <xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <!-- A comment that must not affect the output -->
              <xsl:output method="text"/>
              <xsl:template match="/">
                <xsl:value-of select="/value"/>
              </xsl:template>
            </xsl:stylesheet>
            """;

        private static string Apply(XslCompiledTransform transform, string xml)
        {
            using var reader = XmlReader.Create(new StringReader(xml));
            using var writer = new StringWriter();
            transform.Transform(reader, null, writer);
            return writer.ToString();
        }

        [Test]
        public void Compile_WithStyleSheetText_ReturnsWorkingTransform()
        {
            var transform = XsltCompiler.Compile(StyleSheet);

            Assert.That(Apply(transform, "<value>42</value>"), Is.EqualTo("42"));
        }

        [Test]
        public void Compile_WithNullArguments_ThrowsArgumentNullException()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(static () => XsltCompiler.Compile((string)null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("styleSheet"));
                Assert.That(static () => XsltCompiler.Compile((TextReader)null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("textReader"));
                Assert.That(static () => XsltCompiler.Compile((Stream)null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("stream"));
            }
        }

        [Test]
        public void NormalizeWhitespace_CollapsesWhitespaceWithoutTrimming()
        {
            var tools = new XsltTextTools();

            Assert.That(tools.NormalizeWhitespace("  a \n\t b  "), Is.EqualTo(" a b "));
        }

        [TestCase("no backticks", ExpectedResult = "```")]
        [TestCase("a ``` b", ExpectedResult = "````")]
        [TestCase("a ````` b", ExpectedResult = "``````")]
        public string FenceMarker_IsLongerThanAnyBacktickRunInCode(string code)
        {
            return new XsltMarkdownTools().FenceMarker(code);
        }
    }
}

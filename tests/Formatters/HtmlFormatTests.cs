// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Formatters
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.IO.Writers;
    using NUnit.Framework;
    using System.IO;
    using System.Xml.Linq;

    [TestFixture]
    public class HtmlFormatTests
    {
        private readonly HtmlFormat format = new();

        [Test]
        public void FileExtension_ReturnsHtmlExtension()
        {
            Assert.That(format.FileExtension, Is.EqualTo(".html"));
        }

        [TestCase("", ExpectedResult = "")]
        [TestCase("test", ExpectedResult = "test")]
        [TestCase("A < B & A > C", ExpectedResult = "A &lt; B &amp; A &gt; C")]
        public string Encode_EncodesHtmlCharacters(string input)
        {
            using var writer = new StringWriter();

            format.Encode(input, writer);

            return writer.ToString();
        }

        [TestCase("", ExpectedResult = "")]
        [TestCase("Example <c>code</c>", ExpectedResult = "Example <code>code</code>")]
        [TestCase("Example: <code>var x = 16;              // Assign x\nvar y = x * x + 2 x + 5; // Calculate y</code>", ExpectedResult = "Example: <pre dir=\"ltr\" aria-label=\"Code snippet\"><code>var x = 16;              // Assign x\nvar y = x * x + 2 x + 5; // Calculate y</code></pre>")]
        public string Transform_WritesExpectedHtml(string xmlContent)
        {
            var element = XElement.Parse($"<summary>{xmlContent}</summary>");
            using var writer = new StringWriter();

            format.Transform(writer, element);

            return writer.ToString().Replace("\r", string.Empty);
        }

        [Test]
        public void CreateMarkupWriter_ReturnsHtmlWriterInstance()
        {
            using var writer = new StringWriter();

            using var markdownWriter = format.CreateMarkupWriter(writer);

            Assert.That(markdownWriter, Is.InstanceOf<HtmlWriter>());
        }
    }
}

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
    public class MarkdownFormatTests
    {
        private readonly MarkdownFormat format = new();

        [Test]
        public void FileExtension_ReturnsMarkdownExtension()
        {
            Assert.That(format.FileExtension, Is.EqualTo(".md"));
        }

        [TestCase("", ExpectedResult = "")]
        [TestCase("test", ExpectedResult = "test")]
        [TestCase("> test", ExpectedResult = "\\> test")]
        [TestCase("A > B * C", ExpectedResult = "A > B \\* C")]
        public string Encode_EncodesMarkdownCharacters(string input)
        {
            using var writer = new StringWriter();

            format.Encode(input, writer);

            return writer.ToString();
        }

        [TestCase("", ExpectedResult = "")]
        [TestCase("Example <c>code</c>", ExpectedResult = "Example `code`")]
        [TestCase("Example: <code>var x = 16;              // Assign x\nvar y = x * x + 2 x + 5; // Calculate y</code>", ExpectedResult = "Example: \n```\nvar x = 16;              // Assign x\nvar y = x * x + 2 x + 5; // Calculate y\n```\n")]
        public string Transform_WritesExpectedMarkdown(string xmlContent)
        {
            var element = XElement.Parse($"<summary>{xmlContent}</summary>");
            using var writer = new StringWriter();

            format.Transform(writer, element);

            return writer.ToString().Replace("\r", string.Empty);
        }

        [Test]
        public void CreateMarkupWriter_ReturnsMarkdownWriterInstance()
        {
            using var writer = new StringWriter();

            using var markdownWriter = format.CreateMarkupWriter(writer);

            Assert.That(markdownWriter, Is.InstanceOf<MarkdownWriter>());
        }
    }
}

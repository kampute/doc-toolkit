// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Formatters
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.IO.Minifiers;
    using Kampute.DocToolkit.IO.Writers;
    using NUnit.Framework;
    using System;
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
        public void Constructor_WithNullFileExtension_ThrowsArgumentNullException()
        {
            Assert.That(() => new MarkdownFormat(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("fileExtension"));
        }

        [TestCase(".md")]
        [TestCase(".markdown")]
        [TestCase(".mdx")]
        public void TextTransformers_ForMarkdownExtensionsAndTargetExtension_UseMarkdownLinkTransformer(string extension)
        {
            var customFormat = new MarkdownFormat(".mdx");

            Assert.That(customFormat.TextTransformers.TryGet(extension, out var transformer) ? transformer : null, Is.InstanceOf<MarkdownLinkTransformer>());
        }

        [Test]
        public void CreateMarkupWriter_ReturnsMarkdownWriterInstance()
        {
            using var writer = new StringWriter();

            using var markdownWriter = format.CreateMarkupWriter(writer);

            Assert.That(markdownWriter, Is.InstanceOf<MarkdownWriter>());
        }

        [TestCase(true, ExpectedResult = true)]
        [TestCase(false, ExpectedResult = false)]
        public bool CreateMarkupWriter_DisposesWrappedWriterOnlyWhenRequested(bool disposeWriter)
        {
            var writer = new StringWriter();

            format.CreateMarkupWriter(writer, disposeWriter).Dispose();

            try
            {
                writer.Write('x');
                return false;
            }
            catch (ObjectDisposedException)
            {
                return true;
            }
        }

        [Test]
        public void CreateMinifier_ReturnsMarkdownMinifierThatOwnsWrappedWriter()
        {
            var writer = new StringWriter();

            var minifier = format.CreateMinifier(writer);
            minifier.Dispose();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(minifier, Is.InstanceOf<MarkdownMinifier>());
                Assert.That(() => writer.Write('x'), Throws.TypeOf<ObjectDisposedException>());
            }
        }
    }
}

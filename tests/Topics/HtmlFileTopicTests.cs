// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Topics
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Topics;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class HtmlFileTopicTests
    {
        private readonly string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        [TestCase("test-file.html", "<html><head><title>Test Title</title></head><body>Test content</body></html>", ExpectedResult = "Test Title")]
        [TestCase("no_title.html", "<html><head></head><body>No title here</body></html>", ExpectedResult = "No Title")]
        [TestCase("empty.html", "", ExpectedResult = "Empty")]
        public string? Title_ReturnsExpectedTitle(string fileName, string content)
        {
            var path = Path.Combine(tempDir, fileName);
            File.WriteAllText(path, content);

            var topic = new HtmlFileTopic(Path.GetFileNameWithoutExtension(fileName), path);

            return topic.Title;
        }

        [Test]
        public void Render_WithValidHtml_RendersBodyContent()
        {
            var html = "<html><head><title>Test</title></head><body>Body content</body></html>";

            Assert.That(GetHtmlContent(html), Is.EqualTo("Body content"));
        }

        [Test]
        public void Render_WithComplexHtmlBody_RendersBodyContent()
        {
            var html = "<html><head><title>Complex</title></head><body><h1>Header</h1><p>Paragraph</p><ul><li>Item 1</li><li>Item 2</li></ul></body></html>";

            Assert.That(GetHtmlContent(html), Is.EqualTo("<h1>Header</h1><p>Paragraph</p><ul><li>Item 1</li><li>Item 2</li></ul>"));
        }

        [Test]
        public void Render_WithBodyWithAttributes_RendersBodyContent()
        {
            var html = "<html><head><title>Attributes</title></head><body class=\"main\" id=\"content\"><p>Content with attributes</p></body></html>";

            Assert.That(GetHtmlContent(html), Is.EqualTo("<p>Content with attributes</p>"));
        }

        [Test]
        public void Render_WithEmptyBodyTag_OutputsEmptyString()
        {
            var html = "<html><head><title>Empty Body</title></head><body></body></html>";

            Assert.That(GetHtmlContent(html), Is.Empty);
        }

        [Test]
        public void Render_WithOnlyBodyContent_RendersBodyContent()
        {
            var html = "<body>Just body content</body>";

            Assert.That(GetHtmlContent(html), Is.EqualTo("Just body content"));
        }

        [Test]
        public void Render_WithNoBodyTag_FallsBackToFullContent()
        {
            var html = "<html><head><title>No Body</title></head><div>Content outside body</div></html>";

            Assert.That(GetHtmlContent(html), Is.EqualTo(html));
        }

        [Test]
        public void Render_WithMalformedHtml_FallsBackToFullContent()
        {
            var html = "<html><head><title>Malformed</head><body>Unclosed tags</html>";

            Assert.That(GetHtmlContent(html), Is.EqualTo(html));
        }

        [Test]
        public void Render_WithEmptyHtml_OutputsEmptyString()
        {
            Assert.That(GetHtmlContent(string.Empty), Is.Empty);
        }

        private string GetHtmlContent(string html)
        {
            var path = Path.Combine(tempDir, "test.html");
            File.WriteAllText(path, html);

            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var topic = new HtmlFileTopic("test", path);

            using var textWriter = new StringWriter();
            topic.Render(textWriter, context);

            return textWriter.ToString();
        }
    }
}

// Copyright (C) Kampute
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
    public class MarkdownFileTopicTests
    {
        private string tempDir = null!;

        [SetUp]
        public void Setup()
        {
            tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        [TestCase("no-title.md", "", ExpectedResult = "No Title")]
        [TestCase("no-title.md", "# ", ExpectedResult = "No Title")]
        [TestCase("no-title.md", "This is content without a title.", ExpectedResult = "No Title")]
        [TestCase("test-file.md", "# Simple Title", ExpectedResult = "Simple Title")]
        [TestCase("test-file.md", "#Simple Title", ExpectedResult = "Simple Title")]
        [TestCase("test-file.md", "  # Title with leading whitespace", ExpectedResult = "Title with leading whitespace")]
        [TestCase("test-file.md", "\n\n# Title after line breaks", ExpectedResult = "Title after line breaks")]
        [TestCase("test-file.md", "# Title with trailing whitespace  ", ExpectedResult = "Title with trailing whitespace")]
        [TestCase("test-file.md", "Content before title\n# Title in the middle", ExpectedResult = "Test File")]
        [TestCase("test-file.md", "# First Title\n## Second level heading", ExpectedResult = "First Title")]
        [TestCase("test-file.md", "# Title with symbols: &@#!?", ExpectedResult = "Title with symbols: &@#!?")]
        [TestCase("test-file.md", "---\ntitle: Front Matter Title\n---\n# Markdown Title", ExpectedResult = "Front Matter Title")]
        public string? Title_ReturnsExpectedTitle(string fileName, string content)
        {
            var path = Path.Combine(tempDir, fileName);
            File.WriteAllText(path, content);

            var topic = new MarkdownFileTopic(Path.GetFileNameWithoutExtension(path), path);

            return topic.Title;
        }

        [Test]
        public void Render_WithMarkdownFormat_OmitsFrontMatterAndResolvesTopicLinks()
        {
            var path = Path.Combine(tempDir, "page.md");
            File.WriteAllText(path, "---\ntitle: Page\n---\nSee [guide](guide) and [example](https://example.org/).");
            var topic = new MarkdownFileTopic("page", path);
            var guide = MockTopicBuilder.Topic("guide").Build();
            using var context = MockHelper.CreateDocumentationContext<MarkdownFormat>([guide, topic]);
            using var writer = new StringWriter();

            topic.Render(writer, context);

            Assert.That(writer.ToString(), Is.EqualTo("See [guide](https://example.com/guide) and [example](https://example.org/)."));
        }

        [Test]
        public void Render_WithFormatWithoutMarkdownTransformer_ThrowsNotSupportedException()
        {
            var path = Path.Combine(tempDir, "page.md");
            File.WriteAllText(path, "Content");
            var topic = new MarkdownFileTopic("page", path);
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([topic]);

            Assert.That(() => topic.Render(new StringWriter(), context), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void FrontMatter_ExposesValuesFromYamlHeader()
        {
            var path = Path.Combine(tempDir, "page.md");
            File.WriteAllText(path, "---\ntitle: Page\norder: 2\n---\nContent");

            var topic = new MarkdownFileTopic("page", path);

            Assert.That(topic.FrontMatter["title"], Is.EqualTo("Page"));
        }
    }
}

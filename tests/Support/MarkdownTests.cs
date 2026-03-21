// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Support
{
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class MarkdownTests
    {
        [TestCase("**bold text**", ExpectedResult = @"\*\*bold text\*\*")]
        [TestCase("# Header #1", ExpectedResult = @"\# Header #1")]
        [TestCase("text with *italics* and `code`", ExpectedResult = @"text with \*italics\* and \`code\`")]
        [TestCase("[link](http://example.com)", ExpectedResult = @"\[link\](http://example.com)")]
        public string Encode_ReturnsExpectedText(string text)
        {
            return Markdown.Encode(text);
        }

        [TestCase("**bold text**", ExpectedResult = @"\*\*bold text\*\*")]
        [TestCase("# Header #1", ExpectedResult = @"\# Header #1")]
        [TestCase("text with *italics* and `code`", ExpectedResult = @"text with \*italics\* and \`code\`")]
        [TestCase("[link](http://example.com)", ExpectedResult = @"\[link\](http://example.com)")]
        public string Encode_WritesExpectedText(string text)
        {
            using var writer = new StringWriter();
            Markdown.Encode(text, writer);
            return writer.ToString();
        }

        [TestCase(@"\*\*bold text\*\*", ExpectedResult = "**bold text**")]
        [TestCase(@"\# Header #1", ExpectedResult = "# Header #1")]
        [TestCase(@"text with \*italics\*, \`code\`, and path c:\\root", ExpectedResult = "text with *italics*, `code`, and path c:\\root")]
        [TestCase(@"\[link\](http://example.com)", ExpectedResult = "[link](http://example.com)")]
        public string Decode_ReturnsExpectedText(string text)
        {
            return Markdown.Decode(text);
        }

        [TestCase(@"\*\*bold text\*\*", ExpectedResult = "**bold text**")]
        [TestCase(@"\# Header #1", ExpectedResult = "# Header #1")]
        [TestCase(@"text with \*italics\*, \`code\`, and path c:\\root", ExpectedResult = "text with *italics*, `code`, and path c:\\root")]
        [TestCase(@"\[link\](http://example.com)", ExpectedResult = "[link](http://example.com)")]
        public string Decode_WritesExpectedText(string text)
        {
            using var writer = new StringWriter();
            Markdown.Decode(text, writer);
            return writer.ToString();
        }

        [TestCase("", ExpectedResult = 3)]
        [TestCase("Hello `World`", ExpectedResult = 3)]
        [TestCase("``", ExpectedResult = 3)]
        [TestCase("```csharp", ExpectedResult = 4)]
        [TestCase("a``b```c", ExpectedResult = 4)]
        [TestCase("````html", ExpectedResult = 5)]
        public int GetMinimumFenceBackticks_ReturnsExpectedNumber(string text)
        {
            return Markdown.GetMinimumFenceBackticks(text);
        }

        [TestCase("", ExpectedResult = new string[0])]
        [TestCase("Just plain text\nNo headings here", ExpectedResult = new string[0])]
        [TestCase("# My Heading", ExpectedResult = new[] { "1:My Heading" })]
        [TestCase("# Level 1\n## Level 2\n### Level 3", ExpectedResult = new[] { "1:Level 1", "2:Level 2", "3:Level 3" })]
        [TestCase("#\n# Valid", ExpectedResult = new[] { "1:Valid" })]
        [TestCase("text # not a heading\n# Real Heading", ExpectedResult = new[] { "1:Real Heading" })]
        [TestCase("# Heading 1\r\n## Heading 2", ExpectedResult = new[] { "1:Heading 1", "2:Heading 2" })]
        public string[] EnumerateHeadings_ReturnsExpectedHeadings(string content)
        {
            return [.. Markdown.EnumerateHeadings(content).Select(h => $"{h.Level}:{h.Heading}")];
        }

        [TestCase("", ExpectedResult = null)]
        [TestCase("Just plain text", ExpectedResult = null)]
        [TestCase("#", ExpectedResult = null)]
        [TestCase("# ", ExpectedResult = null)]
        [TestCase("# Title", ExpectedResult = "Title")]
        [TestCase("#Title", ExpectedResult = "Title")]
        [TestCase("## Title\n### Section", ExpectedResult = "Title")]
        [TestCase("\n\n## Title", ExpectedResult = "Title")]
        [TestCase("Some text\n# Section", ExpectedResult = null)]
        public string? TryGetFirstHeading_ReturnsExpectedResult(string content)
        {
            return Markdown.TryGetFirstHeading(content, out var heading) ? heading : null;
        }

        [Test]
        public void TryExtractFrontMatter_NoFrontMatter_ReturnsFalse()
        {
            var content = "Just some content";

            var result = Markdown.TryExtractFrontMatter(content, out var frontMatter, out var contentStart);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(frontMatter, Is.Empty);
                Assert.That(contentStart, Is.Zero);
            }
        }

        [Test]
        public void TryExtractFrontMatter_FrontMatterNotAtStart_ReturnsFalse()
        {
            var content = "Some text\n---\nkey: value\n---\n";

            var result = Markdown.TryExtractFrontMatter(content, out var frontMatter, out var contentStart);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(frontMatter, Is.Empty);
                Assert.That(contentStart, Is.Zero);
            }
        }

        [Test]
        public void TryExtractFrontMatter_ValidFrontMatterWithDashes_ReturnsTrue()
        {
            var content = "---\nkey: value\n---\nBody";

            var result = Markdown.TryExtractFrontMatter(content, out var frontMatter, out var contentStart);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(frontMatter, Has.Count.EqualTo(1));
                Assert.That(contentStart, Is.EqualTo(content.IndexOf("Body")));
            }

            Assert.That(frontMatter, Contains.Key("key").WithValue("value"));
        }

        [Test]
        public void TryExtractFrontMatter_ValidFrontMatterWithDots_ReturnsTrue()
        {
            var content = "---\nkey: value\n...\nBody";

            var result = Markdown.TryExtractFrontMatter(content, out var frontMatter, out var contentStart);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(frontMatter, Has.Count.EqualTo(1));
                Assert.That(contentStart, Is.EqualTo(content.IndexOf("Body")));
            }

            Assert.That(frontMatter, Contains.Key("key").WithValue("value"));
        }

        [Test]
        public void TryExtractFrontMatter_EmptyFrontMatter_ReturnsTrue()
        {
            var content = "---\n---\nBody";

            var result = Markdown.TryExtractFrontMatter(content, out var frontMatter, out var contentStart);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(frontMatter, Is.Empty);
                Assert.That(contentStart, Is.EqualTo(content.IndexOf("Body")));
            }
        }

        [Test]
        public void TryExtractFrontMatter_WithCrLf_ReturnsTrue()
        {
            var content = "---\r\nkey: value\r\n---\r\nBody";

            var result = Markdown.TryExtractFrontMatter(content, out var frontMatter, out var contentStart);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(frontMatter, Has.Count.EqualTo(1));
                Assert.That(contentStart, Is.EqualTo(content.IndexOf("Body")));
            }

            Assert.That(frontMatter, Contains.Key("key").WithValue("value"));
        }

        [Test]
        public void TryExtractFrontMatter_InvalidYaml_ThrowsFormatException()
        {
            var content = "---\ninvalid yaml without colon\n---\n";

            Assert.That(() => Markdown.TryExtractFrontMatter(content, out _, out _), Throws.TypeOf<FormatException>());
        }
    }
}

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
        // Only ASCII punctuation can be backslash-escaped (CommonMark 2.4), so an ordered list marker is
        // neutralized by escaping its delimiter, and a line starting with a number is otherwise left alone.
        [TestCase("1. item", ExpectedResult = @"1\. item")]
        [TestCase("text\n  10) item", ExpectedResult = "text\n  10\\) item")]
        [TestCase("2024 was a year. Really.", ExpectedResult = "2024 was a year. Really.")]
        [TestCase("Version 1. Next", ExpectedResult = "Version 1. Next")]
        public string Encode_ReturnsExpectedText(string text)
        {
            return Markdown.Encode(text);
        }

        [TestCase("**bold text**", ExpectedResult = @"\*\*bold text\*\*")]
        [TestCase("# Header #1", ExpectedResult = @"\# Header #1")]
        [TestCase("text with *italics* and `code`", ExpectedResult = @"text with \*italics\* and \`code\`")]
        [TestCase("[link](http://example.com)", ExpectedResult = @"\[link\](http://example.com)")]
        [TestCase("1. item", ExpectedResult = @"1\. item")]
        [TestCase("text\n  10) item", ExpectedResult = "text\n  10\\) item")]
        [TestCase("2024 was a year. Really.", ExpectedResult = "2024 was a year. Really.")]
        [TestCase("Version 1. Next", ExpectedResult = "Version 1. Next")]
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
        // Every ASCII punctuation character can be backslash-escaped, but a backslash before any other character, or at the
        // end of the text, is a literal backslash (CommonMark 0.31, examples 12, 13 and 644).
        [TestCase(@"\!\""\#\$\%\&\'\(\)\*\+\,\-\.\/\:\;\<\=\>\?\@\[\\\]\^\_\`\{\|\}\~", ExpectedResult = @"!""#$%&'()*+,-./:;<=>?@[\]^_`{|}~")]
        [TestCase("\\\u2192\\A\\a\\ \\3\\\u03C6\\\u00AB", ExpectedResult = "\\\u2192\\A\\a\\ \\3\\\u03C6\\\u00AB")]
        [TestCase(@"c:\root", ExpectedResult = @"c:\root")]
        [TestCase(@"text\", ExpectedResult = @"text\")]
        public string Decode_ReturnsExpectedText(string text)
        {
            return Markdown.Decode(text);
        }

        [TestCase(@"\*\*bold text\*\*", ExpectedResult = "**bold text**")]
        [TestCase(@"\# Header #1", ExpectedResult = "# Header #1")]
        [TestCase(@"text with \*italics\*, \`code\`, and path c:\\root", ExpectedResult = "text with *italics*, `code`, and path c:\\root")]
        [TestCase(@"\[link\](http://example.com)", ExpectedResult = "[link](http://example.com)")]
        [TestCase(@"\!\""\#\$\%\&\'\(\)\*\+\,\-\.\/\:\;\<\=\>\?\@\[\\\]\^\_\`\{\|\}\~", ExpectedResult = @"!""#$%&'()*+,-./:;<=>?@[\]^_`{|}~")]
        [TestCase("\\\u2192\\A\\a\\ \\3\\\u03C6\\\u00AB", ExpectedResult = "\\\u2192\\A\\a\\ \\3\\\u03C6\\\u00AB")]
        [TestCase(@"c:\root", ExpectedResult = @"c:\root")]
        [TestCase(@"text\", ExpectedResult = @"text\")]
        public string Decode_WritesExpectedText(string text)
        {
            using var writer = new StringWriter();
            Markdown.Decode(text, writer);
            return writer.ToString();
        }

        [Test]
        public void Methods_WithNullArgument_ThrowArgumentNullExceptionWhenCalled()
        {
            using var writer = new StringWriter();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(() => Markdown.Encode(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("text"));
                Assert.That(() => Markdown.Encode("text", null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("output"));
                Assert.That(() => Markdown.Decode(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("text"));
                Assert.That(() => Markdown.Decode("text", null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("output"));
                Assert.That(() => Markdown.EnumerateHeadings(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("content"));
            }
        }

        [Test]
        public void IsAsciiPunctuation_ReturnsTrueOnlyForAsciiPunctuation()
        {
            // The ASCII punctuation characters of CommonMark 0.31, section 2.1
            const string punctuation = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

            var misclassified = Enumerable.Range(0, 0x100).Select(static i => (char)i)
                .Where(static c => Markdown.IsAsciiPunctuation(c) != punctuation.Contains(c))
                .ToArray();

            Assert.That(misclassified, Is.Empty);
        }

        [TestCase("```", ExpectedResult = "`:3:")]
        [TestCase("   ~~~~ c# ", ExpectedResult = "~:4:c#")]
        [TestCase("```` md", ExpectedResult = "`:4:md")]
        [TestCase("~~~ a`b", ExpectedResult = "~:3:a`b")]
        [TestCase("``` a`b", ExpectedResult = null)]
        [TestCase("``", ExpectedResult = null)]
        [TestCase("    ```", ExpectedResult = null)]
        [TestCase("\t```", ExpectedResult = null)]
        [TestCase("text ```", ExpectedResult = null)]
        [TestCase("", ExpectedResult = null)]
        public string? TryParseCodeFence_ReturnsExpectedFence(string line)
        {
            // A code fence is at least three backticks or tildes indented by up to three spaces, and the info string of a
            // backtick fence cannot contain backticks (CommonMark 0.31, section 4.5, example 145)
            return Markdown.TryParseCodeFence(line, out var fenceChar, out var fenceLength, out var infoString)
                ? $"{fenceChar}:{fenceLength}:{infoString.ToString()}"
                : null;
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
        // ATX headings have 1 to 6 '#' characters, and an optional closing sequence preceded by a space is not part of the text
        [TestCase("# Title ##\n## C# #\n### Title#\n####### Seven", ExpectedResult = new[] { "1:Title", "2:C#", "3:Title#" })]
        // Backslash escapes are resolved in the heading text, and an escaped '#' is not part of the closing sequence (CommonMark 0.31, example 76)
        [TestCase("# C\\# Guide\n### foo \\###\n# foo \\#", ExpectedResult = new[] { "1:C# Guide", "3:foo ###", "1:foo #" })]
        // Lines inside a fenced code block are not headings; the closing fence uses the same character and is at least as long
        // as the opening fence, and an unclosed fence runs to the end of the content (CommonMark 0.31, section 4.5, examples 122 and 124)
        [TestCase("```\n# Code\n```\n# After", ExpectedResult = new[] { "1:After" })]
        [TestCase("~~~ md\n# Code\n```\n# Still code\n~~~\n# After", ExpectedResult = new[] { "1:After" })]
        [TestCase("````\n# Code\n```\n# Still code\n````\n# After", ExpectedResult = new[] { "1:After" })]
        [TestCase("# Before\n```\n# Code", ExpectedResult = new[] { "1:Before" })]
        // A closing fence cannot have an info string, and a fence indented by four spaces is not a fence (CommonMark 0.31, section 4.5)
        [TestCase("```\n# Code\n``` x\n# Still code\n```\n# After", ExpectedResult = new[] { "1:After" })]
        [TestCase("    ```\n# Heading", ExpectedResult = new[] { "1:Heading" })]
        // An info string of a backtick fence cannot contain backticks, so such a line opens no code block (CommonMark 0.31, example 145)
        [TestCase("``` a`b\n# Heading", ExpectedResult = new[] { "1:Heading" })]
        public string[] EnumerateHeadings_ReturnsExpectedHeadings(string content)
        {
            return [.. Markdown.EnumerateHeadings(content).Select(h => $"{h.Level}:{h.Heading}")];
        }

        [TestCase("", ExpectedResult = null)]
        [TestCase("Just plain text", ExpectedResult = null)]
        [TestCase("#", ExpectedResult = null)]
        [TestCase("# ", ExpectedResult = null)]
        [TestCase("# Title", ExpectedResult = "Title")]
        // Unlike CommonMark, the '#' characters need not be followed by whitespace (documented in the remarks)
        [TestCase("#Title", ExpectedResult = "Title")]
        [TestCase("## Title\n### Section", ExpectedResult = "Title")]
        [TestCase("\n\n## Title", ExpectedResult = "Title")]
        [TestCase("Some text\n# Section", ExpectedResult = null)]
        [TestCase("# Title ##", ExpectedResult = "Title")]
        [TestCase("# C# #", ExpectedResult = "C#")]
        [TestCase("# C\\# Guide", ExpectedResult = "C# Guide")]
        [TestCase("####### Seven", ExpectedResult = null)]
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

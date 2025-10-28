// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Support
{
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class MarkdownHelperTests
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
    }
}

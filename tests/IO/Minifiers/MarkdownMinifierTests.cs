// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.IO.Minifiers
{
    using Kampute.DocToolkit.IO.Minifiers;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class MarkdownMinifierTests
    {
        [TestCase("", ExpectedResult = "")]
        [TestCase("\n  \n\n  \n", ExpectedResult = "")]
        [TestCase("Hello,   world!", ExpectedResult = "Hello, world!")]
        [TestCase("Hello,   \nworld!", ExpectedResult = "Hello,\nworld!")]
        [TestCase("\n\n Paragraph  with    *multiple*   spaces.", ExpectedResult = "Paragraph with *multiple* spaces.")]
        [TestCase("Paragraph 1\r\n\r\n\r\nParagraph 2", ExpectedResult = "Paragraph 1\n\nParagraph 2")]
        [TestCase("Some text  \n\n```\n    code   block \n\n\n  content\n```\n", ExpectedResult = "Some text\n\n```\n    code   block \n\n\n  content\n```")]
        [TestCase("-  item 1\n\t - item   1.1\n  -  item  1.2\n -   item 2", ExpectedResult = "- item 1\n  - item 1.1\n  - item 1.2\n- item 2")]
        [TestCase("Here is some `inline   code` in text.", ExpectedResult = "Here is some `inline code` in text.")]
        [TestCase("  ##  Subheader  \n\nParagraph    with  extra  spaces.\n\n-  List  item 1\n-  List  item 2\n\t>  Quote line  \n", ExpectedResult = "## Subheader\n\nParagraph with extra spaces.\n\n- List item 1\n- List item 2\n  > Quote line")]
        [TestCase("Some   leading   text \n\n\n````md\n# Test\nStart\n\n  ```\n  nested\n  code\n  block\n  ```\nEnd\n\n````\r\n Some  trailing  text \n\n\nBye!", ExpectedResult = "Some leading text\n\n````md\n# Test\nStart\n\n  ```\n  nested\n  code\n  block\n  ```\nEnd\n\n````\nSome trailing text\n\nBye!")]
        public string WriteMinifiedMarkdown(string text)
        {
            using var writer = new MarkdownMinifier(new StringWriter());

            writer.Write(text);

            return writer.ToString();
        }

        [Test]
        public void Dispose_WhenLeaveOpenIsFalse_ClosesUnderlyingWriter()
        {
            var stringWriter = new StringWriter();
            var writer = new MarkdownMinifier(stringWriter, leaveOpen: false);

            writer.Dispose();

            Assert.Throws<ObjectDisposedException>(() => stringWriter.Write("Test"));
        }

        [Test]
        public void Dispose_WhenLeaveOpenIsTrue_DoesNotCloseUnderlyingWriter()
        {
            var stringWriter = new StringWriter();
            var writer = new MarkdownMinifier(stringWriter, leaveOpen: true);

            writer.Dispose();

            Assert.DoesNotThrow(() => stringWriter.Write("Test"));
        }
    }
}

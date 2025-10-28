// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.IO.Writers
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.IO.Writers;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    [TestFixture]
    public class MarkdownWriterTests
    {
        [TestCase("Hello *world*", ExpectedResult = "Hello \\*world\\*")]
        [TestCase("1 > 2 & 3 < 4", ExpectedResult = "\\1 > 2 & 3 \\< 4")]
        [TestCase("", ExpectedResult = "")]
        public string WriteUnsafe_WritesExpectedContent(string text)
        {
            using var writer = new MarkdownWriter(new StringWriter());

            writer.Write(text);

            return writer.ToString();
        }

        [TestCase("Hello *world*", ExpectedResult = "Hello *world*")]
        [TestCase("1 > 2 & 3 < 4", ExpectedResult = "1 > 2 & 3 < 4")]
        [TestCase("", ExpectedResult = "")]
        public string WriteSafe_WritesExpectedContent(string text)
        {
            using var writer = new MarkdownWriter(new StringWriter());

            writer.WriteSafe(text);

            return writer.ToString();
        }

        [TestCase(typeof(Range), NameQualifier.None, ExpectedResult = "[Range](https://example.com/system.range)")]
        [TestCase(typeof(Range[]), NameQualifier.None, ExpectedResult = "[Range](https://example.com/system.range)\\[\\]")]
        [TestCase(typeof(Range[,]), NameQualifier.None, ExpectedResult = "[Range](https://example.com/system.range)\\[,\\]")]
        [TestCase(typeof(Range[,][]), NameQualifier.None, ExpectedResult = "[Range](https://example.com/system.range)\\[,\\]\\[\\]")]
        [TestCase(typeof(Range[][,]), NameQualifier.None, ExpectedResult = "[Range](https://example.com/system.range)\\[\\]\\[,\\]")]
        [TestCase(typeof(Range*), NameQualifier.None, ExpectedResult = "[Range](https://example.com/system.range)\\*")]
        [TestCase(typeof(Range**), NameQualifier.None, ExpectedResult = "[Range](https://example.com/system.range)\\*\\*")]
        [TestCase(typeof(Range?), NameQualifier.None, ExpectedResult = "[Range](https://example.com/system.range)?")]
        [TestCase(typeof(Range?[]), NameQualifier.None, ExpectedResult = "[Range](https://example.com/system.range)?\\[\\]")]
        [TestCase(typeof(Range*[]), NameQualifier.None, ExpectedResult = "[Range](https://example.com/system.range)\\*\\[\\]")]
        [TestCase(typeof(Range?*[][,]), NameQualifier.None, ExpectedResult = "[Range](https://example.com/system.range)?\\*\\[\\]\\[,\\]")]
        [TestCase(typeof(Range?*[,,][]), NameQualifier.None, ExpectedResult = "[Range](https://example.com/system.range)?\\*\\[,,\\]\\[\\]")]
        [TestCase(typeof(Lazy<Range?*[,,][]>), NameQualifier.None, ExpectedResult = "[Lazy](https://example.com/system.lazy-1)\\<[Range](https://example.com/system.range)?\\*\\[,,\\]\\[\\]>")]
        public string WriteDocLink_Member_WritesExpectedMarkdown(Type type, NameQualifier qualifier)
        {
            using var writer = new MarkdownWriter(new StringWriter());
            using var context = MockHelper.CreateDocumentationContext<MarkdownFormat>();

            writer.WriteDocLink(type.GetMetadata(), context, qualifier);

            return writer.ToString();
        }

        [TestCase("M:System.Range.ToString", NameQualifier.DeclaringType, ExpectedResult = "[Range.ToString()](https://example.com/system.range.tostring)")]
        [TestCase("N:System.Text.Json", NameQualifier.None, ExpectedResult = "[System.Text.Json](https://example.com/system.text.json)")]
        public string WriteDocLink_CodeReference_WritesExpectedMarkdown(string cref, NameQualifier qualifier)
        {
            using var writer = new MarkdownWriter(new StringWriter());
            using var context = MockHelper.CreateDocumentationContext<MarkdownFormat>();

            writer.WriteDocLink(cref, context, _ => qualifier);

            return writer.ToString();
        }

        [Test]
        [Acme.Example(typeof(DateTime), Day = DayOfWeek.Saturday)]
        public void WriteDocLink_Attribute_WritesExpectedMarkdown()
        {
            using var writer = new MarkdownWriter(new StringWriter());
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();

            var attributeData = MethodBase.GetCurrentMethod()!.GetMetadata().CustomAttributes
                .First(a => a.Type.FullName == "Acme.ExampleAttribute");

            writer.WriteDocLink(attributeData, context, NameQualifier.None);

            var expected = "[Example](https://example.com/acme.exampleattribute)(typeof([DateTime](https://example.com/system.datetime)), Day = [DayOfWeek](https://example.com/system.dayofweek).Saturday)";
            Assert.That(writer.ToString(), Is.EqualTo(expected));
        }

        [TestCase("https://example.com/", "Example", ExpectedResult = "[Example](https://example.com/)")]
        [TestCase("https://example.com/", "", ExpectedResult = "[https://example.com/](https://example.com/)")]
        [TestCase("https://example.com/", null, ExpectedResult = "[https://example.com/](https://example.com/)")]
        [TestCase("https://example.com/query?param=[value]", "Click *here*", ExpectedResult = "[Click \\*here\\*](https://example.com/query?param=\\[value\\])")]
        public string WriteLink_WritesExpectedMarkdown(string linkUrl, string? linkText)
        {
            using var writer = new MarkdownWriter(new StringWriter());

            writer.WriteLink(new Uri(linkUrl), !string.IsNullOrEmpty(linkText) ? w => w.Write(linkText) : null);

            return writer.ToString();
        }

        [TestCase("https://example.com/image.png", "Example", ExpectedResult = "![Example](https://example.com/image.png \"Example\")")]
        [TestCase("https://example.com/image.png", "*Example*", ExpectedResult = "![\\*Example\\*](https://example.com/image.png \"\\*Example\\*\")")]
        [TestCase("https://example.com/image.png", "\"Example\"", ExpectedResult = "![\"Example\"](https://example.com/image.png \"\\\"Example\\\"\")")]
        [TestCase("https://example.com/image.png", "", ExpectedResult = "![](https://example.com/image.png)")]
        public string WriteImage_WritesExpectedMarkdown(string imageUrl, string imageTitle)
        {
            using var writer = new MarkdownWriter(new StringWriter());

            writer.WriteImage(new Uri(imageUrl), imageTitle);

            return writer.ToString();
        }

        [TestCase("Hello World!", ExpectedResult = "```\nHello World!\n```\n\n")]
        [TestCase("Hello World!\n", ExpectedResult = "```\nHello World!\n```\n\n")]
        [TestCase("SELECT * FROM table;", "sql", ExpectedResult = "```sql\nSELECT * FROM table;\n```\n\n")]
        public string WriteCodeBlock_WritesExpectedMarkdown(string code, string? language = null)
        {
            using var writer = new MarkdownWriter(new StringWriter() { NewLine = "\n" });

            writer.WriteCodeBlock(code, language);

            return writer.ToString();
        }

        [TestCase("code", ExpectedResult = "`code`")]
        [TestCase("`code`", ExpectedResult = "`\\`code\\``")]
        public string WriteInlineCode_WritesExpectedMarkdown(string text)
        {
            using var writer = new MarkdownWriter(new StringWriter());

            writer.WriteInlineCode(text);

            return writer.ToString();
        }

        [TestCase("bold text", ExpectedResult = "**bold text**")]
        [TestCase("**bold**", ExpectedResult = "**\\*\\*bold\\*\\***")]
        public string WriteStrong_WritesExpectedMarkdown(string text)
        {
            using var writer = new MarkdownWriter(new StringWriter());

            writer.WriteStrong(text);

            return writer.ToString();
        }

        [TestCase("italic text", ExpectedResult = "*italic text*")]
        [TestCase("*italic*", ExpectedResult = "*\\*italic\\**")]
        public string WriteEmphasis_WritesExpectedMarkdown(string text)
        {
            using var writer = new MarkdownWriter(new StringWriter());

            writer.WriteEmphasis(text);

            return writer.ToString();
        }

        [TestCase(1, "Heading 1", ExpectedResult = "# Heading 1\n")]
        [TestCase(2, "Heading 2", ExpectedResult = "## Heading 2\n")]
        [TestCase(6, "Heading 6", ExpectedResult = "###### Heading 6\n")]
        public string WriteHeading_WritesExpectedMarkdown(int level, string text)
        {
            using var writer = new MarkdownWriter(new StringWriter() { NewLine = "\n" });

            writer.WriteHeading(level, text);

            return writer.ToString();
        }

        [TestCase("", "This is a paragraph.", ExpectedResult = "This is a paragraph.\n\n")]
        [TestCase("Before", "This is a paragraph.", ExpectedResult = "Before\n\nThis is a paragraph.\n\n")]
        [TestCase("Before\n", "This is a paragraph.", ExpectedResult = "Before\n\nThis is a paragraph.\n\n")]
        public string WriteParagraph_WritesExpectedMarkdown(string initialText, string paragraphText)
        {
            using var writer = new MarkdownWriter(new StringWriter() { NewLine = "\n" });

            writer.Write(initialText);
            writer.WriteParagraph(paragraphText);

            return writer.ToString();
        }

        [TestCase("Blockquote", ExpectedResult = "> Blockquote\n")]
        public string WriteBlockquote_WritesExpectedMarkdown(string text)
        {
            using var writer = new MarkdownWriter(new StringWriter() { NewLine = "\n" });

            writer.WriteBlockquote(text);

            return writer.ToString();
        }

        [Test]
        public void WriteList_UnorderedList_WritesExpectedMarkdown()
        {
            using var writer = new MarkdownWriter(new StringWriter() { NewLine = "\n" });

            writer.WriteList(["Item 1", "Item 2", "Item 3"], isOrdered: false);

            var expected = "- Item 1\n- Item 2\n- Item 3\n\n";
            Assert.That(writer.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void WriteList_OrderedList_WritesExpectedMarkdown()
        {
            using var writer = new MarkdownWriter(new StringWriter() { NewLine = "\n" });

            writer.WriteList(["Item 1", "Item 2", "Item 3"], isOrdered: true);

            var expected = "1. Item 1\n2. Item 2\n3. Item 3\n\n";
            Assert.That(writer.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void WriteTable_WritesExpectedMarkdown()
        {
            using var writer = new MarkdownWriter(new StringWriter() { NewLine = "\n" });

            writer.WriteTable(["Header 1", "Header 2"],
            [
                ["Row 1 Col 1", "Row 1 Col 2"],
                ["Row 2 Col 1", "Row 2 Col 2"]
            ]);

            var expected =
                "| Header 1 | Header 2 |\n" +
                "| --- | --- |\n" +
                "| Row 1 Col 1 | Row 1 Col 2 |\n" +
                "| Row 2 Col 1 | Row 2 Col 2 |\n\n";

            Assert.That(writer.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void WriteHorizontalRule_WritesExpectedMarkdown()
        {
            using var writer = new MarkdownWriter(new StringWriter() { NewLine = "\n" });

            writer.WriteHorizontalRule();

            Assert.That(writer.ToString(), Is.EqualTo("---\n\n"));
        }

        [TestCase("Some text", ExpectedResult = "Some text\n\n")]
        [TestCase("Some text\n", ExpectedResult = "Some text\n\n")]
        [TestCase("Some text\n\n", ExpectedResult = "Some text\n\n")]
        public string EnsureEmptyLine_WritesEmptyLineIfNotAlreadyPresent(string initialText)
        {
            using var writer = new MarkdownWriter(new StringWriter() { NewLine = "\n" });

            writer.Write(initialText);
            writer.EnsureEmptyLine();

            return writer.ToString();
        }

        [TestCase("Some text", ExpectedResult = "Some text\n")]
        [TestCase("Some text\n", ExpectedResult = "Some text\n")]
        [TestCase("Some text\n\n", ExpectedResult = "Some text\n\n")]
        public string EnsureNewLine_WritesNewLineIfNotAlreadyPresent(string initialText)
        {
            using var writer = new MarkdownWriter(new StringWriter() { NewLine = "\n" });

            writer.Write(initialText);
            writer.EnsureNewLine();

            return writer.ToString();
        }

        [Test]
        public void Dispose_WhenLeaveOpenIsFalse_ClosesUnderlyingWriter()
        {
            var stringWriter = new StringWriter();
            var writer = new MarkdownWriter(stringWriter, leaveOpen: false);

            writer.Dispose();

            Assert.Throws<ObjectDisposedException>(() => stringWriter.Write("Test"));
        }

        [Test]
        public void Dispose_WhenLeaveOpenIsTrue_DoesNotCloseUnderlyingWriter()
        {
            var stringWriter = new StringWriter();
            var writer = new MarkdownWriter(stringWriter, leaveOpen: true);

            writer.Dispose();

            Assert.DoesNotThrow(() => stringWriter.Write("Test"));
        }

        [Test]
        public void IsAtStartOfLine_ReturnsExpectedResult()
        {
            using var writer = new MarkdownWriter(new StringWriter());

            Assert.That(writer.IsAtStartOfLine(), Is.True);

            writer.Write('a');
            Assert.That(writer.IsAtStartOfLine(), Is.False);

            writer.WriteLine();
            Assert.That(writer.IsAtStartOfLine(), Is.True);

            writer.Write(' ');
            using (Assert.EnterMultipleScope())
            {
                Assert.That(writer.IsAtStartOfLine(ignoreLeadingWhitespace: true), Is.True);
                Assert.That(writer.IsAtStartOfLine(ignoreLeadingWhitespace: false), Is.False);
            }

            writer.Write('b');
            Assert.That(writer.IsAtStartOfLine(), Is.False);
        }
    }
}

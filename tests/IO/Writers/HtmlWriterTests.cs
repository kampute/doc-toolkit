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
    public class HtmlWriterTests
    {
        [TestCase("Hello <world>", ExpectedResult = "Hello &lt;world&gt;")]
        [TestCase("1 > 2 & 3 < 4", ExpectedResult = "1 &gt; 2 &amp; 3 &lt; 4")]
        [TestCase("", ExpectedResult = "")]
        public string WriteUnsafe_WritesExpectedContent(string text)
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.Write(text);

            return writer.ToString();
        }

        [TestCase("Hello <world>", ExpectedResult = "Hello <world>")]
        [TestCase("1 > 2 & 3 < 4", ExpectedResult = "1 > 2 & 3 < 4")]
        [TestCase("", ExpectedResult = "")]
        public string WriteSafe_WritesExpectedContent(string text)
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteSafe(text);

            return writer.ToString();
        }

        [TestCase(typeof(Range), NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.range\" rel=\"code-reference\">Range</a>")]
        [TestCase(typeof(Range[]), NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.range\" rel=\"code-reference\">Range</a>[]")]
        [TestCase(typeof(Range[,]), NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.range\" rel=\"code-reference\">Range</a>[,]")]
        [TestCase(typeof(Range[,][]), NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.range\" rel=\"code-reference\">Range</a>[,][]")]
        [TestCase(typeof(Range[][,]), NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.range\" rel=\"code-reference\">Range</a>[][,]")]
        [TestCase(typeof(Range*), NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.range\" rel=\"code-reference\">Range</a>*")]
        [TestCase(typeof(Range**), NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.range\" rel=\"code-reference\">Range</a>**")]
        [TestCase(typeof(Range?), NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.range\" rel=\"code-reference\">Range</a>?")]
        [TestCase(typeof(Range?[]), NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.range\" rel=\"code-reference\">Range</a>?[]")]
        [TestCase(typeof(Range*[]), NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.range\" rel=\"code-reference\">Range</a>*[]")]
        [TestCase(typeof(Range?*[][,]), NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.range\" rel=\"code-reference\">Range</a>?*[][,]")]
        [TestCase(typeof(Range?*[,,][]), NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.range\" rel=\"code-reference\">Range</a>?*[,,][]")]
        [TestCase(typeof(Lazy<Range?*[,,][]>), NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.lazy-1\" rel=\"code-reference\">Lazy</a>&lt;<a href=\"https://example.com/system.range\" rel=\"code-reference\">Range</a>?*[,,][]&gt;")]
        public string WriteDocLink_Member_WritesExpectedHtml(Type type, NameQualifier qualifier)
        {
            using var writer = new HtmlWriter(new StringWriter());
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();

            writer.WriteDocLink(type.GetMetadata(), context, qualifier);

            return writer.ToString();
        }

        [TestCase("M:System.Range.ToString", NameQualifier.DeclaringType, ExpectedResult = "<a href=\"https://example.com/system.range.tostring\" rel=\"code-reference\">Range.ToString()</a>")]
        [TestCase("N:System.Text.Json", NameQualifier.None, ExpectedResult = "<a href=\"https://example.com/system.text.json\" rel=\"code-reference\">System.Text.Json</a>")]
        public string WriteDocLink_CodeReference_WritesExpectedHtml(string cref, NameQualifier qualifier)
        {
            using var writer = new HtmlWriter(new StringWriter());
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();

            writer.WriteDocLink(cref, context, _ => qualifier);

            return writer.ToString();
        }

        [Test]
        [Acme.Sample(typeof(DateTime), Days = [DayOfWeek.Saturday, DayOfWeek.Sunday])]
        public void WriteDocLink_Attribute_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();

            var attributeData = MethodBase.GetCurrentMethod()!.GetMetadata().CustomAttributes
                .First(static a => a.Type.FullName == "Acme.SampleAttribute");

            writer.WriteDocLink(attributeData, context, NameQualifier.None);

            var expected = "<a href=\"https://example.com/acme.sampleattribute\" rel=\"code-reference\">Sample</a>(typeof(<a href=\"https://example.com/system.datetime\" rel=\"code-reference\">DateTime</a>), Days = [<a href=\"https://example.com/system.dayofweek\" rel=\"code-reference\">DayOfWeek</a>.Saturday, <a href=\"https://example.com/system.dayofweek\" rel=\"code-reference\">DayOfWeek</a>.Sunday])";
            Assert.That(writer.ToString(), Is.EqualTo(expected));
        }

        [TestCase("https://example.com/", "Example", ExpectedResult = "<a href=\"https://example.com/\">Example</a>")]
        [TestCase("https://example.com/", null, ExpectedResult = "<a href=\"https://example.com/\">https://example.com/</a>")]
        [TestCase("https://example.com/", "", ExpectedResult = "<a href=\"https://example.com/\">https://example.com/</a>")]
        [TestCase("https://example.com/query?param=<value>", "Click <here>", ExpectedResult = "<a href=\"https://example.com/query?param=&lt;value&gt;\">Click &lt;here&gt;</a>")]
        public string WriteLink_WritesExpectedHtml(string linkUrl, string? linkText)
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteLink(new Uri(linkUrl), !string.IsNullOrEmpty(linkText) ? w => w.Write(linkText) : null);

            return writer.ToString();
        }

        [TestCase("https://example.com/image.png", null, ExpectedResult = "<img src=\"https://example.com/image.png\"/>")]
        [TestCase("https://example.com/image.png", "Example Image", ExpectedResult = "<img src=\"https://example.com/image.png\" title=\"Example Image\" alt=\"Example Image\"/>")]
        public string WriteImage_WritesExpectedHtml(string imageUrl, string? imageTitle)
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteImage(new Uri(imageUrl), imageTitle);

            return writer.ToString();
        }

        [TestCase("Hello World!", ExpectedResult = "<pre dir=\"ltr\"><code>Hello World!</code></pre>")]
        [TestCase("SELECT * FROM table;", "sql", ExpectedResult = "<pre dir=\"ltr\"><code class=\"language-sql\">SELECT * FROM table;</code></pre>")]
        public string WriteCodeBlock_WritesExpectedHtml(string code, string? language = null)
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteCodeBlock(code, language);

            return writer.ToString();
        }

        [Test]
        public void WriteInlineCode_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteInlineCode("code");

            Assert.That(writer.ToString(), Is.EqualTo("<code>code</code>"));
        }

        [Test]
        public void WriteStrong_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteStrong("strong text");

            Assert.That(writer.ToString(), Is.EqualTo("<strong>strong text</strong>"));
        }

        [Test]
        public void WriteEmphasis_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteEmphasis("emphasized text");

            Assert.That(writer.ToString(), Is.EqualTo("<em>emphasized text</em>"));
        }

        [Test]
        public void WriteHeading_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteHeading(1, "Heading 1");
            writer.WriteHeading(6, "Heading 6");

            Assert.That(writer.ToString(), Is.EqualTo("<h1>Heading 1</h1><h6>Heading 6</h6>"));
        }

        [Test]
        public void WriteParagraph_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteParagraph("This is a paragraph.");

            Assert.That(writer.ToString(), Is.EqualTo("<p>This is a paragraph.</p>"));
        }

        [Test]
        public void WriteBlockquote_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteBlockquote("This is a blockquote.");

            Assert.That(writer.ToString(), Is.EqualTo("<blockquote>This is a blockquote.</blockquote>"));
        }

        [Test]
        public void WriteList_UnorderedList_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteList(["Item 1", "Item 2", "Item 3"], isOrdered: false);

            Assert.That(writer.ToString(), Is.EqualTo("<ul><li>Item 1</li><li>Item 2</li><li>Item 3</li></ul>"));
        }

        [Test]
        public void WriteList_OrderedList_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteList(["Item 1", "Item 2", "Item 3"], isOrdered: true);

            Assert.That(writer.ToString(), Is.EqualTo("<ol><li>Item 1</li><li>Item 2</li><li>Item 3</li></ol>"));
        }

        [Test]
        public void WriteTable_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteTable(["Header 1", "Header 2"],
            [
                ["Row 1 Col 1", "Row 1 Col 2"],
                ["Row 2 Col 1", "Row 2 Col 2"]
            ]);

            var expected = "<table><thead><tr><th>Header 1</th><th>Header 2</th></tr></thead><tbody><tr><td>Row 1 Col 1</td><td>Row 1 Col 2</td></tr><tr><td>Row 2 Col 1</td><td>Row 2 Col 2</td></tr></tbody></table>";
            Assert.That(writer.ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void WriteHorizontalRule_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteHorizontalRule();

            Assert.That(writer.ToString(), Is.EqualTo("<hr/>"));
        }

        [Test]
        public void WriteComment_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteComment("This is a comment");

            Assert.That(writer.ToString(), Is.EqualTo("<!-- This is a comment -->"));
        }

        [Test]
        public void WriteInlineElement_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteInlineElement("img",
                HtmlWriter.Attribute("class", "example"),
                HtmlWriter.Attribute("src", "https://example.com/image.png"));

            Assert.That(writer.ToString(), Is.EqualTo("<img class=\"example\" src=\"https://example.com/image.png\"/>"));
        }

        [Test]
        public void WriteStartElement_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteStartElement("div");

            Assert.That(writer.ToString(), Is.EqualTo("<div>"));
        }

        [Test]
        public void WriteStartElement_WithAttributes_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteStartElement("div",
                HtmlWriter.Attribute("class", "example"),
                HtmlWriter.Attribute("id", "test"));

            Assert.That(writer.ToString(), Is.EqualTo("<div class=\"example\" id=\"test\">"));
        }

        [Test]
        public void WriteEndElement_WritesExpectedHtml()
        {
            using var writer = new HtmlWriter(new StringWriter());

            writer.WriteStartElement("div");
            writer.WriteEndElement();

            Assert.That(writer.ToString(), Is.EqualTo("<div></div>"));
        }

        [Test]
        public void Dispose_ClosesAllOpenTags()
        {
            var stringWriter = new StringWriter();
            using (var writer = new HtmlWriter(stringWriter, leaveOpen: true))
            {
                writer.WriteStartElement("div");
                writer.WriteStartElement("span");
            }

            Assert.That(stringWriter.ToString(), Is.EqualTo("<div><span></span></div>"));
        }

        [Test]
        public void Dispose_WhenLeaveOpenIsFalse_ClosesUnderlyingWriter()
        {
            var stringWriter = new StringWriter();
            var writer = new HtmlWriter(stringWriter, leaveOpen: false);

            writer.Dispose();

            Assert.That(() => stringWriter.Write("Test"), Throws.InstanceOf<ObjectDisposedException>());
        }

        [Test]
        public void Dispose_WhenLeaveOpenIsTrue_DoesNotCloseUnderlyingWriter()
        {
            var stringWriter = new StringWriter();
            var writer = new HtmlWriter(stringWriter, leaveOpen: true);

            writer.Dispose();

            Assert.That(() => stringWriter.Write("Test"), Throws.Nothing);
        }
    }
}

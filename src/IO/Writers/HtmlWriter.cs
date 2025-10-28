// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.IO.Writers
{
    using Kampute.DocToolkit.IO.Minifiers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a text writer that escapes HTML special characters and provides methods to write HTML elements.
    /// </summary>
    /// <remarks>
    /// This class helps in writing Markdown content safely by escaping special characters where necessary. It extends the
    /// <see cref="MarkupWriter"/> class and provides additional methods tailored for HTML formatting. It ensures well-formed
    /// HTML output through several key features:
    /// <list type="bullet">
    ///   <item>
    ///     <term>Safety</term>
    ///     <description>
    ///       Automatic HTML character escaping converts special characters to their corresponding HTML entities, preventing XSS
    ///       vulnerabilities and ensuring valid HTML output.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Basic API</term>
    ///     <description>
    ///       Methods for creating both block-level and inline HTML elements, with specialized support for common elements like links,
    ///       code blocks, and comments.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Structural Validation</term>
    ///     <description>
    ///       Internal stack tracking of open elements ensures proper nesting and prevents common HTML structural errors.
    ///       The current number of open tags can be retrieved via the <see cref="OpenTagCount"/> property.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Resource Management</term>
    ///     <description>
    ///       Automatic cleanup on disposal ensures all open tags are properly closed, preventing incomplete HTML structures even in
    ///       exceptional cases.
    ///     </description>
    ///   </item>
    /// </list>
    /// <note type="tip" title="Tip">
    /// To minify the generated HTML by removing unnecessary whitespace and comments, consider using an instance of <see cref="HtmlMinifier"/>
    /// as the underlying writer.
    /// </note>
    /// </remarks>
    /// <example>
    /// The following example demonstrates the key features of <see cref="HtmlWriter"/>:
    /// <code>
    /// using var writer = new HtmlDocumentWriter(Console.Out, leaveOpen: true);
    ///
    /// // Write a heading
    /// writer.WriteHeading(1, "Title");
    /// writer.WtiteLine();
    ///
    /// // Write a paragraph
    /// writer.WriteParagraph(w =>
    /// {
    ///     w.Write("This is a paragraph with ");
    ///     w.WriteStrong("bold");
    ///     w.Write(" text.");
    /// });
    /// writer.WtiteLine();
    ///
    /// // Add a blockquote
    /// writer.WriteBlockquote("This is a blockquote.");
    /// writer.WtiteLine();
    ///
    /// // Add a code block
    /// writer.WriteCodeBlock(@"var greeting = ""Hello, World!"";", "csharp");
    /// writer.WtiteLine();
    ///
    /// // Add a link
    /// writer.WriteLink(new Uri("https://example.com"), "Visit Example");
    /// </code>
    /// This produces the following HTML:
    /// <code language="html">
    /// &lt;h1&gt;Title&lt;/h1&gt;
    /// &lt;p&gt;This is a paragraph with &lt;strong&gt;bold&lt;/strong&gt; text.&lt;/p&gt;
    /// &lt;blockquote&gt;This is a blockquote.&lt;/blockquote&gt;
    /// &lt;pre dir=&quot;ltr&quot;&gt;&lt;code class=&quot;language-csharp&quot;&gt;var greeting = &amp;quot;Hello, World!&amp;quot;;&lt;/code&gt;&lt;/pre&gt;
    /// &lt;a href=&quot;https://example.com/&quot;&gt;Visit Example&lt;/a&gt;
    /// </code>
    /// </example>
    /// <threadsafety static="true" instance="false"/>
    /// <seealso cref="HtmlMinifier"/>
    public class HtmlWriter : MarkupWriter
    {
        private readonly Stack<string> openTags = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlWriter"/> class.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the HTML content to.</param>
        /// <param name="leaveOpen">A value indicating whether the underlying writer should be left open when the <see cref="HtmlWriter"/> is disposed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// If <paramref name="leaveOpen"/> is <see langword="true"/>, the underlying <see cref="TextWriter"/> remains open after
        /// the <see cref="HtmlWriter"/> is disposed. This is useful when managing the writer externally.
        /// <note type="tip" title="Tip">
        /// To minify the generated HTML by removing unnecessary whitespace and comments, consider using an instance of <see cref="HtmlMinifier"/>
        /// as the <paramref name="writer"/>.
        /// </note>
        /// </remarks>
        public HtmlWriter(TextWriter writer, bool leaveOpen = false)
            : base(writer, leaveOpen)
        {
        }

        /// <summary>
        /// Gets the number of block elements that are still open.
        /// </summary>
        /// <value>
        /// The number of block elements that are still open.
        /// </value>
        public int OpenTagCount => openTags.Count;

        /// <summary>
        /// Writes a character after escaping HTML special characters.
        /// </summary>
        /// <param name="value">The character to write.</param>
        /// <inheritdoc/>
        public override void Write(char value)
        {
            switch (value)
            {
                case '<':
                    WriteSafe("&lt;");
                    break;
                case '>':
                    WriteSafe("&gt;");
                    break;
                case '&':
                    WriteSafe("&amp;");
                    break;
                case '"':
                    WriteSafe("&quot;");
                    break;
                default:
                    WriteSafe(value);
                    break;
            }
        }

        /// <summary>
        /// Writes a hyperlink to the documentation of a code element with the specified URL and delegate for writing the link text.
        /// </summary>
        /// <param name="docUrl">The URI of the documentation page to link to.</param>
        /// <param name="docLinkTextHandler">The action to write the text of the link, typically the name of the code element.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="docUrl"/> or <paramref name="docLinkTextHandler"/> is <see langword="null"/>.</exception>
        protected override void WriteDocLink(Uri docUrl, Action<MarkupWriter> docLinkTextHandler)
        {
            if (docUrl is null)
                throw new ArgumentNullException(nameof(docUrl));
            if (docLinkTextHandler is null)
                throw new ArgumentNullException(nameof(docLinkTextHandler));

            WriteStartElement("a", [Attribute("href", docUrl.ToString()), Attribute("rel", "code-reference")]);
            docLinkTextHandler(this);
            WriteEndElement();
        }

        /// <summary>
        /// Writes a hyperlink.
        /// </summary>
        /// <param name="url">The <see cref="Uri"/> representing the link's destination.</param>
        /// <param name="linkTextHandler">The action to write the text of the link, or <see langword="null"/> to use the URL as the text.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> is <see langword="null"/>.</exception>
        public override void WriteLink(Uri url, Action<MarkupWriter>? linkTextHandler = null)
        {
            if (url is null)
                throw new ArgumentNullException(nameof(url));

            linkTextHandler ??= writer => writer.Write(url.ToString());

            WriteStartElement("a", Attribute("href", url.ToString()));
            linkTextHandler(this);
            WriteEndElement();
        }

        /// <summary>
        /// Writes an image using the specified URL and an optional title.
        /// </summary>
        /// <param name="imageUrl">The <see cref="Uri"/> representing the image's URL.</param>
        /// <param name="title">The title of the image, or <see langword="null"/> to omit the title.</param>
        public override void WriteImage(Uri imageUrl, string? title = null)
        {
            if (imageUrl is null)
                throw new ArgumentNullException(nameof(imageUrl));

            WriteInlineElement("img", string.IsNullOrEmpty(title)
                ? [Attribute("src", imageUrl.ToString())]
                : [Attribute("src", imageUrl.ToString()), Attribute("title", title), Attribute("alt", title)]);
        }

        /// <summary>
        /// Writes a code block.
        /// </summary>
        /// <param name="code">The code to write.</param>
        /// <param name="language">The language of the code block, or <see langword="null"/> to omit the language specifier.</param>
        public override void WriteCodeBlock(string code, string? language = null)
        {
            WriteStartElement("pre", Attribute("dir", "ltr"));
            if (!string.IsNullOrEmpty(language))
                WriteStartElement("code", Attribute("class", $"language-{language}"));
            else
                WriteStartElement("code");
            Write(code ?? string.Empty);
            WriteEndElement();
            WriteEndElement();
        }

        /// <summary>
        /// Writes a code span using the specified text.
        /// </summary>
        /// <param name="text">The text of the code span.</param>
        public override void WriteInlineCode(string text)
        {
            WriteStartElement("code");
            Write(text ?? string.Empty);
            WriteEndElement();
        }

        /// <summary>
        /// Writes a strong emphasis using the provided delegate to write the content.
        /// </summary>
        /// <param name="contentHandler">The action to write the content of the strong emphasis.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contentHandler"/> is <see langword="null"/>.</exception>
        public override void WriteStrong(Action<MarkupWriter> contentHandler)
        {
            if (contentHandler is null)
                throw new ArgumentNullException(nameof(contentHandler));

            WriteStartElement("strong");
            contentHandler(this);
            WriteEndElement();
        }

        /// <summary>
        /// Writes an emphasis using the provided delegate to write the content.
        /// </summary>
        /// <param name="contentHandler">The action to write the content of the emphasis.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contentHandler"/> is <see langword="null"/>.</exception>
        public override void WriteEmphasis(Action<MarkupWriter> contentHandler)
        {
            if (contentHandler is null)
                throw new ArgumentNullException(nameof(contentHandler));

            WriteStartElement("em");
            contentHandler(this);
            WriteEndElement();
        }

        /// <summary>
        /// Writes a heading using the provided delegate to write the content.
        /// </summary>
        /// <param name="level">The heading level (1-6).</param>
        /// <param name="contentHandler">The action to write the content of the heading.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contentHandler"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="level"/> is less than 1 or greater than 6.</exception>
        public override void WriteHeading(int level, Action<MarkupWriter> contentHandler)
        {
            if (contentHandler is null)
                throw new ArgumentNullException(nameof(contentHandler));
            if (level is < 1 or > 6)
                throw new ArgumentOutOfRangeException(nameof(level), level, "Heading level must be between 1 and 6.");

            WriteStartElement($"h{level}");
            contentHandler(this);
            WriteEndElement();
        }

        /// <summary>
        /// Writes a paragraph using the provided delegate to write the content.
        /// </summary>
        /// <param name="contentHandler">The action to write the content of the paragraph.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contentHandler"/> is <see langword="null"/>.</exception>
        public override void WriteParagraph(Action<MarkupWriter> contentHandler)
        {
            if (contentHandler is null)
                throw new ArgumentNullException(nameof(contentHandler));

            WriteStartElement("p");
            contentHandler(this);
            WriteEndElement();
        }

        /// <summary>
        /// Writes a blockquote using the provided delegate to write the content.
        /// </summary>
        /// <param name="contentHandler">The action to write the content of the blockquote.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contentHandler"/> is <see langword="null"/>.</exception>
        public override void WriteBlockquote(Action<MarkupWriter> contentHandler)
        {
            if (contentHandler is null)
                throw new ArgumentNullException(nameof(contentHandler));

            WriteStartElement("blockquote");
            contentHandler(this);
            WriteEndElement();
        }

        /// <summary>
        /// Writes a list using the provided delegate to write the list items.
        /// </summary>
        /// <param name="count">The number of items in the list.</param>
        /// <param name="itemContentHandler">The action to write the content of each list item.</param>
        /// <param name="isOrdered">Indicates whether the list is ordered (numbered) or unordered (bulleted).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="itemContentHandler"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is negative.</exception>
        public override void WriteList(int count, Action<MarkupWriter, int> itemContentHandler, bool isOrdered = false)
        {
            if (itemContentHandler is null)
                throw new ArgumentNullException(nameof(itemContentHandler));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count cannot be negative.");

            WriteStartElement(isOrdered ? "ol" : "ul");

            for (var i = 0; i < count; ++i)
            {
                WriteStartElement("li");
                itemContentHandler(this, i);
                WriteEndElement();
            }

            WriteEndElement();
        }

        /// <summary>
        /// Writes a table using the provided delegate to write the content of each cell.
        /// </summary>
        /// <param name="columns">The column headers of the table.</param>
        /// <param name="rowCount">The number of rows in the table.</param>
        /// <param name="cellContentHandler">The action to write the content of each cell.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="columns"/> or <paramref name="cellContentHandler"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="rowCount"/> is negative.</exception>
        public override void WriteTable(IReadOnlyList<string> columns, int rowCount, Action<MarkupWriter, int, int> cellContentHandler)
        {
            if (columns is null)
                throw new ArgumentNullException(nameof(columns));
            if (cellContentHandler is null)
                throw new ArgumentNullException(nameof(cellContentHandler));
            if (rowCount < 0)
                throw new ArgumentOutOfRangeException(nameof(rowCount), rowCount, "Row count cannot be negative.");

            WriteStartElement("table");

            WriteStartElement("thead");
            WriteStartElement("tr");
            for (var col = 0; col < columns.Count; ++col)
            {
                WriteStartElement("th");
                Write(columns[col] ?? string.Empty);
                WriteEndElement();
            }
            WriteEndElement();
            WriteEndElement();

            WriteStartElement("tbody");
            for (var row = 0; row < rowCount; row++)
            {
                WriteStartElement("tr");
                for (var col = 0; col < columns.Count; col++)
                {
                    WriteStartElement("td");
                    cellContentHandler(this, row, col);
                    WriteEndElement();
                }
                WriteEndElement();
            }
            WriteEndElement();

            WriteEndElement();
        }

        /// <summary>
        /// Writes a horizontal rule.
        /// </summary>
        public override void WriteHorizontalRule() => WriteInlineElement("hr");

        /// <summary>
        /// Writes an HTML comment.
        /// </summary>
        /// <param name="comment">The comment to write.</param>
        public virtual void WriteComment(string comment)
        {
            WriteSafe("<!-- ");
            Write(comment ?? string.Empty);
            WriteSafe(" -->");
        }

        /// <summary>
        /// Writes an inline HTML element with the specified name and attributes.
        /// </summary>
        /// <param name="tagName">The name of the tag to write.</param>
        /// <param name="attributes">The attributes to write on the tag.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tagName"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attributes"/> is <see langword="null"/>.</exception>
        public virtual void WriteInlineElement(string tagName, params IEnumerable<KeyValuePair<string, string?>> attributes)
        {
            if (string.IsNullOrEmpty(tagName))
                throw new ArgumentException($"'{nameof(tagName)}' cannot be null or empty.", nameof(tagName));
            if (attributes is null)
                throw new ArgumentNullException(nameof(attributes));

            WriteSafe('<');
            WriteSafe(tagName);
            foreach (var (name, value) in attributes)
            {
                WriteSafe(' ');
                WriteAttribute(name, value);
            }
            WriteSafe('/');
            WriteSafe('>');
        }

        /// <summary>
        /// Writes the opening tag of an HTML element with the specified name and attributes.
        /// </summary>
        /// <param name="tagName">The name of the tag to write.</param>
        /// <param name="attributes">The attributes to write on the tag.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tagName"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attributes"/> is <see langword="null"/>.</exception>
        public virtual void WriteStartElement(string tagName, params IEnumerable<KeyValuePair<string, string?>> attributes)
        {
            if (string.IsNullOrEmpty(tagName))
                throw new ArgumentException($"'{nameof(tagName)}' cannot be null or empty.", nameof(tagName));
            if (attributes is null)
                throw new ArgumentNullException(nameof(attributes));

            WriteSafe('<');
            WriteSafe(tagName);
            foreach (var (name, value) in attributes)
            {
                WriteSafe(' ');
                WriteAttribute(name, value);
            }
            WriteSafe('>');

            openTags.Push(tagName);
        }

        /// <summary>
        /// Writes the closing tag of the last open HTML element.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when there are no open tags to close.</exception>
        public virtual void WriteEndElement()
        {
            if (!openTags.TryPop(out var tagName))
                throw new InvalidOperationException("No open tags to close.");

            WriteSafe('<');
            WriteSafe('/');
            WriteSafe(tagName);
            WriteSafe('>');
        }

        /// <summary>
        /// Writes a tag attribute with an optional value.
        /// </summary>
        /// <param name="name">The name of the attribute to write.</param>
        /// <param name="value">The value of the attribute to write, or <see langword="null"/> to write the attribute without a value.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is <see langword="null"/> or empty.</exception>
        protected virtual void WriteAttribute(string name, string? value = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

            WriteSafe(name);
            if (value is not null)
            {
                WriteSafe('=');
                WriteSafe('"');
                Write(value);
                WriteSafe('"');
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="HtmlWriter"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                while (OpenTagCount > 0)
                    WriteEndElement();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Returns an attribute with the specified name and value.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="value">The value of the attribute, or <see langword="null"/> if the attribute is without a value.</param>
        /// <returns>An attribute with the specified name and value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static KeyValuePair<string, string?> Attribute(string name, string? value = null) => KeyValuePair.Create(name, value);
    }
}

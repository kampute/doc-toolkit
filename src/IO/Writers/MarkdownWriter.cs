// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.IO.Writers
{
    using Kampute.DocToolkit.IO.Minifiers;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a text writer that escapes Markdown special characters and provides methods to write Markdown elements.
    /// </summary>
    /// <remarks>
    /// This class helps in writing Markdown content safely by escaping special characters where necessary. It extends the
    /// <see cref="MarkupWriter"/> class and provides additional methods tailored for Markdown formatting.
    /// <note type="tip" title="Tip">
    /// To ignore unnecessary whitespace in the generated Markdown and improve readability and visual appeal of Markdown in
    /// plain text, consider using an instance of <see cref="MarkdownMinifier"/> as the underlying writer.
    /// </note>
    /// </remarks>
    /// <example>
    /// The following example demonstrates the key features of <see cref="MarkdownWriter"/>:
    /// <code>
    /// using var writer = new MarkdownDocumentWriter(Console.Out, leaveOpen: true);
    ///
    /// // Write a heading
    /// writer.WriteHeading(1, "Title");
    ///
    /// // Write a paragraph
    /// writer.WriteParagraph(w =>
    /// {
    ///     w.Write("This is a paragraph with ");
    ///     w.WriteStrong("bold");
    ///     w.Write(" text.");
    /// });
    ///
    /// // Add a blockquote
    /// writer.WriteBlockquote("This is a blockquote.");
    ///
    /// // Add a code block
    /// writer.WriteCodeBlock(@"var greeting = ""Hello, World!"";", "csharp");
    ///
    /// // Add a link
    /// writer.WriteLink(new Uri("https://example.com"), "Visit Example");
    /// </code>
    /// This produces the following Markdown:
    /// <code language="md">
    /// # Title
    ///
    /// This is a paragraph with **bold text**
    ///
    /// &gt; This is a blockquote.
    ///
    /// ```csharp
    /// var greeting = "Hello, World!";
    /// ```
    ///
    /// [Visit Example](https://example.com/)
    /// </code>
    /// </example>
    /// <threadsafety static="true" instance="false"/>
    /// <seealso cref="MarkdownMinifier"/>
    public class MarkdownWriter : MarkupWriter
    {
        private int emptyLineCount = 2;
        private int whitespaceCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownWriter"/> class.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the Markdown content to.</param>
        /// <param name="leaveOpen">
        /// A value indicating whether the underlying writer should be left open when the <see cref="MarkdownWriter"/> is disposed.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// If <paramref name="leaveOpen"/> is <see langword="true"/>, the underlying <see cref="TextWriter"/> remains open after
        /// the <see cref="MarkdownWriter"/> is disposed. This is useful when managing the writer externally.
        /// <note type="tip" title="Tip">
        /// To improve readability and visual appeal of Markdown, consider using an instance of <see cref="MarkdownMinifier"/> as the <paramref name="writer"/>.
        /// </note>
        /// </remarks>
        public MarkdownWriter(TextWriter writer, bool leaveOpen = false)
            : base(writer, leaveOpen)
        {
        }

        /// <summary>
        /// Gets the current indentation level.
        /// </summary>
        /// <value>
        /// The current indentation level, which is used to determine the number of leading spaces or tabs in the output.
        /// </value>
        public int IndentationLevel { get; protected set; }

        /// <summary>
        /// Determines whether the writer is at the start of a line.
        /// </summary>
        /// <param name="ignoreLeadingWhitespace">Indicates whether to ignore leading whitespace when checking.</param>
        /// <returns><see langword="true"/> if the writer is at the start of a line; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAtStartOfLine(bool ignoreLeadingWhitespace = true) => emptyLineCount > 0 && (ignoreLeadingWhitespace || whitespaceCount == 0);

        /// <summary>
        /// Writes a character without escaping Markdown special characters.
        /// </summary>
        /// <param name="value">The character to write.</param>
        /// <inheritdoc/>
        public override void WriteSafe(char value)
        {
            base.WriteSafe(value);

            if (value == '\n')
            {
                emptyLineCount++;
                whitespaceCount = 0;
            }
            else if (!char.IsWhiteSpace(value))
            {
                emptyLineCount = 0;
                whitespaceCount = 0;
            }
            else
            {
                whitespaceCount++;
            }
        }

        /// <summary>
        /// Writes a character after escaping Markdown special characters.
        /// </summary>
        /// <param name="value">The character to write.</param>
        /// <inheritdoc/>
        public override void Write(char value)
        {
            if (Markdown.NeedsEncoding(value, excludeLineMarkers: emptyLineCount == 0))
                WriteSafe('\\');

            WriteSafe(value);
        }

        /// <summary>
        /// Writes a hyperlink.
        /// </summary>
        /// <param name="url">The <see cref="Uri"/> representing the link's destination.</param>
        /// <param name="linkTextHandler">
        /// The action to write the text of the link, or <see langword="null"/> to use the URL as the text.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> is <see langword="null"/>.</exception>
        public override void WriteLink(Uri url, Action<MarkupWriter>? linkTextHandler = null)
        {
            if (url is null)
                throw new ArgumentNullException(nameof(url));

            var href = url.ToString();

            WriteSafe('[');
            if (linkTextHandler is not null)
                linkTextHandler(this);
            else
                Write(href);
            WriteSafe(']');
            WriteSafe('(');
            Write(href);
            WriteSafe(')');
        }

        /// <summary>
        /// Writes an image using the specified URL and an optional title.
        /// </summary>
        /// <param name="imageUrl">The <see cref="Uri"/> representing the image's URL.</param>
        /// <param name="title">The title of the image, or <see langword="null"/> to omit the title.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="imageUrl"/> is <see langword="null"/>.</exception>
        public override void WriteImage(Uri imageUrl, string? title = null)
        {
            if (imageUrl is null)
                throw new ArgumentNullException(nameof(imageUrl));

            WriteSafe('!');
            WriteSafe('[');
            if (!string.IsNullOrEmpty(title))
                Write(title);
            WriteSafe(']');
            WriteSafe('(');
            Write(imageUrl.ToString());
            if (!string.IsNullOrEmpty(title))
            {
                WriteSafe(' ');
                WriteSafe('"');
                if (title.Contains('"'))
                    WriteQuotedText(title);
                else
                    Write(title);
                WriteSafe('"');
            }
            WriteSafe(')');

            void WriteQuotedText(string text)
            {
                var needsQuote = false;
                foreach (var part in text.Split('"'))
                {
                    if (needsQuote)
                    {
                        WriteSafe('\\');
                        WriteSafe('"');
                    }
                    Write(part);
                    needsQuote = true;
                }
            }
        }

        /// <summary>
        /// Writes a block of code with optional language specifier.
        /// </summary>
        /// <param name="code">The code to write.</param>
        /// <param name="language">The language of the code block, or <see langword="null"/> to omit the language specifier.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="code"/> is <see langword="null"/>.</exception>
        public override void WriteCodeBlock(string code, string? language = null)
        {
            if (code is null)
                throw new ArgumentNullException(nameof(code));

            var fence = new string('`', Markdown.GetMinimumFenceBackticks(code));

            EnsureEmptyLine();
            WriteSafe(fence);
            if (!string.IsNullOrEmpty(language))
                WriteSafe(language);
            WriteLine();

            WriteSafe(code);

            if (emptyLineCount == 0)
                WriteLine();

            WriteSafe(fence);
            EnsureEmptyLine();
        }

        /// <summary>
        /// Writes a code span using the specified text.
        /// </summary>
        /// <param name="text">The text of the code span.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
        public override void WriteInlineCode(string text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            WriteSafe('`');
            Write(text);
            WriteSafe('`');
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

            WriteSafe('*');
            WriteSafe('*');
            contentHandler(this);
            WriteSafe('*');
            WriteSafe('*');
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

            WriteSafe('*');
            contentHandler(this);
            WriteSafe('*');
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

            EnsureEmptyLine();

            for (var i = 0; i < level; ++i)
                WriteSafe('#');

            WriteSafe(' ');
            contentHandler(this);
            EnsureNewLine();
        }

        /// <summary>
        /// Writes a paragraph using the provided delegate to write the content.
        /// </summary>
        /// <param name="contentHandler">The action that writes the paragraph content.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contentHandler"/> is <see langword="null"/>.</exception>
        public override void WriteParagraph(Action<MarkupWriter> contentHandler)
        {
            if (contentHandler is null)
                throw new ArgumentNullException(nameof(contentHandler));

            EnsureEmptyLine();
            contentHandler(this);
            EnsureEmptyLine();
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

            EnsureEmptyLine();

            var indetLevel = IndentationLevel++;
            for (var i = 0; i < indetLevel; ++i)
                WriteSafe('\t');

            WriteSafe('>');
            WriteSafe(' ');
            contentHandler(this);

            IndentationLevel = indetLevel;
            EnsureNewLine();
        }

        /// <summary>
        /// Writes a list using a delegate to write the list items.
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

            if (count == 0)
                return;

            EnsureEmptyLine();

            var indetLevel = IndentationLevel++;
            for (var i = 0; i < count; ++i)
            {
                for (var j = 0; j < indetLevel; ++j)
                    WriteSafe('\t');

                if (isOrdered)
                {
                    WriteSafe((i + 1).ToString(CultureInfo.InvariantCulture));
                    WriteSafe('.');
                }
                else
                {
                    WriteSafe('-');
                }

                WriteSafe(' ');
                itemContentHandler(this, i);
                EnsureNewLine();
            }
            IndentationLevel = indetLevel;

            EnsureEmptyLine();
        }

        /// <summary>
        /// Writes a table using a delegate to write the content of each cell.
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

            if (columns.Count == 0)
                return;

            EnsureEmptyLine();

            WriteSafe('|');
            for (var i = 0; i < columns.Count; ++i)
            {
                WriteSafe(' ');
                Write(columns[i] ?? string.Empty);
                WriteSafe(' ');
                WriteSafe('|');
            }
            WriteLine();

            WriteSafe('|');
            for (var i = 0; i < columns.Count; ++i)
                WriteSafe(" --- |");
            WriteLine();

            for (var row = 0; row < rowCount; ++row)
            {
                WriteSafe('|');
                for (var col = 0; col < columns.Count; ++col)
                {
                    WriteSafe(' ');
                    cellContentHandler(this, row, col);
                    WriteSafe(' ');
                    WriteSafe('|');
                }
                WriteLine();
            }

            EnsureEmptyLine();
        }

        /// <summary>
        /// Writes a horizontal rule.
        /// </summary>
        public override void WriteHorizontalRule()
        {
            EnsureEmptyLine();
            WriteSafe("---");
            EnsureEmptyLine();
        }

        /// <summary>
        /// Writes an empty line if the write does not already end with one.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureEmptyLine()
        {
            while (emptyLineCount < 2)
                WriteLine();
        }

        /// <summary>
        /// Writes a newline if the writer does not already end with one.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureNewLine()
        {
            if (emptyLineCount == 0)
                WriteLine();
        }
    }
}

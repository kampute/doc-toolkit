// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Formatters
{
    using Kampute.DocToolkit.IO.Minifiers;
    using Kampute.DocToolkit.IO.Writers;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.XmlDoc;
    using System;
    using System.IO;

    /// <summary>
    /// Represents a documentation formatter that generates Markdown content.
    /// </summary>
    /// <remarks>
    /// The <see cref="MarkdownFormat"/> class provides functionality for generating documentation in Markdown format.
    /// It handles the transformation of XML documentation comments to Markdown, proper encoding of Markdown special
    /// characters, and supports various file extensions commonly associated with Markdown files.
    /// <para>
    /// Use this formatter when you need to generate documentation for platforms that support Markdown, such as
    /// GitHub, GitLab, Azure DevOps Wiki, or other Markdown-compatible documentation systems.
    /// </para>
    /// </remarks>
    /// <seealso cref="HtmlFormat"/>
    public class MarkdownFormat : DocFormatter, IXmlDocReferenceAccessor
    {
        private readonly XmlDocToMarkdownTransformer xmlDocTransformer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownFormat"/> class.
        /// </summary>
        public MarkdownFormat()
            : this(FileExtensions.Markdown)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownFormat"/> class with the specified file extension for the Markdown files.
        /// </summary>
        /// <param name="fileExtension">The file extension of the documentation files for the HTML format.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileExtension"/> is <see langword="null"/>.</exception>
        public MarkdownFormat(string fileExtension)
            : base(fileExtension)
        {
            xmlDocTransformer = new XmlDocToMarkdownTransformer();
            TextTransformers.Register<MarkdownLinkTransformer>([fileExtension, .. FileExtensions.MarkdownExtensions]);
        }

        /// <summary>
        /// Gets the XML documentation transformer for the Markdown format.
        /// </summary>
        /// <value>
        /// The XML documentation transformer for the Markdown format.
        /// </value>
        protected override IXmlDocTransformer XmlDocTransformer => xmlDocTransformer;

        /// <summary>
        /// Encodes special Markdown characters in the specified text and writes the encoded content to the specified writer.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the encoded content to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <see langword="null"/>.</exception>
        public override void Encode(ReadOnlySpan<char> text, TextWriter writer) => Markdown.Encode(text, writer);

        /// <summary>
        /// Creates an instance of <see cref="MarkupWriter"/> class that wraps the specified writer for encoding content in Markdown format.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the encoded content to.</param>
        /// <param name="disposeWriter">
        /// <see langword="true"/> if the wrapped <paramref name="writer"/> should be disposed when the returned <see cref="MarkupWriter"/> is disposed;
        /// otherwise, <see langword="false"/>. The default is <see langword="false"/>.
        /// </param>
        /// <returns>A new instance of a <see cref="MarkdownWriter"/> object that wraps the specified writer.</returns>
        /// <remarks>
        /// This implementation creates a <see cref="MarkdownWriter"/> instance that provides Markdown-specific formatting capabilities,
        /// such as proper syntax for headings, lists, code blocks, and other Markdown-specific elements.
        /// <para>
        /// The writer automatically handles Markdown requirements:
        /// <list type="bullet">
        ///   <item>Escaping special Markdown characters (*, _, #, etc.) in regular text</item>
        ///   <item>Generating well-formed Markdown syntax for semantic document structures</item>
        /// </list>
        /// </para>
        /// <note type="caution" title="Important">
        /// It is the caller's responsibility to dispose the returned <see cref="MarkupWriter"/> instance to release any resources.
        /// </note>
        /// </remarks>
        public override MarkupWriter CreateMarkupWriter(TextWriter writer, bool disposeWriter = false) => new MarkdownWriter(writer, leaveOpen: !disposeWriter);

        /// <summary>
        /// Creates a <see cref="TextWriter"/> instance that wraps the specified writer to minimize the content for Markdown format.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the minimized content to.</param>
        /// <returns>A <see cref="MarkdownMinifier"/> instance that wraps the specified writer for minimizing Markdown content.</returns>
        /// <remarks>
        /// This implementation creates a <see cref="MarkdownMinifier"/> that performs Markdown-specific optimizations such as:
        /// <list type="bullet">
        ///   <item>Removing redundant blank lines while preserving essential spacing for Markdown syntax</item>
        ///   <item>Preserving whitespace in code blocks (both indented and fenced)</item>
        ///   <item>Normalizing line breaks and indentation for cleaner output</item>
        ///   <item>Maintaining proper Markdown structure while reducing file size</item>
        ///   <item>Ensuring consistent indentation and spacing for lists, tables, and other Markdown elements</item>
        ///   <item>Providing consistent empty lines between sections and paragraphs</item>
        /// </list>
        /// <para>
        /// Unlike other minifiers that primarily focus on size reduction, the Markdown minifier balances readability with optimization,
        /// since Markdown files are often viewed in their raw text form. This approach ensures the minimized content is visually appealing
        /// and easy to read while still achieving a smaller file size.
        /// </para>
        /// <para>
        /// It is the caller's responsibility to dispose the returned <see cref="TextWriter"/> instance to release any resources.
        /// </para>
        /// <note type="caution" title="Important">
        /// After calling this method, the caller should not write to, close, or dispose the original writer directly to ensure that the minimized
        /// content is written correctly.
        /// </note>
        /// <note type="caution" title="Important">
        /// It is the caller's responsibility to dispose the returned <see cref="MarkupWriter"/> instance to release any resources.
        /// </note>
        /// </remarks>
        public override TextWriter CreateMinifier(TextWriter writer) => new MarkdownMinifier(writer, leaveOpen: false);

        /// <inheritdoc />
        IXmlDocReferenceResolver? IXmlDocReferenceAccessor.ReferenceResolver
        {
            get => xmlDocTransformer.ReferenceResolver;
            set => xmlDocTransformer.ReferenceResolver = value;
        }
    }
}

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
    using System.Net;

    /// <summary>
    /// Represents a documentation formatter that generates HTML content.
    /// </summary>
    /// <remarks>
    /// The <see cref="HtmlFormat"/> class provides functionality for generating documentation in HTML format.
    /// It handles the transformation of XML documentation comments to HTML, proper encoding of HTML special
    /// characters, and supports various file extensions commonly associated with HTML files.
    /// <para>
    /// Use this formatter when you need to generate web-based documentation that can be viewed in browsers,
    /// including API reference websites and integrated help systems.
    /// </para>
    /// </remarks>
    /// <seealso cref="MarkdownFormat"/>
    public class HtmlFormat : DocFormatter, IXmlDocReferenceAccessor
    {
        private readonly XmlDocToHtmlTransformer xmlDocTransformer;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlFormat"/> class.
        /// </summary>
        public HtmlFormat()
            : this(FileExtensions.Html)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlFormat"/> class with the specified file extension for the HTML files.
        /// </summary>
        /// <param name="fileExtension">The file extension of the documentation files for the HTML format.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileExtension"/> is <see langword="null"/>.</exception>
        public HtmlFormat(string fileExtension)
            : base(fileExtension)
        {
            xmlDocTransformer = new XmlDocToHtmlTransformer();
            TextTransformers.Register<HtmlLinkTransformer>([fileExtension, .. FileExtensions.HtmlExtensions]);
        }

        /// <summary>
        /// Gets the XML documentation transformer for the HTML format.
        /// </summary>
        /// <value>
        /// The XML documentation transformer for the HTML format.
        /// </value>
        protected override IXmlDocTransformer XmlDocTransformer => xmlDocTransformer;

        /// <summary>
        /// Encodes special HTML characters in the specified text and writes the encoded content to the specified writer.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the encoded content to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <see langword="null"/>.</exception>
        public override void Encode(ReadOnlySpan<char> text, TextWriter writer) => WebUtility.HtmlEncode(text.ToString(), writer);

        /// <summary>
        /// Creates an instance of <see cref="MarkupWriter"/> class that wraps the specified writer for encoding content in HTML format.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the encoded content to.</param>
        /// <param name="disposeWriter">
        /// <see langword="true"/> if the wrapped <paramref name="writer"/> should be disposed when the returned <see cref="MarkupWriter"/> is disposed;
        /// otherwise, <see langword="false"/>. The default is <see langword="false"/>.
        /// </param>
        /// <returns>A new instance of a <see cref="HtmlWriter"/> object that wraps the specified writer.</returns>
        /// <remarks>
        /// This implementation creates an <see cref="HtmlWriter"/> instance that provides HTML-specific formatting capabilities,
        /// such as proper HTML tag generation, entity encoding, and structural elements like headings, lists, and tables with appropriate
        /// HTML markup.
        /// <para>
        /// The writer automatically handles HTML-specific requirements:
        /// <list type="bullet">
        ///   <item>Converting special characters to HTML entities (&amp;, &lt;, &gt;, etc.)</item>
        ///   <item>Generating well-formed HTML tags for semantic document structures</item>
        ///   <item>Supporting HTML-specific features like CSS classes and element attributes</item>
        /// </list>
        /// </para>
        /// <note type="caution" title="Important">
        /// It is the caller's responsibility to dispose the returned <see cref="MarkupWriter"/> instance to release any resources.
        /// </note>
        /// </remarks>
        public override MarkupWriter CreateMarkupWriter(TextWriter writer, bool disposeWriter = false) => new HtmlWriter(writer, leaveOpen: !disposeWriter);

        /// <summary>
        /// Creates a <see cref="TextWriter"/> instance that wraps the specified writer to minimize the content for HTML format.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the minimized content to.</param>
        /// <returns>An <see cref="HtmlMinifier"/> instance that wraps the specified writer for minimizing HTML content.</returns>
        /// <remarks>
        /// This implementation creates an <see cref="HtmlMinifier"/> that performs HTML-specific optimizations such as:
        /// <list type="bullet">
        ///   <item>Removing unnecessary whitespace between HTML tags</item>
        ///   <item>Preserving whitespace in <c>&lt;pre&gt;</c> elements and other sensitive contexts</item>
        ///   <item>Normalizing line breaks and indentation</item>
        ///   <item>Maintaining valid HTML structure while reducing file size</item>
        /// </list>
        /// The minification process reduces the size of HTML output without affecting its rendering or structure, making it ideal for
        /// production environments where bandwidth optimization is important.
        /// <note type="caution" title="Important">
        /// After calling this method, the caller should not write to, close, or dispose the original writer directly to ensure that the minimized
        /// content is written correctly.
        /// </note>
        /// <note type="caution" title="Important">
        /// It is the caller's responsibility to dispose the returned <see cref="MarkupWriter"/> instance to release any resources.
        /// </note>
        /// </remarks>
        public override TextWriter CreateMinifier(TextWriter writer) => new HtmlMinifier(writer, leaveOpen: false);

        /// <inheritdoc />
        IXmlDocReferenceResolver? IXmlDocReferenceAccessor.ReferenceResolver
        {
            get => xmlDocTransformer.ReferenceResolver;
            set => xmlDocTransformer.ReferenceResolver = value;
        }
    }
}

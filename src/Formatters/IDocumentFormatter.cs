// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Formatters
{
    using Kampute.DocToolkit.IO.Writers;
    using Kampute.DocToolkit.XmlDoc;
    using System.IO;

    /// <summary>
    /// Defines a contract for a documentation formatter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IDocumentFormatter"/> interface combines text encoding and XML documentation
    /// transformation capabilities to provide comprehensive documentation formatting.
    /// </para>
    /// Documentation formatters are responsible for:
    /// <list type="bullet">
    ///   <item>Converting XML documentation comments to a target format (HTML, Markdown, etc.)</item>
    ///   <item>Properly encoding text for the target format</item>
    ///   <item>Creating specialized writers for dynamic document generation</item>
    ///   <item>Supporting content minification for optimized output</item>
    /// </list>
    /// Implementations typically support specific documentation formats, such as HTML or Markdown, and
    /// can be registered with the <see cref="DocFormatProvider"/> to be discovered by file extension.
    /// </remarks>
    /// <seealso cref="DocFormatProvider"/>
    /// <seealso cref="TextTransformerRegistry"/>
    public interface IDocumentFormatter : ITextEncoder, IXmlDocTransformer
    {
        /// <summary>
        /// Gets the file extension of the documentation files in the target format.
        /// </summary>
        /// <value>
        /// The file extension (including the leading period) of the documentation files in the target format.
        /// </value>
        string FileExtension { get; }

        /// <summary>
        /// Gets the transformers for converting text to the target format.
        /// </summary>
        /// <value>
        /// The <see cref="TextTransformerRegistry"/> that contains the transformers for converting text to the target format.
        /// </value>
        TextTransformerRegistry TextTransformers { get; }

        /// <summary>
        /// Creates an instance of <see cref="MarkupWriter"/> class that wraps the specified writer for encoding content in the target format.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the encoded content to.</param>
        /// <param name="disposeWriter">
        /// <see langword="true"/> if the wrapped <paramref name="writer"/> should be disposed when the returned <see cref="MarkupWriter"/> is disposed;
        /// otherwise, <see langword="false"/>. The default is <see langword="false"/>.
        /// </param>
        /// <returns>A new instance of a <see cref="MarkupWriter"/> object that wraps the specified writer.</returns>
        /// <remarks>
        /// The returned <see cref="MarkupWriter"/> instance provides a rich API for writing structured content with proper encoding for the target
        /// format. It handles format-specific escaping, semantic document elements (headings, lists, tables, etc.), and cross-references.
        /// <note type="caution" title="Important">
        /// It is the caller's responsibility to dispose the returned <see cref="MarkupWriter"/> instance to release any resources.
        /// </note>
        /// </remarks>
        /// <example>
        /// Here is an example of how to use the <see cref="CreateMarkupWriter"/> method:
        /// <code>
        /// using var markupWriter = formatter.CreateMarkupWriter(textWriter);
        /// markupWriter.WriteHeading("Title", 1);
        /// markupWriter.WriteParagraph("This is properly encoded content for the target format.");
        /// </code>
        /// </example>
        MarkupWriter CreateMarkupWriter(TextWriter writer, bool disposeWriter = false);

        /// <summary>
        /// Creates a <see cref="TextWriter"/> instance that wraps the specified writer to minimize the content for the target format.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the minimized content to.</param>
        /// <returns>
        /// A new instance of <see cref="TextWriter"/> that wraps the specified writer for minimizing the content, or the original writer
        /// if the target format does not support minimization.
        /// </returns>
        /// <remarks>
        /// This factory method creates a specialized writer that optimizes the output by removing unnecessary whitespace and formatting
        /// while preserving the semantic meaning of the content. The optimization is format-specific and may include:
        /// <list type="bullet">
        ///   <item>Removing redundant spaces and line breaks</item>
        ///   <item>Compressing consecutive whitespace characters</item>
        ///   <item>Preserving required whitespace in code blocks and other sensitive areas</item>
        /// </list>
        /// <para>
        /// If the target format supports minimization, this method returns a new <see cref="TextWriter"/> that wraps the specified writer.
        /// The wrapper instance takes ownership of the specified writer and will dispose it when the wrapper is disposed. If the target format
        /// does not support minimization, the original writer is returned.
        /// </para>
        /// <note type="caution" title="Important">
        /// After calling this method, the caller should not write to, close, or dispose the original writer directly to ensure that the minimized
        /// content is written correctly.
        /// </note>
        /// <note type="caution" title="Important">
        /// It is the caller's responsibility to dispose the returned <see cref="MarkupWriter"/> instance to release any resources.
        /// </note>
        /// </remarks>
        /// <example>
        /// Here is an example of how to use the <see cref="CreateMinifier"/> method:
        /// <code>
        /// using var minifiedWriter = formatter.CreateMinifier(textWriter);
        /// using var markupWriter = formatter.CreateMarkupWriter(minifiedWriter);
        /// markupWriter.WriteHeading("Title", 1);
        /// markupWriter.WriteParagraph("This is properly encoded content for the target format.");
        /// </code>
        /// </example>
        TextWriter CreateMinifier(TextWriter writer);
    }
}


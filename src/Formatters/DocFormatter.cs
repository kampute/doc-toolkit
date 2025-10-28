// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Formatters
{
    using Kampute.DocToolkit.IO.Writers;
    using Kampute.DocToolkit.XmlDoc;
    using System;
    using System.IO;
    using System.Xml.Linq;

    /// <summary>
    /// Represents a base class for a documentation formatter.
    /// </summary>
    /// <remarks>
    /// The <see cref="DocFormatter"/> class serves as a foundation for creating documentation formatters
    /// that generate content in specific formats (such as HTML or Markdown). It provides common functionality
    /// for transforming XML documentation comments, encoding text, and managing file format support.
    /// <para>
    /// Derived classes must implement specific formatting logic for their target format by overriding abstract
    /// methods like <see cref="Encode"/> and <see cref="CreateMarkupWriter"/>.
    /// </para>
    /// </remarks>
    public abstract class DocFormatter : IDocumentFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocFormatter"/> class.
        /// </summary>
        /// <param name="fileExtension">The file extension of the documentation files in the target format.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileExtension"/> is <see langword="null"/> or empty.</exception>
        protected DocFormatter(string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
                throw new ArgumentException($"{nameof(fileExtension)} cannot be null or empty.", nameof(fileExtension));

            TextTransformers = new TextTransformerRegistry(fileExtension);
        }

        /// <summary>
        /// Gets the XML documentation transformer for the target format.
        /// </summary>
        /// <value>
        /// The XML documentation transformer for the target format.
        /// </value>
        protected abstract IXmlDocTransformer XmlDocTransformer { get; }

        /// <summary>
        /// Gets the text transformer registry for the target format.
        /// </summary>
        /// <value>
        /// The <see cref="TextTransformerRegistry"/> that contains the transformers for converting text to the target format.
        /// </value>
        public TextTransformerRegistry TextTransformers { get; }

        /// <summary>
        /// Gets the file extension of the documentation files in the target format.
        /// </summary>
        /// <value>
        /// The file extension (including the leading period) of the documentation files in the target format.
        /// </value>
        public string FileExtension => TextTransformers.TargetFileExtension;

        /// <summary>
        /// Writes the transformed content of the XML comment element to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the transformed content to.</param>
        /// <param name="comment">The XML comment element to process.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="comment"/> is <see langword="null"/>.</exception>
        public virtual void Transform(TextWriter writer, XElement comment)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (comment is null)
                throw new ArgumentNullException(nameof(comment));

            XmlDocTransformer.Transform(writer, comment);
        }

        /// <summary>
        /// Encodes the specified text for the target format and writes it to the specified writer.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the encoded content to.</param>
        public abstract void Encode(ReadOnlySpan<char> text, TextWriter writer);

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
        /// A derived class must override this method to create a new instance of <see cref="MarkupWriter"/> that wraps the specified writer for encoding
        /// content in the target format.
        /// <note type="caution" title="Important">
        /// It is the caller's responsibility to dispose the returned <see cref="MarkupWriter"/> instance to release any resources.
        /// </note>
        /// </remarks>
        public abstract MarkupWriter CreateMarkupWriter(TextWriter writer, bool disposeWriter = false);

        /// <summary>
        /// Creates a <see cref="TextWriter"/> instance that wraps the specified writer to minimize the content for target format.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the minimized content to.</param>
        /// <returns>A <see cref="TextWriter"/> instance that wraps the specified writer for minimizing the content.</returns>
        /// <remarks>
        /// The default implementation returns the original writer without any wrapping. A derived class can override this method to wrap the original
        /// writer with a new instance that minimizes the content for the target format.
        /// <para>
        /// After calling this method, the caller should not write to, close, or dispose the original writer directly to ensure that the minimized content
        /// is written correctly.
        /// </para>
        /// <note type="caution" title="Important">
        /// It is the caller's responsibility to dispose the returned <see cref="TextWriter"/> instance to release any resources.
        /// </note>
        /// </remarks>
        public virtual TextWriter CreateMinifier(TextWriter writer) => writer;
    }
}

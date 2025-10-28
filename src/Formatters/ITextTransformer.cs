// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Formatters
{
    using Kampute.DocToolkit.Routing;
    using System.IO;

    /// <summary>
    /// Defines a contract for transforming text from one format to another.
    /// </summary>
    /// <remarks>
    /// The <see cref="ITextTransformer"/> interface provides a standardized way to convert text content
    /// from one format to another, such as converting Markdown to HTML.
    /// <para>
    /// Implementations of this interface handle the specific transformation logic for different source and
    /// target formats. They are typically used by <see cref="IDocumentFormatter"/> implementations to transform
    /// content between formats during documentation generation.
    /// </para>
    /// </remarks>
    /// <seealso cref="IDocumentFormatter"/>
    /// <seealso cref="TextTransformerRegistry"/>
    public interface ITextTransformer
    {
        /// <summary>
        /// Transforms the text from the source format to the target format.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read the source text from.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the transformed text to.</param>
        /// <param name="urlTransformer">An optional <see cref="IUrlTransformer"/> for transforming URLs in the text.</param>
        void Transform(TextReader reader, TextWriter writer, IUrlTransformer? urlTransformer = null);
    }
}

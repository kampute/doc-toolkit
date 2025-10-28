// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Formatters
{
    using System;
    using System.IO;

    /// <summary>
    /// Defines a contract for text encoders of specific formats.
    /// </summary>
    /// <remarks>
    /// The <see cref="ITextEncoder"/> interface defines a standard method for encoding text content
    /// to ensure it's properly formatted for a specific output format (such as HTML or Markdown).
    /// <para>
    /// Implementations handle format-specific encoding concerns, like escaping special characters
    /// that have syntactic meaning in the target format. For example, HTML encoders escape characters
    /// like '&lt;' and '&gt;', while Markdown encoders escape characters like '*' and '#'.
    /// </para>
    /// This interface is typically used in conjunction with documentation formatters to ensure that
    /// text content is safely embedded in generated documentation without breaking the format's syntax.
    /// </remarks>
    public interface ITextEncoder
    {
        /// <summary>
        /// Encodes the specified text for the target format and writes it to the specified writer.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the encoded content to.</param>
        void Encode(ReadOnlySpan<char> text, TextWriter writer);
    }
}

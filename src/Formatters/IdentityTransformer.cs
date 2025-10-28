// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Formatters
{
    using Kampute.DocToolkit.Routing;
    using System;
    using System.IO;

    /// <summary>
    /// Represents a format transformer that does not perform any transformation.
    /// </summary>
    /// <remarks>
    /// The <see cref="IdentityTransformer"/> class is a simple pass-through transformer that copies content
    /// without modifying it. This is useful as a default transformer when no specific transformation is needed
    /// between file formats, or when the input format is already compatible with the target format.
    /// <para>
    /// The class is designed as a singleton, so use the <see cref="Instance"/> property to access the shared
    /// instance rather than creating new instances.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public sealed class IdentityTransformer : ITextTransformer
    {
        private IdentityTransformer() { }

        /// <summary>
        /// Gets the singleton instance of the <see cref="IdentityTransformer"/> class.
        /// </summary>
        /// <value>
        /// The singleton instance of the <see cref="IdentityTransformer"/> class.
        /// </value>
        public static IdentityTransformer Instance { get; } = new();

        /// <summary>
        /// Copies the content from the source <see cref="TextReader"/> to the target <see cref="TextWriter"/> without any transformation.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read the source text from.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the transformed text to.</param>
        /// <param name="urlTransformer">The optional <see cref="IUrlTransformer"/> for transforming URLs in the text. This parameter is ignored in this implementation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader"/> or <paramref name="writer"/> is <see langword="null"/>.</exception>
        public void Transform(TextReader reader, TextWriter writer, IUrlTransformer? urlTransformer = null)
        {
            if (reader is null)
                throw new ArgumentNullException(nameof(reader));
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            string? line;
            var isFirstLine = true;
            while ((line = reader.ReadLine()) is not null)
            {
                if (!isFirstLine)
                    writer.WriteLine();
                else
                    isFirstLine = false;
                writer.Write(line);
            }
        }
    }
}

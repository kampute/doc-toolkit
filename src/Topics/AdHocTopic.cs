// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Topics
{
    using Kampute.DocToolkit.IO.Writers;
    using Kampute.DocToolkit.Topics.Abstracts;
    using System;

    /// <summary>
    /// Represents a documentation topic that generates its content using a delegate.
    /// </summary>
    /// <remarks>
    /// The <see cref="AdHocTopic"/> class allows for dynamic generation of documentation content by invoking a provided delegate.
    /// This is useful for scenarios where the content needs to be computed or generated on-the-fly based on the current state of
    /// the documentation context.
    /// </remarks>
    public sealed class AdHocTopic : DynamicTopic
    {
        private readonly Action<MarkupWriter, IDocumentationContext> contentWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdHocTopic"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the topic among its siblings, typically used for URLs or filenames.</param>
        /// <param name="contentWriter">The delegate to write the content of the topic.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is <see langword="null"/>, whitespace, or contains invalid characters.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="contentWriter"/> is <see langword="null"/>.</exception>
        public AdHocTopic(string id, Action<MarkupWriter, IDocumentationContext> contentWriter)
            : base(id)
        {
            this.contentWriter = contentWriter ?? throw new ArgumentNullException(nameof(contentWriter));
        }

        /// <inheritdoc/>
        protected override void GenerateContent(MarkupWriter writer, IDocumentationContext context) => contentWriter(writer, context);
    }
}

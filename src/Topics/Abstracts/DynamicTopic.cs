// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Topics.Abstracts
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.IO.Writers;
    using System;
    using System.IO;

    /// <summary>
    /// Provides the base functionality for a topic source that generates content dynamically in the format
    /// required by the documentation system.
    /// </summary>
    /// <remarks>
    /// This class supports generating documentation content dynamically in the output format specified by the
    /// documentation context. It is useful for content that needs to be computed rather than stored as static
    /// files.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public abstract class DynamicTopic : TopicSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTopic"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the topic among its siblings, typically used for URLs or filenames.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is <see langword="null"/>, whitespace, or contains invalid characters.</exception>
        protected DynamicTopic(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Generates the documentation content using the specified <see cref="MarkupWriter"/> and <see cref="IDocumentationContext"/>.
        /// </summary>
        /// <param name="writer">The <see cref="MarkupWriter"/> used to write the documentation content.</param>
        /// <param name="context">The documentation context, which provides additional information for rendering the topic.</param>
        protected abstract void GenerateContent(MarkupWriter writer, IDocumentationContext context);

        /// <summary>
        /// Renders the documentation content to the specified <see cref="TextWriter"/> using the provided documentation context.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the rendered content will be written.</param>
        /// <param name="context">The documentation context, which provides additional information for rendering the topic.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
        public override void Render(TextWriter writer, IDocumentationContext context)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            using var markupWriter = context.ContentFormatter.CreateMarkupWriter(writer);
            GenerateContent(markupWriter, context);
        }
    }
}

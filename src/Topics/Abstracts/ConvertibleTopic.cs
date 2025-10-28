// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Topics.Abstracts
{
    using Kampute.DocToolkit;
    using System;
    using System.IO;

    /// <summary>
    /// Provides the base functionality for a topic source that requires content conversion.
    /// </summary>
    /// <remarks>
    /// This class handles the transformation of content from its source format to the output format required
    /// by the documentation system.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public abstract class ConvertibleTopic : TopicSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertibleTopic"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the topic among its siblings, typically used for URLs or filenames.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is <see langword="null"/>, whitespace, or contains invalid characters.</exception>
        protected ConvertibleTopic(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Gets the format of the source content.
        /// </summary>
        /// <value>
        /// The file extension (including the leading period) representing the content format.
        /// </value>
        /// <remarks>
        /// This property indicates the original format of the content (e.g., ".md" for Markdown or ".html" for HTML)
        /// to determine the appropriate conversion strategy. Its value must be a valid file extension, including the
        /// leading period.
        /// </remarks>
        protected abstract string ContentFormat { get; }

        /// <summary>
        /// Creates a <see cref="TextReader"/> that provides the rendered content of the documentation based on
        /// the specified context.
        /// </summary>
        /// <param name="context">The context containing the information and settings required to render the documentation.</param>
        /// <returns>A <see cref="TextReader"/> containing the rendered documentation content.</returns>
        /// <remarks>
        /// The <see cref="CreateContentReader"/> method is responsible for generating the content of a topic in
        /// the format specified by the <see cref="ContentFormat"/> property.
        /// <para>
        /// This method is typically called during the documentation generation process to retrieve the content
        /// for each topic. The returned <see cref="TextReader"/> should provide the fully rendered content with
        /// all cross-references, formatting, and other transformations applied according to the context's settings.
        /// </para>
        /// Implementations should ensure that the returned <see cref="TextReader"/> is properly initialized and ready
        /// for reading. The caller is responsible for disposing the <see cref="TextReader"/> when finished with it.
        /// </remarks>
        protected abstract TextReader CreateContentReader(IDocumentationContext context);

        /// <summary>
        /// Renders the documentation content to the specified <see cref="TextWriter"/> using the provided documentation context.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the rendered content will be written.</param>
        /// <param name="context">The documentation context, which provides additional information for rendering the topic.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown when no text transformer is found for the specified content format.</exception>
        public override void Render(TextWriter writer, IDocumentationContext context)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (!context.ContentFormatter.TextTransformers.TryGet(ContentFormat, out var transformer))
                throw new NotSupportedException($"No text transformer found for the file extension '{ContentFormat}'.");

            using var reader = CreateContentReader(context);
            transformer.Transform(reader, writer, context.UrlTransformer);
        }
    }
}

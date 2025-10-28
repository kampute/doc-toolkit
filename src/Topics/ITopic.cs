// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Topics
{
    using Kampute.DocToolkit;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Defines a contract for a documentation topic.
    /// </summary>
    /// <remarks>
    /// The <see cref="ITopic"/> interface represents a discrete content piece such as a conceptual guide, tutorial,
    /// or overview page that complements the automatically generated API reference.
    /// <para>
    /// Topics allow content in a format different from the target documentation format to be represented in the
    /// final documentation. For example, Markdown files can be rendered as HTML through appropriate transformation.
    /// </para>
    /// Implementations work with <see cref="IDocumentationContext"/> to access formatting, linking, and other documentation
    /// services during rendering.
    /// </remarks>
    public interface ITopic
    {
        /// <summary>
        /// Gets the unique identifier of the topic among its siblings.
        /// </summary>
        /// <value>
        /// The unique identifier of the topic among its siblings, typically used for URLs or filenames.
        /// </value>
        /// <remarks>
        /// The <see cref="Id"/> is used to identify a topic among its siblings (topics with the same parent). This
        /// identifier is often used as the name of the topic's file or the last segment of its URL path. Therefore,
        /// it should be unique within the context of its parent topic and should not contain any special characters
        /// that would interfere with URL or file naming conventions.
        /// <note type="caution" title="Caution">
        /// The identifier of topics are case-insensitive, meaning that two topics with identifiers differing only in
        /// case will be treated as the same topic within the same parent.
        /// </note>
        /// </remarks>
        string Id { get; }

        /// <summary>
        /// Gets the title of the topic.
        /// </summary>
        /// <value>
        /// The title of the topic, which is a human-readable string that describes the topic.
        /// </value>
        /// <remarks>
        /// The <see cref="Title"/> is a human-readable string that describes the topic. It is typically used as the
        /// heading or title in the rendered documentation. However, based on the selected addressing strategy, it may
        /// also be used as the URL or filename for the topic. Therefore, it is recommended to keep the title concise
        /// and descriptive, avoiding special characters that could interfere with URL or file naming conventions.
        /// </remarks>
        string Title { get; }

        /// <summary>
        /// Gets the parent topic of the topic.
        /// </summary>
        /// <value>
        /// The parent topic of the topic, or <see langword="null"/> if the topic is not subordinate to any other topic.
        /// </value>
        /// <remarks>
        /// The <see cref="ParentTopic"/> property allows for hierarchical organization of topics. It can be used to
        /// create nested structures or to represent relationships between topics. If the topic is a top-level topic,
        /// this property will be <see langword="null"/>.
        /// </remarks>
        ITopic? ParentTopic { get; }

        /// <summary>
        /// Gets the subtopics of the topic.
        /// </summary>
        /// <value>
        /// A read-only collection of topics that are the subtopics of the topic.
        /// </value>
        /// <remarks>
        /// The <see cref="Subtopics"/> property provides access to the subtopics of the topic. It can be used during
        /// rendering to include links or references to related topics.
        /// </remarks>
        IReadOnlyCollection<ITopic> Subtopics { get; }

        /// <summary>
        /// Renders the content of the topic to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the content will be rendered.</param>
        /// <param name="context">The documentation context, which provides additional information for rendering the topic.</param>
        /// <remarks>
        /// The <see cref="Render"/> method is responsible for writing the content of the topic to the provided
        /// <see cref="TextWriter"/>. This method should take into account the formatting and other requirements
        /// specified by the documentation context.
        /// </remarks>
        void Render(TextWriter writer, IDocumentationContext context);
    }
}

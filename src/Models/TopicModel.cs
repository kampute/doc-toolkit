// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Models
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Represents a documentation model for a topic within a documentation context.
    /// </summary>
    /// <remarks>
    /// The <see cref="TopicModel"/> class serves as a bridge between a topic source and the documentation system. It wraps
    /// an <see cref="ITopic"/> with a specific <see cref="IDocumentationContext"/>, allowing the topic to be rendered with
    /// contextual information such as formatting preferences and addressing strategies.
    /// <para>
    /// Topics are designed to support cross-referencing between different documentation pages. This system ensures that links
    /// between topics remain valid regardless of how the documentation is generated or rendered, even when the underlying
    /// structure changes.
    /// </para>
    /// Topics also enable content transformation, converting source content (like Markdown files) into the target documentation
    /// format (such as HTML) during the rendering process.
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class TopicModel : IDocumentModel, IEquatable<TopicModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TopicModel"/> class with the specified context and top-level source topic.
        /// </summary>
        /// <param name="context">The documentation context associated with the topic.</param>
        /// <param name="source">The source of the topic which must be a top-level topic without a parent.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="source"/> has a parent topic, indicating it is not a top-level topic.</exception>
        public TopicModel(IDocumentationContext context, ITopic source)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Source = source ?? throw new ArgumentNullException(nameof(source));

            if (source.ParentTopic is not null)
                throw new ArgumentException("Source topic must be a top-level topic without a parent.", nameof(source));

            Id = source.Id;
            Subtopics = [.. source.Subtopics.Select(CreateSubtopic)];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicModel"/> class with the specified context, source, and parent topic.
        /// </summary>
        /// <param name="context">The documentation context associated with the topic.</param>
        /// <param name="source">The source of the topic.</param>
        /// <param name="parentTopic">The parent topic of the topic.</param>
        private TopicModel(IDocumentationContext context, ITopic source, TopicModel parentTopic)
        {
            Context = context;
            Source = source;
            ParentTopic = parentTopic;

            Id = parentTopic.Id + '/' + source.Id;
            Subtopics = [.. source.Subtopics.Select(CreateSubtopic)];
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is always <see cref="DocumentationModelType.Topic"/> for this model.
        /// </value>
        public DocumentationModelType ModelType => DocumentationModelType.Topic;

        /// <summary>
        /// Gets the documentation context associated with the topic.
        /// </summary>
        /// <value>
        /// The documentation context associated with the topic, which provides additional information for rendering the topic.
        /// </value>
        public IDocumentationContext Context { get; }

        /// <summary>
        /// Gets the source of the topic.
        /// </summary>
        /// <value>
        /// The source of the topic, which provides the content and rendering logic.
        /// </value>
        public ITopic Source { get; }

        /// <summary>
        /// Gets the parent topic of the current topic.
        /// </summary>
        /// <value>
        /// The main topic of the current topic, or <see langword="null"/> if the topic is a top-level topic.
        /// </value>
        public TopicModel? ParentTopic { get; }

        /// <summary>
        /// Gets the subtopics of the current topic.
        /// </summary>
        /// <value>
        /// A read-only collection of subtopics that the current topic is parent of.
        /// </value>
        public IReadOnlyCollection<TopicModel> Subtopics { get; }

        /// <summary>
        /// Gets the qualified identifier of the topic.
        /// </summary>
        /// <value>
        /// The qualified identifier of the topic, which identifies the topic uniquely within the documentation context.
        /// </value>
        /// <remarks>
        /// The identifier is used for cross-referencing topics within the documentation. It is a combination of the parent
        /// topic's identifier and the source topic's identifier, ensuring uniqueness across the documentation context.
        /// </remarks>
        public string Id { get; }

        /// <summary>
        /// Gets the title of the topic.
        /// </summary>
        /// <value>
        /// The title of the topic, which is typically used as the main heading in the rendered documentation.
        /// </value>
        public string Name => Source.Title;

        /// <summary>
        /// Gets the URL of the topic.
        /// </summary>
        /// <value>
        /// The URL of the topic.
        /// </value>
        /// <inheritdoc cref="IDocumentModel.Url"/>
        public Uri Url => Context.AddressProvider.TryGetTopicUrl(Source, out var url) ? url : UriHelper.EmptyUri;

        /// <summary>
        /// Gets the hierarchy of parent topics that lead to this topic.
        /// </summary>
        /// <value>
        /// An enumerable collection of <see cref="IDocumentModel"/> objects representing the parent hierarchy.
        /// </value>
        public IEnumerable<IDocumentModel> HierarchyPath => ParentTopic is not null ? ParentTopic.HierarchyPath.Append(ParentTopic) : [];

        /// <summary>
        /// Attempts to get the relative file path of the documentation file for the topic.
        /// </summary>
        /// <param name="relativePath">When this method returns, contains the relative file path of the documentation file if applicable; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a documentation file is applicable; otherwise, <see langword="false"/>.</returns>
        public bool TryGetDocumentationFile([NotNullWhen(true)] out string? relativePath) => Context.AddressProvider.TryGetTopicFile(Source, out relativePath);

        /// <summary>
        /// Creates a subtopic from the specified source topic.
        /// </summary>
        /// <param name="source">The source topic from which to create the subtopic.</param>
        /// <returns>A new <see cref="TopicModel"/> instance representing the subtopic.</returns>
        protected virtual TopicModel CreateSubtopic(ITopic source) => new(Context, source, this);

        /// <summary>
        /// Renders the content of the topic to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the content will be rendered.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown when the <see cref="Source"/> cannot be rendered in the format specified by the <see cref="Context"/>.</exception>
        public void Render(TextWriter writer) => Source.Render(writer, Context);

        /// <summary>
        /// Determines whether the specified <see cref="TopicModel"/> is equal to the current <see cref="TopicModel"/> instance.
        /// </summary>
        /// <param name="other">The <see cref="TopicModel"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the specified <see cref="TopicModel"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public bool Equals(TopicModel? other) => other is not null && Source.Equals(other.Source) && Context.Equals(other.Context);

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the specified object is a <see cref="TopicModel"/> and is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj) => obj is TopicModel topic && Equals(topic);

        /// <summary>
        /// Returns a hash code for the current <see cref="TopicModel"/> instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="TopicModel"/> instance.</returns>
        public override int GetHashCode() => HashCode.Combine(Source, Context);

        /// <summary>
        /// Returns a string that represents the current <see cref="TopicModel"/> instance.
        /// </summary>
        /// <returns>
        /// A string that represents the current <see cref="TopicModel"/> instance.
        /// </returns>
        public override string ToString() => Name;
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Topics.Abstracts
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Provides the base functionality for a topic source.
    /// </summary>
    /// <remarks>
    /// The <see cref="TopicSource"/> class implements the <see cref="ITopic"/> interface and provides a
    /// base for creating different types of topic sources, such as file-backed or dynamic topics.
    /// <para>
    /// The class allows for the management of topic name, title, and parent-child relationships between
    /// topics. The subclass can override the <see cref="Render"/> method to provide custom rendering logic
    /// for the topic's content.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public abstract class TopicSource : IMutableTopic
    {
        private string? title;
        private IMutableTopic? parent;
        private readonly List<IMutableTopic> children = [];
        private bool isSynchronizingSubtopic;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicSource"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the topic among its siblings, typically used for URLs or filenames.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is <see langword="null"/>, whitespace, or contains invalid characters.</exception>
        protected TopicSource(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
            if (Path.GetInvalidPathChars().Any(id.Contains))
                throw new ArgumentException($"'{nameof(id)}' contains invalid characters.", nameof(id));

            Id = id;
        }

        /// <summary>
        /// Gets the unique identifier of the topic among its siblings.
        /// </summary>
        /// <value>
        /// The unique identifier of the topic among its siblings, typically used for URLs or filenames.
        /// </value>
        /// <inheritdoc/>
        public string Id { get; }

        /// <summary>
        /// Gets or sets the title of the topic.
        /// </summary>
        /// <value>
        /// The title of the topic, which is a human-readable string that describes the topic.
        /// <note type="tip" title="Tip">
        /// Setting the title to an empty string will cause the title to be derived from the topic's name or content.
        /// </note>
        /// </value>
        /// <inheritdoc/>
        public string Title
        {
            get => title ??= GenerateTitle();
            set => title = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        /// <summary>
        /// Gets or sets the parent topic of the topic.
        /// </summary>
        /// <value>
        /// The parent topic of the topic, or <see langword="null"/> if the topic is not subordinate to any other topic.
        /// </value>
        /// <inheritdoc/>
        public IMutableTopic? ParentTopic
        {
            get => parent;
            set
            {
                if (ReferenceEquals(parent, value))
                    return;

                if (parent is not null)
                {
                    parent.RemoveSubtopic(this);
                    parent = null;
                }
                if (value is not null)
                {
                    value.AddSubtopic(this);
                    parent = value;
                }
            }
        }

        /// <summary>
        /// Gets the collection of subtopics of the topic.
        /// </summary>
        /// <value>
        /// A read-only collection of topics that are the subtopics of the topic.
        /// </value>
        /// <inheritdoc/>
        public IReadOnlyCollection<ITopic> Subtopics => children;

        /// <summary>
        /// Adds a subtopic to the current topic.
        /// </summary>
        /// <param name="subtopic">The subtopic to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="subtopic"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when a subtopic with the same identifier already exists in the current topic.</exception>
        /// <exception cref="InvalidOperationException">Thrown when adding the subtopic would create a circular reference.</exception>
        /// <remarks>
        /// This methods adds a subtopic to the current topic if it is not already a child of the topic, its identifier is not already
        /// used by another subtopic, and it does not create a circular reference in the topic hierarchy.
        /// <para>
        /// If the subtopic is already a child of another topic, it will be moved from that topic to the current topic.
        /// </para>
        /// </remarks>
        public void AddSubtopic(IMutableTopic subtopic)
        {
            if (subtopic is null)
                throw new ArgumentNullException(nameof(subtopic));

            if (isSynchronizingSubtopic || ReferenceEquals(subtopic.ParentTopic, this))
                return;

            if (children.Any(child => child.Id.Equals(subtopic.Id, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"A subtopic with the identifier '{subtopic.Id}' already exists in the topic '{Id}'.", nameof(subtopic));

            if (WouldCreateCircularReference(subtopic))
                throw new InvalidOperationException($"Adding the subtopic '{subtopic.Id}' to the topic '{Id}' would create a circular reference in the topic hierarchy.");

            isSynchronizingSubtopic = true;
            try
            {
                subtopic.ParentTopic = this;
            }
            finally
            {
                isSynchronizingSubtopic = false;
            }
            children.Add(subtopic);
        }

        /// <summary>
        /// Removes a subtopic from the current topic.
        /// </summary>
        /// <param name="subtopic">The subtopic to remove.</param>
        /// <returns>
        /// <see langword="true"/> if the subtopic was successfully removed; otherwise, <see langword="false"/> if the subtopic
        /// is not a child of the current topic.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="subtopic"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method removes a subtopic from the current topic, if it is a child of the topic. The removed subtopic will become
        /// a top-level topic with no parent.
        /// </remarks>
        public bool RemoveSubtopic(IMutableTopic subtopic)
        {
            if (subtopic is null)
                throw new ArgumentNullException(nameof(subtopic));

            if (!isSynchronizingSubtopic && children.Remove(subtopic))
            {
                isSynchronizingSubtopic = true;
                try
                {
                    subtopic.ParentTopic = null;
                }
                finally
                {
                    isSynchronizingSubtopic = false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Renders the content of the topic to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the content will be rendered.</param>
        /// <param name="context">The documentation context, which provides additional information for rendering the topic.</param>
        /// <inheritdoc/>
        public abstract void Render(TextWriter writer, IDocumentationContext context);

        /// <summary>
        /// Generates the title of the topic based on its name or content.
        /// </summary>
        /// <returns>The generated title of the topic.</returns>
        protected virtual string GenerateTitle() => Id.ToTitleCase();

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => Title;

        /// <summary>
        /// Checks if adding the specified topic as a subtopic would create a circular reference.
        /// </summary>
        /// <param name="subtopic">The topic that would be added as a subtopic.</param>
        /// <returns><see langword="true"/> if adding the topic would create a circular reference; otherwise, <see langword="false"/>.</returns>
        protected bool WouldCreateCircularReference(IMutableTopic subtopic)
        {
            if (ReferenceEquals(this, subtopic))
                return true;

            for (var parent = ParentTopic; parent is not null; parent = parent.ParentTopic)
            {
                if (ReferenceEquals(parent, subtopic))
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        string ITopic.Title => Title;

        /// <inheritdoc/>
        ITopic? ITopic.ParentTopic => ParentTopic;
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test
{
    using Kampute.DocToolkit.Topics;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A builder for creating mock <see cref="ITopic"/> instances.
    /// </summary>
    internal class MockTopicBuilder
    {
        private readonly string id;
        private readonly string title;
        private readonly List<MockTopicBuilder> children = [];
        private MockTopicBuilder? parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockTopicBuilder"/> class.
        /// </summary>
        /// <param name="id">The ID of the topic.</param>
        /// <param name="title">The title of the topic.</param>
        protected MockTopicBuilder(string id, string? title = null)
        {
            this.id = id;
            this.title = title ?? $"Title of {id}";
        }

        /// <summary>
        /// Gets the root topic builder in the hierarchy.
        /// </summary>
        /// <value>
        /// The root topic builder.
        /// </value>
        public MockTopicBuilder Root => parent?.Root ?? this;

        /// <summary>
        /// Adds a child topic to this topic and returns this topic (the parent).
        /// </summary>
        /// <param name="id">The ID of the child topic.</param>
        /// <param name="title">The title of the child topic.</param>
        /// <returns>This topic (the parent).</returns>
        public MockTopicBuilder WithChild(string id, string? title = null)
        {
            return WithChild(new MockTopicBuilder(id, title));
        }

        /// <summary>
        /// Adds a child topic to this topic and returns this topic (the parent).
        /// </summary>
        /// <param name="child">The child topic to add.</param>
        /// <returns>This topic (the parent).</returns>
        public MockTopicBuilder WithChild(MockTopicBuilder child)
        {
            child.parent = this;
            children.Add(child);
            return this;
        }

        /// <summary>
        /// Adds multiple child topics to this topic and returns this topic (the parent).
        /// </summary>
        /// <param name="ids">The IDs of the child topics.</param>
        /// <returns>This topic (the parent).</returns>
        public MockTopicBuilder WithChildren(params string[] ids)
        {
            return WithChildren(ids.Select(static id => new MockTopicBuilder(id)));
        }

        /// <summary>
        /// Adds multiple child topics to this topic and returns this topic (the parent).
        /// </summary>
        /// <param name="children">The child topics to add.</param>
        /// <returns>This topic (the parent).</returns>
        public MockTopicBuilder WithChildren(params IEnumerable<MockTopicBuilder> children)
        {
            foreach (var child in children)
            {
                child.parent = this;
                this.children.Add(child);
            }

            return this;
        }

        /// <summary>
        /// Sets the parent of this topic by creating a new parent topic and returns this topic (the child).
        /// </summary>
        /// <param name="id">The ID of the parent topic.</param>
        /// <param name="title">The title of the parent topic.</param>
        /// <returns>This topic (the child).</returns>
        public MockTopicBuilder WithParent(string id, string? title = null)
        {
            return WithParent(new MockTopicBuilder(id, title));
        }

        /// <summary>
        /// Sets the parent of this topic and returns this topic (the child).
        /// </summary>
        /// <param name="parent">The parent topic.</param>
        /// <returns>This topic (the child).</returns>
        public MockTopicBuilder WithParent(MockTopicBuilder parent)
        {
            this.parent = parent;
            parent.children.Add(this);
            return this;
        }

        /// <summary>
        /// Builds the topic using the configured parent-child relationships.
        /// </summary>
        /// <returns>The built topic.</returns>
        public ITopic Build()
        {
            return parent is null
                ? BuildInternal()
                : parent.Build().Subtopics.First(subtopic => subtopic.Id == id);
        }

        /// <summary>
        /// Builds the topic with the specified built parent topic.
        /// </summary>
        /// <param name="builtParent">The built parent topic instance.</param>
        /// <returns>The built topic.</returns>
        private ITopic BuildInternal(ITopic? builtParent = null)
        {
            var topicMock = new Mock<ITopic>();

            topicMock.SetupGet(t => t.Id).Returns(id);
            topicMock.SetupGet(t => t.Title).Returns(title);
            topicMock.SetupGet(t => t.ParentTopic).Returns(builtParent);

            var subtopics = children.Select(child => child.BuildInternal(topicMock.Object)).ToList();
            topicMock.SetupGet(t => t.Subtopics).Returns(subtopics);

            return topicMock.Object;
        }

        /// <summary>
        /// Creates a new topic builder.
        /// </summary>
        /// <param name="id">The ID of the topic.</param>
        /// <param name="title">The title of the topic.</param>
        /// <returns>A new topic builder.</returns>
        public static MockTopicBuilder Topic(string id, string? title = null) => new(id, title);
    }
}

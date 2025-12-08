// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides an abstract base class for HTML-based addressing strategies that organize documentation files in a hierarchical structure.
    /// </summary>
    /// <remarks>
    /// This class defines the common properties and methods for HTML addressing strategies that organize documentation pages in a hierarchical
    /// file structure suitable for web-based documentation.
    /// <para>
    /// It includes properties for API and topic paths, index topic names, and pinned topics, which are used to determine how documentation pages
    /// are addressed and organized within the HTML documentation structure.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public abstract class HtmlAddressingStrategy : AddressingStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlAddressingStrategy"/> class.
        /// </summary>
        /// <param name="options">The addressing options that configure how documentation pages are addressed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
        protected HtmlAddressingStrategy(HtmlAddressingOptions options)
            : base(options)
        {
            ApiPath = options.ApiPath;
            TopicPath = options.TopicPath;
            IndexTopicName = options.IndexTopicName;
            PinnedIndexTopics = options.PinnedIndexTopics.ToDictionary(static pair => pair.Name, static pair => pair.Path, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the path of the API documentation files.
        /// </summary>
        /// <value>
        /// The relative path to the API documentation files relative to the documentation root.
        /// </value>
        public string ApiPath { get; }

        /// <summary>
        /// Gets the path of the topic files.
        /// </summary>
        /// <value>
        /// The relative path to the topic files relative to the documentation root.
        /// </value>
        /// <exception cref="ArgumentException">Thrown when the path is not relative.</exception>
        /// <exception cref="ArgumentException">Thrown when the path contains invalid characters.</exception>
        public string TopicPath { get; }

        /// <summary>
        /// Gets the filename (without extension) used for topics with subtopics and pinned topics.
        /// </summary>
        /// <value>
        /// The name of the topic file for topics with subtopics and pinned topics.
        /// </value>
        public string IndexTopicName { get; }

        /// <summary>
        /// Gets the collection of pinned index topics, which are assigned specific paths relative to the documentation root.
        /// </summary>
        /// <value>
        /// A collection of topics that should be considered as index topics and placed at a specific path. The key of the dictionary
        /// is the topic name, and the value is the relative path where the topic should be located.
        /// </value>
        /// <remarks>
        /// When a topic is pinned, it is given a specific path that overrides the default addressing scheme. If a pinned topic has
        /// subtopics, they will also be addressed relative to the pinned topic's path.
        /// </remarks>
        public IReadOnlyDictionary<string, string> PinnedIndexTopics { get; }

        /// <summary>
        /// Constructs the API path for the specified subpath, ensuring it is relative to the configured API path.
        /// </summary>
        /// <param name="subpath">The subpath to append to the API path.</param>
        /// <returns>A string representing the full API path with the specified subpath appended.</returns>
        protected virtual string GetApiPath(string subpath) => string.IsNullOrEmpty(ApiPath) ? subpath : ApiPath + '/' + subpath;

        /// <summary>
        /// Constructs the path for the specified topic, replacing invalid characters with the provided replacement
        /// character.
        /// </summary>
        /// <param name="topic">The topic for which the path is being constructed.</param>
        /// <param name="replacement">The character used to replace invalid characters in the topic path.</param>
        /// <returns>A string representing the constructed path for the topic.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="topic"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// If the topic is pinned in <see cref="PinnedIndexTopics"/>, the pinned path is used as the base path. Otherwise, the path
        /// is constructed recursively from the topic's parent hierarchy. Invalid characters in each segment are replaced with the
        /// specified <paramref name="replacement"/> character.
        /// </remarks>
        protected virtual string GetTopicPath(ITopic topic, char replacement)
        {
            if (topic is null)
                throw new ArgumentNullException(nameof(topic));

            if (PinnedIndexTopics.TryGetValue(topic.Id, out var pinnedPath))
            {
                return string.IsNullOrEmpty(pinnedPath)
                    ? IndexTopicName
                    : pinnedPath + '/' + IndexTopicName;
            }

            var isPinned = false;
            var pathSegments = new Stack<string>();

            if (topic.Subtopics.Count > 0)
                pathSegments.Push(IndexTopicName);

            PushSanitizedPathSegment(topic.Id);
            for (var parent = topic.ParentTopic; parent is not null; parent = parent.ParentTopic)
            {
                if (PinnedIndexTopics.TryGetValue(parent.Id, out pinnedPath))
                {
                    pathSegments.Push(pinnedPath);
                    isPinned = true;
                    break;
                }

                PushSanitizedPathSegment(parent.Id);
            }

            if (!isPinned && !string.IsNullOrEmpty(TopicPath))
                pathSegments.Push(TopicPath);

            return string.Join('/', pathSegments);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void PushSanitizedPathSegment(string pathSegment)
            {
                var sanitizedSegment = pathSegment.ReplaceChars(UriHelper.InvalidPathSegmentCharacters, replacement, true);
                pathSegments.Push(sanitizedSegment);
            }
        }
    }
}

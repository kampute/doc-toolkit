// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Provides options for HTML-based addressing and organizing documentation files.
    /// </summary>
    /// <remarks>
    /// This abstract class defines common configuration options for HTML addressing strategies that organize documentation files
    /// in a hierarchical structure. Derived classes should extend these options with additional configuration properties
    /// specific to particular HTML addressing schemes.
    /// </remarks>
    /// <seealso cref="HtmlAddressingStrategy"/>
    public abstract class HtmlAddressingOptions : AddressingOptions
    {
        private readonly List<(string Id, string Path)> pinnedIndexTopics = [];

        private string apiPath = string.Empty;
        private string topicPath = string.Empty;
        private string indexTopicName = "index";

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlAddressingOptions"/> class.
        /// </summary>
        protected HtmlAddressingOptions()
            : this(FileExtensions.Html)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlAddressingOptions"/> class with a specified file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension for the documentation files.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fileExtension"/> is <see langword="null"/> or empty.</exception>
        protected HtmlAddressingOptions(string fileExtension)
            : base(fileExtension)
        {
        }

        /// <summary>
        /// Gets or sets the path of the API documentation files.
        /// </summary>
        /// <value>
        /// The relative path to the API documentation files relative to the documentation root.
        /// </value>
        /// <exception cref="ArgumentException">Thrown when the path is not relative.</exception>
        /// <exception cref="ArgumentException">Thrown when the path contains invalid characters.</exception>
        public string ApiPath
        {
            get => apiPath;
            set => apiPath = PathHelper.EnsureValidRelativePath(value);
        }

        /// <summary>
        /// Gets or sets the path of the topic files.
        /// </summary>
        /// <value>
        /// The relative path to the topic files relative to the documentation root.
        /// </value>
        /// <exception cref="ArgumentException">Thrown when the path is not relative.</exception>
        /// <exception cref="ArgumentException">Thrown when the path contains invalid characters.</exception>
        public string TopicPath
        {
            get => topicPath;
            set => topicPath = PathHelper.EnsureValidRelativePath(value);
        }

        /// <summary>
        /// Gets or sets the filename (without extension) used for topics with subtopics and pinned topics.
        /// </summary>
        /// <value>
        /// The name of the topic file for topics with subtopics and pinned topics. The default value is "index".
        /// </value>
        /// <exception cref="ArgumentException">Thrown when the value is <see langword="null"/>, empty, or contains invalid characters.</exception>
        public string IndexTopicName
        {
            get => indexTopicName;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException($"{nameof(IndexTopicName)} cannot be null or empty.", nameof(value));
                if (Path.GetInvalidFileNameChars().Any(value.Contains))
                    throw new ArgumentException($"{nameof(IndexTopicName)} contains invalid characters.", nameof(value));

                indexTopicName = value;
            }
        }

        /// <summary>
        /// Gets the collection of pinned index topics, which are assigned specific paths relative to the documentation root.
        /// </summary>
        /// <value>
        /// A collection of topics that should be considered as index topics and placed at a specific path.
        /// </value>
        /// <remarks>
        /// When a topic is pinned, it is given a specific path that overrides the default addressing scheme. If a pinned topic has
        /// subtopics, they will also be addressed relative to the pinned topic's path.
        /// </remarks>
        public IReadOnlyCollection<(string Name, string Path)> PinnedIndexTopics => pinnedIndexTopics;

        /// <summary>
        /// Adds a topic to the collection of pinned topics.
        /// </summary>
        /// <param name="topicId">The identifier of the topic to add to the pinned topics.</param>
        /// <param name="relativePath">The relative path associated with the topic.</param>
        /// <returns><see langword="true"/> if the topic was successfully added; otherwise, <see langword="false"/> if a topic with the same name or path already exists.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="topicId"/> is <see langword="null"/> or empty, or if <paramref name="relativePath"/> is not relative or contains invalid characters.</exception>
        public bool AddPinnedTopic(string topicId, string relativePath)
        {
            if (string.IsNullOrEmpty(topicId))
                throw new ArgumentException($"{nameof(topicId)} cannot be null or empty.", nameof(topicId));

            var normalizedPath = PathHelper.EnsureValidRelativePath(relativePath);

            foreach (var (name, path) in pinnedIndexTopics)
            {
                if (name.Equals(topicId, StringComparison.OrdinalIgnoreCase))
                    return false;
                if (path.Equals(normalizedPath, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            pinnedIndexTopics.Add((topicId, normalizedPath));
            return true;
        }

        /// <summary>
        /// Removes a topic from the collection of pinned topics.
        /// </summary>
        /// <param name="topicId">The identifier of the topic to remove from the pinned topics.</param>
        /// <returns><see langword="true"/> if the topic was successfully removed; otherwise, <see langword="false"/> if the topic was not found.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="topicId"/> is <see langword="null"/> or empty.</exception>
        public bool RemovePinnedTopic(string topicId)
        {
            if (string.IsNullOrEmpty(topicId))
                throw new ArgumentException($"{nameof(topicId)} cannot be null or empty.", nameof(topicId));

            var i = pinnedIndexTopics.FindIndex(pair => pair.Id.Equals(topicId, StringComparison.OrdinalIgnoreCase));
            if (i != -1)
            {
                pinnedIndexTopics.RemoveAt(i);
                return true;
            }

            return false;
        }
    }
}

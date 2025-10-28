// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Collections
{
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// Represents a collection of top-level topics in a documentation context.
    /// </summary>
    /// <remarks>
    /// The <see cref="TopicCollection"/> class provides functionality for managing top-level topics in a documentation context.
    /// It maintains topics in an efficient dictionary structure with case-insensitive keys, enabling fast lookups for a topic in
    /// the topic hierarchy by its qualified identifier or URI reference.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public class TopicCollection : IReadOnlyTopicCollection
    {
        private readonly List<TopicModel> rootTopics = [];
        private readonly Dictionary<string, TopicModel> allTopics = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, TopicModel> topicsByPath = new(StringComparer.OrdinalIgnoreCase);
        private readonly Func<IDocumentationContext, ITopic, TopicModel> topicModelFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicCollection"/> class.
        /// </summary>
        /// <param name="context">The documentation context to associate with this collection of topics.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
        public TopicCollection(IDocumentationContext context)
            : this(context, static (ctx, src) => new TopicModel(ctx, src))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicCollection"/> class with a custom topic factory.
        /// </summary>
        /// <param name="context">The documentation context to associate with this collection of topics.</param>
        /// <param name="modelFactory">The factory function to create model instances for topics in the collection.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="modelFactory"/> is <see langword="null"/>.</exception>
        public TopicCollection(IDocumentationContext context, Func<IDocumentationContext, ITopic, TopicModel> modelFactory)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            topicModelFactory = modelFactory ?? throw new ArgumentNullException(nameof(modelFactory));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicCollection"/> class with initial topics.
        /// </summary>
        /// <param name="context">The documentation context to associate with this collection of topics.</param>
        /// <param name="topics">The initial topics to add to the collection.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="topics"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="topics"/> contains a topic that is not a top-level topic.</exception>
        public TopicCollection(IDocumentationContext context, IEnumerable<ITopic> topics)
            : this(context)
        {
            if (topics is null)
                throw new ArgumentNullException(nameof(topics));

            foreach (var topic in topics)
                Add(topic);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopicCollection"/> class with a custom topic factory and initial topics.
        /// </summary>
        /// <param name="context">The documentation context to associate with this collection of topics.</param>
        /// <param name="modelFactory">The factory function to create model instances for topics in the collection.</param>
        /// <param name="topics">The initial topics to add to the collection.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/>, <paramref name="modelFactory"/>, or <paramref name="topics"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="topics"/> contains a topic that is not a top-level topic.</exception>
        public TopicCollection(IDocumentationContext context, Func<IDocumentationContext, ITopic, TopicModel> modelFactory, IEnumerable<ITopic> topics)
            : this(context, modelFactory)
        {
            if (topics is null)
                throw new ArgumentNullException(nameof(topics));

            foreach (var topic in topics)
                Add(topic);
        }

        /// <summary>
        /// Gets the documentation context associated with this collection of topics.
        /// </summary>
        /// <value>
        /// The <see cref="IDocumentationContext"/> that this collection belongs to.
        /// </value>
        public IDocumentationContext Context { get; }

        /// <summary>
        /// Gets the number of top-level topics in the collection.
        /// </summary>
        /// <value>
        /// The number of topics in the collection.
        /// </value>
        public int Count => rootTopics.Count;

        /// <summary>
        /// Gets all topics in the collection, including all nested topics.
        /// </summary>
        /// <value>
        /// The read-only collection of all topics in the collection, including nested topics.
        /// </value>
        public IReadOnlyCollection<TopicModel> Flatten => allTopics.Values;

        /// <summary>
        /// Adds a top-level topic and all its subtopics to the collection.
        /// </summary>
        /// <param name="topLevelTopic">The topic to add.</param>
        /// <returns><see langword="true"/> if the topic was added successfully; otherwise, <see langword="false"/>, indicating that a topic with the same identifier already exists.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="topLevelTopic"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="topLevelTopic"/> has a parent topic, indicating that it is not a top-level topic.</exception>
        public bool Add(ITopic topLevelTopic)
        {
            if (topLevelTopic is null)
                throw new ArgumentNullException(nameof(topLevelTopic));

            if (topLevelTopic.ParentTopic is not null)
                throw new ArgumentException("The topic must be a top-level topic without a parent.", nameof(topLevelTopic));

            if (allTopics.ContainsKey(topLevelTopic.Id))
                return false;

            var contextualTopic = topicModelFactory(Context, topLevelTopic);

            rootTopics.Add(contextualTopic);
            allTopics.Add(contextualTopic.Id, contextualTopic);
            if (topLevelTopic is IFileBasedTopic fileTopic)
                topicsByPath.TryAdd(NormalizePathFormat(fileTopic.FilePath), contextualTopic);

            AddSubtopics(contextualTopic);
            return true;

            void AddSubtopics(TopicModel topic)
            {
                foreach (var subtopic in topic.Subtopics)
                {
                    allTopics.TryAdd(subtopic.Id, subtopic);
                    if (subtopic.Source is IFileBasedTopic fileSubtopic)
                        topicsByPath.TryAdd(NormalizePathFormat(fileSubtopic.FilePath), subtopic);
                    if (subtopic.Subtopics.Count > 0)
                        AddSubtopics(subtopic);
                }
            }

            static string NormalizePathFormat(string path) => path.Replace('\\', '/');
        }

        /// <summary>
        /// Removes a top-level topic and all its subtopics from the collection.
        /// </summary>
        /// <param name="topLevelTopic">The topic to remove.</param>
        /// <returns><see langword="true"/> if the topic was successfully removed from the collection; otherwise, <see langword="false"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="topLevelTopic"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="topLevelTopic"/> is not a top-level topic.</exception>
        public bool Remove(TopicModel topLevelTopic)
        {
            if (topLevelTopic is null)
                throw new ArgumentNullException(nameof(topLevelTopic));
            if (topLevelTopic.ParentTopic is not null)
                throw new ArgumentException("The topic must be a top-level topic without a parent.", nameof(topLevelTopic));

            if (ReferenceEquals(topLevelTopic.Context, Context) && rootTopics.Remove(topLevelTopic))
            {
                allTopics.Remove(topLevelTopic.Id);
                if (topLevelTopic is IFileBasedTopic fileTopic)
                    topicsByPath.Remove(fileTopic.FilePath);

                RemoveSubtopics(topLevelTopic);
                return true;
            }

            return false;

            void RemoveSubtopics(TopicModel topic)
            {
                foreach (var subtopic in topic.Subtopics)
                {
                    allTopics.Remove(subtopic.Id);
                    if (subtopic is IFileBasedTopic fileTopic)
                        topicsByPath.Remove(fileTopic.FilePath);
                    if (subtopic.Subtopics.Count > 0)
                        RemoveSubtopics(subtopic);
                }
            }
        }

        /// <summary>
        /// Removes all topics from the collection.
        /// </summary>
        public void Clear()
        {
            rootTopics.Clear();
            allTopics.Clear();
            topicsByPath.Clear();
        }

        /// <summary>
        /// Attempts to resolve a topic in the collection based on the provided reference string.
        /// </summary>
        /// <param name="reference">The reference string used to identify the topic, such as a file path or identifier.</param>
        /// <param name="topic">When this method returns, contains the resolved topic if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the topic was successfully resolved; otherwise, <see langword="false"/>.</returns>
        /// <inheritdoc/>
        public bool TryResolve(string reference, [NotNullWhen(true)] out TopicModel? topic)
        {
            if (Count == 0 || string.IsNullOrEmpty(reference))
            {
                topic = null;
                return false;
            }

            var isAbsolute = reference.StartsWith('/');

            if (!isAbsolute && Context.AddressProvider.ActiveScope.Model is TopicModel referee)
            {
                // If referee is a file-backed topic, attempt to resolve the reference as a relative file path to it
                if (referee.Source is IFileBasedTopic fileTopic)
                {
                    var absolutePath = Path.Combine(Path.GetDirectoryName(fileTopic.FilePath)!, reference);
                    if (TryGetByFilePath(absolutePath, out topic))
                        return true;
                }

                // If the reference is a valid identifier relative to the referee topic, attempt to resolve it
                if (PathHelper.TryNormalizePath(referee.Id + '/' + reference, out var qualifiedId))
                {
                    if (TryGetById(qualifiedId, out topic))
                        return true;
                }
            }

            // When reference is an absolute file path or identifier, attempt to resolve it directly
            if ((isAbsolute && reference.Length > 1) || !PathHelper.HasDotSegment(reference))
            {
                if (isAbsolute)
                    reference = reference[1..];

                if (TryGetById(reference, out topic))
                    return true;

                if (TryFindBySubpath(reference, out topic))
                    return true;
            }

            topic = null;
            return false;
        }

        /// <summary>
        /// Attempts to find a topic in the topic hierarchy by its qualified identifier.
        /// </summary>
        /// <param name="id">The qualified identifier of the topic to lookup.</param>
        /// <param name="topic">When this method returns, contains the topic if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the topic was found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <inheritdoc/>
        public bool TryGetById(string id, [NotNullWhen(true)] out TopicModel? topic)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            return allTopics.TryGetValue(id, out topic);
        }

        /// <summary>
        /// Attempts to lookup a file-backed topic in the topic hierarchy by its full path.
        /// </summary>
        /// <param name="filePath">The file path of the topic to lookup.</param>
        /// <param name="topic">When this method returns, contains the topic if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the topic was found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is <see langword="null"/>.</exception>
        /// <inheritdoc/>
        public bool TryGetByFilePath(string filePath, [NotNullWhen(true)] out TopicModel? topic)
        {
            if (filePath is null)
                throw new ArgumentNullException(nameof(filePath));

            if (PathHelper.TryNormalizePath(filePath, out var normalizedPath))
                return topicsByPath.TryGetValue(normalizedPath, out topic);

            topic = null;
            return false;
        }

        /// <summary>
        /// Attempts to find a file-backed topic in the topic hierarchy by its subpaths.
        /// </summary>
        /// <param name="filePath">The file path or subpath of the topic to lookup.</param>
        /// <param name="topic">When this method returns, contains the topic that uniquely matches the specified file path or subpath; otherwise, <see langword="null"/> if no match or if ambiguous.</param>
        /// <returns><see langword="true"/> if a unique matching topic was found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is <see langword="null"/> or empty.</exception>
        public bool TryFindBySubpath(string filePath, [NotNullWhen(true)] out TopicModel? topic)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException($"{nameof(filePath)} cannot be null or empty.", nameof(filePath));

            if (!PathHelper.TryNormalizePath(filePath, out var subPath))
            {
                topic = null;
                return false;
            }

            topic = null;
            foreach (var (sourceFilePath, fileBasedTopic) in topicsByPath)
            {
                if (PathHelper.IsSubpath(sourceFilePath, subPath))
                {
                    if (topic is not null)
                    {
                        // Ambiguous match
                        topic = null;
                        return false;
                    }

                    topic = fileBasedTopic;
                }
            }
            return topic is not null;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection of top-level topics.</returns>
        public IEnumerator<TopicModel> GetEnumerator() => rootTopics.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

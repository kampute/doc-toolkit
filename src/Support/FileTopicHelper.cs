// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides helper methods for working with topics.
    /// </summary>
    public static class FileTopicHelper
    {
        /// <summary>
        /// Organizes file-backed topics into a hierarchy where files become parent topics for directories with matching names.
        /// </summary>
        /// <param name="topics">The collection of file-backed topics to organize.</param>
        /// <returns>An enumerable collection of <see cref="FileTopic"/> instances that represent the top-level topics in the hierarchy.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="topics"/> is <see langword="null"/> or contains a <see langword="null"/> topic.</exception>
        /// <remarks>
        /// This method creates parent-child relationships by matching file names (without extension) to directory names.
        /// When a file and directory share the same name, the file becomes the parent topic for all files within that directory.
        /// The matching is case-insensitive and works recursively through nested directory structures.
        /// <para>
        /// <note type="caution" title="Caution">
        /// This method assumes that the input topics do not have any existing parent-child relationships. If such relationships exist,
        /// they will be overwritten.
        /// </note>
        /// <note type="tip" title="Tip">
        /// The method preserves the order of topics as they were provided in the input collection. Therefore, sort the input collection
        /// if you want to control the order of top-level topics and their subtopics.
        /// </note>
        /// </para>
        /// <para>
        /// Example file structure and resulting hierarchy:
        /// </para>
        /// <code language="txt">
        /// Input files:
        ///   guides.md
        ///   guides/installation.md
        ///   guides/advanced.md
        ///   guides/advanced/scripting.md
        ///   tutorials.md
        ///   tutorials/setup.md
        ///   standalone.md
        ///
        /// Resulting hierarchy:
        ///   guides.md (parent)
        ///   ├── guides/installation.md
        ///   └── guides/advanced.md (parent)
        ///       └── guides/advanced/scripting.md
        ///   tutorials.md (parent)
        ///   └── tutorials/setup.md
        ///   standalone.md (no children)
        /// </code>
        /// </remarks>
        public static IEnumerable<FileTopic> ConstructHierarchyByDirectory(IEnumerable<FileTopic> topics)
        {
            if (topics is null)
                throw new ArgumentNullException(nameof(topics));

            var allTopics = Collect(topics);
            if (allTopics.Count <= 1)
                return allTopics;

            var topicsByPath = new Dictionary<string, FileTopic>(StringComparer.OrdinalIgnoreCase);
            foreach (var topic in allTopics)
            {
                if (topic is null)
                    throw new ArgumentNullException(nameof(topics), "Topics collection contains a null topic.");

                topic.ParentTopic = null;
                topicsByPath[topic.FilePath] = topic;
            }

            var filesByDirectory = allTopics
                .GroupBy(topic => Path.GetDirectoryName(topic.FilePath) ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.Select(t => t.FilePath).ToList());

            foreach (var (directoryPath, filesInDirectory) in filesByDirectory)
            {
                if (filesInDirectory.Count > 0 && TryFindParentTopicForDirectory(directoryPath, out var parentTopic))
                {
                    foreach (var childFilePath in filesInDirectory)
                    {
                        if (topicsByPath.TryGetValue(childFilePath, out var childTopic))
                            childTopic.ParentTopic = parentTopic;
                    }
                }
            }

            return allTopics.Where(topic => topic.ParentTopic is null);

            bool TryFindParentTopicForDirectory(string directoryPath, [NotNullWhen(true)] out FileTopic? parentTopic)
            {
                parentTopic = null;

                var directoryName = Path.GetFileName(directoryPath);
                if (string.IsNullOrEmpty(directoryName))
                    return false;

                var parentDirectoryPath = Path.GetDirectoryName(directoryPath) ?? string.Empty;

                if (!filesByDirectory.TryGetValue(parentDirectoryPath, out var parentDirectoryFiles))
                    return false;

                var parentFilePath = parentDirectoryFiles.FirstOrDefault(filePath =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    return string.Equals(fileName, directoryName, StringComparison.OrdinalIgnoreCase);
                });

                return parentFilePath is not null && topicsByPath.TryGetValue(parentFilePath, out parentTopic);
            }
        }

        /// <summary>
        /// Organizes file-backed topics into a hierarchy where index files become parent topics for other files in the same directory.
        /// </summary>
        /// <param name="topics">The collection of file-backed topics to organize.</param>
        /// <param name="indexFileName">The file name (without extension) that identifies index files.</param>
        /// <returns>An enumerable collection of <see cref="FileTopic"/> instances that represent the top-level topics in the hierarchy.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="topics"/> is <see langword="null"/> or contains a <see langword="null"/> topic.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="indexFileName"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <remarks>
        /// This method creates parent-child relationships within each directory by designating files with the specified index name
        /// as parent topics for all other files in the same directory. The index file name matching is case-insensitive.
        /// <para>
        /// Files in directories that contain only one file or lack an index file are not assigned parent relationships.
        /// </para>
        /// <para>
        /// <note type="caution" title="Caution">
        /// This method assumes that the input topics do not have any existing parent-child relationships. If such relationships exist,
        /// they will be overwritten.
        /// </note>
        /// <note type="tip" title="Tip">
        /// The method preserves the order of topics as they were provided in the input collection. Therefore, sort the input collection
        /// if you want to control the order of top-level topics and their subtopics.
        /// </note>
        /// </para>
        /// <para>
        /// Example file structure with <c>indexFileName = "index"</c>:
        /// </para>
        /// <code language="txt">
        /// Input files:
        ///   guides/index.md
        ///   guides/installation.md
        ///   guides/advanced/index.md
        ///   guides/advanced/scripting.md
        ///   tutorials/intro.md
        ///   tutorials/setup.md
        ///   standalone.md
        ///
        /// Resulting hierarchy:
        ///   guides/index.md (parent)
        ///   ├── guides/installation.md
        ///   └── guides/advanced/index.md (parent)
        ///       └── guides/advanced/scripting.md
        ///   tutorials/intro.md (no index file in tutorials/)
        ///   tutorials/setup.md
        ///   standalone.md
        /// </code>
        /// </remarks>
        public static IEnumerable<FileTopic> ConstructHierarchyByIndexFile(IEnumerable<FileTopic> topics, string indexFileName)
        {
            if (topics is null)
                throw new ArgumentNullException(nameof(topics));
            if (string.IsNullOrWhiteSpace(indexFileName))
                throw new ArgumentException($"'{nameof(indexFileName)}' cannot be null, empty, or whitespace.", nameof(indexFileName));

            var allTopics = Collect(topics);
            if (allTopics.Count <= 1)
                return allTopics;

            var topicsByPath = new Dictionary<string, FileTopic>(StringComparer.OrdinalIgnoreCase);
            foreach (var topic in allTopics)
            {
                if (topic is null)
                    throw new ArgumentNullException(nameof(topics), "Topics collection contains a null topic.");

                topic.ParentTopic = null;
                topicsByPath[topic.FilePath] = topic;
            }

            var filesByDirectory = allTopics
                .GroupBy(topic => Path.GetDirectoryName(topic.FilePath) ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.Select(t => t.FilePath).ToList());

            foreach (var (directoryPath, filesInDirectory) in filesByDirectory)
            {
                if (filesInDirectory.Count <= 1)
                    continue;

                var indexFilePath = filesInDirectory.FirstOrDefault(filePath =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    return string.Equals(fileName, indexFileName, StringComparison.OrdinalIgnoreCase);
                });

                if (indexFilePath is not null && topicsByPath.TryGetValue(indexFilePath, out var indexTopic))
                {
                    foreach (var childFilePath in filesInDirectory)
                    {
                        if (childFilePath != indexFilePath && topicsByPath.TryGetValue(childFilePath, out var childTopic))
                            childTopic.ParentTopic = indexTopic;
                    }
                }
            }

            return allTopics.Where(topic => topic.ParentTopic is null);
        }

        /// <summary>
        /// Organizes file-backed topics into a hierarchy using delimited prefixes in file names.
        /// </summary>
        /// <param name="topics">The collection of file-backed topics to organize.</param>
        /// <param name="delimiter">The character used to separate hierarchy levels in file names.</param>
        /// <returns>An enumerable collection of <see cref="FileTopic"/> instances that represent the top-level topics in the hierarchy.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="topics"/> is <see langword="null"/> or contains a <see langword="null"/> topic.</exception>
        /// <exception cref="ArgumentException">Thrown when multiple topics have the same file name (without extension), regardless of directory path.</exception>
        /// <remarks>
        /// This method creates parent-child relationships by analyzing file name prefixes separated by the specified delimiter.
        /// Files with fewer delimiter segments become parents of files with more segments that share the same prefix. File name
        /// comparison is case-insensitive.
        /// <para>
        /// In this method, directories of files are ignored, and only the filenames are considered for hierarchy construction.
        /// This means that files with the same name but located in different directories will be treated as if they were in
        /// the same directory, and in such cases, an exception will be thrown.
        /// </para>
        /// <para>
        /// <note type="caution" title="Caution">
        /// This method assumes that the input topics do not have any existing parent-child relationships. If such relationships exist,
        /// they will be overwritten.
        /// </note>
        /// <note type="tip" title="Tip">
        /// The method preserves the order of topics as they were provided in the input collection. Therefore, sort the input collection
        /// if you want to control the order of top-level topics and their subtopics.
        /// </note>
        /// </para>
        /// <para>
        /// Example file structure with <c>delimiter = '.'</c>:
        /// </para>
        /// <code language="txt">
        /// Input files (directories ignored):
        ///   docs/guides.md
        ///   help/guides.installation.md
        ///   guides.advanced.md
        ///   other/guides.advanced.scripting.md
        ///   tutorials.md
        ///   tutorials.setup.md
        ///
        /// Resulting hierarchy:
        ///   guides.md (parent)
        ///   ├── guides.installation.md
        ///   └── guides.advanced.md (parent)
        ///       └── guides.advanced.scripting.md
        ///   tutorials.md (parent)
        ///   └── tutorials.setup.md
        /// </code>
        /// </remarks>
        public static IEnumerable<FileTopic> ConstructHierarchyByFilenamePrefix(IEnumerable<FileTopic> topics, char delimiter)
        {
            if (topics is null)
                throw new ArgumentNullException(nameof(topics));

            var allTopics = Collect(topics);
            if (allTopics.Count <= 1)
                return allTopics;

            var topicsByFileName = new Dictionary<string, FileTopic>(StringComparer.OrdinalIgnoreCase);
            foreach (var topic in allTopics)
            {
                if (topic is null)
                    throw new ArgumentNullException(nameof(topics), "Topics collection contains a null topic.");

                topic.ParentTopic = null;
                var fileName = Path.GetFileNameWithoutExtension(topic.FilePath);
                if (!topicsByFileName.TryAdd(fileName, topic))
                    throw new ArgumentException($"Duplicate topic found with the same filename: '{fileName}'.", nameof(topics));
            }

            if (topicsByFileName.Count <= 1)
                return allTopics;

            var processedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var topic in allTopics)
            {
                var fileName = Path.GetFileNameWithoutExtension(topic.FilePath);
                ProcessHierarchy(topic, fileName);
            }

            return allTopics.Where(topic => topic.ParentTopic is null);

            void ProcessHierarchy(FileTopic topic, string fileName)
            {
                if (!processedFileNames.Add(fileName))
                    return;

                var delimiterIndex = fileName.LastIndexOf(delimiter);
                if (delimiterIndex > 0)
                {
                    var parentFileName = fileName[..delimiterIndex];
                    if (topicsByFileName.TryGetValue(parentFileName, out var parentTopic))
                    {
                        ProcessHierarchy(parentTopic, parentFileName);
                        topic.ParentTopic = parentTopic;
                    }
                }
            }
        }

        /// <summary>
        /// Collects the topics into a read-only collection.
        /// </summary>
        /// <param name="topics">The collection of topics to collect.</param>
        /// <returns>A read-only collection of <see cref="FileTopic"/> instances.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyCollection<FileTopic> Collect(IEnumerable<FileTopic> topics) => topics as IReadOnlyCollection<FileTopic> ?? [.. topics];
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Collections
{
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.Topics;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a read-only collection of top-level topics in a documentation context.
    /// </summary>
    /// <remarks>
    /// This interface provides immutable access to topic instances in a documentation context. It allows efficient
    /// lookup by topic identifier or URI reference for cross-referencing topics within documentation.
    /// </remarks>
    public interface IReadOnlyTopicCollection : IReadOnlyCollection<TopicModel>
    {
        /// <summary>
        /// Gets the documentation context associated with this collection of topics.
        /// </summary>
        /// <value>
        /// The <see cref="IDocumentationContext"/> that this collection belongs to.
        /// </value>
        IDocumentationContext Context { get; }

        /// <summary>
        /// Gets all topics in the collection, including all nested topics.
        /// </summary>
        /// <value>
        /// The read-only collection of all topics in the collection, including nested topics.
        /// </value>
        IReadOnlyCollection<TopicModel> Flatten { get; }

        /// <summary>
        /// Attempts to lookup a topic in the topic hierarchy by its qualified identifier.
        /// </summary>
        /// <param name="id">The qualified identifier of the topic to lookup.</param>
        /// <param name="topic">When this method returns, contains the topic if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the topic was found; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method performs a case-insensitive lookup for a topic by its identifier in constant time O(1).
        /// <para>
        /// The identifier must be fully qualified, which means it should include the identifier of all parent
        /// topics in the hierarchy separated by slashes ('/').
        /// </para>
        /// </remarks>
        bool TryGetById(string id, [NotNullWhen(true)] out TopicModel? topic);

        /// <summary>
        /// Attempts to lookup a file-backed topic in the topic hierarchy by its exact source file path.
        /// </summary>
        /// <param name="filePath">The exact file path of the topic to lookup.</param>
        /// <param name="topic">When this method returns, contains the topic if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the topic was found; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method looks for a file-backed topic in the collection that exactly has the specified file path
        /// as its source in constant time O(1).
        /// <para>
        /// Lookup is performed in a case-insensitive manner and directory separators can be either slash ('/')
        /// or backslash ('\').
        /// </para>
        /// </remarks>
        /// <seealso cref="IFileBasedTopic"/>
        bool TryGetByFilePath(string filePath, [NotNullWhen(true)] out TopicModel? topic);

        /// <summary>
        /// Attempts to find a file-backed topic in the topic hierarchy by its subpaths.
        /// </summary>
        /// <param name="filePath">The file path or subpath of the topic to lookup.</param>
        /// <param name="topic">When this method returns, contains the topic that uniquely matches the specified file path or subpath; otherwise, <see langword="null"/> if no match or if ambiguous.</param>
        /// <returns><see langword="true"/> if a unique matching topic was found; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method searches for a file-backed topic whose file path is equal to or is a more specific path than
        /// the provided <paramref name="filePath"/>. If multiple topics match, the match is considered ambiguous and
        /// the method returns <see langword="false"/>.
        /// <para>
        /// Matching is case-insensitive and directory separators can be either slash ('/') or backslash ('\').
        /// The search is performed in linear time O(n) relative to the number of file-backed
        /// topics.
        /// </para>
        /// </remarks>
        /// <seealso cref="IFileBasedTopic"/>
        bool TryFindBySubpath(string filePath, [NotNullWhen(true)] out TopicModel? topic);

        /// <summary>
        /// Attempts to resolve a topic in the collection based on the provided reference string.
        /// </summary>
        /// <param name="reference">The reference string used to identify the topic, such as a file path or identifier.</param>
        /// <param name="topic">When this method returns, contains the resolved topic if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the topic was successfully resolved; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method attempts to resolve a topic using the provided <paramref name="reference"/>, which can be
        /// a file path or a topic identifier. If the reference is relative, it is resolved against the currently
        /// active model in the documentation context.
        /// <para>
        /// The resolution process first tries to find a file-backed topic by the given reference. If no such topic
        /// is found, it then attempts to find a topic by its identifier. The search is case-insensitive and
        /// directory separators can be either slash ('/') or backslash ('\').
        /// </para>
        /// </remarks>
        bool TryResolve(string reference, [NotNullWhen(true)] out TopicModel? topic);
    }
}

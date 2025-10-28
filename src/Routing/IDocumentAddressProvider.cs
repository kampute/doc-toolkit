// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a contract for resolving the documentation URLs and files for code references, types, members, and topics.
    /// </summary>
    /// <remarks>
    /// This provider unifies various addressing resolution capabilities in the documentation system, combining URL
    /// resolution, file resolution, and topic addressing. It serves as a central access point for all addressing
    /// concerns, simplifying integration with the rest of the documentation generation pipeline. By consolidating
    /// these responsibilities, it ensures consistent addressing behavior across the entire documentation system.
    /// </remarks>
    public interface IDocumentAddressProvider : IApiDocUrlProvider, IDocumentUrlContextProvider
    {
        /// <summary>
        /// Gets the page granularity used for organizing documentation content.
        /// </summary>
        /// <value>
        /// The granularity that specifies which code elements (namespaces, types, members) receive their own dedicated pages
        /// in the documentation system.
        /// </value>
        PageGranularity Granularity { get; }

        /// <summary>
        /// Attempts to retrieve the URL for the specified topic.
        /// </summary>
        /// <param name="topic">The topic to retrieve the URL for.</param>
        /// <param name="url">
        /// When this method returns, contains the URL of the topic if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the URL of the topic is found; otherwise, <see langword="false"/>.</returns>
        bool TryGetTopicUrl(ITopic topic, [NotNullWhen(true)] out Uri? url);

        /// <summary>
        /// Attempts to retrieves the path of the documentation file for the specified namespace.
        /// </summary>
        /// <param name="ns">The namespace to retrieve the documentation file path for.</param>
        /// <param name="relativePath">
        /// When this method returns, contains the relative path of the documentation file for the specified namespace, if the namespace
        /// has a documentation file path; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if the namespace has a documentation file path; otherwise, <see langword="false"/>.</returns>
        bool TryGetNamespaceFile(string ns, [NotNullWhen(true)] out string? relativePath);

        /// <summary>
        /// Attempts to retrieve the path of the documentation file for the specified member.
        /// </summary>
        /// <param name="member">The member to retrieve the documentation file path for.</param>
        /// <param name="relativePath">
        /// When this method returns, contains the relative path of the documentation file for the specified member, if the member has
        /// a documentation file path; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if the member has a documentation file path; otherwise, <see langword="false"/>.</returns>
        bool TryGetMemberFile(IMember member, [NotNullWhen(true)] out string? relativePath);

        /// <summary>
        /// Attempts to retrieve the path of the file for the specified topic.
        /// </summary>
        /// <param name="topic">The topic to retrieve the file path for.</param>
        /// <param name="relativePath">
        /// When this method returns, contains the relative file path the topic if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the file path of the topic is found; otherwise, <see langword="false"/>.</returns>
        bool TryGetTopicFile(ITopic topic, [NotNullWhen(true)] out string? relativePath);
    }
}

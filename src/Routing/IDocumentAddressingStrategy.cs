// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Topics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Specifies how documentation content is organized into addressable pages.
    /// </summary>
    /// <remarks>
    /// The addressing strategy is a core component of the documentation system that defines how documentation resources are organized
    /// and accessed. It handles the translation between topics and code elements (namespaces, types, and members) and their corresponding
    /// documentation resource locations.
    /// <para>
    /// When generating documentation, the addressing strategy maintains consistent linking between topics by resolving the appropriate
    /// URLs for references based on the current configuration. This allows for flexible documentation organization while preserving the
    /// integrity of inter-topic references.
    /// </para>
    /// This enables features like navigation, linking between documentation pages, and integration with external reference systems without
    /// requiring manual updates to cross-references when documentation structure changes.
    /// </remarks>
    public interface IDocumentAddressingStrategy
    {
        /// <summary>
        /// Gets the file extension of the documentation files.
        /// </summary>
        /// <value>
        /// The file extension (including the leading period) of the documentation files.
        /// </value>
        string FileExtension { get; }

        /// <summary>
        /// Gets the page granularity used for organizing documentation content.
        /// </summary>
        /// <value>
        /// The granularity that specifies which code elements (namespaces, types, members) receive their own dedicated pages
        /// in the documentation system.
        /// </value>
        PageGranularity Granularity { get; }

        /// <summary>
        /// Attempts to resolve the address of the documentation content for the specified namespace.
        /// </summary>
        /// <param name="ns">The namespace to resolve the address for.</param>
        /// <param name="address">When this method returns, contains the address of the documentation content for the specified namespace, or <see langword="null"/> if the address could not be resolved.</param>
        /// <returns><see langword="true"/> if the address was successfully resolved; otherwise, <see langword="false"/>.</returns>
        bool TryResolveNamespaceAddress(string ns, [NotNullWhen(true)] out IResourceAddress? address);

        /// <summary>
        /// Attempts to resolve the address of the documentation content for the specified member.
        /// </summary>
        /// <param name="member">The member to resolve the address for.</param>
        /// <param name="address">When this method returns, contains the address of the documentation content for the specified member, or <see langword="null"/> if the address could not be resolved.</param>
        /// <returns><see langword="true"/> if the address was successfully resolved; otherwise, <see langword="false"/>.</returns>
        bool TryResolveMemberAddress(IMember member, [NotNullWhen(true)] out IResourceAddress? address);

        /// <summary>
        /// Attempts to resolve the address for the documentation content of the specified topic.
        /// </summary>
        /// <param name="topic">The topic for which to resolve the address.</param>
        /// <param name="address">When this method returns, contains the address of the documentation content for the specified topic, or <see langword="null"/> if the address could not be resolved.</param>
        /// <returns><see langword="true"/> if the address was successfully resolved; otherwise, <see langword="false"/>.</returns>
        bool TryResolveTopicAddress(ITopic topic, [NotNullWhen(true)] out IResourceAddress? address);

        /// <summary>
        /// Determines whether the specified member can be addressed by this strategy.
        /// </summary>
        /// <param name="member">The member to check.</param>
        /// <returns><see langword="true"/> if the member can be addressed; otherwise, <see langword="false"/>.</returns>
        bool IsAddressable(IMember member);
    }
}

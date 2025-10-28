// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines a contract for resolving URLs to documentation of code elements.
    /// </summary>
    /// <remarks>
    /// This resolver is responsible for mapping code elements to their corresponding documentation URLs, which is essential for creating
    /// hyperlinked references between code and documentation. It enables the documentation system to link to both internal and external
    /// documentation sources, enhancing the connectivity and navigability of the generated documentation. Implementations may use various
    /// strategies including pattern-based lookups, remote APIs, or search services.
    /// </remarks>
    public interface IApiDocUrlProvider
    {
        /// <summary>
        /// Attempts to retrieve the documentation URL for the specified namespace.
        /// </summary>
        /// <param name="ns">The namespace to retrieve the documentation URL for.</param>
        /// <param name="url">When this method returns, contains the documentation URL for the specified namespace, if it has a documentation URL; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the namespace has a documentation URL; otherwise, <see langword="false"/>.</returns>
        bool TryGetNamespaceUrl(string ns, [NotNullWhen(true)] out Uri? url);

        /// <summary>
        /// Attempts to retrieve the documentation URL for the specified member.
        /// </summary>
        /// <param name="member">The member to retrieve the documentation URL for.</param>
        /// <param name="url">When this method returns, contains the documentation URL for the specified member, if it has a documentation URL; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the member has a documentation URL; otherwise, <see langword="false"/>.</returns>
        bool TryGetMemberUrl(IMember member, [NotNullWhen(true)] out Uri? url);
    }
}

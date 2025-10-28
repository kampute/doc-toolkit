// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides abstract functionality to resolve URLs to external documentation sites for code elements in specific namespaces.
    /// </summary>
    /// <remarks>
    /// The <see cref="RemoteApiDocUrlResolver"/> class serves as the foundation for linking to external documentation resources.
    /// It provides namespace pattern matching capabilities that allow selective URL resolution based on namespace hierarchies. This
    /// enables documentation systems to integrate with multiple external documentation sources, each responsible for different parts
    /// of the API surface.
    /// <para>
    /// Derived classes implement specific URL resolution strategies while inheriting the namespace filtering mechanism, creating a
    /// flexible and extensible external documentation linking system.
    /// </para>
    /// </remarks>
    public abstract class RemoteApiDocUrlResolver : IRemoteApiDocUrlResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteApiDocUrlResolver"/> class.
        /// </summary>
        /// <param name="url">The base URL of the documentation site.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> is not an absolute URI.</exception>
        public RemoteApiDocUrlResolver(Uri url)
        {
            if (url is null)
                throw new ArgumentNullException(nameof(url));

            if (!url.IsAbsoluteUri)
                throw new ArgumentException("The URL must be an absolute URI.", nameof(url));

            SiteUrl = url;
        }

        /// <inheritdoc/>
        public Uri SiteUrl { get; }

        /// <summary>
        /// Gets the namespace patterns that determine which namespaces are covered by this URL resolver.
        /// </summary>
        /// <value>
        /// A collection of namespace patterns that determine which namespaces are covered by this URL resolver.
        /// </value>
        public PatternCollection NamespacePatterns { get; } = new(Type.Delimiter);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SupportsNamespace(string ns) => !string.IsNullOrWhiteSpace(ns) && NamespacePatterns.Matches(ns);

        /// <summary>
        /// Determines whether the specified member is covered by the namespace patterns of this URL resolver.
        /// </summary>
        /// <param name="member">The member to check.</param>
        /// <returns><see langword="true"/> if the member is covered by the namespace patterns; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SupportsMember(IMember member) => member is not null ? SupportsNamespace(member.Namespace) : false;

        /// <summary>
        /// Attempts to retrieve the documentation URL for the specified member.
        /// </summary>
        /// <param name="member">The member to retrieve the documentation URL for.</param>
        /// <param name="url">
        /// When this method returns, contains the absolute documentation URL for the specified member, if the member
        /// has a documentation URL; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true"/> if the member has a documentation URL; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method first checks if the specified member is covered by the namespace patterns of this URL resolver.
        /// If it is, it attempts to resolve the documentation URL for the member; otherwise, it returns
        /// <see langword="false"/>.
        /// </remarks>
        public bool TryGetMemberUrl(IMember member, [NotNullWhen(true)] out Uri? url)
        {
            if (SupportsMember(member) && ResolveMemberUrl(member) is Uri resolvedUrl)
            {
                url = resolvedUrl.IsAbsoluteUri ? resolvedUrl : new Uri(SiteUrl, resolvedUrl);
                return true;
            }

            url = null;
            return false;
        }

        /// <summary>
        /// Attempts to retrieve the documentation URL for the specified namespace.
        /// </summary>
        /// <param name="ns">The namespace to retrieve the documentation URL for.</param>
        /// <param name="url">
        /// When this method returns, contains the absolute documentation URL for the specified namespace, if the namespace
        /// has a documentation URL; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true"/> if the namespace has a documentation URL; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method first checks if the specified namespace is covered by the namespace patterns of this URL
        /// resolver. If it is, it attempts to resolve the documentation URL for the namespace; otherwise, it returns
        /// <see langword="false"/>.
        /// </remarks>
        public bool TryGetNamespaceUrl(string ns, [NotNullWhen(true)] out Uri? url)
        {
            if (SupportsNamespace(ns) && ResolveNamespaceUrl(ns) is Uri resolvedUrl)
            {
                url = resolvedUrl.IsAbsoluteUri ? resolvedUrl : new Uri(SiteUrl, resolvedUrl);
                return true;
            }

            url = null;
            return false;
        }

        /// <summary>
        /// Attempts to resolve the documentation URL for the specified member.
        /// </summary>
        /// <param name="member">The member to resolve the documentation URL for.</param>
        /// <returns>The documentation URL for the specified member, if the member has a documentation URL; otherwise, <see langword="null"/>.</returns>
        /// <remarks>
        /// This method resolves the documentation URL for the specified member without checking if the member is covered by
        /// the namespace patterns of this URL resolver.
        /// </remarks>
        protected abstract Uri? ResolveMemberUrl(IMember member);

        /// <summary>
        /// Attempts to resolve the documentation URL for the specified namespace.
        /// </summary>
        /// <param name="ns">The namespace to resolve the documentation URL for.</param>
        /// <returns>The documentation URL for the specified namespace, if the namespace has a documentation URL; otherwise, <see langword="null"/>.</returns>
        /// <remarks>
        /// This method resolves the documentation URL for the specified namespace without checking if the member is covered by
        /// the namespace patterns of this URL resolver.
        /// </remarks>
        protected abstract Uri? ResolveNamespaceUrl(string ns);
    }
}

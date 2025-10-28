// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides URLs and file paths to the documentation of code references, types, and type members.
    /// </summary>
    /// <remarks>
    /// This class is responsible for providing the documentation URLs and file paths for code references, types,
    /// and members based on the given documentation strategy. It checks if the code element is part of the documented
    /// assemblies and retrieves the relative URL or file path accordingly. If the element is not part of the assemblies,
    /// it attempts to resolve the URL using the configured external documentation URL resolvers.
    /// </remarks>
    public class DocumentAddressProvider : IDocumentAddressProvider
    {
        private readonly IDocumentUrlContextProvider urlContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentAddressProvider"/> class with the specified addressing strategy.
        /// </summary>
        /// <param name="strategy">The strategy that determines how generated documentation pages should be addressed.</param>
        /// <param name="baseUrl">The base URL of the documentation site, or <see langword="null"/> for relative URLs.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="baseUrl"/> is not <see langword="null"/> and is not an absolute URL.</exception>
        public DocumentAddressProvider(IDocumentAddressingStrategy strategy, Uri? baseUrl = null)
        {
            if (strategy is null)
                throw new ArgumentNullException(nameof(strategy));

            Strategy = strategy;
            urlContext = baseUrl is null
                ? new ContextAwareUrlNormalizer()
                : new RelativeToAbsoluteUrlNormalizer(baseUrl);
        }

        /// <summary>
        /// Gets the strategy that determines how generated documentation pages should be addressed.
        /// </summary>
        /// <value>
        /// The strategy that determines how generated documentation pages should be addressed.
        /// </value>
        protected IDocumentAddressingStrategy Strategy { get; }

        /// <summary>
        /// Gets the page granularity used for organizing documentation content.
        /// </summary>
        /// <value>
        /// The granularity that specifies which code elements (namespaces, types, members) receive their own dedicated pages
        /// in the documentation system.
        /// </value>

        public PageGranularity Granularity => Strategy.Granularity;

        /// <summary>
        /// Gets the collection of resolvers for finding documentation URLs of code elements in external documentation sources.
        /// </summary>
        /// <value>
        /// A list of <see cref="IRemoteApiDocUrlResolver"/> instances that provide documentation URLs for code elements that are not
        /// part of the assemblies being documented.
        /// </value>
        /// <seealso cref="MicrosoftDocs"/>
        /// <seealso cref="SearchBasedApiDocUrlResolver"/>
        /// <seealso cref="StrategyBasedApiDocUrlResolver"/>
        public List<IRemoteApiDocUrlResolver> ExternalReferences { get; } = [];

        /// <inheritdoc/>
        public DocumentUrlContext ActiveScope => urlContext.ActiveScope;

        /// <inheritdoc/>
        public virtual DocumentUrlContext BeginScope(string directory, IDocumentModel? model) => urlContext.BeginScope(directory, model);

        /// <inheritdoc/>
        public virtual bool TryGetNamespaceUrl(string ns, [NotNullWhen(true)] out Uri? url)
        {
            if (!string.IsNullOrWhiteSpace(ns))
            {
                var externalProvider = FindExternalProviderForNamespace(ns);
                if (externalProvider is not null)
                    return externalProvider.TryGetNamespaceUrl(ns, out url);

                if (Strategy.TryResolveNamespaceAddress(ns, out var address))
                {
                    url = ToDocumentUrl(address.RelativeUrl);
                    return true;
                }
            }

            url = null;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool TryGetMemberUrl(IMember member, [NotNullWhen(true)] out Uri? url)
        {
            if (member is not null)
            {
                var externalProvider = FindExternalProviderForMember(member);
                if (externalProvider is not null)
                    return externalProvider.TryGetMemberUrl(member, out url);

                if (Strategy.TryResolveMemberAddress(member, out var address))
                {
                    url = ToDocumentUrl(address.RelativeUrl);
                    return true;
                }
            }

            url = null;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool TryGetTopicUrl(ITopic topic, [NotNullWhen(true)] out Uri? url)
        {
            if (topic is not null && Strategy.TryResolveTopicAddress(topic, out var address))
            {
                url = ToDocumentUrl(address.RelativeUrl);
                return true;
            }

            url = null;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool TryGetNamespaceFile(string ns, [NotNullWhen(true)] out string? relativePath)
        {
            if
            (
                !string.IsNullOrWhiteSpace(ns) &&
                FindExternalProviderForNamespace(ns) is null &&
                Strategy.TryResolveNamespaceAddress(ns, out var address)
            )
            {
                relativePath = address.RelativeFilePath;
                return !string.IsNullOrEmpty(relativePath);
            }

            relativePath = null;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool TryGetMemberFile(IMember member, [NotNullWhen(true)] out string? relativePath)
        {
            if
            (
                member is not null &&
                FindExternalProviderForMember(member) is null &&
                Strategy.TryResolveMemberAddress(member, out var address)
            )
            {
                relativePath = address.RelativeFilePath;
                return !string.IsNullOrEmpty(relativePath);
            }

            relativePath = null;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool TryGetTopicFile(ITopic topic, [NotNullWhen(true)] out string? relativePath)
        {
            if (topic is not null && Strategy.TryResolveTopicAddress(topic, out var address))
            {
                relativePath = address.RelativeFilePath;
                return !string.IsNullOrEmpty(relativePath);
            }

            relativePath = null;
            return false;
        }

        /// <summary>
        /// Finds an external documentation URL resolver that supports the specified namespace.
        /// </summary>
        /// <param name="ns">The namespace to find a supporting external provider for.</param>
        /// <returns>An <see cref="IRemoteApiDocUrlResolver"/> that supports the specified namespace, or <see langword="null"/> if no provider is found.</returns>
        protected virtual IRemoteApiDocUrlResolver? FindExternalProviderForNamespace(string ns)
        {
            if (string.IsNullOrWhiteSpace(ns))
                return null;

            foreach (var provider in ExternalReferences)
            {
                if (provider.SupportsNamespace(ns))
                    return provider;
            }

            return null;
        }

        /// <summary>
        /// Finds an external documentation URL resolver that supports the specified member.
        /// </summary>
        /// <param name="member">The member to find a supporting external provider for.</param>
        /// <returns>An <see cref="IRemoteApiDocUrlResolver"/> that supports the specified member, or <see langword="null"/> if no provider is found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IRemoteApiDocUrlResolver? FindExternalProviderForMember(IMember member) => member is not null ? FindExternalProviderForNamespace(member.Namespace) : null;

        /// <summary>
        /// Creates an absolute or document-relative URL from a site-relative URL string.
        /// </summary>
        /// <param name="siteRelativeUrl">The site-relative URL string to create an absolute or document-relative URL from.</param>
        /// <returns>A URL that correctly navigates from the current document's location to the target.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Uri ToDocumentUrl(string siteRelativeUrl)
        {
            if (!ActiveScope.TryTransformSiteRelativeUrl(siteRelativeUrl, out var documentUrl))
                documentUrl = siteRelativeUrl;

            return new RawUri(documentUrl, UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DocumentAddressProvider"/> class with the specified addressing strategy.
        /// </summary>
        /// <typeparam name="TStrategy">The type of the addressing strategy to use.</typeparam>
        /// <param name="baseUrl">The base URL of the documentation site, or <see langword="null"/> for relative URLs.</param>
        /// <returns>A new instance of the <see cref="DocumentAddressProvider"/> class.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="baseUrl"/> is not <see langword="null"/> and is not an absolute URL.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DocumentAddressProvider Create<TStrategy>(Uri? baseUrl = null)
            where TStrategy : IDocumentAddressingStrategy, new() => new(new TStrategy(), baseUrl);
    }
}

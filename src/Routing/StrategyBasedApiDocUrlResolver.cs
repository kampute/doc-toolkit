// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Support;
    using System;

    /// <summary>
    /// Resolves URLs to external documentation sites for code elements using a structured addressing strategy.
    /// </summary>
    /// <remarks>
    /// The <see cref="StrategyBasedApiDocUrlResolver"/> class uses an <see cref="IDocumentAddressingStrategy"/> implementation to generate structured,
    /// predictable URLs for external API documentation. This class is ideal for linking to well-organized documentation sites with consistent URL
    /// patterns, such as:
    /// <list type="bullet">
    ///   <item><description>Microsoft Docs (.NET API documentation)</description></item>
    ///   <item><description>DocFx-generated documentation</description></item>
    ///   <item><description>Azure DevOps Wiki documentation</description></item>
    ///   <item><description>Other structured documentation systems with predictable URL patterns</description></item>
    /// </list>
    /// To control which external APIs this resolver handles, add namespace patterns to the <see cref="RemoteApiDocUrlResolver.NamespacePatterns"/>
    /// collection, using exact matches (e.g., "Contoso.Services"), wildcard patterns (e.g., "Contoso.*"), or the universal match pattern "*".
    /// </remarks>
    /// <seealso cref="MicrosoftDocs"/>
    /// <seealso cref="SearchBasedApiDocUrlResolver"/>
    /// <seealso cref="IDocumentAddressingStrategy"/>
    public class StrategyBasedApiDocUrlResolver : RemoteApiDocUrlResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StrategyBasedApiDocUrlResolver"/> class.
        /// </summary>
        /// <param name="url">The base URL of the documentation site.</param>
        /// <param name="strategy">The strategy for resolving URLs for documentation content.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> or <paramref name="strategy"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="url"/> is not an absolute URI.</exception>
        public StrategyBasedApiDocUrlResolver(Uri url, IDocumentAddressingStrategy strategy)
            : base(url)
        {
            Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        /// <summary>
        /// Gets the strategy for resolving URLs for documentation content.
        /// </summary>
        /// <value>
        /// An implementation of the <see cref="IDocumentAddressingStrategy"/> interface that defines how documentation addresses are resolved.
        /// </value>
        protected IDocumentAddressingStrategy Strategy { get; }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> is <see langword="null"/>.</exception>
        protected override Uri? ResolveMemberUrl(IMember member)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            if (Strategy.TryResolveMemberAddress(member, out var address))
                return SiteUrl.Combine(address.RelativeUrl);

            return null;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ns"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
        protected override Uri? ResolveNamespaceUrl(string ns)
        {
            if (string.IsNullOrWhiteSpace(ns))
                throw new ArgumentException("Namespace cannot be null or whitespace.", nameof(ns));

            if (Strategy.TryResolveNamespaceAddress(ns, out var address))
                return SiteUrl.Combine(address.RelativeUrl);

            return null;
        }
    }
}

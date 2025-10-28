// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Support;
    using System;

    /// <summary>
    /// Resolves URLs to search engines or documentation repositories for code elements that lack structured documentation.
    /// </summary>
    /// <remarks>
    /// The <see cref="SearchBasedApiDocUrlResolver"/> class generates search query URLs for code elements that don't have formal
    /// documentation or when the documentation location is unknown. It's particularly useful for:
    /// <list type="bullet">
    ///   <item><description>Third-party libraries without published documentation</description></item>
    ///   <item><description>Legacy or uncommon APIs with limited documentation</description></item>
    ///   <item><description>Custom or internal APIs where documentation might exist but in fragmented locations</description></item>
    /// </list>
    /// This resolver constructs URLs with search query parameters, where the parameter name is specified in the constructor (defaults to "q").
    /// The search term is the fully-qualified name of the code element, which can be optionally formatted using a specified programming language.
    /// <para>
    /// To control which APIs this resolver handles, add namespace patterns to the <see cref="RemoteApiDocUrlResolver.NamespacePatterns"/>
    /// collection, using exact matches (e.g., "Contoso.Services"), wildcard patterns (e.g., "Contoso.*"), or the universal match pattern "*".
    /// </para>
    /// This resolver deliberately does not attempt to resolve URLs for explicitly implemented interface members, constructed generic types,
    /// and types with modifiers. This is because such cases are better understood by breaking them down into their component parts.
    /// For example, an implementation like <c>ICollection&lt;DateTime&gt;.Count</c> is better resolved through separate links to:
    /// <list type="number">
    ///   <item><description>The <c>ICollection&lt;T&gt;</c> interface documentation</description></item>
    ///   <item><description>The <c>DateTime</c> type documentation</description></item>
    ///   <item><description>The <c>Count</c> property documentation on the interface</description></item>
    /// </list>
    /// This approach provides a more robust learning path for developers trying to understand these implementation relationships.
    /// </remarks>
    /// <seealso cref="StrategyBasedApiDocUrlResolver"/>
    public class SearchBasedApiDocUrlResolver : RemoteApiDocUrlResolver, ILanguageSpecific
    {
        private IProgrammingLanguage language = Languages.Language.Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchBasedApiDocUrlResolver"/> class with the specified search query parameter name.
        /// </summary>
        /// <param name="url">The base URL of the search engine or documentation repository.</param>
        /// <param name="paramName">The name of the search query parameter. Defaults to "q" if not specified.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="paramName"/> is <see langword="null"/> or empty.</exception>
        public SearchBasedApiDocUrlResolver(Uri url, string paramName = "q")
            : base(url)
        {
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentException($"'{nameof(paramName)}' cannot be null or empty.", nameof(paramName));

            ParamName = paramName;
        }

        /// <summary>
        /// Gets the name of the search query parameter used in constructing URLs.
        /// </summary>
        /// <value>
        /// The name of the search query parameter used in constructing URLs.
        /// </value>
        protected string ParamName { get; }

        /// <summary>
        /// Gets or sets the programming language used for formatting the search term.
        /// </summary>
        /// <value>
        /// The programming language used for formatting the search term.
        /// </value>
        public IProgrammingLanguage Language
        {
            get => language;
            set => language = value ?? Languages.Language.Default;
        }

        /// <inheritdoc/>
        protected override Uri? ResolveMemberUrl(IMember member) => member is not null
            ? BuildSearchUrl(Language.FormatSignature(member, NameQualifier.Full))
            : null;

        /// <inheritdoc/>
        protected override Uri? ResolveNamespaceUrl(string ns) => !string.IsNullOrWhiteSpace(ns)
            ? BuildSearchUrl(ns)
            : null;

        /// <summary>
        /// Creates a search URL for the specified term.
        /// </summary>
        /// <param name="languageTerm">The language term to build the search URL for.</param>
        /// <returns>A search URL for the specified language term.</returns>
        protected virtual Uri BuildSearchUrl(string languageTerm) => SiteUrl.WithQueryParameter(ParamName, $"{Language.Name} {languageTerm}");
    }
}
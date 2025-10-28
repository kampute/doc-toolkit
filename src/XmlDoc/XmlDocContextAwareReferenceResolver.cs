// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using System;

    /// <summary>
    /// Resolves XML documentation references using the current documentation context.
    /// </summary>
    /// <remarks>
    /// The <see cref="XmlDocContextAwareReferenceResolver"/> provides a context-aware implementation of the <see cref="IXmlDocReferenceResolver"/>
    /// interface. It uses information from a documentation context to resolve references to types, members, and language keywords within XML
    /// documentation comments.
    /// </remarks>
    /// <seealso cref="XmlDocTransformer"/>
    /// <seealso cref="IXmlDocReferenceResolver"/>
    /// <seealso cref="IDocumentationContext"/>
    public class XmlDocContextAwareReferenceResolver : IXmlDocReferenceResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocContextAwareReferenceResolver"/> class.
        /// </summary>
        /// <param name="context">The documentation context to use for resolving references.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
        public XmlDocContextAwareReferenceResolver(IDocumentationContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets the documentation context used for resolving references.
        /// </summary>
        /// <value>
        /// The documentation context used for resolving references.
        /// </value>
        public IDocumentationContext Context { get; }

        /// <summary>
        /// Formats the specified code block for display in a documentation page.
        /// </summary>
        /// <param name="code">The code block to format.</param>
        /// <returns>The normalized code block.</returns>
        /// <seealso cref="TextUtility.NormalizeCodeBlock(string, int)"/>
        public string FormatCode(string code) => TextUtility.NormalizeCodeBlock(code);

        /// <summary>
        /// Retrieves the identifier for the programming language of the specified code block.
        /// </summary>
        /// <param name="code">The code block for which to determine the programming language identifier.</param>
        /// <returns>The identifier of the programming language used for signature highlighting.</returns>
        public string GetLanguageId(string code) => Context.Language.Identifier;

        /// <summary>
        /// Resolves the official documentation URL for the specified language keyword based on the current language context.
        /// </summary>
        /// <param name="keyword">The language-specific keyword.</param>
        /// <returns>The URL string of the official documentation for the specified keyword, or an empty string if it cannot be resolved.</returns>
        public string GetKeywordUrl(string keyword) => Context.Language.TryGetUrl(keyword.Trim(), out var url) ? url.ToString() : string.Empty;

        /// <summary>
        /// Resolves the documentation URL for the specified code reference.
        /// </summary>
        /// <param name="cref">The code reference.</param>
        /// <returns>The URL string of the documentation for the specified code reference, or an empty string if it cannot be resolved.</returns>
        public string GetCodeReferenceUrl(string cref) => Context.AddressProvider.TryGetUrlByCodeReference(cref.Trim(), out var url) ? url.ToString() : string.Empty;

        /// <summary>
        /// Retrieves the title for a code reference based on the current language context.
        /// </summary>
        /// <param name="cref">The code reference.</param>
        /// <returns>The title for the specified code reference, or the original reference if it cannot be resolved.</returns>
        public string GetCodeReferenceTitle(string cref) => Context.Language.FormatCodeReference(cref.Trim(), Context.DetermineNameQualifier);

        /// <summary>
        /// Gets the URL of the documentation for a topic reference.
        /// </summary>
        /// <param name="href">The topic reference</param>
        /// <returns>The URL string of the documentation for the specified topic reference, or the original reference if it cannot be resolved.</returns>
        public string GetTopicUrl(string href) => Context.UrlTransformer.TryTransformUrl(href.Trim(), out var url) ? url.ToString() : href;

        /// <summary>
        /// Gets the title for a topic reference.
        /// </summary>
        /// <param name="href">The topic reference.</param>
        /// <returns>The title for the specified topic reference, or an empty string if it cannot be resolved.</returns>
        public string GetTopicTitle(string href) => Context.Topics.TryResolve(UriHelper.GetPathPart(href.Trim()), out var topic) ? topic.Name : string.Empty;
    }
}

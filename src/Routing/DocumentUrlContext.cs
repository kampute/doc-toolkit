// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a disposable context that manages relative URL resolution within a document context.
    /// </summary>
    /// <remarks>
    /// This abstract class represents a temporary context for URL adjustment that's specific to the document currently being
    /// rendered. It provides access to the base URL used for normalizing relative URLs and ensures the context is properly
    /// terminated when document processing is complete.
    /// <para>
    /// The concrete implementation of this class is responsible for managing the state of the URL adjustment context and
    /// ensuring that the base URL is correctly set based on the current document's location within the documentation site.
    /// </para>
    /// When the context is disposed, any state modifications related to the current document context should be reverted,
    /// restoring the previous URL adjustment context.
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    /// <seealso cref="IDocumentUrlContextProvider"/>
    public abstract class DocumentUrlContext : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentUrlContext"/> class.
        /// </summary>
        /// <param name="directory">The directory path of the document being rendered relative to the documentation root.</param>
        /// <param name="model">The document model associated with the current context or <see langword="null"/> if not applicable.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="directory"/> is <see langword="null"/>.</exception>
        protected DocumentUrlContext(string directory, IDocumentModel? model)
        {
            Directory = directory ?? throw new ArgumentNullException(nameof(directory));
            Model = model;
        }

        /// <summary>
        /// Gets a value indicating whether the current document is at the documentation root.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the current document is at the documentation root; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsDocumentationRoot => Directory.Length == 0;

        /// <summary>
        /// Gets the absolute or document-relative URL to the documentation root for the current context.
        /// </summary>
        /// <value>
        /// The URL that identifies the documentation root from the current document.
        /// </value>
        /// <remarks>
        /// The documentation root may differ from the web site's root, such as when documentation is published below a repository
        /// path. A relative value describes navigation from the current document to the documentation root; an absolute value
        /// identifies the published documentation root directly.
        /// <para>
        /// When the root URL is a relative URL, it represents the path from the current document to the root of the documentation
        /// site. The following examples illustrate how a relative root URL is computed based on document location:
        /// <list type="bullet">
        ///   <item><description>For a document at the root level: <c>""</c></description></item>
        ///   <item><description>For a document in a first-level directory: <c>"../"</c></description></item>
        ///   <item><description>For a document in a second-level directory: <c>"../../"</c></description></item>
        /// </list>
        /// </para>
        /// </remarks>
        public abstract Uri DocumentationRootUrl { get; }

        /// <summary>
        /// Gets the current document directory path relative to the documentation root.
        /// </summary>
        /// <value>
        /// The relative directory path of the document being rendered within the documentation structure.
        /// </value>
        /// <remarks>
        /// This property provides access to the current document's directory path relative to the documentation root.
        /// It can be useful for determining the document's location within the hierarchy or for constructing relative
        /// paths to other documents or resources.
        /// </remarks>
        public string Directory { get; }

        /// <summary>
        /// Gets the document model associated with the current context.
        /// </summary>
        /// <value>
        /// The <see cref="IDocumentModel"/> representing the document being processed in this context, or <see langword="null"/> if not applicable.
        /// </value>
        public IDocumentModel? Model { get; }

        /// <summary>
        /// Resolves a documentation-root-relative URL into an absolute or document-relative URL.
        /// </summary>
        /// <param name="urlString">
        /// A URL string relative to the documentation root (without the <c>~/</c> marker). The URL consists of a normalized path
        /// component and may optionally include a query string and/or fragment.
        /// </param>
        /// <returns>An absolute or document-relative URL string that correctly navigates from the current document's location to the target.</returns>
        /// <remarks>
        /// This method performs the core URL resolution logic based on the current document context. The input URL must be relative
        /// to the documentation root with its path component already normalized (no <c>..</c> segments that escape the root).
        /// <para>
        /// Query strings and fragments in the URL are preserved in the resolved result.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="urlString"/> is <see langword="null"/>.</exception>
        public abstract string ResolveFromDocumentationRoot(string urlString);

        /// <summary>
        /// Attempts to resolve a documentation-root-relative URL into an absolute or document-relative URL.
        /// </summary>
        /// <param name="url">
        /// A URL beginning with <c>~/</c>, where the marker represents the documentation root rather than the web site's root.
        /// </param>
        /// <param name="resolvedUrl">When this method returns, contains the resolved URL if resolution succeeded; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the URL was successfully resolved; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// <para>URLs are interpreted according to their prefix and the active document context:</para>
        /// <list type="bullet">
        /// <item><description>A documentation-root-relative URL beginning with <c>~/</c> is resolved from the documentation root.</description></item>
        /// <item><description>A site-root-relative URL beginning with <c>/</c> is not resolved.</description></item>
        /// <item><description>An ordinary document-relative URL is not resolved.</description></item>
        /// <item><description>Query strings and fragments are preserved.</description></item>
        /// <item><description>Dot segments are resolved within the documentation root; a path that navigates above that root is not resolved.</description></item>
        /// </list>
        /// The operation preserves the query string and fragment components of the URL, if present.
        /// </remarks>
        public bool TryResolveUrl(string url, [NotNullWhen(true)] out string? resolvedUrl)
        {
            if (url is null || !url.StartsWith("~/", StringComparison.Ordinal))
            {
                resolvedUrl = null;
                return false;
            }

            var (path, suffix) = UriHelper.SplitPathAndSuffix(url[2..]);
            if (UriHelper.IsAbsoluteOrRooted(path) || !PathHelper.TryNormalizePath(path, out var normalizedPath) || PathHelper.StartsWithDotSegment(normalizedPath))
            {
                resolvedUrl = null;
                return false;
            }

            resolvedUrl = ResolveFromDocumentationRoot(normalizedPath + suffix);
            return true;
        }

        /// <summary>
        /// Disposes the current instance and restores the previous URL context.
        /// </summary>
        /// <remarks>
        /// Disposing a nested context makes its parent context active again. Implementations may allow repeated disposal without
        /// producing additional state changes.
        /// </remarks>
        public abstract void Dispose();
    }
}

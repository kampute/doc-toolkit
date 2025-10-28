// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

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
        /// Gets a value indicating whether the current document is at the root level of the documentation site.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the current document is at the root level; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsRoot => Directory.Length == 0;

        /// <summary>
        /// Gets the absolute or relative URL to the root of the documentation site for the current context.
        /// </summary>
        /// <value>
        /// The URL that serves as the reference point for resolving relative URLs within the current context.
        /// </value>
        /// <remarks>
        /// The <see cref="RootUrl"/> property serves as a critical reference point for constructing site-wide resource URLs
        /// and cross-document navigation that works consistently regardless of the document's depth in the hierarchy. It is
        /// especially useful for breadcrumb navigation and referencing shared resources like CSS, JavaScript files, and images
        /// from documents at any level in the site hierarchy.
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
        public abstract Uri RootUrl { get; }

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
        /// Attempts to transform a site-relative URL string into an absolute or document-relative URL based on the current context.
        /// </summary>
        /// <param name="siteRelativeUrl">The URL string relative to site root to transform.</param>
        /// <param name="transformedUrl">When this method returns, contains the transformed URL if the transformation succeeded; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the URL was successfully transformed; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method transforms URLs that are specified relative to the site root into URLs that work correctly when referenced
        /// from the current context, regardless of the document's depth in the documentation hierarchy.
        /// </remarks>
        public abstract bool TryTransformSiteRelativeUrl(string siteRelativeUrl, [NotNullWhen(true)] out string? transformedUrl);

        /// <summary>
        /// Disposes the current instance and restores the previous URL context.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Determines whether the specified URL string is a site-relative URL.
        /// </summary>
        /// <param name="urlString">The URL string to check.</param>
        /// <returns><see langword="true"/> if the URL is site-relative; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool IsSiteRelativeUrl(string urlString)
        {
            return !string.IsNullOrEmpty(urlString)
                && !UriHelper.IsQueryOrFragmentOnly(urlString)
                && !UriHelper.IsAbsoluteOrRooted(urlString)
                && !PathHelper.StartsWithDotSegment(urlString);
        }
    }
}
// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Converts documentation-root-relative URLs to absolute URLs using a fixed documentation root URL.
    /// </summary>
    /// <remarks>
    /// This class provides a simple implementation of the URL adjustment infrastructure that converts documentation-root-relative
    /// URLs to absolute URLs by combining them with a configurable documentation root URL.
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public sealed class RelativeToAbsoluteUrlNormalizer : DocumentUrlContextManager
    {
        private readonly string baseUrlString;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeToAbsoluteUrlNormalizer"/> class with the specified base URL.
        /// </summary>
        /// <param name="baseUrl">The base URL to use for creating absolute URLs from relative URLs.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseUrl"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="baseUrl"/> is not an absolute URL.</exception>
        public RelativeToAbsoluteUrlNormalizer(Uri baseUrl)
            : base()
        {
            if (baseUrl is null)
                throw new ArgumentNullException(nameof(baseUrl));
            if (!baseUrl.IsAbsoluteUri)
                throw new ArgumentException($"{nameof(baseUrl)} must be an absolute URL.", nameof(baseUrl));

            BaseUrl = baseUrl;
            baseUrlString = baseUrl.ToString().TrimEnd('/') + '/';
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeToAbsoluteUrlNormalizer"/> class with the specified base URL string.
        /// </summary>
        /// <param name="baseUrlString">The base URL to use for creating absolute URLs from relative URLs.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseUrlString"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="baseUrlString"/> is not an absolute URL.</exception>
        public RelativeToAbsoluteUrlNormalizer(string baseUrlString)
            : base()
        {
            if (baseUrlString is null)
                throw new ArgumentNullException(nameof(baseUrlString));

            this.baseUrlString = baseUrlString.EndsWith('/') ? baseUrlString : baseUrlString + '/';

            if (!Uri.TryCreate(this.baseUrlString, UriKind.Absolute, out var baseUrl))
                throw new ArgumentException($"{nameof(baseUrlString)} must be an absolute URL.", nameof(baseUrlString));

            BaseUrl = baseUrl;
        }

        /// <summary>
        /// Gets the base URL used to create absolute URLs from relative URLs.
        /// </summary>
        /// <value>
        /// The base URL used to create absolute URLs from relative URLs.
        /// </value>
        public Uri BaseUrl { get; }

        /// <summary>
        /// Creates a new scope for the specified directory path and document model.
        /// </summary>
        /// <param name="directory">The relative directory path of the document being rendered within the documentation structure.</param>
        /// <param name="model">The document model being processed, or <see langword="null"/> if not applicable.</param>
        /// <returns>A new <see cref="AbsoluteUrlContext"/> for the specified directory and model.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override UrlContext CreateScope(string directory, IDocumentModel? model) => new AbsoluteUrlContext(this, directory, model);

        /// <summary>
        /// Represents a disposable URL context that converts documentation-root-relative URLs to absolute URLs.
        /// </summary>
        private sealed class AbsoluteUrlContext : UrlContext
        {
            private readonly string documentationRootUrlString;

            /// <summary>
            /// Initializes a new instance of the <see cref="AbsoluteUrlContext"/> class.
            /// </summary>
            /// <param name="owner">The owning normalizer.</param>
            /// <param name="directory">The directory path of the document being rendered relative to the documentation root.</param>
            /// <param name="model">The document model associated with the current context or <see langword="null"/> if not applicable.</param>
            public AbsoluteUrlContext(RelativeToAbsoluteUrlNormalizer owner, string directory, IDocumentModel? model)
                : base(owner, directory, model)
            {
                RootUrl = owner.BaseUrl;
                documentationRootUrlString = owner.baseUrlString;
            }

            /// <summary>
            /// Gets the absolute URL to the root of the documentation site for the current document.
            /// </summary>
            /// <value>
            /// The configured absolute URL of the documentation root, including any path below the web site's root.
            /// </value>
            public override Uri RootUrl { get; }

            /// <summary>
            /// Attempts to transform a documentation-root-relative URL into an absolute URL.
            /// </summary>
            /// <param name="documentationRelativeUrl">The URL beginning with <c>~/</c> to transform.</param>
            /// <param name="transformedUrl">When this method returns, contains the transformed URL if the transformation succeeded; otherwise, <see langword="null"/>.</param>
            /// <returns><see langword="true"/> if the URL was successfully transformed; otherwise, <see langword="false"/>.</returns>
            /// <remarks>Query strings and fragments are preserved, and the active context is not changed.</remarks>
            public override bool TryTransformSiteRelativeUrl(string documentationRelativeUrl, [NotNullWhen(true)] out string? transformedUrl)
            {
                if (!IsSiteRelativeUrl(documentationRelativeUrl, out var relativeUrl))
                {
                    transformedUrl = null;
                    return false;
                }

                transformedUrl = documentationRootUrlString + relativeUrl;
                return true;
            }
        }
    }
}

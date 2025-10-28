// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// Normalizes relative URLs based on the current document context to ensure proper navigation.
    /// </summary>
    /// <remarks>
    /// This class adjusts relative URLs based on the location of the current document being rendered. It ensures that navigation remains
    /// consistent regardless of the document hierarchy by calculating the correct relative path between documents.
    /// <para>
    /// For example, if a document at <c>api/namespace/class.html</c> references another document at <c>api/other-namespace/interface.html</c>,
    /// this normalizer ensures the relative path is correctly calculated as <c>../other-namespace/interface.html</c>. In this case, the base
    /// URL of the documentation site relative to the current document is <c>../../</c>.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    /// <seealso cref="DocumentUrlContext"/>
    public sealed class ContextAwareUrlNormalizer : DocumentUrlContextManager
    {
        private readonly ConcurrentDictionary<string, DirectoryMetadata> directoryCache = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextAwareUrlNormalizer"/> class.
        /// </summary>
        public ContextAwareUrlNormalizer()
            : base()
        {
        }

        /// <summary>
        /// Creates a new scope for the specified directory path and document model.
        /// </summary>
        /// <param name="directory">The relative directory path of the document being rendered within the documentation structure.</param>
        /// <param name="model">The document model being processed, or <see langword="null"/> if not applicable.</param>
        /// <returns>A new <see cref="ContextAwareUrlContext"/> for the specified directory and model.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override UrlContext CreateScope(string directory, IDocumentModel? model)
        {
            var dirData = directoryCache.GetOrAdd(directory, static dir => new DirectoryMetadata(dir));
            return new ContextAwareUrlContext(this, dirData, model);
        }

        /// <summary>
        /// Represents a disposable URL context that converts site-root-relative URLs to document-relative URLs.
        /// </summary>
        private sealed class ContextAwareUrlContext : UrlContext
        {
            private readonly DirectoryMetadata directory;

            /// <summary>
            /// Initializes a new instance of the <see cref="ContextAwareUrlContext"/> class.
            /// </summary>
            /// <param name="owner">The owning normalizer instance.</param>
            /// <param name="directory">The directory metadata for URL resolution.</param>
            /// <param name="model">The document model associated with the current context or <see langword="null"/> if not applicable.</param>
            public ContextAwareUrlContext(ContextAwareUrlNormalizer owner, DirectoryMetadata directory, IDocumentModel? model)
                : base(owner, directory.Path, model)
            {
                this.directory = directory;
            }

            /// <summary>
            /// Gets the relative URL to the root of the documentation site for the current context.
            /// </summary>
            /// <value>
            /// The URL that serves as the reference point for resolving relative URLs within the current context.
            /// </value>
            public override Uri RootUrl => directory.RelativeRootUrl;

            /// <summary>
            /// Attempts to transform a site-relative URL string into a document-relative URL based on the current context.
            /// </summary>
            /// <param name="siteRelativeUrl">The URL string relative to site root to transform.</param>
            /// <param name="transformedUrl">When this method returns, contains the transformed URL if the transformation succeeded; otherwise, <see langword="null"/>.</param>
            /// <returns><see langword="true"/> if the URL was successfully transformed; otherwise, <see langword="false"/>.</returns>
            public override bool TryTransformSiteRelativeUrl(string siteRelativeUrl, [NotNullWhen(true)] out string? transformedUrl)
            {
                if (!IsSiteRelativeUrl(siteRelativeUrl))
                {
                    transformedUrl = null;
                    return false;
                }

                if (directory.Segments.Length == 0)
                {
                    transformedUrl = siteRelativeUrl;
                    return true;
                }

                using var reusable = StringBuilderPool.Shared.GetBuilder();
                var href = reusable.Builder;

                href.EnsureCapacity(directory.RelativeRootPath.Length + siteRelativeUrl.Length);

                var (urlPath, urlSuffix) = UriHelper.SplitPathAndSuffix(siteRelativeUrl);
                var (resourcePath, resourceName) = urlPath.SplitLast('/');

                if (!EqualsIgnoreCase(directory.Path, resourcePath))
                    AppendAdjustedRelativePath(href, resourcePath);

                href.Append(resourceName).Append(urlSuffix);

                transformedUrl = href.ToString();
                return true;
            }

            /// <summary>
            /// Converts a site-root-relative path to a document-relative path based on the current directory and appends
            /// it to the provided <see cref="StringBuilder"/>.
            /// </summary>
            /// <param name="sb">The <see cref="StringBuilder"/> to append the adjusted path to.</param>
            /// <param name="relativePath">The path relative to site root to normalize.</param>
            private void AppendAdjustedRelativePath(StringBuilder sb, string relativePath)
            {
                var targetSegments = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var minLength = Math.Min(directory.Segments.Length, targetSegments.Length);

                var commonPrefixLength = 0;
                while (commonPrefixLength < minLength && EqualsIgnoreCase(directory.Segments[commonPrefixLength], targetSegments[commonPrefixLength]))
                    commonPrefixLength++;

                var upCount = directory.Segments.Length - commonPrefixLength;

                for (var i = 0; i < upCount; ++i)
                    sb.Append("../");

                for (var i = commonPrefixLength; i < targetSegments.Length; i++)
                    sb.Append(targetSegments[i]).Append('/');
            }

            /// <summary>
            /// Compares two strings for equality in a case-insensitive manner.
            /// </summary>
            /// <param name="a">The first string to compare.</param>
            /// <param name="b">The second string to compare.</param>
            /// <returns><see langword="true"/> if the strings are equal; otherwise, <see langword="false"/>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool EqualsIgnoreCase(string? a, string? b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Contains metadata for a directory path used in URL resolution calculations.
        /// </summary>
        private sealed class DirectoryMetadata
        {
            /// <summary>
            /// The original directory path.
            /// </summary>
            public readonly string Path;

            /// <summary>
            /// The directory path split into individual segments.
            /// </summary>
            public readonly string[] Segments;

            /// <summary>
            /// The relative path string to navigate from this directory to the documentation root.
            /// </summary>
            public readonly string RelativeRootPath;

            /// <summary>
            /// The relative URL to navigate from this directory to the documentation root.
            /// </summary>
            public readonly Uri RelativeRootUrl;

            /// <summary>
            /// Initializes a new instance of the <see cref="DirectoryMetadata"/> class.
            /// </summary>
            /// <param name="path">The directory path to analyze and prepare metadata for.</param>
            public DirectoryMetadata(string path)
            {
                Path = path;
                Segments = path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
                RelativeRootPath = Segments.Length > 0
                    ? string.Join(string.Empty, Enumerable.Repeat("../", Segments.Length))
                    : string.Empty;
                RelativeRootUrl = string.IsNullOrEmpty(RelativeRootPath)
                    ? UriHelper.EmptyUri
                    : new RawUri(RelativeRootPath, UriKind.Relative);
            }
        }
    }
}

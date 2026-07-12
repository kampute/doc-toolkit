// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// Manages URL contexts that resolve documentation-root-relative URLs relative to the current document.
    /// </summary>
    /// <remarks>
    /// The manager calculates URLs from each rendered document to resources identified from the documentation root.
    /// <para>
    /// For example, if a document at <c>api/namespace/class.html</c> references another document at <c>api/other-namespace/interface.html</c>,
    /// the managed context produces <c>../other-namespace/interface.html</c>. In this case, the documentation root URL relative
    /// to the current document is <c>../../</c>.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    /// <seealso cref="DocumentUrlContext"/>
    public sealed class RelativeUrlContextManager : DocumentUrlContextManager
    {
        private readonly ConcurrentDictionary<string, DirectoryMetadata> directoryCache = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeUrlContextManager"/> class.
        /// </summary>
        public RelativeUrlContextManager()
            : base()
        {
        }

        /// <summary>
        /// Creates a new scope for the specified directory path and document model.
        /// </summary>
        /// <param name="directory">The relative directory path of the document being rendered within the documentation structure.</param>
        /// <param name="model">The document model being processed, or <see langword="null"/> if not applicable.</param>
        /// <returns>A new <see cref="RelativeUrlContext"/> for the specified directory and model.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override UrlContext CreateScope(string directory, IDocumentModel? model)
        {
            var dirData = directoryCache.GetOrAdd(directory, static dir => new DirectoryMetadata(dir));
            return new RelativeUrlContext(this, dirData, model);
        }

        /// <summary>
        /// Represents a disposable URL context that converts documentation-root-relative URLs to document-relative URLs.
        /// </summary>
        private sealed class RelativeUrlContext : UrlContext
        {
            private readonly DirectoryMetadata directory;

            /// <summary>
            /// Initializes a new instance of the <see cref="RelativeUrlContext"/> class.
            /// </summary>
            /// <param name="owner">The owning context manager.</param>
            /// <param name="directory">The directory metadata for URL resolution.</param>
            /// <param name="model">The document model associated with the current context or <see langword="null"/> if not applicable.</param>
            public RelativeUrlContext(RelativeUrlContextManager owner, DirectoryMetadata directory, IDocumentModel? model)
                : base(owner, directory.Path, model)
            {
                this.directory = directory;
            }

            /// <summary>
            /// Gets the relative URL to the root of the documentation site for the current context.
            /// </summary>
            /// <value>
            /// A relative URL containing the parent segments needed to reach the documentation root, or an empty relative URL
            /// when the current document is already at that root.
            /// </value>
            public override Uri DocumentationRootUrl => directory.RelativeDocumentationRootUrl;

            /// <summary>
            /// Resolves a documentation-root-relative URL into a URL relative to the current document.
            /// </summary>
            /// <param name="urlString">
            /// A URL string relative to the documentation root (without the <c>~/</c> marker). The URL consists of a normalized path
            /// component and may optionally include a query string and/or fragment.
            /// </param>
            /// <returns>A document-relative URL string that navigates from the current document's location to the target.</returns>
            /// <remarks>
            /// The method computes the relative path from the current document's directory to the target resource. If the target is in
            /// the same directory as the current document, only the filename is returned. Query strings and fragments are preserved
            /// in the result.
            /// </remarks>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="urlString"/> is <see langword="null"/>.</exception>
            public override string ResolveFromDocumentationRoot(string urlString)
            {
                if (urlString is null)
                    throw new ArgumentNullException(nameof(urlString));

                if (directory.Segments.Length == 0)
                    return urlString;

                using var reusable = StringBuilderPool.Shared.GetBuilder();
                var href = reusable.Builder;

                href.EnsureCapacity(directory.RelativeRootPath.Length + urlString.Length);

                var (urlPath, urlSuffix) = UriHelper.SplitPathAndSuffix(urlString);
                var (resourcePath, resourceName) = urlPath.SplitLast('/');

                if (!EqualsIgnoreCase(directory.Path, resourcePath))
                    AppendAdjustedRelativePath(href, resourcePath);

                href.Append(resourceName).Append(urlSuffix);

                return href.ToString();
            }

            /// <summary>
            /// Converts a path relative to the documentation root to a document-relative path based on the current directory and appends
            /// it to the provided <see cref="StringBuilder"/>.
            /// </summary>
            /// <param name="sb">The <see cref="StringBuilder"/> to append the adjusted path to.</param>
            /// <param name="relativePath">The path relative to the documentation root to normalize.</param>
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
            public readonly Uri RelativeDocumentationRootUrl;

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
                RelativeDocumentationRootUrl = string.IsNullOrEmpty(RelativeRootPath)
                    ? UriHelper.EmptyUri
                    : new RawUri(RelativeRootPath, UriKind.Relative);
            }
        }
    }
}

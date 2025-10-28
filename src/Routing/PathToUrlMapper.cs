// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;

    /// <summary>
    /// Provides a mapping between file references and their corresponding URLs.
    /// </summary>
    /// <remarks>
    /// The <see cref="PathToUrlMapper"/> class implements a dictionary-based solution for mapping
    /// between file paths and their target URLs in documentation systems. This mapping is essential
    /// when documentation references need to be maintained across different output formats or when
    /// URL structures change.
    /// <para>
    /// This implementation uses case-insensitive key matching and supports partial URL matching,
    /// allowing it to handle URLs with query parameters and fragments. The class can be used
    /// directly for managing URL mappings or as a component in a larger documentation generation
    /// pipeline.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public class PathToUrlMapper : IReadOnlyCollection<KeyValuePair<string, Uri>>, IUrlTransformer
    {
        private readonly Dictionary<string, Uri> mapping = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="PathToUrlMapper"/> class.
        /// </summary>
        public PathToUrlMapper()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the URL mapper has any mappings.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the URL mapper has any mappings; otherwise, <see langword="false"/>.
        /// </value>
        public bool MayTransformUrls => mapping.Count > 0;

        /// <summary>
        /// Gets the number of URL mappings.
        /// </summary>
        /// <value>
        /// The number of URL mappings.
        /// </value>
        public int Count => mapping.Count;

        /// <summary>
        /// Adds a URL mapping for the specified source path.
        /// </summary>
        /// <param name="path">The path to be replaced.</param>
        /// <param name="url">The URL to use as replacement.</param>
        /// <returns><see langword="true"/> if the mapping was successfully added; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> is <see langword="null"/>.</exception>
        public bool Add(string path, Uri? url)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));
            if (url is null)
                throw new ArgumentNullException(nameof(url));

            return mapping.TryAdd(path, url);
        }

        /// <summary>
        /// Removes a URL mapping for the specified source path.
        /// </summary>
        /// <param name="path">The path to remove.</param>
        /// <returns><see langword="true"/> if the mapping was successfully removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/> or empty.</exception>
        public bool Remove(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));

            return mapping.Remove(path);
        }

        /// <summary>
        /// Determines whether the specified path has a URL mapping.
        /// </summary>
        /// <param name="path">The path to check for a mapping.</param>
        /// <returns><see langword="true"/> if the path has a mapping; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is <see langword="null"/> or empty.</exception>
        public bool Contains(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException($"'{nameof(path)}' cannot be null or empty.", nameof(path));

            return mapping.ContainsKey(path);
        }

        /// <summary>
        /// Attempts to transform a URL string to its corresponding replacement URL.
        /// </summary>
        /// <param name="urlString">The URL string to find a replacement for.</param>
        /// <param name="replacementUrl">When this method returns, contains the replacement URL if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a replacement URL was found; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method attempts to find a corresponding replacement URL for the given URL string based on the defined mappings.
        /// It handles several common URL variations to maximize the chance of finding a match:
        /// <list type="bullet">
        ///   <item>It attempts to match both URL-encoded and decoded path variations</item>
        ///   <item>It tries matching paths both with and without file extensions</item>
        /// </list>
        /// When a match is found, the method preserves fragment identifier of the original URL and combines its query parameters
        /// with any existing query parameters in the mapped URL. This ensures that the full context of the original URL is retained
        /// in the replacement URL.
        /// </remarks>
        public bool TryTransformUrl(string urlString, [NotNullWhen(true)] out Uri? replacementUrl)
        {
            if (MayTransformUrls && !string.IsNullOrEmpty(urlString))
            {
                var (urlPath, urlSuffix) = UriHelper.SplitPathAndSuffix(urlString);

                foreach (var path in Variants(urlPath))
                {
                    if (mapping.TryGetValue(path, out var mappedUrl))
                    {
                        replacementUrl = mappedUrl.Combine(urlSuffix);
                        return true;
                    }
                }
            }

            replacementUrl = null;
            return false;

            static IEnumerable<string> Variants(string path)
            {
                yield return path;

                var wasEncoded = false;
                var decodedPath = WebUtility.UrlDecode(path);
                if (decodedPath != path)
                {
                    wasEncoded = true;
                    yield return decodedPath;
                }

                var extensionIndex = path.LastIndexOf('.');
                if (extensionIndex != -1 && extensionIndex > path.LastIndexOf('/'))
                {
                    var pathWithoutExtension = path[..extensionIndex];
                    yield return pathWithoutExtension;

                    if (wasEncoded)
                    {
                        var decodedPathWithoutExtension = WebUtility.UrlDecode(pathWithoutExtension);
                        if (decodedPathWithoutExtension != pathWithoutExtension)
                            yield return decodedPathWithoutExtension;
                    }
                }
            }
        }

        /// <summary>
        /// Clears all URL mappings.
        /// </summary>
        public void Clear() => mapping.Clear();

        /// <summary>
        /// Returns an enumerator that iterates through the collection of URL mappings.
        /// </summary>
        /// <returns>An enumerator that iterates through the path-to-URL mappings.</returns>
        public IEnumerator<KeyValuePair<string, Uri>> GetEnumerator() => mapping.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

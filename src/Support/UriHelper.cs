// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides helper methods for working with URIs.
    /// </summary>
    public static class UriHelper
    {
        static UriHelper()
        {
            InvalidPathSegmentCharacters = [.. new HashSet<char>(Path.GetInvalidPathChars())
            {
                ' ', ':', '?', '&', '+', '%', '#', '=', '@', '/', '\\', '*', '"', '<', '>', '|', '{', '}', '[', ']', '^', '`'
            }];
        }

        /// <summary>
        /// Represents an empty <see cref="Uri"/> instance.
        /// </summary>
        public static readonly Uri EmptyUri = new(string.Empty, UriKind.Relative);

        /// <summary>
        /// Represents the characters that can be used to indicate the end of a path in a URI.
        /// </summary>
        public static readonly char[] PathEndingMarkers = ['?', '#'];

        /// <summary>
        /// Represents that characters that are considered invalid in a URI path segment.
        /// </summary>
        public static readonly char[] InvalidPathSegmentCharacters;

        /// <summary>
        /// Creates a new <see cref="Uri"/> with the specified query parameter added.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> instance to append the query parameter to.</param>
        /// <param name="paramName">The name of the query parameter.</param>
        /// <param name="paramValue">The value of the query parameter, which will be URI-encoded.</param>
        /// <returns>A new <see cref="Uri"/> with the query parameter added.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri"/> or <paramref name="paramValue"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="paramName"/> is <see langword="null"/> or empty.</exception>
        /// <remarks>
        /// This method creates a new <see cref="Uri"/> instance with the specified query parameter added to the original URI.
        /// If the original URI already contains a query string with the same parameter name, rather than replacing it, the method
        /// appends the new parameter to the existing query string.
        /// <note type="caution" title="Caution">
        /// The method URI-encodes the parameter name and value. To avoid double encoding, ensure that the parameter name and value
        /// are not already URI-encoded.
        /// </note>
        /// </remarks>
        public static Uri WithQueryParameter(this Uri uri, string paramName, string paramValue)
        {
            if (uri is null)
                throw new ArgumentNullException(nameof(uri));
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentException($"'{nameof(paramName)}' cannot be empty.", nameof(paramName));
            if (paramValue is null)
                throw new ArgumentNullException(nameof(paramValue));

            var uriString = uri.ToString();
            var fragmentIndex = uriString.IndexOf('#', StringComparison.Ordinal);
            var fragment = fragmentIndex != -1 ? uriString[fragmentIndex..] : string.Empty;
            var uriWithoutFragment = fragmentIndex != -1 ? uriString[..fragmentIndex] : uriString;

            var separator = uriWithoutFragment.Contains('?', StringComparison.Ordinal) ? '&' : '?';
            var queryParam = paramName + '=' + WebUtility.UrlEncode(paramValue);
            var newUriString = uriWithoutFragment + separator + queryParam + fragment;

            return new RawUri(newUriString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
        }

        /// <summary>
        /// Creates a new <see cref="Uri"/> with the specified fragment added.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> instance to append the fragment to.</param>
        /// <param name="fragment">The fragment to append to the URI.</param>
        /// <returns>A new <see cref="Uri"/> with the fragment added.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method creates a new <see cref="Uri"/> instance with the specified fragment. If the original URI already has a fragment,
        /// the new fragment replaces the existing one. If the new fragment is is an empty string, the fragment part is removed.
        /// <note type="tip" title="Tip">
        /// This method is useful for creating anchored links within documentation pages while preserving the original URI structure.
        /// </note>
        /// </remarks>
        public static Uri WithFragment(this Uri uri, string fragment)
        {
            if (uri is null)
                throw new ArgumentNullException(nameof(uri));

            var uriString = uri.ToString();
            var fragmentIndex = uriString.IndexOf('#', StringComparison.Ordinal);
            var uriWithoutFragment = fragmentIndex != -1 ? uriString[..fragmentIndex] : uriString;
            var newUriString = string.IsNullOrEmpty(fragment) ? uriWithoutFragment : uriWithoutFragment + '#' + fragment;

            return new RawUri(newUriString, uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
        }

        /// <summary>
        /// Creates a new <see cref="Uri"/> with the fragment removed.
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> instance to remove the fragment from.</param>
        /// <returns>A new <see cref="Uri"/> with the fragment removed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="uri"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method removes any fragment identifier from the URI. If the URI does not have a fragment, the original URI is returned
        /// unchanged.
        /// </remarks>
        public static Uri WithoutFragment(this Uri uri)
        {
            if (uri is null)
                throw new ArgumentNullException(nameof(uri));

            var uriString = uri.ToString();
            var fragmentIndex = uriString.IndexOf('#', StringComparison.Ordinal);
            return fragmentIndex != -1
                ? new RawUri(uriString[..fragmentIndex], uri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative)
                : uri;
        }

        /// <summary>
        /// Creates a new <see cref="Uri"/> by combining the <see cref="Uri"/> with a relative URI string.
        /// </summary>
        /// <param name="baseUri">The base <see cref="Uri"/> instance.</param>
        /// <param name="relativeUri">The relative URI string to combine with the base URI.</param>
        /// <returns>A new <see cref="Uri"/> that represents the combination of the base <see cref="Uri"/> and the relative URI string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseUri"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="relativeUri"/> attempts to navigate beyond the root directory.</exception>
        /// <remarks>
        /// This method combines the base <see cref="Uri"/> with a relative URI string according to the following rules:
        /// <list type="bullet">
        ///   <item>
        ///     <term>Path</term>
        ///     <description>If the path of <paramref name="relativeUri"/> starts with a forward slash, it replaces the base URI's path; otherwise, it is appended to the base URI's path.</description>
        ///   </item>
        ///   <item>
        ///     <term>Query</term>
        ///     <description>The query string of the resulting URI is a combination of the base URI's query string and the relative URI's query string.</description>
        ///   </item>
        ///   <item>
        ///     <term>Fragment</term>
        ///     <description>If <paramref name="relativeUri"/> contains a fragment, it replaces the base URI's fragment; otherwise, the base URI's fragment is preserved.</description>
        ///   </item>
        /// </list>
        /// There are some differences between URIs created using this method and those created using the <see cref="Uri(Uri, string)"/> constructor:
        /// <list type="table">
        ///   <listheader>
        ///     <term>Aspect</term>
        ///     <description>This Method</description>
        ///     <description><see cref="Uri"/> Constructor</description>
        ///   </listheader>
        ///   <item>
        ///     <term>Path handling</term>
        ///     <description>Appends path segments with a separator if needed. If the relative path starts with '/', it replaces the base path.</description>
        ///     <description>Uses URI resolution rules per RFC 3986, which can result in path segments being removed during normalization.</description>
        ///   </item>
        ///   <item>
        ///     <term>Query parameters</term>
        ///     <description>Preserves and merges query parameters from both URIs.</description>
        ///     <description>Query parameters in the relative URI completely replace those in the base URI.</description>
        ///   </item>
        ///   <item>
        ///     <term>Fragment handling</term>
        ///     <description>Replaces the fragment in the base URI with the fragment in the relative URI.</description>
        ///     <description>Fragment in relative URI always replaces any fragment in the base URI.</description>
        ///   </item>
        ///   <item>
        ///     <term>Relative URI base</term>
        ///     <description>Works with both relative and absolute base URIs.</description>
        ///     <description>Throws an exception when the base URI is relative.</description>
        ///   </item>
        ///   <item>
        ///     <term>Empty relative URI</term>
        ///     <description>Returns the base URI unchanged.</description>
        ///     <description>May still perform normalization on the base URI.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        /// <seealso cref="Uri(Uri, string)"/>
        public static Uri Combine(this Uri baseUri, string relativeUri)
        {
            if (baseUri is null)
                throw new ArgumentNullException(nameof(baseUri));

            if (string.IsNullOrEmpty(relativeUri))
                return baseUri;

            var (basePath, baseQuery, _) = SplitPathQueryAndFragment(baseUri.ToString());
            var (relativePath, relativeQuery, resultFragment) = SplitPathQueryAndFragment(relativeUri);

            string combinedPath;

            if (relativePath.Length == 0)
                combinedPath = basePath;
            else if (basePath.Length == 0 || relativePath.StartsWith('/'))
                combinedPath = baseUri.IsAbsoluteUri ? baseUri.GetLeftPart(UriPartial.Authority) + relativePath : relativePath;
            else
                combinedPath = basePath.EndsWith('/') ? basePath + relativePath : basePath + '/' + relativePath;

            if (!PathHelper.TryNormalizePath(combinedPath, out var resultPath))
                throw new ArgumentException("The resulting path cannot navigate beyond the root directory.", nameof(relativeUri));

            string resultQuery;
            if (relativeQuery.Length == 0)
                resultQuery = baseQuery;
            else if (baseQuery.Length == 0)
                resultQuery = relativeQuery;
            else
                resultQuery = baseQuery + '&' + relativeQuery[1..];

            return new RawUri(resultPath + resultQuery + resultFragment, baseUri.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
        }

        /// <summary>
        /// Parses a query string into a collection of key-value pairs.
        /// </summary>
        /// <param name="queryString">The query string to parse.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of key-value pairs representing the parsed query string.</returns>
        /// <remarks>
        /// This method is implemented by using deferred execution. The immediate return value is an object that stores
        /// all the information that is required to perform the action.
        /// </remarks>
        public static IEnumerable<KeyValuePair<string, string>> ParseQueryString(string queryString)
        {
            if (string.IsNullOrEmpty(queryString) || queryString is "?")
                return [];

            return EnumerateQueryParameters(queryString.StartsWith('?') ? queryString[1..] : queryString);

            static IEnumerable<KeyValuePair<string, string>> EnumerateQueryParameters(string queryString)
            {
                var queryParams = queryString.Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (var param in queryParams)
                {
                    var pair = param.Split('=', 2);
                    var key = pair[0];
                    var value = pair.Length == 2 ? WebUtility.UrlDecode(pair[1]) : string.Empty;
                    yield return KeyValuePair.Create(key, value);
                }
            }
        }

        /// <summary>
        /// Converts a collection of key-value pairs into a query string.
        /// </summary>
        /// <param name="queryParameters">The collection of key-value pairs to convert.</param>
        /// <returns>A string representing the query string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="queryParameters"/> is <see langword="null"/>.</exception>
        public static string ToQueryString(this IEnumerable<KeyValuePair<string, string>> queryParameters)
        {
            if (queryParameters is null)
                throw new ArgumentNullException(nameof(queryParameters));

            using var reusable = StringBuilderPool.Shared.GetBuilder();
            var sb = reusable.Builder;

            foreach (var param in queryParameters)
            {
                if (sb.Length > 0)
                    sb.Append('&');

                sb.Append(WebUtility.UrlEncode(param.Key));
                sb.Append('=');
                sb.Append(WebUtility.UrlEncode(param.Value));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Retrieves the path portion of a URI string, excluding any query string or fragment.
        /// </summary>
        /// <param name="uriString">The URI string to extract the path from.</param>
        /// <returns>A string representing the path portion of the URI.</returns>
        /// <remarks>
        /// This method expects the input to be a valid relative URI string.
        /// If the input does not meet this expectation, the returned value may not be meaningful.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPathPart(string uriString)
        {
            if (string.IsNullOrEmpty(uriString))
                return string.Empty;

            var separatorIndex = uriString.IndexOfAny(PathEndingMarkers);
            return separatorIndex == -1 ? uriString : uriString[..separatorIndex];
        }

        /// <summary>
        /// Splits a URI string into its path and suffix components.
        /// </summary>
        /// <param name="uriString">The URI string to split.</param>
        /// <returns>A tuple where the first item is the path portion of the URI, and the second item is the suffix containing any query string and/or fragment.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="uriString"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method expects the input to be a valid relative URI string.
        /// If the input does not meet this expectation, the returned value may not be meaningful.
        /// </remarks>
        public static (string Path, string Suffix) SplitPathAndSuffix(string uriString)
        {
            if (uriString is null)
                throw new ArgumentNullException(nameof(uriString));

            var separatorIndex = uriString.IndexOfAny(PathEndingMarkers);
            var baseUri = separatorIndex == -1 ? uriString : uriString[..separatorIndex];
            var suffix = separatorIndex == -1 ? string.Empty : uriString[separatorIndex..];

            return (baseUri, suffix);
        }

        /// <summary>
        /// Splits a URI string into its path, query string, and fragment components.
        /// </summary>
        /// <param name="uriString">The URI string to split.</param>
        /// <returns>A tuple containing the path, query string, and fragment components of the URI.</returns>
        /// <remarks>
        /// This method expects the input to be a valid relative URI string.
        /// If the input does not meet this expectation, the returned value may not be meaningful.
        /// </remarks>
        public static (string Path, string QueryString, string Fragment) SplitPathQueryAndFragment(string uriString)
        {
            if (uriString is null)
                throw new ArgumentNullException(nameof(uriString));

            var fragmentIndex = uriString.IndexOf('#', StringComparison.Ordinal);
            var fragment = fragmentIndex != -1 ? uriString[fragmentIndex..] : string.Empty;
            var uriWithoutFragment = fragmentIndex != -1 ? uriString[..fragmentIndex] : uriString;

            var queryIndex = uriWithoutFragment.IndexOf('?', StringComparison.Ordinal);
            var path = queryIndex != -1 ? uriWithoutFragment[..queryIndex] : uriWithoutFragment;
            var queryString = queryIndex != -1 ? uriWithoutFragment[queryIndex..] : string.Empty;

            return (path, queryString, fragment);
        }

        /// <summary>
        /// Determines whether the given URI string is absolute or rooted.
        /// </summary>
        /// <param name="uriString">The URI string to check.</param>
        /// <returns><see langword="true"/> if the URI string is absolute or rooted; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="uriString"/> is <see langword="null"/>.</exception>
        public static bool IsAbsoluteOrRooted(string uriString)
        {
            if (uriString is null)
                throw new ArgumentNullException(nameof(uriString));

            if (uriString.Length == 0)
                return false;

            if (uriString[0] is '/')
                return true;

            var i = uriString.IndexOf('/');
            return i != -1
                ? uriString.LastIndexOf(':', i - 1) != -1
                : uriString.Contains(':');
        }

        /// <summary>
        /// Determines whether the given URI string is a query or fragment-only URI.
        /// </summary>
        /// <param name="uriString">The URI string to check.</param>
        /// <returns><see langword="true"/> if the URI string starts with either a '?' or '#' character; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="uriString"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsQueryOrFragmentOnly(string uriString)
        {
            if (uriString is null)
                throw new ArgumentNullException(nameof(uriString));

            return uriString.Length > 0 && uriString[0] is '?' or '#';
        }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides helper methods for working with file paths.
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Validates the specified path to ensure it is a relative path and does not contain invalid characters.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <param name="caller">The name of the caller member.</param>
        /// <returns>The validated path where backslashes are replaced with forward slashes and trailing slashes are removed.</returns>
        /// <exception cref="ArgumentException">Thrown when the path is not relative.</exception>
        /// <exception cref="ArgumentException">Thrown when the path contains invalid characters.</exception>
        public static string EnsureValidRelativePath(string? path, [CallerMemberName] string caller = "")
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            path = path.Replace('\\', '/').TrimEnd('/');

            if (Path.IsPathRooted(path))
                throw new ArgumentException($"{caller} must be a relative path.", caller);

            if (Path.GetInvalidPathChars().Any(path.Contains))
                throw new ArgumentException($"{caller} contains invalid characters.", caller);

            return path;
        }

        /// <summary>
        /// Attempts to normalize the specified path by resolving relative segments like ".." and ".", and
        /// replacing backslashes with forward slashes.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <param name="normalizedPath">When this method returns, contains the normalized path if the operation was successful; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the path was normalized successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
        public static bool TryNormalizePath(string path, [NotNullWhen(true)] out string? normalizedPath)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));

            if (!HasDotSegment(path))
            {
                normalizedPath = path.Replace('\\', '/');
                return true;
            }

            var segments = path.Split(['/', '\\'], StringSplitOptions.None);
            var normalizedSegments = NormalizePathSegments(segments);

            if (path.StartsWith('/') && normalizedSegments.Contains(".."))
            {
                normalizedPath = null;
                return false;
            }

            normalizedPath = string.Join('/', normalizedSegments);
            return true;
        }

        /// <summary>
        /// Normalizes a collection of path segments by resolving relative segments like ".." and ".".
        /// </summary>
        /// <param name="pathSegments">The collection of path segments to normalize.</param>
        /// <returns>A list of normalized path segments.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pathSegments"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes path segments to resolve:
        /// <list type="bullet">
        ///   <item><description>"." segments, which are removed as they refer to the current directory.</description></item>
        ///   <item><description>".." segments, which navigate to the parent directory by removing the previous segment.</description></item>
        /// </list>
        /// If there are more ".." segments than parent directories available, the extra ".." segments are preserved at the beginning.
        /// </remarks>
        public static List<string> NormalizePathSegments(IEnumerable<string> pathSegments)
        {
            if (pathSegments is null)
                throw new ArgumentNullException(nameof(pathSegments));

            var capacity = 0;
            if (pathSegments is ICollection<string> collection)
            {
                capacity = collection.Count;
                if (capacity == 0)
                    return [];
            }

            var normalizedSegments = new List<string>(capacity);
            foreach (var segment in pathSegments)
            {
                switch (segment)
                {
                    case ".":
                        continue;
                    case ".." when normalizedSegments.Count > 0 && normalizedSegments[^1] is not (".." or ""):
                        normalizedSegments.RemoveAt(normalizedSegments.Count - 1);
                        break;
                    case "" when normalizedSegments.Count > 0 && normalizedSegments[^1].Length is 0:
                        continue;
                    default:
                        normalizedSegments.Add(segment);
                        break;
                }
            }
            return normalizedSegments;
        }

        /// <summary>
        /// Determines if a sub-path is a valid sub-path of a full path.
        /// </summary>
        /// <param name="fullPath">The full path to check against.</param>
        /// <param name="subPath">The sub-path to check.</param>
        /// <returns><see langword="true"/> if the full path matches the sub-path; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fullPath"/> or <paramref name="subPath"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method checks if the <paramref name="subPath"/> is a valid sub-path of the <paramref name="fullPath"/>. The match is case-insensitive.
        /// <note type="caution" title="Caution">
        /// The method assumes that both paths are normalized and use the same directory separator.
        /// </note>
        /// </remarks>
        public static bool IsSubpath(string fullPath, string subPath)
        {
            if (fullPath is null)
                throw new ArgumentNullException(nameof(fullPath));
            if (subPath is null)
                throw new ArgumentNullException(nameof(subPath));

            if (fullPath.Length < subPath.Length)
                return false;

            var i = fullPath.LastIndexOf(subPath, StringComparison.OrdinalIgnoreCase);
            return (i == 0 || (i > 0 && fullPath[i - 1] is '/' or '\\')) && fullPath.Length == i + subPath.Length;
        }

        /// <summary>
        /// Determines if the specified path starts with a dot-segment (i.e., "." or "..").
        /// </summary>
        /// <param name="path">The path to examine.</param>
        /// <returns><see langword="true"/> if the path starts with a dot-segment; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithDotSegment(ReadOnlySpan<char> path)
        {
            if (!path.IsEmpty && path[0] is '.')
            {
                if (path.Length == 1 || path[1] is '/' or '\\')
                    return true;

                if (path.Length > 1 && path[1] is '.')
                    return path.Length == 2 || path[2] is '/' or '\\';
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified path has any dot-segment (i.e., "." or "..").
        /// </summary>
        /// <param name="path">The path to examine.</param>
        /// <returns><see langword="true"/> if the path contains at least one dot-segment; otherwise, <see langword="false"/>.</returns>
        public static bool HasDotSegment(ReadOnlySpan<char> path)
        {
            if (path.IsEmpty)
                return false;

            if (StartsWithDotSegment(path))
                return true;

            var segmentStart = 0;
            for (var i = 0; i <= path.Length; i++)
            {
                if (i == path.Length || path[i] is '/' or '\\')
                {
                    if (StartsWithDotSegment(path[segmentStart..i]))
                        return true;

                    segmentStart = i + 1;
                }
            }
            return false;
        }
    }
}

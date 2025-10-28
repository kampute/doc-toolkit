// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides helper methods for parsing HTML content.
    /// </summary>
    public static class HtmlParsingHelper
    {
        /// <summary>
        /// Attempts to extract the content between opening and closing tags with the specified name.
        /// </summary>
        /// <param name="html">The HTML content to search.</param>
        /// <param name="tagName">The name of the tag to search for.</param>
        /// <param name="content">
        /// When this method returns, contains the content between the tags, if found; otherwise, <see langword="null"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true"/> if the content was successfully extracted; otherwise, <see langword="false"/>.</returns>
        public static bool TryExtractTagContent(string html, string tagName, [NotNullWhen(true)] out string? content)
        {
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(tagName))
            {
                content = null;
                return false;
            }

            var contentStartIndex = FindContentStartIndex($"<{tagName}");
            if (contentStartIndex == -1)
            {
                content = null;
                return false;
            }

            var contentEndIndex = html.IndexOf($"</{tagName}>", contentStartIndex, StringComparison.OrdinalIgnoreCase);
            if (contentEndIndex == -1)
            {
                content = null;
                return false;
            }

            content = html[contentStartIndex..contentEndIndex];
            return true;

            int FindContentStartIndex(string openTag)
            {
                var index = 0;
                while (index < html.Length)
                {
                    index = html.IndexOf(openTag, index, StringComparison.OrdinalIgnoreCase);
                    if (index == -1)
                        return -1;

                    index += openTag.Length;
                    if (index >= html.Length)
                        return -1;

                    var c = html[index];
                    if (c is '>')
                        return index + 1;

                    var found = char.IsWhiteSpace(c);

                    index = html.IndexOf('>', index + 1);
                    if (index == -1)
                        return -1;

                    index++;
                    if (found)
                        return index;
                }
                return -1;
            }
        }
    }
}

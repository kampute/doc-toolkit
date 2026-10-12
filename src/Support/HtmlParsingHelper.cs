// Copyright (C) Kampute
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
        /// <remarks>
        /// The content is taken from the first opening tag up to its matching closing tag, so nested elements with the same tag name
        /// are part of the content. Tag-like text in comments, quoted attributes, and raw text elements is not treated as markup.
        /// </remarks>
        public static bool TryExtractTagContent(string html, string tagName, [NotNullWhen(true)] out string? content)
        {
            content = null;
            if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(tagName))
                return false;

            var index = 0;
            var contentStartIndex = 0;
            var depth = 0;
            string? rawTextTag = null;
            while (TryReadTag(out var name, out var isClosing, out var tagIndex))
            {
                if (!name.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!isClosing)
                {
                    if (depth++ == 0)
                        contentStartIndex = index;
                    continue;
                }

                if (depth > 0 && --depth == 0)
                {
                    content = html[contentStartIndex..tagIndex];
                    return true;
                }
            }

            return false;

            bool TryReadTag(out ReadOnlySpan<char> name, out bool isClosing, out int tagIndex)
            {
                name = default;
                isClosing = false;
                tagIndex = -1;

                while (index < html.Length)
                {
                    tagIndex = rawTextTag is null
                        ? html.IndexOf('<', index)
                        : html.IndexOf("</" + rawTextTag, index, StringComparison.OrdinalIgnoreCase);
                    if (tagIndex == -1)
                        return false;

                    if (rawTextTag is null && html.AsSpan(tagIndex).StartsWith("<!--", StringComparison.Ordinal))
                    {
                        var commentEnd = html.IndexOf("-->", tagIndex + 4, StringComparison.Ordinal);
                        if (commentEnd == -1)
                            return false;

                        index = commentEnd + 3;
                        continue;
                    }

                    index = tagIndex + 1;
                    isClosing = index < html.Length && html[index] is '/';
                    if (isClosing)
                        index++;

                    var nameStart = index;
                    while (index < html.Length && !char.IsWhiteSpace(html[index]) && html[index] is not ('>' or '/'))
                        index++;
                    name = html.AsSpan(nameStart, index - nameStart);

                    var quote = '\0';
                    var foundEnd = false;
                    while (index < html.Length)
                    {
                        var c = html[index++];
                        if (quote != '\0')
                        {
                            if (c == quote)
                                quote = '\0';
                        }
                        else if (c is '"' or '\'')
                        {
                            quote = c;
                        }
                        else if (c is '>')
                        {
                            foundEnd = true;
                            break;
                        }
                    }
                    if (!foundEnd)
                        return false;

                    if (rawTextTag is not null)
                    {
                        if (!name.Equals(rawTextTag, StringComparison.OrdinalIgnoreCase))
                            continue;
                        rawTextTag = null;
                    }
                    else if (!isClosing && IsRawTextElement(name))
                    {
                        rawTextTag = name.ToString();
                    }

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Determines whether an HTML element's text is parsed without recognizing nested tags.
        /// </summary>
        /// <param name="name">The element's tag name.</param>
        /// <returns><see langword="true"/> if only the matching closing tag is recognized; otherwise, <see langword="false"/>.</returns>
        private static bool IsRawTextElement(ReadOnlySpan<char> name)
            => name.Equals("script", StringComparison.OrdinalIgnoreCase)
            || name.Equals("style", StringComparison.OrdinalIgnoreCase)
            || name.Equals("title", StringComparison.OrdinalIgnoreCase)
            || name.Equals("textarea", StringComparison.OrdinalIgnoreCase)
            || name.Equals("xmp", StringComparison.OrdinalIgnoreCase)
            || name.Equals("iframe", StringComparison.OrdinalIgnoreCase)
            || name.Equals("noembed", StringComparison.OrdinalIgnoreCase)
            || name.Equals("noframes", StringComparison.OrdinalIgnoreCase);
    }
}

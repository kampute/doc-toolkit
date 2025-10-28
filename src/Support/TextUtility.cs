// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides utility methods for text processing.
    /// </summary>
    public static class TextUtility
    {
        private const int MaxStackAllocSize = 1024;

        /// <summary>
        /// Normalizes the text of a code block by removing leading and trailing empty lines, collapsing consecutive empty lines, and normalizing indentation.
        /// </summary>
        /// <param name="text">The text to normalize.</param>
        /// <param name="tabSize">The number of spaces to replace each tab with. Default is 4.</param>
        /// <returns>A string with normalized indentation, or an empty string if input is <see langword="null"/> or whitespace.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="bullet">
        ///   <item>
        ///     <description>Returns an empty string if the input is <see langword="null"/> or whitespace.</description>
        ///   </item>
        ///   <item>
        ///     <description>For single-line text, it trims and returns the text as-is.</description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       For multi-line text, it:
        ///       <list type="number">
        ///         <item><description>Converts tabs to spaces and removes trailing whitespace from each line.</description></item>
        ///         <item><description>Determines the minimum indentation across all non-empty lines and removes it from each line.</description></item>
        ///         <item><description>Removes leading and trailing empty lines.</description></item>
        ///         <item><description>Collapses consecutive empty lines into a single empty line.</description></item>
        ///       </list>
        ///     </description>
        ///   </item>
        /// </list>
        /// </remarks>
        public static string NormalizeCodeBlock(string text, int tabSize = 4)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            if (!text.Contains('\n', StringComparison.Ordinal))
                return text.Trim();

            var lines = text.Split('\n');
            NormalizeIndentation(lines, tabSize);

            var start = 0;
            while (start < lines.Length && lines[start].Length == 0)
                ++start;

            var end = lines.Length - 1;
            while (end >= 0 && lines[end].Length == 0)
                --end;

            using var reusable = StringBuilderPool.Shared.GetBuilder();
            var sb = reusable.Builder;

            sb.EnsureCapacity(text.Length + (end - start) * Environment.NewLine.Length);

            var previousWasEmpty = false;
            for (var i = start; i <= end; ++i)
            {
                var line = lines[i];
                var isEmpty = line.Length == 0;
                if (isEmpty && previousWasEmpty)
                    continue;

                if (sb.Length > 0)
                    sb.AppendLine();

                sb.Append(line);
                previousWasEmpty = isEmpty;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Normalizes the indentation of the specified text lines in-place.
        /// </summary>
        /// <param name="lines">The list of text lines to normalize.</param>
        /// <param name="tabSize">The number of spaces to replace each tab with. Default is 4.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="lines"/> array is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method modifies the input array in-place, performing these steps:
        /// <list type="number">
        ///   <item><description>Replaces all tabs with spaces.</description></item>
        ///   <item><description>Removes trailing whitespace from each line.</description></item>
        ///   <item><description>Determines the minimum indentation across all non-empty lines.</description></item>
        ///   <item><description>Removes the minimum indentation from the beginning of each line.</description></item>
        /// </list>
        /// </remarks>
        public static void NormalizeIndentation(IList<string> lines, int tabSize = 4)
        {
            if (lines is null)
                throw new ArgumentNullException(nameof(lines));

            var tab = new string(' ', tabSize);
            for (var i = 0; i < lines.Count; ++i)
                lines[i] = lines[i].TrimEnd().Replace("\t", tab, StringComparison.Ordinal);

            var minIndent = lines
                .Where(static line => line.Length != 0)
                .Select(static line => line.TakeWhile(char.IsWhiteSpace).Count())
                .DefaultIfEmpty()
                .Min();

            for (var i = 0; i < lines.Count; ++i)
                lines[i] = lines[i].Length > minIndent ? lines[i][minIndent..] : string.Empty;
        }

        /// <summary>
        /// Normalizes whitespace characters by replacing sequences of whitespace with a single space.
        /// </summary>
        /// <param name="text">The input text to normalize.</param>
        /// <param name="trim">Indicates whether to trim leading and trailing whitespace. Default is <see langword="false"/>.</param>
        /// <returns>A string with normalized whitespace.</returns>
        public static string NormalizeWhitespace(string text, bool trim = true)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var size = text.Length;
            Span<char> newTextSpan = size <= MaxStackAllocSize ? stackalloc char[size] : new char[size];

            var index = 0;
            var previousWasWhitespace = trim;

            foreach (var c in text.AsSpan())
            {
                if (!char.IsWhiteSpace(c))
                {
                    newTextSpan[index++] = c;
                    previousWasWhitespace = false;
                }
                else if (!previousWasWhitespace)
                {
                    newTextSpan[index++] = ' ';
                    previousWasWhitespace = true;
                }
            }

            if (previousWasWhitespace && trim && index > 0)
                index--;

            return index != size ? newTextSpan[..index].ToString() : text;
        }

        /// <summary>
        /// Enumerates lines of text, splitting the input string into lines based on a specified maximum line width.
        /// </summary>
        /// <param name="text">The text to split into lines.</param>
        /// <param name="maxLineWidth">The maximum width of each line.</param>
        /// <param name="preserveLineBreaks">Indicates whether to preserve existing line breaks in the text.</param>
        /// <returns>An enumerable of strings, each representing a line of text.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLineWidth"/> is less than or equal to zero.</exception>
        /// <remarks>
        /// This method splits the input text into lines based on the specified maximum line width.
        /// If <paramref name="preserveLineBreaks"/> is set to <see langword="true"/>, it will respect existing line breaks in the text.
        /// Otherwise, it will treat the entire text as a single block and wrap it according to the specified width.
        /// <note type="hint" title="Caution">
        /// The method preserves leading and trailing whitespace in the lines. This behavior is intentional to maintain the original formatting
        /// of the text. If this behavior is not desired, consider trimming of each line after splitting.
        /// </note>
        /// <para>
        /// This method is implemented by using deferred execution. The immediate return value is an object that stores all the information
        /// that is required to perform the action.
        /// </para>
        /// </remarks>
        public static IEnumerable<string> SplitLines(string text, int maxLineWidth, bool preserveLineBreaks = false)
        {
            if (maxLineWidth <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLineWidth), "Width must be greater than zero.");

            return string.IsNullOrWhiteSpace(text) ? [] : EnumerateWrappedLines(text, maxLineWidth, preserveLineBreaks);

            static IEnumerable<string> EnumerateWrappedLines(string text, int maxLineWidth, bool preserveLineBreaks)
            {
                if (preserveLineBreaks)
                {
                    foreach (var line in text.Split('\n'))
                    {
                        foreach (var wrappedLine in WrapLine(line.Trim('\r'), maxLineWidth))
                            yield return wrappedLine;
                    }
                }
                else
                {
                    foreach (var wrappedLine in WrapLine(text, maxLineWidth))
                        yield return wrappedLine;
                }

                static IEnumerable<string> WrapLine(string text, int maxLineWidth)
                {
                    var length = text.Length;

                    if (length <= maxLineWidth)
                    {
                        yield return text;
                        yield break;
                    }

                    var start = 0;
                    while (start < length)
                    {
                        var end = start + maxLineWidth;

                        if (end >= length)
                        {
                            yield return text[start..];
                            break;
                        }

                        var breakPoint = -1;
                        for (var i = end; i > start; i--)
                        {
                            if (char.IsWhiteSpace(text[i]))
                            {
                                breakPoint = i;
                                break;
                            }
                        }

                        if (breakPoint == -1)
                        {
                            yield return text[start..end];
                            start = end;
                        }
                        else
                        {
                            yield return text[start..breakPoint];
                            start = breakPoint + 1;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates word boundaries in the given text, identifying words separated by non-alphanumeric characters or case changes,
        /// and provides information about whether each word is an acronym.
        /// </summary>
        /// <param name="text">The text to analyze.</param>
        /// <returns>An enumerable of tuples containing the word range and whether the word is an acronym.</returns>
        /// <remarks>
        /// Word boundaries are determined by:
        /// <list type="bullet">
        ///   <item><description>Non-alphanumeric characters (spaces, punctuation, symbols)</description></item>
        ///   <item><description>Case transitions (e.g., PascalCase becomes "Pascal", "Case")</description></item>
        ///   <item><description>Acronym boundaries (e.g., XMLDocument becomes "XML", "Document")</description></item>
        /// </list>
        /// The non-alphanumeric characters are not included in the ranges, only the alphanumeric parts that form words.
        /// Acronyms are identified as sequences of uppercase letters.
        /// <para>
        /// This method is implemented by using deferred execution. The immediate return value is an object that stores all the information
        /// that is required to perform the action.
        /// </para>
        /// </remarks>
        public static IEnumerable<(Range Range, bool IsAcronym)> SplitWords(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? [] : EnumerateWordBoundaries(text);

            static IEnumerable<(Range, bool)> EnumerateWordBoundaries(string text)
            {
                var wordStart = -1;
                var inAcronym = false;
                var currentWordIsAcronym = false;
                var length = text.Length;

                for (var i = 0; i < length; ++i)
                {
                    var c = text[i];

                    if (!char.IsLetterOrDigit(c))
                    {
                        if (wordStart != -1)
                        {
                            yield return (wordStart..i, currentWordIsAcronym);
                            wordStart = -1;
                        }
                        inAcronym = false;
                        currentWordIsAcronym = false;
                        continue;
                    }

                    var isUpper = char.IsUpper(c);
                    var nextIsUpper = i < length - 1 && char.IsUpper(text[i + 1]);
                    var nextIsLower = i < length - 1 && char.IsLower(text[i + 1]);

                    if (wordStart == -1)
                    {
                        wordStart = i;
                        inAcronym = isUpper && nextIsUpper;
                        currentWordIsAcronym = inAcronym;
                    }
                    else if (inAcronym && isUpper && nextIsLower)
                    {
                        yield return (wordStart..i, currentWordIsAcronym);
                        wordStart = i;
                        inAcronym = false;
                        currentWordIsAcronym = false;
                    }
                    else if (isUpper && !inAcronym)
                    {
                        yield return (wordStart..i, currentWordIsAcronym);
                        wordStart = i;
                        inAcronym = nextIsUpper;
                        currentWordIsAcronym = inAcronym;
                    }
                    else if (isUpper && nextIsUpper && !inAcronym)
                    {
                        inAcronym = true;
                        currentWordIsAcronym = true;
                    }
                }

                if (wordStart != -1)
                    yield return (wordStart..length, currentWordIsAcronym);
            }
        }
    }
}

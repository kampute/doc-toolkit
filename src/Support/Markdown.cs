// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides utility methods for encoding and decoding Markdown special characters.
    /// </summary>
    public static class Markdown
    {
        private const int MaxStackAllocSize = 1024;

        /// <summary>
        /// Determines whether the specified character is a Markdown special character that requires escaping.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <param name="excludeLineMarkers">Indicates whether to exclude Markdown markers that are only valid at the start of a line.</param>
        /// <returns><see langword="true"/> if the character requires encoding; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NeedsEncoding(char c, bool excludeLineMarkers = false) => c switch
        {
            '*' or '_' or '`' or '[' or ']' or '<' or '\\' => true,
            '#' or '+' or '-' or '>' => !excludeLineMarkers,
            _ => false
        };

        /// <summary>
        /// Tracks the leading digits of a line to detect the delimiter of an ordered list marker (e.g., "1." or "1)").
        /// </summary>
        /// <param name="c">The character being written.</param>
        /// <param name="atLineStart">Indicates whether the character is at the start of a line (ignoring leading whitespace).</param>
        /// <param name="leadingDigits">
        /// The number of digits written since the start of the line, or -1 if the line cannot start with an ordered list marker.
        /// </param>
        /// <returns><see langword="true"/> if the character is an ordered list delimiter that requires escaping; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// Digits cannot be backslash-escaped in Markdown, so an ordered list marker is escaped through its delimiter.
        /// </remarks>
        public static bool IsOrderedListDelimiter(char c, bool atLineStart, ref int leadingDigits)
        {
            if (c is >= '0' and <= '9')
            {
                leadingDigits = atLineStart || leadingDigits > 0 ? Math.Max(leadingDigits, 0) + 1 : -1;
                return false;
            }

            var isDelimiter = c is '.' or ')' && leadingDigits > 0;
            if (c == '\n')
                leadingDigits = 0;
            else if (!char.IsWhiteSpace(c) || leadingDigits > 0)
                leadingDigits = -1;
            return isDelimiter;
        }

        /// <summary>
        /// Encodes Markdown special characters in the given text.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <param name="atLineStart">Indicates whether the text is at the start of a line (ignoring leading whitespace).</param>
        /// <returns>A string with Markdown special characters escaped.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// If <paramref name="atLineStart"/> is <see langword="true"/>, the method escapes Markdown characters that have special
        /// meaning at the start of a line. Otherwise, these characters are escaped only if they are special in other contexts.
        /// <para>
        /// Encoding behavior adjusts dynamically, treating newlines as line breaks and ignoring leading whitespace when determining
        /// whether a character appears at the start of a line.
        /// </para>
        /// </remarks>
        public static string Encode(string text, bool atLineStart = true)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            var size = text.Length;
            var requiredCapacity = size * 2;
            var newTextSpan = requiredCapacity <= MaxStackAllocSize
                ? stackalloc char[requiredCapacity]
                : new char[requiredCapacity];

            var index = 0;
            var modified = false;
            var leadingDigits = atLineStart ? 0 : -1;

            foreach (var c in text.AsSpan())
            {
                var isListDelimiter = IsOrderedListDelimiter(c, atLineStart, ref leadingDigits);
                if (isListDelimiter || NeedsEncoding(c, excludeLineMarkers: !atLineStart))
                {
                    newTextSpan[index++] = '\\';
                    atLineStart = false;
                    modified = true;
                }
                else if (c == '\n')
                {
                    atLineStart = true;
                }
                else if (!char.IsWhiteSpace(c))
                {
                    atLineStart = false;
                }
                newTextSpan[index++] = c;
            }

            return modified ? newTextSpan[..index].ToString() : text;
        }

        /// <summary>
        /// Encodes Markdown special characters in the given text and writes the result to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="text">The text to encode.</param>
        /// <param name="output">The <see cref="TextWriter"/> to write the encoded text to.</param>
        /// <param name="atLineStart">Indicates whether the text is at the start of a line (ignoring leading whitespace).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="output"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// If <paramref name="atLineStart"/> is <see langword="true"/>, the method escapes Markdown characters that have special
        /// meaning at the start of a line. Otherwise, these characters are escaped only if they are special in other contexts.
        /// <para>
        /// Encoding behavior adjusts dynamically, treating newlines as line breaks and ignoring leading whitespace when determining
        /// whether a character appears at the start of a line.
        /// </para>
        /// </remarks>
        public static void Encode(ReadOnlySpan<char> text, TextWriter output, bool atLineStart = true)
        {
            if (output is null)
                throw new ArgumentNullException(nameof(output));

            var leadingDigits = atLineStart ? 0 : -1;
            foreach (var c in text)
            {
                var isListDelimiter = IsOrderedListDelimiter(c, atLineStart, ref leadingDigits);
                if (isListDelimiter || NeedsEncoding(c, excludeLineMarkers: !atLineStart))
                {
                    output.Write('\\');
                    atLineStart = false;
                }
                else if (c == '\n')
                {
                    atLineStart = true;
                }
                else if (!char.IsWhiteSpace(c))
                {
                    atLineStart = false;
                }
                output.Write(c);
            }
        }

        /// <summary>
        /// Decodes escaped Markdown special characters in the given text.
        /// </summary>
        /// <param name="text">The text to decode.</param>
        /// <returns>A string with escaped Markdown special characters restored to their original form.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// Only a backslash followed by an ASCII punctuation character is an escape. A backslash followed by any other character,
        /// or at the end of the text, is kept as a literal backslash.
        /// </remarks>
        public static string Decode(string text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            var size = text.Length;
            var newTextSpan = size <= MaxStackAllocSize ? stackalloc char[size] : new char[size];

            var index = 0;
            var modified = false;

            for (var i = 0; i < size; i++)
            {
                var c = text[i];
                if (c == '\\' && i + 1 < size && IsAsciiPunctuation(text[i + 1]))
                {
                    c = text[++i];
                    modified = true;
                }

                newTextSpan[index++] = c;
            }

            return modified ? newTextSpan[..index].ToString() : text;
        }

        /// <summary>
        /// Decodes escaped Markdown special characters in the given text and writes the result to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="text">The text to decode.</param>
        /// <param name="output">The <see cref="TextWriter"/> to write the decoded text to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="output"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// Only a backslash followed by an ASCII punctuation character is an escape. A backslash followed by any other character,
        /// or at the end of the text, is kept as a literal backslash.
        /// </remarks>
        public static void Decode(ReadOnlySpan<char> text, TextWriter output)
        {
            if (output is null)
                throw new ArgumentNullException(nameof(output));

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '\\' && i + 1 < text.Length && IsAsciiPunctuation(text[i + 1]))
                    c = text[++i];

                output.Write(c);
            }
        }

        /// <summary>
        /// Determines whether the specified character is an ASCII punctuation character, which is the only kind of character that
        /// can be backslash-escaped in Markdown.
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns><see langword="true"/> if the character is an ASCII punctuation character; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiPunctuation(char c) => c is (>= '!' and <= '/') or (>= ':' and <= '@') or (>= '[' and <= '`') or (>= '{' and <= '~');

        /// <summary>
        /// Converts the specified heading text to the anchor that GitHub and GitLab generate for a Markdown heading.
        /// </summary>
        /// <param name="heading">The text of the heading.</param>
        /// <returns>The anchor of the heading, without the leading '#'.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="heading"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// The anchor is generated by the following rules:
        /// <list type="bullet">
        ///   <item><description>All characters are converted to lowercase.</description></item>
        ///   <item><description>All characters except letters, numbers, hyphens, underscores and spaces are removed.</description></item>
        ///   <item><description>Each space is converted to a hyphen.</description></item>
        /// </list>
        /// For example, the heading <c>Repeat&lt;TResult&gt;(TResult, int)</c> has the anchor <c>repeattresulttresult-int</c>.
        /// <note type="caution" title="Caution">
        /// When a page has several headings with the same anchor, Markdown processors append a number to all but the first one.
        /// This method cannot account for that, because it sees one heading at a time.
        /// </note>
        /// </remarks>
        /// <seealso href="https://docs.gitlab.com/user/markdown/#heading-ids-and-links">GitLab heading IDs and links</seealso>
        public static string ToHeadingAnchor(string heading)
        {
            if (heading is null)
                throw new ArgumentNullException(nameof(heading));

            var text = heading.ToLowerInvariant();
            using var reusable = StringBuilderPool.Shared.GetBuilder();
            var sb = reusable.Builder;
            sb.EnsureCapacity(text.Length);

            for (var i = 0; i < text.Length; ++i)
            {
                var c = text[i];
                if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    if (char.IsLetter(text, i) || char.IsNumber(text, i))
                        sb.Append(c).Append(text[i + 1]);
                    ++i;
                }
                else if (c is ' ')
                    sb.Append('-');
                else if (c is '-' or '_' || char.IsLetter(c) || char.IsNumber(c))
                    sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines the minimum number of backticks required to fence code in Markdown.
        /// </summary>
        /// <param name="code">The code to fence.</param>
        /// <returns>The minimum number of backticks required to fence the code.</returns>
        /// <remarks>
        /// If the code contains a sequence of backticks, the fence must have one more backtick than the longest sequence;
        /// otherwise, the default backtick count for a fence is 3.
        /// </remarks>
        public static int GetMinimumFenceBackticks(ReadOnlySpan<char> code)
        {
            const int DefaultFenceBackticks = 3;

            var start = code.IndexOf('`');
            if (start == -1)
                return DefaultFenceBackticks;

            var maxConsecutiveBackticks = 0;
            var currentCount = 0;

            for (var i = start; i < code.Length; i++)
            {
                if (code[i] != '`')
                    currentCount = 0;
                else if (++currentCount > maxConsecutiveBackticks)
                    maxConsecutiveBackticks = currentCount;
            }

            return maxConsecutiveBackticks >= DefaultFenceBackticks
                ? maxConsecutiveBackticks + 1
                : DefaultFenceBackticks;
        }

        /// <summary>
        /// Enumerates all ATX-style headings in the given Markdown content, returning their text and corresponding levels.
        /// </summary>
        /// <param name="content">The Markdown content to inspect.</param>
        /// <returns>An enumerable of tuples, each containing the level and text of a heading found in the content.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="content"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// A heading is a line that, ignoring leading and trailing whitespace, starts with one to six '#' characters followed by
        /// non-empty text. Unlike CommonMark, the text does not need to be separated from the '#' characters by whitespace, so
        /// <c>#Title</c> is a heading. An optional closing sequence of '#' characters, when preceded by whitespace, is not part of
        /// the heading text, and backslash escapes in the heading text are decoded. Lines inside fenced code blocks are not headings.
        /// </remarks>
        public static IEnumerable<(int Level, string Heading)> EnumerateHeadings(string content)
        {
            if (content is null)
                throw new ArgumentNullException(nameof(content));

            return EnumerateHeadingsIterator(content);

            static IEnumerable<(int Level, string Heading)> EnumerateHeadingsIterator(string content)
            {
                var pos = 0;
                var length = content.Length;
                var openFenceChar = '\0';
                var openFenceLength = 0;
                while (pos < length)
                {
                    var lineStart = pos;
                    while (pos < length && content[pos] != '\n')
                        pos++;

                    var rawLine = content.AsSpan(lineStart, pos - lineStart);
                    if (pos < length)
                        pos++;

                    if (TryParseCodeFence(rawLine, out var fenceChar, out var fenceLength, out var infoString))
                    {
                        if (openFenceLength == 0)
                            (openFenceChar, openFenceLength) = (fenceChar, fenceLength);
                        else if (fenceChar == openFenceChar && fenceLength >= openFenceLength && infoString.IsEmpty)
                        {
                            openFenceLength = 0;
                        }
                        continue;
                    }

                    if (openFenceLength == 0 && TryParseHeading(rawLine.Trim(), out var level, out var headingText))
                        yield return (level, headingText);
                }
            }
        }

        /// <summary>
        /// Attempts to find the first ATX-style heading that appears before any non-heading content in the given Markdown text.
        /// </summary>
        /// <param name="content">The Markdown content to inspect.</param>
        /// <param name="heading">When this method returns, contains the heading text if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a heading was found and it appears before any non-heading content; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// Headings are recognized by the same rules as in <see cref="EnumerateHeadings(string)"/>, so <c>#Title</c> is a heading.
        /// Empty lines before the heading are skipped.
        /// </remarks>
        public static bool TryGetFirstHeading(ReadOnlySpan<char> content, [NotNullWhen(true)] out string? heading)
        {
            var pos = 0;
            var length = content.Length;
            while (pos < length)
            {
                var lineStart = pos;
                while (pos < length && content[pos] != '\n')
                    pos++;

                var line = content[lineStart..pos].Trim();
                if (pos < length)
                    pos++; // Skip the newline character

                if (line.IsEmpty)
                    continue;

                if (!TryParseHeading(line, out _, out heading))
                    break;

                return true;
            }

            heading = null;
            return false;
        }

        /// <summary>
        /// Attempts to parse a trimmed line as an ATX-style heading.
        /// </summary>
        /// <param name="line">The line to parse, without leading and trailing whitespace.</param>
        /// <param name="level">When this method returns, contains the level of the heading if the line is a heading; otherwise, zero.</param>
        /// <param name="heading">When this method returns, contains the text of the heading if the line is a heading; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the line is a heading with a non-empty text; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// A heading starts with one to six '#' characters. The optional closing sequence of '#' characters, when preceded by
        /// whitespace, is not part of the heading text. Backslash escapes in the heading text are decoded.
        /// </remarks>
        private static bool TryParseHeading(ReadOnlySpan<char> line, out int level, [NotNullWhen(true)] out string? heading)
        {
            const int MaxHeadingLevel = 6;

            level = 0;
            heading = null;

            while (level < line.Length && line[level] == '#')
                level++;

            if (level is 0 or > MaxHeadingLevel)
                return false;

            var text = line[level..].Trim();
            var closingStart = text.Length;
            while (closingStart > 0 && text[closingStart - 1] == '#')
                closingStart--;
            if (closingStart == 0 || char.IsWhiteSpace(text[closingStart - 1]))
                text = text[..closingStart].TrimEnd();

            if (text.IsEmpty)
                return false;

            heading = Decode(text.ToString());
            return true;
        }

        /// <summary>
        /// Attempts to parse a line of Markdown as a code fence, which opens or closes a fenced code block.
        /// </summary>
        /// <param name="line">The line to parse, without its line break.</param>
        /// <param name="fenceChar">When this method returns, contains the fence character ('`' or '~') if the line is a code fence; otherwise, '\0'.</param>
        /// <param name="fenceLength">When this method returns, contains the number of fence characters if the line is a code fence; otherwise, 0.</param>
        /// <param name="infoString">
        /// When this method returns, contains the text after the fence characters without leading and trailing whitespace, such as the
        /// language of the code, if the line is a code fence; otherwise, an empty span.
        /// </param>
        /// <returns><see langword="true"/> if the line is a code fence; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// A code fence is a sequence of at least three backticks or three tildes, indented by no more than three spaces. The info string
        /// of a backtick fence cannot contain backticks.
        /// <para>
        /// A fenced code block is closed by a fence that uses the same character, is at least as long as the opening fence, and has an
        /// empty info string. Determining whether a line closes a block is left to the caller, which knows the opening fence.
        /// </para>
        /// </remarks>
        public static bool TryParseCodeFence(ReadOnlySpan<char> line, out char fenceChar, out int fenceLength, out ReadOnlySpan<char> infoString)
        {
            const int MaxIndentation = 3;
            const int MinFenceLength = 3;

            fenceChar = '\0';
            fenceLength = 0;
            infoString = default;

            var start = 0;
            while (start < line.Length && line[start] == ' ')
                start++;

            if (start > MaxIndentation || start == line.Length || line[start] is not ('`' or '~'))
                return false;

            var end = start + 1;
            while (end < line.Length && line[end] == line[start])
                end++;

            if (end - start < MinFenceLength)
                return false;

            var info = line[end..].Trim();
            if (line[start] is '`' && info.IndexOf('`') >= 0)
                return false;

            fenceChar = line[start];
            fenceLength = end - start;
            infoString = info;
            return true;
        }

        /// <summary>
        /// Attempts to extract and parse the front matter block from Markdown content.
        /// </summary>
        /// <param name="content">The Markdown content to inspect.</param>
        /// <param name="frontMatter">When this method returns, contains the parsed front matter metadata if found; otherwise, an empty dictionary.</param>
        /// <param name="contentStart">When this method returns, contains the index in <paramref name="content"/> at which the body content starts.</param>
        /// <returns><see langword="true"/> if a front matter block was found and parsed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="FormatException">Thrown when a front matter block is found but cannot be parsed as valid YAML.</exception>
        /// <remarks>
        /// Front matter must start at the very beginning of the document, delimited by <c>---</c> on its own line.
        /// The closing delimiter may be either <c>---</c> or <c>...</c>.
        /// </remarks>
        public static bool TryExtractFrontMatter(ReadOnlySpan<char> content, out IReadOnlyDictionary<string, object?> frontMatter, out int contentStart)
        {
            frontMatter = Yaml.Empty;
            contentStart = 0;

            if (!content.StartsWith("---"))
                return false;

            var length = content.Length;

            var pos = 3;
            if (pos < length && content[pos] == '\r')
                pos++;
            if (pos >= length || content[pos] != '\n')
                return false;

            pos++;
            var yamlStart = pos;

            while (pos < length)
            {
                var lineStart = pos;
                while (pos < length && content[pos] != '\n')
                    pos++;

                var lineEnd = (pos > lineStart && content[pos - 1] == '\r') ? pos - 1 : pos;
                if (pos < length)
                    pos++;

                var line = content[lineStart..lineEnd];
                if (line.Equals("---", StringComparison.Ordinal) || line.Equals("...", StringComparison.Ordinal))
                {
                    frontMatter = Yaml.Parse(content[yamlStart..lineStart].TrimEnd());
                    contentStart = pos;
                    return true;
                }
            }

            return false;
        }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using System;
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
            '#' or '+' or '-' or '>' or >= '1' and <= '9' => !excludeLineMarkers,
            _ => false
        };

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
            Span<char> newTextSpan = requiredCapacity <= MaxStackAllocSize
                ? stackalloc char[requiredCapacity]
                : new char[requiredCapacity];

            var index = 0;
            var modified = false;

            foreach (var c in text.AsSpan())
            {
                if (NeedsEncoding(c, excludeLineMarkers: !atLineStart))
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

            foreach (var c in text)
            {
                if (NeedsEncoding(c, excludeLineMarkers: !atLineStart))
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
        public static string Decode(string text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            var size = text.Length;
            Span<char> newTextSpan = size <= MaxStackAllocSize ? stackalloc char[size] : new char[size];

            var index = 0;
            var escaped = false;
            var modified = false;

            foreach (var c in text.AsSpan())
            {
                if (c == '\\' && !escaped)
                {
                    escaped = true;
                    modified = true;
                    continue;
                }

                newTextSpan[index++] = c;
                escaped = false;
            }

            return modified ? newTextSpan[..index].ToString() : text;
        }

        /// <summary>
        /// Decodes escaped Markdown special characters in the given text and writes the result to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="text">The text to decode.</param>
        /// <param name="output">The <see cref="TextWriter"/> to write the decoded text to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="output"/> is <see langword="null"/>.</exception>
        public static void Decode(ReadOnlySpan<char> text, TextWriter output)
        {
            if (output is null)
                throw new ArgumentNullException(nameof(output));

            var escaped = false;
            foreach (var c in text)
            {
                if (c == '\\' && !escaped)
                {
                    escaped = true;
                    continue;
                }

                output.Write(c);
                escaped = false;
            }
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
    }
}

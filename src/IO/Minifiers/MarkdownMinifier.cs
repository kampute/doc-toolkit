// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.IO.Minifiers
{
    using Kampute.DocToolkit.IO.Writers;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A <see cref="TextWriter"/> that wraps another <see cref="TextWriter"/> to output well-formed Markdown content, minifying it by
    /// eliminating superfluous whitespace while preserving whitespace within code blocks to maintain the intended formatting.
    /// </summary>
    /// <remarks>
    /// The <see cref="MarkdownMinifier"/> processes Markdown content by optimizing whitespace usage while ensuring that the formatting
    /// remains intact, particularly within code blocks. This helps improve readability and reduces unnecessary space without altering
    /// the Markdown's intended structure.
    /// <note type="caution" title="Important">
    /// This writer assumes well-formed Markdown input. Malformed Markdown may result in unexpected minification behavior and potentially
    /// corrupt output.
    /// </note>
    /// <note type="info" title="Visual Appeal">
    /// This writer not only eliminates redundant whitespace but also improve readability and visual appeal of Markdown in plain text.
    /// </note>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public class MarkdownMinifier : WrappedTextWriter
    {
        private bool newLineInitiated = true;
        private bool suppressWhitespace = true;

        private uint pendingNewLines = 0;
        private uint pendingSpaces = 0;

        private bool insideCodeBlock = false;
        private uint countedBackTick = 0;
        private uint usedBackTick = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownMinifier"/> class.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the minimized content to.</param>
        /// <param name="leaveOpen">A value indicating whether the underlying writer should be left open when the <see cref="MarkdownMinifier"/> is disposed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <see langword="null"/>.</exception>
        public MarkdownMinifier(TextWriter writer, bool leaveOpen = false)
            : base(writer, leaveOpen)
        {
        }

        /// <summary>
        /// Writes a character to the text stream, if it is not redundant.
        /// </summary>
        /// <param name="value">The character to write.</param>
        public override void Write(char value)
        {
            if (insideCodeBlock)
            {
                if (!IsCodeBlockEnded(value))
                {
                    UnderlyingWriter.Write(value);
                    return;
                }
                insideCodeBlock = false;
            }

            if (char.IsWhiteSpace(value))
            {
                UpdateWhitespaceState(value);
                return;
            }

            if (suppressWhitespace)
            {
                suppressWhitespace = false;
                pendingNewLines = 0;
                pendingSpaces = 0;
            }

            if (newLineInitiated)
            {
                newLineInitiated = false;
                if (value is '#')
                    pendingSpaces = 0; // Reset to zero for headings
                else
                    pendingSpaces &= ~1u; // Round down to nearest even number
            }

            WriteNonWhitespace(value);
            if (IsCodeBlockStarted(value))
                insideCodeBlock = true;
        }

        /// <summary>
        /// Determines whether a code block has started.
        /// </summary>
        /// <param name="value">The character to evaluate.</param>
        /// <returns><see langword="true"/> if a code block has started; otherwise, <see langword="false"/>.</returns>
        private bool IsCodeBlockStarted(char value)
        {
            if (value is '`')
            {
                countedBackTick++;
                return false;
            }

            (usedBackTick, countedBackTick) = (countedBackTick, 0);
            return usedBackTick >= 3;
        }

        /// <summary>
        /// Determines whether a code block has ended.
        /// </summary>
        /// <param name="value">The character to evaluate.</param>
        /// <returns><see langword="true"/> if a code block has ended; otherwise, <see langword="false"/>.</returns>
        private bool IsCodeBlockEnded(char value)
        {
            if (value is '`')
            {
                countedBackTick++;
                return false;
            }

            if (usedBackTick == countedBackTick)
            {
                countedBackTick = usedBackTick = 0;
                return true;
            }

            countedBackTick = 0;
            return false;
        }

        /// <summary>
        /// Updates the state of the writer when a whitespace character is encountered.
        /// </summary>
        /// <param name="whitespace">The whitespace character to process.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateWhitespaceState(char whitespace)
        {
            if (whitespace is '\n')
            {
                newLineInitiated = true;
                pendingSpaces = 0;
                if (pendingNewLines < 2)
                    pendingNewLines++;
            }
            else if (whitespace is '\t')
            {
                if (newLineInitiated)
                    pendingSpaces += 2; // Two spaces for each tab at the beginning of a line
                else
                    pendingSpaces = 1;  // Only one space for consecutive tabs within a line
            }
            else if (whitespace is not '\r')
            {
                if (newLineInitiated)
                    pendingSpaces++;    // One space for each whitespace character at the beginning of a line
                else
                    pendingSpaces = 1;  // Only one space for consecutive spaces within a line
            }
        }

        /// <summary>
        /// Writes pending whitespace characters and the specified non-whitespace character to the underlying <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="nonWhitespace">The non-whitespace character to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteNonWhitespace(char nonWhitespace)
        {
            while (pendingNewLines > 0)
            {
                UnderlyingWriter.Write('\n');
                pendingNewLines--;
            }

            while (pendingSpaces > 0)
            {
                UnderlyingWriter.Write(' ');
                pendingSpaces--;
            }

            UnderlyingWriter.Write(nonWhitespace);
        }
    }
}

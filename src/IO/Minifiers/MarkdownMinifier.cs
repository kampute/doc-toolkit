// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.IO.Minifiers
{
    using Kampute.DocToolkit.IO.Writers;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// A <see cref="TextWriter"/> that wraps another <see cref="TextWriter"/> to output well-formed Markdown content, minifying it by
    /// eliminating superfluous whitespace while preserving whitespace within code blocks to maintain the intended formatting.
    /// </summary>
    /// <remarks>
    /// The <see cref="MarkdownMinifier"/> processes Markdown content by optimizing whitespace usage while ensuring that the formatting
    /// remains intact, particularly within code blocks. This helps improve readability and reduces unnecessary space without altering
    /// the Markdown's intended structure.
    /// <para>
    /// Whitespace is kept as written within fenced code blocks and within indented code blocks. An indented code block starts with a
    /// line that is indented by four or more columns, where a tab counts as four columns, and follows a blank line. Indented lines
    /// within a list are treated as continuations of the list items rather than as code, so code blocks nested inside list items are
    /// minified like other content.
    /// </para>
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

        private readonly StringBuilder lineIndentation = new();
        private bool lineHasContent = false;
        private bool previousLineBlank = true;
        private bool insideIndentedCodeLine = false;
        private bool insideIndentedCodeBlock = false;
        private bool insideList = false;
        private ListMarkerState listMarkerState = ListMarkerState.None;

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

            if (insideIndentedCodeLine)
            {
                WriteIndentedCode(value);
                return;
            }

            UpdateListMarkerState(value);

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
                if (StartsIndentedCodeLine())
                {
                    BeginIndentedCodeLine(value);
                    return;
                }

                StartLine(value);

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
        /// Determines whether the line that is starting belongs to an indented code block.
        /// </summary>
        /// <returns><see langword="true"/> if the line is part of an indented code block; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// A line indented by four or more columns starts an indented code block when it follows a blank line and does not continue
        /// a list item, and continues an indented code block that is already open.
        /// </remarks>
        private bool StartsIndentedCodeLine()
        {
            const int CodeIndentation = 4;

            return GetIndentationColumns() >= CodeIndentation && (insideIndentedCodeBlock || (previousLineBlank && !insideList));
        }

        /// <summary>
        /// Gets the indentation of the current line in columns, where a tab advances to the next multiple of four columns.
        /// </summary>
        /// <returns>The number of columns of the leading spaces and tabs of the current line.</returns>
        private int GetIndentationColumns()
        {
            const int TabSize = 4;

            var columns = 0;
            for (var i = 0; i < lineIndentation.Length; i++)
            {
                var c = lineIndentation[i];
                if (c is ' ')
                    columns++;
                else if (c is '\t')
                    columns += TabSize - (columns % TabSize);
                else
                    break;
            }

            return columns;
        }

        /// <summary>
        /// Writes the first non-whitespace character of a line of an indented code block, preceded by the pending line breaks and
        /// the original indentation of the line.
        /// </summary>
        /// <param name="value">The first non-whitespace character of the line.</param>
        private void BeginIndentedCodeLine(char value)
        {
            while (pendingNewLines > 0)
            {
                UnderlyingWriter.Write('\n');
                pendingNewLines--;
            }

            UnderlyingWriter.Write(lineIndentation.ToString());
            UnderlyingWriter.Write(value);

            pendingSpaces = 0;
            newLineInitiated = false;
            lineHasContent = true;
            insideIndentedCodeLine = true;
            insideIndentedCodeBlock = true;
        }

        /// <summary>
        /// Writes a character of a line of an indented code block as is, and ends the line at a line break.
        /// </summary>
        /// <param name="value">The character to write.</param>
        private void WriteIndentedCode(char value)
        {
            if (value is '\n')
            {
                insideIndentedCodeLine = false;
                UpdateWhitespaceState(value);
            }
            else if (value is not '\r')
            {
                UnderlyingWriter.Write(value);
            }
        }

        /// <summary>
        /// Updates the block state when a line that is not part of an indented code block starts.
        /// </summary>
        /// <param name="value">The first non-whitespace character of the line.</param>
        private void StartLine(char value)
        {
            if (insideIndentedCodeBlock)
            {
                insideIndentedCodeBlock = false;
                if (pendingNewLines > 2)
                    pendingNewLines = 2;
            }

            // A line at the margin after a blank line ends a list, unless it starts another list item.
            if (previousLineBlank && GetIndentationColumns() < 2)
                insideList = false;

            listMarkerState = value switch
            {
                '-' or '*' or '+' => ListMarkerState.Bullet,
                >= '0' and <= '9' => ListMarkerState.Digits,
                _ => ListMarkerState.None
            };

            lineHasContent = true;
        }

        /// <summary>
        /// Tracks the characters that follow the first character of a line to detect a list item marker.
        /// </summary>
        /// <param name="value">The character being written.</param>
        private void UpdateListMarkerState(char value)
        {
            if (listMarkerState is ListMarkerState.None || newLineInitiated)
                return;

            switch (listMarkerState)
            {
                case ListMarkerState.Digits when value is >= '0' and <= '9':
                    return;
                case ListMarkerState.Digits when value is '.' or ')':
                    listMarkerState = ListMarkerState.OrderedDelimiter;
                    return;
                case ListMarkerState.Bullet or ListMarkerState.OrderedDelimiter when value is ' ' or '\t' or '\n':
                    insideList = true;
                    break;
            }

            listMarkerState = ListMarkerState.None;
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
                if (pendingNewLines < 2 || insideIndentedCodeBlock)
                    pendingNewLines++; // Blank lines within an indented code block are kept until the block ends

                previousLineBlank = !lineHasContent;
                lineHasContent = false;
                lineIndentation.Clear();
                return;
            }

            if (newLineInitiated)
                lineIndentation.Append(whitespace);

            if (whitespace is '\t')
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

        /// <summary>
        /// Represents the progress of detecting a list item marker at the start of a line.
        /// </summary>
        private enum ListMarkerState
        {
            /// <summary>
            /// The line does not start with a list item marker, or the marker has been detected.
            /// </summary>
            None,

            /// <summary>
            /// The line starts with a bullet list marker character.
            /// </summary>
            Bullet,

            /// <summary>
            /// The line starts with the digits of an ordered list marker.
            /// </summary>
            Digits,

            /// <summary>
            /// The digits at the start of the line are followed by the delimiter of an ordered list marker.
            /// </summary>
            OrderedDelimiter
        }
    }
}

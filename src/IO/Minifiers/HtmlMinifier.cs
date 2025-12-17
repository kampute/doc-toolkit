// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.IO.Minifiers
{
    using Kampute.DocToolkit.IO.Writers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    /// <summary>
    /// A <see cref="TextWriter"/> that wraps another <see cref="TextWriter"/> that outputs well-formed HTML content, minifying it
    /// by removing unnecessary whitespace and comments, while preserving whitespace within <c>&lt;pre&gt;</c> elements.
    /// </summary>
    /// <remarks>
    /// The <see cref="HtmlMinifier"/> processes HTML content character by character, optimizing it by removing unnecessary whitespace
    /// and comments while preserving essential formatting, particularly within <c>&lt;pre&gt;</c> elements. This results in a more
    /// compact HTML output suitable for production use and bandwidth-sensitive scenarios.
    /// <note type="caution" title="Important">
    /// This writer assumes well-formed HTML input. Malformed or improperly structured HTML may lead to unexpected minification behavior
    /// and potentially corrupt output.
    /// </note>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public class HtmlMinifier : WrappedTextWriter
    {
        // Represents the different parsing states of the writer.
        private enum State { Text, Tag, Comment }

        // Represents the category of an HTML tag.
        private enum TagCategory { Unknown, Opening, Closing, SelfClosing }

        // The buffer used to store the current tag name.
        private readonly StringBuilder tagBuffer = new(32);

        // The current state of the writer.
        private State state = State.Text;

        // The current tag category.
        private TagCategory currentTagCategory;

        // The current tag name, if any.
        private string? currentTagName;

        // The current quote character used for the current attribute value, if any.
        private char currentQuote;

        // The number of consecutive '-' characters in a comment.
        private int commentTracker;

        // The number of nested pre-formatted elements that are being preserved.
        private int preservedDepth;

        // Indicates that a space should be written before the next non-whitespace character.
        private bool hasPendingSpace;

        // Indicates that the next space character should be ignored.
        private bool ignoreNextSpace = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlMinifier"/> class.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the minimized content to.</param>
        /// <param name="leaveOpen">Whether to leave the underlying writer open on disposal.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <see langword="null"/>.</exception>
        public HtmlMinifier(TextWriter writer, bool leaveOpen = false)
            : base(writer, leaveOpen)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether to preserve comments within the HTML content.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to preserve comments within the HTML content; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
        /// </value>
        public bool PreserveComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to preserve whitespace within attribute values.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to preserve whitespace within attribute values; otherwise, <see langword="false"/>. The default is <see langword="false"/>.
        /// </value>
        public bool PreserveAttributeWhitespace { get; set; }

        /// <summary>
        /// Gets the set of HTML tags that are considered pre-formatted elements.
        /// </summary>
        /// <value>
        /// The set of HTML tags that are considered pre-formatted elements. Content within these elements is preserved without
        /// whitespace normalization or comment removal.
        /// </value>
        public HashSet<string> PreformattedTags { get; } = new(StringComparer.OrdinalIgnoreCase)
        {
            "PRE", "TEXTAREA", "SCRIPT", "STYLE"
        };

        /// <summary>
        /// Gets the set of HTML tags that are considered inline elements.
        /// </summary>
        /// <value>
        /// The set of HTML tags that are considered inline elements. Whitespace around these elements is preserved if adjacent
        /// to inline elements or text nodes.
        /// </value>
        public HashSet<string> InlineTags { get; } = new(StringComparer.OrdinalIgnoreCase)
        {
            "A", "ABBR", "B", "BDI", "BDO", "BR", "BUTTON", "CITE", "CODE", "DATA", "DFN", "DEL", "EM", "I",
            "IMG", "INPUT", "INS", "KBD", "LABEL", "MARK", "OBJECT", "OUTPUT", "Q", "RUBY", "RT", "RP", "S",
            "SAMP", "SELECT", "SMALL", "SPAN", "STRONG", "SUB", "SUP", "TIME", "TT", "U", "VAR", "WBR"
        };

        /// <summary>
        /// Processes and writes a single character.
        /// </summary>
        /// <param name="value">The character to process.</param>
        public override void Write(char value)
        {
            switch (state)
            {
                case State.Text: ProcessText(value); break;
                case State.Tag: ProcessTag(value); break;
                case State.Comment: ProcessComment(value); break;
            }
        }

        /// <summary>
        /// Processes a text node character, which may be normalized.
        /// </summary>
        /// <param name="value">The character to process.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessText(char value)
        {
            if (IsTagOpening(value))
                return;

            if (preservedDepth != 0)
            {
                UnderlyingWriter.Write(value);
                return;
            }

            if (char.IsWhiteSpace(value))
            {
                hasPendingSpace = !ignoreNextSpace;
                return;
            }

            FlushPendingSpace();
            UnderlyingWriter.Write(value);
            ignoreNextSpace = false;
        }

        /// <summary>
        /// Processes a comment node character.
        /// </summary>
        /// <param name="value">The character to process.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessComment(char value)
        {
            if (preservedDepth != 0 || PreserveComments)
                UnderlyingWriter.Write(value);

            if (value is '>' && commentTracker >= 2)
                state = State.Text;

            if (value == '-')
                commentTracker++;
            else
                commentTracker = 0;
        }

        /// <summary>
        /// Processes a tag node character.
        /// </summary>
        /// <param name="value">The character to process.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessTag(char value)
        {
            if (currentTagName is not null)
            {
                if (value is not '>')
                    ProcessTagAttributes(value);
                else
                    FinalizeTag();
            }
            else if (DetectTag(value))
            {
                currentTagName = tagBuffer.ToString();
                InitializeTag();
                if (value is '>')
                    FinalizeTag();
            }
        }

        /// <summary>
        /// Processes a tag attribute character.
        /// </summary>
        /// <param name="value">The character to process.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessTagAttributes(char value)
        {
            if (char.IsWhiteSpace(value))
            {
                if (currentQuote is not '\0' && PreserveAttributeWhitespace)
                    UnderlyingWriter.Write(value);
                else
                    hasPendingSpace = !ignoreNextSpace;
                return;
            }

            ignoreNextSpace = false;
            if (currentQuote is '\0')
            {
                if (value is '"' or '\'')
                {
                    currentQuote = value;
                    hasPendingSpace = false;
                    ignoreNextSpace = true;
                }
                else if (value is '=')
                {
                    hasPendingSpace = false;
                    ignoreNextSpace = true;
                }
                else if (value is '/')
                {
                    hasPendingSpace = false;
                    ignoreNextSpace = true;
                    if (currentTagCategory == TagCategory.Unknown)
                        currentTagCategory = TagCategory.SelfClosing;
                }
            }
            else if (currentQuote == value)
            {
                currentQuote = '\0';
                hasPendingSpace = false;
            }

            FlushPendingSpace();
            UnderlyingWriter.Write(value);
        }

        /// <summary>
        /// Initializes the recently detected tag.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitializeTag()
        {
            if (hasPendingSpace)
            {
                hasPendingSpace = false;
                if (currentTagCategory is not TagCategory.Closing && InlineTags.Contains(currentTagName!))
                    UnderlyingWriter.Write(' ');
            }

            UnderlyingWriter.Write('<');
            if (currentTagCategory is TagCategory.Closing)
                UnderlyingWriter.Write('/');
            UnderlyingWriter.Write(currentTagName);
            if (currentTagCategory is TagCategory.SelfClosing)
                UnderlyingWriter.Write('/');

            ignoreNextSpace = true;
            if (currentTagCategory is TagCategory.Unknown)
            {
                hasPendingSpace = true;
                ignoreNextSpace = false;
            }
        }

        /// <summary>
        /// Finalizes the recently detected tag.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FinalizeTag()
        {
            UnderlyingWriter.Write('>');

            if (currentTagCategory is TagCategory.Unknown)
                currentTagCategory = TagCategory.Opening;

            state = State.Text;
            hasPendingSpace = false;
            ignoreNextSpace = currentTagCategory is TagCategory.Opening || !InlineTags.Contains(currentTagName!);

            if (PreformattedTags.Contains(currentTagName!))
            {
                if (currentTagCategory is TagCategory.Opening)
                    preservedDepth++;
                else if (currentTagCategory is TagCategory.Closing)
                    preservedDepth--;
            }
        }

        /// <summary>
        /// Determines whether the specified character is an opening tag.
        /// </summary>
        /// <param name="value">The character to evaluate.</param>
        /// <returns><see langword="true"/> if the character is an opening tag; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsTagOpening(char value)
        {
            if (value is not '<')
                return false;

            tagBuffer.Clear();
            currentTagName = null;
            currentTagCategory = TagCategory.Unknown;
            state = State.Tag;
            return true;
        }

        /// <summary>
        /// Attempts to detect an HTML tag from the current character.
        /// </summary>
        /// <param name="value">The character to process.</param>
        /// <returns><see langword="true"/> if the tag detection process is complete; otherwise, <see langword="false"/>.</returns>
        private bool DetectTag(char value)
        {
            if (char.IsWhiteSpace(value))
                return tagBuffer.Length != 0;

            if (value is '/')
            {
                if (tagBuffer.Length == 0)
                {
                    currentTagCategory = TagCategory.Closing;
                    return false;
                }

                currentTagCategory = TagCategory.SelfClosing;
                return true;
            }

            if (value is '>')
            {
                if (currentTagCategory is TagCategory.Unknown)
                    currentTagCategory = TagCategory.Opening;

                return true;
            }

            tagBuffer.Append(value);
            if (tagBuffer.Length == 3 && tagBuffer[0] is '!' && tagBuffer[1] is '-' && tagBuffer[2] is '-')
            {
                state = State.Comment;
                if (preservedDepth != 0 || PreserveComments)
                    UnderlyingWriter.Write("<!--");
            }

            return false;
        }

        /// <summary>
        /// Writes the pending space character, if any.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushPendingSpace()
        {
            if (!hasPendingSpace)
                return;

            UnderlyingWriter.Write(' ');
            hasPendingSpace = false;
        }
    }
}

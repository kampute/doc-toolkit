// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides simplified YAML parsing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class supports a limited YAML subset intended for lightweight metadata scenarios. It supports
    /// string keys, scalar values, simple sequences, simple nested mappings based on indentation, and
    /// block scalars.
    /// </para>
    /// <note type="important" title="Important">
    /// The parser does not support all YAML features and is designed for simplicity and performance in
    /// common use cases. It does not handle complex constructs such as anchors, aliases, or advanced tags.
    /// For more advanced YAML processing needs, consider using a full-featured YAML library.
    /// </note>
    /// </remarks>
    public static class Yaml
    {
        /// <summary>
        /// Gets a reusable empty read-only YAML mapping.
        /// </summary>
        /// <value>
        /// A reusable empty read-only YAML mapping.
        /// </value>
        public static readonly IReadOnlyDictionary<string, object?> Empty = new Dictionary<string, object?>(0, StringComparer.Ordinal);

        /// <summary>
        /// Parses the specified YAML text into a read-only dictionary.
        /// </summary>
        /// <param name="text">The YAML text to parse.</param>
        /// <returns>A read-only dictionary containing parsed keys and values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
        /// <exception cref="FormatException">Thrown when the YAML content is malformed or uses unsupported constructs.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyDictionary<string, object?> Parse(ReadOnlySpan<char> text)
        {
            return text.IsEmpty ? Empty : new Parser(text).Parse();
        }

        /// <summary>
        /// A lightweight structural YAML parser.
        /// </summary>
        private ref struct Parser
        {
            private readonly ReadOnlySpan<char> source;
            private readonly List<LineToken> tokens;
            private int index;

            /// <summary>
            /// Initializes a new instance of the <see cref="Parser"/> struct.
            /// </summary>
            /// <param name="sourceSpan">The source span.</param>
            public Parser(ReadOnlySpan<char> sourceSpan)
            {
                source = sourceSpan;
                tokens = [];
                index = 0;

                var lineNumber = 0;
                var start = 0;
                for (var i = 0; i <= source.Length; i++)
                {
                    if (i != source.Length && source[i] is not '\n')
                        continue;

                    lineNumber++;

                    var end = i;
                    if (end > start && source[end - 1] is '\r')
                        end--;

                    var lineSpan = source[start..end];
                    var indent = 0;
                    while (indent < lineSpan.Length && char.IsWhiteSpace(lineSpan[indent]))
                        indent++;

                    var contentLen = StripComment(lineSpan[indent..], lineNumber);
                    var contentSpan = lineSpan.Slice(indent, contentLen).TrimEnd();

                    if (!contentSpan.IsEmpty)
                        tokens.Add(new LineToken(indent, start + indent, contentSpan.Length, lineNumber));

                    start = i + 1;
                }
            }

            /// <summary>
            /// Parses the document.
            /// </summary>
            /// <returns>A read-only dictionary of the top-level mapping.</returns>
            public IReadOnlyDictionary<string, object?> Parse()
            {
                if (tokens.Count == 0)
                    return Empty;

                var root = ParseMapping(tokens[0].Indent);
                if (index < tokens.Count)
                    throw CreateFormatException(tokens[index].LineNumber, "Unexpected content after the root mapping.");

                return root;
            }

            /// <summary>
            /// Parses a YAML mapping block.
            /// </summary>
            /// <param name="indent">The expected block indentation.</param>
            /// <returns>The parsed mapping.</returns>
            private Dictionary<string, object?> ParseMapping(int indent)
            {
                var map = new Dictionary<string, object?>(StringComparer.Ordinal);
                while (index < tokens.Count)
                {
                    var token = tokens[index];
                    if (token.Indent < indent)
                        break;

                    if (token.Indent > indent)
                        throw CreateFormatException(token.LineNumber, "Unexpected indentation.");

                    var tokenContent = source.Slice(token.Offset, token.Length);

                    if (IsSequenceItem(tokenContent))
                        throw CreateFormatException(token.LineNumber, "A mapping key was expected.");

                    if (!TrySplitKeyValue(tokenContent, out var keySpan, out var valueSpan))
                        throw CreateFormatException(token.LineNumber, "A mapping entry must contain ':'.");

                    index++;

                    object? value;
                    if (valueSpan is ['|' or '>', ..])
                        value = ParseBlockScalar(valueSpan[0], token.Indent);
                    else if (!valueSpan.IsEmpty)
                        value = ParseInlineValue(valueSpan, token.LineNumber);
                    else if (index < tokens.Count && tokens[index].Indent > indent)
                        value = ParseNode(tokens[index].Indent);
                    else
                        value = null;

                    var key = keySpan.ToString();
                    if (!map.TryAdd(key, value))
                        throw CreateFormatException(token.LineNumber, $"Duplicate key '{key}'.");
                }

                return map;
            }

            /// <summary>
            /// Parses a YAML sequence block.
            /// </summary>
            /// <param name="indent">The expected block indentation.</param>
            /// <returns>A list of objects.</returns>
            private List<object?> ParseSequence(int indent)
            {
                var list = new List<object?>();

                while (index < tokens.Count)
                {
                    var token = tokens[index];
                    if (token.Indent < indent)
                        break;

                    if (token.Indent > indent)
                        throw CreateFormatException(token.LineNumber, "Unexpected indentation.");

                    var tokenContent = source.Slice(token.Offset, token.Length);
                    if (!IsSequenceItem(tokenContent))
                        break;

                    var itemSpan = GetSequenceItemValue(tokenContent);

                    index++;

                    object? value;
                    if (itemSpan is ['|' or '>', ..])
                    {
                        value = ParseBlockScalar(itemSpan[0], token.Indent);
                    }
                    else if (itemSpan.Length == 0)
                    {
                        value = index < tokens.Count && tokens[index].Indent > indent
                            ? ParseNode(tokens[index].Indent)
                            : null;
                    }
                    else if (TrySplitKeyValue(itemSpan, out var keySpan, out var valueSpan))
                    {
                        var key = keySpan.ToString();
                        var itemMap = new Dictionary<string, object?>(StringComparer.Ordinal);

                        itemMap[key] = valueSpan is ['|' or '>', ..]
                            ? ParseBlockScalar(valueSpan[0], token.Indent)
                            : valueSpan.Length > 0
                                ? ParseInlineValue(valueSpan, token.LineNumber)!
                                : index < tokens.Count && tokens[index].Indent > indent
                                    ? ParseNode(tokens[index].Indent)!
                                    : null;

                        if (index < tokens.Count && tokens[index].Indent > indent)
                        {
                            var additionalEntries = ParseMapping(tokens[index].Indent);
                            foreach (var entry in additionalEntries)
                            {
                                if (!itemMap.TryAdd(entry.Key, entry.Value))
                                    throw CreateFormatException(token.LineNumber, $"Duplicate key '{entry.Key}'.");
                            }
                        }

                        value = itemMap;
                    }
                    else
                    {
                        value = ParseInlineValue(itemSpan, token.LineNumber);
                    }

                    list.Add(value!);
                }

                return list;
            }

            /// <summary>
            /// Parses a block scalar.
            /// </summary>
            /// <param name="blockType">The character indicating literal or folded.</param>
            /// <param name="parentIndent">The indentation of the parent block.</param>
            /// <returns>The block scalar string.</returns>
            private string ParseBlockScalar(char blockType, int parentIndent)
            {
                if (index >= tokens.Count)
                    return string.Empty;

                var blockIndent = tokens[index].Indent;
                if (blockIndent <= parentIndent)
                    return string.Empty;

                using var reusable = StringBuilderPool.Shared.GetBuilder();
                var sb = reusable.Builder;

                var literal = blockType == '|';
                var firstLine = true;
                var expectedIndent = blockIndent;

                while (index < tokens.Count)
                {
                    var token = tokens[index];
                    if (token.Indent < expectedIndent)
                        break;

                    if (!firstLine)
                        sb.Append(literal ? '\n' : ' ');

                    var tokenContent = source.Slice(token.Offset, token.Length);
                    sb.Append(tokenContent);
                    firstLine = false;
                    index++;
                }

                return sb.ToString().TrimEnd();
            }

            /// <summary>
            /// Parses an arbitrary node.
            /// </summary>
            /// <param name="indent">The expected node indentation.</param>
            /// <returns>The parsed node.</returns>
            private object? ParseNode(int indent)
            {
                if (index >= tokens.Count)
                    return null;

                var token = tokens[index];
                if (token.Indent < indent)
                    return null;

                var tokenContent = source.Slice(token.Offset, token.Length);
                return IsSequenceItem(tokenContent)
                    ? ParseSequence(indent)
                    : ParseMapping(indent);
            }

            /// <summary>
            /// Parses an inline scalar value.
            /// </summary>
            /// <param name="span">The span to parse.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <returns>The parsed object.</returns>
            private static object? ParseInlineValue(ReadOnlySpan<char> span, int lineNumber)
            {
                span = span.Trim();
                if (span.Length == 0)
                    return string.Empty;

                if (span[0] is '"' or '\'')
                    return ParseQuotedValue(span, lineNumber);

                if (span.Equals("null", StringComparison.OrdinalIgnoreCase) || span.Equals("~", StringComparison.Ordinal))
                    return null;

                if (span.Equals("true", StringComparison.OrdinalIgnoreCase))
                    return true;

                if (span.Equals("false", StringComparison.OrdinalIgnoreCase))
                    return false;

                if (long.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
                    return longValue;

                if (double.TryParse(span, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
                    return doubleValue;

                return span.ToString();
            }

            /// <summary>
            /// Parses a quoted scalar value.
            /// </summary>
            /// <param name="span">The span indicating the quoted value.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <returns>The unquoted string.</returns>
            private static string ParseQuotedValue(ReadOnlySpan<char> span, int lineNumber)
            {
                var quote = span[0];
                if (span.Length < 2 || span[^1] != quote)
                    throw CreateFormatException(lineNumber, "Quoted scalar is not terminated.");

                return quote == '"'
                    ? ParseDoubleQuotedValue(span, lineNumber)
                    : ParseSingleQuotedValue(span, lineNumber);
            }

            /// <summary>
            /// Parses a double-quoted string.
            /// </summary>
            /// <param name="span">The entire double-quoted string span.</param>
            /// <param name="lineNumber">The line number for diagnostic purposes.</param>
            /// <returns>The unescaped string, without quotes.</returns>
            private static string ParseDoubleQuotedValue(ReadOnlySpan<char> span, int lineNumber)
            {
                using var reusable = StringBuilderPool.Shared.GetBuilder();
                var sb = reusable.Builder;

                for (var i = 1; i < span.Length - 1; i++)
                {
                    var c = span[i];
                    if (c is not '\\')
                    {
                        sb.Append(c);
                        continue;
                    }

                    if (i + 1 >= span.Length - 1)
                        throw CreateFormatException(lineNumber, "Invalid escape sequence in double-quoted scalar.");

                    i++;
                    sb.Append(span[i] switch
                    {
                        '\\' => '\\',
                        '"' => '"',
                        '0' => '\0',
                        'a' => '\a',
                        'b' => '\b',
                        'f' => '\f',
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        'v' => '\v',
                        _ => throw CreateFormatException(lineNumber, $"Unsupported escape sequence '\\{span[i]}'.")
                    });
                }

                return sb.ToString();
            }

            /// <summary>
            /// Parses a single-quoted string.
            /// </summary>
            /// <param name="span">The single-quoted span.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <returns>The unescaped string, without quotes.</returns>
            private static string ParseSingleQuotedValue(ReadOnlySpan<char> span, int lineNumber)
            {
                using var reusable = StringBuilderPool.Shared.GetBuilder();
                var sb = reusable.Builder;

                for (var i = 1; i < span.Length - 1; i++)
                {
                    var c = span[i];
                    if (c is not '\'')
                    {
                        sb.Append(c);
                        continue;
                    }

                    if (i + 1 < span.Length - 1 && span[i + 1] == '\'')
                    {
                        sb.Append('\'');
                        i++;
                        continue;
                    }

                    throw CreateFormatException(lineNumber, "Single quote must be escaped by doubling it.");
                }

                return sb.ToString();
            }

            /// <summary>
            /// Splits a `key: value` span into components.
            /// </summary>
            /// <param name="span">The span acting as mapping entry.</param>
            /// <param name="key">Outputs the key span.</param>
            /// <param name="valueSpan">Outputs the value span.</param>
            /// <returns><see langword="true"/> if split was successful; otherwise, <see langword="false"/>.</returns>
            private static bool TrySplitKeyValue(ReadOnlySpan<char> span, out ReadOnlySpan<char> key, out ReadOnlySpan<char> valueSpan)
            {
                var separatorIndex = FindKeyValueSeparator(span);
                if (separatorIndex <= 0)
                {
                    key = default;
                    valueSpan = default;
                    return false;
                }

                key = span[..separatorIndex].Trim();
                valueSpan = span[(separatorIndex + 1)..].TrimStart();
                return !key.IsEmpty;
            }

            /// <summary>
            /// Locates the unescaped colon character separating keys and values.
            /// </summary>
            /// <param name="span">The string span to search within.</param>
            /// <returns>The index of the colon character, or -1 if not found.</returns>
            private static int FindKeyValueSeparator(ReadOnlySpan<char> span)
            {
                var quote = '\0';

                for (var i = 0; i < span.Length; i++)
                {
                    var c = span[i];
                    if (quote is '\0')
                    {
                        if (c is '"' or '\'')
                        {
                            quote = c;
                            continue;
                        }

                        if (c is ':')
                            return i;

                        continue;
                    }

                    if (quote is '"' && c is '\\')
                    {
                        i++;
                        continue;
                    }

                    if (quote is '\'' && c is '\'' && i + 1 < span.Length && span[i + 1] is '\'')
                    {
                        i++;
                        continue;
                    }

                    if (c == quote)
                        quote = '\0';
                }

                return -1;
            }

            /// <summary>
            /// Returns the length of the span unaffected by inline comments (ignoring the # marking the start of a comment unless it's within quotes).
            /// </summary>
            /// <param name="span">The span to scan.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <returns>The length remaining after stripping the comment.</returns>
            private static int StripComment(ReadOnlySpan<char> span, int lineNumber)
            {
                var quote = '\0';

                for (var i = 0; i < span.Length; i++)
                {
                    var c = span[i];
                    if (quote is '\0')
                    {
                        if (c is '"' or '\'')
                        {
                            quote = c;
                            continue;
                        }

                        if (c is '#' && (i == 0 || char.IsWhiteSpace(span[i - 1])))
                            return i;

                        continue;
                    }

                    if (quote is '"' && c is '\\')
                    {
                        i++;
                        continue;
                    }

                    if (quote is '\'' && c is '\'' && i + 1 < span.Length && span[i + 1] is '\'')
                    {
                        i++;
                        continue;
                    }

                    if (c == quote)
                        quote = '\0';
                }

                if (quote != '\0')
                    throw CreateFormatException(lineNumber, "Quoted scalar is not terminated.");

                return span.Length;
            }

            /// <summary>
            /// Checks if a span corresponds to a sequence item block marker.
            /// </summary>
            /// <param name="span">The span.</param>
            /// <returns><see langword="true"/> if the item represents a sequence item; otherwise, <see langword="false"/>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool IsSequenceItem(ReadOnlySpan<char> span) => span is ['-'] or ['-', ' ', ..];

            /// <summary>
            /// Extracts the sequence item value after the '-' prefix.
            /// </summary>
            /// <param name="span">The original span.</param>
            /// <returns>The extracted sequence item value without the prefix.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static ReadOnlySpan<char> GetSequenceItemValue(ReadOnlySpan<char> span) => span is ['-'] ? default : span[2..].TrimStart();

            /// <summary>
            /// Creates a format exception.
            /// </summary>
            /// <param name="lineNumber">The faulty line number.</param>
            /// <param name="message">The failure reason.</param>
            /// <returns>A formatted exception.</returns>
            private static FormatException CreateFormatException(int lineNumber, string message) => new($"YAML parse error on line {lineNumber}: {message}");

            /// <summary>
            /// Represents a line tokenizer output tracking token position mapping to its content and source file details.
            /// </summary>
            private readonly struct LineToken
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="LineToken"/> struct.
                /// </summary>
                /// <param name="indent">The indentation level.</param>
                /// <param name="offset">The offset in original span.</param>
                /// <param name="length">Length of the mapped token string content.</param>
                /// <param name="lineNumber">Its original line number.</param>
                public LineToken(int indent, int offset, int length, int lineNumber)
                {
                    this.Indent = indent;
                    this.Offset = offset;
                    this.Length = length;
                    this.LineNumber = lineNumber;
                }

                /// <summary>
                /// The physical indentation of the token.
                /// </summary>
                public readonly int Indent;

                /// <summary>
                /// The starting original span offset.
                /// </summary>
                public readonly int Offset;

                /// <summary>
                /// The spanned token length.
                /// </summary>
                public readonly int Length;

                /// <summary>
                /// The original line number for formatting exceptions and tracking positions.
                /// </summary>
                public readonly int LineNumber;
            }
        }
    }
}
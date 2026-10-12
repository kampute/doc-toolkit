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
    /// string keys, scalar values, simple sequences, simple nested mappings based on indentation, block scalars,
    /// and flow sequences and mappings written on a single line (such as <c>[a, b]</c> or <c>{a: 1, b: [x, y]}</c>).
    /// </para>
    /// <note type="important" title="Important">
    /// The parser does not support all YAML features and is designed for simplicity and performance in
    /// common use cases. Anchors, aliases, tags, flow collections that span lines, and single-pair mappings
    /// inside flow sequences (such as <c>[a: b]</c>) are rejected with a <see cref="FormatException"/>. For more
    /// advanced YAML processing needs, consider using a full-featured YAML library.
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
            private readonly List<(int Start, int End)> lines;
            private int index;

            /// <summary>
            /// Initializes a new instance of the <see cref="Parser"/> struct.
            /// </summary>
            /// <param name="sourceSpan">The source span.</param>
            public Parser(ReadOnlySpan<char> sourceSpan)
            {
                source = sourceSpan;
                tokens = [];
                lines = [];
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

                    lines.Add((start, end));

                    var lineSpan = source[start..end];
                    var indent = GetIndent(lineSpan);

                    var contentLen = StripComment(lineSpan[indent..]);
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
                        value = ParseBlockScalar(valueSpan[0], token.Indent, token.LineNumber);
                    else if (!valueSpan.IsEmpty)
                        value = ParseInlineValue(valueSpan, token.LineNumber);
                    else if (HasNestedNode(indent))
                        value = ParseNode(tokens[index].Indent);
                    else
                        value = null;

                    var key = ParseKey(keySpan, token.LineNumber);
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
                        value = ParseBlockScalar(itemSpan[0], token.Indent, token.LineNumber);
                    }
                    else if (itemSpan.Length == 0)
                    {
                        value = index < tokens.Count && tokens[index].Indent > indent
                            ? ParseNode(tokens[index].Indent)
                            : null;
                    }
                    else if (itemSpan[0] is not ('[' or '{') && TrySplitKeyValue(itemSpan, out var keySpan, out var valueSpan))
                    {
                        // The keys of a mapping inside a sequence item are aligned with the first key after the '-' indicator
                        var contentIndent = token.Indent + tokenContent.Length - itemSpan.Length;
                        var key = ParseKey(keySpan, token.LineNumber);
                        var itemMap = new Dictionary<string, object?>(StringComparer.Ordinal);

                        itemMap[key] = valueSpan is ['|' or '>', ..]
                            ? ParseBlockScalar(valueSpan[0], contentIndent, token.LineNumber)
                            : valueSpan.Length > 0
                                ? ParseInlineValue(valueSpan, token.LineNumber)!
                                : HasNestedNode(contentIndent)
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
            /// <param name="headerLineNumber">The line number of the line that introduces the block scalar.</param>
            /// <returns>The block scalar string.</returns>
            /// <remarks>
            /// The content of a block scalar is read from the source lines rather than from the tokens, because empty lines,
            /// indentation beyond the block indentation, and '#' characters are all part of the content.
            /// </remarks>
            private string ParseBlockScalar(char blockType, int parentIndent, int headerLineNumber)
            {
                using var reusable = StringBuilderPool.Shared.GetBuilder();
                var sb = reusable.Builder;

                var literal = blockType == '|';
                var blockIndent = -1;
                var lastLineNumber = headerLineNumber;
                var pendingEmptyLines = 0;

                for (var i = headerLineNumber; i < lines.Count; i++)
                {
                    var line = source[lines[i].Start..lines[i].End];
                    var lineIndent = GetIndent(line);

                    if (lineIndent == line.Length)
                    {
                        pendingEmptyLines++;
                        continue;
                    }

                    if (blockIndent < 0)
                    {
                        if (lineIndent <= parentIndent)
                            break;

                        blockIndent = lineIndent;
                    }
                    else if (lineIndent < blockIndent)
                    {
                        break;
                    }

                    if (sb.Length > 0)
                    {
                        if (literal)
                            sb.Append('\n', pendingEmptyLines + 1);
                        else if (pendingEmptyLines > 0)
                            sb.Append('\n', pendingEmptyLines);
                        else
                            sb.Append(' ');
                    }

                    sb.Append(line[blockIndent..]);
                    pendingEmptyLines = 0;
                    lastLineNumber = i + 1;
                }

                while (index < tokens.Count && tokens[index].LineNumber <= lastLineNumber)
                    index++;

                return sb.ToString().TrimEnd();
            }

            /// <summary>
            /// Determines whether the next token starts a node nested under a key with an empty value.
            /// </summary>
            /// <param name="keyIndent">The indentation of the key.</param>
            /// <returns><see langword="true"/> if the next token is more indented than the key, or is a sequence item aligned with the key; otherwise, <see langword="false"/>.</returns>
            private readonly bool HasNestedNode(int keyIndent)
            {
                if (index >= tokens.Count)
                    return false;

                var next = tokens[index];
                return next.Indent > keyIndent || (next.Indent == keyIndent && IsSequenceItem(source.Slice(next.Offset, next.Length)));
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
            /// Parses a mapping key.
            /// </summary>
            /// <param name="span">The span of the key.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <returns>The key, without quotes when it is a quoted scalar.</returns>
            private static string ParseKey(ReadOnlySpan<char> span, int lineNumber)
            {
                if (span[0] is '[' or '{')
                    throw CreateFormatException(lineNumber, "Flow collections are not supported as mapping keys.");

                ThrowIfNodePropertyOrAlias(span, lineNumber);
                return span[0] is '"' or '\'' ? ParseQuotedValue(span, lineNumber) : span.ToString();
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

                if (span[0] is '[' or '{')
                    return ParseFlowCollection(span, lineNumber);

                ThrowIfNodePropertyOrAlias(span, lineNumber);

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
            /// Parses a flow collection, such as <c>[a, b]</c> or <c>{a: 1, b: 2}</c>, that is written on a single line.
            /// </summary>
            /// <param name="span">The span of the flow collection, including its brackets or braces.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <returns>A list for a flow sequence, or a dictionary for a flow mapping.</returns>
            private static object? ParseFlowCollection(ReadOnlySpan<char> span, int lineNumber)
            {
                var position = 0;
                var collection = ParseFlowNode(span, ref position, lineNumber);

                SkipFlowWhitespace(span, ref position);
                if (position < span.Length)
                    throw CreateFormatException(lineNumber, "Unexpected content after a flow collection.");

                return collection;
            }

            /// <summary>
            /// Parses a node inside a flow collection.
            /// </summary>
            /// <param name="span">The line content.</param>
            /// <param name="position">The position of the node; when this method returns, the position after the node.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <returns>The parsed node.</returns>
            private static object? ParseFlowNode(ReadOnlySpan<char> span, ref int position, int lineNumber)
            {
                return span[position] switch
                {
                    '[' => ParseFlowSequence(span, ref position, lineNumber),
                    '{' => ParseFlowMapping(span, ref position, lineNumber),
                    _ => ParseInlineValue(ReadFlowScalar(span, ref position, lineNumber), lineNumber)
                };
            }

            /// <summary>
            /// Parses a flow sequence.
            /// </summary>
            /// <param name="span">The line content.</param>
            /// <param name="position">The position of the opening bracket; when this method returns, the position after the closing bracket.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <returns>A list of the parsed items.</returns>
            private static List<object?> ParseFlowSequence(ReadOnlySpan<char> span, ref int position, int lineNumber)
            {
                var items = new List<object?>();

                position++;
                while (true)
                {
                    SkipFlowWhitespace(span, ref position);
                    if (position >= span.Length)
                        throw CreateFormatException(lineNumber, "A flow sequence must end with ']' on the same line.");

                    if (span[position] is ']')
                    {
                        position++;
                        return items;
                    }

                    items.Add(ParseFlowNode(span, ref position, lineNumber));

                    if (!SkipFlowEntrySeparator(span, ref position, ']'))
                        throw CreateFormatException(lineNumber, "A flow sequence entry must be followed by ',' or ']'.");
                }
            }

            /// <summary>
            /// Parses a flow mapping.
            /// </summary>
            /// <param name="span">The line content.</param>
            /// <param name="position">The position of the opening brace; when this method returns, the position after the closing brace.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <returns>A dictionary of the parsed entries.</returns>
            private static Dictionary<string, object?> ParseFlowMapping(ReadOnlySpan<char> span, ref int position, int lineNumber)
            {
                var map = new Dictionary<string, object?>(StringComparer.Ordinal);

                position++;
                while (true)
                {
                    SkipFlowWhitespace(span, ref position);
                    if (position >= span.Length)
                        throw CreateFormatException(lineNumber, "A flow mapping must end with '}' on the same line.");

                    if (span[position] is '}')
                    {
                        position++;
                        return map;
                    }

                    var key = ParseKey(ReadFlowScalar(span, ref position, lineNumber), lineNumber);

                    // A key without ':' has a null value
                    object? value = null;
                    SkipFlowWhitespace(span, ref position);
                    if (position < span.Length && span[position] is ':')
                    {
                        position++;
                        SkipFlowWhitespace(span, ref position);
                        if (position < span.Length && span[position] is not (',' or '}'))
                            value = ParseFlowNode(span, ref position, lineNumber);
                    }

                    if (!map.TryAdd(key, value))
                        throw CreateFormatException(lineNumber, $"Duplicate key '{key}'.");

                    if (!SkipFlowEntrySeparator(span, ref position, '}'))
                        throw CreateFormatException(lineNumber, "A flow mapping entry must be followed by ',' or '}'.");
                }
            }

            /// <summary>
            /// Reads a quoted or plain scalar inside a flow collection.
            /// </summary>
            /// <param name="span">The line content.</param>
            /// <param name="position">The position of the scalar; when this method returns, the position after the scalar.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <returns>The span of the scalar, including its quotes if it is quoted.</returns>
            /// <remarks>
            /// A plain scalar ends at ',', ']', '}', or at a ':' that is followed by whitespace, by one of these characters, or by the
            /// end of the line.
            /// </remarks>
            private static ReadOnlySpan<char> ReadFlowScalar(ReadOnlySpan<char> span, ref int position, int lineNumber)
            {
                var start = position;
                var quote = span[position];
                if (quote is '"' or '\'')
                {
                    for (position++; position < span.Length; position++)
                    {
                        var c = span[position];
                        if (quote is '"' && c is '\\')
                        {
                            position++;
                        }
                        else if (c == quote)
                        {
                            if (quote is '\'' && position + 1 < span.Length && span[position + 1] is '\'')
                            {
                                position++;
                                continue;
                            }

                            return span[start..++position];
                        }
                    }

                    throw CreateFormatException(lineNumber, "Quoted scalar is not terminated.");
                }

                for (; position < span.Length; position++)
                {
                    var c = span[position];
                    if (c is ',' or ']' or '}')
                        break;

                    if (c is '[' or '{')
                        throw CreateFormatException(lineNumber, $"A plain scalar in a flow collection cannot contain '{c}'.");

                    if (c is ':' && (position + 1 == span.Length || span[position + 1] is ',' or ']' or '}' || char.IsWhiteSpace(span[position + 1])))
                        break;
                }

                var scalar = span[start..position].TrimEnd();
                if (scalar.IsEmpty)
                    throw CreateFormatException(lineNumber, "A flow collection entry cannot be empty.");

                return scalar;
            }

            /// <summary>
            /// Skips the separator after an entry of a flow collection.
            /// </summary>
            /// <param name="span">The line content.</param>
            /// <param name="position">The position after the entry; when this method returns, the position after the separator.</param>
            /// <param name="closing">The closing character of the flow collection.</param>
            /// <returns><see langword="true"/> if the entry is followed by ',' or by the closing character; otherwise, <see langword="false"/>.</returns>
            private static bool SkipFlowEntrySeparator(ReadOnlySpan<char> span, ref int position, char closing)
            {
                SkipFlowWhitespace(span, ref position);
                if (position < span.Length && span[position] is ',')
                {
                    position++;
                    return true;
                }

                // The closing character itself is consumed by the caller
                return position >= span.Length || span[position] == closing;
            }

            /// <summary>
            /// Skips whitespace inside a flow collection.
            /// </summary>
            /// <param name="span">The line content.</param>
            /// <param name="position">The current position; when this method returns, the position of the next non-whitespace character.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void SkipFlowWhitespace(ReadOnlySpan<char> span, ref int position)
            {
                while (position < span.Length && char.IsWhiteSpace(span[position]))
                    position++;
            }

            /// <summary>
            /// Throws a <see cref="FormatException"/> if the specified node starts with an anchor, a tag, or an alias, which are not supported.
            /// </summary>
            /// <param name="span">The span of the node.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <exception cref="FormatException">Thrown when the node starts with an anchor, a tag, or an alias.</exception>
            private static void ThrowIfNodePropertyOrAlias(ReadOnlySpan<char> span, int lineNumber)
            {
                switch (span[0])
                {
                    case '&':
                        throw CreateFormatException(lineNumber, "Anchors are not supported.");
                    case '*':
                        throw CreateFormatException(lineNumber, "Aliases are not supported.");
                    case '!':
                        throw CreateFormatException(lineNumber, "Tags are not supported.");
                }
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
                        if (c is '"' or '\'' && IsQuotedScalarStart(span, i))
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
            /// <returns>The length remaining after stripping the comment.</returns>
            private static int StripComment(ReadOnlySpan<char> span)
            {
                var quote = '\0';
                var flowDepth = 0;

                for (var i = 0; i < span.Length; i++)
                {
                    var c = span[i];
                    if (quote is '\0')
                    {
                        if (c is '"' or '\'' or '[' or '{' && (flowDepth > 0 ? IsFlowNodeStart(span, i) : IsQuotedScalarStart(span, i)))
                        {
                            if (c is '[' or '{')
                                flowDepth++;
                            else
                                quote = c;
                            continue;
                        }

                        if (c is ']' or '}' && flowDepth > 0)
                        {
                            flowDepth--;
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

                return span.Length;
            }

            /// <summary>
            /// Determines whether a quote character at the specified position starts a quoted scalar.
            /// </summary>
            /// <param name="span">The line content.</param>
            /// <param name="position">The position of the quote character.</param>
            /// <returns>
            /// <see langword="true"/> if the quote starts a key, a value, or a sequence item; otherwise, <see langword="false"/>
            /// because the quote is part of a plain scalar.
            /// </returns>
            private static bool IsQuotedScalarStart(ReadOnlySpan<char> span, int position)
            {
                if (position == 0)
                    return true;

                if (!char.IsWhiteSpace(span[position - 1]))
                    return false;

                var preceding = span[..position].TrimEnd();
                return preceding is [.., ':'] or ['-'] or [.., ' ' or '\t', '-'];
            }

            /// <summary>
            /// Determines whether a character inside a flow collection starts a node.
            /// </summary>
            /// <param name="span">The line content.</param>
            /// <param name="position">The position of the character.</param>
            /// <returns><see langword="true"/> if the character follows the start of the collection, a ',' or a ':'; otherwise, <see langword="false"/>.</returns>
            private static bool IsFlowNodeStart(ReadOnlySpan<char> span, int position)
                => span[..position].TrimEnd() is [.., '[' or '{' or ',' or ':'];

            /// <summary>
            /// Gets the number of leading whitespace characters of a line.
            /// </summary>
            /// <param name="line">The line.</param>
            /// <returns>The number of leading whitespace characters.</returns>
            private static int GetIndent(ReadOnlySpan<char> line)
            {
                var indent = 0;
                while (indent < line.Length && char.IsWhiteSpace(line[indent]))
                    indent++;
                return indent;
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
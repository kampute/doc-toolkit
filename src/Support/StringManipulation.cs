// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides extension methods for strings to manipulate text.
    /// </summary>
    public static class StringManipulation
    {
        private const int MaxStackAllocSize = 256;

        /// <summary>
        /// Returns the section of a string after the first occurrence of the specified character.
        /// </summary>
        /// <param name="text">The string to process.</param>
        /// <param name="separator">The separator character.</param>
        /// <returns>The part of the substring after the first occurrence of the separator character or an empty string if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringAfter(this string text, char separator)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var index = text.IndexOf(separator);
            return index != -1 ? text[(index + 1)..] : string.Empty;
        }

        /// <summary>
        /// Returns the section of a string before the first occurrence of the specified character.
        /// </summary>
        /// <param name="text">The string to process.</param>
        /// <param name="separator">The separator character.</param>
        /// <returns>The part of the substring before the first occurrence of the separator character or an empty string if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringBefore(this string text, char separator)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var index = text.IndexOf(separator);
            return index != -1 ? text[..index] : string.Empty;
        }

        /// <summary>
        /// Returns the section of a string after the last occurrence of the specified character.
        /// </summary>
        /// <param name="text">The string to process.</param>
        /// <param name="separator">The separator character.</param>
        /// <returns>The part of the substring after the last occurrence of the separator character or the an empty string if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringAfterLast(this string text, char separator)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var index = text.LastIndexOf(separator);
            return index != -1 ? text[(index + 1)..] : string.Empty;
        }

        /// <summary>
        /// Returns the section of a string before the last occurrence of the specified character.
        /// </summary>
        /// <param name="text">The string to process.</param>
        /// <param name="separator">The separator character.</param>
        /// <returns>The part of the substring before the last occurrence of the separator character or the an empty string if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringBeforeLast(this string text, char separator)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var index = text.LastIndexOf(separator);
            return index != -1 ? text[..index] : string.Empty;
        }

        /// <summary>
        /// Returns the section of a string after the first occurrence of the specified character.
        /// </summary>
        /// <param name="text">The string to process.</param>
        /// <param name="separator">The separator character.</param>
        /// <returns>The part of the substring after the first occurrence of the separator character or the entire string if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringAfterOrSelf(this string text, char separator)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var index = text.IndexOf(separator);
            return index != -1 ? text[(index + 1)..] : text;
        }

        /// <summary>
        /// Returns the section of a string before the first occurrence of the specified character.
        /// </summary>
        /// <param name="text">The string to process.</param>
        /// <param name="separator">The separator character.</param>
        /// <returns>The part of the substring before the first occurrence of the separator character or the entire string if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringBeforeOrSelf(this string text, char separator)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var index = text.IndexOf(separator);
            return index != -1 ? text[..index] : text;
        }

        /// <summary>
        /// Returns the section of a string after the last occurrence of the specified character.
        /// </summary>
        /// <param name="text">The string to process.</param>
        /// <param name="separator">The separator character.</param>
        /// <returns>The part of the substring after the last occurrence of the separator character or the entire string if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringAfterLastOrSelf(this string text, char separator)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var index = text.LastIndexOf(separator);
            return index != -1 ? text[(index + 1)..] : text;
        }

        /// <summary>
        /// Returns the section of a string before the last occurrence of the specified character.
        /// </summary>
        /// <param name="text">The string to process.</param>
        /// <param name="separator">The separator character.</param>
        /// <returns>The part of the substring before the last occurrence of the separator character or the entire string if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SubstringBeforeLastOrSelf(this string text, char separator)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var index = text.LastIndexOf(separator);
            return index != -1 ? text[..index] : text;
        }

        /// <summary>
        /// Splits a string into two parts at the first occurrence of the specified separator character.
        /// </summary>
        /// <param name="text">The string to split.</param>
        /// <param name="separator">The character to use as the separator for splitting the string.</param>
        /// <returns>A tuple containing two substrings:
        ///   <list type="number">
        ///     <item>The substring before the first occurrence of the separator, or the entire string if the separator is not found.</item>
        ///     <item>The substring after the first occurrence of the separator, or an empty string if the separator is not found.</item>
        ///   </list>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (string, string) SplitFirst(this string text, char separator)
        {
            if (string.IsNullOrEmpty(text))
                return (string.Empty, string.Empty);

            var index = text.IndexOf(separator);
            return index != -1
                ? (text[..index], text[(index + 1)..])
                : (text, string.Empty);
        }

        /// <summary>
        /// Splits a string into two parts at the last occurrence of the specified separator character.
        /// </summary>
        /// <param name="text">The string to split.</param>
        /// <param name="separator">The character to use as the separator for splitting the string.</param>
        /// <returns>A tuple containing two substrings:
        ///   <list type="number">
        ///     <item>The substring before the last occurrence of the separator, or an empty string if the separator is not found.</item>
        ///     <item>The substring after the last occurrence of the separator, or the entire string if the separator is not found.</item>
        ///   </list>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (string, string) SplitLast(this string text, char separator)
        {
            if (string.IsNullOrEmpty(text))
                return (string.Empty, string.Empty);

            var index = text.LastIndexOf(separator);
            return index != -1
                ? (text[..index], text[(index + 1)..])
                : (string.Empty, text);
        }

        /// <summary>
        /// Replaces multiple characters in the given string with a single character.
        /// </summary>
        /// <param name="text">The string to process.</param>
        /// <param name="charsToReplace">The characters to replace.</param>
        /// <param name="replacement">The character to replace the old characters with.</param>
        /// <param name="skipConsecutiveReplacements">Indicates whether to skip consecutive occurrences of the replacement character.</param>
        /// <returns>A new string with the specified characters replaced.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
        public static string ReplaceChars(this string text, ReadOnlySpan<char> charsToReplace, char replacement, bool skipConsecutiveReplacements = false)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            var size = text.Length;
            if (size == 0 || charsToReplace.IsEmpty)
                return text;

            var result = size <= MaxStackAllocSize ? stackalloc char[size] : new char[size];

            var index = 0;
            var modified = false;
            var textSpan = text.AsSpan();
            for (var i = 0; i < size; ++i)
            {
                var c = textSpan[i];
                if (charsToReplace.IndexOf(c) == -1)
                {
                    result[index++] = c;
                    continue;
                }

                if (!skipConsecutiveReplacements || index == 0 || result[index - 1] != replacement)
                {
                    result[index++] = replacement;
                    modified = true;
                }
            }

            return modified ? new string(result[..index]) : text;
        }

        /// <summary>
        /// Removes multiple characters from the given string.
        /// </summary>
        /// <param name="text">The string to process.</param>
        /// <param name="charsToRemove">The characters to remove.</param>
        /// <returns>A new string with the specified characters removed.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
        public static string RemoveChars(this string text, ReadOnlySpan<char> charsToRemove)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            var size = text.Length;
            if (size == 0 || charsToRemove.IsEmpty)
                return text;

            var result = size <= MaxStackAllocSize ? stackalloc char[size] : new char[size];

            var index = 0;
            var textSpan = text.AsSpan();
            for (var i = 0; i < size; ++i)
            {
                var c = textSpan[i];
                if (charsToRemove.IndexOf(c) == -1)
                    result[index++] = c;
            }

            return index < size ? new string(result[..index]) : text;
        }

        /// <summary>
        /// Translates multiple characters in the given string to corresponding characters.
        /// </summary>
        /// <param name="text">The string to process.</param>
        /// <param name="fromChars">The characters to translate from.</param>
        /// <param name="toChars">The characters to translate to.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the lengths of <paramref name="fromChars"/> and <paramref name="toChars"/> are not the same.</exception>
        public static string TranslateChars(this string text, ReadOnlySpan<char> fromChars, ReadOnlySpan<char> toChars)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));
            if (fromChars.Length != toChars.Length)
                throw new ArgumentException("The lengths of fromChars and toChars must be the same.", nameof(toChars));

            var size = text.Length;
            if (size == 0 || fromChars.IsEmpty)
                return text;

            var result = size <= MaxStackAllocSize ? stackalloc char[size] : new char[size];

            var modified = false;
            var textSpan = text.AsSpan();
            for (var i = 0; i < size; ++i)
            {
                var c = textSpan[i];
                var mappingIndex = fromChars.IndexOf(c);
                if (mappingIndex != -1)
                {
                    c = toChars[mappingIndex];
                    modified = true;
                }
                result[i] = c;
            }

            return modified ? new string(result) : text;
        }

        /// <summary>
        /// Converts a string to title case, where the first letter of each word is capitalized.
        /// </summary>
        /// <param name="text">The string to convert.</param>
        /// <returns>A new string in title case.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> is <see langword="null"/>.</exception>
        public static string ToTitleCase(this string text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            var size = text.Length;
            if (size == 0)
                return string.Empty;

            var maxSize = size * 2;
            var result = maxSize <= MaxStackAllocSize ? stackalloc char[maxSize] : new char[maxSize];

            var index = 0;
            var textSpan = text.AsSpan();
            foreach (var (range, isAcronym) in TextUtility.SplitWords(text))
            {
                if (index != 0)
                    result[index++] = ' ';

                var word = textSpan[range];
                if (isAcronym)
                {
                    for (var i = 0; i < word.Length; ++i)
                        result[index++] = word[i];
                }
                else
                {
                    result[index++] = char.ToUpper(word[0]);
                    for (var i = 1; i < word.Length; ++i)
                        result[index++] = char.ToLower(word[i]);
                }
            }

            return new string(result[..index]);
        }
    }
}

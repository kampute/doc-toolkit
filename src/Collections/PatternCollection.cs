// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a collection of patterns that can be used to match string values using exact matches and wildcard patterns.
    /// </summary>
    /// <remarks>
    /// The <see cref="PatternCollection"/> class maintains three types of patterns:
    /// <list type="bullet">
    ///   <item><term>Exact matches</term><description>Patterns that must match exactly (e.g., "System.Text")</description></item>
    ///   <item><term>Wildcard patterns</term><description>Patterns ending with a separator followed by a wildcard character (e.g., "System.*")</description></item>
    ///   <item><term>Universal match</term><description>The single wildcard character "*" that matches all non-empty values</description></item>
    /// </list>
    /// The matching is case-sensitive and follows these rules:
    /// <list type="bullet">
    ///   <item><description>For exact patterns, the value must match exactly</description></item>
    ///   <item><description>For wildcard patterns, the value must start with the pattern before the wildcard and be followed by either nothing or the separator character</description></item>
    ///   <item><description>The universal match pattern "*" matches any non-null and non-empty value</description></item>
    /// </list>
    /// When searching for matching patterns, exact matches take precedence over wildcard patterns, and longer wildcard patterns take precedence over shorter ones.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public class PatternCollection : IReadOnlyPatternCollection, ICollection<string>
    {
        private static readonly IComparer<string> StringLengthComparer = Comparer<string>.Create(static (x, y) => x.Length.CompareTo(y.Length));

        private readonly HashSet<string> exactMatches = [];
        private readonly List<string> wildcardPatterns = [];
        private bool matchingAll;

        /// <summary>
        /// The wildcard character used in patterns.
        /// </summary>
        public const char Wildcard = '*';

        /// <summary>
        /// Initializes a new instance of the <see cref="PatternCollection"/> class with the specified separator character.
        /// </summary>
        /// <param name="separator">The character used to separate segments.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="separator"/> is the wildcard character.</exception>
        public PatternCollection(char separator)
        {
            if (separator == Wildcard)
                throw new ArgumentException("Separator cannot be the wildcard character.", nameof(separator));

            Separator = separator;
        }

        /// <summary>
        /// Gets the character used to separate segments in patterns.
        /// </summary>
        /// <value>
        /// The character used to separate segments in patterns.
        /// </value>
        public char Separator { get; }

        /// <summary>
        /// Gets a value indicating whether the collection matches all non-empty values.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the collection matches all non-empty values; otherwise, <see langword="false"/>.
        /// </value>
        public bool MatchesAll => matchingAll;

        /// <summary>
        /// Gets the number of patterns in the collection.
        /// </summary>
        /// <value>
        /// The number of patterns in the collection.
        /// </value>
        public int Count => exactMatches.Count + wildcardPatterns.Count + (matchingAll ? 1 : 0);

        /// <summary>
        /// Adds a pattern to the collection if it does not already exist.
        /// </summary>
        /// <param name="pattern">The pattern to add to the collection.</param>
        /// <returns><see langword="true"/> if the pattern was added; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="pattern"/> is <see langword="null"/>, empty, or invalid.</exception>
        /// <remarks>
        /// The pattern can be one of three types:
        /// <list type="bullet">
        ///   <item><term>Exact pattern</term><description>A pattern without any wildcard characters</description></item>
        ///   <item><term>Wildcard pattern</term><description>A pattern ending with the separator character followed by the wildcard character</description></item>
        ///   <item><term>Universal match</term><description>A single wildcard character that matches all values</description></item>
        /// </list>
        /// </remarks>
        public virtual bool Add(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException("Pattern cannot be null or empty.", nameof(pattern));

            if (IsUniversalMatch(pattern))
            {
                if (matchingAll)
                    return false;

                matchingAll = true;
                return true;
            }

            var wildcardIndex = pattern.IndexOf(Wildcard);
            if (wildcardIndex == -1)
                return exactMatches.Add(pattern);

            if (wildcardIndex != pattern.Length - 1 || pattern[^2] != Separator)
                throw new ArgumentException("A Pattern may only contain a single wildcard character as the last segment.", nameof(pattern));

            var patternWithoutWildcard = pattern[..^2];
            if (wildcardPatterns.Contains(patternWithoutWildcard))
                return false;

            var index = wildcardPatterns.BinarySearch(patternWithoutWildcard, StringLengthComparer);
            wildcardPatterns.Insert(index < 0 ? ~index : index, patternWithoutWildcard);
            return true;
        }

        /// <summary>
        /// Removes the specified pattern from the collection.
        /// </summary>
        /// <param name="pattern">The pattern to remove.</param>
        /// <returns><see langword="true"/> if the pattern was removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pattern"/> is <see langword="null"/>.</exception>
        public virtual bool Remove(string pattern)
        {
            if (pattern is null)
                throw new ArgumentNullException(nameof(pattern));

            if (IsUniversalMatch(pattern))
            {
                if (!matchingAll)
                    return false;

                matchingAll = false;
                return true;
            }

            return pattern.EndsWith(Wildcard)
                ? wildcardPatterns.Remove(pattern[..^2])
                : exactMatches.Remove(pattern);
        }

        /// <summary>
        /// Clears all patterns from the collection.
        /// </summary>
        public virtual void Clear()
        {
            exactMatches.Clear();
            wildcardPatterns.Clear();
            matchingAll = false;
        }

        /// <summary>
        /// Determines whether the collection contains the specified pattern.
        /// </summary>
        /// <param name="pattern">The pattern to check for.</param>
        /// <returns><see langword="true"/> if the collection contains the specified pattern; otherwise, <see langword="false"/>.</returns>
        public bool Contains(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return false;

            if (IsUniversalMatch(pattern))
                return matchingAll;

            return pattern.EndsWith(Wildcard)
                ? wildcardPatterns.Contains(pattern[..^2])
                : exactMatches.Contains(pattern);
        }

        /// <summary>
        /// Attempts to retrieve a pattern that matches the specified value.
        /// </summary>
        /// <param name="value">The value to find a pattern for.</param>
        /// <param name="pattern">
        /// When this method returns, contains the pattern that matches the value, if found; otherwise, <see langword="null"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true"/> if a pattern was found; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method searches for the most specific pattern that matches the value. If the value is an exact match for a pattern
        /// in the collection, that pattern is returned. If the value is a sub-segment of a wildcard pattern, the wildcard pattern
        /// is returned. If no pattern matches the value, <see langword="false"/> is returned.
        /// </remarks>
        public bool TryGetMatchingPattern(string value, [NotNullWhen(true)] out string? pattern)
        {
            if (string.IsNullOrEmpty(value))
            {
                pattern = null;
                return false;
            }

            if (exactMatches.Contains(value))
            {
                pattern = value;
                return true;
            }

            for (var i = wildcardPatterns.Count - 1; i >= 0; --i)
            {
                var p = wildcardPatterns[i];
                if (value.Length < p.Length)
                    continue;
                if (value.Length > p.Length && value[p.Length] != Separator)
                    continue;
                if (!value.StartsWith(p, StringComparison.Ordinal))
                    continue;

                pattern = p + Separator + Wildcard;
                return true;
            }

            if (matchingAll)
            {
                pattern = Wildcard.ToString();
                return true;
            }

            pattern = null;
            return false;
        }

        /// <summary>
        /// Determines whether the specified value matches any pattern in the collection.
        /// </summary>
        /// <param name="value">The value to check for.</param>
        /// <returns><see langword="true"/> if the collection contains a pattern that matches the value; otherwise, <see langword="false"/>.</returns>
        public bool Matches(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            if (matchingAll || exactMatches.Contains(value))
                return true;

            foreach (var pattern in wildcardPatterns)
            {
                if (value.Length < pattern.Length)
                    return false;
                if (value.Length > pattern.Length && value[pattern.Length] != Separator)
                    continue;
                if (value.StartsWith(pattern, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the patterns in the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the patterns in the collection.</returns>
        public IEnumerator<string> GetEnumerator()
        {
            foreach (var pattern in exactMatches)
                yield return pattern;

            if (matchingAll)
                yield return Wildcard.ToString();

            foreach (var pattern in wildcardPatterns)
                yield return pattern + Separator + Wildcard;
        }

        /// <summary>
        /// Determines whether the specified pattern is a universal match pattern.
        /// </summary>
        /// <param name="pattern">The pattern to check.</param>
        /// <returns><see langword="true"/> if the pattern is a universal match; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// A universal match pattern is a single wildcard character (*) that matches any non-empty value.
        /// </remarks>
        public static bool IsUniversalMatch(ReadOnlySpan<char> pattern) => pattern.Length == 1 && pattern[0] == Wildcard;

        #region Explicit Interface Implementations

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        /// <value>
        /// Always <see langword="false"/> as the collection is not read-only.
        /// </value>
        bool ICollection<string>.IsReadOnly => false;

        /// <summary>
        /// Adds a pattern to the collection.
        /// </summary>
        /// <param name="item">The pattern to add to the collection.</param>
        void ICollection<string>.Add(string item) => Add(item);

        /// <summary>
        /// Copies the patterns in the collection to the specified array starting at the specified index.
        /// </summary>
        /// <param name="array">The array to copy the patterns to.</param>
        /// <param name="arrayIndex">The index in the array at which to start copying.</param>
        /// <exception cref="ArgumentNullException">Thrown when the array is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is less than 0.</exception>
        /// <exception cref="ArgumentException">Thrown when the array is too small to contain all patterns.</exception>
        void ICollection<string>.CopyTo(string[] array, int arrayIndex)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("Array is too small");

            var i = arrayIndex;
            foreach (var item in this)
                array[i++] = item;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the patterns in the collection.
        /// </summary>
        /// <returns>An enumerator that iterates through the patterns in the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}

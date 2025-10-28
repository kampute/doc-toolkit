// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Collections
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a read-only collection of patterns that can be used to match values.
    /// </summary>
    /// <remarks>
    /// The <see cref="IReadOnlyPatternCollection"/> interface provides a consistent way to access and use pattern collections
    /// for string matching operations. Pattern collections are particularly useful in documentation generation for filtering
    /// and matching namespaces, type names, and other string-based identifiers.
    /// <para>
    /// This interface supports exact matches as well as wildcard patterns, allowing for flexible inclusion and exclusion rules
    /// when organizing documentation. The matching capabilities enable documentation tools to determine which elements should be
    /// included in generated documentation or how elements should be categorized based on their names or namespaces.
    /// </para>
    /// </remarks>
    public interface IReadOnlyPatternCollection : IReadOnlyCollection<string>
    {
        /// <summary>
        /// Determines whether the collection contains the specified namespace pattern.
        /// </summary>
        /// <param name="pattern">The namespace to check for.</param>
        /// <returns><see langword="true"/> if the collection contains a pattern that matches the namespace; otherwise, <see langword="false"/>.</returns>
        bool Contains(string pattern);

        /// <summary>
        /// Determines whether the specified value matches any pattern in the collection.
        /// </summary>
        /// <param name="value">The value to check for.</param>
        /// <returns><see langword="true"/> if the collection contains a pattern that matches the value; otherwise, <see langword="false"/>.</returns>
        bool Matches(string value);

        /// <summary>
        /// Attempts to retrieve a pattern that matches the specified value.
        /// </summary>
        /// <param name="value">The value to find a pattern for.</param>
        /// <param name="pattern">
        /// When this method returns, contains the pattern that matches the value, if found; otherwise, <see langword="null"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true"/> if a pattern was found; otherwise, <see langword="false"/>.</returns>
        bool TryGetMatchingPattern(string value, [NotNullWhen(true)] out string? pattern);
    }
}

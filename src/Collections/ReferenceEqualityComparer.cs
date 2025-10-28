// Copyright (C) 2019-2025 Kampute
//
// This file is part of the Kampute.DocToolkit package and is released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Collections
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides an equality comparer that uses reference equality (<see cref="object.ReferenceEquals(object, object)"/>)
    /// instead of value equality.
    /// </summary>
    /// <typeparam name="T">The type of objects to compare.</typeparam>
    /// <remarks>
    /// This comparer determines equality based on object identity rather than content. Two object references are considered
    /// equal only if they refer to the same instance in memory.
    /// <para>
    /// Hash codes are computed using <see cref="RuntimeHelpers.GetHashCode(object)"/>, which returns an identity-based hash
    /// code rather than a content-based one.
    /// </para>
    /// </remarks>
    public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="ReferenceEqualityComparer{T}"/>.
        /// </summary>
        /// <value>
        /// The singleton instance of the <see cref="ReferenceEqualityComparer{T}"/> class.
        /// </value>
        public static ReferenceEqualityComparer<T> Instance { get; } = new ReferenceEqualityComparer<T>();

        /// <summary>
        /// Prevents a default instance of the <see cref="ReferenceEqualityComparer{T}"/> class from being created.
        /// </summary>
        private ReferenceEqualityComparer()
        {
        }

        /// <summary>
        /// Determines whether two object references refer to the same instance.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="x"/> and <paramref name="y"/> refer to the same instance or if both are
        /// <see langword="null"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

        /// <summary>
        /// Returns an identity-based hash code for the specified object.
        /// </summary>
        /// <param name="obj">The object for which to get a hash code.</param>
        /// <returns>
        /// An identity-based hash code for <paramref name="obj"/>, or zero if <paramref name="obj"/> is <see langword="null"/>.
        /// </returns>
        public int GetHashCode(T? obj) => obj is null ? 0 : RuntimeHelpers.GetHashCode(obj);
    }
}

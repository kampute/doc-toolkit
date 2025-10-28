// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides extension methods for <see cref="ReadOnlySpan{T}"/> to slice the span based on a separator.
    /// </summary>
    public static class ReadOnlySpanSlicing
    {
        /// <summary>
        /// Gets a slice of the span after the first occurrence of the specified separator.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the span.</typeparam>
        /// <param name="span">The read-only span to slice.</param>
        /// <param name="separator">The separator to find in the span.</param>
        /// <returns>A read-only span that starts after the first occurrence of the separator, or an empty span if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> SliceAfter<T>(this ReadOnlySpan<T> span, T separator)
            where T : IEquatable<T>
        {
            var index = span.IndexOf(separator);
            return index != -1 ? span[(index + 1)..] : [];
        }

        /// <summary>
        /// Gets a slice of the span before the first occurrence of the specified separator.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the span.</typeparam>
        /// <param name="span">The read-only span to slice.</param>
        /// <param name="separator">The separator to find in the span.</param>
        /// <returns>A read-only span that ends before the first occurrence of the separator, or an empty span if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> SliceBefore<T>(this ReadOnlySpan<T> span, T separator)
            where T : IEquatable<T>
        {
            var index = span.IndexOf(separator);
            return index != -1 ? span[..index] : [];
        }

        /// <summary>
        /// Gets a slice of the span after the last occurrence of the specified separator.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the span.</typeparam>
        /// <param name="span">The read-only span to slice.</param>
        /// <param name="separator">The separator to find in the span.</param>
        /// <returns>A read-only span that starts after the last occurrence of the separator, or an empty span if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> SliceAfterLast<T>(this ReadOnlySpan<T> span, T separator)
            where T : IEquatable<T>
        {
            var index = span.LastIndexOf(separator);
            return index != -1 ? span[(index + 1)..] : [];
        }

        /// <summary>
        /// Gets a slice of the span before the last occurrence of the specified separator.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the span.</typeparam>
        /// <param name="span">The read-only span to slice.</param>
        /// <param name="separator">The separator to find in the span.</param>
        /// <returns>A read-only span that ends before the last occurrence of the separator, or an empty span if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> SliceBeforeLast<T>(this ReadOnlySpan<T> span, T separator)
            where T : IEquatable<T>
        {
            var index = span.LastIndexOf(separator);
            return index != -1 ? span[..index] : [];
        }

        /// <summary>
        /// Gets a slice of the span after the first occurrence of the specified separator.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the span.</typeparam>
        /// <param name="span">The read-only span to slice.</param>
        /// <param name="separator">The separator to find in the span.</param>
        /// <returns>A read-only span that starts after the first occurrence of the separator, or the original span if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> SliceAfterOrSelf<T>(this ReadOnlySpan<T> span, T separator)
            where T : IEquatable<T>
        {
            var index = span.IndexOf(separator);
            return index != -1 ? span[(index + 1)..] : span;
        }

        /// <summary>
        /// Gets a slice of the span before the first occurrence of the specified separator.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the span.</typeparam>
        /// <param name="span">The read-only span to slice.</param>
        /// <param name="separator">The separator to find in the span.</param>
        /// <returns>A read-only span that ends before the first occurrence of the separator, or the original span if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> SliceBeforeOrSelf<T>(this ReadOnlySpan<T> span, T separator)
            where T : IEquatable<T>
        {
            var index = span.IndexOf(separator);
            return index != -1 ? span[..index] : span;
        }

        /// <summary>
        /// Gets a slice of the span after the last occurrence of the specified separator.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the span.</typeparam>
        /// <param name="span">The read-only span to slice.</param>
        /// <param name="separator">The separator to find in the span.</param>
        /// <returns>A read-only span that starts after the last occurrence of the separator, or the original span if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> SliceAfterLastOrSelf<T>(this ReadOnlySpan<T> span, T separator)
            where T : IEquatable<T>
        {
            var index = span.LastIndexOf(separator);
            return index != -1 ? span[(index + 1)..] : span;
        }

        /// <summary>
        /// Gets a slice of the span before the last occurrence of the specified separator.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the span.</typeparam>
        /// <param name="span">The read-only span to slice.</param>
        /// <param name="separator">The separator to find in the span.</param>
        /// <returns>A read-only span that ends before the last occurrence of the separator, or the original span if the separator is not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<T> SliceBeforeLastOrSelf<T>(this ReadOnlySpan<T> span, T separator)
            where T : IEquatable<T>
        {
            var index = span.LastIndexOf(separator);
            return index != -1 ? span[..index] : span;
        }
    }
}

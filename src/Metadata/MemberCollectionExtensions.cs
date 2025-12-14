// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides extension methods for member collections.
    /// </summary>
    /// <threadsafety static="true"/>
    public static class MemberCollectionExtensions
    {
        /// <summary>
        /// Orders types by their full names.
        /// </summary>
        /// <typeparam name="T">The type of types in the sequence.</typeparam>
        /// <param name="source">The source types to order.</param>
        /// <returns>An ordered sequence of types.</returns>
        /// <seealso cref="FindByFullName{T}(IReadOnlyList{T}, string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> OrderByFullName<T>(this IEnumerable<T> source) where T : IType => source
            .OrderBy(type => type.FullName, StringComparer.Ordinal);

        /// <summary>
        /// Orders constructors by their parameter count.
        /// </summary>
        /// <param name="source">The source constructors to order.</param>
        /// <returns>An ordered sequence of constructors.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<IConstructor> OrderByParameterCount(this IEnumerable<IConstructor> source) => source
            .OrderBy(c => c.Parameters.Count);

        /// <summary>
        /// Orders operators by their names, then by parameter count.
        /// </summary>
        /// <param name="source">The source operators to order.</param>
        /// <returns>An ordered sequence of operators.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<IOperator> OrderByName(this IEnumerable<IOperator> source) => source
            .OrderBy(op => op.Name, StringComparer.Ordinal)
            .ThenBy(op => op.Parameters.Count);

        /// <summary>
        /// Orders methods by their names, then by type parameter count, then by parameter count.
        /// </summary>
        /// <param name="source">The source methods to order.</param>
        /// <returns>An ordered sequence of methods.</returns>
        /// <seealso cref="FindByName{T}(IReadOnlyList{T}, string)"/>
        /// <seealso cref="FindIndexByName{T}(IReadOnlyList{T}, string)"/>
        /// <seealso cref="WhereName{T}(IReadOnlyList{T}, string, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<IMethod> OrderByName(this IEnumerable<IMethod> source) => source
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ThenBy(m => m.TypeParameters.Count)
            .ThenBy(m => m.Parameters.Count);

        /// <summary>
        /// Orders properties by their names, then by index parameter count.
        /// </summary>
        /// <param name="source">The source properties to order.</param>
        /// <returns>An ordered sequence of properties.</returns>
        /// <seealso cref="FindByName{T}(IReadOnlyList{T}, string)"/>
        /// <seealso cref="FindIndexByName{T}(IReadOnlyList{T}, string)"/>
        /// <seealso cref="WhereName{T}(IReadOnlyList{T}, string, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<IProperty> OrderByName(this IEnumerable<IProperty> source) => source
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ThenBy(p => p.Parameters.Count);

        /// <summary>
        /// Orders events by their names.
        /// </summary>
        /// <param name="source">The source events to order.</param>
        /// <returns>An ordered sequence of events.</returns>
        /// <seealso cref="FindByName{T}(IReadOnlyList{T}, string)"/>
        /// <seealso cref="FindIndexByName{T}(IReadOnlyList{T}, string)"/>
        /// <seealso cref="WhereName{T}(IReadOnlyList{T}, string, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<IEvent> OrderByName(this IEnumerable<IEvent> source) => source
            .OrderBy(e => e.Name, StringComparer.Ordinal);

        /// <summary>
        /// Orders fields by their names.
        /// </summary>
        /// <param name="source">The source fields to order.</param>
        /// <returns>An ordered sequence of fields.</returns>
        /// <seealso cref="FindByName{T}(IReadOnlyList{T}, string)"/>
        /// <seealso cref="FindIndexByName{T}(IReadOnlyList{T}, string)"/>
        /// <seealso cref="WhereName{T}(IReadOnlyList{T}, string, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<IField> OrderByName(this IEnumerable<IField> source) => source
            .OrderBy(f => f.Name, StringComparer.Ordinal);

        /// <summary>
        /// Finds the index of a type by full name using binary search.
        /// </summary>
        /// <typeparam name="T">The type of types in the list.</typeparam>
        /// <param name="types">The list of types, ordered by <see cref="IType.FullName"/>.</param>
        /// <param name="fullName">The full name to search for.</param>
        /// <returns>The type with the specified full name, or <see langword="null"/> if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="types"/> or <paramref name="fullName"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method uses binary search to efficiently locate a type in a sorted collection. The input collection must be
        /// ordered by type full name using ordinal string comparison; otherwise, results are undefined.
        /// </remarks>
        public static T? FindByFullName<T>(this IReadOnlyList<T> types, string fullName)
            where T : IType
        {
            if (types is null)
                throw new ArgumentNullException(nameof(types));
            if (fullName is null)
                throw new ArgumentNullException(nameof(fullName));

            var left = 0;
            var right = types.Count - 1;

            while (left <= right)
            {
                var mid = left + (right - left) / 2;
                var comparison = string.CompareOrdinal(types[mid].FullName, fullName);

                if (comparison < 0)
                    left = mid + 1;
                else if (comparison > 0)
                    right = mid - 1;
                else
                    return types[mid];
            }

            return default;
        }

        /// <summary>
        /// Finds the index of a member by name using binary search.
        /// </summary>
        /// <typeparam name="T">The type of members in the list.</typeparam>
        /// <param name="members">The list of members, ordered by <see cref="IMember.Name"/>.</param>
        /// <param name="name">The name to search for.</param>
        /// <returns>The index of the member with the specified name, or -1 if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="members"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method uses binary search to efficiently locate a member in a sorted collection. The input collection must be
        /// ordered by member name using ordinal string comparison; otherwise, results are undefined.
        /// </remarks>
        public static int FindIndexByName<T>(this IReadOnlyList<T> members, string name)
            where T : IMember
        {
            if (members is null)
                throw new ArgumentNullException(nameof(members));
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            var left = 0;
            var right = members.Count - 1;

            while (left <= right)
            {
                var mid = left + (right - left) / 2;
                var comparison = string.CompareOrdinal(members[mid].Name, name);

                if (comparison < 0)
                    left = mid + 1;
                else if (comparison > 0)
                    right = mid - 1;
                else
                    return mid;
            }

            return -1;
        }

        /// <summary>
        /// Finds a member by name using binary search.
        /// </summary>
        /// <typeparam name="T">The type of members in the list.</typeparam>
        /// <param name="members">The list of members, ordered by <see cref="IMember.Name"/>.</param>
        /// <param name="name">The name to search for.</param>
        /// <returns>The member with the specified name, or <see langword="null"/> if not found.</returns>
        /// <remarks>
        /// This method uses binary search to efficiently locate a member in a sorted collection. The input collection must be
        /// ordered by member name using ordinal string comparison; otherwise, results are undefined.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? FindByName<T>(this IReadOnlyList<T> members, string name)
            where T : IMember
        {
            var index = members.FindIndexByName(name);
            return index != -1 ? members[index] : default;
        }

        /// <summary>
        /// Filters members by name using binary search.
        /// </summary>
        /// <typeparam name="T">The type of members in the list.</typeparam>
        /// <param name="members">The members to filter, ordered by <see cref="IMember.Name"/>.</param>
        /// <param name="name">The name to match.</param>
        /// <param name="preserveOrder">If set to <see langword="true"/>, preserves the original order of members in the output.</param>
        /// <returns>All members with the specified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="members"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method uses binary search to efficiently locate members in sorted collections. The input collection must be
        /// ordered by member name using ordinal string comparison; otherwise, results are undefined.
        /// <para>
        /// This method is implemented by using deferred execution. The immediate return value is an object that stores all the
        /// information required to perform the action.
        /// </para>
        /// </remarks>
        public static IEnumerable<T> WhereName<T>(this IReadOnlyList<T> members, string name, bool preserveOrder = false)
            where T : IMember
        {
            if (members is null)
                throw new ArgumentNullException(nameof(members));
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            return members.Count > 0
                ? preserveOrder
                    ? EnumerateMembersPreservingOrder()
                    : EnumerateMembers()
                : [];

            IEnumerable<T> EnumerateMembers()
            {
                var index = members.FindIndexByName(name);
                if (index == -1)
                    yield break;

                for (var i = index; i >= 0 && members[i].Name == name; --i)
                    yield return members[i];

                for (var i = index + 1; i < members.Count && members[i].Name == name; ++i)
                    yield return members[i];
            }

            IEnumerable<T> EnumerateMembersPreservingOrder()
            {
                var index = members.FindIndexByName(name);
                if (index == -1)
                    yield break;

                while (index > 0 && members[index - 1].Name == name)
                    index--;

                while (index < members.Count && members[index].Name == name)
                    yield return members[index++];
            }
        }

    }
}
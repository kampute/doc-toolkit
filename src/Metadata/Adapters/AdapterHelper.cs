// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides helper methods for adapter implementations.
    /// </summary>
    /// <threadsafety static="true"/>
    public static class AdapterHelper
    {
        /// <summary>
        /// Decodes an explicit interface implementation name into its interface and member components.
        /// </summary>
        /// <param name="explicitName">The explicit interface implementation name.</param>
        /// <returns>A tuple containing the interface name and member name.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="explicitName"/> is <see langword="null"/> or empty.</exception>
        public static (string InterfaceName, string MemberName) DecodeExplicitName(string explicitName)
        {
            if (string.IsNullOrEmpty(explicitName))
                throw new ArgumentException($"'{nameof(explicitName)}' cannot be null or empty.", nameof(explicitName));

            var (qualification, memberName) = explicitName.SplitLast('.');

            var genericStart = qualification.IndexOf('<');
            if (genericStart <= 0)
                return (qualification, memberName);

            var interfaceName = qualification[..genericStart];

            var arity = 1;
            var depth = 0;
            foreach (var c in qualification.AsSpan(genericStart + 1))
            {
                switch (c)
                {
                    case ',' when depth == 0:
                        arity++;
                        break;
                    case '<':
                        depth++;
                        break;
                    case '>' when depth > 0:
                        depth--;
                        break;
                    case '>':
                        goto done;
                }
            }

        done:
            return ($"{interfaceName}`{arity}", memberName);
        }

        /// <summary>
        /// Resolves the canonical form of a type.
        /// </summary>
        /// <param name="type">The type to canonicalize.</param>
        /// <returns>A canonicalized version of the type if applicable; otherwise, the original type.</returns>
        /// <remarks>
        /// This method addresses a specific .NET reflection behavior where accessing nested generic types through inheritance
        /// relationships (such as <see cref="Type.BaseType"/> or <see cref="Type.GetInterfaces()"/>) can return <see cref="Type"/>
        /// instances that are semantically equivalent to direct <see langword="typeof"/> results but are not reference-equal when
        /// using the default equality comparer.
        /// <para>
        /// When a nested generic type definition is accessed via inheritance, the returned <see cref="Type"/> instance may be
        /// incorrectly flagged as a constructed generic type despite having only generic parameters as type arguments. This leads
        /// to discrepancies in equality comparisons, especially when such types are used as keys in collections that rely on
        /// reference equality.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type CanonicalizeType(Type type)
        {
            return type.IsConstructedGenericType
                && type.FullName is null
                && type.ReflectedType is not null
                && type.GenericTypeArguments.All(arg => arg.IsGenericParameter)
                    ? type.GetGenericTypeDefinition()
                    : type;
        }

        /// <summary>
        /// Determines whether two parameter lists represent equivalent signatures from the perspective of method overriding or interface implementation.
        /// </summary>
        /// <param name="baseParameters">The overridden or interface parameter list to compare against.</param>
        /// <param name="derivedParameters">The overriding or implementing parameter list to compare.</param>
        /// <returns><see langword="true"/> if the parameter lists are equivalent; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseParameters"/> or <paramref name="derivedParameters"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreParameterSignaturesMatching(IReadOnlyList<IParameter> baseParameters, IReadOnlyList<IParameter> derivedParameters)
        {
            if (baseParameters is null)
                throw new ArgumentNullException(nameof(baseParameters));
            if (derivedParameters is null)
                throw new ArgumentNullException(nameof(derivedParameters));

            if (baseParameters.Count != derivedParameters.Count)
                return false;

            for (var i = 0; i < baseParameters.Count; ++i)
                if (!baseParameters[i].IsSatisfiableBy(derivedParameters[i]))
                    return false;

            return true;
        }

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

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides helper methods for adapter implementations.
    /// </summary>
    /// <threadsafety static="true"/>
    public static class AdapterHelper
    {
        /// <summary>
        /// Splits an explicit interface implementation name into its interface and member components.
        /// </summary>
        /// <param name="explicitName">The explicit interface implementation name.</param>
        /// <returns>A tuple containing the interface name and member name.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="explicitName"/> is <see langword="null"/> or empty.</exception>
        public static (string InterfaceName, string MemberName) SplitExplicitName(string explicitName)
        {
            if (string.IsNullOrEmpty(explicitName))
                throw new ArgumentException($"'{nameof(explicitName)}' cannot be null or empty.", nameof(explicitName));

            var (qualification, memberName) = explicitName.SplitLast('.');

            var genericStart = qualification.IndexOf('<');
            if (genericStart <= 0)
                return (qualification, memberName);

            var interfaceName = qualification[..genericStart];
            var arity = GetGenericTypeArity(qualification.AsSpan(genericStart + 1));
            return ($"{interfaceName}`{arity}", memberName);

            static int GetGenericTypeArity(ReadOnlySpan<char> span)
            {
                var arity = 1;
                var depth = 0;
                foreach (var c in span)
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
                            return arity;
                    }
                }
                return arity;
            }
        }

        /// <summary>
        /// Determines whether two parameter lists represent equivalent signatures from the perspective of method overriding or interface implementation.
        /// </summary>
        /// <param name="targetParameters">The overridden or interface parameter list to compare against.</param>
        /// <param name="sourceParameters">The overriding or implementing parameter list to compare.</param>
        /// <returns><see langword="true"/> if the parameter lists are equivalent; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetParameters"/> or <paramref name="sourceParameters"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EquivalentParameters(IReadOnlyList<IParameter> targetParameters, IReadOnlyList<IParameter> sourceParameters)
        {
            if (targetParameters is null)
                throw new ArgumentNullException(nameof(targetParameters));
            if (sourceParameters is null)
                throw new ArgumentNullException(nameof(sourceParameters));

            if (targetParameters.Count != sourceParameters.Count)
                return false;

            for (var i = 0; i < targetParameters.Count; ++i)
                if (!targetParameters[i].IsSatisfiableBy(sourceParameters[i]))
                    return false;

            return true;
        }

        /// <summary>
        /// Determines whether the provided type arguments are valid for the given type parameters.
        /// </summary>
        /// <param name="typeParameters">The type parameters to validate against.</param>
        /// <param name="typeArguments">The type arguments to validate.</param>
        /// <returns><see langword="true"/> if the type arguments are valid for the type parameters; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeParameters"/> or <paramref name="typeArguments"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreValidTypeArguments(IReadOnlyList<ITypeParameter> typeParameters, IReadOnlyList<IType> typeArguments)
        {
            if (typeParameters is null)
                throw new ArgumentNullException(nameof(typeParameters));
            if (typeArguments is null)
                throw new ArgumentNullException(nameof(typeArguments));

            if (typeParameters.Count != typeArguments.Count)
                return false;

            for (var i = 0; i < typeParameters.Count; ++i)
                if (!typeParameters[i].IsSubstitutableBy(typeArguments[i]))
                    return false;

            return true;
        }

        /// <summary>
        /// Determines whether two types originate from the same declaring type hierarchy.
        /// </summary>
        /// <param name="a">The first type to compare.</param>
        /// <param name="b">The second type to compare.</param>
        /// <returns><see langword="true"/> if both types originate from the same declaring type hierarchy; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method compares types based on their names and declaring types, ignoring assembly, module information,
        /// and generic type arguments. It is useful for determining type equivalence in scenarios such as method overriding
        /// and interface implementation.
        /// </remarks>
        public static bool HaveSameDeclarationScope(Type? a, Type? b)
        {
            if (ReferenceEquals(a, b))
                return true;

            while (a is not null && b is not null)
            {
                if (a.Name != b.Name)
                    return false;

                a = a.DeclaringType;
                b = b.DeclaringType;
            }

            return a?.Namespace == b?.Namespace;
        }
    }
}

// Copyright (C) Kampute
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

            if (a is null || b is null)
                return false;

            while (a.Name == b.Name)
            {
                if (a.DeclaringType is null || b.DeclaringType is null)
                    return a.DeclaringType is null && b.DeclaringType is null && a.Namespace == b.Namespace;

                a = a.DeclaringType;
                b = b.DeclaringType;
            }

            return false;
        }

        /// <summary>
        /// Determines whether a target type is a valid substitution for a source type in the context of method overriding or interface implementation.
        /// </summary>
        /// <param name="baseMember">The base declaration of the member.</param>
        /// <param name="sourceType">The type used by the base declaration that may be substituted.</param>
        /// <param name="derivedMember">The derived or implementing declaration of the member.</param>
        /// <param name="targetType">The type used by the derived or implementing declaration that may substitute <paramref name="sourceType"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="targetType"/> is a valid substitution for <paramref name="sourceType"/>; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <see langword="null"/>.</exception>
        /// <remarks>
        /// When both members have declaring types, generic type parameters are matched against the actual type arguments in the
        /// derived member's declaring type, base types, and implemented interfaces. An unresolved parameter must remain the same
        /// parameter; satisfying its constraints alone does not establish a matching signature. When declaring type information
        /// is unavailable, the comparison uses <see cref="IsSubstitutableBy(IType, IType)"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidTypeSubstitution(IMember baseMember, IType sourceType, IMember derivedMember, IType targetType)
        {
            if (baseMember is null)
                throw new ArgumentNullException(nameof(baseMember));
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));
            if (derivedMember is null)
                throw new ArgumentNullException(nameof(derivedMember));
            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            var declaringType = baseMember.DeclaringType is not null ? derivedMember.DeclaringType : null;
            return IsSubstitutableBy(sourceType, targetType, declaringType);
        }

        /// <summary>
        /// Determines whether a target type can substitute a source type, where the generic type parameters of a type that appear in
        /// the source type, also within arrays, pointers, by-ref and nullable types, or as type arguments of constructed generic
        /// types, can be substituted by any type that satisfies their constraints.
        /// </summary>
        /// <param name="sourceType">The type used by the base declaration.</param>
        /// <param name="targetType">The type used by the derived or implementing declaration.</param>
        /// <returns><see langword="true"/> if <paramref name="targetType"/> can substitute <paramref name="sourceType"/>; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="sourceType"/> or <paramref name="targetType"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// For example, <c>List&lt;T&gt;</c> of a base declaration can be substituted by <c>List&lt;string&gt;</c> of an overriding
        /// declaration when <c>string</c> satisfies the constraints of <c>T</c>. Types that do not involve generic type parameters
        /// must be equal.
        /// </remarks>
        public static bool IsSubstitutableBy(IType sourceType, IType targetType)
        {
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));
            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            return IsSubstitutableBy(sourceType, targetType, null);
        }

        /// <summary>
        /// Compares types recursively, using actual generic type arguments when a declaring type is available.
        /// </summary>
        /// <param name="sourceType">The type used by the base declaration.</param>
        /// <param name="targetType">The type used by the derived declaration.</param>
        /// <param name="declaringType">The derived declaring type, or <see langword="null"/> for a constraint-based comparison.</param>
        /// <returns><see langword="true"/> if the types match under the applicable substitution rules; otherwise, <see langword="false"/>.</returns>
        private static bool IsSubstitutableBy(IType sourceType, IType targetType, IType? declaringType)
        {
            switch (sourceType)
            {
                case ITypeParameter { IsGenericTypeParameter: true } sourceTypeParameter:
                    return declaringType is null
                        ? sourceTypeParameter.IsSatisfiableBy(targetType)
                        : (FindTypeArgument(sourceTypeParameter, declaringType) ?? sourceTypeParameter).Equals(targetType);

                case ITypeDecorator sourceDecorator:
                    return targetType is ITypeDecorator targetDecorator
                        && sourceDecorator.Modifier == targetDecorator.Modifier
                        && sourceDecorator.ArrayRank == targetDecorator.ArrayRank
                        && IsSubstitutableBy(sourceDecorator.ElementType, targetDecorator.ElementType, declaringType);

                case IGenericCapableType { IsConstructedGenericType: true } sourceGenericType:
                    if (targetType is not IGenericCapableType { IsConstructedGenericType: true } targetGenericType
                        || !Equals(sourceGenericType.GenericTypeDefinition, targetGenericType.GenericTypeDefinition)
                        || sourceGenericType.TypeArguments.Count != targetGenericType.TypeArguments.Count)
                    {
                        return false;
                    }

                    for (var i = 0; i < sourceGenericType.TypeArguments.Count; ++i)
                    {
                        if (!IsSubstitutableBy(sourceGenericType.TypeArguments[i], targetGenericType.TypeArguments[i], declaringType))
                            return false;
                    }

                    return true;

                default:
                    return sourceType.Equals(targetType);
            }
        }

        /// <summary>
        /// Finds the argument bound to a type parameter in a declaring type's inheritance hierarchy.
        /// </summary>
        /// <param name="parameter">The type parameter to resolve.</param>
        /// <param name="declaringType">The declaring type whose hierarchy supplies the binding.</param>
        /// <returns>The bound type argument, or <see langword="null"/> if no binding is found.</returns>
        private static IType? FindTypeArgument(ITypeParameter parameter, IType declaringType)
        {
            // Canonical metadata can erase an open base type's argument bindings; reflection still holds those bindings.
            if (declaringType is TypeAdapter adapter)
                return adapter.FindTypeArgument(parameter);

            for (var type = declaringType; type is not null; type = type.BaseType)
            {
                if (FindInDeclaringTypes(type) is IType argument)
                    return argument;
            }

            if (declaringType is IInterfaceCapableType interfaceCapableType)
            {
                foreach (var interfaceType in interfaceCapableType.Interfaces)
                {
                    if (FindInDeclaringTypes(interfaceType) is IType argument)
                        return argument;
                }
            }

            return null;

            IType? FindInDeclaringTypes(IType type)
            {
                for (var current = type; current is not null; current = current.DeclaringType)
                {
                    if (current is not IGenericCapableType genericType)
                        continue;

                    var definition = genericType.GenericTypeDefinition ?? genericType;
                    if (!definition.Equals(parameter.DeclaringMember))
                        continue;

                    var arguments = genericType.IsConstructedGenericType ? genericType.TypeArguments : genericType.TypeParameters;
                    if (parameter.Position < arguments.Count)
                        return arguments[parameter.Position];
                }

                return null;
            }
        }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Compares <see cref="MemberInfo"/> instances for equality based on their metadata.
    /// </summary>
    /// <remarks>
    /// This comparer considers various aspects of <see cref="MemberInfo"/> and <see cref="Type"/> instances,
    /// including generic parameters, array ranks, and element types, to determine equality.
    /// <para>
    /// It also provides a robust hash code generation mechanism that aligns with the equality comparison logic.
    /// </para>
    /// </remarks>
    public sealed class MemberInfoComparer : IEqualityComparer<MemberInfo>
    {
        private MemberInfoComparer() { }

        /// <summary>
        /// Gets the singleton instance of the <see cref="MemberInfoComparer"/>.
        /// </summary>
        /// <value>
        /// The singleton instance of the <see cref="MemberInfoComparer"/>.
        /// </value>
        public static MemberInfoComparer Instance { get; } = new();

        /// <summary>
        /// Determines whether the specified members are equal.
        /// </summary>
        /// <param name="x">The first member to compare.</param>
        /// <param name="y">The second member to compare.</param>
        /// <returns><see langword="true"/> if the specified members are equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(MemberInfo? x, MemberInfo? y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            if (x.MemberType != y.MemberType)
                return false;

            if (x is Type xType && y is Type yType)
                return TypeEquals(xType, yType);

            if (TryGetMetadataToken(x, out var xToken) && TryGetMetadataToken(y, out var yToken))
            {
                if (!ReferenceEquals(x.Module, y.Module))
                    return false;

                var xDeclaringType = x.DeclaringType;
                var yDeclaringType = y.DeclaringType;
                if (xDeclaringType is null || yDeclaringType is null)
                {
                    if (!(xDeclaringType is null && yDeclaringType is null))
                        return false;
                }
                else if (!TypeEquals(xDeclaringType, yDeclaringType))
                {
                    return false;
                }

                return xToken == yToken;
            }

            return string.Equals(x.Name, y.Name, StringComparison.Ordinal)
                && Equals(x.DeclaringType, y.DeclaringType);
        }

        /// <summary>
        /// Returns a hash code for the specified member.
        /// </summary>
        /// <param name="member">The member for which to get a hash code.</param>
        /// <returns>A hash code for the specified member.</returns>
        public int GetHashCode(MemberInfo member)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            if (member is Type type)
                return GetTypeHashCode(type);

            if (TryGetMetadataToken(member, out var token))
            {
                var hash = new HashCode();
                hash.Add(token);
                hash.Add(GetModuleHash(member.Module));
                hash.Add(member.MemberType);
                if (member.DeclaringType is not null)
                    hash.Add(GetTypeHashCode(member.DeclaringType));
                return hash.ToHashCode();
            }

            var fallback = new HashCode();
            fallback.Add(member.MemberType);
            fallback.Add(member.Name, StringComparer.Ordinal);
            if (member.DeclaringType is not null)
                fallback.Add(GetTypeHashCode(member.DeclaringType));
            return fallback.ToHashCode();
        }

        /// <summary>
        /// Compares two <see cref="Type"/> instances for equality, considering generic parameters, array ranks, and element types.
        /// </summary>
        /// <param name="x">The first type to compare.</param>
        /// <param name="y">The second type to compare.</param>
        /// <returns><see langword="true"/> if the specified types are equal; otherwise, <see langword="false"/>.</returns>
        private static bool TypeEquals(Type x, Type y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            if (x.IsGenericParameter || y.IsGenericParameter)
            {
                if (!x.IsGenericParameter || !y.IsGenericParameter)
                    return false;

                return GenericParameterEquals(x, y);
            }

            if (x.HasElementType || y.HasElementType)
            {
                if (!x.HasElementType || !y.HasElementType)
                    return false;

                if (x.IsArray != y.IsArray)
                    return false;

                if (x.IsArray && x.GetArrayRank() != y.GetArrayRank())
                    return false;

                if (x.IsByRef != y.IsByRef || x.IsPointer != y.IsPointer)
                    return false;

                return TypeEquals(x.GetElementType()!, y.GetElementType()!);
            }

            if (x.IsGenericTypeDefinition ^ y.IsGenericTypeDefinition)
            {
                var definition = x.IsGenericTypeDefinition ? x : y;
                var candidate = x.IsGenericTypeDefinition ? y : x;

                if (candidate.IsConstructedGenericType)
                {
                    var arguments = candidate.GetGenericArguments();
                    if (arguments.Length > 0 && Array.TrueForAll(arguments, static argument => argument.IsGenericParameter))
                        return TypeEquals(candidate.GetGenericTypeDefinition(), definition);
                }

                return false;
            }

            if (x.IsConstructedGenericType && y.IsConstructedGenericType)
            {
                if (!TypeEquals(x.GetGenericTypeDefinition(), y.GetGenericTypeDefinition()))
                    return false;

                var xArgs = x.GetGenericArguments();
                var yArgs = y.GetGenericArguments();

                if (xArgs.Length != yArgs.Length)
                    return false;

                for (var i = 0; i < xArgs.Length; i++)
                {
                    if (!TypeEquals(xArgs[i], yArgs[i]))
                        return false;
                }

                return true;
            }

            if (x.IsGenericTypeDefinition || y.IsGenericTypeDefinition)
                return x.IsGenericTypeDefinition && y.IsGenericTypeDefinition && string.Equals(x.FullName, y.FullName, StringComparison.Ordinal) && string.Equals(x.Assembly.FullName, y.Assembly.FullName, StringComparison.Ordinal);

            if (TryGetMetadataToken(x, out var xToken) && TryGetMetadataToken(y, out var yToken) && ReferenceEquals(x.Module, y.Module))
                return xToken == yToken;

            if (!string.Equals(x.FullName, y.FullName, StringComparison.Ordinal))
                return false;

            if (!string.Equals(x.Assembly.FullName, y.Assembly.FullName, StringComparison.Ordinal))
                return false;

            if (x.DeclaringType is null || y.DeclaringType is null)
                return x.DeclaringType is null && y.DeclaringType is null;

            return TypeEquals(x.DeclaringType, y.DeclaringType);
        }

        /// <summary>
        /// Compares two generic parameter types for equality, considering their position, attributes, and declaring context.
        /// </summary>
        /// <param name="x">The first generic parameter type to compare.</param>
        /// <param name="y">The second generic parameter type to compare.</param>
        /// <returns><see langword="true"/> if the specified generic parameter types are equal; otherwise, <see langword="false"/>.</returns>
        private static bool GenericParameterEquals(Type x, Type y)
        {
            if (TryGetMetadataToken(x, out var xToken) && TryGetMetadataToken(y, out var yToken) && ReferenceEquals(x.Module, y.Module))
                return xToken == yToken;

            if (x.GenericParameterPosition != y.GenericParameterPosition)
                return false;

            if (x.GenericParameterAttributes != y.GenericParameterAttributes)
                return false;

            var declaringMethodX = x.DeclaringMethod;
            var declaringMethodY = y.DeclaringMethod;

            if (declaringMethodX is not null || declaringMethodY is not null)
            {
                if (declaringMethodX is null || declaringMethodY is null)
                    return false;

                return Instance.Equals(declaringMethodX, declaringMethodY);
            }

            var declaringTypeX = x.DeclaringType;
            var declaringTypeY = y.DeclaringType;

            if (declaringTypeX is null || declaringTypeY is null)
                return declaringTypeX is null && declaringTypeY is null;

            return TypeEquals(declaringTypeX, declaringTypeY);
        }

        /// <summary>
        /// Returns a hash code for the specified type, considering generic parameters, array ranks, and element types.
        /// </summary>
        /// <param name="type">The type for which to get a hash code.</param>
        /// <returns>A hash code for the specified type.</returns>
        private static int GetTypeHashCode(Type type)
        {
            if (type.IsGenericParameter)
            {
                var hash = new HashCode();
                hash.Add(type.GenericParameterPosition);
                hash.Add(type.GenericParameterAttributes);
                if (type.DeclaringMethod is not null)
                    hash.Add(Instance.GetHashCode(type.DeclaringMethod));
                if (type.DeclaringType is not null)
                    hash.Add(GetTypeHashCode(type.DeclaringType));
                return hash.ToHashCode();
            }

            if (type.HasElementType)
            {
                var hash = new HashCode();
                hash.Add(type.IsArray);
                if (type.IsArray)
                    hash.Add(type.GetArrayRank());
                hash.Add(type.IsByRef);
                hash.Add(type.IsPointer);
                hash.Add(GetTypeHashCode(type.GetElementType()!));
                return hash.ToHashCode();
            }

            if (type.IsConstructedGenericType)
            {
                var definitionHash = GetTypeHashCode(type.GetGenericTypeDefinition());
                var arguments = type.GetGenericArguments();
                if (arguments.Length == 0)
                    return definitionHash;

                if (type.ContainsGenericParameters && Array.TrueForAll(arguments, static argument => argument.IsGenericParameter))
                    return definitionHash;

                var hash = new HashCode();
                hash.Add(definitionHash);
                foreach (var argument in arguments)
                    hash.Add(GetTypeHashCode(argument));
                return hash.ToHashCode();
            }

            if (type.IsGenericTypeDefinition)
            {
                var hash = new HashCode();
                hash.Add(type.FullName, StringComparer.Ordinal);
                hash.Add(type.Assembly.FullName, StringComparer.Ordinal);
                return hash.ToHashCode();
            }

            if (TryGetMetadataToken(type, out var token))
            {
                var hash = new HashCode();
                hash.Add(token);
                hash.Add(GetModuleHash(type.Module));
                return hash.ToHashCode();
            }

            var fallback = new HashCode();
            fallback.Add(type.FullName, StringComparer.Ordinal);
            fallback.Add(type.Assembly.FullName, StringComparer.Ordinal);
            if (type.DeclaringType is not null)
                fallback.Add(GetTypeHashCode(type.DeclaringType));
            return fallback.ToHashCode();
        }

        /// <summary>
        /// Tries to get the metadata token of a member, returning false if it fails.
        /// </summary>
        /// <param name="member">The member whose metadata token to retrieve.</param>
        /// <param name="token">When this method returns, contains the metadata token if the operation succeeded, or the default value if it failed.</param>
        /// <returns><see langword="true"/> if the metadata token was successfully retrieved; otherwise, <see langword="false"/>.</returns>
        private static bool TryGetMetadataToken(MemberInfo member, out int token)
        {
            try
            {
                token = member.MetadataToken;
                return true;
            }
            catch
            {
                token = default;
                return false;
            }
        }

        /// <summary>
        /// Returns a hash code for the specified module, using its ModuleVersionId if available, otherwise falling back to its runtime hash code.
        /// </summary>
        /// <param name="module">The module for which to get a hash code.</param>
        /// <returns>A hash code for the specified module.</returns>
        private static int GetModuleHash(Module module)
        {
            try
            {
                return module.ModuleVersionId.GetHashCode();
            }
            catch
            {
                return RuntimeHelpers.GetHashCode(module);
            }
        }
    }
}

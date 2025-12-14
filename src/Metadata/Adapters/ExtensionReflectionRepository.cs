// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata.Reflection;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides a repository for managing reflection information about extension blocks and their members within a specified assembly.
    /// </summary>
    /// <remarks>
    /// This repository caches reflection information about extension blocks and their members for specific container types within the 
    /// given assembly.
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class ExtensionReflectionRepository : IExtensionReflectionRepository
    {
        private readonly ConcurrentDictionary<Type, ExtensionContainerInfo> cache = [];

        /// <summary>
        /// Initialize a new instance of <see cref="ExtensionReflectionRepository"/> class.
        /// </summary>
        /// <param name="assembly">The assembly whose extensions are to be managed.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="assembly"/> is <see langword="null"/>.</exception>
        public ExtensionReflectionRepository(IAssembly assembly)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        /// <inheritdoc/>
        public IAssembly Assembly { get; }

        /// <inheritdoc/>
        public IReadOnlyList<ExtensionBlockInfo> GetDeclaredExtensionBlocks(Type containerType) => GetExtensionContainer(containerType).ExtensionBlocks;

        /// <inheritdoc/>
        public IEnumerable<MethodInfo> GetDeclaredExtensionMethods(Type containerType) => GetExtensionContainer(containerType).ExtensionBlockMethods;

        /// <inheritdoc/>
        public IEnumerable<PropertyInfo> GetDeclaredExtensionProperties(Type containerType) => GetExtensionContainer(containerType).ExtensionBlockProperties;

        /// <inheritdoc/>
        public MethodInfo GetNormalizedMethodInfo(MethodInfo methodInfo) => IsPotentialExtensionMethod(methodInfo) && methodInfo is not IExtensionBlockMemberInfo
            ? GetExtensionContainer(methodInfo.DeclaringType).GetNormalizedMethodInfo(methodInfo)
            : methodInfo;

        /// <inheritdoc/>
        public MemberInfo? GetExtensionMemberInfo(MethodInfo methodInfo) => IsPotentialExtensionMethod(methodInfo) && methodInfo is not IExtensionBlockMemberInfo
            ? GetExtensionContainer(methodInfo.DeclaringType).GetExtensionMemberInfo(methodInfo)
            : null;

        /// <summary>
        /// Gets the cached <see cref="ExtensionContainerInfo"/> for the given container type.
        /// </summary>
        /// <param name="containerType">The container type.</param>
        /// <returns>A <see cref="ExtensionContainerInfo"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="containerType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="containerType"/> is not a top-level non-generic static class.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="containerType"/> does not belong to the same assembly as the resolver.</exception>
        protected virtual ExtensionContainerInfo GetExtensionContainer(Type containerType)
        {
            if (containerType is null)
                throw new ArgumentNullException(nameof(containerType));

            if (!Assembly.Represents(containerType.Assembly))
                throw new ArgumentException("Container type must belong to the same assembly as the repository.", nameof(containerType));

            return cache.GetOrAdd(containerType, static type => new ExtensionContainerInfo(type));
        }

        /// <summary>
        /// Determines whether the specified method is a potential extension method.
        /// </summary>
        /// <param name="methodInfo">The method to check.</param>
        /// <returns><see langword="true"/> if the method is a potential extension method; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool IsPotentialExtensionMethod(MethodInfo methodInfo)
        {
            return methodInfo is not null
                && methodInfo.IsStatic is true
                && methodInfo.IsSpecialName is false
                && IsValidExtensionContainerType(methodInfo.DeclaringType);
        }

        /// <summary>
        /// Determines whether the specified type is a valid container for extension members.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><see langword="true"/> if the type is a valid extension container type; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool IsValidExtensionContainerType(Type type)
        {
            return type is not null
                && type.IsSealed is true
                && type.IsAbstract is true
                && type.IsGenericType is false
                && type.IsNested is false;
        }
    }
}

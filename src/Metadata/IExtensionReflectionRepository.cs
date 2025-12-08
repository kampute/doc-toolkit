// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Defines a contract for a repository that manages reflection information about extension methods and 
    /// properties for a specific assembly.
    /// </summary>
    /// <remarks>
    /// This interface defines methods to retrieve reflection information about extension methods and properties 
    /// within an assembly. It allows clients to query for declared extension methods and properties, as well as 
    /// normalize method information to identify extension members.
    /// </remarks>
    public interface IExtensionReflectionRepository
    {
        /// <summary>
        /// Gets the assembly whose extensions are managed by the repository.
        /// </summary>
        /// <value>
        /// The assembly whose extensions are managed by the resolver.
        /// </value>
        IAssembly Assembly { get; }

        /// <summary>
        /// Retrieves reflection information for all extension methods declared on the given container type.
        /// </summary>
        /// <param name="containerType">The container type.</param>
        /// <returns>The extension methods declared on the container type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="containerType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="containerType"/> does not belong to the same assembly as the resolver.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="containerType"/> is not a top-level non-generic static class.</exception>
        IReadOnlyList<MethodInfo> GetDeclaredExtensionMethods(Type containerType);

        /// <summary>
        /// Retrieves reflection information for all extension properties declared on the given container type.
        /// </summary>
        /// <param name="containerType">The container type.</param>
        /// <returns>The extension properties declared on the container type.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="containerType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="containerType"/> does not belong to the same assembly as the repository.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="containerType"/> is not a top-level non-generic static class.</exception>
        IReadOnlyList<PropertyInfo> GetDeclaredExtensionProperties(Type containerType);

        /// <summary>
        /// Retrieves the normalized reflection information for the specified method as an extension method or property accessor.
        /// </summary>
        /// <param name="methodInfo">The reflection information of the method to normalize.</param>
        /// <returns>The normalized extension method if the method is an extension method or property accessor; otherwise, the original method.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="methodInfo"/> does not belong to the same assembly as the repository.</exception>
        /// <remarks>
        /// This method returns the normalized reflection information for the specified <paramref name="methodInfo"/> if the method is an extension method 
        /// or an extension property accessor. If it is not, the original <paramref name="methodInfo"/> is returned.
        /// <para>
        /// The normalized method of an extension method is a logical view of the method as it would appear when invoked on the extended 
        /// type, rather than as a static method on the container type that has the extended type as its first parameter.
        /// </para>
        /// </remarks>
        MethodInfo GetNormalizedMethodInfo(MethodInfo methodInfo);

        /// <summary>
        /// Retrieves the extension member information for the specified method, if it is an extension method or property accessor.
        /// </summary>
        /// <param name="methodInfo">The reflection information of the method to examine.</param>
        /// <returns>The extension member information if the method is an extension method or property accessor; otherwise, <see langword="null"/>.</returns>
        /// <remarks>
        /// This method returns a <see cref="MemberInfo"/> representing the extension member associated with the specified <paramref name="methodInfo"/>.
        /// <list type="bullet">
        ///   <item>If the method is an extension method, the returned <see cref="MemberInfo"/> will be a <see cref="MethodInfo"/>.</item>
        ///   <item>If the method is an extension property accessor, the returned <see cref="MemberInfo"/> will be a <see cref="PropertyInfo"/>.</item>
        ///   <item>If the method is neither, the method returns <see langword="null"/>.</item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="methodInfo"/> does not belong to the same assembly as the resolver.</exception>
        MemberInfo? GetExtensionMemberInfo(MethodInfo methodInfo);
    }
}
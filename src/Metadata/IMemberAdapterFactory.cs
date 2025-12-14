// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Reflection;
    using System;
    using System.Reflection;

    /// <summary>
    /// Defines a contract for creating metadata representations from reflection information of members.
    /// </summary>
    /// <remarks>
    /// This interface provides factory methods to create metadata representations of types, and various member types from
    /// their reflection counterparts.
    /// </remarks>
    public interface IMemberAdapterFactory
    {
        /// <summary>
        /// Creates the extension block metadata for the specified extension block information.
        /// </summary>
        /// <param name="assembly">The assembly metadata that contains the extension block.</param>
        /// <param name="extensionBlock">The extension block information to get metadata for.</param>
        /// <returns>A metadata representation of the specified extension block.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="extensionBlock"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="extensionBlock"/> does not belong to the specified assembly.</exception>
        IExtensionBlock CreateExtensionBlockMetadata(IAssembly assembly, ExtensionBlockInfo extensionBlock);

        /// <summary>
        /// Creates the type metadata for the specified type within the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly metadata that contains the type.</param>
        /// <param name="type">The reflection type to get metadata for.</param>
        /// <returns>A metadata representation of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> does not belong to the specified assembly.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is a nested type.</exception>
        /// <exception cref="NotSupportedException">Thrown when the type is not supported.</exception>
        IType CreateTypeMetadata(IAssembly assembly, Type type);

        /// <summary>
        /// Creates the type metadata for the specified type nested within the given declaring type.
        /// </summary>
        /// <param name="declaringType">The type metadata that contains the type.</param>
        /// <param name="type">The reflection type to get metadata for.</param>
        /// <returns>A metadata representation of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> does not belong to <paramref name="declaringType"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown when the type is not supported.</exception>
        IType CreateTypeMetadata(IType declaringType, Type type);

        /// <summary>
        /// Creates the constructor metadata for the specified constructor within the given declaring type.
        /// </summary>
        /// <param name="declaringType">The type metadata that contains the constructor.</param>
        /// <param name="constructorInfo">The reflection constructor to get metadata for.</param>
        /// <returns>A metadata representation of the specified constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="constructorInfo"/> does not belong to <paramref name="declaringType"/>.</exception>
        IConstructor CreateConstructorMetadata(IType declaringType, ConstructorInfo constructorInfo);

        /// <summary>
        /// Creates the method metadata for the specified method within the given declaring type.
        /// </summary>
        /// <param name="declaringType">The type metadata that contains the method.</param>
        /// <param name="methodInfo">The reflection method to get metadata for.</param>
        /// <returns>A metadata representation of the specified method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="methodInfo"/> does not belong to <paramref name="declaringType"/>.</exception>
        IMethodBase CreateMethodMetadata(IType declaringType, MethodInfo methodInfo);

        /// <summary>
        /// Creates the property metadata for the specified property within the given declaring type.
        /// </summary>
        /// <param name="declaringType">The type metadata that contains the property.</param>
        /// <param name="propertyInfo">The reflection property to get metadata for.</param>
        /// <returns>A metadata representation of the specified property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="propertyInfo"/> does not belong to <paramref name="declaringType"/>.</exception>
        IProperty CreatePropertyMetadata(IType declaringType, PropertyInfo propertyInfo);

        /// <summary>
        /// Creates the event metadata for the specified event within the given declaring type.
        /// </summary>
        /// <param name="declaringType">The type metadata that contains the event.</param>
        /// <param name="eventInfo">The reflection event to get metadata for.</param>
        /// <returns>A metadata representation of the specified event.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="eventInfo"/> does not belong to <paramref name="declaringType"/>.</exception>
        IEvent CreateEventMetadata(IType declaringType, EventInfo eventInfo);

        /// <summary>
        /// Creates the field metadata for the specified field within the given declaring type.
        /// </summary>
        /// <param name="declaringType">The type metadata that contains the field.</param>
        /// <param name="fieldInfo">The reflection field to get metadata for.</param>
        /// <returns>A metadata representation of the specified field.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fieldInfo"/> does not belong to <paramref name="declaringType"/>.</exception>
        IField CreateFieldMetadata(IType declaringType, FieldInfo fieldInfo);

        /// <summary>
        /// Creates the parameter metadata for the specified parameter within the given member.
        /// </summary>
        /// <param name="member">The member metadata that contains the parameter.</param>
        /// <param name="parameterInfo">The reflection parameter to get metadata for.</param>
        /// <returns>A metadata representation of the specified parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> or <paramref name="parameterInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="parameterInfo"/> does not belong to <paramref name="member"/>.</exception>
        IParameter CreateParameterMetadata(IMember member, ParameterInfo parameterInfo);

        /// <summary>
        /// Creates the custom attribute metadata for the specified attribute data.
        /// </summary>
        /// <param name="attributeData">The reflection custom attribute data to get metadata for.</param>
        /// <param name="target">The target element type of the attribute.</param>
        /// <returns>A metadata representation of the specified custom attribute.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attributeData"/> is <see langword="null"/>.</exception>
        ICustomAttribute CreateCustomAttributeMetadata(CustomAttributeData attributeData, AttributeTarget target);
    }
}

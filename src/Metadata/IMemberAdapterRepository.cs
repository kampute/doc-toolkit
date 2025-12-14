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
    /// Defines a contract for a repository that manages member metadata within an assembly.
    /// </summary>
    /// <remarks>
    /// This interface provides methods to retrieve metadata representations of various member types (types, methods, properties,
    /// events, fields) within a specific assembly. It abstracts the underlying reflection mechanisms, allowing consistent access
    /// to metadata regardless of whether the assembly was loaded via Common Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    public interface IMemberAdapterRepository
    {
        /// <summary>
        /// Gets the assembly for which this repository manages member metadata.
        /// </summary>
        /// <value>
        /// The assembly associated with this repository.
        /// </value>
        IAssembly Assembly { get; }

        /// <summary>
        /// Gets the repository that provides access to reflection information about extension members.
        /// </summary>
        /// <value>
        /// An <see cref="IExtensionReflectionRepository"/> instance that provides access to reflection information about extension members.
        /// </value>
        IExtensionReflectionRepository ExtensionReflection { get; }

        /// <summary>
        /// Resolves the canonical form of a type.
        /// </summary>
        /// <param name="type">The type to resolve.</param>
        /// <returns>A <see cref="Type"/> instance that is reference-equal to the canonical type definition if applicable; otherwise, the original <paramref name="type"/>.</returns>
        /// <remarks>
        /// When a nested generic type definition is collected indirectly (such as <see cref="Type.BaseType"/> or
        /// <see cref="Type.GetInterfaces()"/>), the returned <see cref="Type"/> instance may be incorrectly flagged
        /// as a constructed generic type despite having only generic parameters as type arguments. This leads
        /// to discrepancies in equality comparisons, especially when such types are used as keys in collections that
        /// rely on reference equality.
        /// <para>
        /// This method ensures that when such types are encountered, they are converted back to their generic type
        /// definitions and thus reference-equal to instances obtained directly via <see langword="typeof"/> expressions.
        /// </para>
        /// </remarks>
        Type ResolveCanonicalType(Type type);

        /// <summary>
        /// Gets the extension block metadata for the specified extension block within the assembly.
        /// </summary>
        /// <param name="extensionBlock">The extension block information to get metadata for.</param>
        /// <returns>A metadata representation of the specified extension block.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="extensionBlock"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="extensionBlock"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="extensionBlock"/> does not belong to the specified assembly.</exception>
        IExtensionBlock GetExtensionBlockMetadata(ExtensionBlockInfo extensionBlock);

        /// <summary>
        /// Gets the type metadata for the specified type within the assembly.
        /// </summary>
        /// <param name="type">The reflection type to get metadata for.</param>
        /// <returns>A metadata representation of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> does not belong to the assembly.</exception>
        /// <exception cref="NotSupportedException">Thrown when the type is not supported.</exception>
        IType GetTypeMetadata(Type type);

        /// <summary>
        /// Gets the type metadata for the specified type within the assembly.
        /// </summary>
        /// <typeparam name="T">The specific type of type metadata to retrieve.</typeparam>
        /// <param name="type">The reflection type to get metadata for.</param>
        /// <returns>A metadata representation of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> does not belong to the assembly.</exception>
        /// <exception cref="NotSupportedException">Thrown when the type is not supported.</exception>
        /// <exception cref="InvalidCastException">Thrown when the retrieved type metadata cannot be cast to <typeparamref name="T"/>.</exception>
        T GetTypeMetadata<T>(Type type) where T : IType => (T)GetTypeMetadata(type);

        /// <summary>
        /// Gets the method metadata for the specified constructor within the assembly.
        /// </summary>
        /// <param name="constructorInfo">The reflection constructor to get metadata for.</param>
        /// <returns>A metadata representation of the specified constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="constructorInfo"/> does not belong to the assembly.</exception>
        IConstructor GetConstructorMetadata(ConstructorInfo constructorInfo);

        /// <summary>
        /// Gets the method metadata for the specified method within the assembly.
        /// </summary>
        /// <param name="methodInfo">The reflection method to get metadata for.</param>
        /// <param name="asDeclared">Indicates whether to retrieve extension methods in their declared form rather than their usage form.</param>
        /// <returns>A metadata representation of the specified method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="methodInfo"/> does not belong to the assembly.</exception>
        /// <remarks>
        /// When <paramref name="asDeclared"/> is <see langword="false"/> (the default), extension methods are detected and resolved
        /// to reflect their usage form rather than their declared form. Otherwise, the metadata is retrieved as declared, without
        /// extension method detection and resolution.
        /// <para>
        /// Based on the nature of the provided <paramref name="methodInfo"/>, the returned metadata may represent different
        /// method types:
        /// <list type="bullet">
        ///   <item>
        ///     <term>Regular or Extension Method</term>
        ///     <description>The returned metadata is an instance of <see cref="IMethod"/>.</description>
        ///   </item>
        ///   <item>
        ///     <term>Operator Overload</term>
        ///     <description>The returned metadata is an instance of <see cref="IOperator"/>.</description>
        ///   </item>
        /// </list>
        /// </para>
        /// </remarks>
        IMethodBase GetMethodMetadata(MethodInfo methodInfo, bool asDeclared = false);

        /// <summary>
        /// Gets the method metadata for the specified method within the assembly, with a specific return type.
        /// </summary>
        /// <typeparam name="T">The specific type of method metadata to retrieve.</typeparam>
        /// <param name="methodInfo">The reflection method to get metadata for.</param>
        /// <returns>A metadata representation of the specified method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="methodInfo"/> does not belong to the assembly.</exception>
        /// <exception cref="InvalidCastException">Thrown when the retrieved method metadata cannot be cast to <typeparamref name="T"/>.</exception>
        /// <remarks>
        /// Unlike <see cref="GetMethodMetadata(MethodInfo, bool)"/>, this method always retrieves the method metadata in its resolved form,
        /// meaning that extension methods are detected and resolved to reflect their usage form rather than their declared form.
        /// </remarks>
        T GetMethodMetadata<T>(MethodInfo methodInfo) where T : IMethodBase => (T)GetMethodMetadata(methodInfo);

        /// <summary>
        /// Gets the property metadata for the specified property within the assembly.
        /// </summary>
        /// <param name="propertyInfo">The reflection property to get metadata for.</param>
        /// <returns>A metadata representation of the specified property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="propertyInfo"/> does not belong to the assembly.</exception>
        IProperty GetPropertyMetadata(PropertyInfo propertyInfo);

        /// <summary>
        /// Gets the event metadata for the specified event within the assembly.
        /// </summary>
        /// <param name="eventInfo">The reflection event to get metadata for.</param>
        /// <returns>A metadata representation of the specified event.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="eventInfo"/> does not belong to the assembly.</exception>
        IEvent GetEventMetadata(EventInfo eventInfo);

        /// <summary>
        /// Gets the field metadata for the specified field within the assembly.
        /// </summary>
        /// <param name="fieldInfo">The reflection field to get metadata for.</param>
        /// <returns>A metadata representation of the specified field.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fieldInfo"/> does not belong to the assembly.</exception>
        IField GetFieldMetadata(FieldInfo fieldInfo);

        /// <summary>
        /// Gets the parameter metadata for the specified parameter within the assembly.
        /// </summary>
        /// <param name="parameterInfo">The reflection parameter to get metadata for.</param>
        /// <returns>A metadata representation of the specified parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameterInfo"/> is <see langword="null"/>.</exception>
        IParameter GetParameterMetadata(ParameterInfo parameterInfo);

        /// <summary>
        /// Gets the custom attribute metadata for the specified attribute data within the assembly.
        /// </summary>
        /// <param name="attributeData">The reflection custom attribute data to get metadata for.</param>
        /// <param name="target">The target of the attribute.</param>
        /// <returns>A metadata representation of the specified custom attribute.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attributeData"/> is <see langword="null"/>.</exception>
        ICustomAttribute GetCustomAttributeMetadata(CustomAttributeData attributeData, AttributeTarget target);

        /// <summary>
        /// Gets the member metadata for the specified member within the assembly.
        /// </summary>
        /// <param name="memberInfo">The reflection member to get metadata for.</param>
        /// <returns>A metadata representation of the specified member.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="memberInfo"/> does not belong to the assembly.</exception>
        /// <exception cref="NotSupportedException">Thrown when the member is not supported.</exception>
        /// <remarks>
        /// The provided <paramref name="memberInfo"/> can represent any member type, including types, constructors, methods,
        /// properties, events, and fields. If the <paramref name="memberInfo"/> corresponds to an accessor of an extension
        /// property, the method will resolve and return the appropriate property metadata.
        /// </remarks>
        IMember GetMemberMetadata(MemberInfo memberInfo) => memberInfo switch
        {
            null => throw new ArgumentNullException(nameof(memberInfo)),
            Type type => GetTypeMetadata(type),
            ConstructorInfo constructorInfo => GetConstructorMetadata(constructorInfo),
            MethodInfo methodInfo => ExtensionReflection.GetExtensionMemberInfo(methodInfo) switch
            {
                MethodInfo extensionMethodInfo => GetMethodMetadata(extensionMethodInfo, asDeclared: true),
                PropertyInfo extensionPropertyInfo => GetPropertyMetadata(extensionPropertyInfo),
                _ => GetMethodMetadata(methodInfo, asDeclared: true),
            },
            PropertyInfo propertyInfo => GetPropertyMetadata(propertyInfo),
            EventInfo eventInfo => GetEventMetadata(eventInfo),
            FieldInfo fieldInfo => GetFieldMetadata(fieldInfo),
            _ => throw new NotSupportedException($"Member '{memberInfo}' is not supported."),
        };
    }
}

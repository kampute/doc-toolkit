// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Provides a factory for creating member metadata adapters of reflection members.
    /// </summary>
    /// <remarks>
    /// This factory is responsible for creating appropriate metadata adapters for various reflection members,
    /// including types, methods, properties, events, fields, parameters, and custom attributes. It abstracts
    /// the instantiation logic, ensuring that the correct adapter is created based on the type of member being
    /// processed.
    /// </remarks>
    public class MemberAdapterFactory : IMemberAdapterFactory
    {
        private MemberAdapterFactory() { }

        /// <summary>
        /// Gets the singleton instance of the <see cref="MemberAdapterFactory"/>.
        /// </summary>
        /// <value>
        /// The singleton instance of the <see cref="MemberAdapterFactory"/>.
        /// </value>
        public static MemberAdapterFactory Instance { get; } = new();

        /// <inheritdoc/>
        public virtual IType CreateTypeMetadata(IAssembly assembly, Type type)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return CreateSpecificTypeMetadata(assembly, type);
        }

        /// <inheritdoc/>
        public virtual IType CreateTypeMetadata(IType declaringType, Type type)
        {
            if (declaringType is null)
                throw new ArgumentNullException(nameof(declaringType));
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return CreateSpecificTypeMetadata(declaringType, type);
        }

        /// <inheritdoc/>
        public virtual IConstructor CreateConstructorMetadata(IType declaringType, ConstructorInfo constructorInfo)
            => new ConstructorAdapter(declaringType, constructorInfo);

        /// <inheritdoc/>
        public virtual IProperty CreatePropertyMetadata(IType declaringType, PropertyInfo propertyInfo)
            => new PropertyAdapter(declaringType, propertyInfo);

        /// <inheritdoc/>
        public virtual IMethodBase CreateMethodMetadata(IType declaringType, MethodInfo methodInfo)
        {
            if (declaringType is null)
                throw new ArgumentNullException(nameof(declaringType));
            if (methodInfo is null)
                throw new ArgumentNullException(nameof(methodInfo));

            return methodInfo.IsSpecialName && methodInfo.Name.StartsWith("op_", StringComparison.Ordinal)
              ? new OperatorAdapter(declaringType, methodInfo)
              : new MethodAdapter(declaringType, methodInfo);
        }

        /// <inheritdoc/>
        public virtual IEvent CreateEventMetadata(IType declaringType, EventInfo eventInfo)
            => new EventAdapter(declaringType, eventInfo);

        /// <inheritdoc/>
        public virtual IField CreateFieldMetadata(IType declaringType, FieldInfo fieldInfo)
            => new FieldAdapter(declaringType, fieldInfo);

        /// <inheritdoc/>
        public virtual IParameter CreateParameterMetadata(IMember member, ParameterInfo parameterInfo)
            => new ParameterAdapter(member, parameterInfo);

        /// <inheritdoc/>
        public virtual ICustomAttribute CreateCustomAttributeMetadata(CustomAttributeData attributeData, AttributeTarget target)
            => new CustomAttributeAdapter(attributeData, target);

        /// <summary>
        /// Creates type metadata for the specified type, using the provided declaring entity.
        /// </summary>
        /// <param name="declaringEntity">The declaring entity, which can be either an <see cref="IAssembly"/> or an <see cref="IType"/>.</param>
        /// <param name="type">The type for which to create metadata.</param>
        /// <returns>An <see cref="IType"/> representing the metadata of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringEntity"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is neither an <see cref="IAssembly"/> nor an <see cref="IType"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> does not declare <paramref name="type"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown when the specified type is not supported.</exception>
        protected virtual IType CreateSpecificTypeMetadata(object declaringEntity, Type type)
        {
            if (declaringEntity is null)
                throw new ArgumentNullException(nameof(declaringEntity));
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsGenericParameter)
            {
                if (declaringEntity is IAssembly assembly)
                    return new TypeParameterAdapter(assembly, type);

                throw new ArgumentException("A type parameter can only be declared by an assembly.", nameof(declaringEntity));
            }

            if (type.IsPrimitive)
                return new PrimitiveTypeAdapter(declaringEntity, type);

            else if (type.IsArray || type.IsPointer || type.IsByRef || Nullable.GetUnderlyingType(type) is not null)
                return new TypeDecoratorAdapter(declaringEntity, type);

            else if (type.IsEnum)
                return new EnumTypeAdapter(declaringEntity, type);

            if (type.IsValueType)
                return new StructTypeAdapter(declaringEntity, type);

            if (type.IsInterface)
                return new InterfaceTypeAdapter(declaringEntity, type);

            if (type.BaseType?.FullName == "System.MulticastDelegate")
                return new DelegateTypeAdapter(declaringEntity, type);

            if (type.IsClass)
                return new ClassTypeAdapter(declaringEntity, type);

            throw new NotSupportedException($"The type '{type.FullName ?? type.Name}' is not supported.");
        }
    }
}

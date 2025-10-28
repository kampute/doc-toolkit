﻿// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Manages the creation and caching of metadata adapters for members within an assembly.
    /// </summary>
    /// <remarks>
    /// This class is responsible for creating and caching metadata adapters for various member types
    /// such as types, methods, properties, events, fields, and constructors. It ensures that each member
    /// is associated with the correct assembly and declaring type context.
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class MemberAdapterRepository : IMemberAdapterRepository
    {
        private readonly ConcurrentDictionary<MemberInfo, IMember> cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberAdapterRepository"/> class with the default factory and comparer.
        /// </summary>
        /// <param name="assembly">The assembly metadata that contains the members.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> is <see langword="null"/>.</exception>
        public MemberAdapterRepository(IAssembly assembly)
            : this(assembly, MemberAdapterFactory.Instance, EqualityComparer<MemberInfo>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberAdapterRepository"/> class with the default factory and custom comparer.
        /// </summary>
        /// <param name="assembly">The assembly metadata that contains the members.</param>
        /// <param name="comparer">The equality comparer to use for member caching.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> or <paramref name="comparer"/> is <see langword="null"/>.</exception>
        public MemberAdapterRepository(IAssembly assembly, IEqualityComparer<MemberInfo> comparer)
            : this(assembly, MemberAdapterFactory.Instance, comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberAdapterRepository"/> class with the specified factory and default comparer.
        /// </summary>
        /// <param name="assembly">The assembly metadata that contains the members.</param>
        /// <param name="factory">The factory used to create member adapters.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> or <paramref name="factory"/> is <see langword="null"/>.</exception>
        public MemberAdapterRepository(IAssembly assembly, IMemberAdapterFactory factory)
            : this(assembly, factory, EqualityComparer<MemberInfo>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberAdapterRepository"/> class with the specified factory and custom comparer.
        /// </summary>
        /// <param name="assembly">The assembly metadata that contains the members.</param>
        /// <param name="factory">The factory used to create member adapters.</param>
        /// <param name="comparer">The equality comparer to use for member caching.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/>, <paramref name="factory"/>, or <paramref name="comparer"/> is <see langword="null"/>.</exception>
        public MemberAdapterRepository(IAssembly assembly, IMemberAdapterFactory factory, IEqualityComparer<MemberInfo> comparer)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            Factory = factory ?? throw new ArgumentNullException(nameof(factory));
            cache = new(comparer ?? throw new ArgumentNullException(nameof(comparer)));
        }

        /// <inheritdoc/>
        public IAssembly Assembly { get; }

        /// <summary>
        /// Gets the factory used to create member adapters.
        /// </summary>
        /// <value>
        /// The <see cref="IMemberAdapterFactory"/> instance used for creating member adapters.
        /// </value>
        protected IMemberAdapterFactory Factory { get; }

        /// <inheritdoc/>
        public virtual IType GetTypeMetadata(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            type = AdapterHelper.CanonicalizeType(type);

            return (IType)cache.GetOrAdd(type, _ => CreateTypeMetadata(type));
        }

        /// <inheritdoc/>
        public virtual IConstructor GetConstructorMetadata(ConstructorInfo constructorInfo)
        {
            if (constructorInfo is null)
                throw new ArgumentNullException(nameof(constructorInfo));

            return (IConstructor)cache.GetOrAdd(constructorInfo, _ => CreateConstructorMetadata(constructorInfo));
        }

        /// <inheritdoc/>
        public virtual IMethodBase GetMethodMetadata(MethodInfo methodInfo)
        {
            if (methodInfo is null)
                throw new ArgumentNullException(nameof(methodInfo));

            return (IMethodBase)cache.GetOrAdd(methodInfo, _ => CreateMethodMetadata(methodInfo));
        }

        /// <inheritdoc/>
        public virtual IProperty GetPropertyMetadata(PropertyInfo propertyInfo)
        {
            if (propertyInfo is null)
                throw new ArgumentNullException(nameof(propertyInfo));

            return (IProperty)cache.GetOrAdd(propertyInfo, _ => CreatePropertyMetadata(propertyInfo));
        }

        /// <inheritdoc/>
        public virtual IEvent GetEventMetadata(EventInfo eventInfo)
        {
            if (eventInfo is null)
                throw new ArgumentNullException(nameof(eventInfo));

            return (IEvent)cache.GetOrAdd(eventInfo, _ => CreateEventMetadata(eventInfo));
        }

        /// <inheritdoc/>
        public virtual IField GetFieldMetadata(FieldInfo fieldInfo)
        {
            if (fieldInfo is null)
                throw new ArgumentNullException(nameof(fieldInfo));

            return (IField)cache.GetOrAdd(fieldInfo, _ => CreateFieldMetadata(fieldInfo));
        }

        /// <inheritdoc/>
        public virtual IParameter GetParameterMetadata(ParameterInfo parameterInfo)
        {
            if (parameterInfo is null)
                throw new ArgumentNullException(nameof(parameterInfo));

            var member = ((IMemberAdapterRepository)this).GetMemberMetadata(parameterInfo.Member);
            return Factory.CreateParameterMetadata(member, parameterInfo);
        }

        /// <summary>
        /// Gets the custom attribute metadata for the specified attribute data within the assembly.
        /// </summary>
        /// <param name="attributeData">The reflection custom attribute data to get metadata for.</param>
        /// <param name="target">The target of the attribute.</param>
        /// <returns>A metadata representation of the specified custom attribute.</returns>
        public virtual ICustomAttribute GetCustomAttributeMetadata(CustomAttributeData attributeData, AttributeTarget target)
        {
            if (attributeData is null)
                throw new ArgumentNullException(nameof(attributeData));

            return Factory.CreateCustomAttributeMetadata(attributeData, target);
        }

        /// <summary>
        /// Creates type metadata for the specified reflection type.
        /// </summary>
        /// <param name="type">The reflection type to create metadata for.</param>
        /// <returns>The metadata representation of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided type does not belong to the assembly of this provider.</exception>
        /// <exception cref="NotSupportedException">Thrown when the type is not supported.</exception>
        protected virtual IType CreateTypeMetadata(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (type.DeclaringType is null || type.IsGenericParameter)
                return Factory.CreateTypeMetadata(Assembly, type);

            var declaringType = GetTypeMetadata(type.DeclaringType);
            return Factory.CreateTypeMetadata(declaringType, type);
        }

        /// <summary>
        /// Creates constructor metadata for the specified reflection constructor.
        /// </summary>
        /// <param name="constructorInfo">The reflection constructor to create metadata for.</param>
        /// <returns>The metadata representation of the specified constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided constructor does not belong to the assembly of this provider.</exception>
        protected virtual IConstructor CreateConstructorMetadata(ConstructorInfo constructorInfo)
        {
            if (constructorInfo is null)
                throw new ArgumentNullException(nameof(constructorInfo));

            var declaringType = GetTypeMetadata(constructorInfo.DeclaringType);
            return Factory.CreateConstructorMetadata(declaringType, constructorInfo);
        }

        /// <summary>
        /// Creates method metadata for the specified reflection method.
        /// </summary>
        /// <param name="methodInfo">The reflection method to create metadata for.</param>
        /// <returns>The metadata representation of the specified method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided method does not belong to the assembly of this provider.</exception>
        protected virtual IMethodBase CreateMethodMetadata(MethodInfo methodInfo)
        {
            if (methodInfo is null)
                throw new ArgumentNullException(nameof(methodInfo));

            var declaringType = GetTypeMetadata(methodInfo.DeclaringType);
            return Factory.CreateMethodMetadata(declaringType, methodInfo);
        }

        /// <summary>
        /// Creates property metadata for the specified reflection property.
        /// </summary>
        /// <param name="propertyInfo">The reflection property to create metadata for.</param>
        /// <returns>The metadata representation of the specified property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided property does not belong to the assembly of this provider.</exception>
        protected virtual IProperty CreatePropertyMetadata(PropertyInfo propertyInfo)
        {
            if (propertyInfo is null)
                throw new ArgumentNullException(nameof(propertyInfo));

            var declaringType = GetTypeMetadata(propertyInfo.DeclaringType);
            return Factory.CreatePropertyMetadata(declaringType, propertyInfo);
        }

        /// <summary>
        /// Creates event metadata for the specified reflection event.
        /// </summary>
        /// <param name="eventInfo">The reflection event to create metadata for.</param>
        /// <returns>The metadata representation of the specified event.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided event does not belong to the assembly of this provider.</exception>
        protected virtual IEvent CreateEventMetadata(EventInfo eventInfo)
        {
            if (eventInfo is null)
                throw new ArgumentNullException(nameof(eventInfo));

            var declaringType = GetTypeMetadata(eventInfo.DeclaringType);
            return Factory.CreateEventMetadata(declaringType, eventInfo);
        }

        /// <summary>
        /// Creates field metadata for the specified reflection field.
        /// </summary>
        /// <param name="fieldInfo">The reflection field to create metadata for.</param>
        /// <returns>The metadata representation of the specified field.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided field does not belong to the assembly of this provider.</exception>
        protected virtual IField CreateFieldMetadata(FieldInfo fieldInfo)
        {
            if (fieldInfo is null)
                throw new ArgumentNullException(nameof(fieldInfo));

            var declaringType = GetTypeMetadata(fieldInfo.DeclaringType);
            return Factory.CreateFieldMetadata(declaringType, fieldInfo);
        }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// An abstract adapter that wraps a composite <see cref="Type"/> such as a class or struct and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="Type"/> and the metadata
    /// representation defined by the <see cref="ICompositeType"/> interface. It provides access to composite
    /// type information regardless of whether the assembly containing the type was loaded via Common Language
    /// Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public abstract class CompositeTypeAdapter : GenericCapableTypeAdapter, ICompositeType
    {
        private readonly Lazy<IReadOnlyList<IInterfaceType>> interfaces;
        private readonly Lazy<IReadOnlyList<IType>> nestedTypes;
        private readonly Lazy<IReadOnlyList<IConstructor>> constructors;
        private readonly Lazy<IReadOnlyList<IField>> fields;
        private readonly Lazy<IReadOnlyList<IProperty>> properties;
        private readonly Lazy<IReadOnlyList<IMethod>> methods;
        private readonly Lazy<IReadOnlyList<IEvent>> events;
        private readonly Lazy<IReadOnlyList<IOperator>> operators;
        private readonly Lazy<IReadOnlyList<IProperty>> explicitInterfaceProperties;
        private readonly Lazy<IReadOnlyList<IMethod>> explicitInterfaceMethods;
        private readonly Lazy<IReadOnlyList<IEvent>> explicitInterfaceEvents;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeTypeAdapter"/> class.
        /// </summary>
        /// <param name="declaringEntity">The assembly or type that declares the type.</param>
        /// <param name="type">The reflection information of the type to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringEntity"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is neither an <see cref="IAssembly"/> nor an <see cref="IType"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not declared by the <paramref name="declaringEntity"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is a nested type but <paramref name="declaringEntity"/> is an assembly, or when <paramref name="type"/> is a top-level type but <paramref name="declaringEntity"/> is a type.</exception>
        protected CompositeTypeAdapter(object declaringEntity, Type type)
            : base(declaringEntity, type)
        {
            interfaces = new(() => [.. GetInterfaces()]);
            nestedTypes = new(() => [.. GetNestedTypes()]);
            constructors = new(() => [.. GetConstructors()]);
            fields = new(() => [.. GetFields()]);
            methods = new(() => [.. GetMethods()]);
            operators = new(() => [.. GetOperators()]);
            properties = new(() => [.. GetProperties()]);
            events = new(() => [.. GetEvents()]);
            explicitInterfaceMethods = new(() => [.. GetExplicitInterfaceMethods()]);
            explicitInterfaceProperties = new(() => [.. GetExplicitInterfaceProperties()]);
            explicitInterfaceEvents = new(() => [.. GetExplicitInterfaceEvents()]);
        }

        /// <inheritdoc/>
        public virtual IReadOnlyList<IInterfaceType> Interfaces => interfaces.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IConstructor> Constructors => constructors.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IField> Fields => fields.Value;

        /// <inheritdoc/>
        public virtual IReadOnlyList<IMethod> Methods => methods.Value;

        /// <inheritdoc/>
        public virtual IReadOnlyList<IProperty> Properties => properties.Value;

        /// <inheritdoc/>
        public virtual IReadOnlyList<IEvent> Events => events.Value;

        /// <inheritdoc/>
        public virtual IReadOnlyList<IOperator> Operators => operators.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IType> NestedTypes => nestedTypes.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IMethod> ExplicitInterfaceMethods => explicitInterfaceMethods.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IProperty> ExplicitInterfaceProperties => explicitInterfaceProperties.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IEvent> ExplicitInterfaceEvents => explicitInterfaceEvents.Value;

        /// <summary>
        /// Retrieves the interfaces implemented or inherited by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IInterfaceType"/> objects representing the interfaces implemented or inherited by the type.</returns>
        protected virtual IEnumerable<IInterfaceType> GetInterfaces()
        {
            return Reflection
                .GetInterfaces()
                .Where(i => i.IsPublic || i.IsNestedPublic || i.IsNestedFamily || i.IsNestedFamORAssem)
                .Select(MetadataProvider.GetMetadata<IInterfaceType>)
                .OrderBy(i => i.FullName, StringComparer.Ordinal);
        }

        /// <summary>
        /// Retrieves the types nested within the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IType"/> objects representing the types nested within the type.</returns>
        protected virtual IEnumerable<IType> GetNestedTypes() => Reflection
            .GetNestedTypes(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(t => !t.IsSpecialName && IsVisibleNestedType(t))
            .Select(Assembly.Repository.GetTypeMetadata)
            .OrderBy(t => t.Name, StringComparer.Ordinal);

        /// <summary>
        /// Retrieves the visible constructors declared by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IConstructor"/> objects representing the constructors declared by the type.</returns>
        protected virtual IEnumerable<IConstructor> GetConstructors() => Reflection
            .GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(IsVisibleMethod)
            .Select(Assembly.Repository.GetConstructorMetadata)
            .OrderBy(c => c.Parameters.Count);

        /// <summary>
        /// Retrieves the visible fields declared by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IField"/> objects representing the fields declared by the type.</returns>
        protected virtual IEnumerable<IField> GetFields() => Reflection
            .GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(f => !f.IsSpecialName && IsVisibleField(f))
            .Select(Assembly.Repository.GetFieldMetadata)
            .OrderBy(f => f.Name, StringComparer.Ordinal);

        /// <summary>
        /// Retrieves the visible methods declared by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IMethod"/> objects representing the methods declared by the type.</returns>
        protected virtual IEnumerable<IMethod> GetMethods() => Reflection
            .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName && IsVisibleMethod(m))
            .Select(Assembly.Repository.GetMethodMetadata<IMethod>)
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ThenBy(m => m.Parameters.Count);

        /// <summary>
        /// Retrieves the visible properties declared by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IProperty"/> objects representing the properties declared by the type.</returns>
        protected virtual IEnumerable<IProperty> GetProperties() => Reflection
            .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(p => !p.IsSpecialName && (IsVisibleMethod(p.GetMethod) || IsVisibleMethod(p.SetMethod)))
            .Select(Assembly.Repository.GetPropertyMetadata)
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ThenBy(p => p.Parameters.Count);

        /// <summary>
        /// Retrieves the visible events declared by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IEvent"/> objects representing the events declared by the type.</returns>
        protected virtual IEnumerable<IEvent> GetEvents() => Reflection
            .GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(e => !e.IsSpecialName && (IsVisibleMethod(e.AddMethod) || IsVisibleMethod(e.RemoveMethod)))
            .Select(Assembly.Repository.GetEventMetadata)
            .OrderBy(e => e.Name, StringComparer.Ordinal);

        /// <summary>
        /// Retrieves the operator methods declared by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IOperator"/> objects representing the operator methods declared by the type.</returns>
        protected virtual IEnumerable<IOperator> GetOperators() => Reflection
            .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.IsSpecialName && m.Name.StartsWith("op_", StringComparison.Ordinal))
            .Select(Assembly.Repository.GetMethodMetadata<IOperator>)
            .OrderBy(o => o.Name, StringComparer.Ordinal)
            .ThenBy(o => o.Parameters.Count);

        /// <summary>
        /// Retrieves the explicit interface methods implemented by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IMethod"/> objects representing the explicit interface methods implemented by the type.</returns>
        protected virtual IEnumerable<IMethod> GetExplicitInterfaceMethods() => Reflection
            .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName && IsExplicitMember(m))
            .Select(Assembly.Repository.GetMethodMetadata<IMethod>)
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ThenBy(m => m.Parameters.Count);

        /// <summary>
        /// Retrieves the explicit interface properties implemented by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IProperty"/> objects representing the explicit interface properties implemented by the type.</returns>
        protected virtual IEnumerable<IProperty> GetExplicitInterfaceProperties() => Reflection
            .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(p => !p.IsSpecialName && IsExplicitMember(p))
            .Select(Assembly.Repository.GetPropertyMetadata)
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ThenBy(p => p.Parameters.Count);

        /// <summary>
        /// Retrieves the explicit interface events implemented by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IEvent"/> objects representing the explicit interface events implemented by the type.</returns>
        protected virtual IEnumerable<IEvent> GetExplicitInterfaceEvents() => Reflection
            .GetEvents(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(e => !e.IsSpecialName && IsExplicitMember(e))
            .Select(Assembly.Repository.GetEventMetadata)
            .OrderBy(p => p.Name, StringComparer.Ordinal);

        /// <summary>
        /// Determines whether a method is visible based on its access modifiers.
        /// </summary>
        /// <param name="method">The method to check.</param>
        /// <returns><see langword="true"/> if the method is visible; otherwise, <see langword="false"/>.</returns>
        protected virtual bool IsVisibleMethod(MethodBase? method)
        {
            return method is not null
                && !method.IsPrivate
                && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);
        }

        /// <summary>
        /// Determines whether a field is visible based on its access modifiers.
        /// </summary>
        /// <param name="field">The field to check.</param>
        /// <returns><see langword="true"/> if the field is visible; otherwise, <see langword="false"/>.</returns>
        protected virtual bool IsVisibleField(FieldInfo field)
        {
            return field is not null
                && !field.IsPrivate
                && (field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly);
        }

        /// <summary>
        /// Determines whether a nested type is visible based on its access modifiers.
        /// </summary>
        /// <param name="type">The nested type to check.</param>
        /// <returns><see langword="true"/> if the nested type is visible; otherwise, <see langword="false"/>.</returns>
        protected virtual bool IsVisibleNestedType(Type type)
        {
            return type is not null
                && !type.IsNestedPrivate
                && (type.IsNestedPublic || type.IsNestedFamily || type.IsNestedFamORAssem);
        }

        /// <summary>
        /// Determines whether a member is an explicit interface implementation.
        /// </summary>
        /// <param name="member">The member to check.</param>
        /// <returns><see langword="true"/> if the member is an explicit interface implementation; otherwise, <see langword="false"/>.</returns>
        protected virtual bool IsExplicitMember(MemberInfo member)
        {
            return member is not null
                && member.Name.IndexOf('.') > 0
                && !member.CustomAttributes.Any(attr => attr.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
        }
    }
}

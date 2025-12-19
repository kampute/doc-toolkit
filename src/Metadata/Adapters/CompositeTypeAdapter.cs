// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

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
        private readonly Lazy<IReadOnlyList<IOperator>> explicitInterfaceOperators;

        private readonly Lazy<IReadOnlyList<IInterfaceType>> interfaces;
        private readonly Lazy<IReadOnlyList<IInterfaceType>> implementedInterfaces;

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
            nestedTypes = new(() => [.. GetNestedTypes()
                .Select(Assembly.Repository.GetTypeMetadata<IType>)
                .OrderByFullName()]);

            constructors = new(() => [.. GetConstructors()
                .Select(Assembly.Repository.GetConstructorMetadata)
                .OrderByParameterCount()]);

            fields = new(() => [.. GetFields()
                .Select(Assembly.Repository.GetFieldMetadata)
                .OrderByName()]);

            methods = new(() => [.. GetMethods()
                .Select(Assembly.Repository.GetMethodMetadata<IMethod>)
                .OrderByName()]);

            operators = new(() => [.. GetOperators()
                .Select(Assembly.Repository.GetMethodMetadata<IOperator>)
                .OrderByName()]);

            properties = new(() => [.. GetProperties()
                .Select(Assembly.Repository.GetPropertyMetadata)
                .OrderByName()]);

            events = new(() => [.. GetEvents()
                .Select(Assembly.Repository.GetEventMetadata)
                .OrderByName()]);

            explicitInterfaceMethods = new(() => [.. GetExplicitInterfaceMethods()
                .Select(Assembly.Repository.GetMethodMetadata<IMethod>)
                .OrderByName()]);

            explicitInterfaceProperties = new(() => [.. GetExplicitInterfaceProperties()
                .Select(Assembly.Repository.GetPropertyMetadata)
                .OrderByName()]);

            explicitInterfaceEvents = new(() => [.. GetExplicitInterfaceEvents()
                .Select(Assembly.Repository.GetEventMetadata)
                .OrderByName()]);

            explicitInterfaceOperators = new(() => [.. GetExplicitInterfaceOperators()
                .Select(Assembly.Repository.GetMethodMetadata<IOperator>)
                .OrderByName()]);

            interfaces = new(() => [.. GetInterfaces()
                .Select(MetadataProvider.GetMetadata<IInterfaceType>)
                .OrderByFullName()]);

            implementedInterfaces = new(() => [.. Interfaces.Except(GetIndirectInterfaces())]);
        }

        /// <inheritdoc/>
        public IReadOnlyList<IType> NestedTypes => nestedTypes.Value;

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
        public IReadOnlyList<IMethod> ExplicitInterfaceMethods => explicitInterfaceMethods.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IProperty> ExplicitInterfaceProperties => explicitInterfaceProperties.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IEvent> ExplicitInterfaceEvents => explicitInterfaceEvents.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IOperator> ExplicitInterfaceOperators => explicitInterfaceOperators.Value;

        /// <inheritdoc/>
        public virtual IReadOnlyList<IInterfaceType> Interfaces => interfaces.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IInterfaceType> ImplementedInterfaces => implementedInterfaces.Value;

        /// <summary>
        /// Retrieves the interfaces indirectly implemented by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IInterfaceType"/> objects representing the interfaces indirectly implemented by the type.</returns>
        protected virtual IEnumerable<IInterfaceType> GetIndirectInterfaces()
        {
            var allInterfaces = Interfaces;
            if (allInterfaces.Count == 0)
                return allInterfaces;

            var indirectInterfaces = allInterfaces.SelectMany(static i => i.Interfaces);
            return BaseType is not null && BaseType.HasInterfaces
                ? indirectInterfaces.Concat(BaseType.Interfaces)
                : indirectInterfaces;
        }

        /// <summary>
        /// Retrieves all the interfaces implemented or inherited by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IInterfaceType"/> objects representing the interfaces implemented or inherited by the type.</returns>
        protected virtual IEnumerable<Type> GetInterfaces()
        {
            return Reflection
                .GetInterfaces()
                .Where(static i => i.IsPublic || i.IsNestedPublic || i.IsNestedFamily || i.IsNestedFamORAssem);
        }

        /// <summary>
        /// Retrieves the types nested within the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IType"/> objects representing the types nested within the type.</returns>
        protected virtual IEnumerable<Type> GetNestedTypes() => Reflection
            .GetNestedTypes(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(t => !t.IsSpecialName && IsVisibleNestedType(t));

        /// <summary>
        /// Retrieves the visible constructors declared by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IConstructor"/> objects representing the constructors declared by the type.</returns>
        protected virtual IEnumerable<ConstructorInfo> GetConstructors() => Reflection
            .GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(IsVisibleMethod);

        /// <summary>
        /// Retrieves the visible fields declared by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IField"/> objects representing the fields declared by the type.</returns>
        protected virtual IEnumerable<FieldInfo> GetFields() => Reflection
            .GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(f => !f.IsSpecialName && IsVisibleField(f));

        /// <summary>
        /// Retrieves the visible methods declared by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IMethod"/> objects representing the methods declared by the type.</returns>
        protected virtual IEnumerable<MethodInfo> GetMethods() => Reflection
            .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName && IsVisibleMethod(m));

        /// <summary>
        /// Retrieves the visible properties declared by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IProperty"/> objects representing the properties declared by the type.</returns>
        protected virtual IEnumerable<PropertyInfo> GetProperties() => Reflection
            .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(p => !p.IsSpecialName && (IsVisibleMethod(p.GetMethod) || IsVisibleMethod(p.SetMethod)));

        /// <summary>
        /// Retrieves the visible events declared by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IEvent"/> objects representing the events declared by the type.</returns>
        protected virtual IEnumerable<EventInfo> GetEvents() => Reflection
            .GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(e => !e.IsSpecialName && (IsVisibleMethod(e.AddMethod) || IsVisibleMethod(e.RemoveMethod)));

        /// <summary>
        /// Retrieves the operator methods declared by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IOperator"/> objects representing the operator methods declared by the type.</returns>
        protected virtual IEnumerable<MethodInfo> GetOperators() => Reflection
            .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
            .Where(static m => m.IsSpecialName && m.Name.StartsWith("op_", StringComparison.Ordinal));

        /// <summary>
        /// Retrieves the explicit interface methods implemented by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IMethod"/> objects representing the explicit interface methods implemented by the type.</returns>
        protected virtual IEnumerable<MethodInfo> GetExplicitInterfaceMethods() => Reflection
            .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName && m.IsPrivate && m.IsFinal && m.IsVirtual && !IsCompilerGeneratedBridgeMethod(m));

        /// <summary>
        /// Retrieves the explicit interface properties implemented by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IProperty"/> objects representing the explicit interface properties implemented by the type.</returns>
        protected virtual IEnumerable<PropertyInfo> GetExplicitInterfaceProperties() => Reflection
            .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(p => !p.IsSpecialName && IsExplicitMember(p));

        /// <summary>
        /// Retrieves the explicit interface events implemented by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IEvent"/> objects representing the explicit interface events implemented by the type.</returns>
        protected virtual IEnumerable<EventInfo> GetExplicitInterfaceEvents() => Reflection
            .GetEvents(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(e => !e.IsSpecialName && IsExplicitMember(e));

        /// <summary>
        /// Retrieves the explicit interface operators implemented by the type.
        /// </summary>
        /// <returns>An enumeration of <see cref="IOperator"/> objects representing the explicit interface operators implemented by the type.</returns>
        protected virtual IEnumerable<MethodInfo> GetExplicitInterfaceOperators() => Reflection
            .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(m => m.IsPrivate && m.Name.Contains(".op_", StringComparison.Ordinal) && !IsCompilerGeneratedBridgeMethod(m));

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
                && (type.IsNestedPublic || type.IsNestedFamily || type.IsNestedFamORAssem)
                && !type.Name.StartsWith('<');
        }

        /// <summary>
        /// Determines whether a member is an explicit interface implementation.
        /// </summary>
        /// <param name="member">The reflection information of the member to check.</param>
        /// <returns><see langword="true"/> if the member is an explicit interface implementation; otherwise, <see langword="false"/>.</returns>
        protected virtual bool IsExplicitMember(MemberInfo member)
        {
            return member is not null
                && member.Name.IndexOf('.') > 0
                && !member.CustomAttributes.Any(static attr => attr.AttributeType.FullName == AttributeNames.CompilerGenerated);
        }

        /// <summary>
        /// Determines whether an explicit interface implementation is a compiler-generated bridge method.
        /// </summary>
        /// <param name="method">The reflection information of the method to check.</param>
        /// <returns><see langword="true"/> if the method is is a compiler-generated bridge method; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="method"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// A compiler-generated bridge method is created by the C# compiler to handle explicit interface implementations that
        /// involve 'in' parameters. These methods are not part of the original source code and should be excluded from metadata
        /// representation.
        /// <para>
        /// This method checks for the presence of 'in' parameters and attempts to find a corresponding public method for the
        /// explicit interface implementation. If such a method exists, it indicates that the explicit interface method is a
        /// compiler-generated bridge method.
        /// </para>
        /// </remarks>
        protected virtual bool IsCompilerGeneratedBridgeMethod(MethodInfo method)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));

            var parameters = method.GetParameters();
            if (!ContainsInParameter(parameters))
                return false;

            var methodName = method.Name.SubstringAfterLastOrSelf('.'); // Exclude interface name
            var bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | (method.IsStatic ? BindingFlags.Static : BindingFlags.Instance);

            return method.IsGenericMethod
                ? HasMatchingPublicGenericMethod()
                : HasMatchingPublicNonGenericMethod();


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool ContainsInParameter(ParameterInfo[] parameters)
            {
                for (var i = 0; i < parameters.Length; ++i)
                {
                    if (parameters[i].IsIn)
                        return true;
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool HasMatchingPublicNonGenericMethod()
            {
                return Reflection.GetMethod
                (
                    name: methodName,
                    bindingAttr: bindingFlags,
                    binder: null,
                    types: [.. parameters.Select(static p => p.ParameterType)],
                    modifiers: null
                ) is not null;
            }

            bool HasMatchingPublicGenericMethod()
            {
                var genericArity = method.GetGenericArguments().Length;
                foreach (var candidate in Reflection.GetMember(methodName, bindingFlags))
                {
                    if (candidate is not MethodInfo { IsGenericMethod: true } candidateMethod)
                        continue;

                    if (candidateMethod.GetGenericArguments().Length != genericArity)
                        continue;

                    var candidateParameters = candidateMethod.GetParameters();
                    if (candidateParameters.Length != parameters.Length)
                        continue;

                    var match = true;
                    for (var i = 0; i < parameters.Length && match; ++i)
                        match = ParametersMatch(parameters[i], candidateParameters[i]);

                    if (match)
                        return true;
                }

                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool ParametersMatch(ParameterInfo parameter1, ParameterInfo parameter2)
            {
                if (parameter1.ParameterType.IsGenericMethodParameter != parameter2.ParameterType.IsGenericMethodParameter)
                    return false;

                return parameter1.ParameterType.IsGenericMethodParameter
                    ? parameter1.ParameterType.GenericParameterPosition == parameter2.ParameterType.GenericParameterPosition
                    : AdapterHelper.HaveSameDeclarationScope(parameter1.ParameterType, parameter2.ParameterType);
            }
        }
    }
}

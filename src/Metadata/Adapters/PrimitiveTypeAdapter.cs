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
    /// An adapter that wraps a reflection <see cref="Type"/> representing a primitive type and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="Type"/> and the metadata
    /// representation defined by the <see cref="IPrimitiveType"/> interface. It provides access to type-level
    /// information of primitive types, regardless of whether the assembly containing the primitive type was
    /// loaded via Common Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class PrimitiveTypeAdapter : TypeAdapter, IPrimitiveType
    {
        private readonly Lazy<IReadOnlyList<IInterfaceType>> interfaces;
        private readonly Lazy<IReadOnlyList<IField>> fields;
        private readonly Lazy<IReadOnlyList<IMethod>> methods;
        private readonly Lazy<IReadOnlyList<IOperator>> operators;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveTypeAdapter"/> class.
        /// </summary>
        /// <param name="declaringEntity">The assembly or type that declares the primitive type.</param>
        /// <param name="type">The reflection information of the type to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringEntity"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is neither an <see cref="IAssembly"/> nor an <see cref="IType"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not declared by the <paramref name="declaringEntity"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is a nested type but <paramref name="declaringEntity"/> is an assembly, or when <paramref name="type"/> is a top-level type but <paramref name="declaringEntity"/> is a type.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not a primitive type.</exception>
        public PrimitiveTypeAdapter(object declaringEntity, Type type)
            : base(declaringEntity, type)
        {
            if (!type.IsPrimitive)
                throw new ArgumentException("Type must be a primitive type.", nameof(type));

            interfaces = new(() => [.. GetInterfaces()]);
            fields = new(() => [.. GetFields()]);
            methods = new(() => [.. GetMethods()]);
            operators = new(() => [.. GetOperators()]);
        }

        /// <inheritdoc/>
        public IReadOnlyList<IInterfaceType> Interfaces => interfaces.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IField> Fields => fields.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IMethod> Methods => methods.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IOperator> Operators => operators.Value;

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
        /// Retrieves the fields declared by the primitive type.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IField"/> objects representing the fields (usually constants) declared by the type.</returns>
        protected virtual IEnumerable<IField> GetFields() => Reflection
            .GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static)
            .Select(Assembly.Repository.GetFieldMetadata)
            .OrderBy(f => f.Name, StringComparer.Ordinal);

        /// <summary>
        /// Retrieves the methods declared by the primitive type.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IMethod"/> objects representing the methods declared by the type.</returns>
        protected virtual IEnumerable<IMethod> GetMethods() => Reflection
            .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName)
            .Select(Assembly.Repository.GetMethodMetadata<IMethod>)
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ThenBy(m => m.Parameters.Count);

        /// <summary>
        /// Retrieves the operators declared by the primitive type.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IOperator"/> objects representing the operators declared by the type.</returns>
        protected virtual IEnumerable<IOperator> GetOperators() => Reflection
            .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.IsSpecialName && m.Name.StartsWith("op_", StringComparison.Ordinal))
            .Select(Assembly.Repository.GetMethodMetadata<IOperator>)
            .OrderBy(o => o.Name, StringComparer.Ordinal)
            .ThenBy(o => o.Parameters.Count);
    }
}

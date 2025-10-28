// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// An adapter that wraps a <see cref="Type"/> representing a generic type parameter and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="Type"/> and the metadata
    /// representation defined by the <see cref="ITypeParameter"/> interface. It provides access to generic
    /// type parameter-level information regardless of whether the assembly containing the type was loaded
    /// via Common Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class TypeParameterAdapter : TypeAdapter, ITypeParameter
    {
        private readonly Lazy<IMember> declaringMember;
        private readonly Lazy<IReadOnlyList<IType>> typeConstraints;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeParameterAdapter"/> class.
        /// </summary>
        /// <param name="assembly">The assembly that declares the generic parameter.</param>
        /// <param name="type">The reflection information of the runtime generic parameter to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not declared by the <paramref name="assembly"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not a generic type parameter.</exception>
        public TypeParameterAdapter(IAssembly assembly, Type type)
            : base(assembly, type)
        {
            if (!type.IsGenericParameter)
                throw new ArgumentException("Type must be a generic parameter.", nameof(type));

            declaringMember = new(GetDeclaringMember);
            typeConstraints = new(() => [.. GetConstraintTypes()]);
        }

        /// <inheritdoc/>
        public IMember DeclaringMember => declaringMember.Value;

        /// <inheritdoc/>
        public override string Name => Reflection.Name;

        /// <inheritdoc/>
        public virtual int Position => Reflection.GenericParameterPosition;

        /// <inheritdoc/>
        public virtual TypeParameterVariance Variance
        {
            get
            {
                var attrs = Reflection.GenericParameterAttributes;

                if ((attrs & GenericParameterAttributes.Covariant) != 0)
                    return TypeParameterVariance.Covariant;

                if ((attrs & GenericParameterAttributes.Contravariant) != 0)
                    return TypeParameterVariance.Contravariant;

                return TypeParameterVariance.Invariant;
            }
        }

        /// <inheritdoc/>
        public virtual TypeParameterConstraints Constraints
        {
            get
            {
                var attrs = Reflection.GenericParameterAttributes;
                var constraints = TypeParameterConstraints.None;

                if ((attrs & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                    constraints |= TypeParameterConstraints.ReferenceType;

                if ((attrs & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                    constraints |= TypeParameterConstraints.ValueType;

                if ((attrs & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                    constraints |= TypeParameterConstraints.DefaultConstructor;

                /* .NET Standard 2.1 does not have the following flags:

                if ((attrs & GenericParameterAttributes.NotNullConstraint) != 0)
                    constraints |= TypeParameterConstraints.NotNull;

                if ((attrs & GenericParameterAttributes.UnmanagedTypeConstraint) != 0)
                    constraints |= TypeParameterConstraints.UnmanagedType;

                */

                return constraints;
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<IType> TypeConstraints => typeConstraints.Value;

        /// <inheritdoc/>
        public virtual bool IsGenericMethodParameter => Reflection.IsGenericMethodParameter;

        /// <inheritdoc/>
        public virtual bool IsGenericTypeParameter => Reflection.IsGenericTypeParameter;

        /// <inheritdoc/>
        public bool HasConstraints => Constraints != TypeParameterConstraints.None || TypeConstraints.Count > 0;

        /// <inheritdoc/>
        public sealed override bool IsDirectDeclaration => false;

        /// <inheritdoc/>
        public override bool IsAssignableFrom(IType source) => false;

        /// <inheritdoc/>
        public override bool IsSubstitutableBy(IType other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (other is ITypeParameter otherTypeParameter)
                return Accepts(otherTypeParameter);

            // Check reference type constraint
            if (Constraints.HasFlag(TypeParameterConstraints.ReferenceType) && other.IsValueType)
                return false;

            // Check value type constraint
            if (Constraints.HasFlag(TypeParameterConstraints.ValueType) && !other.IsValueType)
                return false;

            // Check default constructor constraint (only applies to reference types)
            if (Constraints.HasFlag(TypeParameterConstraints.DefaultConstructor) && !other.IsValueType)
            {
                if (other is not IWithConstructors sourceWithConstructors)
                    return false;

                if (!sourceWithConstructors.Constructors.Any(c => c.IsDefaultConstructor))
                    return false;
            }

            // Check type constraints
            return TypeConstraints.All(tc => tc.IsAssignableFrom(other));
        }

        /// <summary>
        /// Determines whether this type parameter accepts the specified type parameter as a valid substitution.
        /// </summary>
        /// <param name="source">The type parameter to evaluate for substitution compatibility.</param>
        /// <returns>
        /// <see langword="true"/> if this type parameter accepts the specified type parameter; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// A type parameter is accepted if it represents the same parameter position in the generic hierarchy,
        /// accounting for inheritance chains, method overrides, interface implementations, and type nesting.
        /// <para>
        /// This method is used to match type parameters across method overrides, interface implementations,
        /// and nested type declarations accessing outer type parameters.
        /// </para>
        /// </remarks>
        protected virtual bool Accepts(ITypeParameter source)
        {
            if (source is null)
                return false;

            if (ReferenceEquals(this, source))
                return true;

            if (Position != source.Position || IsGenericMethodParameter != source.IsGenericMethodParameter)
                return false;

            var targetMember = DeclaringMember;
            var sourceMember = source.DeclaringMember;

            if (ReferenceEquals(targetMember, sourceMember))
                return true;

            // Walk up the inheritance chain
            // Overridden methods and base types can access base type parameters
            try
            {
                var inheritedMember = sourceMember.GetInheritedMember();
                while (inheritedMember is not null)
                {
                    if (ReferenceEquals(targetMember, inheritedMember))
                        return true;

                    inheritedMember = inheritedMember.GetInheritedMember();
                }
            }
            catch (InvalidOperationException) when (IsGenericMethodParameter)
            {
                // A recursive call chain may occur if this function is called
                // as part of resolving the base declaration of a generic method.
                // In such case, this exception indicates that the source type
                // parameter and this type parameter are declared on the same
                // generic method definition.
                return true;
            }

            if (IsGenericTypeParameter)
            {
                // Check nesting relationships
                // Nested types can access outer type parameters
                var declaringType = sourceMember.DeclaringType;
                while (declaringType is not null)
                {
                    if (ReferenceEquals(targetMember, declaringType))
                        return true;

                    declaringType = declaringType.DeclaringType;
                }

                // Check implemented interfaces
                // Types can access type parameters declared on their interfaces
                if (sourceMember is IWithInterfaces typeWithInterfaces)
                {
                    return typeWithInterfaces.Interfaces
                        .Select(i => i.IsConstructedGenericType ? (IInterfaceType)i.GenericTypeDefinition! : i)
                        .Contains(targetMember, ReferenceEqualityComparer<IMember>.Instance);
                }
            }

            return false;
        }

        /// <inheritdoc/>
        protected override string ConstructFullName() => Name;

        /// <inheritdoc/>
        protected override string ConstructSignature() => IsGenericMethodParameter ? $"``{Position}" : $"`{Position}";

        /// <summary>
        /// Retrieves the member (type or method) that declares the generic type parameter.
        /// </summary>
        /// <returns>An <see cref="IMember"/> representing the declaring member, or <see langword="null"/> if not available.</returns>
        protected virtual IMember GetDeclaringMember()
            => Assembly.Repository.GetMemberMetadata(IsGenericMethodParameter ? Reflection.DeclaringMethod! : Reflection.DeclaringType!);

        /// <summary>
        /// Retrieves the constraint types for the type parameter.
        /// </summary>
        /// <returns>An enumerable collection <see cref="IType"/> representing the constraint types.</returns>
        protected virtual IEnumerable<IType> GetConstraintTypes()
            => Reflection.GetGenericParameterConstraints().Select(MetadataProvider.GetMetadata);

        /// <inheritdoc/>
        protected override ICustomAttribute CreateAttributeMetadata(CustomAttributeData attribute)
            => Assembly.Repository.GetCustomAttributeMetadata(attribute, AttributeTarget.TypeParameter);
    }
}

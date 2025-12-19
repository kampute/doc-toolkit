// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

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
        private readonly Lazy<TypeParameterConstraints> constraints;
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
            constraints = new(GetConstraints);
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
                var attributes = Reflection.GenericParameterAttributes & GenericParameterAttributes.VarianceMask;

                if (attributes.HasFlag(GenericParameterAttributes.Covariant))
                    return TypeParameterVariance.Covariant;

                if (attributes.HasFlag(GenericParameterAttributes.Contravariant))
                    return TypeParameterVariance.Contravariant;

                return TypeParameterVariance.Invariant;
            }
        }

        /// <inheritdoc/>
        public TypeParameterConstraints Constraints => constraints.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IType> TypeConstraints => typeConstraints.Value;

        /// <inheritdoc/>
        public sealed override bool IsDirectDeclaration => false;

        /// <inheritdoc/>
        public virtual bool IsGenericMethodParameter => Reflection.IsGenericMethodParameter;

        /// <inheritdoc/>
        public virtual bool IsGenericTypeParameter => Reflection.IsGenericTypeParameter;

        /// <inheritdoc/>
        public bool HasConstraints => Constraints != TypeParameterConstraints.None || TypeConstraints.Count > 0;

        /// <inheritdoc/>
        public override bool IsAssignableFrom(IType source) => false;

        /// <inheritdoc/>
        public virtual bool IsSatisfiableBy(IType type)
        {
            if (type is null)
                return false;

            if (type is ITypeParameter typeParameter)
                return IsSatisfiableBy(typeParameter);

            // Check reference type constraint, disallow value and by-ref-like types
            if (Constraints.HasFlag(TypeParameterConstraints.ReferenceType) && type is not IClassType)
                return false;

            // Check not-nullable value type constraint, disallow reference types
            if (Constraints.HasFlag(TypeParameterConstraints.NotNullableValueType) && !type.IsValueType)
                return false;

            // Check by-ref-like constraint, disallow by-ref-like types if not allowed
            if (!Constraints.HasFlag(TypeParameterConstraints.AllowByRefLike) && type is IStructType { IsRefLike: true })
                return false;

            // Check default constructor constraint, disallow types without a default constructor
            if (Constraints.HasFlag(TypeParameterConstraints.DefaultConstructor) && !HasDefaultConstructor(type))
                return false;

            // Check type constraints
            return TypeConstraints.All(tc => tc.IsAssignableFrom(type));
        }

        /// <inheritdoc/>
        public virtual bool IsSatisfiableBy(ITypeParameter other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other is null)
                return false;

            // Check reference type constraint matching
            if (IsConstraintNotSatisfied(TypeParameterConstraints.NotNullableValueType))
                return false;

            // Check by-ref-like constraint matching
            if (IsConstraintNotSatisfied(TypeParameterConstraints.AllowByRefLike))
                return false;

            // Check not-nullable value type constraint matching
            if (IsConstraintNotSatisfied(TypeParameterConstraints.ReferenceType))
            {
                // If the other missing the not-nullable value type constraint,
                // it must have type constraints that are all reference types
                if (other.TypeConstraints.Count == 0 || !other.TypeConstraints.All(static tc => tc is IClassType))
                    return false;
            }

            // Check default constructor constraint matching
            if (IsConstraintNotSatisfied(TypeParameterConstraints.DefaultConstructor))
            {
                // If the other is missing the default constructor constraint,
                // it must have type constraints that all have default constructors
                if (other.TypeConstraints.Count == 0 || !other.TypeConstraints.All(HasDefaultConstructor))
                    return false;
            }

            return TypeConstraints.All(tc => other.TypeConstraints.Any(cct => tc.IsAssignableFrom(cct)));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool IsConstraintNotSatisfied(TypeParameterConstraints flag)
                => Constraints.HasFlag(flag) && !other.Constraints.HasFlag(flag);
        }

        /// <inheritdoc/>
        public override bool Equals(IType? other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return other is ITypeParameter otherParam
                && otherParam.Name == Name
                && otherParam.Position == Position
                && otherParam.Variance == Variance
                && otherParam.Constraints == Constraints
                && otherParam.IsGenericMethodParameter == IsGenericMethodParameter
                && otherParam.IsGenericTypeParameter == IsGenericTypeParameter
                && otherParam.TypeConstraints.SequenceEqual(TypeConstraints);
        }

        /// <inheritdoc/>
        protected override string ConstructFullName() => Name;

        /// <inheritdoc/>
        protected override string ConstructSignature(bool useParameterNotation) => IsGenericMethodParameter ? $"``{Position}" : $"`{Position}";

        /// <summary>
        /// Retrieves the member (type or method) that declares the generic type parameter.
        /// </summary>
        /// <returns>An <see cref="IMember"/> representing the declaring member, or <see langword="null"/> if not available.</returns>
        protected virtual IMember GetDeclaringMember()
            => Assembly.Repository.GetMemberMetadata(IsGenericMethodParameter ? Reflection.DeclaringMethod! : Reflection.DeclaringType!);

        /// <summary>
        /// Retrieves the constraints applied to the type parameter.
        /// </summary>
        /// <returns>A <see cref="TypeParameterConstraints"/> value representing the constraints.</returns>
        protected virtual TypeParameterConstraints GetConstraints()
        {
            var parameterAttributes = Reflection.GenericParameterAttributes;
            var parameterConstraints = TypeParameterConstraints.None;

            if (parameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
                parameterConstraints |= TypeParameterConstraints.ReferenceType;

            if (parameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                parameterConstraints |= TypeParameterConstraints.NotNullableValueType;

            if (parameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                parameterConstraints |= TypeParameterConstraints.DefaultConstructor;

            if (parameterAttributes.HasFlag((GenericParameterAttributes)32 /* AllowByRefLike */))
                parameterConstraints |= TypeParameterConstraints.AllowByRefLike;

            return parameterConstraints;
        }

        /// <summary>
        /// Retrieves the constraint types for the type parameter.
        /// </summary>
        /// <returns>An enumerable collection <see cref="IType"/> representing the constraint types.</returns>
        protected virtual IEnumerable<IType> GetConstraintTypes()
            => Reflection.GetGenericParameterConstraints().Select(MetadataProvider.GetMetadata);

        /// <inheritdoc/>
        protected override ICustomAttribute CreateAttributeMetadata(CustomAttributeData attribute)
            => Assembly.Repository.GetCustomAttributeMetadata(attribute, AttributeTarget.TypeParameter);

        /// <summary>
        /// Determines whether the specified type has a default constructor.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><see langword="true"/> if the type has a default constructor; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool HasDefaultConstructor(IType type)
        {
            if (type.IsValueType)
                return true;

            if (type.IsInterface || type.IsEnum)
                return false;

            return IsAccessibleType(type)
                && type is IWithConstructors { HasConstructors: true } typeWithConstructors
                && typeWithConstructors.Constructors.Any(static c => c.IsPublic && c.IsDefaultConstructor);
        }

        /// <summary>
        /// Determines whether the specified type and its declaring types are accessible.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><see langword="true"/> if the type and its declaring types are accessible; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAccessibleType(IType type)
        {
            for (var current = type; current is not null; current = current.DeclaringType)
            {
                if (!current.IsPublic || current is IClassType { IsAbstract: true })
                    return false;
            }

            return true;
        }
    }
}
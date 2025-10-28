// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Reflection;

    /// <summary>
    /// An abstract base class for adapters that wrap reflected types and provide metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="Type"/> and the metadata
    /// representation defined by the <see cref="IType"/> interface. It provides access to type-level
    /// information regardless of whether the assembly containing the type was loaded via Common
    /// Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public abstract class TypeAdapter : MemberAdapter<Type>, IType
    {
        private readonly Lazy<IClassType?> baseType;
        private string? signature;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeAdapter"/> class.
        /// </summary>
        /// <param name="declaringEntity">The assembly or type that declares the type.</param>
        /// <param name="type">The reflection information of the type to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringEntity"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is neither an <see cref="IAssembly"/> nor an <see cref="IType"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not declared by the <paramref name="declaringEntity"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is a nested type but <paramref name="declaringEntity"/> is an assembly, or when <paramref name="type"/> is a top-level type but <paramref name="declaringEntity"/> is a type.</exception>
        protected TypeAdapter(object declaringEntity, Type type)
            : base(type)
        {
            switch (declaringEntity)
            {
                case null:
                    throw new ArgumentNullException(nameof(declaringEntity));
                case IAssembly assembly:
                    if (!assembly.Represents(type.Assembly))
                        throw new ArgumentException("The type must belong to the specified assembly.", nameof(type));
                    if (type.IsNested && !type.IsGenericParameter)
                        throw new ArgumentException("The type must be a top-level type.", nameof(type));

                    Assembly = assembly;
                    break;
                case IType declaringType when !type.IsGenericParameter:
                    if (!declaringType.Assembly.Represents(type.Assembly))
                        throw new ArgumentException("The type must belong to the specified assembly.", nameof(type));
                    if (!type.IsNested)
                        throw new ArgumentException("The type must be a nested type.", nameof(type));

                    Assembly = declaringType.Assembly;
                    DeclaringType = declaringType;
                    break;
                case IType declaringType when type.IsGenericParameter:
                    throw new ArgumentException("The generic parameter type must be declared by an assembly.", nameof(type));
                default:
                    throw new ArgumentException("The declaring entity must be either an assembly or a type.", nameof(declaringEntity));
            }

            FullName = ConstructFullName();
            baseType = new(GetBaseType);
        }

        /// <inheritdoc/>
        public override IAssembly Assembly { get; }

        /// <inheritdoc/>
        public override string Namespace => Reflection.Namespace ?? string.Empty;

        /// <inheritdoc/>
        public override IType? DeclaringType { get; }

        /// <inheritdoc/>
        public string FullName { get; }

        /// <inheritdoc/>
        public virtual IClassType? BaseType => baseType.Value;

        /// <inheritdoc/>
        public override bool IsStatic => Reflection.IsAbstract && Reflection.IsSealed;

        /// <inheritdoc/>
        public override bool IsUnsafe => false;

        /// <inheritdoc/>
        public override bool IsSpecialName => Reflection.IsSpecialName;

        /// <inheritdoc/>
        public virtual bool IsNested => Reflection.IsNested && !Reflection.IsGenericParameter;

        /// <inheritdoc/>
        public virtual bool IsValueType => Reflection.IsValueType;

        /// <inheritdoc/>
        public virtual bool IsGenericType => Reflection.IsGenericType;

        /// <inheritdoc/>
        public string Signature => signature ??= ConstructSignature();

        /// <inheritdoc/>
        public sealed override string CodeReference => $"T:{Signature}";

        /// <inheritdoc/>
        public override bool Represents(Type reflection) => base.Represents(AdapterHelper.CanonicalizeType(reflection));

        /// <inheritdoc/>
        public virtual bool IsSubstitutableBy(IType other) => Equals(other);

        /// <inheritdoc/>
        public virtual bool IsAssignableFrom(IType source)
        {
            if (source is null)
                return false;

            for (var sourceType = source; sourceType is not null; sourceType = sourceType.BaseType)
            {
                if (Equals(sourceType))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified type is equal to this type based on their signatures.
        /// </summary>
        /// <param name="other">The type to compare with the current type.</param>
        /// <returns><see langword="true"/> if the specified type has the same signature as this type; otherwise, <see langword="false"/>.</returns>
        public virtual bool Equals(IType? other) => other is not null && other.Signature == Signature;

        /// <summary>
        /// Determines whether the specified object is equal to this type.
        /// </summary>
        /// <param name="obj">The object to compare with the current type.</param>
        /// <returns><see langword="true"/> if the specified object is an <see cref="IType"/> with the same signature as this type; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj) => obj is IType other && Equals(other);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>The hash code for the current object.</returns>
        public override int GetHashCode() => Signature.GetHashCode();

        /// <inheritdoc/>
        protected override MemberVisibility GetMemberVisibility()
        {
            if (Reflection.IsPublic || Reflection.IsNestedPublic)
                return MemberVisibility.Public;
            else if (Reflection.IsNestedFamily)
                return MemberVisibility.Protected;
            else if (Reflection.IsNestedAssembly)
                return MemberVisibility.Internal;
            else if (Reflection.IsNestedFamANDAssem)
                return MemberVisibility.PrivateProtected;
            else if (Reflection.IsNestedFamORAssem)
                return MemberVisibility.ProtectedInternal;
            else
                return MemberVisibility.Private;
        }

        /// <summary>
        /// Retrieves the metadata for the base type of the current type.
        /// </summary>
        /// <returns>The metadata for the base type, or <see langword="null"/> if the type has no base type.</returns>
        protected virtual IClassType? GetBaseType() => Reflection.BaseType?.GetMetadata<IClassType>();

        /// <summary>
        /// Constructs the full name of the type when <see cref="Type.FullName"/> is <see langword="null"/>.
        /// </summary>
        /// <returns>The full name of the type.</returns>
        protected virtual string ConstructFullName()
        {
            if (Reflection.FullName is not null)
                return Reflection.FullName.SubstringBeforeOrSelf('[');

            if (DeclaringType is not null)
                return $"{DeclaringType.FullName}+{Name}";

            if (!string.IsNullOrEmpty(Namespace))
                return $"{Namespace}.{Name}";

            return Name;
        }

        /// <summary>
        /// Constructs the signature of the type.
        /// </summary>
        /// <returns>A string representing the type's signature used in code references.</returns>
        protected virtual string ConstructSignature()
        {
            if (DeclaringType is not null)
                return $"{DeclaringType.Signature}.{Name}";

            if (!string.IsNullOrEmpty(Namespace))
                return $"{Namespace}.{Name}";

            return Name;
        }

        /// <inheritdoc/>
        protected override ICustomAttribute CreateAttributeMetadata(CustomAttributeData attribute)
            => Assembly.Repository.GetCustomAttributeMetadata(attribute, AttributeTarget.Type);

        /// <inheritdoc/>
        public override string ToString() => FullName;
    }
}

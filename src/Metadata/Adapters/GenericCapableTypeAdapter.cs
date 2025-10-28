// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An abstract adapter that wraps a generic-capable <see cref="Type"/> and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="Type"/> and the metadata representation
    /// defined by the <see cref="IGenericCapableType"/> interface. It provides access to generic type information
    /// regardless of whether the assembly containing the type was loaded via Common Language Runtime (CLR) or Metadata
    /// Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public abstract class GenericCapableTypeAdapter : TypeAdapter, IGenericCapableType
    {
        private readonly Lazy<IReadOnlyList<ITypeParameter>> typeParameters;
        private readonly Lazy<IReadOnlyList<IType>> typeArguments;
        private readonly Lazy<IGenericCapableType?> genericTypeDefinition;
        private (int, int)? ownGenericArity;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericCapableTypeAdapter"/> class.
        /// </summary>
        /// <param name="declaringEntity">The assembly or type that declares the type.</param>
        /// <param name="type">The reflection information of the type to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringEntity"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is neither an <see cref="IAssembly"/> nor an <see cref="IType"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not declared by the <paramref name="declaringEntity"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is a nested type but <paramref name="declaringEntity"/> is an assembly, or when <paramref name="type"/> is a top-level type but <paramref name="declaringEntity"/> is a type.</exception>
        protected GenericCapableTypeAdapter(object declaringEntity, Type type)
            : base(declaringEntity, type)
        {
            typeParameters = new(() => [.. GetTypeParameters()]);
            typeArguments = new(() => [.. GetTypeArguments()]);
            genericTypeDefinition = new(GetGenericTypeDefinition);
        }

        /// <inheritdoc/>
        public string UnqualifiedName => Name.SubstringBeforeLastOrSelf('`');

        /// <inheritdoc/>
        public virtual bool IsGenericTypeDefinition => Reflection.IsGenericTypeDefinition;

        /// <inheritdoc/>
        public virtual bool IsConstructedGenericType => Reflection.IsConstructedGenericType;

        /// <inheritdoc/>
        public IReadOnlyList<ITypeParameter> TypeParameters => typeParameters.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IType> TypeArguments => typeArguments.Value;

        /// <inheritdoc/>
        public IGenericCapableType? GenericTypeDefinition => genericTypeDefinition.Value;

        /// <inheritdoc/>
        public (int Offset, int Count) OwnGenericParameterRange => ownGenericArity ??= GetOwnGenericParameterRange();

        /// <inheritdoc/>
        public override bool IsDirectDeclaration => !IsGenericType || IsGenericTypeDefinition;

        /// <inheritdoc/>
        protected override string ConstructSignature()
        {
            if (!IsGenericType || IsGenericTypeDefinition)
                return base.ConstructSignature();

            using var reusable = StringBuilderPool.Shared.GetBuilder();
            var sb = reusable.Builder;

            var chain = new Stack<IType>();
            chain.Push(this);
            for (var declaringType = DeclaringType; declaringType is not null; declaringType = declaringType.DeclaringType)
                chain.Push(declaringType);

            var nonGenericDeclaringType = default(IType);
            while (chain.TryPeek(out var type))
            {
                if (type.IsGenericType)
                    break;

                chain.Pop();
                nonGenericDeclaringType = type;
            }

            if (nonGenericDeclaringType is not null)
                sb.Append(nonGenericDeclaringType.Signature).Append('.');
            else if (!string.IsNullOrEmpty(Namespace))
                sb.Append(Namespace).Append('.');

            var typeArgs = TypeArguments;
            var typeArgIndex = 0;
            while (chain.TryPop(out var type))
            {
                if (type.IsGenericType)
                {
                    var genericType = (IGenericCapableType)type;
                    var arity = genericType.OwnGenericParameterRange.Count;
                    sb.Append(genericType.UnqualifiedName);
                    sb.Append('{');
                    for (var i = 0; i < arity; ++i)
                    {
                        if (i > 0)
                            sb.Append(',');

                        sb.Append(typeArgs[typeArgIndex++].Signature);
                    }
                    sb.Append('}');
                }
                else
                {
                    sb.Append(type.Name);
                }

                if (chain.Count > 0)
                    sb.Append('.');
            }

            return sb.ToString();
        }

        /// <summary>
        /// Retrieves the type parameters defined by the type if the type is a generic type definition.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="ITypeParameter"/> objects representing the type parameters defined by the type.</returns>
        protected virtual IEnumerable<ITypeParameter> GetTypeParameters()
            => IsGenericTypeDefinition ? Reflection.GetGenericArguments().Select(MetadataProvider.GetMetadata<ITypeParameter>) : [];

        /// <summary>
        /// Retrieves the type arguments provided to the type if the type is a constructed generic type.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IType"/> objects representing the type arguments provided to the type.</returns>
        protected virtual IEnumerable<IType> GetTypeArguments()
            => IsConstructedGenericType ? Reflection.GetGenericArguments().Select(MetadataProvider.GetMetadata) : [];

        /// <summary>
        /// Retrieves the generic type definition if the type is a constructed generic type.
        /// </summary>
        /// <returns>The <see cref="IGenericCapableType"/> representing the generic type definition, or <see langword="null"/> if the type is not a constructed generic type.</returns>
        protected IGenericCapableType? GetGenericTypeDefinition()
            => IsConstructedGenericType ? MetadataProvider.GetMetadata<IGenericCapableType>(Reflection.GetGenericTypeDefinition()) : null;

        /// <summary>
        /// Retrieves the offset and number of generic parameters belonging exclusively to this type,
        /// excluding any from its declaring type, if the type is generic.
        /// </summary>
        /// <returns>A tuple containing the offset and number of generic parameters belonging exclusively to this type.</returns>
        protected (int Offset, int Count) GetOwnGenericParameterRange()
        {
            var ownedOffset = 0;
            var ownedCount = Reflection.GetGenericArguments().Length;
            if (DeclaringType is IGenericCapableType genericCapableDeclaringType)
            {
                var (offset, count) = genericCapableDeclaringType.OwnGenericParameterRange;
                ownedOffset += offset + count;
                ownedCount -= count;
            }

            return (ownedOffset, ownedCount);
        }
    }
}

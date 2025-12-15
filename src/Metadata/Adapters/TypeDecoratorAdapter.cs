// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using System;
    using System.Linq;

    /// <summary>
    /// An adapter that wraps a reflection <see cref="Type"/> representing an array, pointer, by-ref, or nullable type
    /// and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="Type"/> and the metadata representation
    /// defined by the <see cref="ITypeDecorator"/> interface. It provides access type-level information regardless of
    /// whether the assembly containing the type was loaded via Common Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class TypeDecoratorAdapter : TypeAdapter, ITypeDecorator
    {
        private readonly Lazy<IType> elementType;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDecoratorAdapter"/> class.
        /// </summary>
        /// <param name="declaringEntity">The assembly or type that declares the decorated type.</param>
        /// <param name="type">The reflection information of the array, pointer, by-ref, or nullable type to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringEntity"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is neither an <see cref="IAssembly"/> nor an <see cref="IType"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not declared by the <paramref name="declaringEntity"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is a nested type but <paramref name="declaringEntity"/> is an assembly, or when <paramref name="type"/> is a top-level type but <paramref name="declaringEntity"/> is a type.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not an array, pointer, by-ref, or nullable type.</exception>
        public TypeDecoratorAdapter(object declaringEntity, Type type)
            : base(declaringEntity, type)
        {
            Modifier = GetElementModifier(type);
            elementType = new(GetElementType);
        }

        /// <inheritdoc/>
        public override bool IsUnsafe => Modifier == TypeModifier.Pointer || ElementType.IsUnsafe;

        /// <inheritdoc/>
        public TypeModifier Modifier { get; }

        /// <inheritdoc/>
        public IType ElementType => elementType.Value;

        /// <inheritdoc/>
        public virtual int ArrayRank => Modifier == TypeModifier.Array ? Reflection.GetArrayRank() : 0;

        /// <inheritdoc/>
        public sealed override bool IsDirectDeclaration => false;

        /// <inheritdoc/>
        public override bool IsAssignableFrom(IType source)
        {
            return base.IsAssignableFrom(source)
                || (Modifier is TypeModifier.Nullable && ElementType.IsAssignableFrom(source));
        }

        /// <summary>
        /// Determines the kind of decoration applied to the specified type.
        /// </summary>
        /// <param name="type">The type to evaluate.</param>
        /// <returns>The corresponding <see cref="TypeModifier"/> value</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        protected virtual TypeModifier GetElementModifier(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsArray)
                return TypeModifier.Array;
            if (type.IsByRef)
                return TypeModifier.ByRef;
            if (type.IsPointer)
                return TypeModifier.Pointer;
            if (Nullable.GetUnderlyingType(type) is not null)
                return TypeModifier.Nullable;

            throw new ArgumentException("The provided type is not an array, pointer, by-ref, or nullable type.", nameof(type));
        }

        /// <summary>
        /// Retrieves the underlying element type of the decorator type.
        /// </summary>
        /// <returns>The adapted element <see cref="IType"/> instance.</returns>
        protected virtual IType GetElementType()
        {
            var type = Modifier == TypeModifier.Nullable ? Nullable.GetUnderlyingType(Reflection)! : Reflection.GetElementType()!;
            return MetadataProvider.GetMetadata(type);
        }

        /// <inheritdoc/>
        protected override string ConstructSignature(bool useParameterNotation) => Modifier switch
        {
            TypeModifier.Nullable => $"System.Nullable{{{ElementType.Signature}}}",
            TypeModifier.Array when ArrayRank > 1 => $"{ElementType.Signature}[{string.Join(',', Enumerable.Repeat("0:", ArrayRank))}]",
            TypeModifier.Array => $"{ElementType.Signature}[]",
            TypeModifier.ByRef => $"{ElementType.Signature}@",
            TypeModifier.Pointer => $"{ElementType.Signature}*",
            _ => base.ConstructSignature(useParameterNotation)
        };
    }
}

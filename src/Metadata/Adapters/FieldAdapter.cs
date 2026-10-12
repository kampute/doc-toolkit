// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// An adapter that wraps a <see cref="FieldInfo"/> and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="FieldInfo"/> and the metadata
    /// representation defined by the <see cref="IField"/> interface. It provides access to field-level
    /// information regardless of whether the assembly containing the field's type was loaded via Common
    /// Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class FieldAdapter : TypeMemberAdapter<FieldInfo>, IField
    {
        private readonly Lazy<IType> fieldType;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAdapter"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the field.</param>
        /// <param name="field">The field to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="field"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="field"/> is not a field of <paramref name="declaringType"/>.</exception>
        public FieldAdapter(IType declaringType, FieldInfo field)
            : base(declaringType, field)
        {
            fieldType = new(GetFieldType);
        }

        /// <inheritdoc/>
        public IType Type => fieldType.Value;

        /// <inheritdoc/>
        public override bool IsSpecialName => Reflection.IsSpecialName;

        /// <inheritdoc/>
        public override bool IsStatic => Reflection.IsStatic;

        /// <inheritdoc/>
        public override bool IsUnsafe => Type.IsUnsafe || IsFixedSizeBuffer;

        /// <inheritdoc/>
        public virtual bool IsReadOnly => Reflection.IsInitOnly;

        /// <inheritdoc/>
        public virtual bool IsVolatile => HasRequiredCustomModifier(ModifierNames.IsVolatile);

        /// <inheritdoc/>
        public virtual bool IsFixedSizeBuffer => HasCustomAttribute(AttributeNames.FixedBuffer);

        /// <inheritdoc/>
        public virtual bool IsLiteral => Reflection.IsLiteral;

        /// <inheritdoc/>
        public virtual object? LiteralValue => Reflection.IsLiteral ? Reflection.GetRawConstantValue() : null;

        /// <inheritdoc/>
        public virtual bool TryGetFixedSizeBufferInfo([NotNullWhen(true)] out IType? elementType, out int length)
        {
            if (TryGetFixedSizeBufferAttributeArguments(out var bufferType, out length))
            {
                elementType = MetadataProvider.GetMetadata(bufferType);
                return true;
            }

            elementType = null;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool HasRequiredCustomModifier(string modifierFullName)
        {
            foreach (var modifier in Reflection.GetRequiredCustomModifiers())
            {
                if (modifier.FullName == modifierFullName)
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual bool HasOptionalCustomModifier(string modifierFullName)
        {
            foreach (var modifier in Reflection.GetOptionalCustomModifiers())
            {
                if (modifier.FullName == modifierFullName)
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        protected override MemberVisibility GetMemberVisibility()
        {
            if (Reflection.IsPublic)
                return MemberVisibility.Public;
            else if (Reflection.IsFamily)
                return MemberVisibility.Protected;
            else if (Reflection.IsAssembly)
                return MemberVisibility.Internal;
            else if (Reflection.IsFamilyAndAssembly)
                return MemberVisibility.PrivateProtected;
            else if (Reflection.IsFamilyOrAssembly)
                return MemberVisibility.ProtectedInternal;
            else
                return MemberVisibility.Private;
        }

        /// <inheritdoc/>
        protected sealed override (char, string) GetCodeReferenceParts() => ('F', Name);

        /// <summary>
        /// Retrieves the type of the field.
        /// </summary>
        /// <returns>An <see cref="IType"/> representing the type of the field.</returns>
        /// <remarks>
        /// The type of a fixed-size buffer is an array of its element type, as the buffer is declared in C#, rather than the
        /// compiler-generated structure that holds the buffer.
        /// </remarks>
        protected virtual IType GetFieldType()
        {
            return TryGetFixedSizeBufferAttributeArguments(out var bufferType, out _)
                ? bufferType.MakeArrayType().GetMetadata()
                : Reflection.FieldType.GetMetadata();
        }

        /// <summary>
        /// Attempts to get the element type and length of the fixed-size buffer from its <c>FixedBuffer</c> attribute.
        /// </summary>
        /// <param name="elementType">When this method returns, contains the element type of the buffer if the field is a fixed-size buffer; otherwise, <see langword="null"/>.</param>
        /// <param name="length">When this method returns, contains the length of the buffer if the field is a fixed-size buffer; otherwise, 0.</param>
        /// <returns><see langword="true"/> if the field is a fixed-size buffer; otherwise, <see langword="false"/>.</returns>
        private bool TryGetFixedSizeBufferAttributeArguments([NotNullWhen(true)] out Type? elementType, out int length)
        {
            var args = GetCustomAttributes()
                .FirstOrDefault(static a => a.AttributeType.FullName == AttributeNames.FixedBuffer)?
                .ConstructorArguments;

            if
            (
                args is not null &&
                args.Count == 2 &&
                args[0].ArgumentType.FullName == "System.Type" &&
                args[0].Value is Type bufferType &&
                args[1].ArgumentType.FullName == "System.Int32" &&
                args[1].Value is int bufferSize
            )
            {
                elementType = bufferType;
                length = bufferSize;
                return true;
            }

            elementType = null;
            length = 0;
            return false;
        }
    }
}

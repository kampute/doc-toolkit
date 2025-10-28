// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines a contract for accessing field metadata.
    /// </summary>
    public interface IField : ITypeMember, IWithCustomModifiers
    {
        /// <summary>
        /// Gets the type of the field.
        /// </summary>
        /// <value>
        /// The field's type metadata.
        /// </value>
        IType Type { get; }

        /// <summary>
        /// Gets a value indicating whether the field is read-only.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the field is read-only; otherwise, <see langword="false"/>.
        /// </value>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets a value indicating whether the field is a literal (constant).
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the field is a literal; otherwise, <see langword="false"/>.
        /// </value>
        /// <seealso cref="LiteralValue"/>
        bool IsLiteral { get; }

        /// <summary>
        /// Gets a value indicating whether the field is volatile.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the field is volatile; otherwise, <see langword="false"/>.
        /// </value>
        bool IsVolatile { get; }

        /// <summary>
        /// Gets a value indicating whether the field is a fixed-size buffer.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the field is a fixed-size buffer; otherwise, <see langword="false"/>.
        /// </value>
        bool IsFixedSizeBuffer { get; }

        /// <summary>
        /// Gets a value indicating whether the field is an enum value.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the field is an enum value; otherwise, <see langword="false"/>.
        /// </value>
        bool IsEnumValue => IsLiteral && DeclaringType is IEnumType;

        /// <summary>
        /// Gets the constant value of the field, if it is a literal.
        /// </summary>
        /// <value>
        /// The constant value of the field, or <see langword="null"/> if not a literal.
        /// </value>
        /// <seealso cref="IsLiteral"/>
        object? LiteralValue { get; }

        /// <summary>
        /// Attempts to get information about the fixed-size buffer, if applicable.
        /// </summary>
        /// <param name="elementType">When this method returns, contains the element type of the buffer if it is a fixed-size buffer; otherwise, <see langword="null"/>.</param>
        /// <param name="length">When this method returns, contains the length of the buffer if it is a fixed-size buffer; otherwise, 0.</param>
        /// <returns><see langword="true"/> if the field is a fixed-size buffer and the information was retrieved; otherwise, <see langword="false"/>.</returns>
        bool TryGetFixedSizeBufferInfo([NotNullWhen(true)] out IType? elementType, out int length);
    }
}

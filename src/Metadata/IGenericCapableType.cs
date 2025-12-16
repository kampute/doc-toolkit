// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for accessing metadata for types that can be generic.
    /// </summary>
    /// <remarks>
    /// This interface provides access to metadata specific to types that can be generic (classes, structs, interfaces, delegates).
    /// </remarks>
    public interface IGenericCapableType : IType
    {
        /// <summary>
        /// Gets the name of the type without any generic arity suffix.
        /// </summary>
        /// <value>
        /// The name of the type without any generic arity suffix.
        /// </value>
        string SimpleName { get; }

        /// <summary>
        /// Gets a value indicating whether the type is a generic type definition.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type is a generic type definition; otherwise, <see langword="false"/>.
        /// </value>
        bool IsGenericTypeDefinition { get; }

        /// <summary>
        /// Gets a value indicating whether the type is a constructed generic type.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type is a constructed generic type; otherwise, <see langword="false"/>.
        /// </value>
        bool IsConstructedGenericType { get; }

        /// <summary>
        /// Gets the type parameters declared by the type and its declaring types if the type is a generic type definition.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="ITypeParameter"/> instances representing the type parameters declared by the type or its declaring types.
        /// </value>
        IReadOnlyList<ITypeParameter> TypeParameters { get; }

        /// <summary>
        /// Gets the type arguments provided to the type if the type is a constructed generic type.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="IType"/> instances representing the type arguments provided to the type if the type is a constructed generic type.
        /// </value>
        IReadOnlyList<IType> TypeArguments { get; }

        /// <summary>
        /// Gets the generic type definition if the type is a constructed generic type.
        /// </summary>
        /// <value>
        /// The generic type definition, or <see langword="null"/> if the type is not a constructed generic type.
        /// </value>
        IGenericCapableType? GenericTypeDefinition { get; }

        /// <summary>
        /// Gets the offset and number of generic parameters belonging exclusively to this type, excluding any from its declaring type, if the type is generic.
        /// </summary>
        /// <value>
        /// A tuple containing the offset and number of generic parameters belonging exclusively to this type, or (0, 0) if the type is not generic.
        /// </value>
        (int Offset, int Count) OwnGenericParameterRange { get; }
    }
}

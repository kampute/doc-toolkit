// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for accessing generic type parameter metadata.
    /// </summary>
    public interface ITypeParameter : IType
    {
        /// <summary>
        /// Gets the member that declares this type parameter, which can be either a type or a method.
        /// </summary>
        /// <value>
        /// An <see cref="IMember"/> representing the member that declares this type parameter.
        /// </value>
        IMember DeclaringMember { get; }

        /// <summary>
        /// Gets the position of the type parameter in the type parameter list.
        /// </summary>
        /// <value>
        /// The zero-based position of the type parameter.
        /// </value>
        int Position { get; }

        /// <summary>
        /// Gets the variance of the type parameter.
        /// </summary>
        /// <value>
        /// A <see cref="TypeParameterVariance"/> value indicating whether the type parameter is invariant, covariant, or contravariant.
        /// </value>
        TypeParameterVariance Variance { get; }

        /// <summary>
        /// Gets the constraints applied to the type parameter.
        /// </summary>
        /// <value>
        /// A <see cref="TypeParameterConstraints"/> value representing the constraints applied to the type parameter.
        /// </value>
        TypeParameterConstraints Constraints { get; }

        /// <summary>
        /// Gets the type constraints applied to the type parameter.
        /// </summary>
        /// <value>
        /// A read-only list of types representing the type constraints applied to the type parameter.
        /// </value>
        IReadOnlyList<IType> TypeConstraints { get; }

        /// <summary>
        /// Gets a value indicating whether the type parameter has any constraints.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type parameter has any constraints; otherwise, <see langword="false"/>.
        /// </value>
        bool HasConstraints { get; }

        /// <summary>
        /// Gets a value indicating whether the current type parameter represents a type parameter of a generic type.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type parameter is from a generic type; otherwise, <see langword="false"/>.
        /// </value>
        bool IsGenericTypeParameter => !IsGenericMethodParameter;

        /// <summary>
        /// Gets a value indicating whether the current type parameter represents a type parameter of a generic method.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type parameter is from a generic method; otherwise, <see langword="false"/>.
        /// </value>
        bool IsGenericMethodParameter { get; }
    }
}

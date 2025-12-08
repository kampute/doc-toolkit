// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System;

    /// <summary>
    /// Specifies the constraints on a type parameter.
    /// </summary>
    [Flags]
    public enum TypeParameterConstraints
    {
        /// <summary>
        /// No constraints.
        /// </summary>
        None = 0,

        /// <summary>
        /// The type parameter must be a reference type.
        /// </summary>
        ReferenceType = 1,

        /// <summary>
        /// The type parameter must be a non-nullable value type.
        /// </summary>
        NotNullableValueType = 2,

        /// <summary>
        /// The type parameter must have a public default constructor.
        /// </summary>
        DefaultConstructor = 4,

        /// <summary>
        /// The type parameter can be a <c>ref struct</c> type.
        /// </summary>
        AllowByRefLike = 8,
    }
}

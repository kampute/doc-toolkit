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
        ValueType = 2,

        /// <summary>
        /// The type parameter must have a public parameterless constructor.
        /// </summary>
        DefaultConstructor = 4,

        /// <summary>
        /// The type parameter must be an unmanaged type.
        /// </summary>
        UnmanagedType = 8,

        /// <summary>
        /// The type parameter must not be <see langword="null"/>.
        /// </summary>
        NotNull = 16
    }
}

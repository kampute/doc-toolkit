// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Represents modifiers that can be applied to types.
    /// </summary>
    public enum TypeModifier
    {
        /// <summary>
        /// The type is an array.
        /// </summary>
        Array,

        /// <summary>
        /// The type is a pointer.
        /// </summary>
        Pointer,

        /// <summary>
        /// The type is passed by reference.
        /// </summary>
        ByRef,

        /// <summary>
        /// The type is a nullable value type.
        /// </summary>
        Nullable
    }
}

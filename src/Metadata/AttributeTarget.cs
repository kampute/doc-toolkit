// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Defines the target of an attribute.
    /// </summary>
    public enum AttributeTarget
    {
        /// <summary>
        /// The attribute is applied to an assembly.
        /// </summary>
        Assembly,

        /// <summary>
        /// The attribute is applied to a type.
        /// </summary>
        Type,

        /// <summary>
        /// The attribute is applied to a type's member.
        /// </summary>
        TypeMember,

        /// <summary>
        /// The attribute is applied to a generic type parameter.
        /// </summary>
        TypeParameter,

        /// <summary>
        /// The attribute is applied to a parameter.
        /// </summary>
        Parameter,

        /// <summary>
        /// The attribute is applied to a return parameter.
        /// </summary>
        ReturnParameter
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Specifies the variance of a type parameter.
    /// </summary>
    public enum TypeParameterVariance
    {
        /// <summary>
        /// The type parameter is invariant (neither covariant nor contravariant).
        /// </summary>
        Invariant,

        /// <summary>
        /// The type parameter is covariant (declared with the <c>out</c> keyword).
        /// </summary>
        Covariant,

        /// <summary>
        /// The type parameter is contravariant (declared with the <c>in</c> keyword).
        /// </summary>
        Contravariant
    }
}

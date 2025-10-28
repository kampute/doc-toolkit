// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Specifies the kind of reference for a parameter.
    /// </summary>
    public enum ParameterReferenceKind
    {
        /// <summary>
        /// The parameter is not a by reference parameter.
        /// </summary>
        None,

        /// <summary>
        /// The parameter is a by reference input parameter.
        /// </summary>
        In,
        /// <summary>
        /// The parameter is a by reference output parameter.
        /// </summary>
        Out,

        /// <summary>
        /// The parameter is a by reference input/output parameter.
        /// </summary>
        Ref
    }
}

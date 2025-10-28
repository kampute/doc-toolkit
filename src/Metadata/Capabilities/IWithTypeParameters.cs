// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for members that can be generic and have type parameters.
    /// </summary>
    public interface IWithTypeParameters
    {
        /// <summary>
        /// Gets a value indicating whether the type or member is generic and has type parameters.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type or member has type parameters; otherwise, <see langword="false"/>.
        /// </value>
        bool HasTypeParameters => TypeParameters.Count > 0;

        /// <summary>
        /// Gets all the type parameters declared by the type or member if it is generic.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="ITypeParameter"/> instances representing the type parameters declared by the type or member.
        /// </value>
        IReadOnlyList<ITypeParameter> TypeParameters { get; }
    }
}

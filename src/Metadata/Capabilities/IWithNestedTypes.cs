// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for types that can declare nested types.
    /// </summary>
    public interface IWithNestedTypes
    {
        /// <summary>
        /// Gets a value indicating whether the type has any public or protected nested types.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type has at least one nested type; otherwise, <see langword="false"/>.
        /// </value>
        bool HasNestedTypes => NestedTypes.Count > 0;

        /// <summary>
        /// Gets all the public or protected nested types declared directly by the type.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="IType"/> instances representing the nested types of the type.
        /// The nested types in the list are ordered by name.
        /// </value>
        IReadOnlyList<IType> NestedTypes { get; }
    }
}

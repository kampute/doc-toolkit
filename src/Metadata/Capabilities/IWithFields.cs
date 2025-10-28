// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for types that can declare fields.
    /// </summary>
    public interface IWithFields
    {
        /// <summary>
        /// Gets a value indicating whether the type has any public or protected fields.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type has at least one field; otherwise, <see langword="false"/>.
        /// </value>
        bool HasFields => Fields.Count > 0;

        /// <summary>
        /// Gets all the public or protected fields declared by the type.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="IField"/> instances representing the fields of the type.
        /// For most types, the fields are ordered by name; for enum types, the fields are ordered as declared in source.
        /// </value>
        IReadOnlyList<IField> Fields { get; }
    }
}

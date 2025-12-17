// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for types that can implement explicit interface properties.
    /// </summary>
    public interface IWithExplicitInterfaceProperties
    {
        /// <summary>
        /// Gets a value indicating whether the type has any explicit interface properties.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type has at least one explicit interface property; otherwise, <see langword="false"/>.
        /// </value>
        bool HasExplicitInterfaceProperties => ExplicitInterfaceProperties.Count > 0;

        /// <summary>
        /// Gets all the explicit interface properties implemented by the type.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="IProperty"/> instances representing the explicit interface properties implemented by the type.
        /// The properties in the list are ordered by name and then by the number of index parameters.
        /// </value>
        IReadOnlyList<IProperty> ExplicitInterfaceProperties { get; }
    }
}
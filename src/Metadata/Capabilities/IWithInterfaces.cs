// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for types that can implement or extend interfaces.
    /// </summary>
    public interface IWithInterfaces
    {
        /// <summary>
        /// Gets a value indicating whether the type has any interfaces.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type has at least one interface; otherwise, <see langword="false"/>.
        /// </value>
        bool HasInterfaces => Interfaces.Count > 0;

        /// <summary>
        /// Gets all the interfaces that the type extends, implements, or inherits.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="IInterfaceType"/> instances representing the interfaces.
        /// For interface types, this includes extended interfaces; for class and struct types, this includes
        /// implemented and inherited interfaces. The interfaces in the list are ordered by full name.
        /// </value>
        IReadOnlyList<IInterfaceType> Interfaces { get; }
    }
}

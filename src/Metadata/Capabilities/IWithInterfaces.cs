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
        /// A read-only list of <see cref="IInterfaceType"/> instances representing the interfaces that apply to the type.
        /// For interface types, this contains the interfaces they extend; for classes and structs, this contains the
        /// interfaces they implement or inherit. The list is ordered by each interface's full name.
        /// </value>
        IReadOnlyList<IInterfaceType> Interfaces { get; }
    }
}

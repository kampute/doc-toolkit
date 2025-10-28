// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for types that can declare events.
    /// </summary>
    public interface IWithEvents
    {
        /// <summary>
        /// Gets a value indicating whether the type has any public or protected events.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type has at least one event; otherwise, <see langword="false"/>.
        /// </value>
        bool HasEvents => Events.Count > 0;

        /// <summary>
        /// Gets all the public or protected events declared, overridden, or implemented by the type.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="IEvent"/> instances representing the events of the type.
        /// The events in the list are ordered by name.
        /// </value>
        IReadOnlyList<IEvent> Events { get; }
    }
}

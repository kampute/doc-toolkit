// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for types that can implement explicit interface events.
    /// </summary>
    public interface IWithExplicitInterfaceEvents
    {
        /// <summary>
        /// Gets a value indicating whether the type has any explicit interface events.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type has at least one explicit interface event; otherwise, <see langword="false"/>.
        /// </value>
        bool HasExplicitInterfaceEvents => ExplicitInterfaceEvents.Count > 0;

        /// <summary>
        /// Gets all the explicit interface events implemented by the type.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="IEvent"/> instances representing the explicit interface events implemented by the type.
        /// The events in the list are ordered by name.
        /// </value>
        IReadOnlyList<IEvent> ExplicitInterfaceEvents { get; }
    }
}
// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Defines a contract for accessing event metadata.
    /// </summary>
    public interface IEvent : IVirtualTypeMember
    {
        /// <summary>
        /// Gets the type of the event handler.
        /// </summary>
        /// <value>
        /// The event handler type metadata.
        /// </value>
        IType Type { get; }

        /// <summary>
        /// Gets the add method of the event.
        /// </summary>
        /// <value>
        /// The add method metadata.
        /// </value>
        IMethod AddMethod { get; }

        /// <summary>
        /// Gets the remove method of the event.
        /// </summary>
        /// <value>
        /// The remove method metadata.
        /// </value>
        IMethod RemoveMethod { get; }

        /// <summary>
        /// Gets the raise method of the event.
        /// </summary>
        /// <value>The raise method metadata, or <see langword="null"/> if not explicitly defined.</value>
        IMethod? RaiseMethod { get; }

        /// <summary>
        /// Gets the base event that this event overrides, if any.
        /// </summary>
        /// <value>
        /// The base event that this event overrides, or <see langword="null"/> if none.
        /// </value>
        IEvent? OverriddenEvent { get; }

        /// <summary>
        /// Gets the interface event that this event implements, if any.
        /// </summary>
        /// <value>
        /// The interface event that this event implements, or <see langword="null"/> if none.
        /// </value>
        IEvent? ImplementedEvent { get; }
    }
}

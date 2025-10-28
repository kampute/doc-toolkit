// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines a contract for types that can implement explicit interface members.
    /// </summary>
    public interface IWithExplicitInterfaceMembers
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

        /// <summary>
        /// Gets a value indicating whether the type has any explicit interface methods.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type has at least one explicit interface method; otherwise, <see langword="false"/>.
        /// </value>
        bool HasExplicitInterfaceMethods => ExplicitInterfaceMethods.Count > 0;

        /// <summary>
        /// Gets all the explicit interface methods implemented by the type.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="IMethod"/> instances representing the explicit interface methods implemented by the type.
        /// The methods in the list are ordered by name and then by the number of type parameters.
        /// </value>
        IReadOnlyList<IMethod> ExplicitInterfaceMethods { get; }

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

        /// <summary>
        /// Gets a value indicating whether the type has any explicit interface members.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type has at least one explicit interface member; otherwise, <see langword="false"/>.
        /// </value>
        bool HasExplicitInterfaceMembers => HasExplicitInterfaceProperties || HasExplicitInterfaceMethods || HasExplicitInterfaceEvents;

        /// <summary>
        /// Gets all the explicit interface members implemented by the type.
        /// </summary>
        /// <value>
        /// An enumerable of <see cref="IVirtualTypeMember"/> instances representing all explicit interface members implemented by the type.
        /// </value>
        IEnumerable<IVirtualTypeMember> ExplicitInterfaceMembers => Enumerable.Empty<IVirtualTypeMember>()
            .Concat(ExplicitInterfaceProperties).Concat(ExplicitInterfaceMethods).Concat(ExplicitInterfaceEvents);
    }
}

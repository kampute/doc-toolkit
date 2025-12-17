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
    public interface IWithExplicitInterfaceMembers : IWithExplicitInterfaceMethods, IWithExplicitInterfaceProperties, IWithExplicitInterfaceEvents, IWithExplicitInterfaceOperators
    {
        /// <summary>
        /// Gets a value indicating whether the type has any explicit interface members.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type has at least one explicit interface member; otherwise, <see langword="false"/>.
        /// </value>
        bool HasExplicitInterfaceMembers => HasExplicitInterfaceMethods || HasExplicitInterfaceProperties || HasExplicitInterfaceEvents || HasExplicitInterfaceOperators;

        /// <summary>
        /// Gets all the explicit interface members implemented by the type.
        /// </summary>
        /// <value>
        /// An enumerable of <see cref="IVirtualTypeMember"/> instances representing all explicit interface members implemented by the type.
        /// </value>
        IEnumerable<IVirtualTypeMember> ExplicitInterfaceMembers => Enumerable.Empty<IVirtualTypeMember>()
            .Concat(ExplicitInterfaceMethods).Concat(ExplicitInterfaceProperties).Concat(ExplicitInterfaceEvents).Concat(ExplicitInterfaceOperators);
    }
}

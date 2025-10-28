// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines a contract for type members that can have overloads, such as methods or constructors.
    /// </summary>
    public interface IWithOverloads
    {
        /// <summary>
        /// Gets a value indicating whether the member has any overloads.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the member has at least one overload; otherwise, <see langword="false"/>.
        /// </value>
        bool HasOverloads => Overloads.Any();

        /// <summary>
        /// Gets all the overloads of the member, excluding the member itself.
        /// </summary>
        /// <value>
        /// An enumerable of <see cref="IMember"/> instances representing all overloads with the same name but different signatures.
        /// The member itself is excluded from this collection. The overloads are ordered by the number of parameters.
        /// </value>
        IEnumerable<IMember> Overloads { get; }
    }
}

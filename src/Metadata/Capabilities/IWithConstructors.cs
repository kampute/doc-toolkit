// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for types that can declare constructors.
    /// </summary>
    public interface IWithConstructors
    {
        /// <summary>
        /// Gets a value indicating whether the type has any public or protected constructors.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type has at least one constructor; otherwise, <see langword="false"/>.
        /// </value>
        bool HasConstructors => Constructors.Count > 0;

        /// <summary>
        /// Gets all the public or protected constructors declared by the type.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="IConstructor"/> instances representing the constructors of the type.
        /// The constructors in the list are ordered by their number of parameters, from the least to the most.
        /// </value>
        IReadOnlyList<IConstructor> Constructors { get; }
    }
}

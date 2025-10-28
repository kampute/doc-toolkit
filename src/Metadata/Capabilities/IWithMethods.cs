// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for types that can declare methods.
    /// </summary>
    public interface IWithMethods
    {
        /// <summary>
        /// Gets a value indicating whether the type has any public or protected methods.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type has at least one method; otherwise, <see langword="false"/>.
        /// </value>
        bool HasMethods => Methods.Count > 0;

        /// <summary>
        /// Gets all the public or protected methods declared, overridden, or implemented by the type.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="IMethod"/> instances representing the methods of the type.
        /// The methods in the list are ordered by name and then by the number of type parameters.
        /// </value>
        IReadOnlyList<IMethod> Methods { get; }
    }
}

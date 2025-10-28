// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for types that can declare operator methods.
    /// </summary>
    public interface IWithOperators
    {
        /// <summary>
        /// Gets a value indicating whether the type has any operator methods.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type has at least one operator method; otherwise, <see langword="false"/>.
        /// </value>
        bool HasOperators => Operators.Count > 0;

        /// <summary>
        /// Gets all the operator methods declared by the type.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="IOperator"/> instances representing the operator methods declared by the type.
        /// The operators in the list are ordered by name and then by the number of type parameters.
        /// </value>
        IReadOnlyList<IOperator> Operators { get; }
    }
}

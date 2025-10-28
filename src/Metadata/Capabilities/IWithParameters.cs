// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for members that can have parameters.
    /// </summary>
    public interface IWithParameters
    {
        /// <summary>
        /// Gets a value indicating whether the member has any parameters.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the member has at least one parameter; otherwise, <see langword="false"/>.
        /// </value>
        bool HasParameters => Parameters.Count > 0;

        /// <summary>
        /// Gets all the parameters declared by the member.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="IParameter"/> instances representing the parameters declared by the member.
        /// </value>
        IReadOnlyList<IParameter> Parameters { get; }
    }
}

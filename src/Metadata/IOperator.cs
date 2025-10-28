// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Defines a contract for accessing operator method metadata.
    /// </summary>
    public interface IOperator : IMethodBase
    {
        /// <summary>
        /// Gets a value indicating whether the method is a conversion operator.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the method is a conversion operator; otherwise, <see langword="false"/>.
        /// </value>
        bool IsConversionOperator { get; }
    }
}

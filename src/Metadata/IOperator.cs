// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Defines a contract for accessing operator operator metadata.
    /// </summary>
    public interface IOperator : IMethodBase
    {
        /// <summary>
        /// Gets the name of the operator method as defined by the reflection API.
        /// </summary>
        /// <value>
        /// The name of the operator method as defined by the reflection API.
        /// </value>
        string MethodName { get; }

        /// <summary>
        /// Gets a value indicating whether the operator is a conversion operator.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the operator is a conversion operator; otherwise, <see langword="false"/>.
        /// </value>
        bool IsConversionOperator { get; }

        /// <summary>
        /// Gets the base operator that this operator overrides, if any.
        /// </summary>
        /// <value>
        /// The base operator that this operator overrides, or <see langword="null"/> if none.
        /// </value>
        IOperator? OverriddenOperator { get; }

        /// <summary>
        /// Gets the interface operator that this operator implements, if any.
        /// </summary>
        /// <value>
        /// The interface operator that this operator implements, or <see langword="null"/> if none.
        /// </value>
        IOperator? ImplementedOperator { get; }
    }
}

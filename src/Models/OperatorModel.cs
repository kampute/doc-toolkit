// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Models
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using System;

    /// <summary>
    /// Represents a documentation model for an operator within a type.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public class OperatorModel : TypeMemberModel<IOperator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorModel"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the operator.</param>
        /// <param name="op">The metadata of the operator represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="op"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="op"/> is not an operator of <paramref name="declaringType"/>.</exception>
        public OperatorModel(TypeModel declaringType, IOperator op)
            : base(declaringType, op)
        {
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is always <see cref="DocumentationModelType.Operator"/> for this model.
        /// </value>
        public override DocumentationModelType ModelType => DocumentationModelType.Operator;
    }
}

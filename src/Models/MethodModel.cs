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
    /// Represents a documentation model for a method within a type.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public class MethodModel : TypeMemberModel<IMethod>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodModel"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the method.</param>
        /// <param name="method">The metadata of the method represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="method"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="method"/> is not a method of <paramref name="declaringType"/>.</exception>
        public MethodModel(TypeModel declaringType, IMethod method)
            : base(declaringType, method)
        {
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is always <see cref="DocumentationModelType.Method"/> for this model.
        /// </value>
        public override DocumentationModelType ModelType => DocumentationModelType.Method;
    }
}

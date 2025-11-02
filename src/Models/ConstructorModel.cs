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
    /// Represents a documentation model for a constructor within a type.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public class ConstructorModel : TypeMemberModel<IConstructor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorModel"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the constructor.</param>
        /// <param name="constructor">The constructor information.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="constructor"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="constructor"/> is not a constructor of <paramref name="declaringType"/>.</exception>
        public ConstructorModel(TypeModel declaringType, IConstructor constructor)
            : base(declaringType, constructor)
        {
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is always <see cref="DocumentationModelType.Constructor"/> for this model.
        /// </value>
        public override DocumentationModelType ModelType => DocumentationModelType.Constructor;

        /// <inheritdoc/>
        public override string ToString() => DeclaringType.Name;
    }
}

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
    /// Represents a documentation model for a property within a type.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public class PropertyModel : TypeMemberModel<IProperty>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyModel"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the property.</param>
        /// <param name="property">The metadata of the property represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="property"/> is not a property of <paramref name="declaringType"/>.</exception>
        public PropertyModel(TypeModel declaringType, IProperty property)
            : base(declaringType, property)
        {
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is always <see cref="DocumentationModelType.Property"/> for this model.
        /// </value>
        public override DocumentationModelType ModelType => DocumentationModelType.Property;
    }
}

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
    /// Represents a documentation model for a field within a type.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public class FieldModel : TypeMemberModel<IField>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldModel"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the field.</param>
        /// <param name="field">The metadata of the field represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="field"/> is <see langword="null"/>.</exception>
        public FieldModel(TypeModel declaringType, IField field)
            : base(declaringType, field)
        {
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is always <see cref="DocumentationModelType.Field"/> for this model.
        /// </value>
        public override DocumentationModelType ModelType => DocumentationModelType.Field;
    }
}

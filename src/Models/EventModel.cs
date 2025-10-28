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
    /// Represents a documentation model for an event within a type.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public class EventModel : TypeMemberModel<IEvent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventModel"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the event.</param>
        /// <param name="evt">The metadata of the event represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="evt"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="evt"/> is not an event of <paramref name="declaringType"/>.</exception>
        public EventModel(TypeModel declaringType, IEvent evt)
            : base(declaringType, evt)
        {
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is always <see cref="DocumentationModelType.Event"/> for this model.
        /// </value>
        public override DocumentationModelType ModelType => DocumentationModelType.Event;
    }
}

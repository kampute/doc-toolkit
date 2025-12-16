// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Models
{
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a documentation model for an extension block.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public class ExtensionBlockModel : TypeMemberModel<IExtensionBlock>
    {
        private readonly Lazy<List<PropertyModel>> properties;
        private readonly Lazy<List<MethodModel>> methods;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventModel"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the extension block.</param>
        /// <param name="extensionBlock">The metadata of the extension block represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="extensionBlock"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="extensionBlock"/> is not an extension block of <paramref name="declaringType"/>.</exception>
        public ExtensionBlockModel(TypeModel declaringType, IExtensionBlock extensionBlock)
            : base(declaringType, extensionBlock)
        {
            properties = new(() => DeclaringType is CompositeTypeModel container
                ? [.. container.Properties.Where(p => ReferenceEquals(p.Metadata.ExtensionBlock, Metadata))]
                : []);

            methods = new(() => DeclaringType is CompositeTypeModel container
                ? [.. container.Methods.Where(m => ReferenceEquals(m.Metadata.ExtensionBlock, Metadata))]
                : []);
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is always <see cref="DocumentationModelType.ExtensionBlock"/> for this model.
        /// </value>
        public override DocumentationModelType ModelType => DocumentationModelType.ExtensionBlock;

        /// <summary>
        /// Gets the properties declared by the type and visible within its scope.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="PropertyModel"/> representing the properties declared by the type.
        /// The properties in the collection  are ordered by their names and number of index parameters.
        /// </value>
        public IReadOnlyList<PropertyModel> Properties => properties.Value;

        /// <summary>
        /// Gets the methods declared by the type and visible within its scope.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="MethodModel"/> representing the methods declared by the type.
        /// The methods in the collection are ordered by their names and number of parameters.
        /// </value>
        public IReadOnlyList<MethodModel> Methods => methods.Value;
    }
}

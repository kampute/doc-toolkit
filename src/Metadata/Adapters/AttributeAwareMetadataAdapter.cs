// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides the base implementation for metadata adapters for elements that can have custom attributes.
    /// </summary>
    /// <typeparam name="T">The type of the reflection information.</typeparam>
    /// <threadsafety static="true" instance="true"/>
    public abstract class AttributeAwareMetadataAdapter<T> : MetadataAdapter<T>, IWithCustomAttributes
        where T : class, ICustomAttributeProvider
    {
        private readonly Lazy<IReadOnlyList<ICustomAttribute>> customAttributes;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeAwareMetadataAdapter{T}"/> class.
        /// </summary>
        /// <param name="reflectionInfo">The native instance to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reflectionInfo"/> is <see langword="null"/>.</exception>
        protected AttributeAwareMetadataAdapter(T reflectionInfo)
            : base(reflectionInfo)
        {
            customAttributes = new(() => [.. GetCustomAttributes().Select(CreateAttributeMetadata)]);
        }

        /// <inheritdoc/>
        public IReadOnlyList<ICustomAttribute> CustomAttributes => customAttributes.Value;

        /// <inheritdoc/>
        public virtual bool HasCustomAttribute(string attributeFullName) => GetCustomAttributes().Any(attr => attr.AttributeType.FullName == attributeFullName);

        /// <summary>
        /// Retrieves the underlying custom attributes of the member.
        /// </summary>
        /// <returns>An enumerable of custom attribute data instances associated with the member.</returns>
        protected abstract IEnumerable<CustomAttributeData> GetCustomAttributes();

        /// <summary>
        /// Creates a custom attribute metadata instance for the given attribute data.
        /// </summary>
        /// <param name="attribute">The custom attribute data.</param>
        /// <returns>An instance of <see cref="ICustomAttribute"/> representing the custom attribute.</returns>
        protected abstract ICustomAttribute CreateAttributeMetadata(CustomAttributeData attribute);
    }
}

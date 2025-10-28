// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using System;

    /// <summary>
    /// Provides the base implementation for metadata adapters.
    /// </summary>
    /// <typeparam name="T">The type of the reflection information.</typeparam>
    public abstract class MetadataAdapter<T> : IMetadataAdapter<T>
        where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataAdapter{T}"/> class.
        /// </summary>
        /// <param name="reflection">The reflection information instance to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reflection"/> is <see langword="null"/>.</exception>
        /// <threadsafety static="true" instance="true"/>
        protected MetadataAdapter(T reflection)
        {
            Reflection = reflection ?? throw new ArgumentNullException(nameof(reflection));
        }

        /// <summary>
        /// Gets the underlying reflection information.
        /// </summary>
        /// <value>
        /// The reflection information wrapped by this adapter.
        /// </value>
        protected T Reflection { get; }

        /// <summary>
        /// Gets the name of the metadata element.
        /// </summary>
        /// <value>
        /// The name of the metadata element.
        /// </value>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public virtual bool Represents(T reflection) => ReferenceEquals(Reflection, reflection);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj) => obj is MetadataAdapter<T> other && Represents(other.Reflection);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>The hash code for the current object.</returns>
        public override int GetHashCode() => Reflection.GetHashCode();

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => Name;
    }
}

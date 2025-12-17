// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using System;

    /// <summary>
    /// An adapter that wraps a reflection <see cref="Type"/> representing a struct type and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="Type"/> and the metadata
    /// representation defined by the <see cref="IStructType"/> interface. It provides access to struct-level
    /// information regardless of whether the assembly containing the struct type was loaded via Common
    /// Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class StructTypeAdapter : CompositeTypeAdapter, IStructType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StructTypeAdapter"/> class.
        /// </summary>
        /// <param name="declaringEntity">The assembly or type that declares the struct type.</param>
        /// <param name="structType">The reflection information of the struct type to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringEntity"/> or <paramref name="structType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is neither an <see cref="IAssembly"/> nor an <see cref="IType"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="structType"/> is not declared by the <paramref name="declaringEntity"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="structType"/> is a nested type but <paramref name="declaringEntity"/> is an assembly, or when <paramref name="structType"/> is a top-level type but <paramref name="declaringEntity"/> is a type.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="structType"/> is not a struct.</exception>
        public StructTypeAdapter(object declaringEntity, Type structType)
            : base(declaringEntity, structType)
        {
            if (!structType.IsValueType || structType.IsEnum)
                throw new ArgumentException("Type must be a struct.", nameof(structType));
        }

        /// <inheritdoc/>
        public virtual bool IsReadOnly => HasCustomAttribute(AttributeNames.IsReadOnly);

        /// <inheritdoc/>
        public virtual bool IsRefLike => Reflection.IsByRefLike;
    }
}

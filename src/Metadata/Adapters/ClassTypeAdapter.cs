// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using System;

    /// <summary>
    /// An adapter that wraps a reflection <see cref="Type"/> representing a class type and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="Type"/> and the metadata
    /// representation defined by the <see cref="IClassType"/> interface. It provides access to class-level
    /// information regardless of whether the assembly containing the class was loaded via Common Language
    /// Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class ClassTypeAdapter : CompositeTypeAdapter, IClassType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassTypeAdapter"/> class.
        /// </summary>
        /// <param name="declaringEntity">The assembly or type that declares the class type.</param>
        /// <param name="classType">The reflection information of the class type to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringEntity"/> or <paramref name="classType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is neither an <see cref="IAssembly"/> nor an <see cref="IType"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="classType"/> is not declared by the <paramref name="declaringEntity"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="classType"/> is a nested type but <paramref name="declaringEntity"/> is an assembly, or when <paramref name="classType"/> is a top-level type but <paramref name="declaringEntity"/> is a type.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="classType"/> is not a class type.</exception>
        public ClassTypeAdapter(object declaringEntity, Type classType)
            : base(declaringEntity, classType)
        {
            if (!classType.IsClass)
                throw new ArgumentException("Type must be a class.", nameof(classType));
        }

        /// <inheritdoc/>
        public override bool IsStatic => Reflection.IsSealed && Reflection.IsAbstract;

        /// <inheritdoc/>
        public virtual bool IsAbstract => Reflection.IsAbstract && !Reflection.IsSealed;

        /// <inheritdoc/>
        public virtual bool IsSealed => Reflection.IsSealed && !Reflection.IsAbstract;
    }
}

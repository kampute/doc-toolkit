// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using System;

    /// <summary>
    /// An adapter that wraps a reflection <see cref="Type"/> representing an interface type and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="Type"/> and the metadata
    /// representation defined by the <see cref="IInterfaceType"/> interface. It provides access to interface-level
    /// information regardless of whether the assembly containing the interface's type was loaded via Common
    /// Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class InterfaceTypeAdapter : CompositeTypeAdapter, IInterfaceType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceTypeAdapter"/> class.
        /// </summary>
        /// <param name="declaringEntity">The assembly or type that declares the interface type.</param>
        /// <param name="interfaceType">The reflection information of the interface type to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringEntity"/> or <paramref name="interfaceType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is neither an <see cref="IAssembly"/> nor an <see cref="IType"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="interfaceType"/> is not declared by the <paramref name="declaringEntity"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="interfaceType"/> is a nested type but <paramref name="declaringEntity"/> is an assembly, or when <paramref name="interfaceType"/> is a top-level type but <paramref name="declaringEntity"/> is a type.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="interfaceType"/> is not an interface.</exception>
        public InterfaceTypeAdapter(object declaringEntity, Type interfaceType)
            : base(declaringEntity, interfaceType)
        {
            if (!interfaceType.IsInterface)
                throw new ArgumentException("Type must be an interface.", nameof(interfaceType));
        }

        /// <inheritdoc/>
        public override bool IsAssignableFrom(IType source)
            => base.IsAssignableFrom(source) || (source is IInterfaceCapableType iSource && iSource.Implements(this));
    }
}

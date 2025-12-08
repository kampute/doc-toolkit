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
    /// Represents a documentation model for a .NET interface.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public class InterfaceModel : CompositeTypeModel<IInterfaceType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceModel"/> class.
        /// </summary>
        /// <param name="declaringEntity">The object that declares the type, which is either an <see cref="AssemblyModel"/> for top-level types or a <see cref="TypeModel"/> for nested types.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="declaringEntity"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is not an instance of <see cref="AssemblyModel"/> or <see cref="TypeModel"/>.</exception>
        protected InterfaceModel(object declaringEntity, IInterfaceType type)
            : base(declaringEntity, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceModel"/> class as a top-level type.
        /// </summary>
        /// <param name="assembly">The assembly that contains the type.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not a top-level type of <paramref name="assembly"/>.</exception>
        public InterfaceModel(AssemblyModel assembly, IInterfaceType type)
            : this((object)assembly ?? throw new ArgumentNullException(nameof(assembly)), type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceModel"/> class as a nested type.
        /// </summary>
        /// <param name="declaringType">The type that declares the type.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not directly nested within <paramref name="declaringType"/>.</exception>
        public InterfaceModel(TypeModel declaringType, IInterfaceType type)
            : this((object)declaringType ?? throw new ArgumentNullException(nameof(declaringType)), type)
        {
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is always <see cref="DocumentationModelType.Interface"/> for this model.
        /// </value>
        public override DocumentationModelType ModelType => DocumentationModelType.Interface;
    }
}

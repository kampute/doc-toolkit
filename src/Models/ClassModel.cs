﻿// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Models
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using System;

    /// <summary>
    /// Represents a documentation model for a .NET class.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public class ClassModel : CompositeTypeModel<IClassType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassModel"/> class.
        /// </summary>
        /// <param name="declaryingEntity">The object that declares the type, which is either an <see cref="AssemblyModel"/> for top-level types or a <see cref="TypeModel"/> for nested types.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="declaryingEntity"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaryingEntity"/> is not an instance of <see cref="AssemblyModel"/> or <see cref="TypeModel"/>.</exception>
        protected ClassModel(object declaryingEntity, IClassType type)
            : base(declaryingEntity, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassModel"/> class as a top-level type.
        /// </summary>
        /// <param name="assembly">The assembly that contains the type.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not a top-level type of <paramref name="assembly"/>.</exception>
        public ClassModel(AssemblyModel assembly, IClassType type)
            : this((object)assembly ?? throw new ArgumentNullException(nameof(assembly)), type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassModel"/> class as a nested type.
        /// </summary>
        /// <param name="declaringType">The type that declares the type.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not directedly nested within <paramref name="declaringType"/>.</exception>
        public ClassModel(TypeModel declaringType, IClassType type)
            : this((object)declaringType ?? throw new ArgumentNullException(nameof(declaringType)), type)
        {
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is always <see cref="DocumentationModelType.Class"/> for this model.
        /// </value>
        public override DocumentationModelType ModelType => DocumentationModelType.Class;
    }
}

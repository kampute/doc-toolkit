// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Collections
{
    using Kampute.DocToolkit.Models;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a read-only collection of type models in a documentation context.
    /// </summary>
    public interface IReadOnlyTypeCollection : IReadOnlyCollection<TypeModel>
    {
        /// <summary>
        /// Gets all class type models in the collection.
        /// </summary>
        /// <value>
        /// A read-only collection <see cref="ClassModel"/> containing all class type models in the collection.
        /// </value>
        IReadOnlyCollection<ClassModel> Classes { get; }

        /// <summary>
        /// Gets all struct type models in the collection.
        /// </summary>
        /// <value>
        /// A read-only collection <see cref="StructModel"/> containing all struct type models in the collection.
        /// </value>
        IReadOnlyCollection<StructModel> Structs { get; }

        /// <summary>
        /// Gets all interface type models in the collection.
        /// </summary>
        /// <value>
        /// A read-only collection <see cref="InterfaceModel"/> containing all interface type models in the collection.
        /// </value>
        IReadOnlyCollection<InterfaceModel> Interfaces { get; }

        /// <summary>
        /// Gets all enum type models in the collection.
        /// </summary>
        /// <value>
        /// A read-only collection <see cref="EnumModel"/> containing all enum type models in the collection.
        /// </value>
        IReadOnlyCollection<EnumModel> Enums { get; }

        /// <summary>
        /// Gets all delegate type models in the collection.
        /// </summary>
        /// <value>
        /// A read-only collection <see cref="DelegateModel"/> containing all delegate type models in the collection.
        /// </value>
        IReadOnlyCollection<DelegateModel> Delegates { get; }
    }
}

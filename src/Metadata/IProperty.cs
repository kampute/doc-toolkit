// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for accessing property metadata.
    /// </summary>
    public interface IProperty : IVirtualTypeMember, IWithParameters, IWithOverloads, IWithCustomModifiers
    {
        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        /// <value>
        /// The property's type metadata.
        /// </value>
        IType Type { get; }

        /// <summary>
        /// Gets a value indicating whether the property is an indexer.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the property is an indexer; otherwise, <see langword="false"/>.
        /// </value>
        bool IsIndexer { get; }

        /// <summary>
        /// Gets a value indicating whether the property is read-only.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the property is read-only; otherwise, <see langword="false"/>.
        /// </value>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets a value indicating whether the property is init-only.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the property is init-only; otherwise, <see langword="false"/>.
        /// </value>
        bool IsInitOnly { get; }

        /// <summary>
        /// Gets a value indicating whether the property is required.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the property is required; otherwise, <see langword="false"/>.
        /// </value>
        bool IsRequired { get; }

        /// <summary>
        /// Gets a value indicating whether the property can be read.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the property can be read; otherwise, <see langword="false"/>.
        /// </value>
        bool CanRead { get; }

        /// <summary>
        /// Gets a value indicating whether the property can be written.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the property can be written; otherwise, <see langword="false"/>.
        /// </value>
        bool CanWrite { get; }

        /// <summary>
        /// Gets the get method of the property.
        /// </summary>
        /// <value>
        /// The get method metadata, or <see langword="null"/> if the property is write-only.
        /// </value>
        IMethod? GetMethod { get; }

        /// <summary>
        /// Gets the set method of the property.
        /// </summary>
        /// <value>
        /// The set method metadata, or <see langword="null"/> if the property is read-only.
        /// </value>
        IMethod? SetMethod { get; }

        /// <summary>
        /// Gets the base property that this property overrides, if any.
        /// </summary>
        /// <value>
        /// The base property that this property overrides, or <see langword="null"/> if none.
        /// </value>
        IProperty? OverriddenProperty { get; }

        /// <summary>
        /// Gets the interface property that this property implements, if any.
        /// </summary>
        /// <value>
        /// The interface property that this property implements, or <see langword="null"/> if none.
        /// </value>
        IProperty? ImplementedProperty { get; }

        /// <summary>
        /// Gets all accessor methods (get and set) of the property.
        /// </summary>
        /// <returns>An enumerable of accessor methods.</returns>
        IEnumerable<IMethod> GetAccessors();
    }
}

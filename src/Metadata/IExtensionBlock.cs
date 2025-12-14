// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for accessing metadata about extension blocks.
    /// </summary>
    /// <remarks>
    /// An extension block represents a logical grouping of extension members that share the same receiver type (the type being extended).
    /// </remarks>
    public interface IExtensionBlock : IWithProperties, IWithMethods
    {
        /// <summary>
        /// Gets the assembly that defines this extension block.
        /// </summary>
        /// <value>
        /// The assembly defining this extension block.
        /// </value>
        IAssembly Assembly { get; }

        /// <summary>
        /// Gets the type that declares this extension block.
        /// </summary>
        /// <value>
        /// The declaring type of this extension block.
        /// </value>
        IClassType DeclaringType { get; }

        /// <summary>
        /// Gets the receiver parameter information for this extension block.
        /// </summary>
        /// <value>
        /// The receiver parameter information of this extension block.
        /// </value>
        IParameter Receiver { get; }

        /// <summary>
        /// Gets a value indicating whether this extension block is generic.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this extension block is generic; otherwise, <see langword="false"/>.
        /// </value>
        bool IsGenericBlock { get; }

        /// <summary>
        /// Gets the generic type parameters of this extension block if it is generic.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="IType"/> representing the generic type parameters of this extension block.
        /// </value>
        IReadOnlyList<ITypeParameter> TypeParameters { get; }

        /// <summary>
        /// Gets the code reference identifier for this extension block.
        /// </summary>
        /// <value>
        /// The code reference identifier of this extension block.
        /// </value>
        string CodeReference { get; }

        /// <summary>
        /// Determines whether this extension block extends the specified type.
        /// </summary>
        /// <param name="type">The type to check against.</param>
        /// <returns><see langword="true"/> if this extension block extends the specified type; otherwise, <see langword="false"/>.</returns>
        bool Extends(IType type);
    }
}

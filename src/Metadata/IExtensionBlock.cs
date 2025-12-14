// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for accessing metadata about extension blocks.
    /// </summary>
    /// <remarks>
    /// An extension block represents a logical grouping of extension methods that share the same receiver parameter type.
    /// </remarks>
    public interface IExtensionBlock
    {
        /// <summary>
        /// Gets the assembly that defines this extension block.
        /// </summary>
        /// <value>
        /// The assembly defining this extension block.
        /// </value>
        IAssembly Assembly { get; }

        /// <summary>
        /// Gets the receiver parameter information for this extension block.
        /// </summary>
        /// <value>
        /// The receiver parameter information of this extension block.
        /// </value>
        IParameter Receiver { get; }

        /// <summary>
        /// Gets a value indicating whether this extension block is synthetic.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this extension block is synthetic; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// A synthetic extension block is one that represents an aggregation of classic (non-block) extension methods
        /// with the same receiver parameter that have been logically grouped together for organizational purposes.
        /// </remarks>
        bool IsSynthetic { get; }

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
        /// Gets the code reference identifier for this extension block, if not synthetic.
        /// </summary>
        /// <value>
        /// The code reference identifier of this extension block; otherwise, <see langword="null"/> if synthetic.
        /// </value>
        string? CodeReference { get; }

        /// <summary>
        /// Determines whether this extension block extends the specified type.
        /// </summary>
        /// <param name="type">The type to check against.</param>
        /// <returns><see langword="true"/> if this extension block extends the specified type; otherwise, <see langword="false"/>.</returns>
        bool Extends(IType type);
    }
}

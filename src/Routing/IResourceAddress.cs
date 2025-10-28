// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    /// <summary>
    /// Defines a contract to represent a resource address.
    /// </summary>
    /// <remarks>
    /// Resource addresses are a key abstraction in the documentation system that provide a unified way to reference documentation
    /// resources regardless of their physical location.
    /// </remarks>
    public interface IResourceAddress
    {
        /// <summary>
        /// Gets the address of the resource as a URL.
        /// </summary>
        /// <value>
        /// The URL representation of the resource address.
        /// </value>
        string RelativeUrl { get; }

        /// <summary>
        /// Gets the address of the resource as a file path if applicable.
        /// </summary>
        /// <value>
        /// The file path representation of the resource address if applicable; otherwise, <see langword="null"/>.
        /// </value>
        string? RelativeFilePath { get; }
    }
}

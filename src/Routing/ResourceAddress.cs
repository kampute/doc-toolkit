// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using System;

    /// <summary>
    /// Represents a resource address with support for both URL and file path representations.
    /// </summary>
    /// <remarks>
    /// This class provides a unified implementation for addressing resources where the URL and file path may differ.
    /// Both representations are relative to the documentation root.
    /// <note type="caution" title="Caution">
    /// This class assumes that the path does not contain any character that has a reserved meaning in a URI or file path.
    /// </note>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public readonly struct ResourceAddress : IResourceAddress
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceAddress"/> struct with the specified URL and file path.
        /// </summary>
        /// <param name="urlString">The URL string representing the resource address.</param>
        /// <param name="filePath">The file path representing the resource physical location, if applicable.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="urlString"/> is <see langword="null"/> or empty.</exception>
        public ResourceAddress(string urlString, string? filePath = null)
        {
            if (string.IsNullOrEmpty(urlString))
                throw new ArgumentException($"'{nameof(urlString)}' cannot be null or empty.", nameof(urlString));

            RelativeUrl = urlString;
            RelativeFilePath = filePath;
        }

        /// <summary>
        /// Gets the address of the resource as a URL string relative to the documentation root.
        /// </summary>
        /// <value>
        /// The URL representation of the resource address, including any URL fragment or query string.
        /// </value>
        public readonly string RelativeUrl { get; }

        /// <summary>
        /// Gets the address of the resource as a file path relative to the documentation root, if applicable.
        /// </summary>
        /// <value>
        /// The file path representation of the resource address, if applicable; otherwise, <see langword="null"/>.
        /// </value>
        public readonly string? RelativeFilePath { get; }
    }
}

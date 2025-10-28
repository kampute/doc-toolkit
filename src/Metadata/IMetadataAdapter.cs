// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Defines a contract for metadata adapters that wrap reflection elements.
    /// </summary>
    /// <typeparam name="T">The type of the reflection element.</typeparam>
    public interface IMetadataAdapter<T>
        where T : class
    {
        /// <summary>
        /// Determines whether this metadata adapter represents the specified reflection element.
        /// </summary>
        /// <param name="reflection">The reflection element to check.</param>
        /// <returns><see langword="true"/> if this adapter represents the specified reflection element; otherwise, <see langword="false"/>.</returns>
        bool Represents(T reflection);
    }
}

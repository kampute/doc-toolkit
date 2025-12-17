// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Defines a contract for accessing struct-specific metadata.
    /// </summary>
    public interface IStructType : ICompositeType
    {
        /// <summary>
        /// Gets a value indicating whether the struct is readonly.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the struct is readonly; otherwise, <see langword="false"/>.
        /// </value>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets a value indicating whether the struct a by-ref-like struct.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the struct is a by-ref-like struct; otherwise, <see langword="false"/>.
        /// </value>
        bool IsRefLike { get; }
    }
}

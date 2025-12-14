// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Reflection
{
    /// <summary>
    /// Provides an interface for reflection member info objects that represent extension members.
    /// </summary>
    public interface IExtensionMemberInfo
    {
        /// <summary>
        /// Gets the extension block information associated with the extension member.
        /// </summary>
        /// <value>
        /// An instance of <see cref="ExtensionBlockInfo"/> representing the extension block information
        /// associated with the extension member.
        /// </value>
        ExtensionBlockInfo ExtensionBlock { get; }
    }
}

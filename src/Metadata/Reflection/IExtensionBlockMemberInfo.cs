// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Reflection
{
    /// <summary>
    /// Provides an interface for reflection member information that represent extension members defined in an extension block.
    /// </summary>
    public interface IExtensionBlockMemberInfo
    {
        /// <summary>
        /// Gets the extension block that declares the extension member.
        /// </summary>
        /// <value>
        /// An instance of <see cref="ExtensionBlockInfo"/> representing the extension block information that declares the extension member.
        /// </value>
        ExtensionBlockInfo DeclaringBlock { get; }
    }
}

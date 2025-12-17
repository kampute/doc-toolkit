// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Reflection
{
    using System.Reflection;

    /// <summary>
    /// Provides an interface for reflection property information that represent extension properties defined in an extension block.
    /// </summary>
    public interface IExtensionBlockPropertyInfo : IExtensionBlockMemberInfo
    {
        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> representing the extension property as it appears on the extended type (extension receiver).
        /// </summary>
        /// <value>
        /// An instance of <see cref="PropertyInfo"/> representing the extension property as it appears on the extended type.
        /// </value>
        PropertyInfo ReceivedProperty { get; }
    }
}

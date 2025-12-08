// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Reflection
{
    using System.Reflection;

    /// <summary>
    /// Provides an interface for reflection property info objects that represent extension properties.
    /// </summary>
    public interface IExtensionPropertyInfo : IExtensionMemberInfo
    {
        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> representing the extension property as it appears on the receiver (the extended type).
        /// </summary>
        /// <value>
        /// The <see cref="PropertyInfo"/> representing the extension property as it appears on the receiver (the extended type).
        /// </value>
        PropertyInfo ReceiverProperty { get; }
    }
}

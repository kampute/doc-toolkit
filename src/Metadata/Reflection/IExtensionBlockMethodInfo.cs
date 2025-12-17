// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Reflection
{
    using System.Reflection;

    /// <summary>
    /// Provides an interface for reflection method information that represent extension methods defined in an extension block.
    /// </summary>
    public interface IExtensionBlockMethodInfo : IExtensionBlockMemberInfo
    {
        /// <summary>
        /// Gets the <see cref="MethodInfo"/> representing the declaration of the extension method.
        /// </summary>
        /// <value>
        /// An instance of <see cref="MethodInfo"/> representing the declaration of the extension method.
        /// </value>
        MethodInfo DeclaredMethod { get; }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> representing the extension method as it appears on the extended type (extension receiver).
        /// </summary>
        /// <value>
        /// An instance of <see cref="MethodInfo"/> representing the extension method as it appears on the extended type.
        /// </value>
        MethodInfo ReceivedMethod { get; }
    }
}

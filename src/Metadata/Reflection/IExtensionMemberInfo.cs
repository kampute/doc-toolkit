// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Reflection
{
    using System.Reflection;

    /// <summary>
    /// Provides an interface for reflection member info objects that represent extension members.
    /// </summary>
    public interface IExtensionMemberInfo
    {
        /// <summary>
        /// Gets the parameter information for the receiver of the extension member.
        /// </summary>
        /// <value>
        /// The <see cref="ParameterInfo"/> representing the receiver parameter.
        /// </value>
        ParameterInfo ReceiverParameter { get; }
    }
}

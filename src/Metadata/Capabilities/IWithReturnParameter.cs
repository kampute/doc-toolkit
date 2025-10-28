// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    /// <summary>
    /// Defines a contract for type members that have a return parameter.
    /// </summary>
    public interface IWithReturnParameter
    {
        /// <summary>
        /// Gets a value indicating whether the member returns a value.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the member returns a value; otherwise, <see langword="false"/> if the return type is <see langword="void"/>.
        /// </value>
        bool HasReturnValue => Return.Type.FullName != "System.Void";

        /// <summary>
        /// Gets the return parameter of the member.
        /// </summary>
        /// <value>
        /// An <see cref="IParameter"/> instance representing the return parameter of the member.
        /// </value>
        IParameter Return { get; }
    }
}

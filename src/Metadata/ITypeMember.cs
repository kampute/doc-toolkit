// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Defines a contract for accessing type member metadata of a specific underlying type.
    /// </summary>
    public interface ITypeMember : IMember
    {
        /// <summary>
        /// Gets the type that declares this member.
        /// </summary>
        /// <value>
        /// The declaring type metadata.
        /// </value>
        new IType DeclaringType { get; }
    }
}

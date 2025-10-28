// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Represents the visibility states for a member.
    /// </summary>
    public enum MemberVisibility
    {
        /// <summary>
        /// The member is private and accessible only within its own class or struct.
        /// </summary>
        Private,

        /// <summary>
        /// The member is accessible within its own class and by derived class instances in the same assembly.
        /// </summary>
        PrivateProtected,

        /// <summary>
        /// The member is accessible within its own assembly.
        /// </summary>
        Internal,

        /// <summary>
        /// The member is protected and visible outside its assembly only to derived types.
        /// </summary>
        Protected,

        /// <summary>
        /// The member is accessible within its own assembly and by derived class instances.
        /// </summary>
        ProtectedInternal,

        /// <summary>
        /// The member is accessible from any other code.
        /// </summary>
        Public
    }
}

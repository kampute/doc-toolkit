// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Represents the virtuality states for a member.
    /// </summary>
    public enum MemberVirtuality
    {
        /// <summary>
        /// The member is neither virtual, abstract nor an override.
        /// </summary>
        None,

        /// <summary>
        /// The member is declared virtual.
        /// </summary>
        Virtual,

        /// <summary>
        /// The member is declared abstract.
        /// </summary>
        Abstract,

        /// <summary>
        /// The member overrides a base implementation.
        /// </summary>
        Override,

        /// <summary>
        /// The member overrides a base implementation and is sealed.
        /// </summary>
        SealedOverride
    }
}

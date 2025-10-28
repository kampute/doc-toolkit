// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    /// <summary>
    /// Defines a contract for metadata elements that can have custom modifiers.
    /// </summary>
    public interface IWithCustomModifiers
    {
        /// <summary>
        /// Determines whether the element has a specific required custom modifier applied.
        /// </summary>
        /// <param name="modifierFullName">The full name of the modifier type to check for.</param>
        /// <returns>
        /// <see langword="true"/> if the element has the specified required custom modifier applied; otherwise, <see langword="false"/>.
        /// </returns>
        bool HasRequiredCustomModifier(string modifierFullName);

        /// <summary>
        /// Determines whether the element has a specific optional custom modifier applied.
        /// </summary>
        /// <param name="modifierFullName">The full name of the modifier type to check for.</param>
        /// <returns>
        /// <see langword="true"/> if the element has the specified optional custom modifier applied; otherwise, <see langword="false"/>.
        /// </returns>
        bool HasOptionalCustomModifier(string modifierFullName);
    }
}

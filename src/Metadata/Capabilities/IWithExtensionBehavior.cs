// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    /// <summary>
    /// Defines a contract for members that can exhibit extension behavior.
    /// </summary>
    /// <remarks>
    /// Members implementing this interface can represent extension members, providing access to their associated
    /// extension block information and related behaviors.
    /// </remarks>
    public interface IWithExtensionBehavior
    {
        /// <summary>
        /// Gets the extension block information if the member is an extension member.
        /// </summary>
        /// <value>
        /// An <see cref="IExtensionBlock"/> instance representing the extension block information; otherwise, <see langword="null"/> 
        /// if the member is not an extension member.
        /// </value>
        IExtensionBlock? ExtensionBlock { get; }

        /// <summary>
        /// Gets a value indicating whether the member is an extension member.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the member is an extension member; otherwise, <see langword="false"/>.
        /// </value>
        bool IsExtension => ExtensionBlock is not null;

        /// <summary>
        /// Determines whether this member is an extension of the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><see langword="true"/> if this member is an extension of the specified type; otherwise, <see langword="false"/>.</returns>
        bool IsExtensionOf(IType type) => ExtensionBlock?.Extends(type) ?? false;
    }
}

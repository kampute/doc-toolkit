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
        /// Gets a value indicating whether the member is an extension member.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the member is an extension member; otherwise, <see langword="false"/>.
        /// </value>
        bool IsExtension => ExtensionReceiver is not null;

        /// <summary>
        /// Gets the receiver parameter of the extension member if it is an extension member.
        /// </summary>
        /// <value>
        /// The receiver parameter of the extension member, or <see langword="null"/> if the member is not an extension member.
        /// </value>
        IParameter? ExtensionReceiver => ExtensionBlock?.Receiver;

        /// <summary>
        /// Gets the extension block where the member is defined if the member is a block extension member.
        /// </summary>
        /// <value>
        /// The extension block where the member is defined, or <see langword="null"/> if the member is not an extension member or
        /// is a classic (non-block) extension method.
        /// </value>
        /// <remarks>
        /// The extension block provides context about the extension member, including its receiver parameter and other members
        /// defined within the same block.
        /// <para>
        /// If the member is a classic extension method, the extension block will be <see langword="null"/>. In such cases, the
        /// receiver parameter is represented directly by the first parameter of the method.
        /// </para>
        /// </remarks>
        IExtensionBlock? ExtensionBlock { get; }

        /// <summary>
        /// Determines whether this member is an extension of the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><see langword="true"/> if this member is an extension of the specified type; otherwise, <see langword="false"/>.</returns>
        bool Extends(IType type) => ExtensionReceiver is not null && ExtensionReceiver.Type.IsAssignableFrom(type);
    }
}

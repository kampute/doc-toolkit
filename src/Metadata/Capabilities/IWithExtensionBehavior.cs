// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    /// <summary>
    /// Defines a contract for members that are can be an extension member of a type.
    /// </summary>
    public interface IWithExtensionBehavior
    {
        /// <summary>
        /// Gets the receiver parameter of the extension member.
        /// </summary>
        /// <value>
        /// The <see cref="IParameter"/> representing the receiver parameter if the member is an extension member; otherwise, <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// The receiver parameter represents the type and modifiers of the instance that the extension member extends.
        /// If the member is not an extension member, this property returns <see langword="null"/>.
        /// </remarks>
        IParameter? ReceiverParameter { get; }

        /// <summary>
        /// Gets a value indicating whether the member is an extension member.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the member is an extension member; otherwise, <see langword="false"/>.
        /// </value>
        bool IsExtension => ReceiverParameter is not null;

        /// <summary>
        /// Determines whether this member is an extension of the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><see langword="true"/> if this member is an extension of the specified type; otherwise, <see langword="false"/>.</returns>
        bool IsExtensionOf(IType type) => ReceiverParameter?.Type.IsAssignableFrom(type) ?? false;
    }
}

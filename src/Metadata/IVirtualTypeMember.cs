// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Defines a contract for accessing metadata of members that can be overridden or implemented.
    /// </summary>
    public interface IVirtualTypeMember : ITypeMember
    {
        /// <summary>
        /// Gets the virtuality of the member.
        /// </summary>
        /// <value>
        /// A <see cref="MemberVirtuality"/> value indicating whether the member is virtual,
        /// abstract, an override, or none of those.
        /// </value>
        MemberVirtuality Virtuality { get; }

        /// <summary>
        /// Gets a value indicating whether the member is abstract.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the member is abstract; otherwise, <see langword="false"/>.
        /// </value>
        bool IsAbstract => Virtuality is MemberVirtuality.Abstract;

        /// <summary>
        /// Gets a value indicating whether the member can be overridden in derived types.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the member can be overridden; otherwise, <see langword="false"/>.
        /// </value>
        bool IsOverridable => Virtuality is MemberVirtuality.Virtual or MemberVirtuality.Abstract or MemberVirtuality.Override;

        /// <summary>
        /// Gets a value indicating whether the member is an explicit interface implementation.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the member is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// </value>
        bool IsExplicitInterfaceImplementation { get; }

        /// <summary>
        /// Gets a value indicating whether the member is a default interface implementation.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the member is a default interface implementation; otherwise, <see langword="false"/>.
        /// </value>
        bool IsDefaultInterfaceImplementation => DeclaringType.IsInterface && !IsAbstract;

        /// <summary>
        /// Gets the base member that this member overrides in a base class, if any.
        /// </summary>
        /// <value>
        /// The base member that this member overrides, or <see langword="null"/> if none.
        /// </value>
        IVirtualTypeMember? OverriddenMember { get; }

        /// <summary>
        /// Gets the interface member that this member implements, if any.
        /// </summary>
        /// <value>
        /// The interface member that this member implements, or <see langword="null"/> if none.
        /// </value>
        IVirtualTypeMember? ImplementedMember { get; }

        /// <summary>
        /// Get the generic member definition for this member, if it is a constructed generic member.
        /// </summary>
        /// <value>
        /// The generic member definition, or <see langword="null"/> if this member is not a constructed generic member.
        /// </value>
        IVirtualTypeMember? GenericMemberDefinition { get; }
    }
}

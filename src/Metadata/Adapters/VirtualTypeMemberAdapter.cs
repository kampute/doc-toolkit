// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using System;
    using System.Reflection;

    /// <summary>
    /// An abstract base class for adapters that wrap reflected type members that can be virtual.
    /// </summary>
    /// <typeparam name="T">The type of the reflected member.</typeparam>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="MemberInfo"/> and the metadata
    /// representation defined by the <see cref="IVirtualTypeMember"/> interface. It provides access to virtual
    /// type member-level information regardless of whether the assembly containing the member was loaded
    /// via Common Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public abstract class VirtualTypeMemberAdapter<T> : TypeMemberAdapter<T>, IVirtualTypeMember
        where T : MemberInfo
    {
        private readonly Lazy<MemberVirtuality> virtuality;
        private readonly Lazy<IVirtualTypeMember?> overriddenMember;
        private readonly Lazy<IVirtualTypeMember?> implementedMember;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualTypeMemberAdapter{T}"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type metadata of the member.</param>
        /// <param name="underlying">The underlying member info.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="underlying"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringType"/> does not match the member's declaring type.</exception>
        protected VirtualTypeMemberAdapter(IType declaringType, T underlying)
            : base(declaringType, underlying)
        {
            virtuality = new(GetMemberVirtuality);
            overriddenMember = new(FindOverriddenMember);
            implementedMember = new(FindImplementedMember);
        }

        /// <inheritdoc/>
        public MemberVirtuality Virtuality => virtuality.Value;

        /// <inheritdoc/>
        public virtual bool IsInterfaceMember => Reflection.DeclaringType!.IsInterface;

        /// <inheritdoc/>
        public virtual bool IsExplicitInterfaceImplementation => !IsStatic && Name.IndexOf('.') > 0;

        /// <inheritdoc/>
        public IVirtualTypeMember? OverriddenMember => overriddenMember.Value;

        /// <inheritdoc/>
        public IVirtualTypeMember? ImplementedMember => implementedMember.Value;

        /// <inheritdoc/>
        public IVirtualTypeMember? GenericMemberDefinition => FindGenericDefinition();

        /// <summary>
        /// Retrieves the virtuality of the member.
        /// </summary>
        /// <returns>The <see cref="MemberVirtuality"/> of the member.</returns>
        protected abstract MemberVirtuality GetMemberVirtuality();

        /// <summary>
        /// Searches the base types for a member that this member overrides.
        /// </summary>
        /// <returns>The overridden member if found; otherwise, <see langword="null"/>.</returns>
        protected abstract IVirtualTypeMember? FindOverriddenMember();

        /// <summary>
        /// Searches the implemented interfaces for a member that this member implements.
        /// </summary>
        /// <returns>The implemented member if found; otherwise, <see langword="null"/>.</returns>
        protected abstract IVirtualTypeMember? FindImplementedMember();

        /// <summary>
        /// Searches for the generic definition of this member if it is a constructed generic member.
        /// </summary>
        /// <returns>The generic member definition if found; otherwise, <see langword="null"/>.</returns>
        protected abstract IVirtualTypeMember? FindGenericDefinition();
    }
}

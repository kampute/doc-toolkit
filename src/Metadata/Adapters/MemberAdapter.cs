// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// An abstract base class for adapters that wrap reflected members.
    /// </summary>
    /// <typeparam name="T">The type of the reflected member.</typeparam>
    /// <threadsafety static="true" instance="true"/>
    public abstract class MemberAdapter<T> : AttributeAwareMetadataAdapter<T>, IMember
        where T : MemberInfo
    {
        private readonly Lazy<MemberVisibility> visibility;
        private readonly Lazy<IType?> reflectedType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberAdapter{T}"/> class.
        /// </summary>
        /// <param name="memberInfo">The reflection information of the member to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberInfo"/> is <see langword="null"/>.</exception>
        protected MemberAdapter(T memberInfo)
            : base(memberInfo)
        {
            visibility = new(GetMemberVisibility);
            reflectedType = new(GetReflectedType);
        }

        /// <inheritdoc/>
        public abstract IAssembly Assembly { get; }

        /// <inheritdoc/>
        public abstract string Namespace { get; }

        /// <inheritdoc/>
        public abstract IType? DeclaringType { get; }

        /// <inheritdoc/>
        public IType? ReflectedType => reflectedType.Value;

        /// <inheritdoc/>
        public override string Name => Reflection.Name;

        /// <inheritdoc/>
        public MemberVisibility Visibility => visibility.Value;

        /// <inheritdoc/>
        public virtual bool IsVisible => Visibility is MemberVisibility.Public or MemberVisibility.Protected or MemberVisibility.ProtectedInternal;

        /// <inheritdoc/>
        public virtual bool IsPublic => Visibility == MemberVisibility.Public;

        /// <inheritdoc/>
        public abstract bool IsStatic { get; }

        /// <inheritdoc/>
        public abstract bool IsUnsafe { get; }

        /// <inheritdoc/>
        public abstract bool IsSpecialName { get; }

        /// <inheritdoc/>
        public virtual bool IsDirectDeclaration => true;

        /// <inheritdoc/>
        public bool IsCompilerGenerated => HasCustomAttribute(AttributeNames.CompilerGenerated);

        /// <inheritdoc/>
        public abstract string CodeReference { get; }

        /// <inheritdoc/>
        protected override IEnumerable<CustomAttributeData> GetCustomAttributes() => Reflection.CustomAttributes;

        /// <summary>
        /// Retrieves the visibility of the member.
        /// </summary>
        /// <returns>The <see cref="MemberVisibility"/> of the member.</returns>
        protected abstract MemberVisibility GetMemberVisibility();

        /// <summary>
        /// Retrieves the metadata type that was used to obtain this member.
        /// </summary>
        /// <returns>The metadata type that was used to obtain this member, or <see langword="null"/> if the member was obtained through a different mechanism.</returns>
        protected virtual IType? GetReflectedType() => Reflection.ReflectedType?.GetMetadata<IType>();

        /// <inheritdoc/>
        bool IMetadataAdapter<MemberInfo>.Represents(MemberInfo reflection) => ReferenceEquals(Reflection, reflection);
    }
}

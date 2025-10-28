// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Models
{
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a documentation model for a type member.
    /// </summary>
    /// <remarks>
    /// The <see cref="TypeMemberModel"/> class provides a foundation for documenting type members such as constructors, methods,
    /// properties, events, operators, and fields.
    /// </remarks>
    public abstract class TypeMemberModel : MemberModel<ITypeMember>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMemberModel"/> class.
        /// </summary>
        /// <param name="declaringType">The type that declares the member.</param>
        /// <param name="member">The metadata of the type member represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="member"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="member"/> is not declared by <paramref name="declaringType"/>.</exception>
        protected TypeMemberModel(TypeModel declaringType, ITypeMember member)
            : base(member ?? throw new ArgumentNullException(nameof(member)))
        {
            DeclaringType = declaringType ?? throw new ArgumentNullException(nameof(declaringType));
        }

        /// <summary>
        /// Gets the assembly that contains the member.
        /// </summary>
        /// <value>
        /// The assembly that contains the member.
        /// </value>
        public override AssemblyModel Assembly => DeclaringType.Assembly;

        /// <summary>
        /// Gets the declaring type of the member.
        /// </summary>
        /// <value>
        /// The type that declares the member.
        /// </value>
        public override TypeModel DeclaringType { get; }

        /// <inheritdoc/>
        public override IEnumerable<IDocumentModel> HierarchyPath
            => Context.AddressProvider.Granularity.HasFlag(PageGranularity.Type) ? DeclaringType.HierarchyPath.Append(DeclaringType) : [];

        /// <inheritdoc/>
        protected override NameQualifier MemberNameQualifier => NameQualifier.None;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>The name of the member qualified by the name of its declaring type.</returns>
        public override string ToString() => $"{DeclaringType.Name}{Type.Delimiter}{Name}";
    }

    /// <summary>
    /// Represents a documentation model for a specific type of type member.
    /// </summary>
    /// <typeparam name="T">The type of the reflection metadata, which must implement <see cref="ITypeMember"/>.</typeparam>
    public abstract class TypeMemberModel<T> : TypeMemberModel
        where T : class, ITypeMember
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMemberModel{T}"/> class.
        /// </summary>
        /// <param name="declaringType">The type that declares the member.</param>
        /// <param name="member">The metadata of the type member represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="member"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="member"/> is not declared by <paramref name="declaringType"/>.</exception>
        protected TypeMemberModel(TypeModel declaringType, T member)
            : base(declaringType, member)
        {
        }

        /// <summary>
        /// Gets the reflection metadata for this member.
        /// </summary>
        /// <value>
        /// The reflection metadata object that provides detailed information about the member.
        /// </value>
        public new T Metadata => (T)base.Metadata;
    }
}

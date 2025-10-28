// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Models
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a documentation model for a .NET type.
    /// </summary>
    /// <remarks>
    /// The <see cref="TypeModel"/> class serves as the foundation for all type representations. It provides the
    /// base implementation for documenting .NET types, encapsulating common properties and behaviors shared across
    /// all types.
    /// </remarks>
    public abstract class TypeModel : MemberModel<IType>, IComparable<TypeModel>
    {
        private readonly Lazy<List<MethodModel>> extensionMethods;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeModel"/> class.
        /// </summary>
        /// <param name="declaringEntity">The object that declares the type, which is either an <see cref="AssemblyModel"/> for top-level types or a <see cref="TypeModel"/> for nested types.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="declaringEntity"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is not a relevant instance of <see cref="AssemblyModel"/> or <see cref="TypeModel"/>.</exception>
        protected TypeModel(object declaringEntity, IType type)
            : base(type ?? throw new ArgumentNullException(nameof(type)))
        {
            if (declaringEntity is null)
                throw new ArgumentNullException(nameof(declaringEntity));

            if (type.DeclaringType is null)
            {
                if (declaringEntity is not AssemblyModel assembly)
                    throw new ArgumentException("The declaring entity must be an instance of AssemblyInfo.", nameof(declaringEntity));
                if (!ReferenceEquals(type.Assembly, assembly.Metadata))
                    throw new ArgumentException("The declaring entity must be an AssemblyInfo instance that contains the type.", nameof(declaringEntity));

                Assembly = assembly;
            }
            else
            {
                if (declaringEntity is not TypeModel declaringType)
                    throw new ArgumentException("The declaring entity must be an instance of TypeElement.", nameof(declaringEntity));
                if (!ReferenceEquals(type.DeclaringType, declaringType.Metadata))
                    throw new ArgumentException("The declaring entity must be a TypeElement instance that declares the type.", nameof(declaringEntity));

                Assembly = declaringType.Assembly;
                DeclaringType = declaringType;
            }

            extensionMethods = new(() => [.. Metadata.ExtensionMethods
                .Select(Context.FindMember)
                .OfType<MethodModel>()
                .OrderBy(m => m.Name)
                .ThenBy(m => m.Metadata.Parameters.Count)]);
        }

        /// <summary>
        /// Gets the assembly that contains the type.
        /// </summary>
        /// <value>
        /// The assembly that contains the type.
        /// </value>
        public override AssemblyModel Assembly { get; }

        /// <summary>
        /// Gets the declaring type of the type.
        /// </summary>
        /// <value>
        /// The type that declares this type, or <see langword="null"/> if the type is not nested.
        /// </value>
        public override TypeModel? DeclaringType { get; }

        /// <summary>
        /// Gets all the members declared or implemented by the type.
        /// </summary>
        /// <value>
        /// An enumerable collection of <see cref="TypeMemberModel"/> representing all members declared or implemented by the type.
        /// </value>
        public virtual IEnumerable<TypeMemberModel> Members => [];

        /// <summary>
        /// Gets the extension methods associated with the type.
        /// </summary>
        /// <value>
        /// A read-only collection of extension methods associated with the type.
        /// </value>
        public IReadOnlyList<MethodModel> ExtensionMethods => extensionMethods.Value;

        /// <inheritdoc/>
        public override IEnumerable<IDocumentModel> HierarchyPath
            => Context.AddressProvider.Granularity.HasFlag(PageGranularity.Namespace) ? Namespace.HierarchyPath.Append(Namespace) : [];

        /// <inheritdoc/>
        protected override NameQualifier MemberNameQualifier => NameQualifier.DeclaringType;

        /// <summary>
        /// Compares the current type with another type based on their full names.
        /// </summary>
        /// <param name="other">The other type to compare with.</param>
        /// <returns>An integer that indicates the relative order of the types being compared.</returns>
        public int CompareTo(TypeModel? other) => other is not null ? string.Compare(Metadata.FullName, other.Metadata.FullName, StringComparison.Ordinal) : 1;

        /// <summary>
        /// Finds the documentation model representing the specified reflection member metadata within the type.
        /// </summary>
        /// <param name="member">The metadata of the member to find the documentation model for.</param>
        /// <returns>The documentation model representing the member if found; otherwise, <see langword="null"/>.</returns>
        public virtual MemberModel? FindMember(IMember member) => null;
    }

    /// <summary>
    /// Represents a documentation model for a .NET type of a specific kind.
    /// </summary>
    /// <typeparam name="T">The type of the reflection metadata, which must implement <see cref="IType"/>.</typeparam>
    public abstract class TypeModel<T> : TypeModel
        where T : class, IType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeModel{T}"/> class.
        /// </summary>
        /// <param name="declaringEntity">The object that declares the type, which is either an <see cref="AssemblyModel"/> for top-level types or a <see cref="TypeModel"/> for nested types.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="declaringEntity"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is not a relevant instance of <see cref="AssemblyModel"/> or <see cref="TypeModel"/>.</exception>
        protected TypeModel(object declaringEntity, T type)
            : base(declaringEntity, type)
        {
        }

        /// <summary>
        /// Gets the reflection metadata for the type.
        /// </summary>
        /// <value>
        /// The reflection metadata object that provides detailed information about the type.
        /// </value>
        public new T Metadata => (T)base.Metadata;
    }
}

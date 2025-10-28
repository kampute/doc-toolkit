// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Models
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a documentation model for a .NET enumeration type.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public class EnumModel : TypeModel<IEnumType>
    {
        private readonly Lazy<List<FieldModel>> values;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumModel"/> class.
        /// </summary>
        /// <param name="declaringEntity">The declaring entity of the type, which is either an <see cref="AssemblyModel"/> for top-level types or a <see cref="TypeModel"/> for nested types.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringEntity"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is not an instance of <see cref="AssemblyModel"/> or <see cref="TypeModel"/>.</exception>
        protected EnumModel(object declaringEntity, IEnumType type)
            : base(declaringEntity, type)
        {
            values = new(() => [.. type.Fields.Select(field => new FieldModel(this, field))]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumModel"/> class as a top-level type.
        /// </summary>
        /// <param name="assembly">The assembly that contains the type.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not a top-level type of <paramref name="assembly"/>.</exception>
        public EnumModel(AssemblyModel assembly, IEnumType type)
            : this((object)assembly ?? throw new ArgumentNullException(nameof(assembly)), type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumModel"/> class as a nested type.
        /// </summary>
        /// <param name="declaringType">The type that declares the type.</param>
        /// <param name="type">The type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not directedly nested within <paramref name="declaringType"/>.</exception>
        public EnumModel(TypeModel declaringType, IEnumType type)
            : this((object)declaringType ?? throw new ArgumentNullException(nameof(declaringType)), type)
        {
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is always <see cref="DocumentationModelType.Enum"/> for this model.
        /// </value>
        public override DocumentationModelType ModelType => DocumentationModelType.Enum;

        /// <summary>
        /// Gets the values of the enumeration type.
        /// </summary>
        /// <value>
        /// The read-only collection of the values of the <see langword="enum"/>.
        /// The values are ordered as they appear in the metadata.
        /// </value>
        public IReadOnlyList<FieldModel> Values => values.Value;

        /// <inheritdoc/>
        public override IEnumerable<TypeMemberModel> Members => Values;

        /// <inheritdoc/>
        public override MemberModel? FindMember(IMember member)
        {
            if (member is null || !ReferenceEquals(Metadata, member.DeclaringType) || member is not IField { IsEnumValue: true })
                return null;

            return Values.FirstOrDefault(v => ReferenceEquals(v.Metadata, member));
        }
    }
}

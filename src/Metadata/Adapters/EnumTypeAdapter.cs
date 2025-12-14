// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// An adapter that wraps a reflection <see cref="Type"/> representing an enum type and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="Type"/> and the metadata
    /// representation defined by the <see cref="IEnumType"/> interface. It provides access to enum-level
    /// information regardless of whether the assembly containing the enum was loaded via Common Language
    /// Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class EnumTypeAdapter : TypeAdapter, IEnumType
    {
        private readonly Lazy<IType> underlyingType;
        private readonly Lazy<IReadOnlyList<IField>> fields;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumTypeAdapter"/> class.
        /// </summary>
        /// <param name="declaringEntity">The assembly or type that declares the enum type.</param>
        /// <param name="enumType">The reflection information of the enum type to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringEntity"/> or <paramref name="enumType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is neither an <see cref="IAssembly"/> nor an <see cref="IType"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="enumType"/> is not declared by the <paramref name="declaringEntity"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="enumType"/> is a nested type but <paramref name="declaringEntity"/> is an assembly, or when <paramref name="enumType"/> is a top-level type but <paramref name="declaringEntity"/> is a type.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="enumType"/> is not an enum.</exception>
        public EnumTypeAdapter(object declaringEntity, Type enumType)
            : base(declaringEntity, enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("Type must be an enum.", nameof(enumType));

            underlyingType = new(() => MetadataProvider.GetMetadata(Enum.GetUnderlyingType(Reflection)));
            fields = new(() => [.. GetValues().Select(Assembly.Repository.GetFieldMetadata)]);
        }

        /// <inheritdoc/>
        public bool IsFlagsEnum => HasCustomAttribute(AttributeNames.Flags);

        /// <inheritdoc/>
        public virtual IType UnderlyingType => underlyingType.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IField> Fields => fields.Value;

        /// <inheritdoc/>
        public override bool IsAssignableFrom(IType source) => base.IsAssignableFrom(source) || UnderlyingType.IsAssignableFrom(source);

        /// <inheritdoc/>
        public virtual string? GetEnumName(object value)
        {
            if (value is null || ToNumericValue(value) is not ulong numericValue)
                return null;

            foreach (var field in Fields)
                if (ToNumericValue(field.LiteralValue) == numericValue)
                    return field.Name;

            return null;

            static ulong? ToNumericValue(object? val)
            {
                try
                {
                    return Convert.ToUInt64(val, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Retrieves the fields representing the enum values.
        /// </summary>
        /// <returns>An enumerable of <see cref="FieldInfo"/> representing the enum values.</returns>
        protected virtual IEnumerable<FieldInfo> GetValues() => Reflection
            .GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral);
    }
}

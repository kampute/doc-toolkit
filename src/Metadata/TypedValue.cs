// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System;

    /// <summary>
    /// Represents a value along with its associated type.
    /// </summary>
    public readonly struct TypedValue : IEquatable<TypedValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypedValue"/> struct with the specified type and value.
        /// </summary>
        /// <param name="type">The type of the value.</param>
        /// <param name="value">The value itself.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        public TypedValue(IType type, object? value)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Value = value;
        }

        /// <summary>
        /// Gets the type associated with the value.
        /// </summary>
        /// <value>
        /// The type of the value.
        /// </value>
        public readonly IType Type { get; }

        /// <summary>
        /// Gets the value itself.
        /// </summary>
        /// <value>
        /// The value, which can be <see langword="null"/>.
        /// </value>
        public readonly object? Value { get; }

        /// <summary>
        /// Determines whether the specified <see cref="TypedValue"/> is equal to the current <see cref="TypedValue"/>.
        /// </summary>
        /// <param name="other">The <see cref="TypedValue"/> to compare with the current <see cref="TypedValue"/>.</param>
        /// <returns><see langword="true"/> if the specified <see cref="TypedValue"/> is equal to the current <see cref="TypedValue"/>; otherwise, <see langword="false"/>.</returns>
        public readonly bool Equals(TypedValue other) => Equals(other.Type, Type) && Equals(other.Value, Value);

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="TypedValue"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current <see cref="TypedValue"/>.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current <see cref="TypedValue"/>; otherwise, <see langword="false"/>.</returns>
        public override readonly bool Equals(object? obj) => obj is TypedValue other && Equals(other);

        /// <summary>
        /// Returns a hash code for the current <see cref="TypedValue"/>.
        /// </summary>
        /// <returns>A hash code for the current <see cref="TypedValue"/>.</returns>
        public override readonly int GetHashCode() => HashCode.Combine(Type, Value);

        /// <summary>
        /// Returns a string that represents the current <see cref="TypedValue"/>.
        /// </summary>
        /// <returns>A string that represents the current <see cref="TypedValue"/>.</returns>
        public override readonly string ToString() => $"{Type.FullName}: {Value ?? "null"}";

        /// <summary>
        /// Determines whether two specified <see cref="TypedValue"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="TypedValue"/> to compare.</param>
        /// <param name="right">The second <see cref="TypedValue"/> to compare.</param>
        /// <returns><see langword="true"/> if the two <see cref="TypedValue"/> instances are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(TypedValue left, TypedValue right) => left.Equals(right);

        /// <summary>
        /// Determines whether two <see cref="TypedValue"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="TypedValue"/> to compare.</param>
        /// <param name="right">The second <see cref="TypedValue"/> to compare.</param>
        /// <returns><see langword="true"/> if the specified <see cref="TypedValue"/> instances are not equal; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool operator !=(TypedValue left, TypedValue right) => !(left == right);
    }
}

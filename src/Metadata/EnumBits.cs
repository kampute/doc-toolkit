// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Converts enum values to and from a common 64-bit pattern, so values of any underlying integral type can be
    /// compared and combined bitwise.
    /// </summary>
    /// <remarks>
    /// Signed values are sign-extended and unsigned values are zero-extended. As a result, a negative value such as
    /// <c>~0</c> keeps all of its bits set instead of failing the conversion, and values of the same enum that are
    /// boxed as different integral types produce the same pattern.
    /// </remarks>
    internal static class EnumBits
    {
        /// <summary>
        /// Converts the specified enum or integral value to its 64-bit pattern.
        /// </summary>
        /// <param name="value">The enum or integral value to convert.</param>
        /// <returns>The 64-bit pattern of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidCastException">Thrown when <paramref name="value"/> is not convertible to an integral value.</exception>
        /// <exception cref="OverflowException">Thrown when <paramref name="value"/> is a non-integral value outside the range of <see cref="ulong"/>.</exception>
        public static ulong ToBits(object value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return Convert.GetTypeCode(value) switch
            {
                TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64
                    => unchecked((ulong)Convert.ToInt64(value, CultureInfo.InvariantCulture)),
                _ => Convert.ToUInt64(value, CultureInfo.InvariantCulture),
            };
        }

        /// <summary>
        /// Converts the specified 64-bit pattern back to a value of the integral type identified by <paramref name="typeCode"/>.
        /// </summary>
        /// <param name="bits">The 64-bit pattern to convert.</param>
        /// <param name="typeCode">The type code of the integral type to convert to.</param>
        /// <returns>The value of <paramref name="bits"/> truncated to the integral type identified by <paramref name="typeCode"/>, or <paramref name="bits"/> itself if <paramref name="typeCode"/> is not an integral type.</returns>
        public static object FromBits(ulong bits, TypeCode typeCode) => typeCode switch
        {
            TypeCode.SByte => unchecked((sbyte)bits),
            TypeCode.Byte => unchecked((byte)bits),
            TypeCode.Int16 => unchecked((short)bits),
            TypeCode.UInt16 => unchecked((ushort)bits),
            TypeCode.Int32 => unchecked((int)bits),
            TypeCode.UInt32 => unchecked((uint)bits),
            TypeCode.Int64 => unchecked((long)bits),
            _ => bits,
        };
    }
}

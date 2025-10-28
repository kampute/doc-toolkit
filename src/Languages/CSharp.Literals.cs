// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Languages
{
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    public partial class CSharp
    {
        /// <summary>
        /// Writes a literal value to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="value">The literal value to write.</param>
        /// <exception cref="ArgumentNullException">Throw if <paramref name="writer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Throw if <paramref name="value"/> is not a valid literal.</exception>
        public static void WriteLiteral(TextWriter writer, object value)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            switch (value)
            {
                case null:
                    writer.Write("null");
                    break;
                case bool booleanLiteral:
                    writer.Write(booleanLiteral ? "true" : "false");
                    break;
                case float floatLiteral:
                    writer.Write(floatLiteral.ToString(null, CultureInfo.InvariantCulture));
                    writer.Write('F');
                    break;
                case double doubleLiteral:
                    writer.Write(doubleLiteral.ToString(null, CultureInfo.InvariantCulture));
                    break;
                case decimal decimalLiteral:
                    writer.Write(decimalLiteral.ToString(null, CultureInfo.InvariantCulture));
                    writer.Write('M');
                    break;
                case long longLiteral:
                    writer.Write(longLiteral.ToString(null, CultureInfo.InvariantCulture));
                    break;
                case ulong unsignedLongLiteral:
                    writer.Write(unsignedLongLiteral.ToString(null, CultureInfo.InvariantCulture));
                    break;
                case int integerLiteral:
                    writer.Write(integerLiteral.ToString(null, CultureInfo.InvariantCulture));
                    break;
                case uint unsignedIntegerLiteral:
                    writer.Write(unsignedIntegerLiteral.ToString(null, CultureInfo.InvariantCulture));
                    break;
                case short shortIntegerLiteral:
                    writer.Write(shortIntegerLiteral.ToString(null, CultureInfo.InvariantCulture));
                    break;
                case ushort unsignedShortLiteral:
                    writer.Write(unsignedShortLiteral.ToString(null, CultureInfo.InvariantCulture));
                    break;
                case byte byteLiteral:
                    writer.Write(byteLiteral.ToString(null, CultureInfo.InvariantCulture));
                    break;
                case sbyte signedLiteral:
                    writer.Write(signedLiteral.ToString(null, CultureInfo.InvariantCulture));
                    break;
                case char charLiteral:
                    writer.Write('\'');
                    WriteCharLiteral(writer, charLiteral, '\'');
                    writer.Write('\'');
                    break;
                case string stringLiteral:
                    writer.Write('"');
                    foreach (var c in stringLiteral.AsSpan())
                        WriteCharLiteral(writer, c, '"');
                    writer.Write('"');
                    break;
                default:
                    throw new ArgumentException("The value is not a valid literal.", nameof(value));
            }
        }

        /// <summary>
        /// Writes a character to the <see cref="TextWriter"/>, escaping it if necessary.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="c">The character to write.</param>
        /// <param name="quote">The quote character used to delimit the string or character literal.</param>
        private static void WriteCharLiteral(TextWriter writer, char c, char quote)
        {
            switch (c)
            {
                case '\\':
                    writer.Write(@"\\");
                    return;
                case '\n':
                    writer.Write(@"\n");
                    return;
                case '\r':
                    writer.Write(@"\r");
                    return;
                case '\t':
                    writer.Write(@"\t");
                    return;
                case '\a':
                    writer.Write(@"\a");
                    return;
                case '\b':
                    writer.Write(@"\b");
                    return;
                case '\f':
                    writer.Write(@"\f");
                    return;
                case '\v':
                    writer.Write(@"\v");
                    return;
                case '\e':
                    writer.Write(@"\e");
                    return;
                case '\0':
                    writer.Write(@"\0");
                    return;
                case ' ':
                    writer.Write(' ');
                    return;
            }

            if (c == quote)
            {
                writer.Write('\\');
                writer.Write(c);
            }
            else if (c is < (char)0x20 or > (char)0x7E)
            {
                writer.Write(@"\u");
                writer.Write(((int)c).ToString("X4", CultureInfo.InvariantCulture));
            }
            else
            {
                writer.Write(c);
            }
        }

        /// <summary>
        /// Writes the specified enum value to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="enumType">The type of the enum.</param>
        /// <param name="enumValue">The enum value to write.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/>, <paramref name="enumType"/>, <paramref name="enumValue"/>, or <paramref name="linker"/> is <see langword="null"/>.</exception>
        private void WriteEnumValue(TextWriter writer, IEnumType enumType, object enumValue, MemberDocLinker linker)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (enumType is null)
                throw new ArgumentNullException(nameof(enumType));
            if (enumValue is null)
                throw new ArgumentNullException(nameof(enumValue));
            if (linker is null)
                throw new ArgumentNullException(nameof(linker));

            if (enumType.GetEnumName(enumValue) is string enumName)
                WriteEnumName(enumName);
            else if (enumType.IsFlagsEnum)
                WriteEnumFlagNames();
            else
                WriteConstantValue(writer, enumValue, enumType.UnderlyingType, linker);

            void WriteEnumFlagNames()
            {
                var flags = enumType.Fields.Select(field =>
                {
                    var value = ToNumericValue(field.LiteralValue!);
                    var bits = CountSetBits(value);
                    return new
                    {
                        field.Name,
                        Value = value,
                        BitCount = bits,
                    };
                }).OrderByDescending(flag => flag.BitCount).ThenBy(flag => flag.Value);

                var remaining = ToNumericValue(enumValue);

                var needsDisjunction = false;
                foreach (var flag in flags)
                {
                    if (flag.Value == 0 || (flag.Value & remaining) != flag.Value)
                        continue;

                    if (needsDisjunction)
                        writer.Write(" | ");
                    else
                        needsDisjunction = true;

                    WriteEnumName(flag.Name);

                    remaining &= ~flag.Value;
                    if (remaining == 0)
                        return;
                }

                if (needsDisjunction)
                    writer.Write(" | ");

                WriteConstantValue(writer, remaining, enumType.UnderlyingType, linker);
            }

            void WriteEnumName(string name)
            {
                linker(writer, enumType, enumType.Name);
                writer.Write(Type.Delimiter);
                writer.Write(name);
            }

            static ulong ToNumericValue(object value)
            {
                return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
            }

            static int CountSetBits(ulong value)
            {
                var count = 0;
                while (value != 0)
                {
                    count += (int)(value & 1);
                    value >>= 1;
                }
                return count;
            }
        }
    }
}

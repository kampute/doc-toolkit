// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Languages
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Support;
    using System;

    /// <summary>
    /// Provides extension methods for formatting code elements using an <see cref="IProgrammingLanguage"/> instance.
    /// </summary>
    public static class LanguageExtensions
    {
        /// <summary>
        /// Formats the definition of the specified member according to syntax rules of the language.
        /// </summary>
        /// <param name="language">The <see cref="IProgrammingLanguage"/> instance.</param>
        /// <param name="member">The member whose definition to be formatted.</param>
        /// <returns>The definition of the specified member as it appears in the source code.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="language"/> or <paramref name="member"/> is <see langword="null"/>.</exception>
        public static string FormatDefinition(this IProgrammingLanguage language, IMember member)
        {
            if (language is null)
                throw new ArgumentNullException(nameof(language));
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            using var writer = StringBuilderPool.Shared.GetWriter();
            language.WriteDefinition(writer, member);
            return writer.ToString();
        }

        /// <summary>
        /// Formats the signature of the specified member according to syntax rules of the language.
        /// </summary>
        /// <param name="language">The <see cref="IProgrammingLanguage"/> instance.</param>
        /// <param name="member">The member whose signature to be formatted.</param>
        /// <param name="qualifier">The level of qualification to apply to the member's name.</param>
        /// <returns>The signature of the specified member according to syntax rules of the language.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="language"/> or <paramref name="member"/> is <see langword="null"/>.</exception>
        public static string FormatSignature(this IProgrammingLanguage language, IMember member, NameQualifier qualifier)
        {
            if (language is null)
                throw new ArgumentNullException(nameof(language));
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            using var writer = StringBuilderPool.Shared.GetWriter();
            language.WriteSignature(writer, member, qualifier);
            return writer.ToString();
        }

        /// <summary>
        /// Formats the name of the specified member according to syntax rules of the language.
        /// </summary>
        /// <param name="language">The <see cref="IProgrammingLanguage"/> instance.</param>
        /// <param name="member">The member whose name to be formatted.</param>
        /// <param name="qualifier">The level of qualification to apply to the member's name.</param>
        /// <returns>The name of the specified member according to syntax rules of the language.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="language"/> or <paramref name="member"/> is <see langword="null"/>.</exception>
        public static string FormatName(this IProgrammingLanguage language, IMember member, NameQualifier qualifier)
        {
            if (language is null)
                throw new ArgumentNullException(nameof(language));
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            using var writer = StringBuilderPool.Shared.GetWriter();
            language.WriteName(writer, member, qualifier);
            return writer.ToString();
        }

        /// <summary>
        /// Formats the specified custom attribute according to syntax rules of the language.
        /// </summary>
        /// <param name="language">The <see cref="IProgrammingLanguage"/> instance.</param>
        /// <param name="attribute">The <see cref="ICustomAttribute"/> representing the attribute to be formatted.</param>
        /// <param name="qualifier">The level of qualification to apply to the attribute's name.</param>
        /// <returns>The representation of the specified attribute according to syntax rules of the language.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="language"/> or <paramref name="attribute"/> is <see langword="null"/>.</exception>
        public static string FormatAttribute(this IProgrammingLanguage language, ICustomAttribute attribute, NameQualifier qualifier)
        {
            if (language is null)
                throw new ArgumentNullException(nameof(language));
            if (attribute is null)
                throw new ArgumentNullException(nameof(attribute));

            using var writer = StringBuilderPool.Shared.GetWriter();
            language.WriteAttribute(writer, attribute, qualifier);
            return writer.ToString();
        }

        /// <summary>
        /// Formats the given literal value according to syntax rules of the language.
        /// </summary>
        /// <param name="language">The <see cref="IProgrammingLanguage"/> instance.</param>
        /// <param name="value">The value to be formatted as a literal.</param>
        /// <returns>The representation of the specified value according to syntax rules of the language.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="language"/> is <see langword="null"/>.</exception>
        public static string FormatLiteral(this IProgrammingLanguage language, object? value)
        {
            if (language is null)
                throw new ArgumentNullException(nameof(language));

            using var writer = StringBuilderPool.Shared.GetWriter();
            language.WriteConstantValue(writer, value);
            return writer.ToString();
        }

        /// <summary>
        /// Formats the given code reference (cref) according to syntax rules of the language.
        /// </summary>
        /// <param name="language">The <see cref="IProgrammingLanguage"/> instance.</param>
        /// <param name="cref">The code reference string to format.</param>
        /// <param name="qualifierSelector">An optional function to determine the level of qualification for the member's name; if not provided, defaults to <see cref="NameQualifier.DeclaringType"/>.</param>
        /// <returns>A formatted representation of the code reference according to syntax rules of the language if it can be resolved; otherwise, the original <paramref name="cref"/> string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="language"/> or <paramref name="cref"/> is <see langword="null"/>.</exception>
        public static string FormatCodeReference(this IProgrammingLanguage language, string cref, Func<IMember, NameQualifier>? qualifierSelector = null)
        {
            if (language is null)
                throw new ArgumentNullException(nameof(language));
            if (cref is null)
                throw new ArgumentNullException(nameof(cref));

            if (CodeReference.IsNamespace(cref))
                return cref[2..];

            if (CodeReference.ResolveMember(cref) is IMember member)
                return language.FormatSignature(member, qualifierSelector is not null ? qualifierSelector(member) : NameQualifier.DeclaringType);

            return cref;
        }
    }
}

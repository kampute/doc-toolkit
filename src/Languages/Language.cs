// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Languages
{
    using System;

    /// <summary>
    /// Provides access to the default programming language used for formatting code elements.
    /// </summary>
    public static class Language
    {
        private static IProgrammingLanguage defaultLanguage = new CSharp();

        /// <summary>
        /// Gets or sets the default programming language used for formatting code elements.
        /// </summary>
        /// <value>
        /// The default <see cref="IProgrammingLanguage"/> instance.
        /// </value>
        /// <exception cref="ArgumentNullException">Thrown when attempting to set a <see langword="null"/> value.</exception>
        public static IProgrammingLanguage Default
        {
            get => defaultLanguage;
            set => defaultLanguage = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Determines whether the specified language is the default language.
        /// </summary>
        /// <param name="language">The programming language to check.</param>
        /// <returns><see langword="true"/> if the specified language is the default language; otherwise, <see langword="false"/>.</returns>
        public static bool IsDefault(IProgrammingLanguage language) => ReferenceEquals(language, defaultLanguage);

        /// <summary>
        /// Determines whether the specified identifier is valid.
        /// </summary>
        /// <param name="identifier">The identifier to check.</param>
        /// <returns><see langword="true"/> if the identifier is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return false;

            if (!char.IsLetter(identifier[0]) && identifier[0] is not '_')
                return false;

            for (var i = 1; i < identifier.Length; i++)
            {
                var c = identifier[i];
                if (!char.IsLetterOrDigit(c) && c is not '_')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified namespace name is valid.
        /// </summary>
        /// <param name="namespaceName">The namespace name to check.</param>
        /// <returns><see langword="true"/> if the namespace name is valid; otherwise, <see langword="false"/>.</returns>
        public static bool IsValidNamespace(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
                return false;

            foreach (var part in namespaceName.Split('.'))
            {
                if (!IsValidIdentifier(part))
                    return false;
            }

            return true;
        }
    }
}

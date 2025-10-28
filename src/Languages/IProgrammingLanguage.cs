// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Languages
{
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// Represents a .NET programming language.
    /// </summary>
    /// <remarks>
    /// The <see cref="IProgrammingLanguage"/> interface defines a contract for language-specific formatters
    /// that can render code elements according to the syntax rules of a particular programming language.
    /// This abstraction allows the documentation system to:
    /// <list type="bullet">
    ///   <item><description>Format code elements according to the syntax rules of the target language</description></item>
    ///   <item><description>Provide language-specific features like syntax highlighting identifiers and keyword documentation links</description></item>
    /// </list>
    /// Implementations of this interface encapsulate the rules and formatting conventions of specific languages (such as C#, F#, etc.),
    /// allowing the documentation system to produce language-appropriate representations of code elements without needing to know the
    /// details of each language's syntax.
    /// </remarks>
    public interface IProgrammingLanguage
    {
        /// <summary>
        /// Gets the name of the language.
        /// </summary>
        /// <value>A <see cref="string"/> representing the name of the language.</value>
        string Name { get; }

        /// <summary>
        /// Gets the identifier for signature highlighting of code written in this language.
        /// </summary>
        /// <value>A <see cref="string"/> representing the signature highlighting identifier.</value>
        string Identifier { get; }

        /// <summary>
        /// Attempts to retrieve the official documentation URL for the specified language keyword.
        /// </summary>
        /// <param name="keyword">The language keyword to look up.</param>
        /// <param name="url">
        /// When this method returns, contains the URL of the keyword documentation, if the keyword is found; otherwise, <see langword="null"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true"/> if the keyword is found; otherwise, <see langword="false"/>.</returns>
        bool TryGetUrl(string keyword, [NotNullWhen(true)] out Uri? url);

        /// <summary>
        /// Writes the definition of a specified member formatted according to syntax rules of the language to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the member definition is written.</param>
        /// <param name="member">The member whose definition is to be written.</param>
        /// <param name="linker">An optional delegate for linking to the documentation of a type or type's member.</param>
        void WriteDefinition(TextWriter writer, IMember member, MemberDocLinker? linker = null);

        /// <summary>
        /// Writes the signature of a member formatted according to the syntax rules of the language to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the member's signature is written.</param>
        /// <param name="member">The member whose signature is to be written.</param>
        /// <param name="qualification">The level of qualification to apply to the member's name.</param>
        /// <param name="linker">An optional delegate for linking to the documentation of a type or type's member.</param>
        void WriteSignature(TextWriter writer, IMember member, NameQualifier qualification, MemberDocLinker? linker = null);

        /// <summary>
        /// Writes the name of a member formatted according to the syntax rules of the language to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the member's signature is written.</param>
        /// <param name="member">The member whose signature is to be written.</param>
        /// <param name="qualification">The level of qualification to apply to the member's name.</param>
        /// <param name="linker">An optional delegate for linking to the documentation of a type or type's member.</param>
        void WriteName(TextWriter writer, IMember member, NameQualifier qualification, MemberDocLinker? linker = null);

        /// <summary>
        /// Writes the specified attribute formatted according to the syntax rules of the language to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the attribute to.</param>
        /// <param name="attribute">The <see cref="ICustomAttribute"/> representing the attribute to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the attribute's name.</param>
        /// <param name="linker">An optional delegate for linking to the documentation of a type or type's member.</param>
        void WriteAttribute(TextWriter writer, ICustomAttribute attribute, NameQualifier qualifier, MemberDocLinker? linker = null);

        /// <summary>
        /// Writes a constant value formatted according to the syntax rules of the language to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the literal value is written.</param>
        /// <param name="value">The value to be formatted as a literal and written.</param>
        /// <param name="valueType">An optional <see cref="IType"/> representing the type of the value.</param>
        /// <param name="linker">An optional delegate for linking to the documentation of a type or type's member.</param>
        void WriteConstantValue(TextWriter writer, object? value, IType? valueType = null, MemberDocLinker? linker = null);
    }
}
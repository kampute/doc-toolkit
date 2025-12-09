// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines a contract for providing parsed XML documentation for namespaces, types and type members.
    /// </summary>
    /// <seealso cref="XmlDocProvider"/>
    public interface IXmlDocProvider
    {
        /// <summary>
        /// Gets a value indicating whether the XML documentation provider has any documentation available.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the provider contains documentation; otherwise, <see langword="false"/>.
        /// </value>
        bool HasDocumentation { get; }

        /// <summary>
        /// Attempts to retrieves the XML documentation for the specified code reference.
        /// </summary>
        /// <param name="cref">The code reference to retrieve the documentation for.</param>
        /// <param name="doc">
        /// When this method returns, contains the <see cref="XmlDocEntry"/> representing the documentation for the code,
        /// reference if the code reference is valid and documentation is found; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if the documentation is found; otherwise, <see langword="false"/>.</returns>
        bool TryGetDoc(string cref, [NotNullWhen(true)] out XmlDocEntry? doc);
    }
}

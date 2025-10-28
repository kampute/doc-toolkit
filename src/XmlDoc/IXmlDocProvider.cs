// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines a contract for providing XML documentation for code references, types and type members.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IXmlDocProvider"/> interface defines the core functionality for retrieving documentation
    /// associated with types and members within an assembly.
    /// </para>
    /// Implementations of this interface are responsible for:
    /// <list type="bullet">
    ///   <item><description>Loading and parsing XML documentation from various sources</description></item>
    ///   <item><description>Resolving documentation requests by code reference or reflection member</description></item>
    ///   <item><description>Managing the internal storage and retrieval of documentation content</description></item>
    /// </list>
    /// The <see cref="XmlDocProvider"/> class is a concrete implementation of this interface that loads XML documentation
    /// from XML files generated during compilation.
    /// </remarks>
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
        /// When this method returns, contains the <see cref="XmlDocEntry"/> representing the documentation for the code reference,
        /// if the documentation is available; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if the documentation is available; otherwise, <see langword="false"/>.</returns>
        bool TryGetDoc(string cref, [NotNullWhen(true)] out XmlDocEntry? doc);
    }
}

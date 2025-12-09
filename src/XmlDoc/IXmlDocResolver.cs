// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    /// <summary>
    /// Defines a contract for providing XML documentation associated with code references.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IXmlDocErrorHandler"/> interface defines the core functionality for retrieving documentation
    /// associated with a code reference.
    /// </para>
    /// Implementations of this interface are responsible for:
    /// <list type="bullet">
    ///   <item><description>Loading and parsing XML documentation from various sources</description></item>
    ///   <item><description>Resolving documentation requests by code reference</description></item>
    ///   <item><description>Managing the internal storage and retrieval of documentation content</description></item>
    /// </list>
    /// </remarks>
    public interface IXmlDocResolver
    {
        /// <summary>
        /// Gets a value indicating whether the resolver contains any XML documentation.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the resolver has no XML documentation; otherwise, <see langword="false"/>.
        /// </value>
        bool HasDocumentation { get; }

        /// <summary>
        /// Attempts to retrieves the XML documentation for the specified code reference.
        /// </summary>
        /// <param name="cref">The code reference to retrieve the documentation for.</param>
        /// <param name="xmlDoc">
        /// When this method returns, contains the XML element representing the documentation for the code reference,
        /// if the documentation is available; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if the documentation is found; otherwise, <see langword="false"/>.</returns>
        bool TryGetXmlDoc(string cref, [NotNullWhen(true)] out XElement? xmlDoc);
    }
}

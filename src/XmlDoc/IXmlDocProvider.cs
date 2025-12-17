// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using Kampute.DocToolkit.Metadata;
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
        /// Attempts to retrieves the XML documentation for the specified namespace.
        /// </summary>
        /// <param name="ns">The namespace to retrieve the documentation for.</param>
        /// <param name="doc">
        /// When this method returns, contains the <see cref="XmlDocEntry"/> representing the documentation for the namespace,
        /// if the documentation is available; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if the documentation is available; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The XML documentation for a namespace is represented by a special type named "NamespaceDoc" within the namespace.
        /// </remarks>
        bool TryGetNamespaceDoc(string ns, [NotNullWhen(true)] out XmlDocEntry? doc);

        /// <summary>
        /// Attempts to retrieves the XML documentation for the specified member.
        /// </summary>
        /// <param name="member">The member to retrieve the documentation for.</param>
        /// <param name="doc">
        /// When this method returns, contains the <see cref="XmlDocEntry"/> representing the documentation for the member,
        /// if the documentation is available; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if the documentation is available; otherwise, <see langword="false"/>.</returns>
        bool TryGetMemberDoc(IMember member, [NotNullWhen(true)] out XmlDocEntry? doc);
    }
}

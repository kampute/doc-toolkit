// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.XPath;

    /// <summary>
    /// Provides XML documentation for .NET types and members by parsing and resolving documentation files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="XmlDocRepository"/> is a central component in the documentation generation system that manages the
    /// loading, parsing, and resolution of XML documentation files. It serves as a repository of documentation content
    /// that can be queried by code reference.
    /// </para>
    /// This repository handles several key documentation processing tasks:
    /// <list type="bullet">
    ///   <item><description>Loading XML documentation from files generated during compilation</description></item>
    ///   <item><description>Resolving external content referenced by <c>include</c> elements</description></item>
    ///   <item><description>Resolving inherited documentation via <c>inheritdoc</c> elements</description></item>
    ///   <item><description>Offering standardized access to documentation via code references</description></item>
    /// </list>
    /// The repository uses lazy resolution for <c>inheritdoc</c> elements, meaning these elements are only resolved
    /// when documentation is actually requested. This approach ensures that inherited documentation is available
    /// even when documentation is loaded in a non-optimal order.
    /// <para>
    /// When handling error conditions, the repository uses a configurable error handler that can be customized through
    /// the <see cref="ErrorHandler"/> property. The default behavior is to silently ignore errors during resolution,
    /// except for missing included files which will throw a <see cref="FileNotFoundException"/>.
    /// </para>
    /// </remarks>
    public class XmlDocRepository : IXmlDocResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocRepository"/> class.
        /// </summary>
        public XmlDocRepository()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the XML documentation repository has any documentation available.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the repository contains documentation; otherwise, <see langword="false"/>.
        /// </value>
        public bool HasDocumentation => Members.Count > 0;

        /// <summary>
        /// Gets or sets the error handler for the repository.
        /// </summary>
        /// <value>
        /// The error handler for the repository, or <see langword="null"/> for default behavior.
        /// </value>
        /// <remarks>
        /// The default error handling behavior is to ignore any errors that occur while resolving <c>inheritdoc</c> or <c>include</c> elements,
        /// except for missing included files, which throw a <see cref="FileNotFoundException"/>.
        /// </remarks>
        public IXmlDocErrorHandler? ErrorHandler { get; set; }

        /// <summary>
        /// Gets the dictionary of XML documentation elements.
        /// </summary>
        /// <value>
        /// A dictionary mapping code references to their XML documentation elements.
        /// </value>
        protected Dictionary<string, XElement> Members { get; } = new(StringComparer.Ordinal);

        /// <summary>
        /// Imports XML documentation from the specified documentation file, resolving any include directives.
        /// </summary>
        /// <param name="xmlDocPath">The absolute or relative path to the XML documentation file.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="xmlDocPath"/> is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the XML documentation file does not exist.</exception>
        /// <exception cref="FileNotFoundException">Thrown when an included file does not exist and no error handler is provided.</exception>
        /// <remarks>
        /// This method first resolves any include directives in the XML document using the file's directory as the base path,
        /// then imports the member documentation. If the XML document contains members that already exist in the repository,
        /// the new documentation replaces the existing documentation.
        /// </remarks>
        public virtual void ImportFile(string xmlDocPath)
        {
            if (string.IsNullOrEmpty(xmlDocPath))
                throw new ArgumentException($"'{nameof(xmlDocPath)}' cannot be null or empty.", nameof(xmlDocPath));
            if (!File.Exists(xmlDocPath))
                throw new FileNotFoundException("The XML documentation file does not exist.", xmlDocPath);

            var document = XDocument.Load(xmlDocPath);
            var directory = Path.GetDirectoryName(Path.GetFullPath(xmlDocPath));

            ResolveIncludes(document, directory!);
            Import(document);
        }

        /// <summary>
        /// Imports XML documentation of members from the specified XML document.
        /// </summary>
        /// <param name="document">The XML document to import.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method adds the XML documentation of members from the provided XML document to the repository's collection.
        /// If the XML document contains members with the same name as existing members, the new members replace the existing ones.
        /// <note type="tip" title="Tip">
        /// This method does not resolve any <c>include</c> elements in the XML document. To resolve <c>include</c> elements, use
        /// the <see cref="ResolveIncludes(XDocument, string)"/> method before the import.
        /// </note>
        /// </remarks>
        /// <seealso cref="ResolveIncludes(XDocument, string)"/>
        public virtual void Import(XDocument document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            foreach (var element in document.Descendants("member"))
            {
                if (element.TryGetAttributeValue("name", out var name))
                    Members[name] = element;
            }
        }

        /// <summary>
        /// Resolves <c>include</c> elements in the specified XML document by importing content from external files.
        /// </summary>
        /// <param name="document">The XML document to resolve its <c>include</c> elements.</param>
        /// <param name="includeDir">The directory to search for the included files.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="document"/> or <paramref name="includeDir"/> is <see langword="null"/>.</exception>
        /// <exception cref="FileNotFoundException">Thrown when an included file is not found and no error handler is provided.</exception>
        public virtual void ResolveIncludes(XDocument document, string includeDir)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));
            if (includeDir is null)
                throw new ArgumentNullException(nameof(includeDir));

            var includeElements = document.Descendants("include").ToList();
            if (includeElements.Count == 0)
                return;

            var loadedDocuments = new Dictionary<string, XDocument?>(StringComparer.Ordinal);

            foreach (var includeElement in includeElements)
            {
                if (!includeElement.TryGetAttributeValue("file", out var includeFilePath))
                    continue;

                if (!includeElement.TryGetAttributeValue("path", out var includeXPath))
                    continue;

                if (!loadedDocuments.TryGetValue(includeFilePath, out var includedDocument))
                {
                    var includeFileFullPath = Path.Combine(includeDir, includeFilePath);
                    if (File.Exists(includeFileFullPath))
                        includedDocument = XDocument.Load(includeFileFullPath);
                    else if (ErrorHandler is null)
                        throw new FileNotFoundException("The included file does not exist.", includeFileFullPath);

                    loadedDocuments[includeFilePath] = includedDocument;
                }

                if (includedDocument is null)
                {
                    ErrorHandler?.IncludeFileNotFound(includeElement.Parent!, includeFilePath);
                    continue;
                }

                var element = includedDocument.XPathSelectElement(includeXPath);
                if (element is null)
                {
                    ErrorHandler?.IncludeMemberNotFound(includeElement.Parent!);
                    continue;
                }

                includeElement.ReplaceWith(element);
            }
        }

        /// <summary>
        /// Attempts to retrieves the XML documentation element for the specified code reference.
        /// </summary>
        /// <param name="cref">The code reference to retrieve the documentation for.</param>
        /// <param name="xmlDoc">
        /// When this method returns, contains the <see cref="XElement"/> that represents the documentation for the code reference,
        /// if the documentation is available; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true"/> if the documentation is available; otherwise, <see langword="false"/>.</returns>
        public virtual bool TryGetXmlDoc(string cref, [NotNullWhen(true)] out XElement? xmlDoc)
        {
            if (!string.IsNullOrWhiteSpace(cref) && Members.TryGetValue(cref, out var element))
            {
                if (element.Element("inheritdoc") is not null && !ResolveInheritDoc(element))
                    ErrorHandler?.InheritDocNotFound(element);

                xmlDoc = element;
                return true;
            }

            xmlDoc = null;
            return false;
        }

        /// <summary>
        /// Resolves the <c>inheritdoc</c> elements in the specified XML element.
        /// </summary>
        /// <param name="element">The element to resolve its <c>inheritdoc</c> element.</param>
        /// <returns><see langword="true"/> if the <c>inheritdoc</c> element was resolved; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method attempts to resolve each <c>inheritdoc</c> element by finding the base member's documentation and merging it
        /// into the current element.
        /// <para>
        /// If an <c>inheritdoc</c> element has a <c>cref</c> or <c>path</c> attribute, it uses that to find the base member. If not, it attempts to infer
        /// the inherited member from the current element's <c>name</c> attribute. If the base member's documentation is found, its child elements are copied
        /// into the current element, and the <c>inheritdoc</c> element is removed.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> is <see langword="null"/>.</exception>
        protected virtual bool ResolveInheritDoc(XElement element)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));

            var resolved = false;
            var foundElements = new HashSet<XElement>();
            foreach (var inheritDoc in element.Elements("inheritdoc").ToList())
            {
                inheritDoc.Remove();
                if (TryResolveInheritedElement(element, inheritDoc, out var inheritedElement) && foundElements.Add(inheritedElement))
                {
                    foreach (var comment in inheritedElement.Elements())
                        element.Add(new XElement(comment));

                    resolved = true;
                }
            }
            return resolved;
        }

        /// <summary>
        /// Attempts to resolve the inherited element for the specified <c>inheritdoc</c> element.
        /// </summary>
        /// <param name="element">The element to merge the inherited documentation into.</param>
        /// <param name="inheritDoc">The <c>inheritdoc</c> element to resolve.</param>
        /// <param name="inheritedElement">When this method returns, contains the inherited element, if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the inherited element was found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> or <paramref name="inheritDoc"/> is <see langword="null"/>.</exception>
        protected virtual bool TryResolveInheritedElement(XElement element, XElement inheritDoc, [NotNullWhen(true)] out XElement? inheritedElement)
        {
            if (element is null)
                throw new ArgumentNullException(nameof(element));
            if (inheritDoc is null)
                throw new ArgumentNullException(nameof(inheritDoc));

            if (inheritDoc.TryGetAttributeValue("cref", out var cref))
                return TryGetXmlDoc(cref, out inheritedElement);

            if (inheritDoc.TryGetAttributeValue("path", out var expression))
            {
                inheritedElement = element.Document?.XPathSelectElement(expression);
                if (inheritedElement is null)
                    return false;
                if (inheritedElement.Name != "member")
                    inheritedElement = inheritedElement.Parent!;
                return true;
            }

            if (element.TryGetAttributeValue("name", out cref) && CodeReference.ResolveMember(cref) is IMember member)
            {
                var baseMember = member.GetInheritedMember()?.GetMemberDefinition();
                if (baseMember is not null && TryGetXmlDoc(baseMember.CodeReference, out inheritedElement))
                    return true;
            }

            inheritedElement = null;
            return false;
        }
    }
}

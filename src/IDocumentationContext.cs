// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.XmlDoc;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for providing the context for the documentation generation process.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IDocumentationContext"/> interface represents the central hub of the documentation generation system, binding together
    /// all the necessary components for creating complete API documentation. It serves as a repository of information about the assemblies,
    /// types, and members being documented, while also providing access to formatting, addressing, and content extraction services.
    /// </para>
    /// A documentation context typically contains:
    /// <list type="bullet">
    ///   <item><description>The assemblies being documented and their contained types and members</description></item>
    ///   <item><description>The topics that provide narrative content for the documentation</description></item>
    ///   <item><description>The strategy for organizing documentation pages</description></item>
    ///   <item><description>The formatter for transforming XML documentation comments into readable content</description></item>
    ///   <item><description>The content provider for extracting XML documentation comments from code elements</description></item>
    ///   <item><description>The address provider for resolving URLs and file paths for documentation elements</description></item>
    ///   <item><description>The URL translator provider for replacing URLs in documentation content, particularly for cross-references in topics</description></item>
    /// </list>
    /// Documentation generators use this context to access all the information needed to create a complete set of documentation
    /// pages, ensuring consistency across the entire documentation set. Extensions can be built on top of this interface to
    /// provide specialized functionality like documentation validation, search index generation, or cross-reference resolution.
    /// </remarks>
    public interface IDocumentationContext : IDisposable
    {
        /// <summary>
        /// Gets the assemblies for which documentation will be generated.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="AssemblyModel"/> representing the assemblies to generate documentation for.
        /// </value>
        IReadOnlyCollection<AssemblyModel> Assemblies { get; }

        /// <summary>
        /// Gets all namespaces in the assemblies being documented.
        /// </summary>
        /// <value>
        /// The read-only collection of <see cref="NamespaceModel"/> objects representing all namespaces in the assemblies being documented.
        /// </value>
        IReadOnlyCollection<NamespaceModel> Namespaces { get; }

        /// <summary>
        /// Gets all exported types in the assemblies being documented.
        /// </summary>
        /// <value>
        /// The read-only collection of <see cref="TypeModel"/> objects representing all exported types in the assemblies being documented,
        /// including their nested types.
        /// </value>
        IReadOnlyTypeCollection Types { get; }

        /// <summary>
        /// Gets all top-level topics in the documentation.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="TopicModel"/> objects representing the top-level topics to incorporate into the
        /// documentation. The subtopics can be accessed via the <see cref="TopicModel.Subtopics"/> property.
        /// </value>
        IReadOnlyTopicCollection Topics { get; }

        /// <summary>
        /// Gets the programming language of the codebase being documented.
        /// </summary>
        /// <value>
        /// The programming language of the codebase being documented.
        /// </value>
        IProgrammingLanguage Language { get; }

        /// <summary>
        /// Gets the object responsible for formatting the documentation content.
        /// </summary>
        /// <value>
        /// The object responsible for formatting the documentation content.
        /// </value>
        IDocumentFormatter ContentFormatter { get; }

        /// <summary>
        /// Gets the provider responsible for finding documentation content of elements.
        /// </summary>
        /// <value>
        /// The provider for finding documentation content of elements.
        /// </value>
        IXmlDocProvider ContentProvider { get; }

        /// <summary>
        /// Gets the provider responsible for resolving documentation URLs and file paths of elements.
        /// </summary>
        /// <value>
        /// The provider for resolving documentation URLs and file paths of elements.
        /// </value>
        IDocumentAddressProvider AddressProvider { get; }

        /// <summary>
        /// Gets the object responsible for transforming non-API site-root-relative URLs to an absolute or document-relative URL.
        /// </summary>
        /// <value>
        /// The object responsible for transforming non-API site-root-relative URLs to an absolute or document-relative URL.
        /// </value>
        IUrlTransformer UrlTransformer { get; }
    }
}
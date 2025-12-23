// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Topics;
    using Kampute.DocToolkit.XmlDoc;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides the context for generating documentation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DocumentationContext"/> class serves as the central repository for all information needed to generate
    /// API documentation. It implements the <see cref="IDocumentationContext"/> interface to provide a unified way to access
    /// assemblies, their types and members, formatted documentation content, and documentation addressing schemes.
    /// </para>
    /// This class combines several key components of the documentation system:
    /// <list type="bullet">
    ///   <item><description>Assembly and type metadata through managed assemblies</description></item>
    ///   <item><description>XML documentation content from compiler-generated XML files</description></item>
    ///   <item><description>Formatting services for transforming XML content into readable formats (e.g. HTML, Markdown, etc.)</description></item>
    ///   <item><description>Addressing strategies for organizing documentation into files and URLs</description></item>
    ///   <item><description>External reference resolvers for linking to third-party documentation</description></item>
    ///   <item><description>Programming language services for syntax-specific formatting</description></item>
    ///   <item><description>Topic management for narrative content in the documentation</description></item>
    /// </list>
    /// </remarks>
    /// <see cref="FileSystemDocumentationComposer"/>
    public class DocumentationContext : IDocumentationContext
    {
        private readonly Lazy<IReadOnlyCollection<NamespaceModel>> namespaces;
        private readonly Lazy<IReadOnlyTypeCollection> types;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentationContext"/> class.
        /// </summary>
        /// <param name="language">The programming language of the codebase being documented.</param>
        /// <param name="addressProvider">The provider responsible for resolving documentation URLs and file paths of elements.</param>
        /// <param name="contentProvider">The provider responsible for finding documentation content of elements.</param>
        /// <param name="contentFormatter">The object responsible for formatting the documentation content.</param>
        /// <param name="assemblies">The assemblies that contain the types and members to document.</param>
        /// <param name="topics">The top-level topics to incorporate into the documentation, including their subtopics.</param>
        /// <param name="metadata">Optional metadata to associate with the documentation context.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="topics"/> contains duplicate topic names.</exception>
        /// <remarks>
        /// The created <see cref="DocumentationContext"/> instance takes ownership of the provided <paramref name="contentFormatter"/>,
        /// <paramref name="contentProvider"/>, <paramref name="addressProvider"/>, and <paramref name="metadata"/> objects. If any of these
        /// objects implement <see cref="IDisposable"/>, they will be disposed of when the <see cref="DocumentationContext"/> is disposed.
        /// </remarks>
        public DocumentationContext
        (
            IProgrammingLanguage language,
            IDocumentAddressProvider addressProvider,
            IXmlDocProvider contentProvider,
            IDocumentFormatter contentFormatter,
            IEnumerable<IAssembly> assemblies,
            IEnumerable<ITopic> topics,
            object? metadata = null
        )
        {
            Metadata = metadata;
            Language = language ?? throw new ArgumentNullException(nameof(language));
            AddressProvider = addressProvider ?? throw new ArgumentNullException(nameof(addressProvider));
            ContentProvider = contentProvider ?? throw new ArgumentNullException(nameof(contentProvider));
            ContentFormatter = contentFormatter ?? throw new ArgumentNullException(nameof(contentFormatter));

            Assemblies = assemblies is not null
                ? [.. assemblies.Select(assembly => new AssemblyModel(this, assembly))]
                : throw new ArgumentNullException(nameof(assemblies));

            Topics = topics is not null
                ? new TopicCollection(this, (_, topic) => ToScopedTopic(topic), topics)
                : throw new ArgumentNullException(nameof(topics));

            UrlTransformer = CreateUrlTransformer();

            namespaces = new(GetAllNamespaces);
            types = new(GetAllTypes);

            if (contentFormatter is IXmlDocReferenceAccessor accessor && accessor.ReferenceResolver is null)
                accessor.ReferenceResolver = new XmlDocContextAwareReferenceResolver(this);
        }

        /// <summary>
        /// Gets the programming language of the codebase being documented.
        /// </summary>
        /// <value>
        /// The programming language of the codebase being documented.
        /// </value>
        public IProgrammingLanguage Language { get; }

        /// <summary>
        /// Gets the object responsible for formatting the documentation content.
        /// </summary>
        /// <value>
        /// The object responsible for formatting the documentation content.
        /// </value>
        public IDocumentFormatter ContentFormatter { get; }

        /// <summary>
        /// Gets the provider responsible for finding documentation content of elements.
        /// </summary>
        /// <value>
        /// The provider for finding documentation content of elements.
        /// </value>
        public IXmlDocProvider ContentProvider { get; }

        /// <summary>
        /// Gets the provider responsible for resolving documentation URLs and file paths of elements.
        /// </summary>
        /// <value>
        /// The provider for resolving documentation URLs and file paths of elements.
        /// </value>
        public IDocumentAddressProvider AddressProvider { get; }

        /// <summary>
        /// Gets the object responsible for transforming non-API site-root-relative URLs to an absolute or document-relative URL.
        /// </summary>
        /// <value>
        /// The object responsible for transforming non-API site-root-relative URLs to an absolute or document-relative URL.
        /// </value>
        public IUrlTransformer UrlTransformer { get; }

        /// <summary>
        /// Gets the assemblies to generate documentation for.
        /// </summary>
        /// <value>
        /// The read-only collection of the assemblies being documented.
        /// </value>
        public IReadOnlyCollection<AssemblyModel> Assemblies { get; }

        /// <summary>
        /// Gets all namespaces in the assemblies being documented.
        /// </summary>
        /// <value>
        /// The read-only collection of all namespaces in the assemblies being documented.
        /// The namespaces in the collection are ordered by their names.
        /// </value>
        public IReadOnlyCollection<NamespaceModel> Namespaces => namespaces.Value;

        /// <summary>
        /// Gets all exported types in the assemblies being documented.
        /// </summary>
        /// <value>
        /// The read-only collection of all exported types in the assemblies being documented.
        /// The types in the collection are ordered by their full names and categorized by their kinds.
        /// </value>
        public IReadOnlyTypeCollection Types => types.Value;

        /// <summary>
        /// Gets the topics in the documentation context.
        /// </summary>
        /// <value>
        /// The read-only collection of the topics in the documentation context.
        /// </value>
        public IReadOnlyTopicCollection Topics { get; }

        /// <summary>
        /// Gets the metadata associated with the documentation context.
        /// </summary>
        /// <value>
        /// The optional metadata associated with the documentation context.
        /// </value>
        protected object? Metadata { get; }

        /// <summary>
        /// Disposes of the resources used by the <see cref="DocumentationContext"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the resources used by the <see cref="DocumentationContext"/> class.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is being called from the <see cref="Dispose()"/> method or the finalizer.</param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                (ContentFormatter as IDisposable)?.Dispose();
                (ContentProvider as IDisposable)?.Dispose();
                (AddressProvider as IDisposable)?.Dispose();
                (Metadata as IDisposable)?.Dispose();
            }
        }

        /// <summary>
        /// Creates a contextual reference to the specified topic.
        /// </summary>
        /// <param name="topic">The topic to scope.</param>
        /// <returns>A <see cref="TopicModel"/> representing the scoped topic.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="topic"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method associates a topic with the context, creating a <see cref="TopicModel"/> that encapsulates both.
        /// This allows topics to be rendered in a context-aware manner.
        /// <para>
        /// Override this method in derived classes to customize how topics are scoped to contexts, for example, to apply additional
        /// properties or metadata to the topic based on the context.
        /// </para>
        /// </remarks>
        protected virtual TopicModel ToScopedTopic(ITopic topic) => new(this, topic ?? throw new ArgumentNullException(nameof(topic)));

        /// <summary>
        /// Creates the URL transformer for the documentation context.
        /// </summary>
        /// <returns>The URL transformer to use for transforming non-API URLs in the documentation.</returns>
        /// <remarks>
        /// This method creates an implementation of <see cref="IUrlTransformer"/> for transforming non-API
        /// site-root-relative URLs to absolute or document-relative URLs.
        /// <para>
        /// The default implementation returns an instance of the <see cref="ContextAwareUrlTransformer"/> class.
        /// Override this method in derived classes to provide a custom URL transformer implementation if needed.
        /// </para>
        /// </remarks>
        protected virtual IUrlTransformer CreateUrlTransformer() => new ContextAwareUrlTransformer(this);

        /// <summary>
        /// Retrieves all unique namespaces with exported types from the assemblies in the documentation context.
        /// </summary>
        /// <returns>A read-only collection of unique namespaces in the context.</returns>
        private IReadOnlyCollection<NamespaceModel> GetAllNamespaces() => Assemblies.Count switch
        {
            0 => [],
            1 => [.. Assemblies.First().Namespaces.Values],
            _ => [.. NamespaceModel.MergeDuplicates(Assemblies.SelectMany(static asm => asm.Namespaces.Values)).OrderBy(static ns => ns.Name, StringComparer.Ordinal)]
        };

        /// <summary>
        /// Retrieves all exported types from the assemblies in the documentation context.
        /// </summary>
        /// <returns>A collection of all exported types in the context.</returns>
        private IReadOnlyTypeCollection GetAllTypes() => Assemblies.Count switch
        {
            0 => TypeCollection.Empty,
            1 => Assemblies.First().ExportedTypes,
            _ => new TypeCollection(Assemblies.SelectMany(static asm => asm.ExportedTypes).OrderBy(static type => type.Name, StringComparer.Ordinal))
        };
    }
}

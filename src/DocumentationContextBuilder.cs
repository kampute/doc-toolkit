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
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Topics;
    using Kampute.DocToolkit.XmlDoc;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Represents an abstract builder for creating documentation contexts, where the API documentation is provided
    /// by XML documentation files.
    /// </summary>
    /// <typeparam name="TContext">The type of documentation context to build.</typeparam>
    /// <remarks>
    /// This class provides a fluent interface for configuring and building documentation contexts. It supports chaining
    /// configuration methods to set up assemblies, XML documentation files, topics, and various options before creating
    /// the final context.
    /// </remarks>
    public abstract class DocumentationContextBuilder<TContext>
        where TContext : IDocumentationContext
    {
        private readonly HashSet<IRemoteApiDocUrlResolver> externalReferences = [];
        private readonly Dictionary<string, string> assemblyPaths = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> xmlDocPaths = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ITopic> topics = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentationContextBuilder"/> class.
        /// </summary>
        protected DocumentationContextBuilder()
        {
        }

        /// <summary>
        /// Gets the document addressing strategy.
        /// </summary>
        /// <value>
        /// The addressing strategy implementation.
        /// </value>
        public abstract IDocumentAddressingStrategy Strategy { get; }

        /// <summary>
        /// Gets or sets the base URL for documentation links.
        /// </summary>
        /// <value>The base URL as a <see cref="Uri"/>, or <see langword="null"/> if not specified.</value>
        public Uri? BaseUrl { get; protected set; }

        /// <summary>
        /// Gets the programming language for syntax formatting.
        /// </summary>
        /// <value>
        /// The programming language implementation, or <see langword="null"/> if not specified.
        /// </value>
        public IProgrammingLanguage? Language { get; protected set; }

        /// <summary>
        /// Gets the XML documentation error handler.
        /// </summary>
        /// <value>
        /// The error handler implementation, or <see langword="null"/> if not specified.
        /// </value>
        public IXmlDocErrorHandler? XmlDocErrorHandler { get; protected set; }

        /// <summary>
        /// Gets the collection of API documentation resolvers for external references.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="IApiDocUrlProvider"/> implementations.
        /// </value>
        public IReadOnlyCollection<IRemoteApiDocUrlResolver> ExternalReferences => externalReferences;

        /// <summary>
        /// Gets the collection of assembly files to document.
        /// </summary>
        /// <value>
        /// A read-only collection of assembly file paths that should be documented.
        /// </value>
        public IReadOnlyCollection<string> AssemblyPaths => assemblyPaths.Values;

        /// <summary>
        /// Gets the collection of XML documentation file paths.
        /// </summary>
        /// <value>
        /// A read-only collection of file paths to XML documentation files.
        /// </value>
        public IReadOnlyCollection<string> XmlDocPaths => xmlDocPaths;

        /// <summary>
        /// Gets the collection of top-level topics to include in the documentation.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="ITopic"/> objects representing conceptual content.
        /// </value>
        public IReadOnlyCollection<ITopic> Topics => topics.Values;

        /// <summary>
        /// Sets the programming language for syntax formatting.
        /// </summary>
        /// <param name="language">The programming language to use.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="language"/> is <see langword="null"/>.</exception>
        public virtual DocumentationContextBuilder<TContext> WithLanguage(IProgrammingLanguage language)
        {
            Language = language ?? throw new ArgumentNullException(nameof(language));
            return this;
        }

        /// <summary>
        /// Sets the XML documentation error handler.
        /// </summary>
        /// <param name="errorHandler">The error handler to use for XML documentation processing.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="errorHandler"/> is <see langword="null"/>.</exception>
        public virtual DocumentationContextBuilder<TContext> WithXmlDocErrorHandler(IXmlDocErrorHandler errorHandler)
        {
            XmlDocErrorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            return this;
        }

        /// <summary>
        /// Sets the base URL for documentation links.
        /// </summary>
        /// <param name="baseUrl">The base URL as a <see cref="string"/>.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="baseUrl"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <exception cref="UriFormatException">Thrown when <paramref name="baseUrl"/> is not a valid absolute URI.</exception>
        public virtual DocumentationContextBuilder<TContext> WithBaseUrl(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException($"'{nameof(baseUrl)}' cannot be null or empty.", nameof(baseUrl));

            return WithBaseUrl(new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + '/', UriKind.Absolute));
        }

        /// <summary>
        /// Sets the base URL for documentation links.
        /// </summary>
        /// <param name="baseUrl">The base URL as a <see cref="Uri"/>.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseUrl"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="baseUrl"/> is not an absolute URI.</exception>
        public virtual DocumentationContextBuilder<TContext> WithBaseUrl(Uri baseUrl)
        {
            if (baseUrl is null)
                throw new ArgumentNullException(nameof(baseUrl));
            if (!baseUrl.IsAbsoluteUri)
                throw new ArgumentException($"'{nameof(baseUrl)}' must be an absolute URI.", nameof(baseUrl));

            BaseUrl = baseUrl;
            return this;
        }

        /// <summary>
        /// Adds an assembly file to the documentation if it has not been added before.
        /// </summary>
        /// <param name="assemblyPath">The path to the assembly file.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="assemblyPath"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the assembly file cannot be found.</exception>
        public virtual DocumentationContextBuilder<TContext> AddAssembly(string assemblyPath)
        {
            if (string.IsNullOrEmpty(assemblyPath))
                throw new ArgumentException($"'{nameof(assemblyPath)}' cannot be null or empty.", nameof(assemblyPath));

            var fileName = Path.GetFileName(assemblyPath);
            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException($"Assembly file not found: {assemblyPath}", assemblyPath);

            assemblyPaths.TryAdd(fileName, assemblyPath);
            return this;
        }

        /// <summary>
        /// Adds an XML documentation file to the documentation.
        /// </summary>
        /// <param name="xmlDocPath">The path to the XML documentation file.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="xmlDocPath"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the XML documentation file cannot be found.</exception>
        public virtual DocumentationContextBuilder<TContext> AddXmlDoc(string xmlDocPath)
        {
            if (string.IsNullOrEmpty(xmlDocPath))
                throw new ArgumentException($"'{nameof(xmlDocPath)}' cannot be null or empty.", nameof(xmlDocPath));

            if (!File.Exists(xmlDocPath))
                throw new FileNotFoundException($"XML documentation file not found: {xmlDocPath}", xmlDocPath);

            xmlDocPaths.Add(xmlDocPath);
            return this;
        }

        /// <summary>
        /// Adds a top-level topic to the documentation using the specified topic ID and file path.
        /// </summary>
        /// <param name="topicId"> The unique identifier of the topic.</param>
        /// <param name="topicPath">The path to the topic file.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="topicId"/> or <paramref name="topicPath"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="topicId"/> is already used by another topic.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the topic file cannot be found.</exception>
        public virtual DocumentationContextBuilder<TContext> AddTopic(string topicId, string topicPath)
        {
            if (string.IsNullOrWhiteSpace(topicId))
                throw new ArgumentException($"'{nameof(topicId)}' cannot be null or whitespace.", nameof(topicId));
            if (string.IsNullOrWhiteSpace(topicPath))
                throw new ArgumentException($"'{nameof(topicPath)}' cannot be null or whitespace.", nameof(topicPath));

            if (!File.Exists(topicPath))
                throw new FileNotFoundException($"Topic file not found: {topicPath}", topicPath);

            AddTopic(topicId, topicPath);
            return this;
        }

        /// <summary>
        /// Adds a top-level topic, including its subtopics, to the documentation.
        /// </summary>
        /// <param name="topic">The topic to add.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="topic"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="topic"/> is a subtopic.</exception>
        /// <exception cref="ArgumentException">Thrown when a topic with the same ID already exists.</exception>
        public virtual DocumentationContextBuilder<TContext> AddTopic(ITopic topic)
        {
            if (topic is null)
                throw new ArgumentNullException(nameof(topic));

            if (topic.ParentTopic is not null)
                throw new ArgumentException($"'{nameof(topic)}' must be a top-level topic.", nameof(topic));

            if (!topics.TryAdd(topic.Id, topic))
                throw new ArgumentException($"A topic with the ID '{topic.Id}' already exists.", nameof(topic));

            return this;
        }

        /// <summary>
        /// Adds an API documentation resolver for external references.
        /// </summary>
        /// <param name="resolver">The external reference resolver to add.</param>
        /// <returns>The current builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="resolver"/> is <see langword="null"/>.</exception>
        public virtual DocumentationContextBuilder<TContext> AddExternalReference(IRemoteApiDocUrlResolver resolver)
        {
            if (resolver is null)
                throw new ArgumentNullException(nameof(resolver));

            externalReferences.Add(resolver);
            return this;
        }

        /// <summary>
        /// Builds the documentation context with the configured settings.
        /// </summary>
        /// <returns>A new instance of <typeparamref name="TContext"/>.</returns>
        public virtual TContext Build()
        {
            var context = CreateContext();

            if (Strategy is IContextualAddressingStrategy contextAwareStrategy)
                contextAwareStrategy.InitializeWithContext(context);

            return context;
        }

        /// <summary>
        /// Creates a new instance of <typeparamref name="TContext"/> with the configured settings.
        /// </summary>
        /// <returns>A new instance of <typeparamref name="TContext"/> representing the documentation context.</returns>
        /// <remarks>
        /// This method is responsible for assembling all the components configured in the builder into a complete documentation
        /// context. The implementation must provide the logic to create the specific type of documentation context.
        /// </remarks>
        protected abstract TContext CreateContext();

        /// <summary>
        /// Creates the metadata universe based on the configured assembly paths.
        /// </summary>
        /// <returns>An instance of <see cref="MetadataUniverse"/> representing the metadata universe.</returns>
        /// <remarks>
        /// This method initializes the metadata universe with the directories of the specified assembly paths.
        /// It allows the universe to probe for assemblies and metadata in those directories, and load them as
        /// needed without executing any code in the assemblies.
        /// </remarks>
        protected virtual MetadataUniverse CreateMetadataUniverse()
        {
            var searchPaths = new HashSet<string>(assemblyPaths.Values.Select(Path.GetDirectoryName)!);
            return MetadataUniverse.FromProbeFolders(searchPaths);
        }

        /// <summary>
        /// Creates the XML documentation provider based on the configured settings.
        /// </summary>
        /// <returns>An instance of <see cref="IXmlDocProvider"/> representing the XML documentation provider.</returns>
        /// <remarks>
        /// This method initializes the XML documentation provider with the specified error handler and imports all configured
        /// XML documentation files into the provider.
        /// </remarks>
        protected virtual IXmlDocProvider CreateXmlDocProvider()
        {
            var repository = new XmlDocRepository
            {
                ErrorHandler = XmlDocErrorHandler
            };

            foreach (var xmlDocPath in XmlDocPaths)
                repository.ImportFile(xmlDocPath);

            return new XmlDocProvider(repository);
        }

        /// <summary>
        /// Creates the document address provider based on the configured settings.
        /// </summary>
        /// <param name="assemblies">The assemblies to be documented.</param>
        /// <returns>An instance of <see cref="IDocumentAddressProvider"/> representing the document address provider.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assemblies"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the addressing strategy is not configured.</exception>
        /// <remarks>
        /// This method initializes the document address provider with the specified addressing strategy and base URL. It also
        /// adds all configured external references to the provider. The type and type members that are not defined in the provided
        /// assemblies will be treated as external references.
        /// <para>
        /// If no reference to <see cref="MicrosoftDocs"/> is provided, it will add it by default to ensure that .NET API documentation
        /// links can be resolved.
        /// </para>
        /// </remarks>
        protected virtual IDocumentAddressProvider CreateAddressProvider(IEnumerable<IAssembly> assemblies)
        {
            if (assemblies is null)
                throw new ArgumentNullException(nameof(assemblies));
            if (Strategy is null)
                throw new InvalidOperationException("Addressing strategy must be configured.");

            var addressProvider = new DocumentAddressProvider(Strategy, assemblies, BaseUrl);

            if (!ExternalReferences.OfType<MicrosoftDocs>().Any())
                addressProvider.ExternalReferences.Add(new MicrosoftDocs());

            foreach (var resolver in ExternalReferences)
                addressProvider.ExternalReferences.Add(resolver);

            return addressProvider;
        }

        /// <summary>
        /// Creates the document formatter based on the configured settings.
        /// </summary>
        /// <returns>An instance of <see cref="IDocumentFormatter"/> representing the document formatter.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the addressing strategy is not configured.</exception>
        /// <exception cref="NotSupportedException">Thrown when no formatter is available for the specified configuration.</exception>
        /// <remarks>
        /// This method initializes the document formatter using the file extension specified by the addressing strategy.
        /// </remarks>
        protected virtual IDocumentFormatter CreateFormatter()
        {
            return DocFormatProvider.GetRequiredFormatterByExtension(Strategy.FileExtension);
        }

        /// <summary>
        /// Loads assemblies from the configured assembly paths.
        /// </summary>
        /// <param name="universe">The metadata universe to use for loading assemblies.</param>
        /// <returns>A set of <see cref="IAssembly"/> instances loaded from the specified paths.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="universe"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method loads assemblies from the paths specified in <see cref="AssemblyPaths"/>. If the same assembly is specified
        /// multiple times using different paths, it will be loaded and returned only once.
        /// </remarks>
        protected virtual HashSet<IAssembly> LoadAssembliesFromPaths(MetadataUniverse universe)
        {
            if (universe is null)
                throw new ArgumentNullException(nameof(universe));

            var assemblies = new HashSet<IAssembly>(ReferenceEqualityComparer<IAssembly>.Instance);

            foreach (var assemblyPath in AssemblyPaths)
            {
                var assembly = universe.LoadFromPath(assemblyPath);
                var assemblyMetadata = MetadataProvider.GetMetadata(assembly);
                assemblies.Add(assemblyMetadata);
            }

            return assemblies;
        }
    }

    /// <summary>
    /// Represents a builder for creating <see cref="DocumentationContext"/> instances.
    /// </summary>
    /// <remarks>
    /// This class provides a concrete implementation of the abstract builder pattern for creating
    /// documentation contexts. It validates the required configuration and assembles all components
    /// into a functional documentation context.
    /// </remarks>
    public class DocumentationContextBuilder : DocumentationContextBuilder<DocumentationContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentationContextBuilder"/> class.
        /// </summary>
        /// <param name="strategy">The addressing strategy used for creating documentation context.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is <see langword="null"/>.</exception>
        public DocumentationContextBuilder(IDocumentAddressingStrategy strategy)
        {
            Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        /// <summary>
        /// Gets the document addressing strategy.
        /// </summary>
        /// <value>
        /// The addressing strategy implementation.
        /// </value>
        public override IDocumentAddressingStrategy Strategy { get; }

        /// <summary>
        /// Creates a new instance of <see cref="DocumentationContext"/> with the configured settings.
        /// </summary>
        /// <returns>A new instance of <see cref="DocumentationContext"/> with the configured settings.</returns>
        protected override DocumentationContext CreateContext()
        {
            var universe = CreateMetadataUniverse();
            var assemblies = LoadAssembliesFromPaths(universe);
            var addressProvider = CreateAddressProvider(assemblies);
            var xmlProvider = CreateXmlDocProvider();
            var formatter = CreateFormatter();

            return new DocumentationContext
            (
                Language ?? Languages.Language.Default,
                addressProvider,
                xmlProvider,
                formatter,
                assemblies,
                Topics,
                universe
            );
        }

        /// <summary>
        /// Creates a new instance of <see cref="DocumentationContextBuilder"/> for building documentation context to
        /// generate Markdown pages for Azure DevOps Wiki.
        /// </summary>
        /// <param name="options">The options for Azure Wiki pages. If <see langword="null"/>, default options are used.</param>
        /// <returns>A <see cref="DocumentationContextBuilder"/> configured for Markdown pages with Azure DevOps Wiki addressing strategy.</returns>
        public static DocumentationContextBuilder DevOpsWiki(DevOpsWikiOptions? options = null) => new(new DevOpsWikiStrategy(options ?? new DevOpsWikiOptions()));

        /// <summary>
        /// Creates a new instance of <see cref="DocumentationContextBuilder"/> for building documentation context to
        /// generate HTML pages with structure matching DocFX conventions.
        /// </summary>
        /// <param name="options">The options for DocFX pages. If <see langword="null"/>, default options are used.</param>
        /// <returns>A <see cref="DocumentationContextBuilder"/> configured for HTML pages with DocFX addressing strategy.</returns>
        public static DocumentationContextBuilder DocFx(DocFxOptions? options = null) => new(new DocFxStrategy(options ?? new DocFxOptions()));

        /// <summary>
        /// Creates a new instance of <see cref="DocumentationContextBuilder"/> for building documentation context to
        /// generate HTML pages with structure matching .NET API browser conventions.
        /// </summary>
        /// <param name="options">The options for .NET API pages. If <see langword="null"/>, default options are used.</param>
        /// <returns>A <see cref="DocumentationContextBuilder"/> configured for HTML pages with .NET API addressing strategy.</returns>
        public static DocumentationContextBuilder DotNetApi(DotNetApiOptions? options = null) => new(new DotNetApiStrategy(options ?? new DotNetApiOptions()));
    }
}

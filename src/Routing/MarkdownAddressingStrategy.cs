// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides an abstract base class for Markdown-based addressing strategies that organize documentation pages as a hierarchy of
    /// pages named after their titles.
    /// </summary>
    /// <remarks>
    /// This class defines the page structure shared by Markdown publishing targets such as wikis, where the name of a page is derived
    /// from its title and the pages nested under a page are placed in a folder with the same name:
    /// <list type="bullet">
    ///   <item><description>Each namespace has a page named after the namespace.</description></item>
    ///   <item><description>Each type has a page named after the type, nested under the page of its namespace.</description></item>
    ///   <item><description>Type members are documented on the page of their declaring type, and are addressed by the anchor of their heading.</description></item>
    ///   <item><description>Each topic has a page named after its title, nested under the page of its parent topic.</description></item>
    /// </list>
    /// When <see cref="MarkdownAddressingOptions.MainTopicId"/> is set, all other pages are nested under the page of the main topic.
    /// When <see cref="MarkdownAddressingOptions.ApiTopicId"/> is set, the namespace pages are nested under the page of the API topic.
    /// <para>
    /// Derived classes define how a title becomes a page name by implementing <see cref="EncodePageName(string)"/>, and can override
    /// <see cref="EncodeFragmentIdentifier(string)"/> when the publishing target generates heading anchors differently from GitHub and
    /// GitLab. They can also override the methods that construct the page paths to change the page structure.
    /// </para>
    /// <note type="important" title="Important">
    /// A type member is addressed by the anchor of a heading whose text is the member signature formatted by <see cref="Language"/>
    /// with <see cref="NameQualifier.None"/>. Renderers must therefore write that heading for each member on the page of its declaring
    /// type, so that the generated links reach the member.
    /// </note>
    /// </remarks>
    /// <threadsafety static="true" instance="false">
    /// Public instance members can be considered thread-safe if neither <see cref="MarkdownAddressingOptions.MainTopicId"/> nor
    /// <see cref="MarkdownAddressingOptions.ApiTopicId"/> is set in the construction options of the instance. If either property
    /// is set, instance members are only thread-safe if the instance is fully initialized  (that is, <see cref="IContextualAddressingStrategy.InitializeWithContext(IDocumentationContext)"/>
    /// has been called)  before any other method is accessed, and there is no concurrent access during or before initialization.
    /// <para>
    /// The class is not thread-safe otherwise, because it contains a mutable context field and uses lazy initialization that depends
    /// on this field, without synchronization.
    /// </para>
    /// </threadsafety>
    public abstract class MarkdownAddressingStrategy : AddressingStrategy, ILanguageSpecific, IContextualAddressingStrategy
    {
        private IDocumentationContext? context;
        private readonly Lazy<string?> mainPath;
        private readonly Lazy<string?> apiPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownAddressingStrategy"/> class.
        /// </summary>
        /// <param name="options">The addressing options that configure how documentation pages are addressed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
        protected MarkdownAddressingStrategy(MarkdownAddressingOptions options)
            : base(options)
        {
            Language = options.Language;

            var mainTopicId = options.MainTopicId;
            mainPath = new Lazy<string?>(() => ResolveMainPath(mainTopicId));

            var apiTopicId = options.ApiTopicId;
            apiPath = new Lazy<string?>(() => ResolveApiPath(apiTopicId));
        }

        /// <inheritdoc/>
        public override PageGranularity Granularity => PageGranularity.NamespaceType;

        /// <summary>
        /// Gets the programming language formatter used for formatting member names.
        /// </summary>
        /// <value>
        /// The <see cref="IProgrammingLanguage"/> instance that defines the syntax rules for formatting member names.
        /// </value>
        public IProgrammingLanguage Language { get; }

        /// <summary>
        /// Gets the file path of the main documentation page.
        /// </summary>
        /// <value>
        /// The file path of the main documentation page, which also serves as the folder for all other pages.
        /// </value>
        protected string? MainPath => mainPath.Value;

        /// <summary>
        /// Gets the file path of the API documentation page.
        /// </summary>
        /// <value>
        /// The file path of the API documentation page, which also serves as the folder for all API reference pages.
        /// </value>
        protected string? ApiPath => apiPath.Value;

        /// <inheritdoc/>
        public override bool TryResolveNamespaceAddress(string ns, [NotNullWhen(true)] out IResourceAddress? address)
        {
            if (string.IsNullOrWhiteSpace(ns))
            {
                address = null;
                return false;
            }

            var path = GetNamespacePath(ns);
            address = CreateAddressFromPath(path);
            return true;
        }

        /// <inheritdoc/>
        public override bool TryResolveMemberAddress(IMember member, [NotNullWhen(true)] out IResourceAddress? address)
        {
            if (!IsAddressable(member))
            {
                address = null;
                return false;
            }

            if (member is IType type)
            {
                var typePath = GetTypePath(type);
                address = CreateAddressFromPath(typePath);
                return true;
            }

            var declaringTypePath = GetTypePath(member.DeclaringType!);
            var memberSignature = Language.FormatSignature(member, NameQualifier.None);
            address = CreateAddressFromPath(declaringTypePath, EncodeFragmentIdentifier(memberSignature));
            return true;
        }

        /// <inheritdoc/>
        public override bool TryResolveTopicAddress(ITopic topic, [NotNullWhen(true)] out IResourceAddress? address)
        {
            if (topic is null)
            {
                address = null;
                return false;
            }

            var path = GetTopicPath(topic);
            address = CreateAddressFromPath(path);
            return true;
        }

        /// <inheritdoc/>
        public override bool IsAddressable(IMember member) => base.IsAddressable(member) && member is not IVirtualTypeMember { IsExplicitInterfaceImplementation: true };

        /// <summary>
        /// Retrieves the relative documentation path for the specified type.
        /// </summary>
        /// <param name="type">The type for which to retrieve the path.</param>
        /// <returns>The relative documentation path for the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        protected virtual string GetTypePath(IType type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var typeName = Language.FormatSignature(type, NameQualifier.DeclaringType);
            var path = EncodePageName(typeName);

            if (!string.IsNullOrEmpty(type.Namespace))
                path = GetNamespacePath(type.Namespace) + '/' + path;
            else if (!string.IsNullOrEmpty(ApiPath))
                path = ApiPath + '/' + path;

            return path;
        }

        /// <summary>
        /// Retrieves the relative documentation path for the specified namespace.
        /// </summary>
        /// <param name="ns">The namespace name.</param>
        /// <returns>The relative documentation path for the specified namespace.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ns"/> is <see langword="null"/> or empty.</exception>
        protected virtual string GetNamespacePath(string ns)
        {
            if (string.IsNullOrEmpty(ns))
                throw new ArgumentException($"'{nameof(ns)}' cannot be null or empty.", nameof(ns));

            var path = EncodePageName(ns);

            if (!string.IsNullOrEmpty(ApiPath))
                path = ApiPath + '/' + path;

            return path;
        }

        /// <summary>
        /// Retrieves the relative documentation path for the specified topic.
        /// </summary>
        /// <param name="topic">The topic for which to retrieve the path.</param>
        /// <returns>The relative documentation path for the specified topic.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="topic"/> is <see langword="null"/>.</exception>
        protected virtual string GetTopicPath(ITopic topic)
        {
            if (topic is null)
                throw new ArgumentNullException(nameof(topic));

            var path = EncodePageName(topic.Title);

            if (topic.ParentTopic is not null)
                path = GetTopicPath(topic.ParentTopic) + '/' + path;
            else if (!string.IsNullOrEmpty(MainPath) && path != MainPath)
                path = MainPath + '/' + path;

            return path;
        }

        /// <summary>
        /// Converts the specified title to the name of a page, as it appears in the file path and URL of the page.
        /// </summary>
        /// <param name="title">The title of the page.</param>
        /// <returns>The page name for the specified title.</returns>
        /// <remarks>
        /// The page name is used as a single segment of a path, so it must not contain a directory separator.
        /// </remarks>
        protected abstract string EncodePageName(string title);

        /// <summary>
        /// Converts the text of a heading to the fragment identifier that addresses the heading.
        /// </summary>
        /// <param name="heading">The text of the heading.</param>
        /// <returns>The fragment identifier for the specified heading, without the leading '#'.</returns>
        /// <remarks>
        /// The default implementation generates the heading anchor the way GitHub and GitLab do, as described by
        /// <see cref="Markdown.ToHeadingAnchor(string)"/>.
        /// </remarks>
        protected virtual string EncodeFragmentIdentifier(string heading) => Markdown.ToHeadingAnchor(heading);

        /// <summary>
        /// Resolves the main topic path.
        /// </summary>
        /// <param name="mainTopicId">The identifier of the main topic.</param>
        /// <returns>The resolved main path or <see langword="null"/> if not found.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="mainTopicId"/> does not exist in the documentation context.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the addressing strategy is not bound to a documentation context.</exception>
        private string? ResolveMainPath(string mainTopicId)
        {
            if (string.IsNullOrEmpty(mainTopicId))
                return null;

            if (context is null)
                throw new InvalidOperationException("The addressing strategy is not yet bound to a documentation context.");

            if (!context.Topics.TryGetById(mainTopicId, out var mainTopic))
                throw new ArgumentException($"Could not find main topic '{mainTopicId}' in the documentation context.", nameof(mainTopicId));

            return EncodePageName(mainTopic.Name);
        }

        /// <summary>
        /// Resolves the API topic path.
        /// </summary>
        /// <param name="apiTopicId">The identifier of the API topic.</param>
        /// <returns>The resolved API path or <see langword="null"/> if not resolvable.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="apiTopicId"/> does not exist in the documentation context.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the addressing strategy is not bound to a documentation context.</exception>
        private string? ResolveApiPath(string apiTopicId)
        {
            if (string.IsNullOrEmpty(apiTopicId))
                return MainPath;

            if (context is null)
                throw new InvalidOperationException("The addressing strategy is not yet bound to a documentation context.");

            if (!context.Topics.TryGetById(apiTopicId, out var apiTopic))
                throw new ArgumentException($"Could not find API topic '{apiTopicId}' in the documentation context.", nameof(apiTopicId));

            var apiPath = EncodePageName(apiTopic.Name);
            return string.IsNullOrEmpty(MainPath) ? apiPath : MainPath + '/' + apiPath;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the addressing strategy is already bound to a documentation context.</exception>
        void IContextualAddressingStrategy.InitializeWithContext(IDocumentationContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (this.context is not null)
                throw new InvalidOperationException("The addressing strategy is already bound to a documentation context.");

            this.context = context;
        }
    }
}

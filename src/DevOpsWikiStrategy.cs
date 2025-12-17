// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides an addressing strategy for organizing and addressing documentation pages hosted on an Azure DevOps Wiki.
    /// </summary>
    /// <remarks>
    /// The <see cref="DevOpsWikiStrategy"/> class provides a structured approach for organizing and resolving documentation
    /// pages hosted on an Azure DevOps Wiki. It translates code references into wiki-friendly URLs and file paths, ensuring
    /// compatibility with Azure DevOps' wiki structure.
    /// <para>
    /// The strategy organizes API documentation content into namespace and type pages. Namespaces receive dedicated pages,
    /// with types documented on separate pages nested under their namespaces. Type members such as methods and properties are
    /// documented on their declaring type's page, accessible through URL fragments for efficient navigation while maintaining
    /// a clean wiki structure.
    /// </para>
    /// <para>
    /// Content organization for topics follows a simple hierarchical structure based on parent-child relationships. Topic
    /// paths are constructed using encoded titles, with child topics nested under their parents to maintain a clean and
    /// navigable wiki structure.
    /// </para>
    /// Key features of the <see cref="DevOpsWikiStrategy"/> addressing strategy include:
    /// <list type="bullet">
    ///   <item><description>Compatibility with Azure DevOps Wiki linking conventions</description></item>
    ///   <item><description>Namespace-then-type hierarchy for intuitive API documentation navigation</description></item>
    ///   <item><description>URL fragments for type members to maintain clean page structure</description></item>
    /// </list>
    /// This strategy works well for teams that want to maintain their documentation directly within their Azure DevOps environment,
    /// providing seamless integration between code and narrative documentation that's easily accessible to all team members.
    /// </remarks>
    /// <threadsafety static="true" instance="false">
    /// Public instance members can be considered thread-safe if neither <see cref="DevOpsWikiOptions.MainTopicId"/> nor
    /// <see cref="DevOpsWikiOptions.ApiTopicId"/> is set in the construction options of the instance. If either property
    /// is set, instance members are only thread-safe if the instance is fully initialized  (that is, <see cref="IContextualAddressingStrategy.InitializeWithContext(IDocumentationContext)"/>
    /// has been called)  before any other method is accessed, and there is no concurrent access during or before initialization.
    /// <para>
    /// The class is not thread-safe otherwise, because it contains a mutable context field and uses lazy initialization that depends
    /// on this field, without synchronization.
    /// </para>
    /// </threadsafety>
    /// <seealso href="https://learn.microsoft.com/en-us/azure/devops/project/wiki/about-readme-wiki?view=azure-devops">Azure DevOps Wiki</seealso>
    public class DevOpsWikiStrategy : AddressingStrategy, ILanguageSpecific, IContextualAddressingStrategy
    {
        private IDocumentationContext? context;
        private readonly Lazy<string?> mainPath;
        private readonly Lazy<string?> apiPath;

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="DevOpsWikiStrategy"/> class.
        /// </summary>
        /// <seealso cref="DevOpsWikiOptions"/>
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="DevOpsWikiStrategy"/> class with default options.
        /// </summary>
        public DevOpsWikiStrategy()
            : this(new DevOpsWikiOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DevOpsWikiStrategy"/> class with the specified options.
        /// </summary>
        /// <param name="options">The Azure Wiki addressing options.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
        public DevOpsWikiStrategy(DevOpsWikiOptions options)
            : base(options)
        {
            Language = options.Language;

            var mainTopicName = options.MainTopicId;
            mainPath = new Lazy<string?>(() => ResolveMainPath(mainTopicName));

            var apiTopicName = options.ApiTopicId;
            apiPath = new Lazy<string?>(() => ResolveApiPath(apiTopicName));
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
        /// Gets the file path of the main API page.
        /// </summary>
        /// <value>
        /// The file path of the main API page, which also serves as the folder for all API reference pages.
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
            address = CreateAddressFromPath(declaringTypePath, EncodeWikiFragmentIdentifier(memberSignature));
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
        /// <param name="type">The reflection metadata of the type.</param>
        /// <returns>The relative documentation path for the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        protected virtual string GetTypePath(IType type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var typeName = Language.FormatSignature(type, NameQualifier.DeclaringType);

            var path = EncodeWikiPath(typeName);
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

            var path = EncodeWikiPath(ns);
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

            var path = EncodeWikiPath(topic.Title);

            if (topic.ParentTopic is not null)
                path = GetTopicPath(topic.ParentTopic) + '/' + path;
            else if (!string.IsNullOrEmpty(MainPath) && path != MainPath)
                path = MainPath + '/' + path;

            return path;
        }

        /// <summary>
        /// Encodes special characters in the given title as per the Azure DevOps Wiki requirements for page titles.
        /// </summary>
        /// <param name="title">The title to encode.</param>
        /// <returns>The encoded title.</returns>
        /// <remarks>
        /// This method encodes special characters in the title according to Azure DevOps Wiki requirements:
        /// <list type="bullet">
        ///   <item><description>Spaces are replaced with hyphens.</description></item>
        ///   <item><description>Reserved or special characters are percent-encoded.</description></item>
        ///   <item><description>Forbidden characters such (#, /, \) are replaced with underscores.</description></item>
        ///   <item><description>All other characters are left unchanged.</description></item>
        /// </list>
        /// This ensures generated wiki paths are valid and compatible with Azure DevOps Wiki page title rules.
        /// </remarks>
        /// <seealso href="https://learn.microsoft.com/azure/devops/project/wiki/wiki-file-structure?view=azure-devops#special-characters-in-wiki-page-titles">
        /// Special characters in Wiki page titles
        /// </seealso>
        public static string EncodeWikiPath(ReadOnlySpan<char> title)
        {
            using var reusable = StringBuilderPool.Shared.GetBuilder();
            var sb = reusable.Builder;

            sb.EnsureCapacity(title.Length * 3);

            foreach (var c in title)
            {
                switch (c)
                {
                    case ' ':
                        sb.Append('-');
                        break;
                    case ':':
                        sb.Append("%3A");
                        break;
                    case '<':
                        sb.Append("%3C");
                        break;
                    case '>':
                        sb.Append("%3E");
                        break;
                    case '*':
                        sb.Append("%2A");
                        break;
                    case '?':
                        sb.Append("%3F");
                        break;
                    case '|':
                        sb.Append("%7C");
                        break;
                    case '-':
                        sb.Append("%2D");
                        break;
                    case '"':
                        sb.Append("%22");
                        break;
                    case '#' or '/' or '\\':
                        sb.Append('_');
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Encodes a string for use as a fragment identifier compatible with Azure DevOps Wiki heading fragments.
        /// </summary>
        /// <param name="heading">The heading to encode as a fragment identifier.</param>
        /// <returns>The encoded fragment identifier.</returns>
        /// <remarks>
        /// This method encodes fragments to match Azure DevOps Wiki heading fragment rules:
        /// <list type="bullet">
        ///   <item><description>All characters are converted to lowercase.</description></item>
        ///   <item><description>Spaces are replaced with hyphens.</description></item>
        ///   <item><description>Reserved or special characters are percent-encoded.</description></item>
        ///   <item><description>Forbidden characters such (#, /, \) are replaced with underscores.</description></item>
        ///   <item><description>All other characters are left unchanged.</description></item>
        /// </list>
        /// </remarks>
        public static string EncodeWikiFragmentIdentifier(string heading)
        {
            if (string.IsNullOrEmpty(heading))
                return string.Empty;

            return EncodeWikiPath(heading.ToLowerInvariant());
        }

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

            return EncodeWikiPath(mainTopic.Name);
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

            var apiPath = EncodeWikiPath(apiTopic.Name);
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
// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using System;

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
    /// Public instance members can be considered thread-safe if neither <see cref="MarkdownAddressingOptions.MainTopicId"/> nor
    /// <see cref="MarkdownAddressingOptions.ApiTopicId"/> is set in the construction options of the instance. If either property
    /// is set, instance members are only thread-safe if the instance is fully initialized  (that is, <see cref="IContextualAddressingStrategy.InitializeWithContext(IDocumentationContext)"/>
    /// has been called)  before any other method is accessed, and there is no concurrent access during or before initialization.
    /// <para>
    /// The class is not thread-safe otherwise, because it contains a mutable context field and uses lazy initialization that depends
    /// on this field, without synchronization.
    /// </para>
    /// </threadsafety>
    /// <seealso href="https://learn.microsoft.com/en-us/azure/devops/project/wiki/about-readme-wiki?view=azure-devops">Azure DevOps Wiki</seealso>
    public class DevOpsWikiStrategy : MarkdownAddressingStrategy
    {
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
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The page name is encoded by <see cref="EncodeWikiPath(ReadOnlySpan{char})"/>.
        /// </remarks>
        protected override string EncodePageName(string title) => EncodeWikiPath(title);

        /// <inheritdoc/>
        /// <remarks>
        /// The fragment identifier is encoded by <see cref="EncodeWikiFragmentIdentifier(string)"/>.
        /// </remarks>
        protected override string EncodeFragmentIdentifier(string heading) => EncodeWikiFragmentIdentifier(heading);

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
    }
}

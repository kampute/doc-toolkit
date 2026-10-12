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
    /// Provides an addressing strategy for organizing and addressing documentation pages hosted on a GitLab Wiki.
    /// </summary>
    /// <remarks>
    /// The <see cref="GitLabWikiStrategy"/> class provides a structured approach for organizing and resolving documentation
    /// pages hosted on a GitLab Wiki. It translates code references into wiki pages and heading anchors that follow GitLab's
    /// conventions.
    /// <para>
    /// The strategy organizes API documentation content into namespace and type pages. Namespaces receive dedicated pages,
    /// with types documented on separate pages nested under their namespaces. Type members such as methods and properties are
    /// documented on their declaring type's page, and are addressed by the anchor GitLab generates for their headings.
    /// </para>
    /// <para>
    /// Content organization for topics follows a simple hierarchical structure based on parent-child relationships. Topic
    /// pages are named after their titles, with child topics nested under their parents.
    /// </para>
    /// <note type="tip" title="Tip">
    /// GitLab displays the file name of a page as its title, showing hyphens as spaces. Because page names cannot contain every
    /// character a title can, renderers can write the original title in the <c>title</c> field of the page's front matter, which
    /// GitLab displays instead of the file name.
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
    /// <seealso href="https://docs.gitlab.com/user/project/wiki/">GitLab Wiki</seealso>
    public class GitLabWikiStrategy : MarkdownAddressingStrategy
    {
        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="GitLabWikiStrategy"/> class.
        /// </summary>
        /// <seealso cref="GitLabWikiOptions"/>
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="GitLabWikiStrategy"/> class with default options.
        /// </summary>
        public GitLabWikiStrategy()
            : this(new GitLabWikiOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GitLabWikiStrategy"/> class with the specified options.
        /// </summary>
        /// <param name="options">The GitLab Wiki addressing options.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
        public GitLabWikiStrategy(GitLabWikiOptions options)
            : base(options)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The page name is encoded by <see cref="EncodeWikiPath(ReadOnlySpan{char})"/>.
        /// </remarks>
        protected override string EncodePageName(string title) => EncodeWikiPath(title);

        /// <summary>
        /// Converts the given title to a GitLab Wiki page name that is valid as a file name and as a URL path segment.
        /// </summary>
        /// <param name="title">The title to encode.</param>
        /// <returns>The encoded title.</returns>
        /// <remarks>
        /// This method converts the title as follows:
        /// <list type="bullet">
        ///   <item><description>Spaces are replaced with hyphens, which GitLab displays as spaces.</description></item>
        ///   <item><description>Angle brackets (&lt; and &gt;) are replaced with braces ({ and }), so <c>List&lt;T&gt;</c> becomes <c>List{T}</c>.</description></item>
        ///   <item><description>Characters that are not allowed in file names or have a special meaning in URLs (:, ", /, \, |, ?, *, #, %) are replaced with underscores.</description></item>
        ///   <item><description>All other characters are left unchanged.</description></item>
        /// </list>
        /// </remarks>
        public static string EncodeWikiPath(ReadOnlySpan<char> title)
        {
            using var reusable = StringBuilderPool.Shared.GetBuilder();
            var sb = reusable.Builder;
            sb.EnsureCapacity(title.Length);

            foreach (var c in title)
            {
                sb.Append(c switch
                {
                    ' ' => '-',
                    '<' => '{',
                    '>' => '}',
                    ':' or '"' or '/' or '\\' or '|' or '?' or '*' or '#' or '%' => '_',
                    _ => c,
                });
            }

            return sb.ToString();
        }
    }
}

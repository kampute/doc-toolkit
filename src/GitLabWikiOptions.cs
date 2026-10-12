// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Routing;

    /// <summary>
    /// Provides options for generating documentation addresses compatible with GitLab Wiki.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="GitLabWikiOptions"/> class provides configuration for documentation file addressing
    /// specifically tailored for GitLab Wiki compatibility.
    /// </para>
    /// This class implements the following default configuration:
    /// <list type="bullet">
    ///   <item><description>Sets file extension to Markdown (<c>.md</c>)</description></item>
    ///   <item><description>Includes file extension in generated URLs, so that GitLab resolves links relative to the current page</description></item>
    ///   <item><description>Uses C# for formatting member names</description></item>
    /// </list>
    /// </remarks>
    /// <seealso cref="GitLabWikiStrategy"/>
    public class GitLabWikiOptions : MarkdownAddressingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitLabWikiOptions"/> class.
        /// </summary>
        public GitLabWikiOptions()
            : base()
        {
        }
    }
}

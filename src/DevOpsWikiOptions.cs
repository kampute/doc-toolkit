// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;

    /// <summary>
    /// Provides options for generating documentation addresses compatible with Azure DevOps Wiki.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DevOpsWikiOptions"/> class provides configuration for documentation file addressing
    /// specifically tailored for Azure DevOps Wiki compatibility.
    /// </para>
    /// This class implements the following default configuration:
    /// <list type="bullet">
    ///   <item><description>Sets file extension to Markdown (<c>.md</c>)</description></item>
    ///   <item><description>Includes file extension in generated URLs</description></item>
    ///   <item><description>Use C# as for formatting member names</description></item>
    /// </list>
    /// </remarks>
    /// <seealso cref="DevOpsWikiStrategy"/>
    public class DevOpsWikiOptions : MarkdownAddressingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DevOpsWikiOptions"/> class.
        /// </summary>
        public DevOpsWikiOptions()
            : base(FileExtensions.Markdown)
        {
            // Although page references in Azure DevOps Wiki are without extensions,
            // we still use the file extension for compatibility with other Markdown-based systems.
            OmitExtensionInUrls = false;
        }
    }
}

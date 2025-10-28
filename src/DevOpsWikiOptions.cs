// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Languages;
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
    public class DevOpsWikiOptions : AddressingOptions
    {
        private IProgrammingLanguage language = Languages.Language.Default;

        private string mainTopicId = string.Empty;
        private string apiTopicId = string.Empty;

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

        /// <summary>
        /// Gets or sets the programming language formatter to be used for formatting member names.
        /// </summary>
        /// <value>
        /// The <see cref="IProgrammingLanguage"/> instance that defines the syntax rules for formatting member names.
        /// </value>
        public IProgrammingLanguage Language
        {
            get => language;
            set => language = value ?? Languages.Language.Default;
        }

        /// <summary>
        /// Gets or sets the identifier of the topic that should serve as the main entry point for the documentation.
        /// </summary>
        /// <value>
        /// The identifier of the topic that should serve as the main entry point for the documentation.
        /// </value>
        /// <remarks>
        /// When this property is not empty and there is a topic with the specified identifier, that topic will be used
        /// as the main topic of the documentation and all other pages will be placed under it.
        /// </remarks>
        public string MainTopicId
        {
            get => mainTopicId;
            set => mainTopicId = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        /// <summary>
        /// Gets or sets the identifier of the topic that should serve as the introduction or overview of the API references.
        /// </summary>
        /// <value>
        /// The identifier of the topic that should serve as the introduction or overview of the API references.
        /// </value>
        /// <remarks>
        /// When this property is not empty and there is a topic with the specified identifier, that topic will be used as
        /// the API topic of the documentation and all API references will be placed under it.
        /// </remarks>
        public string ApiTopicId
        {
            get => apiTopicId;
            set => apiTopicId = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}

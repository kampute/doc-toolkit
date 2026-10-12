// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Support;
    using System;

    /// <summary>
    /// Provides options for Markdown-based addressing and organizing documentation pages.
    /// </summary>
    /// <remarks>
    /// This abstract class defines common configuration options for Markdown addressing strategies that organize documentation pages
    /// as a hierarchy of pages named after their titles, such as the pages of a wiki. Derived classes should extend these options with
    /// additional configuration properties specific to particular Markdown publishing targets.
    /// <para>
    /// By default, the file extension is <c>.md</c>, it is included in the generated URLs, and C# is used for formatting member names.
    /// </para>
    /// </remarks>
    /// <seealso cref="MarkdownAddressingStrategy"/>
    public abstract class MarkdownAddressingOptions : AddressingOptions
    {
        private IProgrammingLanguage language = Languages.Language.Default;

        private string mainTopicId = string.Empty;
        private string apiTopicId = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownAddressingOptions"/> class.
        /// </summary>
        protected MarkdownAddressingOptions()
            : this(FileExtensions.Markdown)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownAddressingOptions"/> class with a specified file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension for the documentation files.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fileExtension"/> is <see langword="null"/> or empty.</exception>
        protected MarkdownAddressingOptions(string fileExtension)
            : base(fileExtension)
        {
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
        /// When this property is not empty, the topic with the specified identifier is used as the main topic of the documentation,
        /// and all other pages are placed under it. If the documentation context has no topic with this identifier, the addressing
        /// strategy throws an <see cref="ArgumentException"/> when it resolves page addresses.
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
        /// When this property is not empty, the topic with the specified identifier is used as the API topic of the documentation,
        /// and all API references are placed under it. If the documentation context has no topic with this identifier, the addressing
        /// strategy throws an <see cref="ArgumentException"/> when it resolves API reference addresses.
        /// </remarks>
        public string ApiTopicId
        {
            get => apiTopicId;
            set => apiTopicId = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}

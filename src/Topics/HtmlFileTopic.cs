// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Topics
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Support;
    using System;
    using System.IO;

    /// <summary>
    /// Represents a documentation topic that is stored in an HTML file.
    /// </summary>
    /// <remarks>
    /// The <see cref="HtmlFileTopic"/> class handles an HTML file as a documentation topic source. It automatically
    /// extracts the title from HTML <c>&lt;title&gt;</c> tag and attempts to convert the content of the <c>&lt;body&gt;</c>
    /// tag to the appropriate format of the documentation system.
    /// <note type="tip" title="Tip">
    /// If you need to include the entire HTML file as a topic, including the <c>&lt;head&gt;</c> tags, consider using the
    /// <see cref="HtmlFileTopic"/> class.
    /// </note>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public class HtmlFileTopic : FileTopic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlFileTopic"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the topic among its siblings, typically used for URLs or filenames.</param>
        /// <param name="path">The path to the file that contains the topic content.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="id"/> is <see langword="null"/>, whitespace, or contains invalid characters,
        /// or when <paramref name="path"/> is <see langword="null"/> or whitespace.
        /// </exception>
        public HtmlFileTopic(string id, string path)
            : base(id, path)
        {
        }

        /// <summary>
        /// Gets the format of the content in the topic.
        /// </summary>
        /// <value>
        /// The file extension (including the leading period) representing the format of the content in the topic,
        /// which is always ".html" for HTML files.
        /// </value>
        protected sealed override string ContentFormat => FileExtensions.Html;

        /// <summary>
        /// Creates a <see cref="TextReader"/> to read the content of the topic
        /// </summary>
        /// <param name="context">The documentation context that provides additional information for the operation.</param>
        /// <returns>A <see cref="TextReader"/> to read the content of the topic.</returns>
        /// <exception cref="IOException">Thrown when an I/O error occurs while reading the file specified by <see cref="FileTopic.FilePath"/>.</exception>
        /// <remarks>
        /// This methods attempts to extract the content of the <c>&lt;body&gt;</c> tag from the HTML file and returns a <see cref="StringReader"/>
        /// for that content. If the <c>&lt;body&gt;</c> tag is not found, it returns a <see cref="StringReader"/> for the entire HTML file content.
        /// </remarks>
        protected override TextReader CreateContentReader(IDocumentationContext context)
        {
            var html = File.ReadAllText(FilePath);
            return HtmlParsingHelper.TryExtractTagContent(html, "body", out var content)
                ? new StringReader(content)
                : new StringReader(html);
        }

        /// <summary>
        /// Extracts the title of the topic from the HTML file.
        /// </summary>
        /// <returns>The title of the topic.</returns>
        /// <exception cref="IOException">Thrown when an I/O error occurs while reading the file specified by <see cref="FileTopic.FilePath"/>.</exception>
        /// <remarks>
        /// This method attempts to extract the content of the <c>&lt;title&gt;</c> tag from the HTML file. If the
        /// tag is not found, it falls back to the default title generation from the topic's name.
        /// </remarks>
        protected override string GenerateTitle()
        {
            var html = File.ReadAllText(FilePath);
            return HtmlParsingHelper.TryExtractTagContent(html, "title", out var title) && !string.IsNullOrWhiteSpace(title)
                ? TextUtility.NormalizeWhitespace(title)
                : base.GenerateTitle();
        }
    }
}

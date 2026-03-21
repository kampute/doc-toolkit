// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Topics
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Represents a documentation topic that is stored in a Markdown file.
    /// </summary>
    /// <remarks>
    /// The <see cref="MarkdownFileTopic"/> class handles a Markdown file as a documentation topic source. It automatically
    /// extracts the title from Markdown headings and attempts to convert the content to the appropriate format of the documentation
    /// system.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public class MarkdownFileTopic : FileTopic
    {
        private readonly Lazy<MarkdownData> markdown;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownFileTopic"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the topic among its siblings, typically used for URLs or filenames.</param>
        /// <param name="path">The path to the file that contains the topic content.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="id"/> is <see langword="null"/>, whitespace, or contains invalid characters,
        /// or when <paramref name="path"/> is <see langword="null"/> or whitespace.
        /// </exception>
        public MarkdownFileTopic(string id, string path)
            : base(id, path)
        {
            markdown = new(ParseMarkdown);
        }

        /// <summary>
        /// Gets the collection of front matter metadata associated with the Markdown file topic.
        /// </summary>
        /// <value>
        /// A read-only dictionary containing the front matter metadata, where the keys are the metadata names
        /// and the values are the corresponding metadata values.
        /// </value>
        public IReadOnlyDictionary<string, object?> FrontMatter => markdown.Value.FrontMatter;

        /// <summary>
        /// Gets the format of the content in the topic.
        /// </summary>
        /// <value>
        /// The file extension (including the period ".") representing the format of the content in the topic,
        /// which is always ".md" for Markdown files.
        /// </value>
        protected sealed override string ContentFormat => FileExtensions.Markdown;

        /// <summary>
        /// Creates a <see cref="TextReader"/> to read the content of the source file, excluding any front matter.
        /// </summary>
        /// <param name="context">The documentation context that provides additional information for the operation.</param>
        /// <returns>A <see cref="TextReader"/> for reading the content of the file specified by <see cref="FileTopic.FilePath"/>, starting after the front matter if present.</returns>
        /// <exception cref="IOException">Thrown when an I/O error occurs while reading the file specified by <see cref="FileTopic.FilePath"/>.</exception>
        protected override TextReader CreateContentReader(IDocumentationContext context) => new StringReader(markdown.Value.Content);

        /// <summary>
        /// Extracts the title of the topic from the Markdown file.
        /// </summary>
        /// <returns>The title of the topic.</returns>
        /// <remarks>
        /// This method starts by checking the front matter for a "title" entry. If a valid title exists, it uses that as the topic title. 
        /// If not, it tries to extract the first Markdown heading from the file. If successful, that heading becomes the title. 
        /// Otherwise, it reverts to the base class's default title generation logic.
        /// </remarks>
        protected override string GenerateTitle()
        {
            try
            {
                if (FrontMatter.TryGetValue("title", out var rawTitle) && rawTitle is string title && !string.IsNullOrWhiteSpace(title))
                    return title.Trim();

                if (Markdown.TryGetFirstHeading(markdown.Value.Content, out var heading))
                    return heading;
            }
            catch (Exception)
            {
                // Ignore any exceptions and fall back to the default title generation.
            }

            return base.GenerateTitle();
        }

        /// <summary>
        /// Parses the Markdown file to extract the front matter and content.
        /// </summary>
        /// <returns>A <see cref="MarkdownData"/> struct containing the parsed front matter and content of the Markdown file.</returns>
        /// <exception cref="IOException">Thrown when an I/O error occurs while reading the file specified by <see cref="FileTopic.FilePath"/>.</exception>
        private MarkdownData ParseMarkdown()
        {
            var text = File.ReadAllText(FilePath);
            return Markdown.TryExtractFrontMatter(text, out var frontMatter, out var contentStart)
                ? new MarkdownData(frontMatter, text[contentStart..])
                : new MarkdownData(Yaml.Empty, text);
        }

        /// <summary>
        /// Represents the parsed front matter and content of a Markdown file topic.
        /// </summary>
        private sealed class MarkdownData
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MarkdownData"/> struct with the specified front matter and content.
            /// </summary>
            /// <param name="frontMatter">The front matter metadata extracted from the Markdown file.</param>
            /// <param name="content">The content of the Markdown file, excluding any front matter.</param>
            public MarkdownData(IReadOnlyDictionary<string, object?> frontMatter, string content)
            {
                FrontMatter = frontMatter ?? Yaml.Empty;
                Content = content ?? string.Empty;
            }

            /// <summary>
            /// Gets the front matter metadata extracted from the Markdown file.
            /// </summary>
            /// <value>
            /// A read-only dictionary containing the front matter metadata, where the keys are the metadata names
            /// and the values are the corresponding metadata values.
            /// </value>
            public IReadOnlyDictionary<string, object?> FrontMatter { get; }

            /// <summary>
            /// Gets the content of the Markdown file, excluding any front matter.
            /// </summary>
            /// <value>
            /// The content of the Markdown file, excluding any front matter, which is intended to be rendered as 
            /// the main body of the documentation topic.
            /// </value>
            public string Content { get; }
        }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Topics
{
    using Kampute.DocToolkit.Support;
    using System;
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
        }

        /// <summary>
        /// Gets the format of the content in the topic.
        /// </summary>
        /// <value>
        /// The file extension (including the period ".") representing the format of the content in the topic,
        /// which is always ".md" for Markdown files.
        /// </value>
        protected sealed override string ContentFormat => FileExtensions.Markdown;

        /// <summary>
        /// Extracts the title of the topic from the Markdown file.
        /// </summary>
        /// <returns>The title of the topic.</returns>
        /// <exception cref="IOException">Thrown when an I/O error occurs while reading the file specified by <see cref="FileTopic.FilePath"/>.</exception>
        /// <remarks>
        /// This method attempts to extract the first Markdown heading from the Markdown file. If the heading is not found,
        /// it falls back to the default title generation from the topic's name.
        /// </remarks>
        protected override string GenerateTitle()
        {
            try
            {
                using var reader = File.OpenText(FilePath);

                string? line;
                while ((line = reader.ReadLine()) is not null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (!line.TrimStart(' ').StartsWith('#'))
                        break;

                    var title = line.TrimStart(['#', ' ']).TrimEnd();
                    if (title.Length == 0)
                        break;

                    return title;
                }
            }
            catch (Exception)
            {
                // Ignore any exceptions and fall back to the default title generation.
            }

            return base.GenerateTitle();
        }
    }
}

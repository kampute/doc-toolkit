// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Topics
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics.Abstracts;
    using System;
    using System.IO;

    /// <summary>
    /// Represents source of a documentation topic that reads content from a file.
    /// </summary>
    /// <remarks>
    /// The <see cref="FileTopic"/> class serves as the foundation for file-backed documentation topics. It handles reading
    /// content from disk files and provides core functionality for deriving topic names and titles from file properties.
    /// <para>
    /// This class can be used directly, or extended for specialized file formats like Markdown or HTML through derived
    /// classes.
    /// </para>
    /// <note type="caution" title="Caution">
    /// The constructor of the class does not validate the existence of the file at <see cref="FilePath"/>. It
    /// assumes that the file will be present when the content is read. If the file does not exist at the time of
    /// reading, an <see cref="IOException"/> will be thrown.
    /// </note>
    /// </remarks>
    /// <seealso cref="FileTopicFactory"/>
    /// <seealso cref="FileTopicHelper"/>
    /// <threadsafety static="true" instance="false"/>
    public class FileTopic : ConvertibleTopic, IFileBasedTopic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileTopic"/> class.
        /// </summary>
        /// <param name="id">The unique identifier of the topic among its siblings, typically used for URLs or filenames.</param>
        /// <param name="path">The path to the file that contains the topic content.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="id"/> is <see langword="null"/>, whitespace, or contains invalid characters,
        /// or when <paramref name="path"/> is <see langword="null"/> or whitespace.
        /// </exception>
        public FileTopic(string id, string path)
            : base(id)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace.", nameof(path));

            FilePath = path;
        }

        /// <summary>
        /// Gets the path to the file that contains the topic content.
        /// </summary>
        /// <value>
        /// The file path to the file that contains the topic content.
        /// </value>
        public string FilePath { get; }

        /// <summary>
        /// Gets the format of the content in the topic.
        /// </summary>
        /// <value>
        /// The file extension (including the leading period) representing the format of the content in the topic.
        /// </value>
        protected override string ContentFormat => Path.GetExtension(FilePath);

        /// <summary>
        /// Creates a <see cref="TextReader"/> to read the content of the source file.
        /// </summary>
        /// <param name="context">The documentation context that provides additional information for the operation.</param>
        /// <returns>A <see cref="TextReader"/> for reading the content of the file specified by <see cref="FilePath"/>.</returns>
        /// <exception cref="IOException">Thrown when an I/O error occurs while reading the file specified by <see cref="FilePath"/>.</exception>
        protected override TextReader CreateContentReader(IDocumentationContext context) => File.OpenText(FilePath);
    }
}

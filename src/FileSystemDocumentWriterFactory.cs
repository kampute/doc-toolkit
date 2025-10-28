// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.IO.Writers;
    using System;
    using System.IO;

    /// <summary>
    /// Provides a file system-based implementation of the <see cref="IDocumentWriterFactory"/> interface for writing documentation
    /// content to the file system.
    /// </summary>
    /// <remarks>
    /// The <see cref="FileSystemDocumentWriterFactory"/> provides a concrete implementation of <see cref="IDocumentWriterFactory"/>
    /// that creates rendering context for writing documentation content to the file system. This factory is responsible for:
    /// <list type="bullet">
    ///   <item><description>Managing the output directory structure for documentation files</description></item>
    ///   <item><description>Resolving relative file paths for code elements and topics</description></item>
    ///   <item><description>Optionally applying content minification to the output</description></item>
    /// </list>
    /// The factory works in conjunction with the document context's addressing strategy to determine where each piece of documentation
    /// should be placed in the output folder structure. By centralizing file path resolution and rendering context creation, it ensures
    /// consistent output location and format across the documentation generation process.
    /// </remarks>
    public class FileSystemDocumentWriterFactory : IDocumentWriterFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemDocumentWriterFactory"/> class.
        /// </summary>
        /// <param name="outputFolder">The output folder for the documentation files.</param>
        /// <param name="options">The options for output generation, or <see langword="null"/> to use default options.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="outputFolder"/> is <see langword="null"/>.</exception>
        public FileSystemDocumentWriterFactory(string outputFolder, OutputOptions? options = null)
        {
            OutputFolder = outputFolder ?? throw new ArgumentNullException(nameof(outputFolder));
            IsMinificationEnabled = options is null || !options.DisableMinification;
        }

        /// <summary>
        /// Gets the output folder for the documentation files.
        /// </summary>
        /// <value>
        /// The output folder for the documentation files.
        /// </value>
        public string OutputFolder { get; }

        /// <summary>
        /// Gets a value indicating whether the output minification is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if minification is enabled; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsMinificationEnabled { get; }

        /// <summary>
        /// Creates an instance of <see cref="TextWriter"/> for rendering documentation content of the specified document model.
        /// </summary>
        /// <param name="model">The model to create a rendering target for.</param>
        /// <returns>An instance of <see cref="TextWriter"/> for writing documentation content.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the file path of the model could not be resolved.</exception>
        public virtual TextWriter CreateWriter(IDocumentModel model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            var context = model.Context;

            if (!model.TryGetDocumentationFile(out var relativePath))
                throw new ArgumentException($"The file path of '{model}' could not be resolved", nameof(model));

            var textWriter = CreateTextWriter(relativePath);
            if (IsMinificationEnabled)
                textWriter = context.ContentFormatter.CreateMinifier(textWriter);

            var dirPath = Path.GetDirectoryName(relativePath);
            var scope = context.AddressProvider.BeginScope(dirPath!, model);

            return new UrlContextAwareWriter(textWriter, scope);
        }

        /// <summary>
        /// Creates a <see cref="TextWriter"/> for writing documentation content to the specified relative path.
        /// </summary>
        /// <param name="relativePath">The relative path where the documentation content will be written.</param>
        /// <returns>A <see cref="TextWriter"/> for writing documentation content.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="relativePath"/> is <see langword="null"/> or empty.</exception>
        /// <remarks>
        /// This method combines the <see cref="OutputFolder"/> property with the specified <paramref name="relativePath"/> to determine the full file path.
        /// If the directory for the resulting path does not exist, it will be created automatically. The returned <see cref="TextWriter"/> writes directly
        /// to the specified file and does not apply any content transformations, such as minification.
        /// </remarks>
        public virtual TextWriter CreateTextWriter(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                throw new ArgumentException($"'{nameof(relativePath)}' cannot be null or empty.", nameof(relativePath));

            var path = Path.Combine(OutputFolder, relativePath);
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            return File.CreateText(path);
        }
    }
}

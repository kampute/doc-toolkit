// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using System;

    /// <summary>
    /// Represents a generator that creates documentation files for code elements in a given context.
    /// </summary>
    /// <remarks>
    /// The <see cref="FileSystemDocumentationComposer"/> class provides a concrete implementation for generating complete API
    /// documentation sets. As a subclass of <see cref="DocumentationComposer"/>, it orchestrates the entire documentation generation
    /// process by rendering documentation pages and writing them to a specified output folder.
    /// <para>
    /// This class serves as the main entry point for the documentation generation process, taking a documentation context (containing
    /// assemblies and XML documentations) and producing organized documentation files. It handles both the logical organization of
    /// documentation content and the physical file output.
    /// </para>
    /// Typical usage involves:
    /// <list type="number">
    ///   <item>
    ///     Configuring a documentation context with assemblies and XML documentation sources
    ///   </item>
    ///   <item>
    ///     Creating an instance of <see cref="FileSystemDocumentationComposer"/>
    ///   </item>
    ///   <item>
    ///     Calling <see cref="GenerateDocumentation(IDocumentationContext, string, OutputOptions?)"/> to produce the complete
    ///     set of documentation files for the specified context in the desired output folder.
    ///   </item>
    /// </list>
    /// </remarks>
    /// <seealso cref="IDocumentRenderer"/>
    public class FileSystemDocumentationComposer : DocumentationComposer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemDocumentationComposer"/> class.
        /// </summary>
        /// <param name="renderer">The documentation renderer.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="renderer"/> is <see langword="null"/>.</exception>
        public FileSystemDocumentationComposer(IDocumentRenderer renderer)
        {
            Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        /// <summary>
        /// Gets the documentation renderer.
        /// </summary>
        /// <value>
        /// The documentation renderer.
        /// </value>
        protected override IDocumentRenderer Renderer { get; }

        /// <summary>
        /// Generates documentation pages for the specified context and writes the output to the specified folder.
        /// </summary>
        /// <param name="documentationContext">The documentation context.</param>
        /// <param name="outputFolder">The output folder for the documentation files.</param>
        /// <param name="options">The options for output generation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="documentationContext"/> or <paramref name="outputFolder"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">Throw when the format of a documentation topic is not supported.</exception>
        public virtual void GenerateDocumentation(IDocumentationContext documentationContext, string outputFolder, OutputOptions? options = null)
        {
            if (documentationContext is null)
                throw new ArgumentNullException(nameof(documentationContext));
            if (outputFolder is null)
                throw new ArgumentNullException(nameof(outputFolder));

            var writerFactory = new FileSystemDocumentWriterFactory(outputFolder, options);
            GenerateDocumentation(writerFactory, documentationContext);
        }
    }
}

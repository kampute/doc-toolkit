// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using System.IO;

    /// <summary>
    /// Defines a contract for creating <see cref="TextWriter"/> instances to write content of a specific documentation
    /// element.
    /// </summary>
    /// <remarks>
    /// The <see cref="IDocumentWriterFactory"/> interface provides an abstraction for creating rendering contexts that are used
    /// throughout the documentation generation process. The factory pattern used here allows the documentation system to create
    /// appropriate <see cref="TextWriter"/> instances for different types of content while ensuring consistent handling
    /// of resources and proper disposal. This separation of concerns enables consumers to focus on writing documentation content
    /// without needing to manage the details of file paths, URL resolution, or resource cleanup.
    /// <para>
    /// Implementations of this interface are responsible for determining:
    /// <list type="bullet">
    ///   <item><description>Where documentation content will be written (file system, memory, etc.)</description></item>
    ///   <item><description>How to scope URLs for cross-reference links in the documentation content</description></item>
    ///   <item><description>Any additional processing that should be applied before or after rendering</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public interface IDocumentWriterFactory
    {
        /// <summary>
        /// Creates a <see cref="TextWriter"/> instance for rendering documentation content of the specified document model.
        /// </summary>
        /// <param name="model">The document model to create a context for.</param>
        /// <returns>A <see cref="TextWriter"/> for rendering documentation content.</returns>
        TextWriter CreateWriter(IDocumentModel model);
    }
}

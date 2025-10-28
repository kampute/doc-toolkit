// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    /// <summary>
    /// Defines a contract for establishing and managing document-specific URL contexts for adjusting cross-document relative links.
    /// </summary>
    /// <remarks>
    /// This interface provides a mechanism to establish a temporary context for the document currently being rendered, enabling
    /// proper URL adjustment based on the document's location in the documentation hierarchy.
    /// <para>
    /// Implementations of this interface track the current document's path information, which serves as the basis for normalizing
    /// relative URLs referenced within that document. This ensures that navigation between documents works correctly regardless of
    /// the document's location in the hierarchy.
    /// </para>
    /// The context can be established temporarily for a single document and then automatically restored when processing is complete
    /// by disposing the <see cref="DocumentUrlContext"/> returned by <see cref="BeginScope"/>.
    /// </remarks>
    /// <seealso cref="DocumentUrlContext"/>
    public interface IDocumentUrlContextProvider
    {
        /// <summary>
        /// Gets the currently active URL context.
        /// </summary>
        /// <value>
        /// The currently active URL context.
        /// </value>
        DocumentUrlContext ActiveScope { get; }

        /// <summary>
        /// Creates a scoped URL context for the specified document path and model.
        /// </summary>
        /// <param name="directory">The directory path of the document being rendered relative to the documentation root.</param>
        /// <param name="model">The document model being processed, or <see langword="null"/> if not applicable.</param>
        /// <returns>A <see cref="DocumentUrlContext"/> object that represents the scoped URL context. When disposed, the context will be reset.</returns>
        /// <remarks>
        /// This method should be called before processing a document to establish the correct context for resolving any relative URLs
        /// within that document. The returned context object should be disposed when document processing is complete to restore the
        /// previous context.
        /// </remarks>
        DocumentUrlContext BeginScope(string directory, IDocumentModel? model);
    }
}
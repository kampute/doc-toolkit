// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines a contract for a model in the documentation that can be rendered with a specific context.
    /// </summary>
    /// <remarks>
    /// This interface serves as a base for all models that are part of the documentation system. It provides properties
    /// to access the documentation context and the URL for the model. Implementing this interface allows the model to
    /// be rendered with the appropriate context and provides a way to link to the model's documentation page.
    /// </remarks>
    public interface IDocumentModel
    {
        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, represented by the <see cref="DocumentationModelType"/> enumeration.
        /// </value>
        DocumentationModelType ModelType { get; }

        /// <summary>
        /// Gets the documentation context associated with the model.
        /// </summary>
        /// <value>
        /// The documentation context associated with the model, which provides additional information for rendering the model.
        /// </value>
        IDocumentationContext Context { get; }

        /// <summary>
        /// Gets the display name or title of the model.
        /// </summary>
        /// <value>
        /// The name or title of the model, used for display purposes.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the documentation URL for the model.
        /// </summary>
        /// <value>
        /// The URL of the model, which is used for linking to the model's documentation page.
        /// </value>
        /// <remarks>
        /// This property provides access to the location where this model's documentation can be found, either as an absolute
        /// URL or as a URL relative to the current document context.
        /// <para>
        /// The property value depends on the active <see cref="Routing.DocumentUrlContext"/>, which  adjusts URLs based on the
        /// current document's location within the documentation hierarchy. URLs that work correctly in one document context may
        /// need different path adjustments in another.
        /// </para>
        /// <note type="caution" title="Caution">
        /// Do not store or cache this URL value persistently. Always retrieve it when needed to ensure it correctly reflects
        /// the current active URL context. The same model may yield different URLs when accessed from different locations in
        /// the documentation.
        /// </note>
        /// </remarks>
        Uri Url { get; }

        /// <summary>
        /// Gets a value indicating whether this model represents an API model.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this model represents an API model; otherwise, <see langword="false"/>.
        /// </value>
        bool IsApiModel => ModelType != DocumentationModelType.Topic;

        /// <summary>
        /// Gets the hierarchy of parent models that lead to this model.
        /// </summary>
        /// <value>
        /// An enumerable collection of <see cref="IDocumentModel"/> objects representing the parent hierarchy.
        /// </value>
        IEnumerable<IDocumentModel> HierarchyPath { get; }

        /// <summary>
        /// Attempts to get the relative path of the documentation file for the model.
        /// </summary>
        /// <param name="relativePath">When this method returns, contains the relative file path of the documentation file for the model, if applicable; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a documentation file is applicable; otherwise, <see langword="false"/>.</returns>
        bool TryGetDocumentationFile([NotNullWhen(true)] out string? relativePath);
    }
}

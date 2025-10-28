// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    /// <summary>
    /// Defines a contract for accessing and managing the reference resolver, which is responsible for resolving
    /// references within XML documentation comments during transformation.
    /// </summary>
    /// <remarks>
    /// The <see cref="IXmlDocReferenceAccessor"/> interface provides a standardized way to access and modify
    /// the reference resolver component used during XML documentation transformation. This interface enables
    /// components that transform XML documentation to expose their reference resolution capabilities without
    /// revealing their internal implementation details.
    /// </remarks>
    public interface IXmlDocReferenceAccessor
    {
        /// <summary>
        /// Gets or sets the reference resolver for resolving references in XML documentation comments.
        /// </summary>
        /// <value>
        /// The reference resolver for resolving references in XML documentation comments, or <see langword="null"/> to use none.
        /// </value>
        IXmlDocReferenceResolver? ReferenceResolver { get; set; }
    }
}

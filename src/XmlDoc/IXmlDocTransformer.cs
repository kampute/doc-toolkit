// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using System.IO;
    using System.Xml.Linq;

    /// <summary>
    /// Defines a contract for extracting and writing the text content of XML documentation comments.
    /// </summary>
    /// <remarks>
    /// The <see cref="IXmlDocTransformer"/> interface is a core component of the documentation generation pipeline that
    /// converts XML documentation comments into various output formats such as HTML or Markdown.
    ///<para>
    /// Implementations of this interface take XML elements containing documentation comments and transform them into
    /// the desired output format. The transformation process typically involves parsing the XML structure, interpreting
    /// documentation tags, and resolving any references such as cross-references to other types or members.
    /// </para>
    /// This interface is intentionally minimal, focusing solely on the transformation capability, which makes it easy to
    /// create different transformers for various output formats while maintaining a consistent API.
    /// </remarks>
    /// <seealso cref="IXmlDocReferenceResolver"/>
    public interface IXmlDocTransformer
    {
        /// <summary>
        /// Writes the transformed content of the XML comment element to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the transformed content to.</param>
        /// <param name="comment">The XML comment element to process.</param>
        void Transform(TextWriter writer, XElement comment);
    }
}

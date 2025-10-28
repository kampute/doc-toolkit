// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using System.Xml.Linq;

    /// <summary>
    /// Defines a contract for reporting errors that occur due to unresolved elements or references while processing
    /// XML documentation comments.
    /// </summary>
    /// <remarks>
    /// This interface provides a standardized way to handle error conditions that occur during XML documentation
    /// processing. Implementations should provide meaningful error reporting while allowing the documentation processor
    /// to continue processing other elements when possible.
    /// </remarks>
    /// <seealso cref="XmlDocProvider"/>
    public interface IXmlDocErrorHandler
    {
        /// <summary>
        /// Reports that an <c>inheritdoc</c> element is used in a context where it cannot be resolved.
        /// </summary>
        /// <param name="memberElement">The <c>member</c> element containing the <c>inheritdoc</c> element.</param>
        /// <remarks>
        /// The following are situations where an <c>inheritdoc</c> element cannot be resolved:
        /// <list type="bullet">
        ///   <item>
        ///   The <c>inheritdoc</c> element has a <c>cref</c> or <c>path</c> attribute that cannot be resolved.
        ///   </item>
        ///   <item>
        ///   The <c>inheritdoc</c> has no attributes and the target element is neither overriding a member from a base
        ///   class nor implementing an interface member, or the inherited member is without XML documentation comment.
        ///   </item>
        /// </list>
        /// It is the responsibility of the implementation to decide how to handle this error. If the implementation
        /// does not throw an exception, the error is ignored by the XML documentation processor.
        /// </remarks>
        void InheritDocNotFound(XElement memberElement);

        /// <summary>
        /// Reports that the <c>file</c> attribute of an <c>include</c> element references a file that does not exist.
        /// </summary>
        /// <param name="memberElement">The <c>member</c> element containing the <c>include</c> element.</param>
        /// <param name="includeFilePath">The include file path.</param>
        /// <remarks>
        /// It is the responsibility of the implementation to decide how to handle this error. If the implementation
        /// does not throw an exception, the error is ignored by the XML documentation processor.
        /// </remarks>
        void IncludeFileNotFound(XElement memberElement, string includeFilePath);

        /// <summary>
        /// Reports that the <c>path</c> attribute of an <c>include</c> element references an XPath that cannot be resolved.
        /// </summary>
        /// <param name="memberElement">The <c>member</c> element containing the <c>include</c> element.</param>
        /// <remarks>
        /// It is the responsibility of the implementation to decide how to handle this error. If the implementation
        /// does not throw an exception, the error is ignored by the XML documentation processor.
        /// </remarks>
        void IncludeMemberNotFound(XElement memberElement);
    }
}

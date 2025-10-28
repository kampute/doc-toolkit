// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using System;
    using System.Xml.Linq;

    /// <summary>
    /// Reports errors that occur due to unresolved elements or references while processing XML documentation
    /// comments by invoking user-defined error handlers.
    /// </summary>
    /// <remarks>
    /// This class provides a flexible error handling mechanism for XML documentation processors. It allows
    /// consumers to register custom delegates that will be invoked when specific XML documentation errors occur,
    /// such as missing include files or unresolved inheritdoc references.
    /// </remarks>
    public sealed class XmlDocErrorHandler : IXmlDocErrorHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocErrorHandler"/> class.
        /// </summary>
        public XmlDocErrorHandler()
        {
        }

        /// <summary>
        /// Gets or sets the delegate for handling unresolved <c>inheritdoc</c> element errors.
        /// </summary>
        /// <value>
        /// A delegate that handles unresolved <c>inheritdoc</c> element errors, or <see langword="null"/> if no handler is set.
        /// </value>
        /// <remarks>
        /// The delegate takes an <see cref="XElement"/> representing the <c>member</c> element containing the
        /// unresolved <c>inheritdoc</c> element. If the delegate does not throw an exception, the error is
        /// ignored by the XML documentation processor.
        /// </remarks>
        public Action<XElement>? InheritDocNotFoundError { get; set; }

        /// <summary>
        /// Gets or sets the delegate for handling missing include file errors.
        /// </summary>
        /// <value>
        /// A delegate that handles missing include file errors, or <see langword="null"/> if no handler is set.
        /// </value>
        /// <remarks>
        /// The delegate takes an <see cref="XElement"/> representing the <c>member</c> element containing the
        /// <c>include</c> element and a <see cref="string"/> representing the include file path. If the delegate
        /// does not throw an exception, the error is ignored by the XML documentation processor.
        /// </remarks>
        public Action<XElement, string>? IncludeFileNotFoundError { get; set; }

        /// <summary>
        /// Gets or sets the delegate for handling unresolved include member path errors.
        /// </summary>
        /// <value>
        /// The delegate that handles unresolved include member path errors, or <see langword="null"/> if no handler is set.
        /// </value>
        /// <remarks>
        /// The delegate takes an <see cref="XElement"/> representing the <c>member</c> element containing the
        /// unresolved <c>include</c> element. If the delegate does not throw an exception, the error is
        /// ignored by the XML documentation processor.
        /// </remarks>
        public Action<XElement>? IncludeMemberNotFoundError { get; set; }

        /// <inheritdoc/>
        void IXmlDocErrorHandler.InheritDocNotFound(XElement memberElement)
            => InheritDocNotFoundError?.Invoke(memberElement);

        /// <inheritdoc/>
        void IXmlDocErrorHandler.IncludeFileNotFound(XElement memberElement, string includeFilePath)
            => IncludeFileNotFoundError?.Invoke(memberElement, includeFilePath);

        /// <inheritdoc/>
        void IXmlDocErrorHandler.IncludeMemberNotFound(XElement memberElement)
            => IncludeMemberNotFoundError?.Invoke(memberElement);
    }
}

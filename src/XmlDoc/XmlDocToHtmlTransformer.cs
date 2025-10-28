// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using Kampute.DocToolkit.Xslt;
    using System;
    using System.Xml.Xsl;

    /// <summary>
    /// Converts the text content of XML documentation comments into HTML format using XSLT transformations.
    /// </summary>
    /// <remarks>
    /// The <see cref="XmlDocToHtmlTransformer"/> class transforms XML documentation comments into HTML format, making
    /// it suitable for web-based documentation systems, help pages, and other HTML-based documentation presentations.
    /// <para>
    /// By default, the transformer uses an embedded XSLT resource specifically designed for HTML output. You can also
    /// provide a custom XSLT transformation if you need specialized HTML formatting.
    /// </para>
    /// The <see cref="XmlDocToHtmlTransformer"/> leverages the <see cref="XsltTextTools"/> extension methods to provide
    /// additional text processing capabilities during the transformation process, such as text escaping, formatting,
    /// and whitespace normalization.
    /// </remarks>
    /// <seealso href="xmldoc-tags/xmldoc-to-html.md"/>
    public class XmlDocToHtmlTransformer : XmlDocTransformer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocToHtmlTransformer"/> class with the default XSLT transform.
        /// </summary>
        public XmlDocToHtmlTransformer()
            : this(XsltCompiler.CompileEmbeddedResource("html"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocToHtmlTransformer"/> class with the specified XSLT transform.
        /// </summary>
        /// <param name="transformer">The compiled XSLT transform for converting XML documentation into the target format.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="transformer"/> <see langword="null"/>.</exception>
        /// <seealso cref="XsltCompiler"/>
        public XmlDocToHtmlTransformer(XslCompiledTransform transformer)
            : base(transformer)
        {
            AddExtension(XsltTextTools.NamespaceUri, new XsltTextTools());
        }
    }
}

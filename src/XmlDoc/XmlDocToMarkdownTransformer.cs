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
    /// Converts the text content of XML documentation comments into Markdown format using XSLT transformations.
    /// </summary>
    /// <remarks>
    /// The <see cref="XmlDocToMarkdownTransformer"/> class transforms XML documentation comments into Markdown format,
    /// making it ideal for documentation that will be published on platforms that support Markdown such as GitHub,
    /// GitLab, Azure DevOps Wiki, Bitbucket, or any other Markdown-based documentation system.
    /// <para>
    /// By default, the transformer uses an embedded XSLT resource specifically designed for Markdown output. You can
    /// also provide a custom XSLT transformation if you need specialized Markdown formatting that adheres to particular
    /// Markdown flavors or extensions.
    /// </para>
    /// In addition to the standard capabilities, this transformer includes support for extended features through the
    /// <see cref="XsltTextTools"/> and <see cref="XsltMarkdownTools"/> extension methods, which provide specialized
    /// formatting for Markdown-specific elements beyond basic text processing.
    /// </remarks>
    /// <seealso href="xmldoc-tags/xmldoc-to-markdown"/>
    public class XmlDocToMarkdownTransformer : XmlDocTransformer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocToMarkdownTransformer"/> class with the default XSLT transform.
        /// </summary>
        public XmlDocToMarkdownTransformer()
            : this(XsltCompiler.CompileEmbeddedResource("md"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocToMarkdownTransformer"/> class with the specified XSLT transform.
        /// </summary>
        /// <param name="transformer">The compiled XSLT transform for converting XML documentation into the target format.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="transformer"/> <see langword="null"/>.</exception>
        /// <seealso cref="XsltCompiler"/>
        public XmlDocToMarkdownTransformer(XslCompiledTransform transformer)
            : base(transformer)
        {
            AddExtension(XsltTextTools.NamespaceUri, new XsltTextTools());
            AddExtension(XsltMarkdownTools.NamespaceUri, new XsltMarkdownTools());
        }
    }
}

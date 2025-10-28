// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using Kampute.DocToolkit.Xslt;
    using System;
    using System.IO;
    using System.Xml.Linq;
    using System.Xml.Xsl;

    /// <summary>
    /// Extracts and formats the text content of XML documentation comments using XSLT style-sheets.
    /// </summary>
    /// <remarks>
    /// The <see cref="XmlDocTransformer"/> class is responsible for extracting and formatting the content of XML documentation
    /// comments. The transformation process leverages XSLT (Extensible Stylesheet Language Transformations) to define the rules
    /// for converting XML documentation into the target format. This approach provides flexibility and maintainability since the
    /// transformation logic is separated from the code that processes the XML.
    /// <para>
    /// The <see cref="ReferenceResolver"/> property provides methods for resolving references within XML documentation, such as
    /// code references and language keywords. These methods are exposed within the XSLT transformation through the namespace URI
    /// <c>http://kampute.com/doc-tools/transform/xml-doc</c>.
    /// </para>
    /// Typically, you would use one of the derived classes like <see cref="XmlDocToHtmlTransformer"/> or <see cref="XmlDocToMarkdownTransformer"/>
    /// rather than using this class directly, unless you need to create a custom transformer for a specific output format.
    /// </remarks>
    public class XmlDocTransformer : IXmlDocTransformer, IXmlDocReferenceAccessor
    {
        private readonly XslCompiledTransform transformer;
        private readonly XsltArgumentList xsltArguments;
        private IXmlDocReferenceResolver? referenceResolver;

        /// <summary>
        /// The namespace URI for the reference resolver extension methods in the XSLT transformation.
        /// </summary>
        protected const string ResolverNamespaceUri = "http://kampute.com/doc-tools/transform/xml-doc";

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocTransformer"/> class.
        /// </summary>
        /// <param name="transformer">The compiled XSLT transform for converting XML documentation into the target format.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="transformer"/> <see langword="null"/>.</exception>
        /// <seealso cref="XsltCompiler"/>
        public XmlDocTransformer(XslCompiledTransform transformer)
        {
            this.transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));

            xsltArguments = new();
            xsltArguments.AddExtensionObject(ResolverNamespaceUri, new XmlDocNullReferenceResolver());
        }

        /// <summary>
        /// Gets or sets the reference resolver for resolving references in XML documentation comments.
        /// </summary>
        /// <value>
        /// The reference resolver for resolving references in XML documentation comments, or <see langword="null"/> to use none.
        /// </value>
        public IXmlDocReferenceResolver? ReferenceResolver
        {
            get => referenceResolver;
            set
            {
                if (Equals(referenceResolver, value))
                    return;

                referenceResolver = value;
                xsltArguments.RemoveExtensionObject(ResolverNamespaceUri);
                xsltArguments.AddExtensionObject(ResolverNamespaceUri, value ?? new XmlDocNullReferenceResolver());
            }
        }

        /// <summary>
        /// Extracts and formats the content of the specified XML element and writes it to the specified text writer.
        /// </summary>
        /// <param name="writer">The text writer to write the content to.</param>
        /// <param name="comment">The XML comment element to process.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="comment"/> is <see langword="null"/>.</exception>
        public void Transform(TextWriter writer, XElement comment)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (comment is null)
                throw new ArgumentNullException(nameof(comment));

            if (!comment.IsEmpty)
            {
                using var xmlReader = comment.CreateReader();
                transformer.Transform(xmlReader, xsltArguments, writer);
            }
        }

        /// <summary>
        /// Adds an extension object to the XSLT transformation using the specified namespace URI as string.
        /// </summary>
        /// <param name="namespaceUri">The namespace URI of the extension object.</param>
        /// <param name="extension">The extension object to add.</param>
        /// <exception cref="ArgumentNullException">Throw when <paramref name="namespaceUri"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="namespaceUri"/> is already has an extension object associated with it.</exception>
        protected void AddExtension(string namespaceUri, object extension) => xsltArguments.AddExtensionObject(namespaceUri, extension);
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

#pragma warning disable CA1822 // Mark members as static
namespace Kampute.DocToolkit.Xslt
{
    using Kampute.DocToolkit.Support;

    /// <summary>
    /// Provides extension methods to assist in XSLT transformations for Markdown content.
    /// </summary>
    /// <remarks>
    /// This class provides utilities for handling Markdown syntax within XSLT transformations.
    /// It helps with properly escaping special Markdown characters and managing code blocks.
    /// <para>
    /// To use these extensions in an XSLT stylesheet, include the namespace declaration:
    /// <code language="xml">&lt;xsl:stylesheet xmlns:md="http://kampute.com/doc-tools/transform/markdown"&gt;</code>
    /// Then call the methods using the namespace prefix:
    /// <code language="xml">&lt;xsl:value-of select="md:Escape(text())" /&gt;</code>
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class XsltMarkdownTools
    {
        /// <summary>
        /// The namespace URI for the Markdown extension methods.
        /// </summary>
        public const string NamespaceUri = "http://kampute.com/doc-tools/transform/markdown";

        /// <summary>
        /// Initializes a new instance of the <see cref="XsltMarkdownTools"/> class.
        /// </summary>
        public XsltMarkdownTools()
        {
        }

        /// <summary>
        /// Escapes Markdown special characters in the specified text.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <returns>The text with escaped Markdown special characters.</returns>
        public string Escape(string text) => Markdown.Encode(text, atLineStart: true);

        /// <summary>
        /// Escapes Markdown special characters in the specified text, excluding line markers.
        /// </summary>
        /// <param name="text">The text to escape.</param>
        /// <returns>The text with escaped Markdown special characters.</returns>
        public string EscapeInline(string text) => Markdown.Encode(text, atLineStart: false);

        /// <summary>
        /// Retrieves the fence marker for a code block with the specified content.
        /// </summary>
        /// <param name="code">The code block content.</param>
        /// <returns>The fence marker for the code block.</returns>
        public string FenceMarker(string code) => new('`', Markdown.GetMinimumFenceBackticks(code));
    }
}
#pragma warning restore CA1822 // Mark members as static
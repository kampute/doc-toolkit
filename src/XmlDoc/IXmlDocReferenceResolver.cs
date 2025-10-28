// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    /// <summary>
    /// Provides methods for resolving context information related to XML documentation comments.
    /// </summary>
    /// <remarks>
    /// The <see cref="IXmlDocReferenceResolver"/> interface defines a contract for components that resolve
    /// references and context information within XML documentation. The resolvers provide context-specific
    /// information that enhances the documentation with proper links and formatting.
    /// <para>
    /// This interface is typically used by XML documentation transformers during the conversion process from
    /// XML documentation comments to output formats like HTML or Markdown.
    /// </para>
    /// Implementations of this interface are responsible for:
    /// <list type="bullet">
    ///   <item><description>Resolving name and URL of code references to types and members</description></item>
    ///   <item><description>Resolving title and URL of topic references to documentation topics</description></item>
    ///   <item><description>Resolving official documentation URLs to language-specific keywords</description></item>
    ///   <item><description>Converting site-root-relative URLs to absolute or document-relative URLs</description></item>
    ///   <item><description>Formatting code blocks for display in documentation</description></item>
    ///   <item><description>Detecting and identifying programming languages for code snippets</description></item>
    /// </list>
    /// Common implementations include <see cref="XmlDocNullReferenceResolver"/> which provides no-op implementations
    /// with no actual resolution, and <see cref="XmlDocContextAwareReferenceResolver"/> which uses the current documentation
    /// context to resolve references appropriately.
    /// </remarks>
    /// <seealso cref="IXmlDocTransformer"/>
    /// <seealso cref="XmlDocNullReferenceResolver"/>
    /// <seealso cref="XmlDocContextAwareReferenceResolver"/>
    public interface IXmlDocReferenceResolver
    {
        /// <summary>
        /// Formats the specified code block for display in a documentation page.
        /// </summary>
        /// <param name="code">The code block to format.</param>
        /// <returns>The formatted code block.</returns>
        string FormatCode(string code);

        /// <summary>
        /// Gets the identifier of the programming language used for signature highlighting of a code block.
        /// </summary>
        /// <param name="code">The code block for which to determine the programming language identifier.</param>
        /// <returns>A <see cref="string"/> representing the programming language identifier.</returns>
        string GetLanguageId(string code);

        /// <summary>
        /// Gets the URL of the official documentation for a language-specific keyword.
        /// </summary>
        /// <param name="keyword">The language-specific keyword.</param>
        /// <returns>The URL of the official documentation for the specified keyword,.</returns>
        string GetKeywordUrl(string keyword);

        /// <summary>
        /// Gets the URL of the documentation for a code reference.
        /// </summary>
        /// <param name="cref">The code reference.</param>
        /// <returns>The URL of the documentation for the specified code reference.</returns>
        string GetCodeReferenceUrl(string cref);

        /// <summary>
        /// Gets the title for a code reference.
        /// </summary>
        /// <param name="cref">The code reference.</param>
        /// <returns>The title for the specified code reference.</returns>
        string GetCodeReferenceTitle(string cref);

        /// <summary>
        /// Gets the URL of the documentation for a topic reference.
        /// </summary>
        /// <param name="href">The topic reference.</param>
        /// <returns>The URL of the documentation for the specified topic reference.</returns>
        string GetTopicUrl(string href);

        /// <summary>
        /// Gets the title for a topic reference.
        /// </summary>
        /// <param name="href">The topic reference.</param>
        /// <returns>The title for the specified topic reference.</returns>
        string GetTopicTitle(string href);
    }
}

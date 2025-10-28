// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using Kampute.DocToolkit.Support;

    /// <summary>
    /// Provides a resolver that performs no resolution for XML documentation references.
    /// </summary>
    /// <remarks>
    /// The <see cref="XmlDocNullReferenceResolver"/> implements the Null Object pattern for the <see cref="IXmlDocReferenceResolver"/>
    /// interface. It serves as a non-functional reference resolver that performs minimal operations without any actual resolution
    /// capabilities.
    /// </remarks>
    /// <seealso cref="XmlDocTransformer"/>
    /// <threadsafety static="true" instance="true"/>
    public sealed class XmlDocNullReferenceResolver : IXmlDocReferenceResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocNullReferenceResolver"/> class.
        /// </summary>
        public XmlDocNullReferenceResolver()
        {
        }

        /// <summary>
        /// Formats the specified code block for display in a documentation page.
        /// </summary>
        /// <param name="code">The code block to format.</param>
        /// <returns>The normalized code block.</returns>
        /// <seealso cref="TextUtility.NormalizeCodeBlock(string, int)"/>
        public string FormatCode(string code) => TextUtility.NormalizeCodeBlock(code);

        /// <summary>
        /// Retrieves the identifier for the programming language of the specified code block.
        /// </summary>
        /// <param name="code">The code block for which to determine the programming language identifier.</param>
        /// <returns>An empty string, as no actual resolution is performed.</returns>
        public string GetLanguageId(string code) => string.Empty;

        /// <summary>
        /// Resolves the official documentation URL for the specified language keyword.
        /// </summary>
        /// <param name="keyword">The language-specific keyword.</param>
        /// <returns>An empty string, as no actual resolution is performed.</returns>
        public string GetKeywordUrl(string keyword) => string.Empty;

        /// <summary>
        /// Resolves the documentation URL for the specified code reference.
        /// </summary>
        /// <param name="cref">The code reference.</param>
        /// <returns>An empty string, as no actual resolution is performed.</returns>
        public string GetCodeReferenceUrl(string cref) => string.Empty;

        /// <summary>
        /// Retrieves the title for a code reference based on the current language context.
        /// </summary>
        /// <param name="cref">The code reference.</param>
        /// <returns>The input code reference as the title, as no actual resolution is performed.</returns>
        public string GetCodeReferenceTitle(string cref) => cref;

        /// <summary>
        /// Gets the URL of the documentation for a topic reference.
        /// </summary>
        /// <param name="href">The topic reference.</param>
        /// <returns>The input topic reference as the URL, as no actual resolution is performed.</returns>
        public string GetTopicUrl(string href) => href;

        /// <summary>
        /// Gets the title for a topic reference.
        /// </summary>
        /// <param name="href">The topic reference.</param>
        /// <returns>The input topic reference as the title, as no actual resolution is performed.</returns>
        public string GetTopicTitle(string href) => href;
    }
}

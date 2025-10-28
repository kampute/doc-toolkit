// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

#pragma warning disable CA1822 // Mark members as static
namespace Kampute.DocToolkit.Xslt
{
    using Kampute.DocToolkit.Support;

    /// <summary>
    /// Provides extension methods to assist in XSLT transformations for text content.
    /// </summary>
    /// <remarks>
    /// This class defines extension methods that can be used within XSLT transformations to manipulate text content.
    /// <para>
    /// To use these extensions in an XSLT stylesheet, include the namespace declaration:
    /// <code language="xml">&lt;xsl:stylesheet xmlns:txt="http://kampute.com/doc-tools/transform/text"&gt;</code>
    /// Then call the methods using the namespace prefix:
    /// <code language="xml">&lt;xsl:value-of select="txt:NormalizeWhitespace(text())" /&gt;</code>
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class XsltTextTools
    {
        /// <summary>
        /// The namespace URI for the text manipulation extension methods.
        /// </summary>
        public const string NamespaceUri = "http://kampute.com/doc-tools/transform/text";

        /// <summary>
        /// Initializes a new instance of the <see cref="XsltTextTools"/> class.
        /// </summary>
        public XsltTextTools()
        {
        }

        /// <summary>
        /// Replaces all sequences of whitespace characters with a single space, without trimming.
        /// </summary>
        /// <param name="text">The text to adjust white spaces for.</param>
        /// <returns>The text with normalized whitespace.</returns>
        public string NormalizeWhitespace(string text) => TextUtility.NormalizeWhitespace(text, trim: false);

        /// <summary>
        /// Replaces all occurrences of a specified string in the provided text with a new value.
        /// </summary>
        /// <param name="text">The text to search and replace in.</param>
        /// <param name="oldValue">The string to replace.</param>
        /// <param name="newValue">The string to replace <paramref name="oldValue"/> with.</param>
        /// <returns>The text with all occurrences of <paramref name="oldValue"/> replaced by <paramref name="newValue"/>.</returns>
        public string Replace(string text, string oldValue, string newValue) => text.Replace(oldValue, newValue);

        /// <summary>
        /// Trims the white spaces from the provided text.
        /// </summary>
        /// <param name="text">The text to trim.</param>
        /// <returns>The text with leading and trailing white spaces removed.</returns>
        public string Trim(string text) => text.Trim();

        /// <summary>
        /// Trims the leading white spaces from the provided text.
        /// </summary>
        /// <param name="text">The text to trim.</param>
        /// <returns>The text with leading white spaces removed.</returns>
        public string TrimStart(string text) => text.TrimStart();

        /// <summary>
        /// Trims the trailing white spaces from the provided text.
        /// </summary>
        /// <param name="text">The text to trim.</param>
        /// <returns>The text with trailing white spaces removed.</returns>
        public string TrimEnd(string text) => text.TrimEnd();
    }
}
#pragma warning restore CA1822 // Mark members as static
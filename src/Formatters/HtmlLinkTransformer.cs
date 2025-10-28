// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Formatters
{
    using Kampute.DocToolkit.Routing;
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents a text transformer that processes URLs in HTML content.
    /// </summary>
    /// <remarks>
    /// The <see cref="HtmlLinkTransformer"/> class provides functionality for transforming URLs within HTML content.
    /// It processes HTML attributes that commonly contain URLs, such as <c>href</c> and <c>src</c>, replacing the
    /// URLs according to the provided <see cref="IUrlTransformer"/>.
    /// <para>
    /// This transformer is particularly useful when generating HTML documentation where URLs need to be adjusted
    /// based on output structure or when implementing cross-references between documentation pages.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    /// <seealso cref="HtmlFormat"/>
    /// <seealso cref="ITextTransformer"/>
    /// <seealso cref="TextTransformerRegistry"/>
    /// <seealso cref="IdentityTransformer"/>
    /// <seealso cref="MarkdownLinkTransformer"/>
    public sealed partial class HtmlLinkTransformer : ITextTransformer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlLinkTransformer"/> class.
        /// </summary>
        public HtmlLinkTransformer()
        {
        }

        /// <summary>
        /// Transforms HTML content by updating URLs according to the specified URL translator.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read the source HTML from.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the transformed HTML to.</param>
        /// <param name="urlTransformer">The optional <see cref="IUrlTransformer"/> for transforming URLs in the content.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="reader"/> or <paramref name="writer"/> is <see langword="null"/>.</exception>
        public void Transform(TextReader reader, TextWriter writer, IUrlTransformer? urlTransformer = null)
        {
            if (reader is null)
                throw new ArgumentNullException(nameof(reader));
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            if (urlTransformer is null || !urlTransformer.MayTransformUrls)
            {
                IdentityTransformer.Instance.Transform(reader, writer);
                return;
            }

            var content = reader.ReadToEnd();
            content = LinkPattern.InAttributes.Replace(content, match =>
            {
                var urlString = match.Groups[2].Success
                    ? match.Groups[2].Value.Trim()
                    : match.Groups[3].Value;
                return urlTransformer.TryTransformUrl(urlString, out var replacementUrl)
                    ? match.Value.Replace(urlString, replacementUrl.ToString())
                    : match.Value;
            });
            writer.Write(content);
        }

        /// <summary>
        /// Provides regular expressions for matching HTML URL patterns.
        /// </summary>
        private static partial class LinkPattern
        {
            /// <summary>
            /// Regular expression to match HTML attributes containing URLs, handling both quoted and unquoted formats.
            /// </summary>
            public static readonly Regex InAttributes = new(@"(?:href|src)\s*=\s*(?:([""'])(.*?)\1|([^\s>]+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}
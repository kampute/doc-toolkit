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
    /// Represents a text transformer that processes URLs in Markdown content.
    /// </summary>
    /// <remarks>
    /// The <see cref="MarkdownLinkTransformer"/> class provides functionality for transforming URLs within Markdown content.
    /// It processes both inline links `[text](url "title")` and reference link definitions `[ref]: url "title"`,
    /// replacing URLs according to the provided <see cref="IUrlTransformer"/>.
    /// <para>
    /// This transformer is particularly useful when generating Markdown documentation where URLs need to be adjusted
    /// based on output structure or when implementing cross-references between documentation pages.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    /// <seealso cref="MarkdownFormat"/>
    /// <seealso cref="ITextTransformer"/>
    /// <seealso cref="TextTransformerRegistry"/>
    /// <seealso cref="IdentityTransformer"/>
    /// <seealso cref="HtmlLinkTransformer"/>
    public sealed partial class MarkdownLinkTransformer : ITextTransformer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownLinkTransformer"/> class.
        /// </summary>
        public MarkdownLinkTransformer()
        {
        }

        /// <summary>
        /// Transforms Markdown content by updating URLs according to the specified URL translator.
        /// </summary>
        /// <param name="reader">The <see cref="TextReader"/> to read the source Markdown from.</param>
        /// <param name="writer">The <see cref="TextWriter"/> to write the transformed Markdown to.</param>
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
            content = LinkPattern.Inline.Replace(content, ReplaceUrl);
            content = LinkPattern.ReferenceDefinition.Replace(content, ReplaceUrl);
            writer.Write(content);

            string ReplaceUrl(Match match)
            {
                var urlString = match.Groups[1].Value;
                return urlTransformer.TryTransformUrl(urlString, out var replacementUrl)
                    ? match.Value.Replace(urlString, replacementUrl.ToString())
                    : match.Value;
            }
        }

        /// <summary>
        /// Provides regular expressions for matching Markdown link patterns.
        /// </summary>
        private static partial class LinkPattern
        {
            /// <summary>
            /// Regular expression to match URLs in Markdown inline links: [text](URL "title")
            /// </summary>
            public static readonly Regex Inline = new(@"\[[^\]]*\]\(\s*([^\s\)]+)[^\)]*\)", RegexOptions.Compiled);

            /// <summary>
            /// Regular expression to match URLs in Markdown reference link definitions: [ref]: URL "title"
            /// </summary>
            public static readonly Regex ReferenceDefinition = new(@"^\s*\[[^\]]+\]:\s*(\S+).*?$", RegexOptions.Compiled | RegexOptions.Multiline);
        }
    }
}
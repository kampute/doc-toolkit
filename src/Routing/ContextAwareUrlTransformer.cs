// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// Resolves documentation references according to their URL scope and the current document context.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="IUrlTransformer"/> interface and is designed to work with a specific documentation context.
    /// Documentation-root-relative URLs use the <c>~/</c> marker. Site-root-relative URLs beginning with <c>/</c> and ordinary
    /// document-relative URLs retain their standard URL meaning.
    /// </remarks>
    /// <seealso cref="IUrlTransformer"/>
    /// <seealso cref="IDocumentationContext"/>
    public class ContextAwareUrlTransformer : IUrlTransformer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContextAwareUrlTransformer"/> class.
        /// </summary>
        /// <param name="context">The documentation context to use for transforming URLs.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
        public ContextAwareUrlTransformer(IDocumentationContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets the documentation context used for transforming URLs.
        /// </summary>
        /// <value>
        /// The documentation context used for transforming URLs.
        /// </value>
        public IDocumentationContext Context { get; }

        /// <summary>
        /// Gets a value indicating whether the URLs need to be transformed based on the current context and the active URL context.
        /// </summary>
        /// <value>
        /// Always <see langword="true"/>, indicating that URLs may be transformed.
        /// </value>
        public virtual bool MayTransformUrls => true;

        /// <summary>
        /// Attempts to transform a URL string into an absolute or document-relative URL.
        /// </summary>
        /// <param name="urlString">The URL string to transform.</param>
        /// <param name="transformedUrl">
        /// When this method returns, contains the transformed URL if the method returns <see langword="true"/>; otherwise, <see langword="null"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true"/> if the URL was transformed; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method enables context-aware URL resolution across documentation components, ensuring that links work correctly
        /// regardless of where they appear within the documentation hierarchy.
        /// <list type="bullet">
        ///   <item>Topic references are resolved to their corresponding URLs.</item>
        ///   <item>File references in topics are resolved to either absolute or document-relative URLs.</item>
        ///   <item>URLs beginning with <c>~/</c> are resolved from the documentation root.</item>
        /// </list>
        /// The method will not transform the provided URL when it matches any of the following:
        /// <list type="bullet">
        ///   <item>Empty or consists only of white-space.</item>
        ///   <item>A query or fragment-only URL (for example, "#section" or "?q=1").</item>
        ///   <item>An absolute or site-root-relative URL (for example, starting with a scheme like "http:" or a leading "/").</item>
        ///   <item>An unresolved document-relative URL without the <c>~/</c> marker.</item>
        /// </list>
        /// Resolving a topic-relative file reference may read file-system metadata to determine whether the referenced file exists.
        /// The operation does not create or modify files and does not change the active document URL context.
        /// </remarks>
        public virtual bool TryTransformUrl(string urlString, [NotNullWhen(true)] out Uri? transformedUrl)
        {
            if (string.IsNullOrWhiteSpace(urlString) || UriHelper.IsQueryOrFragmentOnly(urlString) || UriHelper.IsAbsoluteOrRooted(urlString))
            {
                transformedUrl = null;
                return false;
            }

            var scope = Context.AddressProvider.ActiveScope;

            // Documentation-root-relative resolution
            if (scope.TryTransformSiteRelativeUrl(urlString, out var scopedUrlString))
            {
                transformedUrl = new RawUri(scopedUrlString, UriKind.RelativeOrAbsolute);
                return true;
            }

            var (urlPath, urlSuffix) = UriHelper.SplitPathAndSuffix(urlString);
            // Topic resolution
            if (Context.Topics.TryResolve(urlPath, out var topic))
            {
                transformedUrl = string.IsNullOrEmpty(urlSuffix) ? topic.Url : topic.Url.Combine(urlSuffix);
                return true;
            }

            // Asset resolution
            if (scope.Model is TopicModel currentTopic && currentTopic.Source is IFileBasedTopic sourceTopic)
            {
                var topicDirectory = Path.GetDirectoryName(sourceTopic.FilePath) ?? string.Empty;
                if (PathHelper.TryNormalizePath(Path.Combine(topicDirectory, urlPath), out var filePath) && File.Exists(filePath))
                {
                    if (scope.RootUrl.IsAbsoluteUri)
                    {
                        transformedUrl = scope.RootUrl.Combine(urlString);
                    }
                    else
                    {
                        var relativePath = "../" + scope.RootUrl + urlString;
                        transformedUrl = currentTopic.Url.IsAbsoluteUri
                            ? new Uri(currentTopic.Url, relativePath)
                            : currentTopic.Url.Combine(relativePath);
                    }
                    return true;
                }
            }

            transformedUrl = null;
            return false;
        }
    }
}

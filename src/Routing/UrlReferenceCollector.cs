// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides a URL transformer that records all URLs that are processed through it.
    /// </summary>
    /// <remarks>
    /// This class decorates an existing <see cref="IUrlTransformer"/> instance, intercepting URL transformation requests
    /// to record all URLs that are processed. It maintains a collection of <see cref="UrlReference"/> instances representing
    /// the URLs that have been recorded, along with their associated documentation models.
    /// <para>
    /// The recorded URLs can be used to validate links, generate reports, or perform other analysis on the URLs referenced
    /// in the documentation topics or <c>&lt;see&gt;</c> or <c>&lt;seealso&gt;</c> tags with <c>href</c> attributes in XML
    /// comments.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public sealed class UrlReferenceCollector : IUrlTransformer
    {
        private readonly IDocumentationContext context;
        private readonly IUrlTransformer urlTransformer;
        private readonly ConcurrentBag<UrlReference> urls = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlReferenceCollector"/> class.
        /// </summary>
        /// <param name="context">The documentation context used to obtain the active model.</param>
        /// <param name="urlTransformer">The inner URL transformer to decorate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="urlTransformer"/> is <see langword="null"/>.</exception>
        public UrlReferenceCollector(IDocumentationContext context, IUrlTransformer urlTransformer)
        {
            this.urlTransformer = urlTransformer ?? throw new ArgumentNullException(nameof(urlTransformer));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets the collection of URLs that have been recorded so far.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="UrlReference"/> instances representing the URLs that have been recorded.
        /// </value>
        public IReadOnlyCollection<UrlReference> Urls => urls;

        /// <inheritdoc/>
        /// <remarks>
        /// Since <see cref="UrlReferenceCollector"/> class records all URLs that are processed through it, the <see cref="MayTransformUrls"/>
        /// property always returns <see langword="true"/>, regardless of the state of the underlying URL transformer.
        /// </remarks>
        public bool MayTransformUrls => true;

        /// <inheritdoc/>
        public bool TryTransformUrl(string urlString, [NotNullWhen(true)] out Uri? transformedUrl)
        {
            var scope = context.AddressProvider.ActiveScope;
            var transformed = urlTransformer.TryTransformUrl(urlString, out transformedUrl);

            if (scope.Model is not null)
                urls.Add(new UrlReference(scope, urlString, transformedUrl));

            return transformed;
        }
    }
}
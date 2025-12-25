// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using System;

    /// <summary>
    /// Represents a non-API URL that has been referenced in the documentation.
    /// </summary>
    public class UrlReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlReference"/> class using the specified scope.
        /// </summary>
        /// <param name="scope">The scope in which the URL is referenced.</param>
        /// <param name="sourceUrl">The original URL string from the documentation source (e.g., XML comment or topic).</param>
        /// <param name="targetUrl">The URI corresponding to the <paramref name="sourceUrl"/> in the generated documentation, if available.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="scope"/> or <paramref name="sourceUrl"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="scope"/> does not have an associated documentation model.</exception>
        public UrlReference(DocumentUrlContext scope, string sourceUrl, Uri? targetUrl = null)
        {
            if (scope is null)
                throw new ArgumentNullException(nameof(scope));

            ReferencingModel = scope.Model ?? throw new ArgumentException("The scope must have an associated documentation model.", nameof(scope));
            BaseDirectory = scope.Directory;
            SourceUrl = sourceUrl ?? throw new ArgumentNullException(nameof(sourceUrl));
            TargetUrl = targetUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlReference"/> class using the referencing model and base directory.
        /// </summary>
        /// <param name="referencingModel">The documentation model in which the URL is referenced.</param>
        /// <param name="baseDirectory">The directory path of the referencing model's documentation page, relative to the documentation root.</param>
        /// <param name="sourceUrl">The original URL string from the documentation source (e.g., XML comment or topic).</param>
        /// <param name="targetUrl">The URI corresponding to the <paramref name="sourceUrl"/> in the generated documentation, if available.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="referencingModel"/>, <paramref name="baseDirectory"/>, or <paramref name="sourceUrl"/> is <see langword="null"/>.</exception>
        public UrlReference(IDocumentModel referencingModel, string baseDirectory, string sourceUrl, Uri? targetUrl = null)
        {
            ReferencingModel = referencingModel ?? throw new ArgumentNullException(nameof(referencingModel));
            BaseDirectory = baseDirectory ?? throw new ArgumentNullException(nameof(baseDirectory));
            SourceUrl = sourceUrl ?? throw new ArgumentNullException(nameof(sourceUrl));
            TargetUrl = targetUrl;
        }

        /// <summary>
        /// Gets the documentation model in which the URL is referenced.
        /// </summary>
        /// <value>
        /// The documentation model in which the URL is referenced.
        /// </value>
        public IDocumentModel ReferencingModel { get; }

        /// <summary>
        /// Gets the directory path of the referencing model's documentation page, relative to the documentation root.
        /// </summary>
        /// <value>
        /// A string representing the relative directory path of the referencing model's documentation page.
        /// </value>
        /// <remarks>
        /// This path is relative to the root directory of the documentation site.
        /// <note type="hint" title="Hint">
        /// When the <see cref="TargetUrl"/> is a relative URL, it is relative to this directory.
        /// </note>
        /// </remarks>
        public string BaseDirectory { get; }

        /// <summary>
        /// Gets the original URL string from the documentation source (e.g., XML comment or topic).
        /// </summary>
        /// <value>
        /// A string representing the source URL.
        /// </value>
        public string SourceUrl { get; }

        /// <summary>
        /// Gets the URL corresponding to the <see cref="SourceUrl"/> in the generated documentation.
        /// </summary>
        /// <value>
        /// A <see cref="Uri"/> representing the target URL, or <see langword="null"/> if the URL could not be resolved.
        /// </value>
        /// <remarks>
        /// If the <see cref="SourceUrl"/> could not be resolved to a URL of an internal resource, this property will be <see langword="null"/>.
        /// Common reasons for a <see langword="null"/> value include:
        /// <list type="bullet">
        ///   <item><description>The source string is not a well-formed absolute or relative URI.</description></item>
        ///   <item><description>The source contains only a fragment identifier or only a query string with no path to resolve.</description></item>
        ///   <item><description>The source points to a URL outside the scope of the documentation set.</description></item>
        /// </list>
        /// When the <see cref="TargetUrl"/> is a relative URL, it is relative to the directory of the referencing model's
        /// documentation page as indicated by the <see cref="BaseDirectory"/> property.
        /// <para>
        /// <note type="caution" title="Caution">
        /// A non-<see langword="null"/> <see cref="TargetUrl"/> indicates a resolved URL, but it does not guarantee that the URL points to
        /// an  existing resource within the documentation. This is because the target resource may not have been generated yet or might not
        /// be included in the documentation set at the time of URL resolution.
        /// </note>
        /// </para>
        /// </remarks>
        public Uri? TargetUrl { get; }

        /// <summary>
        /// Returns a string that represents the current <see cref="UrlReference"/>.
        /// </summary>
        /// <returns>A string that represents the current <see cref="UrlReference"/>.</returns>
        public override string ToString() => SourceUrl;
    }
}
// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides constants for common file extensions used in documentation tools.
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// The file extensions for Markdown files.
        /// </summary>
        /// <remarks>
        /// The underlying collection is case-sensitive.
        /// </remarks>
        public static readonly IReadOnlyCollection<string> MarkdownExtensions = [".md", ".markdown"];

        /// <summary>
        /// The file extensions for HTML files.
        /// </summary>
        /// <remarks>
        /// The underlying collection is case-sensitive.
        /// </remarks>
        public static readonly IReadOnlyCollection<string> HtmlExtensions = [".html", ".htm", ".xhtml"];

        /// <summary>
        /// The default file extension for Markdown files.
        /// </summary>
        public const string Markdown = ".md";

        /// <summary>
        /// The default file extension for HTML files.
        /// </summary>
        public const string Html = ".html";
    }
}

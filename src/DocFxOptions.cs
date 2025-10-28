// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Routing;

    /// <summary>
    /// Provides options for generating documentation addresses compatible with DocFx.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DocFxOptions"/> class provides configuration for documentation file addressing
    /// specifically tailored for DocFx documentation generator compatibility.
    /// </para>
    /// This class implements the following default configuration:
    /// <list type="bullet">
    ///   <item><description>Sets file extension to HTML (<c>.html</c>)</description></item>
    ///   <item><description>Includes file extensions in generated URLs</description></item>
    ///   <item><description>Uses the <c>api</c> path for API documentation</description></item>
    ///   <item><description>Places top-level topic files in the root directory</description></item>
    /// </list>
    /// These settings align with DocFx's conventional file organization and URL structure.
    /// </remarks>
    /// <seealso cref="DocFxStrategy"/>
    public class DocFxOptions : HtmlAddressingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocFxOptions"/> class.
        /// </summary>
        public DocFxOptions() : base()
        {
            ApiPath = "api"; // DocFx places API documentation under the "api" path
            TopicPath = string.Empty; // DocFx places topic files in the root directory
            OmitExtensionInUrls = false; // DocFx includes file extensions in URLs
        }
    }
}

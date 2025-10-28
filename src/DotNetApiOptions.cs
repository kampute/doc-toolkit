// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Routing;

    /// <summary>
    /// Provides options for generating documentation addresses compatible with .NET API documentation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DotNetApiOptions"/> class provides configuration for documentation file addressing
    /// specifically tailored to match the structure of official .NET API documentation.
    /// </para>
    /// This class implements the following default configuration:
    /// <list type="bullet">
    ///   <item><description>Sets file extension to HTML (<c>.html</c>)</description></item>
    ///   <item><description>Includes file extensions in generated URLs</description></item>
    ///   <item><description>Uses the <c>api</c> path for API documentation</description></item>
    ///   <item><description>Places top-level topic files in the root directory</description></item>
    /// </list>
    /// While official .NET documentation URLs don't have file extensions, this implementation includes
    /// them for better web server compatibility by default.
    /// </remarks>
    /// <seealso cref="DotNetApiStrategy"/>
    public class DotNetApiOptions : HtmlAddressingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetApiOptions"/> class.
        /// </summary>
        public DotNetApiOptions() : base()
        {
            ApiPath = "api"; // .NET API documentation is placed under the "api" path
            TopicPath = string.Empty; // .NET topics are typically placed under the root directory
            OmitExtensionInUrls = false; // Although .NET documentation URLs are without extensions, this option is set to false for web server compatibility
        }
    }
}

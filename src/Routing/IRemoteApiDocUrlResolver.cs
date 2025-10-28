// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using System;

    /// <summary>
    /// Defines a contract for resolving URLs to documentation of code elements that are hosted remotely.
    /// </summary>
    /// <remarks>
    /// This resolver is responsible for mapping code elements to their corresponding documentation URLs for external
    /// documentation sources.
    /// </remarks>
    public interface IRemoteApiDocUrlResolver : IApiDocUrlProvider
    {
        /// <summary>
        /// Gets the base URL of the documentation site.
        /// </summary>
        /// <value>
        /// A <see cref="Uri"/> representing the base URL of the documentation site.
        /// </value>
        Uri SiteUrl { get; }

        /// <summary>
        /// Determines whether the specified namespace is covered by this URL resolver.
        /// </summary>
        /// <param name="ns">The namespace to check for.</param>
        /// <returns><see langword="true"/> if the namespace is covered by this URL resolver; otherwise, <see langword="false"/>.</returns>
        bool SupportsNamespace(string ns);
    }
}

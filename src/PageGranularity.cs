// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using System;

    /// <summary>
    /// Specifies how documentation content is organized into addressable pages, affecting URLs and navigation structure.
    /// </summary>
    /// <remarks>
    /// The page granularity is a key architectural decision that impacts the entire documentation structure. The chosen granularity
    /// affects navigation patterns, URL structure, search engine optimization, and overall user experience. Different granularity
    /// levels offer trade-offs between content density and navigational complexity.
    /// </remarks>
    [Flags]
    public enum PageGranularity
    {
        /// <summary>
        /// No page granularity, resulting in all content being documented within the same page.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that namespaces have their own documentation pages.
        /// </summary>
        Namespace = 1,

        /// <summary>
        /// Indicates that types have their own documentation pages.
        /// </summary>
        Type = 2,

        /// <summary>
        /// Indicates that type members have their own documentation pages.
        /// </summary>
        Member = 4,

        /// <summary>
        /// Indicates that namespaces and types have their own documentation pages, but type members are documented within their containing type pages.
        /// </summary>
        NamespaceType = Namespace | Type,

        /// <summary>
        /// Indicates that namespaces, types, and type members all have their own documentation pages.
        /// </summary>
        NamespaceTypeMember = Namespace | Type | Member,
    }
}
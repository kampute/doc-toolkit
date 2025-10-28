// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides a mapping between references and their corresponding URLs.
    /// </summary>
    /// <remarks>
    /// The <see cref="IUrlTransformer"/> interface addresses a key challenge in documentation systems: maintaining valid
    /// cross-references when URL structures change. It serves as a mapping layer between source paths and their target
    /// URLs, allowing documentation to adapt to format changes or different addressing strategies without breaking existing
    /// references.
    /// <para>
    /// When documentation topics reference other topics, the actual URLs may change due to format changes or shifts in
    /// addressing strategy. This interface ensures that references remain valid by providing a centralized mapping mechanism
    /// that translates between different addressing schemes.
    /// </para>
    /// </remarks>
    public interface IUrlTransformer
    {
        /// <summary>
        /// Gets a value indicating whether the URL transformer is active and performs any URL transformations.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the transformer is active and may transform a URL; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This property indicates whether the URL transformer has any active transformation rules. If there are no active rules,
        /// the transformer will not perform any transformations, and the transformation methods will always return <see langword="false"/>.
        /// In such cases, the transformer may be skipped to improve performance.
        /// </remarks>
        bool MayTransformUrls { get; }

        /// <summary>
        /// Attempts to transform a URL string to a different URL.
        /// </summary>
        /// <param name="urlString">The URL string to transform.</param>
        /// <param name="transformedUrl">
        /// When this method returns, contains the transformed URL if the method returns <see langword="true"/>; otherwise, <see langword="null"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true"/> if the URL was transformed; otherwise, <see langword="false"/>.</returns>
        bool TryTransformUrl(string urlString, [NotNullWhen(true)] out Uri? transformedUrl);
    }
}
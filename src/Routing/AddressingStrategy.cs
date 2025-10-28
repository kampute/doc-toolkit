// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Represents a base class for defining strategies to organize and address documentation pages.
    /// </summary>
    /// <remarks>
    /// The <see cref="AddressingStrategy"/> class provides a foundation for implementing various documentation
    /// addressing schemes. It handles common functionality like file extension management and the creation of
    /// appropriate resource addresses based on whether extensions should appear in URLs. This abstraction allows
    /// the documentation system to support multiple addressing patterns while maintaining consistent behavior
    /// for extension handling, URL generation, and path resolution across different documentation formats and
    /// publishing targets.
    /// </remarks>
    public abstract class AddressingStrategy : IDocumentAddressingStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddressingStrategy"/> class.
        /// </summary>
        /// <param name="options">The addressing options that configure how documentation pages are addressed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
        protected AddressingStrategy(AddressingOptions options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            FileExtension = options.FileExtension;
            OmitExtensionInUrls = options.OmitExtensionInUrls;
        }

        /// <summary>
        /// Gets the file extension to be used for documentation files.
        /// </summary>
        /// <value>
        /// The file extension (including the leading period) to be used for documentation files.
        /// </value>
        public string FileExtension { get; }

        /// <summary>
        /// Gets a value indicating whether the file extension should be excluded from URLs.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the file extension should be omitted in URLs; otherwise, <see langword="false"/>.
        /// </value>
        public bool OmitExtensionInUrls { get; }

        /// <summary>
        /// Gets the page granularity used for organizing documentation content.
        /// </summary>
        /// <value>
        /// The granularity that specifies which code elements (namespaces, types, members) receive their own dedicated pages
        /// in the documentation system.
        /// </value>
        public abstract PageGranularity Granularity { get; }

        /// <inheritdoc/>
        public abstract bool TryResolveNamespaceAddress(string ns, [NotNullWhen(true)] out IResourceAddress? address);

        /// <inheritdoc/>
        public abstract bool TryResolveMemberAddress(IMember member, [NotNullWhen(true)] out IResourceAddress? address);

        /// <inheritdoc/>
        public abstract bool TryResolveTopicAddress(ITopic topic, [NotNullWhen(true)] out IResourceAddress? address);

        /// <inheritdoc/>
        public virtual bool IsAddressable(IMember member)
        {
            return member is not null
                && member.IsDirectDeclaration == true
                && member.IsCompilerGenerated == false
                && member is not IField { IsEnumValue: true };
        }

        /// <summary>
        /// Builds the address for the specified path without extension and the optional URL suffix.
        /// </summary>
        /// <param name="pathWithoutExtension">The path without the file extension.</param>
        /// <param name="fragmentIdentifier">The optional fragment identifier to append to the URL.</param>
        /// <returns>A <see cref="IResourceAddress"/> instance representing the address.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="pathWithoutExtension"/> is <see langword="null"/> or empty.</exception>
        protected virtual IResourceAddress CreateAddressFromPath(string pathWithoutExtension, string? fragmentIdentifier = null)
        {
            if (string.IsNullOrEmpty(pathWithoutExtension))
                throw new ArgumentException($"'{nameof(pathWithoutExtension)}' cannot be null or empty.", nameof(pathWithoutExtension));

            var filePath = pathWithoutExtension + FileExtension;
            var urlString = OmitExtensionInUrls ? pathWithoutExtension : filePath;
            if (!string.IsNullOrEmpty(fragmentIdentifier))
            {
                urlString += '#' + fragmentIdentifier;
                if (!Granularity.HasFlag(PageGranularity.Member))
                    filePath = null;
            }

            return new ResourceAddress(urlString, filePath);
        }

        /// <summary>
        /// Creates a resource address from the specified path segments and an optional URL suffix.
        /// </summary>
        /// <param name="pathSegments">The segments that make up the path.</param>
        /// <param name="fragmentIdentifier">The optional fragment identifier to append to the URL.</param>
        /// <returns>A <see cref="IResourceAddress"/> instance representing the address.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="pathSegments"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="pathSegments"/> is empty or contains only empty segments.</exception>
        protected IResourceAddress CreateAddressFromPathSegments(IEnumerable<string> pathSegments, string? fragmentIdentifier = null)
        {
            if (pathSegments is null)
                throw new ArgumentNullException(nameof(pathSegments));

            var pathWithoutExtension = string.Join('/', pathSegments.Where(static segment => !string.IsNullOrEmpty(segment)));
            if (string.IsNullOrEmpty(pathWithoutExtension))
                throw new ArgumentException($"'{nameof(pathSegments)}' cannot be empty or contain only empty segments.", nameof(pathSegments));

            return CreateAddressFromPath(pathWithoutExtension, fragmentIdentifier);
        }
    }
}

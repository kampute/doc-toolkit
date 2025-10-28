// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A wrapper for <see cref="IXmlDocProvider"/> that adds caching support to improve performance for repeated queries.
    /// </summary>
    /// <remarks>
    /// The <see cref="XmlDocProviderCache"/> wraps an existing <see cref="IXmlDocProvider"/> instance and caches the results of documentation
    /// queries. This can significantly improve performance when the same documentation is requested multiple times.
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public sealed class XmlDocProviderCache : IXmlDocProvider
    {
        private readonly IXmlDocProvider innerProvider;
        private readonly ConcurrentDictionary<string, XmlDocEntry?> cache = new(StringComparer.Ordinal);

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocProviderCache"/> class.
        /// </summary>
        /// <param name="provider">The inner <see cref="IXmlDocProvider"/> to wrap and cache.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider"/> is <see langword="null"/>.</exception>
        public XmlDocProviderCache(IXmlDocProvider provider)
        {
            innerProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Gets a value indicating whether the XML documentation provider has any documentation available.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the provider contains documentation; otherwise, <see langword="false"/>.
        /// </value>
        public bool HasDocumentation => innerProvider.HasDocumentation;

        /// <summary>
        /// Attempts to retrieve the XML documentation for the specified code reference.
        /// </summary>
        /// <param name="cref">The code reference to retrieve the documentation for.</param>
        /// <param name="doc">
        /// When this method returns, contains the <see cref="XmlDocEntry"/> representing the documentation for the code reference,
        /// if the documentation is available; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if the documentation is available; otherwise, <see langword="false"/>.</returns>
        public bool TryGetDoc(string cref, [NotNullWhen(true)] out XmlDocEntry? doc)
        {
            if (string.IsNullOrWhiteSpace(cref))
            {
                doc = null;
                return false;
            }

            if (cache.TryGetValue(cref, out doc))
                return doc is not null;

            var found = innerProvider.TryGetDoc(cref, out doc);
            cache[cref] = doc;
            return found;
        }
    }
}
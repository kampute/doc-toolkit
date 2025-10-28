// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    /// <summary>
    /// Provides metadata abstraction interfaces and adapters for unified access to reflection objects.
    /// </summary>
    /// <remarks>
    /// This namespace contains interfaces and implementations that provide a unified abstraction layer over .NET reflection
    /// objects. This abstraction enables consistent access to assembly, type, and member metadata regardless of whether the
    /// assembly was loaded via Common Language Runtime (CLR) or through MetadataLoadContext (MLC).
    /// <para>
    /// The key benefits of this abstraction layer include:
    /// <list type="bullet">
    ///   <item><description>Cross-loading context compatibility for metadata access</description></item>
    ///   <item><description>Consistent equality semantics based on metadata identity rather than object references</description></item>
    ///   <item><description>Type-safe access to reflection metadata with compile-time checking</description></item>
    ///   <item><description>Performance optimization through lazy loading and caching</description></item>
    ///   <item><description>Extensibility for additional metadata sources in the future</description></item>
    /// </list>
    /// </para>
    /// Use <see cref="MetadataProvider"/> to create appropriate metadata abstractions from reflection objects.
    /// </remarks>
    internal static class NamespaceDoc
    {
        // This class doesn't contain any code
        // It exists only to hold the namespace documentation
    }
}

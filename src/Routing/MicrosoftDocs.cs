// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Provides URLs to the Microsoft Docs documentation site for .NET code elements.
    /// </summary>
    /// <remarks>
    /// This class resolves URLs for .NET code elements by linking to <see href="https://learn.microsoft.com/dotnet/api">the
    /// official Microsoft Docs API documentation</see>. By default, this resolver is configured to handle code elements in
    /// namespaces matching the following patterns:
    /// <list type="bullet">
    ///   <item><description><c>System.*</c></description></item>
    ///   <item><description><c>Microsoft.*</c></description></item>
    ///   <item><description><c>Windows.*</c></description></item>
    ///   <item><description><c>Visibility.*</c></description></item>
    /// </list>
    /// </remarks>
    public sealed class MicrosoftDocs : StrategyBasedApiDocUrlResolver
    {
        private readonly ConcurrentDictionary<string, Uri?> cache = new(StringComparer.Ordinal);

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftDocs"/> class.
        /// </summary>
        public MicrosoftDocs()
            : base(new("https://learn.microsoft.com/dotnet/"), new DotNetApiStrategy(new DotNetApiOptions { OmitExtensionInUrls = true }))
        {
            NamespacePatterns.Add("System.*");
            NamespacePatterns.Add("Microsoft.*");
            NamespacePatterns.Add("Windows.*");
            NamespacePatterns.Add("Visibility.*");
        }

        /// <inheritdoc/>
        protected override Uri? ResolveMemberUrl(IMember member) => member is not null && Strategy.IsAddressable(member)
            ? cache.GetOrAdd(member.CodeReference, _ => base.ResolveMemberUrl(member))
            : null;

        /// <inheritdoc/>
        protected override Uri? ResolveNamespaceUrl(string ns) => !string.IsNullOrWhiteSpace(ns)
            ? cache.GetOrAdd(ns, _ => base.ResolveNamespaceUrl(ns))
            : null;
    }
}

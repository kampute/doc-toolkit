// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Capabilities;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides an addressing strategy for organizing and addressing documentation pages similar to the .NET API documentation
    /// structure hosted on Microsoft Learn.
    /// </summary>
    /// <remarks>
    /// The <see cref="DotNetApiStrategy"/> class provides an addressing strategy that mimics the structure of the .NET API documentation
    /// hosted on Microsoft Learn.
    /// <para>
    /// The strategy organizes API documentation with namespaces, types, and type members documented on dedicated pages with flat
    /// addressing and method overloads differentiated by URL fragments.
    /// </para>
    /// <para>
    /// Topic organization follows a hierarchical structure based on parent-child relationships. Topics with subtopics use index
    /// files to serve as landing pages, with topic paths using lowercase naming while replacing invalid file system characters
    /// with hyphens. Pinned topics can override default paths for custom organization.
    /// </para>
    /// Key features of the <see cref="DotNetApiStrategy"/> addressing strategy include:
    /// <list type="bullet">
    ///   <item><description>.NET API-style addressing with dedicated pages for namespaces, types, and individual type members</description></item>
    ///   <item><description>URL fragments for method overloads for direct navigation to specific overloads</description></item>
    ///   <item><description>Hierarchical topic organization and pinned topic functionality for custom path overrides</description></item>
    /// </list>
    /// This strategy is ideal for large, complex APIs where granular addressing enables direct navigation to specific members,
    /// making documentation easier to reference and maintain.
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    /// <seealso href="https://learn.microsoft.com/dotnet/api">Microsoft .NET API documentation</seealso>
    public class DotNetApiStrategy : HtmlAddressingStrategy
    {
        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetApiStrategy"/> class.
        /// </summary>
        /// <seealso cref="DotNetApiOptions"/>
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetApiStrategy"/> class with default options.
        /// </summary>
        public DotNetApiStrategy()
            : this(new DotNetApiOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetApiStrategy"/> class with the specified options.
        /// </summary>
        /// <param name="options">The .NET API addressing options.</param>
        public DotNetApiStrategy(DotNetApiOptions options)
            : base(options)
        {
        }

        /// <inheritdoc/>
        public override PageGranularity Granularity => PageGranularity.NamespaceTypeMember;

        /// <inheritdoc/>
        public override bool TryResolveNamespaceAddress(string ns, [NotNullWhen(true)] out IResourceAddress? address)
        {
            if (string.IsNullOrWhiteSpace(ns))
            {
                address = null;
                return false;
            }

            var path = GetApiPath(ns.ToLowerInvariant());
            address = CreateAddressFromPath(path);
            return true;
        }

        /// <inheritdoc/>
        public override bool TryResolveMemberAddress(IMember member, [NotNullWhen(true)] out IResourceAddress? address)
        {
            if (!IsAddressable(member))
            {
                address = null;
                return false;
            }

            if (member is IType type)
            {
                var typePath = type.Signature.Replace('`', '-').ToLowerInvariant();
                address = CreateAddressFromPath(GetApiPath(typePath));
                return true;
            }

            var signature = ToNormalizedSignature(member.CodeReference!);

            var memberPath = signature;
            if (member is IWithParameters { HasParameters: true })
                memberPath = signature.SubstringBeforeOrSelf('(');
            if (member is IMethod { IsGenericMethod: true } && memberPath.LastIndexOf("--") is int i and > 0)
                memberPath = memberPath[..i];

            var memberAnchor = string.Empty;
            if (member is IWithOverloads { HasOverloads: true })
                memberAnchor = signature.ReplaceChars(['.', '_'], '-').Replace("--", "-");

            address = CreateAddressFromPath(GetApiPath(memberPath), memberAnchor);
            return true;
        }

        /// <inheritdoc/>
        public override bool TryResolveTopicAddress(ITopic topic, [NotNullWhen(true)] out IResourceAddress? address)
        {
            if (topic is null)
            {
                address = null;
                return false;
            }

            var path = GetTopicPath(topic, '-').ToLower();
            address = CreateAddressFromPath(path);
            return true;
        }

        /// <summary>
        /// Converts the given code reference to a normalized signature suitable for use in URLs.
        /// </summary>
        /// <param name="codeReference">The code reference to extract and normalize the signature from.</param>
        /// <returns>The normalized signature.</returns>
        private static string ToNormalizedSignature(ReadOnlySpan<char> codeReference)
        {
            var size = codeReference.Length - 2;
            Span<char> signature = stackalloc char[size];

            for (var i = 0; i < size; ++i)
            {
                var c = codeReference[i + 2];
                signature[i] = c is '`' or '#' or ',' or '~' or '{' or '}' ? '-' : char.ToLowerInvariant(c);
            }

            return signature.ToString();
        }
    }
}

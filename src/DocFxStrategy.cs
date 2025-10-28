// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides an addressing strategy for organizing and addressing documentation pages in a manner similar to the DocFX
    /// documentation generator.
    /// </summary>
    /// <remarks>
    /// The <see cref="DocFxStrategy"/> class provides an addressing strategy that mimics the organization and conventions of
    /// the DocFX documentation generator.
    /// <para>
    /// The strategy organizes API documentation with namespaces and types documented on separate pages, while type members
    /// such as methods, properties, fields, and events are consolidated on their declaring type's page. URL fragments enable
    /// direct navigation to specific members within consolidated pages, maintaining clean URLs while preserving precise
    /// addressing.
    /// </para>
    /// <para>
    /// Topic organization follows a hierarchical structure based on parent-child relationships. Topics with subtopics use index
    /// files to serve as landing pages, with topic paths preserving original naming while replacing invalid file system characters
    /// with underscores. Pinned topics can override default paths for custom organization.
    /// </para>
    /// Key features of the <see cref="DocFxStrategy"/> addressing strategy include:
    /// <list type="bullet">
    ///   <item><description>DocFX-compatible path structure where type members are documented on the same page as their containing type</description></item>
    ///   <item><description>URL fragments for direct navigation to specific members within type pages</description></item>
    ///   <item><description>Hierarchical topic organization and pinned topic functionality for custom path overrides</description></item>
    /// </list>
    /// This strategy is ideal for small to medium-sized APIs where consolidated member documentation improves readability,
    /// and for projects maintaining compatibility with existing DocFX-generated documentation.
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    /// <seealso href="https://github.com/dotnet/docfx">DocFX on GitHub</seealso>
    public class DocFxStrategy : HtmlAddressingStrategy
    {
        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="DocFxStrategy"/> class.
        /// </summary>
        /// <seealso cref="DocFxOptions"/>
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="DocFxStrategy"/> class with default options.
        /// </summary>
        public DocFxStrategy()
            : this(new DocFxOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocFxOptions"/> class with the specified options.
        /// </summary>
        /// <param name="options">The DocFx addressing options.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
        public DocFxStrategy(DocFxOptions options)
            : base(options)
        {
        }

        /// <inheritdoc/>
        public override PageGranularity Granularity => PageGranularity.NamespaceType;

        /// <inheritdoc/>
        public override bool TryResolveNamespaceAddress(string ns, [NotNullWhen(true)] out IResourceAddress? address)
        {
            if (string.IsNullOrWhiteSpace(ns))
            {
                address = null;
                return false;
            }

            var path = GetApiPath(ns);
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

            var memberAnchor = string.Empty;
            if (member is not IType type)
            {
                type = member.DeclaringType!;
                memberAnchor = member.CodeReference[2..].ReplaceMany(['.', '`', '#', ',', '(', ')', '~'], '_');
            }

            var typePath = type.Signature.Replace('`', '_');
            address = CreateAddressFromPath(GetApiPath(typePath), memberAnchor);
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

            var path = GetTopicPath(topic, '_');
            address = CreateAddressFromPath(path);
            return true;
        }

        /// <inheritdoc/>
        public override bool IsAddressable(IMember member) => base.IsAddressable(member) && member is not IVirtualTypeMember { IsExplicitInterfaceImplementation: true };
    }
}

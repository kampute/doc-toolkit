// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.XmlDoc.Comments;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    /// <summary>
    /// Represents XML documentation for a namespace, type, or type member.
    /// </summary>
    /// <remarks>
    /// Provides structured access to XML documentation comments extracted from source code. Parses standard XML documentation tags for consumption by documentation generators.
    /// <para>
    /// Each instance represents complete documentation for a single code element and provides lazy-loaded access to documentation sections.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    /// <seealso href="xmldoc-tags"/>
    public class XmlDocEntry
    {
        /// <summary>
        /// Represents an empty documentation instance.
        /// </summary>
        public static readonly XmlDocEntry Empty = new(new("member"));

        private readonly Lazy<Comment> summary;
        private readonly Lazy<Comment> remarks;
        private readonly Lazy<Comment> example;
        private readonly Lazy<List<ReferenceComment>> exceptions;
        private readonly Lazy<List<ReferenceComment>> permissions;
        private readonly Lazy<List<ReferenceComment>> events;
        private readonly Lazy<List<SeeAlsoComment>> seeAlso;
        private readonly Lazy<ThreadSafetyComment> threadSafety;

        private readonly Lazy<Dictionary<string, Comment>> typeParameters;
        private readonly Lazy<Dictionary<string, Comment>> parameters;
        private readonly Lazy<Comment> returnDescription;
        private readonly Lazy<Comment> valueDescription;

        private readonly Lazy<XmlDocEntry> overloads;
        private readonly Lazy<XmlDocEntry> extensionBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocEntry"/> class.
        /// </summary>
        /// <param name="element">The XML element containing the documentation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> is <see langword="null"/>.</exception>
        public XmlDocEntry(XElement element)
        {
            XmlElement = element ?? throw new ArgumentNullException(nameof(element));

            summary = new(() => Comment.Create(element.Element("summary")));
            remarks = new(() => Comment.Create(element.Element("remarks")));
            example = new(() => Comment.Create(element.Element("example")));
            threadSafety = new(() => ThreadSafetyComment.Create(element.Element("threadsafety")));

            typeParameters = new(() => Comment.Collect(element.Elements("typeparam")));
            parameters = new(() => Comment.Collect(element.Elements("param")));
            returnDescription = new(() => Comment.Create(element.Element("returns")));
            valueDescription = new(() => Comment.Create(element.Element("value") ?? element.Element("returns")));

            exceptions = new(() => [.. ReferenceComment.Collect(element.Elements("exception"))]);
            permissions = new(() => [.. ReferenceComment.Collect(element.Elements("permission"))]);
            events = new(() => [.. ReferenceComment.Collect(element.Elements("event"))]);
            seeAlso = new(() => [.. SeeAlsoComment.Collect(element.Elements("seealso"))]);

            overloads = new(() => element.Element("overloads") is XElement { IsEmpty: false } overloadsElement
                ? new XmlDocEntry(overloadsElement) { Context = Context }
                : Empty);

            extensionBlock = new(() => element.Element("extensionblock") is XElement { IsEmpty: false } extensionBlockElement
                ? new XmlDocEntry(extensionBlockElement) { Context = Context }
                : Empty);
        }

        /// <summary>
        /// Initializes a new instance associated with the specified documentation context.
        /// </summary>
        /// <param name="other">The instance to copy.</param>
        /// <param name="context">The documentation context to associate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
        protected XmlDocEntry(XmlDocEntry other, IDocumentationContext context)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other));
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            Context = context;

            XmlElement = other.XmlElement;
            summary = other.summary;
            remarks = other.remarks;
            example = other.example;
            threadSafety = other.threadSafety;
            typeParameters = other.typeParameters;
            parameters = other.parameters;
            returnDescription = other.returnDescription;
            valueDescription = other.valueDescription;
            exceptions = other.exceptions;
            permissions = other.permissions;
            events = other.events;
            seeAlso = other.seeAlso;

            overloads = other.overloads.IsValueCreated && !ReferenceEquals(other.overloads.Value, Empty)
                ? new(() => new XmlDocEntry(other.overloads.Value, context))
                : other.overloads;

            extensionBlock = other.extensionBlock.IsValueCreated && !ReferenceEquals(other.extensionBlock.Value, Empty)
                ? new(() => new XmlDocEntry(other.extensionBlock.Value, context))
                : other.extensionBlock;
        }

        /// <summary>
        /// Gets the documentation context associated with this instance.
        /// </summary>
        /// <value>
        /// The associated documentation context, or <see langword="null"/> if no context is associated.
        /// </value>
        public IDocumentationContext? Context { get; private set; }

        /// <summary>
        /// Gets the underlying XML element.
        /// </summary>
        /// <value>
        /// The XML element containing the documentation.
        /// </value>
        protected XElement XmlElement { get; }

        /// <summary>
        /// Gets a value indicating whether the documentation is empty.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the documentation is empty; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsEmpty => XmlElement.IsEmpty;

        /// <summary>
        /// Gets the summary.
        /// </summary>
        /// <value>
        /// The summary documentation.
        /// </value>
        public Comment Summary => summary.Value;

        /// <summary>
        /// Gets the remarks.
        /// </summary>
        /// <value>
        /// The remarks documentation.
        /// </value>
        public Comment Remarks => remarks.Value;

        /// <summary>
        /// Gets the usage example.
        /// </summary>
        /// <value>
        /// The example demonstrating usage.
        /// </value>
        public Comment Example => example.Value;

        /// <summary>
        /// Gets exceptions that might be thrown.
        /// </summary>
        /// <value>
        /// A collection describing each exception that might be thrown.
        /// </value>
        public IReadOnlyList<ReferenceComment> Exceptions => exceptions.Value;

        /// <summary>
        /// Gets required permissions.
        /// </summary>
        /// <value>
        /// A collection describing each required permission.
        /// </value>
        public IReadOnlyList<ReferenceComment> Permissions => permissions.Value;

        /// <summary>
        /// Gets events that might be raised.
        /// </summary>
        /// <value>
        /// A collection describing each event that might be raised.
        /// </value>
        public IReadOnlyList<ReferenceComment> Events => events.Value;

        /// <summary>
        /// Gets related references.
        /// </summary>
        /// <value>
        /// A collection of related references and links.
        /// </value>
        public IEnumerable<SeeAlsoComment> SeeAlso => Context is not null ? seeAlso.Value.Select(seeAlso => seeAlso.WithResolvedHyperlink(Context)) : seeAlso.Value;

        /// <summary>
        /// Gets thread safety information.
        /// </summary>
        /// <value>
        /// The thread safety details.
        /// </value>
        public ThreadSafetyComment ThreadSafety => threadSafety.Value;

        /// <summary>
        /// Gets type parameter descriptions.
        /// </summary>
        /// <value>
        /// A dictionary mapping type parameter names to their descriptions.
        /// </value>
        public IReadOnlyDictionary<string, Comment> TypeParameters => typeParameters.Value;

        /// <summary>
        /// Gets parameter descriptions.
        /// </summary>
        /// <value>
        /// A dictionary mapping parameter names to their descriptions.
        /// </value>
        public IReadOnlyDictionary<string, Comment> Parameters => parameters.Value;

        /// <summary>
        /// Gets the value description for properties.
        /// </summary>
        /// <value>
        /// The description of the property value.
        /// </value>
        public Comment ValueDescription => valueDescription.Value;

        /// <summary>
        /// Gets the return value description.
        /// </summary>
        /// <value>
        /// The description of the return value.
        /// </value>
        public Comment ReturnDescription => returnDescription.Value;

        /// <summary>
        /// Gets common documentation for overloads of a member.
        /// </summary>
        /// <value>
        /// The shared documentation for all overloads of the member.
        /// </value>
        public XmlDocEntry Overloads => overloads.Value;

        /// <summary>
        /// Gets the documentation for the extension block of an extension member.
        /// </summary>
        /// <value>
        /// The extension block documentation for the extension member.
        /// </value>
        public XmlDocEntry ExtensionBlock => extensionBlock.Value;

        /// <summary>
        /// Creates a copy associated with the specified documentation context.
        /// </summary>
        /// <param name="context">The documentation context to associate.</param>
        /// <returns>A new instance associated with the specified context, or the current instance if this is the <see cref="Empty"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlDocEntry WithContext(IDocumentationContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            return ReferenceEquals(this, Empty) || ReferenceEquals(Context, context) ? this : new(this, context);
        }
    }
}

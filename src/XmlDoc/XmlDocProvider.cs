// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Capabilities;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    /// <summary>
    /// Provides XML documentation for .NET types and members by parsing and resolving documentation files.
    /// </summary>
    public class XmlDocProvider : IXmlDocProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocProvider"/> class.
        /// </summary>
        /// <param name="resolver">The XML documentation resolver to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="resolver"/> is <see langword="null"/>.</exception>
        public XmlDocProvider(IXmlDocResolver resolver)
        {
            Resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        /// <summary>
        /// Gets the XML documentation resolver used by this provider.
        /// </summary>
        /// <value>
        /// The XML documentation resolver used by this provider.
        /// </value>
        protected IXmlDocResolver Resolver { get; }

        /// <inheritdoc/>
        public virtual bool HasDocumentation => Resolver.HasDocumentation;

        /// <inheritdoc/>
        public virtual bool TryGetNamespaceDoc(string ns, [NotNullWhen(true)] out XmlDocEntry? doc)
        {
            if (!string.IsNullOrWhiteSpace(ns) && Resolver.TryGetXmlDoc($"T:{ns}.{nameof(NamespaceDoc)}", out var xmlDoc))
            {
                doc = new XmlDocEntry(xmlDoc);
                return true;
            }

            doc = null;
            return false;
        }

        /// <inheritdoc/>
        public virtual bool TryGetMemberDoc(IMember member, [NotNullWhen(true)] out XmlDocEntry? doc)
        {
            if (member is not null && member.IsDirectDeclaration && Resolver.TryGetXmlDoc(member.CodeReference, out var memberXmlDoc))
            {
                if (member is IWithExtensionBehavior { ExtensionBlock: IExtensionBlock extensionBlock } && !memberXmlDoc.HasAttribute("isExtension"))
                {
                    memberXmlDoc.SetAttributeValue("isExtension", true);
                    if (Resolver.TryGetXmlDoc(extensionBlock.CodeReference, out var extensionBlockXmlDoc))
                        memberXmlDoc.AddFirst(new XElement("extensionblock", extensionBlockXmlDoc.Elements()));
                }

                doc = new XmlDocEntry(memberXmlDoc);
                return true;
            }

            doc = null;
            return false;
        }
    }
}

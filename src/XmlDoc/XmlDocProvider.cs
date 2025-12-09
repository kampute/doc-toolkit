// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides XML documentation for .NET types and members by parsing and resolving documentation files.
    /// </summary>
    public class XmlDocProvider : XmlDocRepository, IXmlDocProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocProvider"/> class.
        /// </summary>
        public XmlDocProvider()
        {
        }

        /// <inheritdoc/>
        public virtual bool TryGetDoc(string cref, [NotNullWhen(true)] out XmlDocEntry? doc)
        {
            if (TryGetXmlDoc(cref, out var xmlDoc))
            {
                doc = new XmlDocEntry(xmlDoc);
                return true;
            }

            doc = null;
            return false;
        }
    }
}

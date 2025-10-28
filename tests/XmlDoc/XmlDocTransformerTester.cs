// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc
{
    using Kampute.DocToolkit.XmlDoc;
    using System.IO;
    using System.Xml.Linq;

    public abstract class XmlDocTransformerTester<T>
        where T : XmlDocTransformer, new()
    {
        protected string Transform(string xmlContent)
        {
            var element = XElement.Parse($"<summary>{xmlContent}</summary>");
            var transformer = new T()
            {
                ReferenceResolver = MockHelper.CreateXmlDocReferenceResolver(),
            };

            using var writer = new StringWriter();
            transformer.Transform(writer, element);
            return writer.ToString().Replace("\r", string.Empty);
        }
    }
}

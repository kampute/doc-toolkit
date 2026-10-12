// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc
{
    using Kampute.DocToolkit.XmlDoc;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Xml.Linq;

    [TestFixture]
    public class XmlDocErrorHandlerTests
    {
        [Test]
        public void ErrorMethods_InvokeTheirOwnDelegateWithTheirArguments()
        {
            var calls = new List<string>();
            var member = new XElement("member", new XAttribute("name", "M:A.B"));
            IXmlDocErrorHandler handler = new XmlDocErrorHandler
            {
                InheritDocNotFoundError = e => calls.Add($"inheritdoc:{e.Attribute("name")!.Value}"),
                IncludeFileNotFoundError = (e, path) => calls.Add($"file:{e.Attribute("name")!.Value}:{path}"),
                IncludeMemberNotFoundError = e => calls.Add($"member:{e.Attribute("name")!.Value}"),
            };

            handler.InheritDocNotFound(member);
            handler.IncludeFileNotFound(member, "missing.xml");
            handler.IncludeMemberNotFound(member);

            Assert.That(calls, Is.EqualTo(new[] { "inheritdoc:M:A.B", "file:M:A.B:missing.xml", "member:M:A.B" }));
        }

        [Test]
        public void ErrorMethods_WithoutDelegates_IgnoreTheError()
        {
            var member = new XElement("member");
            IXmlDocErrorHandler handler = new XmlDocErrorHandler();

            Assert.That(() =>
            {
                handler.InheritDocNotFound(member);
                handler.IncludeFileNotFound(member, "missing.xml");
                handler.IncludeMemberNotFound(member);
            }, Throws.Nothing);
        }
    }
}

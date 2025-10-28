// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc
{
    using Kampute.DocToolkit.XmlDoc;
    using NUnit.Framework;
    using System.Linq;
    using System.Xml.Linq;

    [TestFixture]
    public class XmlDocEntryTests
    {
        private const string XmlDoc = @"
            <member>
                <summary>Test summary</summary>
                <remarks>Test remarks</remarks>
                <example>Test example</example>
                <typeparam name='TKey'>Test type parameter (TKey)</typeparam>
                <typeparam name='TValue'>Test type parameter (TValue)</typeparam>
                <param name='key'>Test parameter (key)</param>
                <param name='value'>Test parameter (value)</param>
                <value>Test value</value>
                <returns>Test return value</returns>
                <exception cref='T:System.Error.Exception1'>Test exception 1</exception>
                <exception cref='T:System.Error.Exception2'>Test exception 2</exception>
                <permission cref='T:System.Security.Permission1'>Test permission 1</permission>
                <permission cref='T:System.Security.Permission2'>Test permission 2</permission>
                <event cref='E:System.Notification.Event1'>Test event 1</event>
                <event cref='E:System.Notification.Event2'>Test event 2</event>
                <threadsafety static='true' instance='false'>Test thread safety</threadsafety>
                <seealso cref='T:System.String'/>
                <seealso href='http://example.com/'>www.example.com</seealso>
                <overloads>
                    <summary>Test overload summary</summary>
                </overloads>
            </member>
        ";

        private XmlDocEntry doc = XmlDocEntry.Empty;

        [SetUp]
        public void SetUp()
        {
            doc = new XmlDocEntry(XElement.Parse(XmlDoc));
        }

        [Test]
        public void Summary_ReturnsExpectedSummary()
        {
            Assert.That(doc.Summary.ToString(), Is.EqualTo("Test summary"));
        }

        [Test]
        public void Remarks_ReturnsExpectedRemarks()
        {
            Assert.That(doc.Remarks.ToString(), Is.EqualTo("Test remarks"));
        }

        [Test]
        public void Example_ReturnsExpectedExample()
        {
            Assert.That(doc.Example.ToString(), Is.EqualTo("Test example"));
        }

        [Test]
        public void Exceptions_ReturnsExpectedExceptions()
        {
            Assert.That(doc.Exceptions, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(doc.Exceptions[0].Reference.ToString(), Is.EqualTo("T:System.Error.Exception1"));
                Assert.That(doc.Exceptions[0].ToString(), Is.EqualTo("Test exception 1"));
                Assert.That(doc.Exceptions[1].Reference.ToString(), Is.EqualTo("T:System.Error.Exception2"));
                Assert.That(doc.Exceptions[1].ToString(), Is.EqualTo("Test exception 2"));
            }
        }

        [Test]
        public void Permissions_ReturnsExpectedPermissions()
        {
            Assert.That(doc.Permissions, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(doc.Permissions[0].Reference.ToString(), Is.EqualTo("T:System.Security.Permission1"));
                Assert.That(doc.Permissions[0].ToString(), Is.EqualTo("Test permission 1"));
                Assert.That(doc.Permissions[1].Reference.ToString(), Is.EqualTo("T:System.Security.Permission2"));
                Assert.That(doc.Permissions[1].ToString(), Is.EqualTo("Test permission 2"));
            }
        }

        [Test]
        public void Events_ReturnsExpectedEvents()
        {
            Assert.That(doc.Events, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(doc.Events[0].Reference.ToString(), Is.EqualTo("E:System.Notification.Event1"));
                Assert.That(doc.Events[0].ToString(), Is.EqualTo("Test event 1"));
                Assert.That(doc.Events[1].Reference.ToString(), Is.EqualTo("E:System.Notification.Event2"));
                Assert.That(doc.Events[1].ToString(), Is.EqualTo("Test event 2"));
            }
        }

        [Test]
        public void SeeAlso_ReturnsExpectedSeeAlsoReferences()
        {
            var seeAlsoList = doc.SeeAlso.ToList();
            Assert.That(seeAlsoList, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(seeAlsoList[0].Target, Is.EqualTo("T:System.String"));
                Assert.That(seeAlsoList[0].IsHyperlink, Is.False);
                Assert.That(seeAlsoList[0].ToString(), Is.Empty);
                Assert.That(seeAlsoList[1].Target, Is.EqualTo("http://example.com/"));
                Assert.That(seeAlsoList[1].IsHyperlink, Is.True);
                Assert.That(seeAlsoList[1].ToString(), Is.EqualTo("www.example.com"));
            }
        }

        [Test]
        public void ThreadSafety_ReturnsExcpectedThreadSafety()
        {
            Assert.That(doc.ThreadSafety, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(doc.ThreadSafety.IsStaticSafe, Is.True);
                Assert.That(doc.ThreadSafety.IsInstanceSafe, Is.False);
                Assert.That(doc.ThreadSafety.ToString(), Is.EqualTo("Test thread safety"));
            }
        }

        [Test]
        public void TypeParameters_ReturnsExpectedTypeParameter()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(doc.TypeParameters["TKey"].ToString(), Is.EqualTo("Test type parameter (TKey)"));
                Assert.That(doc.TypeParameters["TValue"].ToString(), Is.EqualTo("Test type parameter (TValue)"));
            }
        }

        [Test]
        public void Parameters_ReturnsExpectedParameter()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(doc.Parameters["key"].ToString(), Is.EqualTo("Test parameter (key)"));
                Assert.That(doc.Parameters["value"].ToString(), Is.EqualTo("Test parameter (value)"));
            }
        }

        [Test]
        public void ReturnDescription_ReturnsExpectedDescription()
        {
            Assert.That(doc.ReturnDescription.ToString(), Is.EqualTo("Test return value"));
        }

        [Test]
        public void ValueDescription_ReturnsExpectedDescription()
        {
            Assert.That(doc.ValueDescription.ToString(), Is.EqualTo("Test value"));
        }

        [Test]
        public void Overloads_ReturnsExpectedOverloads()
        {
            Assert.That(doc.Overloads.Summary.ToString(), Is.EqualTo("Test overload summary"));
        }
    }
}

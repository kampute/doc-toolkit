// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc
{
    using Kampute.DocToolkit.XmlDoc;
    using Moq;
    using NUnit.Framework;
    using System.Xml.Linq;

    [TestFixture]
    public class XmlDocProviderCacheTests
    {
        [Test]
        public void Constructor_WhenProviderIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new XmlDocProviderCache(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("provider"));
        }

        [Test]
        public void HasDocumentation_DelegatesToInnerProvider()
        {
            var mockProvider = new Mock<IXmlDocProvider>();
            var cache = new XmlDocProviderCache(mockProvider.Object);

            mockProvider.Setup(p => p.HasDocumentation).Returns(true);
            Assert.That(cache.HasDocumentation, Is.True);
            mockProvider.Verify(p => p.HasDocumentation, Times.Once);
        }

        [Test]
        public void TryGetDoc_CacheMiss_CallsInnerProvider()
        {
            var cref = "T:TestClass";
            var expectedDoc = new XmlDocEntry(new XElement("member", new XElement("summary", "Test summary")));

            var mockProvider = new Mock<IXmlDocProvider>();
            var cache = new XmlDocProviderCache(mockProvider.Object);

            mockProvider.Setup(p => p.TryGetDoc(cref, out expectedDoc)).Returns(true);

            var result = cache.TryGetDoc(cref, out var actualDoc);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(actualDoc, Is.EqualTo(expectedDoc));
            }

            mockProvider.Verify(p => p.TryGetDoc(cref, out expectedDoc), Times.Once);
        }

        [Test]
        public void TryGetDoc_CacheHit_DoesNotCallInnerProvider()
        {
            var cref = "T:TestClass";
            var expectedDoc = new XmlDocEntry(new XElement("member", new XElement("summary", "Test summary")));

            var mockProvider = new Mock<IXmlDocProvider>();
            var cache = new XmlDocProviderCache(mockProvider.Object);

            mockProvider.Setup(p => p.TryGetDoc(cref, out expectedDoc)).Returns(true);

            cache.TryGetDoc(cref, out _);
            mockProvider.Reset();

            var result = cache.TryGetDoc(cref, out var actualDoc);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(actualDoc, Is.EqualTo(expectedDoc));
            }

            mockProvider.Verify(p => p.TryGetDoc(It.IsAny<string>(), out It.Ref<XmlDocEntry?>.IsAny), Times.Never);
        }

        [Test]
        public void TryGetDoc_CachesNullResults()
        {
            var cref = "T:NonExistentClass";
            XmlDocEntry? expectedDoc = null;

            var mockProvider = new Mock<IXmlDocProvider>();
            var cache = new XmlDocProviderCache(mockProvider.Object);

            mockProvider.Setup(p => p.TryGetDoc(cref, out expectedDoc)).Returns(false);

            cache.TryGetDoc(cref, out _);
            mockProvider.Reset();

            var result = cache.TryGetDoc(cref, out var actualDoc);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(actualDoc, Is.Null);
            }

            mockProvider.Verify(p => p.TryGetDoc(It.IsAny<string>(), out It.Ref<XmlDocEntry?>.IsAny), Times.Never);
        }
    }
}
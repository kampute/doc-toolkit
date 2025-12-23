// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Routing
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Routing;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Linq;

    [TestFixture]
    public class UrlReferenceCollectorTests
    {
        [Test]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            var urlTransformerMock = new Mock<IUrlTransformer>();

            Assert.That
            (
                () => new UrlReferenceCollector(null!, urlTransformerMock.Object),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("context")
            );
        }

        [Test]
        public void Constructor_WithNullUrlTransformer_ThrowsArgumentNullException()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();

            Assert.That
            (
                () => new UrlReferenceCollector(context, null!),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("urlTransformer")
            );
        }

        [Test]
        public void MayTransformUrls_AlwaysReturnsTrue()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var urlTransformerMock = Mock.Of<IUrlTransformer>(m => m.MayTransformUrls == false);
            var collector = new UrlReferenceCollector(context, urlTransformerMock);

            Assert.That(collector.MayTransformUrls, Is.True);
        }

        [Test]
        public void Urls_Initially_ReturnsEmptyCollection()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var urlTransformerMock = new Mock<IUrlTransformer>();
            var collector = new UrlReferenceCollector(context, urlTransformerMock.Object);

            Assert.That(collector.Urls, Is.Empty);
        }

        [Test]
        public void TryTransformUrl_WhenUnderlyingTransformerReturnsTrueAndModelIsNull_ReturnsTrueAndDoesNotAddUrlReference()
        {
            var expectedTransformedUri = new Uri("../transformed", UriKind.Relative);

            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();

            var urlTransformerMock = new Mock<IUrlTransformer>();
            urlTransformerMock.Setup(x => x.TryTransformUrl("test", out It.Ref<Uri?>.IsAny))
                .Returns((string url, out Uri? uri) =>
                {
                    uri = expectedTransformedUri;
                    return true;
                });

            var collector = new UrlReferenceCollector(context, urlTransformerMock.Object);

            using var _ = context.AddressProvider.BeginScope("dir", null);

            var result = collector.TryTransformUrl("test", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl, Is.EqualTo(expectedTransformedUri));
                Assert.That(collector.Urls, Is.Empty);
            }
        }

        [Test]
        public void TryTransformUrl_WhenUnderlyingTransformerReturnsTrueAndModelIsNotNull_ReturnsTrueAndAddsUrlReference()
        {
            var expectedTransformedUri = new Uri("../transformed", UriKind.Relative);

            var model = Mock.Of<IDocumentModel>();
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();

            var urlTransformerMock = new Mock<IUrlTransformer>();
            urlTransformerMock.Setup(x => x.TryTransformUrl("test", out It.Ref<Uri?>.IsAny))
                .Returns((string url, out Uri? uri) =>
                {
                    uri = expectedTransformedUri;
                    return true;
                });

            var collector = new UrlReferenceCollector(context, urlTransformerMock.Object);

            using var _ = context.AddressProvider.BeginScope("dir", model);

            var result = collector.TryTransformUrl("test", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl, Is.EqualTo(expectedTransformedUri));
                Assert.That(collector.Urls, Has.Count.EqualTo(1));

                var urlReference = collector.Urls.First();
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(urlReference.ReferencingModel, Is.SameAs(model));
                    Assert.That(urlReference.BaseDirectory, Is.EqualTo("dir"));
                    Assert.That(urlReference.SourceUrl, Is.EqualTo("test"));
                    Assert.That(urlReference.TargetUrl, Is.EqualTo(expectedTransformedUri));
                }
            }
        }

        [Test]
        public void TryTransformUrl_WhenUnderlyingTransformerReturnsFalseAndModelIsNull_ReturnsFalseAndDoesNotAddUrlReference()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();

            var urlTransformerMock = new Mock<IUrlTransformer>();
            urlTransformerMock.Setup(x => x.TryTransformUrl("test", out It.Ref<Uri?>.IsAny)).Returns(false);

            var collector = new UrlReferenceCollector(context, urlTransformerMock.Object);

            using var _ = context.AddressProvider.BeginScope("dir", null);

            var result = collector.TryTransformUrl("test", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(transformedUrl, Is.Null);
                Assert.That(collector.Urls, Is.Empty);
            }
        }

        [Test]
        public void TryTransformUrl_WhenUnderlyingTransformerReturnsFalseAndModelIsNotNull_ReturnsFalseAndAddsUrlReference()
        {
            var expectedTransformedUri = new Uri("../transformed", UriKind.Relative);

            var model = Mock.Of<IDocumentModel>();
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();

            var urlTransformerMock = new Mock<IUrlTransformer>();
            urlTransformerMock.Setup(x => x.TryTransformUrl("test", out It.Ref<Uri?>.IsAny)).Returns(false);

            var collector = new UrlReferenceCollector(context, urlTransformerMock.Object);

            using var _ = context.AddressProvider.BeginScope("dir", model);

            var result = collector.TryTransformUrl("test", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(transformedUrl, Is.Null);
                Assert.That(collector.Urls, Has.Count.EqualTo(1));

                var urlReference = collector.Urls.First();
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(urlReference.ReferencingModel, Is.SameAs(model));
                    Assert.That(urlReference.BaseDirectory, Is.EqualTo("dir"));
                    Assert.That(urlReference.SourceUrl, Is.EqualTo("test"));
                    Assert.That(urlReference.TargetUrl, Is.Null);
                }
            }
        }
    }
}
// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Routing
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Topics;
    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class ContextAwareUrlTransformerTests
    {
        [Test]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            Assert.That(static () => new ContextAwareUrlTransformer(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("context"));
        }

        [Test]
        public void Context_Property_ReturnsProvidedContext()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            Assert.That(transformer.Context, Is.SameAs(context));
        }

        [Test]
        public void MayTransformUrls_Property_Always_ReturnsTrue()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            Assert.That(transformer.MayTransformUrls, Is.True);
        }

        [Test]
        public void TryTransformUrl_WhenTopicFound_ReturnsTopicUrl()
        {
            var topic = MockTopicBuilder.Topic("TestTopic").Build();
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([topic]);
            var transformer = new ContextAwareUrlTransformer(context);

            context.Topics.TryGetById(topic.Id, out var contextualTopic);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("testTopic", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl, Is.EqualTo(contextualTopic!.Url));
            }
        }

        [Test]
        public void TryTransformUrl_WithActiveScope_WhenTopicNotFound_ReturnsTransformedUrl()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("testTopic.html", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo("../testTopic.html"));
            }
        }

        [TestCase(null)]
        [TestCase("")]
        public void TryTransformUrl_WithNullOrEmptyUrlString_ReturnsFalse(string? urlString)
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl(urlString!, out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(transformedUrl, Is.Null);
            }
        }

        [Test]
        public void TryTransformUrl_WithAbsoluteUrl_ReturnsFalse()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("https://example.com/page.html", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(transformedUrl, Is.Null);
            }
        }

        [TestCase("/page.html")]
        [TestCase("?query=value")]
        [TestCase("#fragment")]
        [TestCase("?query=value#fragment")]
        public void TryTransformUrl_WithLeadingSpecialCharacters_ReturnsFalse(string urlString)
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl(urlString, out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(transformedUrl, Is.Null);
            }
        }

        [Test]
        public void TryTransformUrl_WithSpecialCharactersInPath_HandlesCorrectly()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("special%20page.html", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo("../special%20page.html"));
            }
        }

        [Test]
        public void TryTransformUrl_WithQueryString_PreservesQueryString()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("page.html?query=value&param=test", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo("../page.html?query=value&param=test"));
            }
        }

        [Test]
        public void TryTransformUrl_WithFragment_PreservesFragment()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("page.html#section", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo("../page.html#section"));
            }
        }

        [Test]
        public void TryTransformUrl_WithQueryStringAndFragment_PreservesBoth()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("page.html?query=value#section", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo("../page.html?query=value#section"));
            }
        }

        [Test]
        public void TryTransformUrl_InNestedScope_AddsCorrectRelativePath()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("api/namespace", null);

            var result = transformer.TryTransformUrl("guide.html", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo("../../guide.html"));
            }
        }

        [Test]
        public void TryTransformUrl_WithMixedCaseTopic_ResolvesCorrectly()
        {
            var topic = MockTopicBuilder.Topic("TestTopic").Build();
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([topic]);
            var transformer = new ContextAwareUrlTransformer(context);

            context.Topics.TryGetById(topic.Id, out var contextualTopic);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("TESTtopic", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl, Is.EqualTo(contextualTopic!.Url));
            }
        }

        [Test]
        public void TryTransformUrl_WithTopicAndQueryString_PreservesQueryString()
        {
            var topic = MockTopicBuilder.Topic("TestTopic").Build();
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([topic]);
            var transformer = new ContextAwareUrlTransformer(context);

            context.Topics.TryGetById(topic.Id, out var contextualTopic);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("testTopic?query=value", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo($"{contextualTopic!.Url}?query=value"));
            }
        }

        [Test]
        public void TryTransformUrl_WithTopicAndFragment_PreservesFragment()
        {
            var topic = MockTopicBuilder.Topic("TestTopic").Build();
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([topic]);
            var transformer = new ContextAwareUrlTransformer(context);

            context.Topics.TryGetById(topic.Id, out var contextualTopic);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("testTopic#section", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo($"{contextualTopic!.Url}#section"));
            }
        }

        [Test]
        public void TryTransformUrl_WithTopicAndBothQueryAndFragment_PreservesBoth()
        {
            var topic = MockTopicBuilder.Topic("TestTopic").Build();
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([topic]);
            var transformer = new ContextAwareUrlTransformer(context);

            context.Topics.TryGetById(topic.Id, out var contextualTopic);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("testTopic?complex=param&more=data#complex-section", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo($"{contextualTopic!.Url}?complex=param&more=data#complex-section"));
            }
        }

        [Test]
        public void TryTransformUrl_WithChildTopicFromRoot_ResolvesToChildUrl()
        {
            var topic = MockTopicBuilder.Topic("parent").WithChildren("child").Build();

            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([topic]);
            var transformer = new ContextAwareUrlTransformer(context);

            context.Topics.TryGetById("parent/child", out var contextualChild);

            using var _ = context.AddressProvider.BeginScope("", null);

            var result = transformer.TryTransformUrl("parent/child", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl, Is.EqualTo(contextualChild!.Url));
            }
        }

        [Test]
        public void TryTransformUrl_WithChildTopicFromParent_ResolvesToChildUrl()
        {
            var topic = MockTopicBuilder.Topic("parent").WithChildren("child").Build();

            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([topic]);
            var transformer = new ContextAwareUrlTransformer(context);

            context.Topics.TryGetById("parent", out var contextualParent);
            context.Topics.TryGetById("parent/child", out var contextualChild);

            using var _ = context.AddressProvider.BeginScope("parent", contextualParent);

            var result = transformer.TryTransformUrl("child", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl, Is.EqualTo(contextualChild!.Url));
            }
        }

        [Test]
        public void TryTransformUrl_WithSiblingTopicFromChild_ResolvesToChildUrl()
        {
            var topic = MockTopicBuilder.Topic("parent").WithChildren("child1", "child2").Build();

            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([topic]);
            var transformer = new ContextAwareUrlTransformer(context);

            context.Topics.TryGetById("parent", out var contextualParent);
            context.Topics.TryGetById("parent/child1", out var contextualChild1);
            context.Topics.TryGetById("parent/child2", out var contextualChild2);

            using var _ = context.AddressProvider.BeginScope("parent", contextualChild1);

            var result = transformer.TryTransformUrl("../child2", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl, Is.EqualTo(contextualChild2!.Url));
            }
        }

        [Test]
        public void TryTransformUrl_WithNestedTopicFromDifferentBranch_ResolvesToCorrectUrl()
        {
            var topic1 = MockTopicBuilder.Topic("Topic1").WithChildren("SubA").Build();
            var topic2 = MockTopicBuilder.Topic("Topic2").WithChildren("SubB").Build();

            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([topic1, topic2]);
            var transformer = new ContextAwareUrlTransformer(context);

            context.Topics.TryGetById("Topic1/SubA", out var contextualSubA);
            context.Topics.TryGetById("Topic2/SubB", out var contextualSubB);

            using var _ = context.AddressProvider.BeginScope("topic1", contextualSubA);

            var result = transformer.TryTransformUrl("Topic2/SubB", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl, Is.EqualTo(contextualSubB!.Url));
            }
        }

        [Test]
        public void TryTransformUrl_WithFileBasedTopicAndExistingAsset_ReturnsAssetUrl()
        {
            var directory = Path.GetTempPath();
            var topicFile = Path.Combine(directory, "guides", "test-topic.md");
            var assetFile = Path.Combine(directory, "assets", "license.txt");
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(assetFile)!);
                File.WriteAllText(assetFile, "fake license content");

                var fileBasedTopic = new MarkdownFileTopic("TestTopic", topicFile);
                using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([fileBasedTopic]);
                var transformer = new ContextAwareUrlTransformer(context);

                context.Topics.TryGetById("TestTopic", out var contextualTopic);

                using var _ = context.AddressProvider.BeginScope("test", contextualTopic);

                var result = transformer.TryTransformUrl("../assets/license.txt", out var transformedUrl);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(result, Is.True);
                    Assert.That(transformedUrl?.ToString(), Is.EqualTo("assets/license.txt"));
                }
            }
            finally
            {
                if (File.Exists(assetFile))
                    File.Delete(assetFile);
                if (Directory.Exists(Path.GetDirectoryName(assetFile)!))
                    Directory.Delete(Path.GetDirectoryName(assetFile)!);
            }
        }

        [Test]
        public void TryTransformUrl_WithFileBasedTopicAndBeyondRootRelativeAsset_ReturnsFalse()
        {
            var fileBasedTopic = new MarkdownFileTopic("TestTopic", "/topics/test-topic.md");
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([fileBasedTopic]);
            var transformer = new ContextAwareUrlTransformer(context);

            context.Topics.TryGetById("TestTopic", out var contextualTopic);

            using var _ = context.AddressProvider.BeginScope("test", contextualTopic);

            var result = transformer.TryTransformUrl("../../../license.txt", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(transformedUrl, Is.Null);
            }
        }

        [Test]
        public void TryTransformUrl_WithNonFileBasedTopicAndExistingAsset_FallsBackToSiteRelative()
        {
            var directory = Path.GetTempPath();
            var assetFile = Path.Combine(directory, "assets", "license.txt");
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(assetFile)!);
                File.WriteAllText(assetFile, "fake license content");

                var nonFileBasedTopic = MockTopicBuilder.Topic("NonFileBasedTopic").Build();
                using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([nonFileBasedTopic]);
                var transformer = new ContextAwareUrlTransformer(context);

                context.Topics.TryGetById("NonFileBasedTopic", out var contextualTopic);

                using var _ = context.AddressProvider.BeginScope("test", contextualTopic);

                var result = transformer.TryTransformUrl("license.txt", out var transformedUrl);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(result, Is.True);
                    Assert.That(transformedUrl?.ToString(), Is.EqualTo("../license.txt"));
                }
            }
            finally
            {
                if (File.Exists(assetFile))
                    File.Delete(assetFile);
                if (Directory.Exists(Path.GetDirectoryName(assetFile)!))
                    Directory.Delete(Path.GetDirectoryName(assetFile)!);
            }
        }

        [Test]
        public void TryTransformUrl_WithFileBasedTopicAndNonExistingAsset_FallsBackToSiteRelative()
        {
            var fileBasedTopic = new MarkdownFileTopic("TestTopic", "/topics/test-topic.md");
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([fileBasedTopic]);
            var transformer = new ContextAwareUrlTransformer(context);

            context.Topics.TryGetById("TestTopic", out var contextualTopic);

            using var _ = context.AddressProvider.BeginScope("test", contextualTopic);

            var result = transformer.TryTransformUrl("nonexistent.png", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo("../nonexistent.png"));
            }
        }

        [Test]
        public void TryTransformUrl_WithNoActiveScope_FallsBackToSiteRelative()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            var result = transformer.TryTransformUrl("some-asset.png", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo("some-asset.png"));
            }
        }

        [Test]
        public void TryTransformUrl_WithEmptyPathAndQueryString_HandlesCorrectly()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("?query=value", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(transformedUrl, Is.Null);
            }
        }

        [TestCase("scheme://host/path")]
        [TestCase("mailto:user@example.com")]
        [TestCase("ftp://example.com/file")]
        [TestCase("file:///C:/path/to/file")]
        public void TryTransformUrl_WithDifferentAbsoluteSchemes_ReturnsFalse(string urlString)
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl(urlString, out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(transformedUrl, Is.Null);
            }
        }

        [TestCase("  ")]
        [TestCase("\t")]
        [TestCase("\n")]
        [TestCase("\r\n")]
        public void TryTransformUrl_WithWhitespaceOnly_ReturnsFalse(string urlString)
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl(urlString, out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(transformedUrl, Is.Null);
            }
        }

        [Test]
        public void TryTransformUrl_WithDeeplyNestedScope_AddsCorrectRelativePath()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("api/namespace/type/member", null);

            var result = transformer.TryTransformUrl("guide.html", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo("../../../../guide.html"));
            }
        }

        [Test]
        public void TryTransformUrl_WithRelativePathInUrlWithoutTopicScope_ReturnsFalse()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("api/namespace", null);

            var result = transformer.TryTransformUrl("../guides/tutorial.html", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(transformedUrl, Is.Null);
            }
        }

        [Test]
        public void TryTransformUrl_WithPercentEncodedCharacters_PreservesEncoding()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("file%20with%20spaces.html", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo("../file%20with%20spaces.html"));
            }
        }

        [Test]
        public void TryTransformUrl_WithUnicodeCharacters_HandlesCorrectly()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("файл.html", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo("../файл.html"));
            }
        }

        [Test]
        public void TryTransformUrl_WithEmptyFragment_PreservesEmptyFragment()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("page.html#", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo("../page.html#"));
            }
        }

        [Test]
        public void TryTransformUrl_WithEmptyQueryString_PreservesEmptyQuery()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var transformer = new ContextAwareUrlTransformer(context);

            using var _ = context.AddressProvider.BeginScope("test", null);

            var result = transformer.TryTransformUrl("page.html?", out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformedUrl?.ToString(), Is.EqualTo("../page.html?"));
            }
        }
    }
}

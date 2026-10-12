// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.XmlDoc;
    using NUnit.Framework;

    [TestFixture]
    public class XmlDocContextAwareReferenceResolverTests
    {
        private IDocumentationContext context = null!;
        private XmlDocContextAwareReferenceResolver resolver = null!;

        [SetUp]
        public void SetUp()
        {
            MetadataProvider.RegisterRuntimeAssemblies(); // To ensure all code references are resolvable

            var topic = MockTopicBuilder.Topic("guide", "Guide").Build();
            context = MockHelper.CreateDocumentationContext<HtmlFormat>([topic]);
            resolver = new XmlDocContextAwareReferenceResolver(context);
        }

        [TearDown]
        public void TearDown()
        {
            context.Dispose();
        }

        [Test]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            Assert.That(static () => new XmlDocContextAwareReferenceResolver(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("context"));
        }

        [Test]
        public void FormatCode_NormalizesCodeBlock()
        {
            // Leading and trailing empty lines are removed, consecutive empty lines are collapsed, tabs become four spaces,
            // trailing whitespace is removed, and the common indentation is removed.
            const string code = "\n\n    line1\n\n\n        line2\t  \n\tline3\n\n";

            var result = resolver.FormatCode(code).Replace("\r\n", "\n");

            Assert.That(result, Is.EqualTo("line1\n\n    line2\nline3"));
        }

        [Test]
        public void GetLanguageId_ReturnsIdentifierOfContextLanguage()
        {
            Assert.That(resolver.GetLanguageId("int x;"), Is.EqualTo(context.Language.Identifier));
        }

        [Test]
        public void GetKeywordUrl_WithKnownKeyword_ReturnsLanguageUrlOfTrimmedKeyword()
        {
            Assert.That(context.Language.TryGetUrl("null", out var expected), Is.True, "Precondition: the language documents the keyword.");

            Assert.That(resolver.GetKeywordUrl(" null "), Is.EqualTo(expected!.ToString()));
        }

        [Test]
        public void GetKeywordUrl_WithUnknownKeyword_ReturnsEmpty()
        {
            Assert.That(resolver.GetKeywordUrl("not-a-keyword"), Is.Empty);
        }

        [Test]
        public void GetCodeReferenceUrl_WithResolvableReference_ReturnsMemberUrl()
        {
            Assert.That(context.AddressProvider.TryGetMemberUrl(typeof(Acme.SampleMethods).GetMetadata(), out var expected), Is.True);

            Assert.That(resolver.GetCodeReferenceUrl(" T:Acme.SampleMethods "), Is.EqualTo(expected!.ToString()));
        }

        [Test]
        public void GetCodeReferenceUrl_WithUnresolvableReference_ReturnsEmpty()
        {
            Assert.That(resolver.GetCodeReferenceUrl("T:Acme.DoesNotExist"), Is.Empty);
        }

        [Test]
        public void GetCodeReferenceTitle_WithUnresolvableReference_ReturnsReference()
        {
            Assert.That(resolver.GetCodeReferenceTitle("T:Acme.DoesNotExist"), Is.EqualTo("T:Acme.DoesNotExist"));
        }

        [Test]
        public void GetTopicUrl_WithTopicReference_ReturnsTransformedUrl()
        {
            Assert.That(context.UrlTransformer.TryTransformUrl("guide", out var expected), Is.True, "Precondition: the topic reference is transformable.");

            Assert.That(resolver.GetTopicUrl(" guide "), Is.EqualTo(expected!.ToString()));
        }

        [Test]
        public void GetTopicUrl_WithAbsoluteUrl_ReturnsUrlUnchanged()
        {
            Assert.That(resolver.GetTopicUrl("https://example.org/page"), Is.EqualTo("https://example.org/page"));
        }

        [TestCase("guide", ExpectedResult = "Guide")]
        [TestCase("guide#section", ExpectedResult = "Guide")]
        [TestCase("missing", ExpectedResult = "")]
        public string GetTopicTitle_ReturnsTitleOfResolvedTopicOrEmpty(string href)
        {
            return resolver.GetTopicTitle(href);
        }
    }
}

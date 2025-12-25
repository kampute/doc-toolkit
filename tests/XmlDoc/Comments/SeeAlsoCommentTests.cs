// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc.Comments
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.XmlDoc.Comments;
    using NUnit.Framework;
    using System.Linq;
    using System.Xml.Linq;

    [TestFixture]
    public class SeeAlsoCommentTests
    {
        [Test]
        public void WithResolvedHyperlink_WithCodeReference_ReturnsSameComment()
        {
            using var docContext = MockHelper.CreateDocumentationContext<HtmlFormat>();

            var element = XElement.Parse("<seealso cref=\"T:System.String\"/>");
            SeeAlsoComment.TryCreate(element, out var seeAlso);

            var result = seeAlso!.WithResolvedHyperlink(docContext);

            Assert.That(result, Is.SameAs(seeAlso));
        }

        [Test]
        public void WithResolvedHyperlink_WithAbsoluteUrl_ReturnsSameComment()
        {
            using var docContext = MockHelper.CreateDocumentationContext<HtmlFormat>();

            var element = XElement.Parse("<seealso href=\"http://example.com/\"/>");
            SeeAlsoComment.TryCreate(element, out var seeAlso);

            var result = seeAlso!.WithResolvedHyperlink(docContext);

            Assert.That(result, Is.SameAs(seeAlso));
        }

        [Test]
        public void WithResolvedHyperlink_WithUnresolvableRelativeUrl_ReturnsSameComment()
        {
            using var docContext = MockHelper.CreateDocumentationContext<HtmlFormat>();

            var element = XElement.Parse("<seealso href=\"../unresolvable\"/>");
            SeeAlsoComment.TryCreate(element, out var seeAlso);

            var result = seeAlso!.WithResolvedHyperlink(docContext);

            Assert.That(result, Is.SameAs(seeAlso));
        }

        [Test]
        public void WithResolvedHyperlink_WithResolvableRelativeUrl_UpdatesHrefAttribute()
        {
            var topic = MockTopicBuilder.Topic("api-guide", "API Guide").Build();
            using var docContext = MockHelper.CreateDocumentationContext<HtmlFormat>([topic]);

            var element = XElement.Parse("<seealso href=\"api-guide\">Original text</seealso>");
            SeeAlsoComment.TryCreate(element, out var seeAlso);

            var result = seeAlso!.WithResolvedHyperlink(docContext);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.SameAs(seeAlso));
                Assert.That(result.Target, Is.EqualTo("https://example.com/api-guide"));
                Assert.That(result.Content.Value, Is.EqualTo("Original text"));
            }
        }

        [Test]
        public void WithResolvedHyperlink_WithResolvableRelativeUrlAndEmptyComment_AddsTopicTitle()
        {
            var topic = MockTopicBuilder.Topic("api-guide", "API Guide").Build();
            using var docContext = MockHelper.CreateDocumentationContext<HtmlFormat>([topic]);

            var element = XElement.Parse("<seealso href=\"api-guide\"/>");
            SeeAlsoComment.TryCreate(element, out var seeAlso);

            var result = seeAlso!.WithResolvedHyperlink(docContext);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.SameAs(seeAlso));
                Assert.That(result.Target, Is.EqualTo("https://example.com/api-guide"));
                Assert.That(result.Content.Value, Is.EqualTo("API Guide"));
            }
        }

        [Test]
        public void WithResolvedHyperlink_WithResolvableChildTopicUrl_UpdatesHrefAttribute()
        {
            var parentTopic = MockTopicBuilder.Topic("parent", "Parent Guide").WithChild("child", "Child Guide").Build();
            using var docContext = MockHelper.CreateDocumentationContext<HtmlFormat>([parentTopic]);

            var element = XElement.Parse("<seealso href=\"parent/child\">Original text</seealso>");
            SeeAlsoComment.TryCreate(element, out var seeAlso);

            var result = seeAlso!.WithResolvedHyperlink(docContext);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.SameAs(seeAlso));
                Assert.That(result.Target, Is.EqualTo("https://example.com/parent/child"));
                Assert.That(result.Content.Value, Is.EqualTo("Original text"));
            }
        }

        [Test]
        public void WithResolvedHyperlink_WithResolvableChildTopicUrlAndEmptyComment_AddsTopicTitle()
        {
            var parentTopic = MockTopicBuilder.Topic("parent", "Parent Guide").WithChild("child", "Child Guide").Build();
            using var docContext = MockHelper.CreateDocumentationContext<HtmlFormat>([parentTopic]);

            var element = XElement.Parse("<seealso href=\"parent/child\"/>");
            SeeAlsoComment.TryCreate(element, out var seeAlso);

            var result = seeAlso!.WithResolvedHyperlink(docContext);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.Not.SameAs(seeAlso));
                Assert.That(result.Target, Is.EqualTo("https://example.com/parent/child"));
                Assert.That(result.Content.Value, Is.EqualTo("Child Guide"));
            }
        }

        [Test]
        public void TryCreate_cref_CreatesComment()
        {
            var element = XElement.Parse("<seealso cref=\"T:System.String\"/>");

            var result = SeeAlsoComment.TryCreate(element, out var comment);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(comment, Is.Not.Null);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comment.IsCodeReference, Is.True);
                Assert.That(comment.IsHyperlink, Is.False);
            }
        }

        [Test]
        public void TryCreate_href_CreatesComment()
        {
            var element = XElement.Parse("<seealso href=\"http://example.com\"/>");

            var result = SeeAlsoComment.TryCreate(element, out var comment);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(comment, Is.Not.Null);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comment.IsHyperlink, Is.True);
                Assert.That(comment.IsCodeReference, Is.False);
            }
        }

        [Test]
        public void Collect_WithMixedElements_ReturnsValidComments()
        {
            var elements = new[]
            {
                XElement.Parse("<seealso cref=\"T:System.String\"/>"),
                XElement.Parse("<seealso href=\"http://example.com\"/>"),
            };

            var comments = SeeAlsoComment.Collect(elements).ToList();

            Assert.That(comments, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(comments[0].IsCodeReference, Is.True);
                Assert.That(comments[1].IsHyperlink, Is.True);
            }
        }
    }
}
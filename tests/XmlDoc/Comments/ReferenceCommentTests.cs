// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc.Comments
{
    using Kampute.DocToolkit.XmlDoc.Comments;
    using NUnit.Framework;
    using System.Linq;
    using System.Xml.Linq;

    [TestFixture]
    public class ReferenceCommentTests
    {
        [Test]
        public void TryCreate_WithCodeReference_CreatesReferenceComment()
        {
            var element = XElement.Parse("<exception cref=\"T:System.ArgumentNullException\">Error message</exception>");

            var result = ReferenceComment.TryCreate(element, out var comment);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(comment, Is.Not.Null);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comment.Reference, Is.Not.Null);
                Assert.That(comment.Reference.ToString(), Is.EqualTo("T:System.ArgumentNullException"));
                Assert.That(comment.ToString(), Is.EqualTo("Error message"));
            }
        }

        [Test]
        public void TryCreate_WithNoCodeReference_ReturnsFalse()
        {
            var element = XElement.Parse("<exception>Error message</exception>");

            var result = ReferenceComment.TryCreate(element, out var comment);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(comment, Is.Null);
            }
        }

        [Test]
        public void TryCreate_WithNullElement_ReturnsFalse()
        {
            var result = ReferenceComment.TryCreate(null!, out var comment);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(comment, Is.Null);
            }
        }

        [Test]
        public void Collect_MixedElements_ReturnsOnlyValidReferences()
        {
            var elements = new[]
            {
                XElement.Parse("<exception cref=\"T:System.ArgumentNullException\">Null argument</exception>"),
                XElement.Parse("<exception cref=\"T:System.InvalidOperationException\">Invalid operation</exception>"),
                XElement.Parse("<exception>No cref attribute</exception>")
            };

            var comments = ReferenceComment.Collect(elements).ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comments, Has.Count.EqualTo(2));
                Assert.That(comments[0].Reference.ToString(), Is.EqualTo("T:System.ArgumentNullException"));
                Assert.That(comments[1].Reference.ToString(), Is.EqualTo("T:System.InvalidOperationException"));
            }
        }

        [Test]
        public void Collect_ElementsWithDuplicateAndUniqueReferences_GroupsDuplicates()
        {
            var elements = new[]
            {
                XElement.Parse("<exception cref=\"T:System.ArgumentNullException\">Null argument</exception>"),
                XElement.Parse("<exception cref=\"T:System.ArgumentException\">Parameter is empty</exception>"),
                XElement.Parse("<exception cref=\"T:System.ArgumentException\">Parameter contains invalid characters</exception>"),
                XElement.Parse("<exception cref=\"T:System.ArgumentException\">Parameter exceeds maximum length</exception>")
            };

            var comments = ReferenceComment.Collect(elements).ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comments, Has.Count.EqualTo(2));
                Assert.That(comments[0].Reference, Is.EqualTo("T:System.ArgumentNullException"));
                Assert.That(comments[0].Content.Value, Is.EqualTo("Null argument"));
                Assert.That(comments[0].Variants, Is.Empty);
                Assert.That(comments[1].Reference, Is.EqualTo("T:System.ArgumentException"));
                Assert.That(comments[1].Content.Value, Is.EqualTo("Parameter is empty"));
                Assert.That(comments[1].Variants, Has.Count.EqualTo(2));
                Assert.That(comments[1].Variants[0].Value, Is.EqualTo("Parameter contains invalid characters"));
                Assert.That(comments[1].Variants[1].Value, Is.EqualTo("Parameter exceeds maximum length"));
            }
        }
    }
}
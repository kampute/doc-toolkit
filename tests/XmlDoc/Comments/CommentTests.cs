// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc.Comments
{
    using Kampute.DocToolkit.XmlDoc;
    using Kampute.DocToolkit.XmlDoc.Comments;
    using Moq;
    using NUnit.Framework;
    using System.Xml.Linq;

    [TestFixture]
    public class CommentTests
    {
        [Test]
        public void Empty_IsEmpty_ReturnsTrue()
        {
            Assert.That(Comment.Empty.IsEmpty, Is.True);
        }

        [Test]
        public void Create_NullElement_ReturnsEmptyComment()
        {
            var comment = Comment.Create(null);

            Assert.That(comment, Is.SameAs(Comment.Empty));
        }

        [Test]
        public void Create_EmptyElement_ReturnsEmptyComment()
        {
            var element = XElement.Parse("<summary></summary>");

            var comment = Comment.Create(element);

            Assert.That(comment.IsEmpty, Is.True);
        }

        [Test]
        public void Create_ElementWithContent_ReturnsNonEmptyComment()
        {
            var element = XElement.Parse("<summary>Test content</summary>");

            var comment = Comment.Create(element);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comment.IsEmpty, Is.False);
                Assert.That(comment.ToString(), Is.EqualTo("Test content"));
            }
        }

        [Test]
        public void IsEmpty_WhitespaceContent_ReturnsTrue()
        {
            var element = XElement.Parse("<summary>   </summary>");

            var comment = Comment.Create(element);

            Assert.That(comment.IsEmpty, Is.True);
        }

        [Test]
        public void ToString_ReturnsXmlElementValue()
        {
            var element = XElement.Parse("<summary>Test content</summary>");

            var comment = Comment.Create(element);

            Assert.That(comment.ToString(), Is.EqualTo("Test content"));
        }

        [Test]
        public void ToString_WithFormatter_TransformsContent()
        {
            var element = XElement.Parse("<summary>Test content</summary>");
            var formatterMock = new Mock<IXmlDocTransformer>();
            formatterMock.Setup(f => f.Transform(It.IsAny<System.IO.TextWriter>(), It.IsAny<XElement>()))
                .Callback<System.IO.TextWriter, XElement>((writer, _) => writer.Write("Transformed content"));

            var comment = Comment.Create(element);
            var result = comment.ToString(formatterMock.Object);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.EqualTo("Transformed content"));
                formatterMock.Verify(f => f.Transform(It.IsAny<System.IO.TextWriter>(), element), Times.Once);
            }
        }

        [Test]
        public void Collect_MixedElements_ReturnsNamedComments()
        {
            var elements = new[]
            {
                XElement.Parse("<param name=\"first\">First parameter</param>"),
                XElement.Parse("<param name=\"second\">Second parameter</param>"),
                XElement.Parse("<param>No name attribute</param>"),
            };

            var comments = Comment.Collect(elements);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comments, Has.Count.EqualTo(2));
                Assert.That(comments.ContainsKey("first"), Is.True);
                Assert.That(comments.ContainsKey("second"), Is.True);
                Assert.That(comments["first"].ToString(), Is.EqualTo("First parameter"));
                Assert.That(comments["second"].ToString(), Is.EqualTo("Second parameter"));
            }
        }
    }
}
// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc.Comments
{
    using Kampute.DocToolkit.XmlDoc.Comments;
    using NUnit.Framework;
    using System.Xml.Linq;

    [TestFixture]
    public class ThreadSafetyCommentTests
    {
        [Test]
        public void Constructor_WithBothAttributes_SetsProperties()
        {
            var element = XElement.Parse("<threadsafety static=\"true\" instance=\"false\">Thread safety comment</threadsafety>");

            var comment = new ThreadSafetyComment(element);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comment.IsStaticSafe, Is.True);
                Assert.That(comment.IsInstanceSafe, Is.False);
                Assert.That(comment.IsSafe, Is.False);
                Assert.That(comment.IsEmpty, Is.False);
                Assert.That(comment.ToString(), Is.EqualTo("Thread safety comment"));
            }
        }

        [Test]
        public void Constructor_WithOnlyStaticAttribute_SetsOnlyStaticProperty()
        {
            var element = XElement.Parse("<threadsafety static=\"true\"></threadsafety>");

            var comment = new ThreadSafetyComment(element);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comment.IsStaticSafe, Is.True);
                Assert.That(comment.IsInstanceSafe, Is.Null);
                Assert.That(comment.IsSafe, Is.Null);
            }
        }

        [Test]
        public void Constructor_WithOnlyInstanceAttribute_SetsOnlyInstanceProperty()
        {
            var element = XElement.Parse("<threadsafety instance=\"true\"></threadsafety>");

            var comment = new ThreadSafetyComment(element);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comment.IsStaticSafe, Is.Null);
                Assert.That(comment.IsInstanceSafe, Is.True);
                Assert.That(comment.IsSafe, Is.Null);
            }
        }

        [Test]
        public void Constructor_WithBothTrueAttributes_SetsBothPropertiesAndIsSafe()
        {
            var element = XElement.Parse("<threadsafety static=\"true\" instance=\"true\"></threadsafety>");

            var comment = new ThreadSafetyComment(element);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comment.IsStaticSafe, Is.True);
                Assert.That(comment.IsInstanceSafe, Is.True);
                Assert.That(comment.IsSafe, Is.True);
            }
        }

        [Test]
        public void Constructor_WithInvalidAttributes_SetsPropertiesToNull()
        {
            var element = XElement.Parse("<threadsafety static=\"invalid\" instance=\"invalid\"></threadsafety>");

            var comment = new ThreadSafetyComment(element);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comment.IsStaticSafe, Is.Null);
                Assert.That(comment.IsInstanceSafe, Is.Null);
                Assert.That(comment.IsSafe, Is.Null);
            }
        }

        [Test]
        public void Empty_IsEmpty_ReturnsTrue()
        {
            Assert.That(ThreadSafetyComment.Empty.IsEmpty, Is.True);
        }

        [Test]
        public void IsEmpty_NoAttributesOrContent_ReturnsTrue()
        {
            var element = XElement.Parse("<threadsafety></threadsafety>");

            var comment = new ThreadSafetyComment(element);

            Assert.That(comment.IsEmpty, Is.True);
        }

        [Test]
        public void IsEmpty_WithAttributesButNoContent_ReturnsFalse()
        {
            var element = XElement.Parse("<threadsafety static=\"true\"></threadsafety>");

            var comment = new ThreadSafetyComment(element);

            Assert.That(comment.IsEmpty, Is.False);
        }

        [Test]
        public void IsEmpty_WithContentButNoAttributes_ReturnsFalse()
        {
            var element = XElement.Parse("<threadsafety>Thread safety information</threadsafety>");

            var comment = new ThreadSafetyComment(element);

            Assert.That(comment.IsEmpty, Is.False);
        }

        [Test]
        public void Create_NullElement_ReturnsEmptyComment()
        {
            var comment = ThreadSafetyComment.Create(null);

            Assert.That(comment, Is.SameAs(ThreadSafetyComment.Empty));
        }

        [Test]
        public void Create_ValidElement_ReturnsThreadSafetyComment()
        {
            var element = XElement.Parse("<threadsafety static=\"true\" instance=\"true\"></threadsafety>");

            var comment = ThreadSafetyComment.Create(element);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comment, Is.Not.Null);
                Assert.That(comment.IsStaticSafe, Is.True);
                Assert.That(comment.IsInstanceSafe, Is.True);
            }
        }
    }
}
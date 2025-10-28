// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Topics
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Topics;
    using Kampute.DocToolkit.Topics.Abstracts;
    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class TopicSourceTests
    {
        private class TestableTopic : TopicSource
        {
            public TestableTopic(string id) : base(id) { }

            public override void Render(TextWriter writer, IDocumentationContext context) { }
        }

        [Test]
        public void Title_WhenSetToNull_GeneratesFromName()
        {
            var topic = new TestableTopic("test-name")
            {
                Title = null!
            };

            Assert.That(topic.Title, Is.EqualTo("Test Name"));
        }

        [Test]
        public void Title_WhenSetToEmptyString_GeneratesFromName()
        {
            var topic = new TestableTopic("test-name")
            {
                Title = string.Empty
            };

            Assert.That(topic.Title, Is.EqualTo("Test Name"));
        }

        [Test]
        public void Title_WhenSetToWhitespace_GeneratesFromName()
        {
            var topic = new TestableTopic("test-name")
            {
                Title = "   "
            };

            Assert.That(topic.Title, Is.EqualTo("Test Name"));
        }

        [Test]
        public void Title_WhenSetToValidValue_SetsValue()
        {
            var topic = new TestableTopic("test-name")
            {
                Title = "Valid Title"
            };

            Assert.That(topic.Title, Is.EqualTo("Valid Title"));
        }

        [Test]
        public void ParentTopic_WhenSetToNonNull_AddsToParent()
        {
            var parent = new TestableTopic("parent");
            var topic = new TestableTopic("topic")
            {
                ParentTopic = parent
            };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(parent.Subtopics, Does.Contain(topic));
                Assert.That(topic.ParentTopic, Is.SameAs(parent));
            }
        }

        [Test]
        public void ParentTopic_WhenSetToNull_RemovesFromOldParent()
        {
            var oldParent = new TestableTopic("parent");
            var topic = new TestableTopic("topic")
            {
                ParentTopic = oldParent
            };

            topic.ParentTopic = null;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(oldParent.Subtopics, Does.Not.Contain(topic));
                Assert.That(topic.ParentTopic, Is.Null);
            }
        }

        [Test]
        public void ParentTopic_WhenSetToNewValue_RemovesFromOldParentAndAddsToNewParent()
        {
            var oldParent = new TestableTopic("old-parent");
            var newParent = new TestableTopic("new-parent");
            var topic = new TestableTopic("topic")
            {
                ParentTopic = oldParent
            };

            topic.ParentTopic = newParent;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(oldParent.Subtopics, Does.Not.Contain(topic));
                Assert.That(newParent.Subtopics, Does.Contain(topic));
            }
        }

        [Test]
        public void AddSubtopic_WhenSubtopicDoesNotExist_AddsSubtopic()
        {
            var topic = new TestableTopic("topic");
            var subtopic = new TestableTopic("subtopic");

            topic.AddSubtopic(subtopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(topic.Subtopics, Does.Contain(subtopic));
                Assert.That(subtopic.ParentTopic, Is.SameAs(topic));
            }
        }

        [Test]
        public void AddSubtopic_WhenAddingSameInstances_DoesNothing()
        {
            var topic = new TestableTopic("topic");
            var subtopic = new TestableTopic("subtopic");
            topic.AddSubtopic(subtopic);

            Assert.That(() => topic.AddSubtopic(subtopic), Throws.Nothing);
            Assert.That(topic.Subtopics, Has.Count.EqualTo(1));
        }

        [Test]
        public void AddSubtopic_WhenAddingDifferentInstancesWithSameId_ThrowsArgumentException()
        {
            var topic = new TestableTopic("topic");
            var subtopic1 = new TestableTopic("subtopic");
            var subtopic2 = new TestableTopic("subtopic");
            topic.AddSubtopic(subtopic1);

            Assert.That(() => topic.AddSubtopic(subtopic2), Throws.ArgumentException);
        }

        [Test]
        public void AddSubtopic_WhenAddingSelf_ThrowsInvalidOperationException()
        {
            var topic = new TestableTopic("topic");

            Assert.That(() => topic.AddSubtopic(topic), Throws.InvalidOperationException);
        }

        [Test]
        public void AddSubtopic_WhenAddingParentAsSubtopic_ThrowsInvalidOperationException()
        {
            var parent = new TestableTopic("parent");
            var child = new TestableTopic("child");
            parent.AddSubtopic(child);

            Assert.That(() => child.AddSubtopic(parent), Throws.InvalidOperationException);
        }

        [Test]
        public void AddSubtopic_AfterRemove_AddsSubtopicAgain()
        {
            var topic = new TestableTopic("topic");
            var subtopic = new TestableTopic("subtopic");
            topic.AddSubtopic(subtopic);
            topic.RemoveSubtopic(subtopic);

            Assert.That(() => topic.AddSubtopic(subtopic), Throws.Nothing);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(topic.Subtopics, Does.Contain(subtopic));
                Assert.That(subtopic.ParentTopic, Is.SameAs(topic));
            }
        }

        [Test]
        public void AddSubtopic_WhenSubtopicHasExistingParent_MovesToNewParent()
        {
            var parent1 = new TestableTopic("parent1");
            var parent2 = new TestableTopic("parent2");
            var child = new TestableTopic("child");
            parent1.AddSubtopic(child);
            parent2.AddSubtopic(child);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(parent1.Subtopics, Does.Not.Contain(child));
                Assert.That(parent2.Subtopics, Does.Contain(child));
                Assert.That(child.ParentTopic, Is.SameAs(parent2));
            }
        }

        [Test]
        public void RemoveSubtopic_WhenSubtopicExists_ReturnsTrueAndRemovesSubtopic()
        {
            var topic = new TestableTopic("topic");
            var subtopic = new TestableTopic("subtopic");
            topic.AddSubtopic(subtopic);

            var result = topic.RemoveSubtopic(subtopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(topic.Subtopics, Does.Not.Contain(subtopic));
                Assert.That(subtopic.ParentTopic, Is.Null);
            }
        }

        [Test]
        public void RemoveSubtopic_WhenSubtopicDoesNotExist_ReturnsFalse()
        {
            var topic = new TestableTopic("topic");
            var subtopic = new TestableTopic("subtopic");

            var result = topic.RemoveSubtopic(subtopic);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ToString_ReturnsTitle()
        {
            var topic = new TestableTopic("test-name");

            var result = topic.ToString();

            Assert.That(result, Is.EqualTo("Test Name"));
        }

        [Test]
        public void ITopic_ParentTopic_ReturnsParentTopic()
        {
            var parent = new TestableTopic("parent");
            ITopic topic = new TestableTopic("topic")
            {
                ParentTopic = parent
            };

            Assert.That(topic.ParentTopic, Is.SameAs(parent));
        }
    }
}

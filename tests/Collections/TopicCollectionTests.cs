// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Collections
{
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.Topics;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class TopicCollectionTests
    {
        private IDocumentationContext context;

        [SetUp]
        public void SetUp()
        {
            context = MockHelper.CreateDocumentationContext<HtmlFormat>();
        }

        [TearDown]
        public void TearDown()
        {
            context?.Dispose();
        }

        [Test]
        public void Constructor_WithContext_CreatesEmptyCollection()
        {
            var collection = new TopicCollection(context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection, Is.Empty);
                Assert.That(collection.Context, Is.SameAs(context));
                Assert.That(collection.Flatten, Is.Empty);
            }
        }

        [Test]
        public void Constructor_WithContextAndFactory_CreatesEmptyCollection()
        {
            static TopicModel factory(IDocumentationContext ctx, ITopic src) => new(ctx, src);
            var collection = new TopicCollection(context, factory);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection, Is.Empty);
                Assert.That(collection.Context, Is.SameAs(context));
                Assert.That(collection.Flatten, Is.Empty);
            }
        }

        [Test]
        public void Constructor_WithContextAndTopics_CreatesCollectionWithTopics()
        {
            var topics = new[] { CreateMockTopic("Topic1"), CreateMockTopicWithChildren("Topic2", ["Child1", "Child2"]) };
            var collection = new TopicCollection(context, topics);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection, Has.Count.EqualTo(2));
                Assert.That(collection.Context, Is.SameAs(context));
                Assert.That(collection.Flatten, Has.Count.EqualTo(4));
            }
        }

        [Test]
        public void Constructor_WithContextAndFactoryAndTopics_CreatesCollectionWithTopics()
        {
            static TopicModel factory(IDocumentationContext ctx, ITopic src) => new(ctx, src);
            var topics = new[] { CreateMockTopic("Topic1"), CreateMockTopicWithChildren("Topic2", ["Child1", "Child2"]) };
            var collection = new TopicCollection(context, factory, topics);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection, Has.Count.EqualTo(2));
                Assert.That(collection.Context, Is.SameAs(context));
                Assert.That(collection.Flatten, Has.Count.EqualTo(4));
            }
        }

        [Test]
        public void Add_WithValidTopLevelTopic_AddsTopicAndItsSubtopics()
        {
            var topic = CreateMockTopicWithChildren("Topic", "Subtopic1", "Subtopic2");
            var collection = new TopicCollection(context);

            var result = collection.Add(topic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(collection, Has.Count.EqualTo(1));
                Assert.That(collection.Flatten, Has.Count.EqualTo(3));
            }
        }

        [Test]
        public void Add_WithNullTopic_ThrowsArgumentNullException()
        {
            var collection = new TopicCollection(context);

            Assert.That(() => collection.Add(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("topLevelTopic"));
        }

        [Test]
        public void Add_WithNonTopLevelTopic_ThrowsArgumentException()
        {
            var topicWithParent = CreateMockTopicWithParent("Child", "Parent");
            var collection = new TopicCollection(context);

            Assert.That(() => collection.Add(topicWithParent),
                Throws.ArgumentException.With.Property("ParamName").EqualTo("topLevelTopic")
                .And.Message.Contains("top-level topic"));
        }

        [Test]
        public void Add_WithDuplicateId_IgnoresDuplicateAndReturnsFalse()
        {
            var topic1 = CreateMockTopic("Topic");
            var topic2 = CreateMockTopic("Topic");
            var collection = new TopicCollection(context)
            {
                topic1
            };

            var result = collection.Add(topic2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(collection, Has.Count.EqualTo(1));
            }
        }

        [Test]
        public void Add_WithCaseInsensitiveDuplicateId_IgnoresDuplicateAndReturnsFalse()
        {
            var topic1 = CreateMockTopic("Topic");
            var topic2 = CreateMockTopic("TOPIC");
            var collection = new TopicCollection(context)
            {
                topic1
            };

            var result = collection.Add(topic2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(collection, Has.Count.EqualTo(1));
            }
        }

        [Test]
        public void Remove_WithExistingTopic_RemovesTopicAndItsSubtopics()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopicWithChildren("Topic", "Subtopic1", "Subtopic2")
            };

            Assert.That(collection.TryGetById("Topic", out var contextualTopic), Is.True);

            var result = collection.Remove(contextualTopic!);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(collection, Is.Empty);
                Assert.That(collection.Flatten, Is.Empty);
            }
        }

        [Test]
        public void Remove_WithNullTopic_ThrowsArgumentNullException()
        {
            var collection = new TopicCollection(context);

            Assert.That(() => collection.Remove(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("topLevelTopic"));
        }

        [Test]
        public void Remove_WithNonTopLevelTopic_ThrowsArgumentException()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopicWithChildren("Parent", "Child1", "Child2")
            };

            Assert.That(collection.TryGetById("Parent/Child1", out var topicWithParent), Is.True);

            Assert.That(() => collection.Remove(topicWithParent!),
                Throws.ArgumentException.With.Property("ParamName").EqualTo("topLevelTopic")
                .And.Message.Contains("top-level topic"));
        }

        [Test]
        public void Remove_WithNonExistentTopic_ReturnsFalse()
        {
            var topic1 = CreateMockTopic("Topic1");
            var topic2 = CreateMockTopic("Topic2");
            var collection = new TopicCollection(context)
            {
                topic1
            };

            using var anotherContext = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var contextualTopic2 = new TopicModel(anotherContext, topic2);

            var result = collection.Remove(contextualTopic2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(collection, Has.Count.EqualTo(1));
            }
        }

        [Test]
        public void Clear_RemovesAllTopics()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopic("Topic1"),
                CreateMockTopicWithChildren("Topic2", "Child1", "Child2")
            };

            collection.Clear();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection, Is.Empty);
                Assert.That(collection.Flatten, Is.Empty);
            }
        }

        [Test]
        public void TryGetById_WithNullId_ThrowsArgumentNullException()
        {
            var collection = new TopicCollection(context);

            Assert.That(() => collection.TryGetById(null!, out _), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("id"));
        }

        [Test]
        public void TryGetById_WithNonExistentId_ReturnsFalse()
        {
            var collection = new TopicCollection(context);

            var result = collection.TryGetById("NonExistent", out var found);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(found, Is.Null);
            }
        }

        [Test]
        public void TryGetById_WithExistingTopicId_ReturnsTrueAndTopic()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopic("Topic")
            };

            var result = collection.TryGetById("Topic", out var found);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(found?.Id, Is.EqualTo("Topic"));
            }
        }

        [Test]
        public void TryGetById_WithExistingIdInDifferentCase_ReturnsTrueAndTopic()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopic("Topic")
            };

            var result = collection.TryGetById("TOPIC", out var found);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(found?.Id, Is.EqualTo("Topic"));
            }
        }

        [Test]
        public void TryGetById_WithExistingSubtopicId_ReturnsTrueAndSubtopic()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopicWithChildren("Parent", "Child")
            };

            var result = collection.TryGetById("Parent/Child", out var found);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(found?.Id, Is.EqualTo("Parent/Child"));
            }
        }

        [Test]
        public void TryGetByFilePath_WithNullPath_ThrowsArgumentNullException()
        {
            var collection = new TopicCollection(context);

            Assert.That(() => collection.TryGetByFilePath(null!, out _), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("filePath"));
        }

        [Test]
        public void TryGetFilePath_WithNonExistentPath_ReturnsFalse()
        {
            var collection = new TopicCollection(context);

            var result = collection.TryGetByFilePath("/non/existent/path.md", out var topic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(topic, Is.Null);
            }
        }

        [Test]
        public void TryGetByFilePath_WithExistingTopicPath_ReturnsTrueAndTopic()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("Topic", "/docs/topic.md")
            };

            var result = collection.TryGetByFilePath("/docs/topic.md", out var found);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(found?.Id, Is.EqualTo("Topic"));
            }
        }

        [Test]
        public void TryGetByFilePath_WithExistingTopicPathInDifferentCaseAndFormat_ReturnsTrueAndTopic()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("Topic", "/docs/topic.md")
            };

            var result = collection.TryGetByFilePath("\\DOCS\\TOPIC.md", out var found);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(found?.Id, Is.EqualTo("Topic"));
            }
        }

        [Test]
        public void TryGetByFilePath_WithExistingSubtopicPath_ReturnsTrueAndSubtopic()
        {
            var parent = new FileTopic("Parent", "/docs/parent.md");
            parent.AddSubtopic(new FileTopic("Child", "/docs/parent/child.md"));
            var collection = new TopicCollection(context)
            {
                parent
            };

            var result = collection.TryGetByFilePath("/docs/parent/child.md", out var found);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(found?.Id, Is.EqualTo("Parent/Child"));
            }
        }

        [Test]
        public void TryFindBySubpath_WithNullRelativePath_ThrowsArgumentException()
        {
            var collection = new TopicCollection(context);

            Assert.That(() => collection.TryFindBySubpath(null!, out _), Throws.ArgumentException
                .With.Property("ParamName").EqualTo("filePath"));
        }

        [Test]
        public void TryFindBySubpath_WithEmptyRelativePath_ThrowsArgumentException()
        {
            var collection = new TopicCollection(context);

            Assert.That(() => collection.TryFindBySubpath("", out _), Throws.ArgumentException
                .With.Property("ParamName").EqualTo("filePath"));
        }

        [Test]
        public void TryFindBySubpath_WithInvalidRelativePath_ReturnsNull()
        {
            var collection = new TopicCollection(context);

            var result = collection.TryFindBySubpath("invalid:path", out var found);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(found, Is.Null);
            }
        }

        [Test]
        public void TryFindBySubpath_WithNoMatchingTopics_ReturnsNull()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("Topic", "/docs/topic.md")
            };

            var result = collection.TryFindBySubpath("/non/matching/path", out var found);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(found, Is.Null);
            }
        }

        [Test]
        public void TryFindBySubpath_WithMatchingTopic_ReturnsMatchingTopics()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("Topic1", "/docs/folder/topic1.md"),
                new FileTopic("Topic2", "/docs/folder/subfolder/topic2.md"),
                new FileTopic("Topic3", "/other/folder/topic3.md")
            };

            var result = collection.TryFindBySubpath("subfolder/topic2.md", out var found);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(found?.Id, Is.EqualTo("Topic2"));
            }
        }

        [Test]
        public void TryFindBySubpath_WithDifferentCaseAndFormatPath_ReturnsMatchingTopics()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("Topic", "/docs/folder/topic.md")
            };

            var result = collection.TryFindBySubpath("FOLDER\\TOPIC.md", out var found);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(found?.Id, Is.EqualTo("Topic"));
            }
        }

        [Test]
        public void TryFindBySubpath_WithSubtopicsMatching_ReturnsMatchingSubtopic()
        {
            var parent = new FileTopic("Parent", "/docs/parent.md");
            parent.AddSubtopic(new FileTopic("Child", "/docs/parent/child.md"));
            var collection = new TopicCollection(context)
            {
                parent
            };

            var result = collection.TryFindBySubpath("parent/child.md", out var found);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(found?.Id, Is.EqualTo("Parent/Child"));
            }
        }

        [Test]
        public void TryFindBySubpath_WithMultipleMatches_ReturnsNull()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("Topic1", "/docs/folder/topic.md"),
                new FileTopic("Topic2", "/docs/other/topic.md")
            };

            var result = collection.TryFindBySubpath("topic.md", out var found);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(found, Is.Null);
            }
        }

        [Test]
        public void TryResolve_WithNullReference_ReturnsFalse()
        {
            var collection = new TopicCollection(context);

            var result = collection.TryResolve(null!, out var resolved);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(resolved, Is.Null);
            }
        }

        [Test]
        public void TryResolve_WithWhitespaceReference_ReturnsFalse()
        {
            var collection = new TopicCollection(context);

            var result = collection.TryResolve("   ", out var topic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(topic, Is.Null);
            }
        }

        [Test]
        public void TryResolve_EmptyCollection_ReturnsFalse()
        {
            var collection = new TopicCollection(context);

            var result = collection.TryResolve("any-topic", out var topic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(topic, Is.Null);
            }
        }

        [Test]
        public void TryResolve_NonExistentTopic_ReturnsFalse()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopic("Topic")
            };

            var result = collection.TryResolve("non-existent-topic", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(resolvedTopic, Is.Null);
            }
        }

        [Test]
        public void TryResolve_WithAbsoluteTopicId_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopicWithChildren("guides", "installation")
            };

            var result = collection.TryResolve("guides/installation", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("guides/installation"));
            }
        }

        [Test]
        public void TryResolve_WithRelativeTopicId_ToChild_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopicWithChildren("guides", "installation")
            };

            Assert.That(collection.TryGetById("guides", out var guidesContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics", guidesContextTopic);

            var result = collection.TryResolve("installation", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("guides/installation"));
            }
        }

        [Test]
        public void TryResolve_WithRelativeTopicId_ToParent_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopicWithChildren("guides", "installation")
            };

            Assert.That(collection.TryGetById("guides/installation", out var installationContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics/guides", installationContextTopic);

            var result = collection.TryResolve("..", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("guides"));
            }
        }

        [Test]
        public void TryResolve_WithRelativeTopicId_ToSibling_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopicWithChildren("guides", "installation", "advanced")
            };

            Assert.That(collection.TryGetById("guides/installation", out var installationContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics/guides", installationContextTopic);

            var result = collection.TryResolve("../advanced", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("guides/advanced"));
            }
        }

        [Test]
        public void TryResolve_WithRelativeTopicId_ToRoot_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopicWithChildren("guides", "installation"),
                CreateMockTopic("tutorials")
            };

            Assert.That(collection.TryGetById("guides/installation", out var installationContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics/guides", installationContextTopic);

            var result = collection.TryResolve("/tutorials", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("tutorials"));
            }
        }

        [Test]
        public void TryResolve_WithRelativeTopicId_OutOfScope_ReturnsFalse()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopicWithChildren("guides", "installation")
            };

            var result = collection.TryResolve("installation", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(resolvedTopic, Is.Null);
            }
        }

        [Test]
        public void TryResolve_WithAbsoluteFilePath_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/folder1/file1.md"),
                new FileTopic("topic2", "/docs/folder2/file2.md")
            };

            var result = collection.TryResolve("/docs/folder2/file2.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("topic2"));
            }
        }

        [Test]
        public void TryResolve_WithRelativeFilePath_SameDirectory_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/folder1/file1.md"),
                new FileTopic("topic2", "/docs/folder1/file2.md")
            };

            Assert.That(collection.TryGetById("topic1", out var topic1ContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics", topic1ContextTopic);

            var result = collection.TryResolve("file2.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("topic2"));
            }
        }

        [Test]
        public void TryResolve_WithRelativeFilePath_Subdirectory_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/folder1/file1.md"),
                new FileTopic("topic2", "/docs/folder1/subfolder/file2.md")
            };

            Assert.That(collection.TryGetById("topic1", out var topic1ContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics", topic1ContextTopic);

            var result = collection.TryResolve("subfolder/file2.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("topic2"));
            }
        }

        [Test]
        public void TryResolve_WithRelativeFilePath_ParentDirectory_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/folder1/file1.md"),
                new FileTopic("topic2", "/docs/folder1/subfolder/file2.md")
            };

            Assert.That(collection.TryGetById("topic2", out var topic2ContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics", topic2ContextTopic);

            var result = collection.TryResolve("../file1.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("topic1"));
            }
        }

        [Test]
        public void TryResolve_WithRelativeFilePath_SiblingDirectory_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/folder1/subfolder1/file1.md"),
                new FileTopic("topic2", "/docs/folder1/subfolder2/file2.md")
            };

            Assert.That(collection.TryGetById("topic1", out var topic1ContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics", topic1ContextTopic);

            var result = collection.TryResolve("../subfolder2/file2.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("topic2"));
            }
        }

        [Test]
        public void TryResolve_WithRelativeFilePath_MultipleParentDirectories_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/folder1/file1.md"),
                new FileTopic("topic2", "/docs/folder1/sub1/sub2/file2.md")
            };

            Assert.That(collection.TryGetById("topic2", out var topic2ContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics", topic2ContextTopic);

            var result = collection.TryResolve("../../file1.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("topic1"));
            }
        }

        [Test]
        public void TryResolve_WithBackslashFilePath_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/folder1/file1.md"),
                new FileTopic("topic2", "/docs/folder1/subfolder/file2.md")
            };

            Assert.That(collection.TryGetById("topic1", out var topic1ContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics", topic1ContextTopic);

            var result = collection.TryResolve(@"subfolder\file2.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("topic2"));
            }
        }

        [Test]
        public void TryResolve_WithMixedSlashesFilePath_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/folder1/file1.md"),
                new FileTopic("topic2", "/docs/folder1/sub1/sub2/file2.md")
            };

            Assert.That(collection.TryGetById("topic1", out var topic1ContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics", topic1ContextTopic);

            var result = collection.TryResolve(@"sub1\sub2/file2.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("topic2"));
            }
        }

        [Test]
        public void TryResolve_WithFilePathToRoot_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/file1.md"),
                new FileTopic("topic2", "/docs/folder1/subfolder/file2.md")
            };

            Assert.That(collection.TryGetById("topic2", out var topic2ContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics", topic2ContextTopic);

            var result = collection.TryResolve("/docs/file1.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("topic1"));
            }
        }

        [Test]
        public void TryResolve_WithNonExistentFilePath_ReturnsFalse()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/folder1/file1.md")
            };

            Assert.That(collection.TryGetById("topic1", out var topic1ContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics", topic1ContextTopic);

            var result = collection.TryResolve("nonexistent.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(resolvedTopic, Is.Null);
            }
        }

        [Test]
        public void TryResolve_WithFilePathOutOfScope_NonAmbiguousMatch_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/folder1/file1.md"),
                new FileTopic("topic2", "/docs/folder2/file2.md")
            };

            var result = collection.TryResolve("file2.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("topic2"));
            }
        }

        [Test]
        public void TryResolve_WithFilePathOutOfScope_AmbiguousMatch_ReturnsFalse()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/folder1/file1.md"),
                new FileTopic("topic2", "/docs/folder2/file1.md")
            };

            var result = collection.TryResolve("file1.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(resolvedTopic, Is.Null);
            }
        }

        [Test]
        public void TryResolve_WithFilePathNavigatingBeyondRoot_ReturnsFalse()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/file1.md")
            };

            Assert.That(collection.TryGetById("topic1", out var topic1ContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics", topic1ContextTopic);

            var result = collection.TryResolve("../../outside.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(resolvedTopic, Is.Null);
            }
        }

        [Test]
        public void TryResolve_WithFilePathWithDifferentExtension_ReturnsFalse()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/folder1/file1.md")
            };

            var result = collection.TryResolve("/docs/folder1/file1.txt", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(resolvedTopic, Is.Null);
            }
        }

        [Test]
        public void TryResolve_WithFilePathCaseInsensitive_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/Folder1/File1.md")
            };

            var result = collection.TryResolve("/docs/folder1/file1.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("topic1"));
            }
        }

        [Test]
        public void TryResolve_WithFilePathContainingDots_ResolvesCorrectly()
        {
            var collection = new TopicCollection(context)
            {
                new FileTopic("topic1", "/docs/folder1/file1.md"),
                new FileTopic("topic2", "/docs/folder1/sub.folder/file.name.md")
            };

            Assert.That(collection.TryGetById("topic1", out var topic1ContextTopic), Is.True);

            using var scope = context.AddressProvider.BeginScope("topics", topic1ContextTopic);

            var result = collection.TryResolve("sub.folder/file.name.md", out var resolvedTopic);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(resolvedTopic?.Id, Is.EqualTo("topic2"));
            }
        }

        [Test]
        public void GetEnumerator_EnumeratesTopLevelTopicsOnly()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopic("Topic1"),
                CreateMockTopicWithChildren("Topic2", "Child1", "Child2")
            };

            var enumerated = collection.ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(enumerated, Has.Count.EqualTo(2));
                Assert.That(enumerated.Select(static t => t.Id), Contains.Item("Topic1"));
                Assert.That(enumerated.Select(static t => t.Id), Contains.Item("Topic2"));
            }
        }

        [Test]
        public void Context_Property_ReturnsProvidedContext()
        {
            var collection = new TopicCollection(context);

            Assert.That(collection.Context, Is.SameAs(context));
        }

        [Test]
        public void Count_Property_ReturnsTopLevelTopicsCount()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopic("Topic1"),
                CreateMockTopicWithChildren("Topic2", "Child1", "Child2")
            };

            Assert.That(collection, Has.Count.EqualTo(2));
        }

        [Test]
        public void Flatten_Property_WithTopicsAndSubtopics_ReturnsAllTopics()
        {
            var collection = new TopicCollection(context)
            {
                CreateMockTopic("Topic1"),
                CreateMockTopicWithChildren("Topic2", "Child1", "Child2")
            };

            var flattened = collection.Flatten;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(flattened, Has.Count.EqualTo(4));
                Assert.That(flattened.Select(static t => t.Id), Contains.Item("Topic1"));
                Assert.That(flattened.Select(static t => t.Id), Contains.Item("Topic2"));
                Assert.That(flattened.Select(static t => t.Id), Contains.Item("Topic2/Child1"));
                Assert.That(flattened.Select(static t => t.Id), Contains.Item("Topic2/Child2"));
            }
        }

        private static ITopic CreateMockTopic(string id)
        {
            return MockTopicBuilder.Topic(id).Build();
        }

        private static ITopic CreateMockTopicWithChildren(string id, params string[] childIds)
        {
            return MockTopicBuilder.Topic(id).WithChildren(childIds).Build();
        }

        private static ITopic CreateMockTopicWithParent(string id, string parentId)
        {
            return MockTopicBuilder.Topic(id).WithParent(parentId).Build();
        }
    }
}
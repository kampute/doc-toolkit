// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Support
{
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class FileTopicHelperTests
    {
        #region ConstructHierarchyByDirectory Tests

        [Test]
        public void ConstructHierarchyByDirectory_WithEmptyCollection_ReturnsEmpty()
        {
            var topics = new List<FileTopic>();

            var result = FileTopicHelper.ConstructHierarchyByDirectory(topics);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ConstructHierarchyByDirectory_WithSingleTopic_ReturnsSingleTopic()
        {
            var topics = new List<FileTopic>
            {
                new("standalone", "standalone.md")
            };

            var result = FileTopicHelper.ConstructHierarchyByDirectory(topics).ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result[0].Id, Is.EqualTo("standalone"));
                Assert.That(result[0].ParentTopic, Is.Null);
            }
        }

        [Test]
        public void ConstructHierarchyByDirectory_WithParentChildPair_EstablishesRelationship()
        {
            var topics = new List<FileTopic>
            {
                new("guides", "guides.md"),
                new("intro", Path.Combine("guides", "intro.md"))
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByDirectory(topics).ToList();

            Assert.That(topLevelTopics, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(topLevelTopics[0].Id, Is.EqualTo("guides"));
                Assert.That(topLevelTopics[0].ParentTopic, Is.Null);
            }

            var childTopic = topics.First(static t => t.Id == "intro");
            Assert.That(childTopic.ParentTopic, Is.Not.Null);
            Assert.That(childTopic.ParentTopic!.Id, Is.EqualTo("guides"));
        }

        [Test]
        public void ConstructHierarchyByDirectory_WithMultipleChildrenInSameDirectory_EstablishesAllRelationships()
        {
            var topics = new List<FileTopic>
            {
                new("guides", "guides.md"),
                new("intro", Path.Combine("guides", "intro.md")),
                new("advanced", Path.Combine("guides", "advanced.md")),
                new("getting-started", Path.Combine("guides", "getting-started.md"))
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByDirectory(topics).ToList();

            Assert.That(topLevelTopics, Has.Count.EqualTo(1));
            Assert.That(topLevelTopics[0].Id, Is.EqualTo("guides"));

            var childTopics = topics.Where(static t => t.Id != "guides").ToList();
            Assert.That(childTopics, Has.Count.EqualTo(3));
            Assert.That(childTopics.All(static t => t.ParentTopic?.Id == "guides"), Is.True);
        }

        [Test]
        public void ConstructHierarchyByDirectory_WithNestedHierarchy_EstablishesMultipleLevels()
        {
            var topics = new List<FileTopic>
            {
                new("guides", "guides.md"),
                new("advanced", Path.Combine("guides", "advanced.md")),
                new("intro", Path.Combine("guides", "advanced", "intro.md")),
                new("scripting", Path.Combine("guides", "advanced", "scripting.md")),
                new("styling", Path.Combine("guides", "advanced", "styling.md"))
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByDirectory(topics).ToList();

            Assert.That(topLevelTopics, Has.Count.EqualTo(1));
            Assert.That(topLevelTopics[0].Id, Is.EqualTo("guides"));

            var advancedTopic = topics.First(static t => t.Id == "advanced");
            Assert.That(advancedTopic.ParentTopic?.Id, Is.EqualTo("guides"));

            var nestedTopics = topics.Where(static t => new[] { "intro", "scripting", "styling" }.Contains(t.Id)).ToList();
            Assert.That(nestedTopics, Has.Count.EqualTo(3));
            Assert.That(nestedTopics.All(static t => t.ParentTopic?.Id == "advanced"), Is.True);
        }

        [Test]
        public void ConstructHierarchyByDirectory_WithMultipleSeparateHierarchies_EstablishesAllCorrectly()
        {
            var topics = new List<FileTopic>
            {
                new("guides", "guides.md"),
                new("guides-intro", Path.Combine("guides", "intro.md")),
                new("tutorials", "tutorials.md"),
                new("tutorials-intro", Path.Combine("tutorials", "intro.md")),
                new("tutorials-setup", Path.Combine("tutorials", "setup.md")),
                new("standalone", "standalone.md")
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByDirectory(topics).ToList();

            Assert.That(topLevelTopics, Has.Count.EqualTo(3));
            Assert.That(topLevelTopics.Select(static t => t.Id), Is.EquivalentTo(["guides", "tutorials", "standalone"]));

            var guidesChildren = topics.Where(static t => t.Id == "guides-intro");
            Assert.That(guidesChildren.All(static t => t.ParentTopic?.Id == "guides"), Is.True);

            var tutorialsChildren = topics.Where(static t => new[] { "tutorials-intro", "tutorials-setup" }.Contains(t.Id));
            Assert.That(tutorialsChildren.All(static t => t.ParentTopic?.Id == "tutorials"), Is.True);

            var standaloneTopic = topics.First(static t => t.Id == "standalone");
            Assert.That(standaloneTopic.ParentTopic, Is.Null);
        }

        [Test]
        public void ConstructHierarchyByDirectory_WithOrphanedChildrenNoParent_LeavesChildrenAsTopLevel()
        {
            var topics = new List<FileTopic>
            {
                new("intro", Path.Combine("guides", "intro.md")),
                new("advanced", Path.Combine("guides", "advanced.md"))
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByDirectory(topics).ToList();

            Assert.That(topLevelTopics, Has.Count.EqualTo(2));
            Assert.That(topLevelTopics.All(static t => t.ParentTopic == null), Is.True);
        }

        [Test]
        public void ConstructHierarchyByDirectory_WithCaseInsensitiveMatching_EstablishesRelationship()
        {
            var topics = new List<FileTopic>
            {
                new("Guides", "Guides.md"),
                new("intro", Path.Combine("guides", "intro.md"))
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByDirectory(topics).ToList();

            Assert.That(topLevelTopics, Has.Count.EqualTo(1));
            Assert.That(topLevelTopics[0].Id, Is.EqualTo("Guides"));

            var childTopic = topics.First(static t => t.Id == "intro");
            Assert.That(childTopic.ParentTopic?.Id, Is.EqualTo("Guides"));
        }

        [Test]
        public void ConstructHierarchyByDirectory_WithDifferentFileExtensions_EstablishesRelationship()
        {
            var topics = new List<FileTopic>
            {
                new("guides", "guides.html"),
                new("intro", Path.Combine("guides", "intro.md"))
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByDirectory(topics).ToList();

            Assert.That(topLevelTopics, Has.Count.EqualTo(1));
            Assert.That(topLevelTopics[0].Id, Is.EqualTo("guides"));

            var childTopic = topics.First(static t => t.Id == "intro");
            Assert.That(childTopic.ParentTopic?.Id, Is.EqualTo("guides"));
        }

        [Test]
        public void ConstructHierarchyByDirectory_WithRootLevelFiles_DoesNotCreateParentRelationships()
        {
            var topics = new List<FileTopic>
            {
                new("file1", "file1.md"),
                new("file2", "file2.md"),
                new("file3", "file3.md")
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByDirectory(topics).ToList();

            Assert.That(topLevelTopics, Has.Count.EqualTo(3));
            Assert.That(topLevelTopics.All(static t => t.ParentTopic == null), Is.True);
        }

        [Test]
        public void ConstructHierarchyByDirectory_WithComplexNestedStructure_EstablishesCorrectHierarchy()
        {
            var topics = new List<FileTopic>
            {
                new("docs", "docs.md"),
                new("api", Path.Combine("docs", "api.md")),
                new("types", Path.Combine("docs", "api", "types.md")),
                new("class1", Path.Combine("docs", "api", "types", "class1.md")),
                new("class2", Path.Combine("docs", "api", "types", "class2.md")),
                new("methods", Path.Combine("docs", "api", "methods.md")),
                new("method1", Path.Combine("docs", "api", "methods", "method1.md")),
                new("tutorials", Path.Combine("docs", "tutorials.md")),
                new("basic", Path.Combine("docs", "tutorials", "basic.md"))
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByDirectory(topics).ToList();

            // Should have only one top-level topic
            Assert.That(topLevelTopics, Has.Count.EqualTo(1));
            Assert.That(topLevelTopics[0].Id, Is.EqualTo("docs"));

            // Check API branch
            var apiTopic = topics.First(static t => t.Id == "api");
            Assert.That(apiTopic.ParentTopic?.Id, Is.EqualTo("docs"));

            var typesTopic = topics.First(static t => t.Id == "types");
            Assert.That(typesTopic.ParentTopic?.Id, Is.EqualTo("api"));

            var classTopics = topics.Where(static t => new[] { "class1", "class2" }.Contains(t.Id));
            Assert.That(classTopics.All(static t => t.ParentTopic?.Id == "types"), Is.True);

            var methodsTopic = topics.First(static t => t.Id == "methods");
            Assert.That(methodsTopic.ParentTopic?.Id, Is.EqualTo("api"));

            var methodTopic = topics.First(static t => t.Id == "method1");
            Assert.That(methodTopic.ParentTopic?.Id, Is.EqualTo("methods"));

            // Check tutorials branch
            var tutorialsTopic = topics.First(static t => t.Id == "tutorials");
            Assert.That(tutorialsTopic.ParentTopic?.Id, Is.EqualTo("docs"));

            var basicTopic = topics.First(static t => t.Id == "basic");
            Assert.That(basicTopic.ParentTopic?.Id, Is.EqualTo("tutorials"));
        }

        [Test]
        public void ConstructHierarchyByDirectory_WithMixedStandaloneAndHierarchicalTopics_ReturnsCorrectTopLevelTopics()
        {
            var topics = new List<FileTopic>
            {
                new("guides", "guides.md"),
                new("intro", Path.Combine("guides", "intro.md")),
                new("standalone1", "standalone1.md"),
                new("tutorials", "tutorials.md"),
                new("setup", Path.Combine("tutorials", "setup.md")),
                new("standalone2", "standalone2.md")
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByDirectory(topics).ToList();

            Assert.That(topLevelTopics, Has.Count.EqualTo(4));
            Assert.That(topLevelTopics.Select(static t => t.Id), Is.EquivalentTo(["guides", "standalone1", "standalone2", "tutorials"]));

            var introTopic = topics.First(static t => t.Id == "intro");
            Assert.That(introTopic.ParentTopic?.Id, Is.EqualTo("guides"));

            var setupTopic = topics.First(static t => t.Id == "setup");
            Assert.That(setupTopic.ParentTopic?.Id, Is.EqualTo("tutorials"));
        }

        [Test]
        public void ConstructHierarchyByDirectory_PreservesTopLevelOrder()
        {
            var topics = new List<FileTopic>
            {
                new("zebra", "zebra.md"),
                new("alpha", "alpha.md"),
                new("beta", "beta.md"),
                new("gamma", "gamma.md")
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByDirectory(topics);

            Assert.That(topLevelTopics.Select(static t => t.Id), Is.EqualTo(["zebra", "alpha", "beta", "gamma"]));
        }

        [Test]
        public void ConstructHierarchyByDirectory_PreservesChildOrderWithinParent()
        {
            var topics = new List<FileTopic>
            {
                new("parent", "parent.md"),
                new("child-z", Path.Combine("parent", "child-z.md")),
                new("child-a", Path.Combine("parent", "child-a.md")),
                new("child-m", Path.Combine("parent", "child-m.md"))
            };

            var _ = FileTopicHelper.ConstructHierarchyByDirectory(topics).ToList();

            var parentTopic = topics.First(t => t.Id == "parent");
            var children = topics.Where(t => t.ParentTopic == parentTopic);

            Assert.That(children.Select(t => t.Id), Is.EqualTo(["child-z", "child-a", "child-m"]));
        }

        #endregion

        #region ConstructHierarchyByIndexFile Tests

        [Test]
        public void ConstructHierarchyByIndexFile_WithNullTopics_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(static () => FileTopicHelper.ConstructHierarchyByIndexFile(null!, "index"));
        }

        [Test]
        public void ConstructHierarchyByIndexFile_WithNullIndexFileName_ThrowsArgumentException()
        {
            var topics = new List<FileTopic>();
            Assert.Throws<ArgumentException>(() => FileTopicHelper.ConstructHierarchyByIndexFile(topics, null!));
        }

        [Test]
        public void ConstructHierarchyByIndexFile_WithEmptyIndexFileName_ThrowsArgumentException()
        {
            var topics = new List<FileTopic>();
            Assert.Throws<ArgumentException>(() => FileTopicHelper.ConstructHierarchyByIndexFile(topics, ""));
        }

        [Test]
        public void ConstructHierarchyByIndexFile_WithIndexFiles_EstablishesHierarchy()
        {
            var topics = new List<FileTopic>
            {
                new("index", Path.Combine("guides", "index.md")),
                new("intro", Path.Combine("guides", "intro.md")),
                new("advanced", Path.Combine("guides", "advanced.md")),
                new("readme", Path.Combine("tutorials", "README.md")),
                new("setup", Path.Combine("tutorials", "setup.md")),
                new("standalone", "standalone.md")
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByIndexFile(topics, "index").ToList();

            Assert.That(topLevelTopics, Has.Count.EqualTo(4)); // index, README, setup, standalone

            var indexTopic = topics.First(static t => t.Id == "index");
            var introTopic = topics.First(static t => t.Id == "intro");
            var advancedTopic = topics.First(static t => t.Id == "advanced");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(introTopic.ParentTopic, Is.EqualTo(indexTopic));
                Assert.That(advancedTopic.ParentTopic, Is.EqualTo(indexTopic));
            }
        }

        [Test]
        public void ConstructHierarchyByIndexFile_WithReadMeFiles_EstablishesHierarchy()
        {
            var topics = new List<FileTopic>
            {
                new("readme", Path.Combine("docs", "README.md")),
                new("guide1", Path.Combine("docs", "guide1.md")),
                new("guide2", Path.Combine("docs", "guide2.md"))
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByIndexFile(topics, "README").ToList();

            Assert.That(topLevelTopics, Has.Count.EqualTo(1));

            var readmeTopic = topics.First(static t => t.Id == "readme");
            var guide1Topic = topics.First(static t => t.Id == "guide1");
            var guide2Topic = topics.First(static t => t.Id == "guide2");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(guide1Topic.ParentTopic, Is.EqualTo(readmeTopic));
                Assert.That(guide2Topic.ParentTopic, Is.EqualTo(readmeTopic));
            }
        }

        [Test]
        public void ConstructHierarchyByIndexFile_PreservesTopLevelOrder()
        {
            var topics = new List<FileTopic>
            {
                new("standalone-z", "standalone-z.md"),
                new("index-section", Path.Combine("section", "index.md")),
                new("standalone-a", "standalone-a.md"),
                new("child", Path.Combine("section", "child.md"))
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByIndexFile(topics, "index");

            Assert.That(topLevelTopics.Select(static t => t.Id), Is.EqualTo(["standalone-z", "index-section", "standalone-a"]));
        }

        [Test]
        public void ConstructHierarchyByIndexFile_PreservesChildOrderWithinParent()
        {
            var topics = new List<FileTopic>
            {
                new("index", Path.Combine("docs", "index.md")),
                new("zebra", Path.Combine("docs", "zebra.md")),
                new("alpha", Path.Combine("docs", "alpha.md")),
                new("beta", Path.Combine("docs", "beta.md"))
            };

            var _ = FileTopicHelper.ConstructHierarchyByIndexFile(topics, "index").ToList();

            var indexTopic = topics.First(t => t.Id == "index");
            var children = topics.Where(t => t.ParentTopic == indexTopic);

            Assert.That(children.Select(t => t.Id), Is.EqualTo(["zebra", "alpha", "beta"]));
        }

        #endregion

        #region ConstructHierarchyByFilenamePrefix Tests

        [Test]
        public void ConstructHierarchyByFilenamePrefix_WithNullTopics_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(static () => FileTopicHelper.ConstructHierarchyByFilenamePrefix(null!, '.'));
        }

        [Test]
        public void ConstructHierarchyByFilenamePrefix_WithDotDelimiter_EstablishesHierarchy()
        {
            var topics = new List<FileTopic>
            {
                new("guides", "guides.md"),
                new("guides-intro", "guides.intro.md"),
                new("guides-advanced", "guides.advanced.md"),
                new("guides-advanced-scripting", "guides.advanced.scripting.md"),
                new("tutorials", "tutorials.md"),
                new("tutorials-setup", "tutorials.setup.md"),
                new("standalone", "standalone.md")
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByFilenamePrefix(topics, '.').ToList();

            Assert.That(topLevelTopics, Has.Count.EqualTo(3)); // guides, tutorials, standalone

            var guidesTopic = topics.First(static t => t.Id == "guides");
            var guidesIntroTopic = topics.First(static t => t.Id == "guides-intro");
            var guidesAdvancedTopic = topics.First(static t => t.Id == "guides-advanced");
            var guidesAdvancedScriptingTopic = topics.First(static t => t.Id == "guides-advanced-scripting");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(guidesIntroTopic.ParentTopic, Is.EqualTo(guidesTopic));
                Assert.That(guidesAdvancedTopic.ParentTopic, Is.EqualTo(guidesTopic));
                Assert.That(guidesAdvancedScriptingTopic.ParentTopic, Is.EqualTo(guidesAdvancedTopic));
            }
        }

        [Test]
        public void ConstructHierarchyByFilenamePrefix_WithDashDelimiter_EstablishesHierarchy()
        {
            var topics = new List<FileTopic>
            {
                new("api", "api.md"),
                new("api-types", "api-types.md"),
                new("api-types-class", "api-types-class.md"),
                new("api-methods", "api-methods.md")
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByFilenamePrefix(topics, '-').ToList();

            Assert.That(topLevelTopics, Has.Count.EqualTo(1)); // api

            var apiTopic = topics.First(static t => t.Id == "api");
            var apiTypesTopic = topics.First(static t => t.Id == "api-types");
            var apiTypesClassTopic = topics.First(static t => t.Id == "api-types-class");
            var apiMethodsTopic = topics.First(static t => t.Id == "api-methods");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(apiTypesTopic.ParentTopic, Is.EqualTo(apiTopic));
                Assert.That(apiMethodsTopic.ParentTopic, Is.EqualTo(apiTopic));
                Assert.That(apiTypesClassTopic.ParentTopic, Is.EqualTo(apiTypesTopic));
            }
        }

        [Test]
        public void ConstructHierarchyByFilenamePrefix_PreservesTopLevelOrder()
        {
            var topics = new List<FileTopic>
            {
                new("zebra", "zebra.md"),
                new("alpha", "alpha.md"),
                new("beta", "beta.md"),
                new("gamma", "gamma.md")
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByFilenamePrefix(topics, '.');

            Assert.That(topLevelTopics.Select(static t => t.Id), Is.EqualTo(["zebra", "alpha", "beta", "gamma"]));
        }

        [Test]
        public void ConstructHierarchyByFilenamePrefix_PreservesChildOrderWithinParent()
        {
            var topics = new List<FileTopic>
            {
                new("parent", "parent.md"),
                new("parent-zebra", "parent.zebra.md"),
                new("parent-alpha", "parent.alpha.md"),
                new("parent-beta", "parent.beta.md")
            };

            var _ = FileTopicHelper.ConstructHierarchyByFilenamePrefix(topics, '.').ToList();

            var parentTopic = topics.First(t => t.Id == "parent");
            var children = topics.Where(t => t.ParentTopic == parentTopic);

            Assert.That(children.Select(t => t.Id), Is.EqualTo(["parent-zebra", "parent-alpha", "parent-beta"]));
        }

        [Test]
        public void ConstructHierarchyByFilenamePrefix_PreservesOrderInMultiLevelHierarchy()
        {
            var topics = new List<FileTopic>
            {
                new("z", "z.md"),
                new("a", "a.md"),
                new("a-z", "a.z.md"),
                new("a-a", "a.a.md"),
                new("a-z-final", "a.z.final.md"),
                new("a-z-initial", "a.z.initial.md"),
                new("b", "b.md")
            };

            var topLevelTopics = FileTopicHelper.ConstructHierarchyByFilenamePrefix(topics, '.').ToList();

            // Check top-level order
            Assert.That(topLevelTopics.Select(static t => t.Id), Is.EqualTo(["z", "a", "b"]));

            // Check second-level order under 'a'
            var aTopic = topics.First(static t => t.Id == "a");
            Assert.That(aTopic.Subtopics.Select(static t => t.Id), Is.EqualTo(["a-z", "a-a"]));

            // Check third-level order under 'a.z'
            var azTopic = topics.First(static t => t.Id == "a-z");
            Assert.That(azTopic.Subtopics.Select(static t => t.Id), Is.EqualTo(["a-z-final", "a-z-initial"]));
        }

        #endregion
    }
}

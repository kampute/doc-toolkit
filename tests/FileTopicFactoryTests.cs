// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Topics;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class FileTopicFactoryTests
    {
        private string folder = null!;

        [SetUp]
        public void Setup()
        {
            folder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(folder);
            UnregisterTestTopics();
        }

        [TearDown]
        public void TearDown()
        {
            UnregisterTestTopics(); // The registry is global, so registrations must not leak into other tests.

            if (Directory.Exists(folder))
                Directory.Delete(folder, recursive: true);
        }

        private static void UnregisterTestTopics()
        {
            FileTopicFactory.Unregister<TestTopic>();
            FileTopicFactory.Unregister<AnotherTestTopic>();
        }

        private string CreateFile(string fileName)
        {
            var path = Path.Combine(folder, fileName);
            File.WriteAllText(path, "Test content");
            return path;
        }

        [Test]
        public void Register_RegistersTopicForMultipleExtensions()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test", ".tst");

            var topic1 = FileTopicFactory.Create(CreateFile("test.test"));
            var topic2 = FileTopicFactory.Create(CreateFile("test.tst"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(topic1, Is.TypeOf<TestTopic>());
                Assert.That(topic2, Is.TypeOf<TestTopic>());
            }
        }

        [Test]
        public void Register_OverwritesExistingRegistration()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test");
            FileTopicFactory.Register(static (id, path) => new AnotherTestTopic(id, path), ".test");

            var topic = FileTopicFactory.Create(CreateFile("test.test"));

            Assert.That(topic, Is.TypeOf<AnotherTestTopic>());
        }

        [Test]
        public void Unregister_ByTopicType_RemovesAllAssociatedExtensions()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test", ".tst");

            FileTopicFactory.Unregister<TestTopic>();

            var topic1 = FileTopicFactory.Create(CreateFile("test.test"));
            var topic2 = FileTopicFactory.Create(CreateFile("test.tst"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(topic1, Is.TypeOf<FileTopic>());
                Assert.That(topic2, Is.TypeOf<FileTopic>());
            }
        }

        [Test]
        public void Unregister_ByTopicType_RemovesOnlySpecifiedTopicType()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test");
            FileTopicFactory.Register(static (id, path) => new AnotherTestTopic(id, path), ".another");

            FileTopicFactory.Unregister<TestTopic>();

            var topic1 = FileTopicFactory.Create(CreateFile("test.test"));
            var topic2 = FileTopicFactory.Create(CreateFile("test.another"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(topic1, Is.TypeOf<FileTopic>());
                Assert.That(topic2, Is.TypeOf<AnotherTestTopic>());
            }
        }

        [Test]
        public void Unregister_ByExtension_RemovesOnlySpecifiedExtension()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test", ".tst");

            FileTopicFactory.Unregister(".test");

            var topic1 = FileTopicFactory.Create(CreateFile("test.test"));
            var topic2 = FileTopicFactory.Create(CreateFile("test.tst"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(topic1, Is.TypeOf<FileTopic>());
                Assert.That(topic2, Is.TypeOf<TestTopic>());
            }
        }

        [Test]
        public void IsRegistered_ForRegisteredExtensions_ReturnsTrue()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test");

            Assert.That(FileTopicFactory.IsRegistered(".test"), Is.True);
        }

        [Test]
        public void IsRegistered_ForUnregisteredExtensions__ReturnsFalse()
        {
            Assert.That(FileTopicFactory.IsRegistered(".unknown"), Is.False);
        }

        [Test]
        public void IsRegistered_IsCaseInsensitive()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".TEST");

            Assert.That(FileTopicFactory.IsRegistered(".test"), Is.True);
        }

        [Test]
        public void Create_WithDefaultName_UsesFileNameWithoutExtensionAsId()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test");
            var filePath = CreateFile("getting-started.test");

            var topic = FileTopicFactory.Create(filePath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(topic, Is.TypeOf<TestTopic>());
                Assert.That(topic.Id, Is.EqualTo("getting-started"));
                Assert.That(topic.FilePath, Is.EqualTo(filePath));
            }
        }

        [Test]
        public void Create_WithCustomName_ReturnsCorrectTopicType()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test");
            var filePath = CreateFile("test.test");

            var topic = FileTopicFactory.Create("test-topic", filePath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(topic, Is.TypeOf<TestTopic>());
                Assert.That(topic.Id, Is.EqualTo("test-topic"));
                Assert.That(topic.FilePath, Is.EqualTo(filePath));
            }
        }

        [Test]
        public void Create_WithUnregisteredExtension_ReturnsPlainFileTopicWithIdAndPath()
        {
            var filePath = CreateFile("notes.txt");

            var topic = FileTopicFactory.Create("release-notes", filePath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(topic, Is.TypeOf<FileTopic>());
                Assert.That(topic.Id, Is.EqualTo("release-notes"));
                Assert.That(topic.FilePath, Is.EqualTo(filePath));
            }
        }

        [TestCase(".md", ExpectedResult = typeof(MarkdownFileTopic))]
        [TestCase(".markdown", ExpectedResult = typeof(MarkdownFileTopic))]
        [TestCase(".html", ExpectedResult = typeof(HtmlFileTopic))]
        [TestCase(".htm", ExpectedResult = typeof(HtmlFileTopic))]
        [TestCase(".xhtml", ExpectedResult = typeof(HtmlFileTopic))]
        public Type BuiltInTopics_AreRegisteredByDefault(string extension)
        {
            var topic = FileTopicFactory.Create(CreateFile($"test{extension}"));

            return topic.GetType();
        }

        private class TestTopic : FileTopic
        {
            public TestTopic(string id, string filePath) : base(id, filePath) { }
        }

        private class AnotherTestTopic : FileTopic
        {
            public AnotherTestTopic(string id, string filePath) : base(id, filePath) { }
        }
    }
}

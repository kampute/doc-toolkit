// Copyright (C) 2025 Kampute
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
        [SetUp]
        public void Setup()
        {
            FileTopicFactory.Unregister<TestTopic>();
            FileTopicFactory.Unregister<AnotherTestTopic>();
        }

        [Test]
        public void Register_RegistersTopicForMultipleExtensions()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test", ".tst");

            var testFilePath = Path.Combine(Path.GetTempPath(), "test.test");
            var tstFilePath = Path.Combine(Path.GetTempPath(), "test.tst");
            try
            {
                File.WriteAllText(testFilePath, "Test content");
                File.WriteAllText(tstFilePath, "Test content");

                var topic1 = FileTopicFactory.Create(testFilePath);
                var topic2 = FileTopicFactory.Create(tstFilePath);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(topic1, Is.TypeOf<TestTopic>());
                    Assert.That(topic2, Is.TypeOf<TestTopic>());
                }
            }
            finally
            {
                if (File.Exists(testFilePath)) File.Delete(testFilePath);
                if (File.Exists(tstFilePath)) File.Delete(tstFilePath);
            }
        }

        [Test]
        public void Register_OverwritesExistingRegistration()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test");
            FileTopicFactory.Register(static (id, path) => new AnotherTestTopic(id, path), ".test");

            var testFilePath = Path.Combine(Path.GetTempPath(), "test.test");
            try
            {
                File.WriteAllText(testFilePath, "Test content");

                var topic = FileTopicFactory.Create(testFilePath);

                Assert.That(topic, Is.TypeOf<AnotherTestTopic>());
            }
            finally
            {
                if (File.Exists(testFilePath)) File.Delete(testFilePath);
            }
        }

        [Test]
        public void Unregister_ByTopicType_RemovesAllAssociatedExtensions()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test", ".tst");

            FileTopicFactory.Unregister<TestTopic>();

            var testFilePath = Path.Combine(Path.GetTempPath(), "test.test");
            var tstFilePath = Path.Combine(Path.GetTempPath(), "test.tst");
            try
            {
                File.WriteAllText(testFilePath, "Test content");
                File.WriteAllText(tstFilePath, "Test content");

                var topic1 = FileTopicFactory.Create(testFilePath);
                var topic2 = FileTopicFactory.Create(tstFilePath);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(topic1, Is.TypeOf<FileTopic>());
                    Assert.That(topic2, Is.TypeOf<FileTopic>());
                }
            }
            finally
            {
                if (File.Exists(testFilePath)) File.Delete(testFilePath);
                if (File.Exists(tstFilePath)) File.Delete(tstFilePath);
            }
        }

        [Test]
        public void Unregister_ByTopicType_RemovesOnlySpecifiedTopicType()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test");
            FileTopicFactory.Register(static (id, path) => new AnotherTestTopic(id, path), ".another");

            FileTopicFactory.Unregister<TestTopic>();

            var testFilePath = Path.Combine(Path.GetTempPath(), "test.test");
            var anotherFilePath = Path.Combine(Path.GetTempPath(), "test.another");
            try
            {
                File.WriteAllText(testFilePath, "Test content");
                File.WriteAllText(anotherFilePath, "Test content");

                var topic1 = FileTopicFactory.Create(testFilePath);
                var topic2 = FileTopicFactory.Create(anotherFilePath);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(topic1, Is.TypeOf<FileTopic>());
                    Assert.That(topic2, Is.TypeOf<AnotherTestTopic>());
                }
            }
            finally
            {
                if (File.Exists(testFilePath)) File.Delete(testFilePath);
                if (File.Exists(anotherFilePath)) File.Delete(anotherFilePath);
            }
        }

        [Test]
        public void Unregister_ByExtension_RemovesOnlySpecifiedExtension()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test", ".tst");

            FileTopicFactory.Unregister(".test");

            var testFilePath = Path.Combine(Path.GetTempPath(), "test.test");
            var tstFilePath = Path.Combine(Path.GetTempPath(), "test.tst");
            try
            {
                File.WriteAllText(testFilePath, "Test content");
                File.WriteAllText(tstFilePath, "Test content");

                var topic1 = FileTopicFactory.Create(testFilePath);
                var topic2 = FileTopicFactory.Create(tstFilePath);

                using (Assert.EnterMultipleScope())
                {
                    Assert.That(topic1, Is.TypeOf<FileTopic>());
                    Assert.That(topic2, Is.TypeOf<TestTopic>());
                }
            }
            finally
            {
                if (File.Exists(testFilePath)) File.Delete(testFilePath);
                if (File.Exists(tstFilePath)) File.Delete(tstFilePath);
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
        public void Create_WithDefaultName_ReturnsCorrectTopicType()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test");

            var testFilePath = Path.Combine(Path.GetTempPath(), "test.test");
            try
            {
                File.WriteAllText(testFilePath, "Test content");

                var topic = FileTopicFactory.Create(testFilePath);

                Assert.That(topic, Is.TypeOf<TestTopic>());
            }
            finally
            {
                if (File.Exists(testFilePath)) File.Delete(testFilePath);
            }
        }

        [Test]
        public void Create_WithCustomName_ReturnsCorrectTopicType()
        {
            FileTopicFactory.Register(static (id, path) => new TestTopic(id, path), ".test");

            var testFilePath = Path.Combine(Path.GetTempPath(), "test.test");
            try
            {
                File.WriteAllText(testFilePath, "Test content");

                var topic = FileTopicFactory.Create("test-topic", testFilePath);

                Assert.That(topic, Is.TypeOf<TestTopic>());
                Assert.That(topic.Id, Is.EqualTo("test-topic"));
            }
            finally
            {
                if (File.Exists(testFilePath)) File.Delete(testFilePath);
            }
        }

        [TestCase(".md", ExpectedResult = typeof(MarkdownFileTopic))]
        [TestCase(".markdown", ExpectedResult = typeof(MarkdownFileTopic))]
        [TestCase(".html", ExpectedResult = typeof(HtmlFileTopic))]
        [TestCase(".htm", ExpectedResult = typeof(HtmlFileTopic))]
        [TestCase(".xhtml", ExpectedResult = typeof(HtmlFileTopic))]
        public Type BuiltInTopics_AreRegisteredByDefault(string extension)
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"test{extension}");
            try
            {
                File.WriteAllText(tempFilePath, "Test content");

                var topic = FileTopicFactory.Create(tempFilePath);

                return topic.GetType();
            }
            finally
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
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
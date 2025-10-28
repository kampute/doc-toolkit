// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Topics
{
    using Kampute.DocToolkit.Topics;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class MarkdownFileTopicTests
    {
        private readonly string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        [TestCase("no-title.md", "", ExpectedResult = "No Title")]
        [TestCase("no-title.md", "# ", ExpectedResult = "No Title")]
        [TestCase("no-title.md", "This is content without a title.", ExpectedResult = "No Title")]
        [TestCase("test-file.md", "# Simple Title", ExpectedResult = "Simple Title")]
        [TestCase("test-file.md", "#Simple Title", ExpectedResult = "Simple Title")]
        [TestCase("test-file.md", "  # Title with leading whitespace", ExpectedResult = "Title with leading whitespace")]
        [TestCase("test-file.md", "\n\n# Title after line breaks", ExpectedResult = "Title after line breaks")]
        [TestCase("test-file.md", "# Title with trailing whitespace  ", ExpectedResult = "Title with trailing whitespace")]
        [TestCase("test-file.md", "Content before title\n# Title in the middle", ExpectedResult = "Test File")]
        [TestCase("test-file.md", "# First Title\n## Second level heading", ExpectedResult = "First Title")]
        [TestCase("test-file.md", "# Title with symbols: &@#!?", ExpectedResult = "Title with symbols: &@#!?")]
        public string? Title_ReturnsExpectedTitle(string fileName, string content)
        {
            var path = Path.Combine(tempDir, fileName);
            File.WriteAllText(path, content);

            var topic = new MarkdownFileTopic(Path.GetFileNameWithoutExtension(path), path);

            return topic.Title;
        }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Support
{
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class PathHelperTests
    {
        [TestCase(null, ExpectedResult = "")]
        [TestCase("", ExpectedResult = "")]
        [TestCase(@"folder/file.txt", ExpectedResult = "folder/file.txt")]
        [TestCase(@"folder\file.txt", ExpectedResult = "folder/file.txt")]
        [TestCase(@"folder/", ExpectedResult = "folder")]
        [TestCase(@"folder\", ExpectedResult = "folder")]
        public string EnsureValidRelativePath_WithValidInput_ReturnsNormalizedPath(string? path)
        {
            return PathHelper.EnsureValidRelativePath(path);
        }

        [TestCase(@"\folder\file.txt")]
        [TestCase(@"/folder/file.txt")]
        public void EnsureValidRelativePath_WithAbsolutePath_ThrowsArgumentException(string path)
        {
            Assert.That(() => PathHelper.EnsureValidRelativePath(path), Throws.ArgumentException
                .With.Message.Contain("must be a relative path"));
        }

        [Test]
        public void EnsureValidRelativePath_WithInvalidCharacters_ThrowsArgumentException()
        {
            var invalidChars = Path.GetInvalidPathChars();
            if (invalidChars.Length == 0)
            {
                Assert.Inconclusive("No invalid path characters found on this platform.");
                return;
            }

            var path = $"folder{invalidChars[0]}/file.txt";

            Assert.That(() => PathHelper.EnsureValidRelativePath(path), Throws.ArgumentException
                .With.Message.Contain("contains invalid characters"));
        }

        [Test]
        public void EnsureValidRelativePath_WithCustomCallerName_UsesCallerNameInException()
        {
            var path = @"/folder/file.txt";
            var customCaller = "CustomMethod";

            Assert.That(() => PathHelper.EnsureValidRelativePath(path, customCaller), Throws.ArgumentException
                .With.Property("ParamName").EqualTo(customCaller));
        }

        [TestCase("", ExpectedResult = "")]
        [TestCase("simple/path", ExpectedResult = "simple/path")]
        [TestCase("/absolute/path", ExpectedResult = "/absolute/path")]
        [TestCase("./current", ExpectedResult = "current")]
        [TestCase("path/./current", ExpectedResult = "path/current")]
        [TestCase("./path/current", ExpectedResult = "path/current")]
        [TestCase("path/current/.", ExpectedResult = "path/current")]
        [TestCase("../parent", ExpectedResult = "../parent")]
        [TestCase("path/..", ExpectedResult = "")]
        [TestCase("path/../parent", ExpectedResult = "parent")]
        [TestCase("path/subpath/../parent", ExpectedResult = "path/parent")]
        [TestCase("/path/../parent", ExpectedResult = "/parent")]
        [TestCase("/path/subpath/../parent", ExpectedResult = "/path/parent")]
        [TestCase("/./path/../sibling", ExpectedResult = "/sibling")]
        [TestCase("../../grandparent", ExpectedResult = "../../grandparent")]
        [TestCase("path/../../grandparent", ExpectedResult = "../grandparent")]
        [TestCase("path/./subpath/../parent", ExpectedResult = "path/parent")]
        [TestCase("./path/../current/./file", ExpectedResult = "current/file")]
        [TestCase("a/b/c/../../../d", ExpectedResult = "d")]
        [TestCase("/a/b/c/../../../d", ExpectedResult = "/d")]
        public string? TryNormalizePath_NormalizesPath(string path)
        {
            PathHelper.TryNormalizePath(path, out var normalizedPath);
            return normalizedPath;
        }

        [Test]
        public void TryNormalizePath_WhenPathNavigatesBeyondRoot_ReturnsFalse()
        {
            var result = PathHelper.TryNormalizePath("/../outside", out var normalizedPath);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(normalizedPath, Is.Null);
            }
        }

        [TestCase("path/to/file.txt", "file.txt", ExpectedResult = true)]
        [TestCase("path/to/file.txt", "to/file.txt", ExpectedResult = true)]
        [TestCase("path/to/file.txt", "path/to/file.txt", ExpectedResult = true)]
        [TestCase("path/to/file.txt", "path", ExpectedResult = false)]
        [TestCase("path/to/file.txt", "ile.txt", ExpectedResult = false)]
        [TestCase("path\\to\\file.txt", "file.txt", ExpectedResult = true)]
        [TestCase("path\\to\\file.txt", "to\\file.txt", ExpectedResult = true)]
        [TestCase("path\\to\\file.txt", "path\\to\\file.txt", ExpectedResult = true)]
        [TestCase("", "", ExpectedResult = true)]
        [TestCase("file.txt", "", ExpectedResult = false)]
        [TestCase("", "file.txt", ExpectedResult = false)]
        [TestCase("PATH/TO/FILE.TXT", "file.txt", ExpectedResult = true)]
        [TestCase("some/path/file.txt", "path/file.txt", ExpectedResult = true)]
        [TestCase("file.txt", "file.txt", ExpectedResult = true)]
        [TestCase("another-file.txt", "file.txt", ExpectedResult = false)]
        public bool IsSubpath_ReturnsExpectedResult(string fullPath, string subPath)
        {
            return PathHelper.IsSubpath(fullPath, subPath);
        }

        [TestCase("", ExpectedResult = false)]
        [TestCase(".", ExpectedResult = true)]
        [TestCase("..", ExpectedResult = true)]
        [TestCase("./", ExpectedResult = true)]
        [TestCase("../", ExpectedResult = true)]
        [TestCase(".\\", ExpectedResult = true)]
        [TestCase("..\\", ExpectedResult = true)]
        [TestCase("./file", ExpectedResult = true)]
        [TestCase("../file", ExpectedResult = true)]
        [TestCase(".\\file", ExpectedResult = true)]
        [TestCase("..\\file", ExpectedResult = true)]
        [TestCase("file", ExpectedResult = false)]
        [TestCase("file.txt", ExpectedResult = false)]
        [TestCase(".file", ExpectedResult = false)]
        [TestCase("..file", ExpectedResult = false)]
        [TestCase("...file", ExpectedResult = false)]
        [TestCase("file.", ExpectedResult = false)]
        [TestCase("file..", ExpectedResult = false)]
        [TestCase("file...", ExpectedResult = false)]
        [TestCase("a.", ExpectedResult = false)]
        [TestCase("a..", ExpectedResult = false)]
        [TestCase("a./", ExpectedResult = false)]
        [TestCase("a../", ExpectedResult = false)]
        [TestCase(".a", ExpectedResult = false)]
        [TestCase("..a", ExpectedResult = false)]
        [TestCase(".a/", ExpectedResult = false)]
        [TestCase("..a/", ExpectedResult = false)]
        public bool StartsWithDotSegment_ReturnsExpectedResult(string path)
        {
            return PathHelper.StartsWithDotSegment(path);
        }

        [TestCase("", ExpectedResult = false)]
        [TestCase("regular/path", ExpectedResult = false)]
        [TestCase("path/to/file", ExpectedResult = false)]
        [TestCase(".", ExpectedResult = true)]
        [TestCase("..", ExpectedResult = true)]
        [TestCase("./path", ExpectedResult = true)]
        [TestCase("../path", ExpectedResult = true)]
        [TestCase(".\\path", ExpectedResult = true)]
        [TestCase("..\\path", ExpectedResult = true)]
        [TestCase("path/.", ExpectedResult = true)]
        [TestCase("path/..", ExpectedResult = true)]
        [TestCase("path\\.", ExpectedResult = true)]
        [TestCase("path\\..", ExpectedResult = true)]
        [TestCase("path/./file", ExpectedResult = true)]
        [TestCase("path/../file", ExpectedResult = true)]
        [TestCase("path\\./file", ExpectedResult = true)]
        [TestCase("path\\../file", ExpectedResult = true)]
        [TestCase("path/to/./file", ExpectedResult = true)]
        [TestCase("path/to/../file", ExpectedResult = true)]
        [TestCase("path\\to\\.\\file", ExpectedResult = true)]
        [TestCase("path\\to\\..\\file", ExpectedResult = true)]
        [TestCase(".file", ExpectedResult = false)]
        [TestCase("..file", ExpectedResult = false)]
        [TestCase("...file", ExpectedResult = false)]
        [TestCase("path/.file", ExpectedResult = false)]
        [TestCase("path/..file", ExpectedResult = false)]
        [TestCase("path/...file", ExpectedResult = false)]
        [TestCase("file.", ExpectedResult = false)]
        [TestCase("file..", ExpectedResult = false)]
        [TestCase("file...", ExpectedResult = false)]
        [TestCase("path/file.", ExpectedResult = false)]
        [TestCase("path/file..", ExpectedResult = false)]
        [TestCase("path/file...", ExpectedResult = false)]
        [TestCase("a./b", ExpectedResult = false)]
        [TestCase("a../b", ExpectedResult = false)]
        [TestCase("a/b.", ExpectedResult = false)]
        [TestCase("a/b..", ExpectedResult = false)]
        public bool HasDotSegment_ReturnsExpectedResult(string path)
        {
            return PathHelper.HasDotSegment(path);
        }
    }
}

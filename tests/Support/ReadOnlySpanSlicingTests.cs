// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Support
{
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class ReadOnlySpanSlicingTests
    {
        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "b.c.d")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "b.c.d.")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = "a.b.c.d")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "")]
        public string SliceAfter_ReturnsExpectedResult(string text, char separator)
        {
            return text.AsSpan().SliceAfter(separator).ToString();
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "a")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "a")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "")]
        public string SliceBefore_ReturnsExpectedResult(string text, char separator)
        {
            return text.AsSpan().SliceBefore(separator).ToString();
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "d")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = "d")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "")]
        public string SliceAfterLast_ReturnsExpectedResult(string text, char separator)
        {
            return text.AsSpan().SliceAfterLast(separator).ToString();
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "a.b.c")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "a.b.c.d")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = ".a.b.c")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "")]
        public string SliceBeforeLast_ReturnsExpectedResult(string text, char separator)
        {
            return text.AsSpan().SliceBeforeLast(separator).ToString();
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "b.c.d")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "b.c.d.")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = "a.b.c.d")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "a.b.c.d")]
        public string SliceAfterOrSelf_ReturnsExpectedResult(string text, char separator)
        {
            return text.AsSpan().SliceAfterOrSelf(separator).ToString();
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "a")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "a")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "a.b.c.d")]
        public string SliceBeforeOrSelf_ReturnsExpectedResult(string text, char separator)
        {
            return text.AsSpan().SliceBeforeOrSelf(separator).ToString();
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "d")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = "d")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "a.b.c.d")]
        public string SliceAfterLastOrSelf_ReturnsExpectedResult(string text, char separator)
        {
            return text.AsSpan().SliceAfterLastOrSelf(separator).ToString();
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "a.b.c")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "a.b.c.d")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = ".a.b.c")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "a.b.c.d")]
        public string SliceBeforeLastOrSelf_ReturnsExpectedResult(string text, char separator)
        {
            return text.AsSpan().SliceBeforeLastOrSelf(separator).ToString();
        }
    }
}

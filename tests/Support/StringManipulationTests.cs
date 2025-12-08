// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Support
{
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;

    [TestFixture]
    public class StringManipulationTests
    {
        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "b.c.d")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "b.c.d.")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = "a.b.c.d")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "a.b.c.d")]
        public string SubstringAfterOrSelf_ReturnsExpectedResult(string text, char separator)
        {
            return text.SubstringAfterOrSelf(separator);
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "a")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "a")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "a.b.c.d")]
        public string SubstringBeforeOrSelf_ReturnsExpectedResult(string text, char separator)
        {
            return text.SubstringBeforeOrSelf(separator);
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "d")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = "d")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "a.b.c.d")]
        public string SubstringAfterLastOrSelf_ReturnsExpectedResult(string text, char separator)
        {
            return text.SubstringAfterLastOrSelf(separator);
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "a.b.c")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "a.b.c.d")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = ".a.b.c")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "a.b.c.d")]
        public string SubstringBeforeLastOrSelf_ReturnsExpectedResult(string text, char separator)
        {
            return text.SubstringBeforeLastOrSelf(separator);
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "b.c.d")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "b.c.d.")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = "a.b.c.d")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "")]
        public string SubstringAfter_ReturnsExpectedResult(string text, char separator)
        {
            return text.SubstringAfter(separator);
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "a")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "a")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "")]
        public string SubstringBefore_ReturnsExpectedResult(string text, char separator)
        {
            return text.SubstringBefore(separator);
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "d")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = "d")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "")]
        public string SubstringAfterLast_ReturnsExpectedResult(string text, char separator)
        {
            return text.SubstringAfterLast(separator);
        }

        [TestCase("", '.', ExpectedResult = "")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "a.b.c")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "a.b.c.d")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = ".a.b.c")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "")]
        public string SubstringBeforeLast_ReturnsExpectedResult(string text, char separator)
        {
            return text.SubstringBeforeLast(separator);
        }

        [TestCase("hello world", "eo", '-', false, ExpectedResult = "h-ll- w-rld")]
        [TestCase("hello world", "eo", '-', true, ExpectedResult = "h-ll- w-rld")]
        [TestCase("aaaa", "a", 'b', false, ExpectedResult = "bbbb")]
        [TestCase("aaaa", "a", 'b', true, ExpectedResult = "b")]
        [TestCase("hello", "hel", '-', false, ExpectedResult = "----o")]
        [TestCase("hello", "hel", '-', true, ExpectedResult = "-o")]
        [TestCase("aabbcc", "ab", '-', false, ExpectedResult = "----cc")]
        [TestCase("aabbcc", "ab", '-', true, ExpectedResult = "-cc")]
        [TestCase("a b c", " ", '_', true, ExpectedResult = "a_b_c")]
        public string ReplaceChars_ReturnsExpectedResult(
            string text, string charsToReplace, char replacement, bool skipConsecutiveReplacements)
        {
            return text.ReplaceChars(charsToReplace, replacement, skipConsecutiveReplacements);
        }

        [TestCase("hello world", "eo", ExpectedResult = "hll wrld")]
        [TestCase("aaaa", "a", ExpectedResult = "")]
        [TestCase("hello", "xyz", ExpectedResult = "hello")]
        [TestCase("", "xyz", ExpectedResult = "")]
        [TestCase("hello", "", ExpectedResult = "hello")]
        [TestCase("hello", "h", ExpectedResult = "ello")]
        [TestCase("hello", "o", ExpectedResult = "hell")]
        [TestCase("hello", "hel", ExpectedResult = "o")]
        public string RemoveChars_ReturnsExpectedResult(string text, string charsToRemove)
        {
            return text.RemoveChars(charsToRemove);
        }

        [TestCase("hello", "", "", ExpectedResult = "hello")]
        [TestCase("hallo", "e", "a", ExpectedResult = "hallo")]
        [TestCase("abc", "ab", "xy", ExpectedResult = "xyc")]
        [TestCase("hello", "xyz", "123", ExpectedResult = "hello")]
        [TestCase("", "a", "b", ExpectedResult = "")]
        [TestCase("a", "a", "b", ExpectedResult = "b")]
        [TestCase("aa", "a", "b", ExpectedResult = "bb")]
        [TestCase("hello world", "eo", "12", ExpectedResult = "h1ll2 w2rld")]
        [TestCase("test", "t", "T", ExpectedResult = "TesT")]
        [TestCase("12345", "123", "abc", ExpectedResult = "abc45")]
        public string TranslateChars_ReturnsExpectedResult(string text, string fromChars, string toChars)
        {
            return text.TranslateChars(fromChars, toChars);
        }

        [TestCase("simple_title", ExpectedResult = "Simple Title")]
        [TestCase("simple-title_with_mixed-separators", ExpectedResult = "Simple Title With Mixed Separators")]
        [TestCase("already-Title-Cased", ExpectedResult = "Already Title Cased")]
        [TestCase("multiple--separators___together", ExpectedResult = "Multiple Separators Together")]
        [TestCase("PascalCaseName", ExpectedResult = "Pascal Case Name")]
        [TestCase("camelCaseName", ExpectedResult = "Camel Case Name")]
        [TestCase("MixedCase_with-separators", ExpectedResult = "Mixed Case With Separators")]
        [TestCase("XMLDocument", ExpectedResult = "XML Document")]
        [TestCase("EnhancedIOStream", ExpectedResult = "Enhanced IO Stream")]
        [TestCase("DataProcessorAPI", ExpectedResult = "Data Processor API")]
        public string ToTitleCase_ReturnsExpectedResult(string text)
        {
            return text.ToTitleCase();
        }

        [TestCase("", '.', ExpectedResult = "|")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "a|b.c.d")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "a|b.c.d.")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = "|a.b.c.d")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "a.b.c.d|")]
        [TestCase("hello world", '.', ExpectedResult = "hello world|")]
        [TestCase(".", '.', ExpectedResult = "|")]
        public string SplitFirst_ReturnsExpectedResult(string text, char separator)
        {
            var result = text.SplitFirst(separator);
            return result.Item1 + '|' + result.Item2;
        }

        [TestCase("", '.', ExpectedResult = "|")]
        [TestCase("a.b.c.d", '.', ExpectedResult = "a.b.c|d")]
        [TestCase("a.b.c.d.", '.', ExpectedResult = "a.b.c.d|")]
        [TestCase(".a.b.c.d", '.', ExpectedResult = ".a.b.c|d")]
        [TestCase("a.b.c.d", '-', ExpectedResult = "|a.b.c.d")]
        [TestCase("hello world", '.', ExpectedResult = "|hello world")]
        [TestCase(".", '.', ExpectedResult = "|")]
        public string SplitLast_ReturnsExpectedResult(string text, char separator)
        {
            var result = text.SplitLast(separator);
            return result.Item1 + '|' + result.Item2;
        }
    }
}

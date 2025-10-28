// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See t        [TestCase("long\ttext\twith\ttabs\tthat\tneeds\twrapping", 15, false, ExpectedResult = "long\ttext\twith\ntabs\tthat\tneeds\nwrapping")]e LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Support
{
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class TextUtilityTests
    {
        [TestCase("", ExpectedResult = "")]
        [TestCase("   ", ExpectedResult = "")]
        [TestCase("single line text", ExpectedResult = "single line text")]
        [TestCase("  single line text  ", ExpectedResult = "single line text")]
        [TestCase("line1\nline2\nline3", ExpectedResult = "line1\nline2\nline3")]
        [TestCase("  line1\n  line2\n  line3  ", ExpectedResult = "line1\nline2\nline3")]
        [TestCase("\tline1\n\tline2\n\tline3", ExpectedResult = "line1\nline2\nline3")]
        [TestCase("line1\n\nline2\n\n\nline3", ExpectedResult = "line1\n\nline2\n\nline3")]
        [TestCase("\n\nline1\nline2\nline3\n\n", ExpectedResult = "line1\nline2\nline3")]
        public string NormalizeCodeBlock_ReturnsExpectedText(string text)
        {
            return TextUtility.NormalizeCodeBlock(text).Replace("\r", string.Empty);
        }

        [TestCase("    ", ExpectedResult = "")]
        [TestCase("     single line text     ", ExpectedResult = "single line text")]
        [TestCase("    line1 \n    line2    \n    line3      ", ExpectedResult = "line1\nline2\nline3")]
        [TestCase("        line1 \n    line2     \n        line3    ", ExpectedResult = "    line1\nline2\n    line3")]
        public string NormalizeIndentation_ReturnsExpectedText(string text)
        {
            var lines = text.Split('\n');
            TextUtility.NormalizeIndentation(lines);
            return string.Join('\n', lines);
        }

        [TestCase("   ", false, ExpectedResult = " ")]
        [TestCase("   ", true, ExpectedResult = "")]
        [TestCase("This  is  a  test.", false, ExpectedResult = "This is a test.")]
        [TestCase("This  is  a  test.", true, ExpectedResult = "This is a test.")]
        [TestCase("  This  is  another  test.  ", false, ExpectedResult = " This is another test. ")]
        [TestCase("  This  is  another  test.  ", true, ExpectedResult = "This is another test.")]
        [TestCase("\t This\t is\t a\t test.\n", false, ExpectedResult = " This is a test. ")]
        [TestCase("\t This\t is\t a\t test.\n", true, ExpectedResult = "This is a test.")]
        [TestCase("This\r\nis\n\ra\rtest.", false, ExpectedResult = "This is a test.")]
        [TestCase("This\r\nis\n\ra\rtest.", true, ExpectedResult = "This is a test.")]
        public string NormalizeWhitespace_ReturnExpectedText(string text, bool trim)
        {
            return TextUtility.NormalizeWhitespace(text, trim);
        }

        [TestCase("This is a test.", 10, false, ExpectedResult = "This is a\ntest.")]
        [TestCase("Text with multiple words.", 5, false, ExpectedResult = "Text\nwith\nmulti\nple\nwords\n.")]
        [TestCase("This is a  TEST  with a line longer than the specified width.", 22, false, ExpectedResult = "This is a  TEST  with\na line longer than the\nspecified width.")]
        [TestCase("Another test case for the\nmax line width boundary and preserving\nline breaks.", 15, true, ExpectedResult = "Another test\ncase for the\nmax line width\nboundary and\npreserving\nline breaks.")]
        // Edge cases
        [TestCase("", 10, false, ExpectedResult = "")]
        [TestCase("   ", 10, false, ExpectedResult = "")]
        [TestCase("a", 1, false, ExpectedResult = "a")]
        [TestCase("ab", 1, false, ExpectedResult = "a\nb")]
        [TestCase("abc", 1, false, ExpectedResult = "a\nb\nc")]
        // Single word longer than max width
        [TestCase("supercalifragilisticexpialidocious", 10, false, ExpectedResult = "supercalif\nragilistic\nexpialidoc\nious")]
        [TestCase("verylongwordwithoutspaces", 5, false, ExpectedResult = "veryl\nongwo\nrdwit\nhouts\npaces")]
        // Multiple spaces handling
        [TestCase("word   with   multiple   spaces", 15, false, ExpectedResult = "word   with  \nmultiple  \nspaces")]
        [TestCase("  leading spaces", 10, false, ExpectedResult = "  leading\nspaces")]
        [TestCase("trailing spaces  ", 10, false, ExpectedResult = "trailing\nspaces  ")]
        // Exact width matches
        [TestCase("exactly", 7, false, ExpectedResult = "exactly")]
        [TestCase("exact fit", 9, false, ExpectedResult = "exact fit")]
        [TestCase("exact fit!", 10, false, ExpectedResult = "exact fit!")]
        // Line breaks preservation
        [TestCase("line1\nline2", 20, true, ExpectedResult = "line1\nline2")]
        [TestCase("short\nvery long line that exceeds the width", 10, true, ExpectedResult = "short\nvery long\nline that\nexceeds\nthe width")]
        [TestCase("first\n\n\tsecond", 10, true, ExpectedResult = "first\n\n\tsecond")]
        [TestCase("a\nb\nc", 10, true, ExpectedResult = "a\nb\nc")]
        // Mixed line breaks and wrapping
        [TestCase("This is a long line\nThis is another long line", 10, true, ExpectedResult = "This is a\nlong line\nThis is\nanother\nlong line")]
        [TestCase("Short\nThis is a very long line that needs wrapping", 15, true, ExpectedResult = "Short\nThis is a very\nlong line that\nneeds wrapping")]
        // Special characters and punctuation
        [TestCase("word-with-hyphens-that-is-long", 10, false, ExpectedResult = "word-with-\nhyphens-th\nat-is-long")]
        [TestCase("Hello, world! How are you today?", 12, false, ExpectedResult = "Hello,\nworld! How\nare you\ntoday?")]
        [TestCase("sentence.with.dots.and.periods", 15, false, ExpectedResult = "sentence.with.d\nots.and.periods")]
        // Windows line endings (\r\n)
        [TestCase("line1\r\nline2\r\nline3", 20, true, ExpectedResult = "line1\nline2\nline3")]
        [TestCase("long line with windows endings\r\nshort", 15, true, ExpectedResult = "long line with\nwindows endings\nshort")]
        // Tab characters
        [TestCase("word\twith\ttabs", 10, false, ExpectedResult = "word\twith\ntabs")]
        [TestCase("long\ttext\twith\ttabs\tthat\tneeds\twrapping", 15, false, ExpectedResult = "long\ttext\twith\ntabs\tthat\tneeds\nwrapping")]
        // Numbers and mixed content
        [TestCase("Version 1.2.3 of the software", 12, false, ExpectedResult = "Version\n1.2.3 of the\nsoftware")]
        [TestCase("ID: 12345 Name: John Doe Age: 30", 15, false, ExpectedResult = "ID: 12345 Name:\nJohn Doe Age:\n30")]
        // Large width (no wrapping needed)
        [TestCase("This is a normal sentence.", 100, false, ExpectedResult = "This is a normal sentence.")]
        [TestCase("Multiple words in a sentence.", 50, false, ExpectedResult = "Multiple words in a sentence.")]
        public string SplitLines_ReturnsCorrectLines(string text, int maxLineWidth, bool preserveLineBreaks)
        {
            return string.Join('\n', TextUtility.SplitLines(text, maxLineWidth, preserveLineBreaks));
        }

        [Test]
        public void SplitWords_ReturnsCorrectWords()
        {
            var testCases = new[]
            {
                ("my.property.name", [("my", false), ("property", false), ("name", false)]),
                ("kebab-case", new[] { ("kebab", false), ("case", false) }),
                ("snake_case", [("snake", false), ("case", false)]),
                ("PascalCase", [("Pascal", false), ("Case", false)]),
                ("camelCase", [("camel", false), ("Case", false)]),
                ("IOStream", [("IO", true), ("Stream", false)]),
                ("XMLHttpRequestAPI", [("XML", true), ("Http", false), ("Request", false), ("API", true)]),
                ("A", [("A", false)]),
                ("AB", [("AB", true)]),
                ("ABC", [("ABC", true)]),
                ("AbC", [("Ab", false), ("C", false)]),
                ("word", [("word", false)]),
                ("WORD", [("WORD", true)]),
                ("", []),
                ("   ", []),
                ("123", [("123", false)]),
                ("A1B2C3", [("A1", false), ("B2", false), ("C3", false)])
            };

            foreach (var (input, expected) in testCases)
            {
                var result = TextUtility.SplitWords(input).Select(word => (input[word.Range], word.IsAcronym));
                Assert.That(result, Is.EqualTo(expected), $"Failed for input: '{input}'");
            }
        }
    }
}

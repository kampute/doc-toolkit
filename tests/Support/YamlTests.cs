// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Support
{
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

    [TestFixture]
    public class YamlTests
    {
        #region Empty and Null Cases

        [Test]
        public void Parse_EmptyString_ReturnsEmptyDictionary()
        {
            var result = Yaml.Parse("");

            Assert.That(result, Is.Empty);
            Assert.That(result, Is.SameAs(Yaml.Empty));
        }

        [Test]
        public void Parse_WhitespaceOnly_ReturnsEmptyDictionary()
        {
            var result = Yaml.Parse("   \n   \n   ");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Parse_CommentsOnly_ReturnsEmptyDictionary()
        {
            var yaml = """
                # This is a comment
                  # Another comment
                # Yet another comment
                """;

            var result = Yaml.Parse(yaml);

            Assert.That(result, Is.Empty);
        }

        #endregion

        #region Simple Key-Value Pairs

        [Test]
        public void Parse_SingleStringKeyValue_ReturnsDictionary()
        {
            var yaml = "key: value";

            var result = Yaml.Parse(yaml);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result["key"], Is.EqualTo("value"));
        }

        [Test]
        public void Parse_MultipleStringKeyValues_ReturnsDictionary()
        {
            var yaml = """
                key1: value1
                key2: value2
                key3: value3
                """;

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["key1"], Is.EqualTo("value1"));
                Assert.That(result["key2"], Is.EqualTo("value2"));
                Assert.That(result["key3"], Is.EqualTo("value3"));
            }
        }

        [Test]
        public void Parse_KeyWithEmptyValue_ReturnsNull()
        {
            var yaml = "key:";

            var result = Yaml.Parse(yaml);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result["key"], Is.Null);
        }

        [Test]
        public void Parse_KeyWithWhitespaceValue_ReturnsNull()
        {
            var yaml = "key:    ";

            var result = Yaml.Parse(yaml);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result["key"], Is.Null);
        }

        #endregion

        #region Null and Boolean Values

        [TestCase("key: null")]
        [TestCase("key: Null")]
        [TestCase("key: NULL")]
        [TestCase("key: ~")]
        public void Parse_NullValues_ReturnsNull(string yaml)
        {
            var result = Yaml.Parse(yaml);
            Assert.That(result["key"], Is.Null);
        }

        [TestCase("key: true", ExpectedResult = true)]
        [TestCase("key: True", ExpectedResult = true)]
        [TestCase("key: TRUE", ExpectedResult = true)]
        [TestCase("key: false", ExpectedResult = false)]
        [TestCase("key: False", ExpectedResult = false)]
        [TestCase("key: FALSE", ExpectedResult = false)]
        public object? Parse_BooleanValues_ReturnsBoolean(string yaml)
        {
            var result = Yaml.Parse(yaml);
            return result["key"];
        }

        #endregion

        #region Numeric Values

        [TestCase("key: 0", ExpectedResult = 0L)]
        [TestCase("key: 42", ExpectedResult = 42L)]
        [TestCase("key: -42", ExpectedResult = -42L)]
        [TestCase("key: 9223372036854775807", ExpectedResult = 9223372036854775807L)]
        [TestCase("key: -9223372036854775808", ExpectedResult = -9223372036854775808L)]
        public object? Parse_IntegerValues_ReturnsLong(string yaml)
        {
            var result = Yaml.Parse(yaml);
            return result["key"];
        }

        [TestCase("key: 0.0", ExpectedResult = 0.0)]
        [TestCase("key: 3.14", ExpectedResult = 3.14)]
        [TestCase("key: -3.14", ExpectedResult = -3.14)]
        [TestCase("key: 1.23e10", ExpectedResult = 1.23e10)]
        [TestCase("key: -1.23e-10", ExpectedResult = -1.23e-10)]
        public object? Parse_FloatValues_ReturnsDouble(string yaml)
        {
            var result = Yaml.Parse(yaml);
            return result["key"];
        }

        #endregion

        #region Quoted Strings

        [Test]
        public void Parse_DoubleQuotedString_ReturnsUnquotedString()
        {
            var yaml = @"key: ""value with spaces""";

            var result = Yaml.Parse(yaml);

            Assert.That(result["key"], Is.EqualTo("value with spaces"));
        }

        [Test]
        public void Parse_SingleQuotedString_ReturnsUnquotedString()
        {
            var yaml = "key: 'value with spaces'";

            var result = Yaml.Parse(yaml);

            Assert.That(result["key"], Is.EqualTo("value with spaces"));
        }

        [Test]
        public void Parse_DoubleQuotedStringWithEscapes_ReturnsUnescapedString()
        {
            var yaml = @"key: ""line1\nline2\ttab\\backslash\""quote""";

            var result = Yaml.Parse(yaml);

            Assert.That(result["key"], Is.EqualTo("line1\nline2\ttab\\backslash\"quote"));
        }

        [TestCase(@"key: ""\0""", ExpectedResult = "\0")]
        [TestCase(@"key: ""\a""", ExpectedResult = "\a")]
        [TestCase(@"key: ""\b""", ExpectedResult = "\b")]
        [TestCase(@"key: ""\f""", ExpectedResult = "\f")]
        [TestCase(@"key: ""\n""", ExpectedResult = "\n")]
        [TestCase(@"key: ""\r""", ExpectedResult = "\r")]
        [TestCase(@"key: ""\t""", ExpectedResult = "\t")]
        [TestCase(@"key: ""\v""", ExpectedResult = "\v")]
        [TestCase(@"key: ""\\""", ExpectedResult = "\\")]
        [TestCase(@"key: ""\""""", ExpectedResult = "\"")]
        public object? Parse_DoubleQuotedEscapeSequences_ReturnsCorrectCharacter(string yaml)
        {
            var result = Yaml.Parse(yaml);
            return result["key"];
        }

        [Test]
        public void Parse_SingleQuotedStringWithDoubledQuote_ReturnsQuote()
        {
            var yaml = "key: 'can''t'";

            var result = Yaml.Parse(yaml);

            Assert.That(result["key"], Is.EqualTo("can't"));
        }

        [Test]
        public void Parse_QuotedKeyValue_ParsesCorrectly()
        {
            var yaml = "\"quoted key\": \"quoted value\"";

            var result = Yaml.Parse(yaml);

            Assert.That(result["quoted key"], Is.EqualTo("quoted value"));
        }

        [Test]
        public void Parse_UnterminatedDoubleQuotedString_ThrowsFormatException()
        {
            var yaml = "key: \"unterminated";

            Assert.That(() => Yaml.Parse(yaml), Throws.TypeOf<FormatException>()
                .With.Message.Contains("not terminated"));
        }

        [Test]
        public void Parse_UnterminatedSingleQuotedString_ThrowsFormatException()
        {
            var yaml = "key: 'unterminated";

            Assert.That(() => Yaml.Parse(yaml), Throws.TypeOf<FormatException>()
                .With.Message.Contains("not terminated"));
        }

        [Test]
        public void Parse_InvalidEscapeSequence_ThrowsFormatException()
        {
            var yaml = @"key: ""\x""";

            Assert.That(() => Yaml.Parse(yaml), Throws.TypeOf<FormatException>()
                .With.Message.Contains("Unsupported escape sequence"));
        }

        [Test]
        public void Parse_SingleQuoteNotDoubled_ThrowsFormatException()
        {
            var yaml = "key: 'it's'";

            Assert.That(() => Yaml.Parse(yaml), Throws.TypeOf<FormatException>()
                .With.Message.Contains("escaped by doubling"));
        }

        #endregion

        #region Comments

        [Test]
        public void Parse_InlineComment_IgnoresComment()
        {
            var yaml = "key: value # this is a comment";

            var result = Yaml.Parse(yaml);

            Assert.That(result["key"], Is.EqualTo("value"));
        }

        [TestCase("title: It's here", ExpectedResult = "It's here")]
        [TestCase("title: It's a test # comment", ExpectedResult = "It's a test")]
        [TestCase("title: say \"hi", ExpectedResult = "say \"hi")]
        public object? Parse_PlainScalarContainingQuote_ReturnsString(string yaml)
        {
            return Yaml.Parse(yaml)["title"];
        }

        [Test]
        public void Parse_HashInQuotedString_PreservesHash()
        {
            var yaml = @"key: ""value # not a comment""";

            var result = Yaml.Parse(yaml);

            Assert.That(result["key"], Is.EqualTo("value # not a comment"));
        }

        [Test]
        public void Parse_HashWithoutPrecedingSpace_NotTreatedAsComment()
        {
            var yaml = "key: value#not-a-comment";

            var result = Yaml.Parse(yaml);

            Assert.That(result["key"], Is.EqualTo("value#not-a-comment"));
        }

        [Test]
        public void Parse_MultipleCommentsAndData_ParsesDataCorrectly()
        {
            var yaml = """
                # Comment before
                key1: value1 # inline comment
                # Comment between
                key2: value2
                # Comment after
                """;

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["key1"], Is.EqualTo("value1"));
                Assert.That(result["key2"], Is.EqualTo("value2"));
            }
        }

        #endregion

        #region Nested Mappings

        [Test]
        public void Parse_NestedMapping_ReturnsNestedDictionary()
        {
            var yaml = """
                parent:
                  child1: value1
                  child2: value2
                """;

            var result = Yaml.Parse(yaml);

            var nested = result["parent"] as IReadOnlyDictionary<string, object?>;
            Assert.That(nested, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(nested!["child1"], Is.EqualTo("value1"));
                Assert.That(nested["child2"], Is.EqualTo("value2"));
            }
        }

        [Test]
        public void Parse_DeeplyNestedMapping_ReturnsNestedDictionaries()
        {
            var yaml = """
                level1:
                  level2:
                    level3:
                      key: value
                """;

            var result = Yaml.Parse(yaml);

            var level1 = result["level1"] as IReadOnlyDictionary<string, object?>;
            Assert.That(level1, Is.Not.Null);

            var level2 = level1!["level2"] as IReadOnlyDictionary<string, object?>;
            Assert.That(level2, Is.Not.Null);

            var level3 = level2!["level3"] as IReadOnlyDictionary<string, object?>;
            Assert.That(level3, Is.Not.Null);

            Assert.That(level3!["key"], Is.EqualTo("value"));
        }

        [Test]
        public void Parse_MixedLevelMappings_ParsesCorrectly()
        {
            var yaml = """
                root1: value1
                root2:
                  child1: value2
                  child2: value3
                root3: value4
                """;

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["root1"], Is.EqualTo("value1"));
                Assert.That(result["root3"], Is.EqualTo("value4"));
            }

            var nested = result["root2"] as IReadOnlyDictionary<string, object?>;
            Assert.That(nested, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(nested!["child1"], Is.EqualTo("value2"));
                Assert.That(nested["child2"], Is.EqualTo("value3"));
            }
        }

        #endregion

        #region Sequences

        [Test]
        public void Parse_SimpleSequence_ReturnsList()
        {
            var yaml = """
                items:
                  - item1
                  - item2
                  - item3
                """;

            var result = Yaml.Parse(yaml);

            var list = result["items"] as IList<object?>;
            Assert.That(list, Is.EqualTo(["item1", "item2", "item3"]));
        }

        [Test]
        public void Parse_SequenceWithMixedTypes_ReturnsList()
        {
            var yaml = """
                items:
                  - string value
                  - 42
                  - 3.14
                  - true
                  - null
                """;

            var result = Yaml.Parse(yaml);

            var list = result["items"] as IList<object?>;
            Assert.That(list, Is.EqualTo(new object?[] { "string value", 42L, 3.14, true, null }));
        }

        [Test]
        public void Parse_SequenceWithNestedMappings_ReturnsList()
        {
            var yaml = """
                items:
                  - name: item1
                    value: 1
                  - name: item2
                    value: 2
                """;

            var result = Yaml.Parse(yaml);

            var list = result["items"] as IList<object?>;
            Assert.That(list, Is.Not.Null);

            var item1 = list![0] as IReadOnlyDictionary<string, object?>;
            Assert.That(item1, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(item1!["name"], Is.EqualTo("item1"));
                Assert.That(item1["value"], Is.EqualTo(1L));
            }

            var item2 = list[1] as IReadOnlyDictionary<string, object?>;
            Assert.That(item2, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(item2!["name"], Is.EqualTo("item2"));
                Assert.That(item2["value"], Is.EqualTo(2L));
            }
        }

        [Test]
        public void Parse_SequenceWithEmptyItems_ReturnsListWithNulls()
        {
            var yaml = """
                items:
                  -
                  -
                  - value
                """;

            var result = Yaml.Parse(yaml);

            var list = result["items"] as IList<object?>;
            Assert.That(list, Is.EqualTo(new object?[] { null, null, "value" }));
        }

        [Test]
        public void Parse_SequenceItemWithMapping_ParsesCorrectly()
        {
            var yaml = """
                items:
                  - key1: value1
                  -
                    key2: value2
                    key3: value3
                """;

            var result = Yaml.Parse(yaml);

            var list = result["items"] as IList<object?>;
            Assert.That(list, Is.Not.Null);

            var item1 = list![0] as IReadOnlyDictionary<string, object?>;
            Assert.That(item1, Is.Not.Null);
            Assert.That(item1!["key1"], Is.EqualTo("value1"));

            var item2 = list[1] as IReadOnlyDictionary<string, object?>;
            Assert.That(item2, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(item2!["key2"], Is.EqualTo("value2"));
                Assert.That(item2["key3"], Is.EqualTo("value3"));
            }
        }

        [Test]
        public void Parse_SequenceIndentedLikeItsKey_ReturnsList()
        {
            var yaml = "tags:\n- one\n- two\nnext: value";

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["tags"], Is.EqualTo(new[] { "one", "two" }));
                Assert.That(result["next"], Is.EqualTo("value"));
            }
        }

        [Test]
        public void Parse_SequenceItemMappingWithEmptyValueFollowedBySiblingKey_ReturnsSiblingEntries()
        {
            var yaml = "items:\n  - first:\n    second: value";

            var result = Yaml.Parse(yaml);

            var item = (result["items"] as IList<object?>)?[0] as IReadOnlyDictionary<string, object?>;
            Assert.That(item, Is.EquivalentTo(new Dictionary<string, object?> { ["first"] = null, ["second"] = "value" }));
        }

        #endregion

        #region Block Scalars

        [Test]
        public void Parse_LiteralBlockScalar_PreservesNewlines()
        {
            var yaml = """
                description: |
                  Line 1
                  Line 2
                  Line 3
                """;

            var result = Yaml.Parse(yaml);

            Assert.That(result["description"], Is.EqualTo("Line 1\nLine 2\nLine 3"));
        }

        [Test]
        public void Parse_FoldedBlockScalar_FoldsNewlines()
        {
            var yaml = """
                description: >
                  Line 1
                  Line 2
                  Line 3
                """;

            var result = Yaml.Parse(yaml);

            Assert.That(result["description"], Is.EqualTo("Line 1 Line 2 Line 3"));
        }

        [Test]
        public void Parse_EmptyLiteralBlockScalar_ReturnsEmptyString()
        {
            var yaml = """
                description: |
                next: value
                """;

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["description"], Is.EqualTo(""));
                Assert.That(result["next"], Is.EqualTo("value"));
            }
        }

        [Test]
        public void Parse_EmptyFoldedBlockScalar_ReturnsEmptyString()
        {
            var yaml = """
                description: >
                next: value
                """;

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["description"], Is.EqualTo(""));
                Assert.That(result["next"], Is.EqualTo("value"));
            }
        }

        [Test]
        public void Parse_BlockScalarWithTrailingWhitespace_TrimsEnd()
        {
            var yaml = """
                description: |
                  Line 1
                  Line 2  


                """;

            var result = Yaml.Parse(yaml);

            Assert.That(result["description"], Is.EqualTo("Line 1\nLine 2"));
        }

        [Test]
        public void Parse_BlockScalarInSequence_ParsesCorrectly()
        {
            var yaml = """
                items:
                  - |
                    Block 1
                    Line 2
                  - normal value
                  - >
                    Folded
                    text
                """;

            var result = Yaml.Parse(yaml);

            var list = result["items"] as IList<object?>;
            Assert.That(list, Is.EqualTo(["Block 1\nLine 2", "normal value", "Folded text"]));
        }

        [Test]
        public void Parse_BlockScalarInNestedMapping_ParsesCorrectly()
        {
            var yaml = """
                parent:
                  description: |
                    Multi
                    Line
                  other: value
                """;

            var result = Yaml.Parse(yaml);

            var nested = result["parent"] as IReadOnlyDictionary<string, object?>;
            Assert.That(nested, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(nested!["description"], Is.EqualTo("Multi\nLine"));
                Assert.That(nested["other"], Is.EqualTo("value"));
            }
        }

        [Test]
        public void Parse_LiteralBlockScalar_PreservesContentAsIs()
        {
            var yaml = "code: |\n  if (it's)\n\n  # not a comment\n    indented\nnext: value";

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["code"], Is.EqualTo("if (it's)\n\n# not a comment\n  indented"));
                Assert.That(result["next"], Is.EqualTo("value"));
            }
        }

        [Test]
        public void Parse_FoldedBlockScalar_KeepsEmptyLineAsLineBreak()
        {
            var yaml = "text: >\n  Paragraph 1\n  continues\n\n  Paragraph 2\nnext: value";

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["text"], Is.EqualTo("Paragraph 1 continues\nParagraph 2"));
                Assert.That(result["next"], Is.EqualTo("value"));
            }
        }

        [Test]
        public void Parse_EmptyBlockScalarInSequenceItemMapping_DoesNotConsumeSiblingKey()
        {
            var yaml = "items:\n  - text: |\n    other: value";

            var result = Yaml.Parse(yaml);

            var item = (result["items"] as IList<object?>)?[0] as IReadOnlyDictionary<string, object?>;
            Assert.That(item, Is.EquivalentTo(new Dictionary<string, object?> { ["text"] = "", ["other"] = "value" }));
        }

        #endregion

        #region Indentation and Structure

        [Test]
        public void Parse_IncorrectIndentation_ThrowsFormatException()
        {
            var yaml = """
                key1: value1
                  key2: value2
                """;

            Assert.That(() => Yaml.Parse(yaml), Throws.TypeOf<FormatException>()
                .With.Message.Contains("Unexpected indentation"));
        }

        [Test]
        public void Parse_SequenceAtRootLevel_ThrowsFormatException()
        {
            var yaml = """
                - item1
                - item2
                """;

            Assert.That(() => Yaml.Parse(yaml), Throws.TypeOf<FormatException>()
                .With.Message.Contains("mapping key was expected"));
        }

        [Test]
        public void Parse_MissingColon_ThrowsFormatException()
        {
            var yaml = "key value";

            Assert.That(() => Yaml.Parse(yaml), Throws.TypeOf<FormatException>()
                .With.Message.Contains("must contain ':'"));
        }

        [Test]
        public void Parse_DuplicateKey_ThrowsFormatException()
        {
            var yaml = """
                key: value1
                key: value2
                """;

            Assert.That(() => Yaml.Parse(yaml), Throws.TypeOf<FormatException>()
                .With.Message.Contains("Duplicate key"));
        }

        [Test]
        public void Parse_DuplicateKeyInNested_ThrowsFormatException()
        {
            var yaml = """
                parent:
                  key: value1
                  key: value2
                """;

            Assert.That(() => Yaml.Parse(yaml), Throws.TypeOf<FormatException>()
                .With.Message.Contains("Duplicate key"));
        }

        [Test]
        public void Parse_DuplicateKeyInSequenceItem_ThrowsFormatException()
        {
            var yaml = """
                items:
                  - name: item1
                    name: duplicate
                """;

            Assert.That(() => Yaml.Parse(yaml), Throws.TypeOf<FormatException>()
                .With.Message.Contains("Duplicate key"));
        }

        #endregion

        #region Complex Scenarios

        [Test]
        public void Parse_ComplexDocument_ParsesCorrectly()
        {
            var yaml = """
                # Configuration file
                version: 1.0
                enabled: true
                count: 42

                database:
                  host: localhost
                  port: 5432
                  credentials:
                    username: admin
                    password: "secret!@#"

                servers:
                  - name: server1
                    ip: 192.168.1.1
                    active: true
                  - name: server2
                    ip: 192.168.1.2
                    active: false

                description: |
                  This is a multi-line
                  description that preserves
                  line breaks.

                summary: >
                  This is a folded
                  multi-line text
                  that joins lines.

                tags:
                  - production
                  - critical
                  - monitored

                metadata: null
                """;

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["version"], Is.EqualTo(1.0));
                Assert.That(result["enabled"], Is.True);
                Assert.That(result["count"], Is.EqualTo(42L));
                Assert.That(result["metadata"], Is.Null);
            }

            var database = result["database"] as IReadOnlyDictionary<string, object?>;
            Assert.That(database, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(database!["host"], Is.EqualTo("localhost"));
                Assert.That(database["port"], Is.EqualTo(5432L));
            }

            var credentials = database!["credentials"] as IReadOnlyDictionary<string, object?>;
            Assert.That(credentials, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(credentials!["username"], Is.EqualTo("admin"));
                Assert.That(credentials["password"], Is.EqualTo("secret!@#"));
            }

            var servers = result["servers"] as IList<object?>;
            Assert.That(servers, Is.Not.Null);

            var server1 = servers![0] as IReadOnlyDictionary<string, object?>;
            Assert.That(server1, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(server1!["name"], Is.EqualTo("server1"));
                Assert.That(server1["ip"], Is.EqualTo("192.168.1.1"));
                Assert.That(server1["active"], Is.True);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["description"], Is.EqualTo("This is a multi-line\ndescription that preserves\nline breaks."));
                Assert.That(result["summary"], Is.EqualTo("This is a folded multi-line text that joins lines."));
            }

            var tags = result["tags"] as IList<object?>;
            Assert.That(tags, Is.EqualTo(["production", "critical", "monitored"]));
        }

        [Test]
        public void Parse_ColonInValue_ParsesCorrectly()
        {
            var yaml = """
                url: http://example.com:8080
                time: 12:30:45
                quoted: "key: value"
                """;

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["url"], Is.EqualTo("http://example.com:8080"));
                Assert.That(result["time"], Is.EqualTo("12:30:45"));
                Assert.That(result["quoted"], Is.EqualTo("key: value"));
            }
        }

        [Test]
        public void Parse_WindowsAndUnixLineEndings_ParsesCorrectly()
        {
            var yaml = "key1: value1\r\nkey2: value2\nkey3: value3\r\n";

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["key1"], Is.EqualTo("value1"));
                Assert.That(result["key2"], Is.EqualTo("value2"));
                Assert.That(result["key3"], Is.EqualTo("value3"));
            }
        }

        [Test]
        public void Parse_TrailingWhitespace_HandlesCorrectly()
        {
            var yaml = """
                key1: value1   
                key2: value2	
                key3: value3
                """;

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["key1"], Is.EqualTo("value1"));
                Assert.That(result["key2"], Is.EqualTo("value2"));
                Assert.That(result["key3"], Is.EqualTo("value3"));
            }
        }

        [Test]
        public void Parse_EmptyMapping_ReturnsNull()
        {
            var yaml = """
                parent:
                next: value
                """;

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["parent"], Is.Null);
                Assert.That(result["next"], Is.EqualTo("value"));
            }
        }

        [Test]
        public void Parse_EmptySequenceItem_ReturnsNull()
        {
            var yaml = """
                items:
                  -
                """;

            var result = Yaml.Parse(yaml);

            var list = result["items"] as IList<object?>;
            Assert.That(list, Is.EqualTo(new object?[] { null }));
        }

        #endregion

        #region Edge Cases with Special Characters

        [Test]
        public void Parse_DashInValue_NotTreatedAsSequence()
        {
            var yaml = "key: value-with-dashes";

            var result = Yaml.Parse(yaml);

            Assert.That(result["key"], Is.EqualTo("value-with-dashes"));
        }

        [Test]
        public void Parse_ColonInQuotedKey_ParsesCorrectly()
        {
            var yaml = "\"key:with:colons\": value";

            var result = Yaml.Parse(yaml);

            Assert.That(result["key:with:colons"], Is.EqualTo("value"));
        }

        [Test]
        public void Parse_QuotedBooleanString_ReturnsString()
        {
            var yaml = """
                key1: "true"
                key2: "false"
                key3: "null"
                """;

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["key1"], Is.EqualTo("true"));
                Assert.That(result["key2"], Is.EqualTo("false"));
                Assert.That(result["key3"], Is.EqualTo("null"));
            }
        }

        [Test]
        public void Parse_QuotedNumericString_ReturnsString()
        {
            var yaml = """
                key1: "42"
                key2: "3.14"
                """;

            var result = Yaml.Parse(yaml);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result["key1"], Is.EqualTo("42"));
                Assert.That(result["key2"], Is.EqualTo("3.14"));
            }
        }

        [Test]
        public void Parse_LeadingZeros_ParsesAsInteger()
        {
            var yaml = "key: 007";

            var result = Yaml.Parse(yaml);

            Assert.That(result["key"], Is.EqualTo(7L));
        }

        #endregion

        #region Flow Sequences and Unsupported Constructs

        [Test]
        public void Parse_FlowSequence_ReturnsList()
        {
            var result = Yaml.Parse("tags: [alpha, beta]");

            Assert.That(result["tags"], Is.EqualTo(new object?[] { "alpha", "beta" }));
        }

        [Test]
        public void Parse_FlowSequenceWithMixedScalars_ReturnsTypedItems()
        {
            var result = Yaml.Parse("items: [one, 2, \"x, y\", 'it''s', true, null]");

            Assert.That(result["items"], Is.EqualTo(new object?[] { "one", 2L, "x, y", "it's", true, null }));
        }

        [TestCase("items: []", ExpectedResult = new object[0])]
        [TestCase("items: [ a , b, ]", ExpectedResult = new object[] { "a", "b" })]
        public object? Parse_FlowSequenceEdgeCases_ReturnsExpectedList(string yaml)
        {
            return Yaml.Parse(yaml)["items"];
        }

        [Test]
        public void Parse_FlowSequenceAsSequenceItem_ReturnsNestedList()
        {
            var yaml = """
                items:
                  - [a, b]
                  - c
                """;

            var result = Yaml.Parse(yaml);

            Assert.That(result["items"], Is.EqualTo(new object?[] { new object?[] { "a", "b" }, "c" }));
        }

        [Test]
        public void Parse_FlowMapping_ReturnsDictionary()
        {
            var result = Yaml.Parse("key: {a: 1, \"b\": \"x, y\", c}");

            Assert.That(result["key"], Is.EqualTo(new Dictionary<string, object?> { ["a"] = 1L, ["b"] = "x, y", ["c"] = null }));
        }

        [Test]
        public void Parse_EmptyFlowMapping_ReturnsEmptyDictionary()
        {
            var result = Yaml.Parse("key: {}");

            Assert.That(result["key"], Is.EqualTo(new Dictionary<string, object?>()));
        }

        [Test]
        public void Parse_NestedFlowCollections_ReturnsNestedValues()
        {
            var result = Yaml.Parse("key: {list: [1, [a, b]], map: {url: http://example.com}}");

            var expected = new Dictionary<string, object?>
            {
                ["list"] = new object?[] { 1L, new object?[] { "a", "b" } },
                ["map"] = new Dictionary<string, object?> { ["url"] = "http://example.com" }
            };
            Assert.That(result["key"], Is.EqualTo(expected));
        }

        [Test]
        public void Parse_FlowMappingAsSequenceItem_ReturnsDictionary()
        {
            var yaml = """
                items:
                  - {name: x, value: 2}
                """;

            var result = Yaml.Parse(yaml);

            Assert.That(result["items"], Is.EqualTo(new object?[] { new Dictionary<string, object?> { ["name"] = "x", ["value"] = 2L } }));
        }

        [Test]
        public void Parse_QuotedFlowItemContainingHash_KeepsHash()
        {
            var result = Yaml.Parse("items: [\"a #b\", c] # comment");

            Assert.That(result["items"], Is.EqualTo(new object?[] { "a #b", "c" }));
        }

        // Unsupported constructs and malformed flow collections are rejected rather than read as plain strings
        [TestCase("key: [a, b", TestName = "Parse_UnterminatedFlowSequence_ThrowsFormatException")]
        [TestCase("key: {a: 1", TestName = "Parse_UnterminatedFlowMapping_ThrowsFormatException")]
        [TestCase("key: [a, , b]", TestName = "Parse_FlowSequenceWithEmptyEntry_ThrowsFormatException")]
        [TestCase("key: {a: 1, , b: 2}", TestName = "Parse_FlowMappingWithEmptyEntry_ThrowsFormatException")]
        [TestCase("key: {a: 1, a: 2}", TestName = "Parse_FlowMappingWithDuplicateKey_ThrowsFormatException")]
        [TestCase("key: {a: \"x\" b: 2}", TestName = "Parse_FlowMappingWithoutSeparator_ThrowsFormatException")]
        [TestCase("key: [\"a\" b]", TestName = "Parse_FlowSequenceWithoutSeparator_ThrowsFormatException")]
        [TestCase("key: {a", TestName = "Parse_FlowMappingEndingAfterKey_ThrowsFormatException")]
        [TestCase("key: [a: b]", TestName = "Parse_SinglePairMappingInFlowSequence_ThrowsFormatException")]
        [TestCase("key: [a] b", TestName = "Parse_ContentAfterFlowCollection_ThrowsFormatException")]
        [TestCase("key: &anchor value", TestName = "Parse_Anchor_ThrowsFormatException")]
        [TestCase("&anchor key: value", TestName = "Parse_AnchorOnKey_ThrowsFormatException")]
        [TestCase("key: *alias", TestName = "Parse_Alias_ThrowsFormatException")]
        [TestCase("items:\n  - *alias", TestName = "Parse_AliasAsSequenceItem_ThrowsFormatException")]
        [TestCase("key: !!str value", TestName = "Parse_Tag_ThrowsFormatException")]
        [TestCase("!tag key: value", TestName = "Parse_TagOnKey_ThrowsFormatException")]
        public void Parse_UnsupportedConstruct_ThrowsFormatException(string yaml)
        {
            Assert.That(() => Yaml.Parse(yaml), Throws.TypeOf<FormatException>());
        }

        #endregion
    }
}

// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Collections
{
    using Kampute.DocToolkit.Collections;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

    [TestFixture]
    public class PatternCollectionTests
    {
        [Test]
        public void Constructor_WithWildcardSeparator_ThrowsArgumentException()
        {
            Assert.That(() => new PatternCollection(PatternCollection.Wildcard), Throws.ArgumentException.With.Property("ParamName").EqualTo("separator"));
        }

        [Test]
        public void Count_ReturnsCorrectCount()
        {
            var collection = new PatternCollection('.')
            {
                "System.Text",
                "System.*",
                "*"
            };

            Assert.That(collection, Has.Count.EqualTo(3));
        }

        [Test]
        public void Add_Exact_AddsToCollection()
        {
            var collection = new PatternCollection('.')
            {
                "System.Text"
            };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection, Does.Contain("System.Text"));
                Assert.That(collection, Has.Count.EqualTo(1));
            }
        }

        [Test]
        public void Add_Wildcard_AddsToCollection()
        {
            var collection = new PatternCollection('.')
            {
                "System.*"
            };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection, Does.Contain("System.*"));
                Assert.That(collection, Has.Count.EqualTo(1));
            }
        }

        [Test]
        public void Add_InvalidWildcard_ThrowsArgumentException()
        {
            var collection = new PatternCollection('.');

            Assert.Throws<ArgumentException>(() => collection.Add("System*"));
            Assert.Throws<ArgumentException>(() => collection.Add("System.**"));
            Assert.Throws<ArgumentException>(() => collection.Add("System.*.Text"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void Add_WithNullOrEmptyPattern_ThrowsArgumentException(string? pattern)
        {
            var collection = new PatternCollection('.');

            Assert.That(() => collection.Add(pattern!), Throws.ArgumentException.With.Property("ParamName").EqualTo("pattern"));
        }

        [Test]
        public void Remove_WithNullPattern_ThrowsArgumentNullException()
        {
            var collection = new PatternCollection('.');

            Assert.That(() => collection.Remove(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("pattern"));
        }

        [TestCase("System/*")]
        [TestCase("Systemx*")]
        public void ContainsAndRemove_WithWildcardNotPrecededBySeparator_DoNotMatchWildcardPattern(string pattern)
        {
            var collection = new PatternCollection('.') { "System.*" };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection.Contains(pattern), Is.False);
                Assert.That(collection.Remove(pattern), Is.False);
                Assert.That(collection, Is.EqualTo(["System.*"]));
            }
        }

        [TestCase("System.Text")]
        [TestCase("System.*")]
        [TestCase("*")]
        public void Add_WithDuplicatePattern_ReturnsFalseAndKeepsSinglePattern(string pattern)
        {
            var collection = new PatternCollection('.');

            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection.Add(pattern), Is.True);
                Assert.That(collection.Add(pattern), Is.False);
                Assert.That(collection, Is.EqualTo([pattern]));
            }
        }

        [Test]
        public void Remove_UniversalPattern_PreservesSpecificPatterns()
        {
            var collection = new PatternCollection('.') { "*", "System.*", "Exact" };

            Assert.That(collection.Remove("*"), Is.True);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection.MatchesAll, Is.False);
                Assert.That(collection.Remove("*"), Is.False);
                Assert.That(collection, Is.EquivalentTo(["System.*", "Exact"]));
                Assert.That(collection.Matches("System.Text"), Is.True);
                Assert.That(collection.Matches("Exact"), Is.True);
                Assert.That(collection.Matches("Other"), Is.False);
            }
        }

        [TestCase(null)]
        [TestCase("")]
        public void Matches_WithUniversalPattern_RejectsNullAndEmptyValues(string? value)
        {
            var collection = new PatternCollection('.') { "*" };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection.Matches(value!), Is.False);
                Assert.That(collection.TryGetMatchingPattern(value!, out var pattern), Is.False);
                Assert.That(pattern, Is.Null);
            }
        }

        [TestCase("Systematic.Text")]
        [TestCase("system.Text")]
        [TestCase("Sys")]
        [TestCase("Other")]
        public void Matches_WithWildcard_RejectsNonMatchingValues(string value)
        {
            var collection = new PatternCollection('.') { "System.*" };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection.Matches(value), Is.False);
                Assert.That(collection.TryGetMatchingPattern(value, out var pattern), Is.False);
                Assert.That(pattern, Is.Null);
            }
        }

        [Test]
        public void TryGetMatchingPattern_AfterRemovingMostSpecificPattern_FallsBackToRemainingPatterns()
        {
            var collection = new PatternCollection('.') { "*", "System.*", "System.Text.*", "System.Text.Json" };

            collection.Remove("System.Text.Json");
            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection.TryGetMatchingPattern("System.Text.Json", out var pattern), Is.True);
                Assert.That(pattern, Is.EqualTo("System.Text.*"));
            }

            collection.Remove("System.Text.*");
            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection.TryGetMatchingPattern("System.Text.Json", out var pattern), Is.True);
                Assert.That(pattern, Is.EqualTo("System.*"));
            }

            collection.Remove("System.*");
            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection.TryGetMatchingPattern("System.Text.Json", out var pattern), Is.True);
                Assert.That(pattern, Is.EqualTo("*"));
            }
        }

        [Test]
        public void Remove_Exact_RemovesFromCollection()
        {
            var collection = new PatternCollection('.')
            {
                "System.Text"
            };

            var result = collection.Remove("System.Text");

            Assert.That(result, Is.True);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection, Does.Not.Contain("System.Text"));
                Assert.That(collection, Is.Empty);
            }
        }

        [Test]
        public void Remove_Wildcard_RemovesFromCollection()
        {
            var collection = new PatternCollection('.')
            {
                "System.*"
            };

            var result = collection.Remove("System.*");

            Assert.That(result, Is.True);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection, Does.Not.Contain("System.*"));
                Assert.That(collection, Is.Empty);
            }
        }

        [Test]
        public void Contains_WithNonExisting_Exact_ReturnsFalse()
        {
            var collection = new PatternCollection('.')
            {
                "System.Text"
            };

            Assert.That(collection, Does.Not.Contain("System.Text.Json"));
        }

        [Test]
        public void Contains_WithNonExisting_Wildcard_ReturnsFalse()
        {
            var collection = new PatternCollection('.')
            {
                "System.*"
            };

            Assert.That(collection, Does.Not.Contain("System.Text.*"));
        }

        [TestCase("System.Text", ExpectedResult = true)]
        [TestCase("System", ExpectedResult = false)]
        public bool Matches_Exact_ReturnsCorrectResult(string ns)
        {
            var collection = new PatternCollection('.')
            {
                "System.Text"
            };

            return collection.Matches(ns);
        }

        [TestCase("System", ExpectedResult = true)]
        [TestCase("System.Text", ExpectedResult = true)]
        [TestCase("System.Text.Json", ExpectedResult = true)]
        [TestCase("Microsoft", ExpectedResult = false)]
        public bool Matches_Wildcard_ReturnsCorrectResult(string ns)
        {
            var collection = new PatternCollection('.')
            {
                "System.*"
            };

            return collection.Matches(ns);
        }

        [Test]
        public void Matches_WithValueShorterThanLongerWildcard_MatchesShorterWildcard()
        {
            var collection = new PatternCollection('.')
            {
                "System.Collections.Generic.*",
                "System.*"
            };

            Assert.That(collection.Matches("System.Text"), Is.True);
        }

        [TestCase("System", ExpectedResult = "System.*")]
        [TestCase("System.Text", ExpectedResult = "System.*")]
        [TestCase("System.Text.Json", ExpectedResult = "System.Text.Json")]
        [TestCase("System.Collections", ExpectedResult = "System.Collections.*")]
        [TestCase("System.Collections.Concurrency", ExpectedResult = "System.Collections.*")]
        [TestCase("System.Collections.Generic", ExpectedResult = "System.Collections.Generic.*")]
        [TestCase("Microsoft", ExpectedResult = "*")]
        public string? TryGetMatchingPattern_FindsPattern(string ns)
        {
            var collection = new PatternCollection('.')
            {
                "System.Text.Json",
                "System.*",
                "System.Collections.*",
                "System.Collections.Generic.*",
                "*"
            };

            collection.TryGetMatchingPattern(ns, out var pattern);
            return pattern;
        }

        [Test]
        public void Clear_EmptiesCollection()
        {
            var collection = new PatternCollection('.')
            {
                "System.Text",
                "System.*",
                "*",
            };

            collection.Clear();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(collection, Is.Empty);
                Assert.That(collection.MatchesAll, Is.False);
                Assert.That(collection.Matches("System.Text"), Is.False);
                Assert.That(collection.TryGetMatchingPattern("System.Text", out var pattern), Is.False);
                Assert.That(pattern, Is.Null);
            }
        }

        [Test]
        public void CopyTo_CopiessToArray()
        {
            var collection = new PatternCollection('.')
            {
                "System.Text",
                "System.*",
                "*",
            };

            var array = new string[collection.Count];
            ((ICollection<string>)collection).CopyTo(array, 0);

            Assert.That(array, Is.EquivalentTo(["System.Text", "System.*", "*"]));
        }

        [Test]
        public void CopyTo_WithInvalidArguments_ThrowsDocumentedExceptions()
        {
            ICollection<string> collection = new PatternCollection('.') { "System.Text", "System.*" };

            using (Assert.EnterMultipleScope())
            {
                Assert.That(() => collection.CopyTo(null!, 0), Throws.ArgumentNullException);
                Assert.That(() => collection.CopyTo(new string[2], -1), Throws.TypeOf<ArgumentOutOfRangeException>());
                Assert.That(() => collection.CopyTo(new string[2], 1), Throws.TypeOf<ArgumentException>());
            }
        }
    }
}

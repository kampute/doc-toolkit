// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Collections
{
    using Kampute.DocToolkit.Collections;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class PatternCollectionTests
    {
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
        public void Contains_WithExisting_Exact_ReturnsTrue()
        {
            var collection = new PatternCollection('.')
            {
                "System.Text"
            };

            Assert.That(collection, Does.Contain("System.Text"));
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
        public void Contains_WithExisting_Wildcard_ReturnsTrue()
        {
            var collection = new PatternCollection('.')
            {
                "System.*"
            };

            Assert.That(collection, Does.Contain("System.*"));
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

            Assert.That(collection, Is.Empty);
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
        public void GetEnumerator_Enumeratess()
        {
            var collection = new PatternCollection('.')
            {
                "System.Text",
                "System.*",
                "*"
            };

            var enumerated = collection.ToList();

            Assert.That(enumerated, Is.EquivalentTo(["System.Text", "System.*", "*"]));
        }
    }
}

// Copyright (C) 2019-2025 Kampute
//
// This file is part of the Kampute.DocToolkit package and is released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Collections
{
    using Kampute.DocToolkit.Collections;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    [TestFixture]
    public class ReferenceEqualityComparerTests
    {
        private sealed class TestObject
        {
            public string Value { get; set; }

            public TestObject(string value) => Value = value;

            public override bool Equals(object? obj) => obj is TestObject other && Value == other.Value;

            public override int GetHashCode() => Value.GetHashCode();
        }

        [Test]
        public void Instance_ReturnsSameInstance()
        {
            var instance1 = ReferenceEqualityComparer<TestObject>.Instance;
            var instance2 = ReferenceEqualityComparer<TestObject>.Instance;

            Assert.That(instance1, Is.SameAs(instance2));
        }

        [Test]
        public void Equals_SameReference_ReturnsTrue()
        {
            var comparer = ReferenceEqualityComparer<TestObject>.Instance;
            var obj = new TestObject("test");

            Assert.That(comparer.Equals(obj, obj), Is.True);
        }

        [Test]
        public void Equals_BothNull_ReturnsTrue()
        {
            var comparer = ReferenceEqualityComparer<TestObject>.Instance;

            Assert.That(comparer.Equals(null, null), Is.True);
        }

        [Test]
        public void Equals_OneNull_ReturnsFalse()
        {
            var comparer = ReferenceEqualityComparer<TestObject>.Instance;
            var obj = new TestObject("test");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(comparer.Equals(obj, null), Is.False);
                Assert.That(comparer.Equals(null, obj), Is.False);
            }
        }

        [Test]
        public void Equals_IgnoresValueEquality()
        {
            var comparer = ReferenceEqualityComparer<TestObject>.Instance;
            var obj1 = new TestObject("test");
            var obj2 = new TestObject("test");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(obj1, Is.EqualTo(obj2), "Value equality should be true");
                Assert.That(comparer.Equals(obj1, obj2), Is.False, "Reference equality should be false");
            }
        }

        [Test]
        public void GetHashCode_Null_ReturnsZero()
        {
            var comparer = ReferenceEqualityComparer<TestObject>.Instance;

            Assert.That(comparer.GetHashCode(null), Is.Zero);
        }

        [Test]
        public void GetHashCode_ReturnsIdentityHashCodeInsteadOfContentBasedHashCode()
        {
            var comparer = ReferenceEqualityComparer<TestObject>.Instance;
            var obj = new TestObject("test");

            Assert.That(comparer.GetHashCode(obj), Is.EqualTo(RuntimeHelpers.GetHashCode(obj))); // Documented in the class remarks
        }

        [Test]
        public void UsageInHashSet_DistinguishesByReference()
        {
            var comparer = ReferenceEqualityComparer<TestObject>.Instance;
            var set = new HashSet<TestObject>(comparer);

            var obj1 = new TestObject("test");
            var obj2 = new TestObject("test");

            set.Add(obj1);
            set.Add(obj2);
            set.Add(obj1);
            set.Add(obj2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(set, Has.Count.EqualTo(2));
                Assert.That(set, Does.Contain(obj1));
                Assert.That(set, Does.Contain(obj2));
            }
        }
    }
}

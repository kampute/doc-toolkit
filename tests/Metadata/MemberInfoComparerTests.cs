// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.Reflection;

    [TestFixture]
    public static class MemberInfoComparerTests
    {
        private static readonly MemberInfoComparer Comparer = MemberInfoComparer.Instance;

        [Test]
        public static void Equals_GenericTypeParametersWithDifferentOwners_ReturnsFalse()
        {
            var first = typeof(TestTypes.IndependentGenericClass<>).GetGenericArguments()[0];
            var second = typeof(TestTypes.AnotherIndependentGenericClass<>).GetGenericArguments()[0];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(first, second), Is.False);
                Assert.That(Comparer.GetHashCode(first), Is.Not.EqualTo(Comparer.GetHashCode(second)));
            }
        }

        [Test]
        public static void Equals_GenericMethodParametersWithDifferentOwners_ReturnsFalse()
        {
            var host = typeof(TestTypes.GenericMethodHost);
            var first = host.GetMethod(nameof(TestTypes.GenericMethodHost.FirstMethod), BindingFlags.Public | BindingFlags.Static)!.GetGenericArguments()[0];
            var second = host.GetMethod(nameof(TestTypes.GenericMethodHost.SecondMethod), BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes)!.GetGenericArguments()[0];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(first, second), Is.False);
                Assert.That(Comparer.GetHashCode(first), Is.Not.EqualTo(Comparer.GetHashCode(second)));
            }
        }

        [Test]
        public static void Equals_NestedTypesResolvedThroughDifferentPaths_ReturnsTrue()
        {
            var direct = typeof(TestTypes.TestNestedClass.InnerClass.DeepestClass);
            var viaReflection = typeof(TestTypes.TestNestedClass)
                .GetNestedType("InnerClass", BindingFlags.Public | BindingFlags.NonPublic)!
                .GetNestedType("DeepestClass", BindingFlags.Public | BindingFlags.NonPublic)!;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(direct, viaReflection), Is.True);
                Assert.That(Comparer.GetHashCode(direct), Is.EqualTo(Comparer.GetHashCode(viaReflection)));
            }
        }

        [Test]
        public static void Equals_BaseTypesResolvedThroughDifferentPaths_ReturnsTrue()
        {
            var direct = typeof(TestTypes.TestBaseClass);
            var viaReflection = typeof(TestTypes.TestDerivedClass).BaseType!;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(direct, viaReflection), Is.True);
                Assert.That(Comparer.GetHashCode(direct), Is.EqualTo(Comparer.GetHashCode(viaReflection)));
            }
        }

        [Test]
        public static void Equals_GenericBaseTypesResolvedThroughDifferentPaths_ReturnsTrue()
        {
            var direct = typeof(TestTypes.GenericBaseClass<>);
            var viaReflection = typeof(TestTypes.GenericDerivedClass<>).BaseType!;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(direct, viaReflection), Is.True);
                Assert.That(Comparer.GetHashCode(direct), Is.EqualTo(Comparer.GetHashCode(viaReflection)));
            }
        }

        [Test]
        public static void Equals_ConstructedGenericBaseTypesResolvedThroughDifferentPaths_ReturnsTrue()
        {
            var direct = typeof(TestTypes.GenericBaseClass<int>);
            var viaReflection = typeof(TestTypes.ConstructedGenericDerivedClass).BaseType!;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(direct, viaReflection), Is.True);
                Assert.That(Comparer.GetHashCode(direct), Is.EqualTo(Comparer.GetHashCode(viaReflection)));
            }
        }

        [Test]
        public static void Equals_ImplementedInterfacesResolvedThroughDifferentPaths_ReturnsTrue()
        {
            var direct = typeof(TestTypes.ITestInterface);
            var viaReflection = typeof(TestTypes.TestDerivedClass).GetInterfaces()[0];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(direct, viaReflection), Is.True);
                Assert.That(Comparer.GetHashCode(direct), Is.EqualTo(Comparer.GetHashCode(viaReflection)));
            }
        }

        [Test]
        public static void Equals_GenericImplementedInterfacesResolvedThroughDifferentPaths_ReturnsTrue()
        {
            var direct = typeof(TestTypes.IGenericTestInterface<>);
            var viaReflection = typeof(TestTypes.GenericImplementedClass<>).GetInterfaces()[0];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(direct, viaReflection), Is.True);
                Assert.That(Comparer.GetHashCode(direct), Is.EqualTo(Comparer.GetHashCode(viaReflection)));
            }
        }

        [Test]
        public static void Equals_ConstructedGenericImplementedInterfacesResolvedThroughDifferentPaths_ReturnsTrue()
        {
            var direct = typeof(TestTypes.IGenericTestInterface<string>);
            var viaReflection = typeof(TestTypes.ConstructedGenericImplementedClass).GetInterfaces()[0];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(direct, viaReflection), Is.True);
                Assert.That(Comparer.GetHashCode(direct), Is.EqualTo(Comparer.GetHashCode(viaReflection)));
            }
        }

        [Test]
        public static void Equals_ConstructedGenericTypeWithSameArguments_ReturnsTrue()
        {
            var first = typeof(TestTypes.GenericBaseClass<int>);
            var second = typeof(TestTypes.GenericBaseClass<int>);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(first, second), Is.True);
                Assert.That(Comparer.GetHashCode(first), Is.EqualTo(Comparer.GetHashCode(second)));
            }
        }

        [Test]
        public static void Equals_ConstructedGenericTypesWithDifferentArguments_ReturnsFalse()
        {
            var first = typeof(TestTypes.GenericBaseClass<int>);
            var second = typeof(TestTypes.GenericBaseClass<long>);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(first, second), Is.False);
                Assert.That(Comparer.GetHashCode(first), Is.Not.EqualTo(Comparer.GetHashCode(second)));
            }
        }

        [Test]
        public static void Equals_GenericTypeDefinitionAndConstructedGenericType_ReturnsFalse()
        {
            var first = typeof(TestTypes.GenericBaseClass<>);
            var second = typeof(TestTypes.GenericBaseClass<int>);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(first, second), Is.False);
                Assert.That(Comparer.GetHashCode(first), Is.Not.EqualTo(Comparer.GetHashCode(second)));
            }
        }

        [Test]
        public static void Equals_DifferentGenericTypeArgumentsFromSameGenericDefinition_ReturnsFalse()
        {
            var first = typeof(TestTypes.GenericBaseClass<int>).GetGenericArguments()[0];
            var second = typeof(TestTypes.GenericBaseClass<long>).GetGenericArguments()[0];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(first, second), Is.False);
                Assert.That(Comparer.GetHashCode(first), Is.Not.EqualTo(Comparer.GetHashCode(second)));
            }
        }

        [Test]
        public static void Equals_SameGenericTypeArgumentsFromSameGenericDefinition_ReturnsTrue()
        {
            var first = typeof(TestTypes.GenericBaseClass<int>).GetGenericArguments()[0];
            var second = typeof(TestTypes.GenericBaseClass<int>).GetGenericArguments()[0];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(first, second), Is.True);
                Assert.That(Comparer.GetHashCode(first), Is.EqualTo(Comparer.GetHashCode(second)));
            }
        }
    }
}
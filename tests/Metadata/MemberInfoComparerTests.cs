// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public static class MemberInfoComparerTests
    {
        private static readonly MemberInfoComparer Comparer = MemberInfoComparer.Instance;

        [Test]
        public static void Equals_GenericTypeParametersWithDifferentOwners_ReturnsFalse()
        {
            var first = typeof(Acme.SampleGenericClass<>).GetGenericArguments()[0];
            var second = typeof(Acme.SampleGenericStruct<>).GetGenericArguments()[0];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(first, second), Is.False);
                Assert.That(Comparer.GetHashCode(first), Is.Not.EqualTo(Comparer.GetHashCode(second)));
            }
        }

        [Test]
        public static void Equals_GenericMethodParametersWithDifferentOwners_ReturnsFalse()
        {
            var host = typeof(Acme.SampleMethods);
            var first = host.GetMethod(nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints))!.GetGenericArguments()[0];
            var second = host.GetMethod(nameof(Acme.SampleMethods.GenericMethodWithoutTypeConstraints))!.GetGenericArguments()[0];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(first, second), Is.False);
                Assert.That(Comparer.GetHashCode(first), Is.Not.EqualTo(Comparer.GetHashCode(second)));
            }
        }

        [Test]
        public static void Equals_NestedTypesResolvedThroughDifferentPaths_ReturnsTrue()
        {
            var direct = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass);
            var viaReflection = typeof(Acme.SampleGenericClass<>)
                .GetNestedType("InnerGenericClass`2")!
                .GetNestedType("DeepInnerGenericClass")!;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(direct, viaReflection), Is.True);
                Assert.That(Comparer.GetHashCode(direct), Is.EqualTo(Comparer.GetHashCode(viaReflection)));
            }
        }

        [Test]
        public static void Equals_GenericBaseTypesResolvedThroughDifferentPaths_ReturnsTrue()
        {
            var direct = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass);
            var viaReflection = typeof(Acme.SampleDerivedGenericClass<,,>).BaseType!;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(direct, viaReflection), Is.True);
                Assert.That(Comparer.GetHashCode(direct), Is.EqualTo(Comparer.GetHashCode(viaReflection)));
            }
        }

        [Test]
        public static void Equals_ConstructedGenericBaseTypesResolvedThroughDifferentPaths_ReturnsTrue()
        {
            var direct = typeof(Acme.SampleDerivedGenericClass<object, int, string>);
            var viaReflection = typeof(Acme.SampleDerivedConstructedGenericClass).BaseType!;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(direct, viaReflection), Is.True);
                Assert.That(Comparer.GetHashCode(direct), Is.EqualTo(Comparer.GetHashCode(viaReflection)));
            }
        }

        [Test]
        public static void Equals_ImplementedInterfacesResolvedThroughDifferentPaths_ReturnsTrue()
        {
            var direct = typeof(Acme.ISampleInterface);
            var viaReflection = typeof(Acme.SampleDerivedConstructedGenericClass).GetInterfaces()
                .First(static i => i.Name == nameof(Acme.ISampleInterface));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(direct, viaReflection), Is.True);
                Assert.That(Comparer.GetHashCode(direct), Is.EqualTo(Comparer.GetHashCode(viaReflection)));
            }
        }

        [Test]
        public static void Equals_GenericImplementedInterfacesResolvedThroughDifferentPaths_ReturnsTrue()
        {
            var direct = typeof(System.Collections.Generic.IEnumerable<>);
            var viaReflection = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass).GetInterfaces()
                .First(static i => i.Name == "IEnumerable`1");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(direct, viaReflection), Is.True);
                Assert.That(Comparer.GetHashCode(direct), Is.EqualTo(Comparer.GetHashCode(viaReflection)));
            }
        }

        [Test]
        public static void Equals_ConstructedGenericImplementedInterfacesResolvedThroughDifferentPaths_ReturnsTrue()
        {
            var direct = typeof(System.Collections.Generic.IEnumerable<string>);
            var viaReflection = typeof(Acme.SampleDerivedConstructedGenericClass).GetInterfaces()
                .First(static i => i.Name == "IEnumerable`1");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(direct, viaReflection), Is.True);
                Assert.That(Comparer.GetHashCode(direct), Is.EqualTo(Comparer.GetHashCode(viaReflection)));
            }
        }

        [Test]
        public static void Equals_GenericTypeDefinitionAndConstructedGenericType_ReturnsFalse()
        {
            var first = typeof(Acme.SampleGenericClass<>);
            var second = typeof(Acme.SampleGenericClass<object>);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(first, second), Is.False);
                Assert.That(Comparer.GetHashCode(first), Is.Not.EqualTo(Comparer.GetHashCode(second)));
            }
        }

        [Test]
        public static void Equals_DifferentGenericTypeArgumentsFromSameGenericDefinition_ReturnsFalse()
        {
            var first = typeof(Acme.SampleGenericClass<object>).GetGenericArguments()[0];
            var second = typeof(Acme.SampleGenericClass<string>).GetGenericArguments()[0];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(first, second), Is.False);
                Assert.That(Comparer.GetHashCode(first), Is.Not.EqualTo(Comparer.GetHashCode(second)));
            }
        }

        [Test]
        public static void Equals_SameGenericTypeArgumentsFromSameGenericDefinition_ReturnsTrue()
        {
            var first = typeof(Acme.SampleGenericClass<object>).GetGenericArguments()[0];
            var second = typeof(Acme.SampleGenericClass<object>).GetGenericArguments()[0];

            using (Assert.EnterMultipleScope())
            {
                Assert.That(Comparer.Equals(first, second), Is.True);
                Assert.That(Comparer.GetHashCode(first), Is.EqualTo(Comparer.GetHashCode(second)));
            }
        }
    }
}
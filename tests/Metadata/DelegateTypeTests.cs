// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.Linq;

    [TestFixture]
    public class DelegateTypeTests
    {
        [TestCase(typeof(Action), "Action")]
        [TestCase(typeof(Action<>), "Action`1")]
        [TestCase(typeof(Func<>), "Func`1")]
        [TestCase(typeof(Func<int, string>), "Func`2")]
        public void ImplementsDelegateType(Type delegateType, string expectedName)
        {
            var metadata = delegateType.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IDelegateType>());
            Assert.That(metadata.Name, Is.EqualTo(expectedName));
        }

        [Test]
        public void NonGeneric_HasCorrectMetadata()
        {
            var metadata = typeof(Action).GetMetadata<IDelegateType>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsGenericTypeDefinition, Is.False);
                Assert.That(metadata.TypeParameters, Is.Empty);

                Assert.That(metadata.IsConstructedGenericType, Is.False);
                Assert.That(metadata.TypeArguments, Is.Empty);
                Assert.That(metadata.GenericTypeDefinition, Is.Null);

                Assert.That(metadata.Parameters, Is.Empty);
                Assert.That(metadata.Return.Type.Name, Is.EqualTo("Void"));
            }
        }

        [Test]
        public void GenericTypeDefinition_HasCorrectGenericMetadata()
        {
            var metadata = typeof(Func<,>).GetMetadata<IDelegateType>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsGenericTypeDefinition, Is.True);
                Assert.That(metadata.TypeParameters, Has.Count.EqualTo(2));
                Assert.That(metadata.TypeParameters[0].Name, Is.EqualTo("T"));
                Assert.That(metadata.TypeParameters[1].Name, Is.EqualTo("TResult"));

                Assert.That(metadata.IsConstructedGenericType, Is.False);
                Assert.That(metadata.TypeArguments, Is.Empty);
                Assert.That(metadata.GenericTypeDefinition, Is.Null);

                Assert.That(metadata.Parameters, Has.Count.EqualTo(1));
                Assert.That(metadata.Parameters[0].Type, Is.InstanceOf<ITypeParameter>());
                Assert.That(metadata.Parameters[0].Type.Name, Is.EqualTo("T"));
                Assert.That(metadata.Return.Type, Is.InstanceOf<ITypeParameter>());
                Assert.That(metadata.Return.Type.Name, Is.EqualTo("TResult"));
            }
        }

        [Test]
        public void ConstructedGenericType_HasCorrectGenericMetadata()
        {
            var metadata = typeof(Func<string, int>).GetMetadata<IDelegateType>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsGenericTypeDefinition, Is.False);
                Assert.That(metadata.TypeParameters, Is.Empty);

                Assert.That(metadata.IsConstructedGenericType, Is.True);
                Assert.That(metadata.TypeArguments, Has.Count.EqualTo(2));
                Assert.That(metadata.TypeArguments[0].Name, Is.EqualTo("String"));
                Assert.That(metadata.TypeArguments[1].Name, Is.EqualTo("Int32"));
                Assert.That(metadata.GenericTypeDefinition, Is.Not.Null);

                Assert.That(metadata.Parameters, Has.Count.EqualTo(1));
                Assert.That(metadata.Parameters[0].Type.Name, Is.EqualTo("String"));
                Assert.That(metadata.Return.Type.Name, Is.EqualTo("Int32"));
            }
        }

        [TestCase(typeof(Action), 0, 0)]
        [TestCase(typeof(Action<>), 0, 1)]
        [TestCase(typeof(Action<int>), 0, 1)]
        [TestCase(typeof(Func<,>), 0, 2)]
        [TestCase(typeof(Func<string, int>), 0, 2)]
        [TestCase(typeof(Func<string, int, bool>), 0, 3)]
        public void OwnGenericParameterRange_HasExpectedValue(Type delegateType, int expectedOffset = 0, int expectedCount = 0)
        {
            var metadata = delegateType.GetMetadata<IDelegateType>();

            Assert.That(metadata.OwnGenericParameterRange, Is.EqualTo((expectedOffset, expectedCount)));
        }

        [TestCase(typeof(Action), ExpectedResult = "T:System.Action")]
        [TestCase(typeof(Action<>), ExpectedResult = "T:System.Action`1")]
        [TestCase(typeof(Func<>), ExpectedResult = "T:System.Func`1")]
        [TestCase(typeof(Func<,>), ExpectedResult = "T:System.Func`2")]
        public string CodeReference_HasExpectedValue(Type delegateType)
        {
            var metadata = delegateType.GetMetadata<IDelegateType>();

            return metadata.CodeReference;
        }

        [TestCase(typeof(Action),
            nameof(Object),
            nameof(Delegate),
            nameof(MulticastDelegate)
        )]
        public void BaseTypeHierarchy_HasExpectedValue(Type type, params string[] expectedNames)
        {
            var metadata = type.GetMetadata<IDelegateType>();

            Assert.That(metadata.BaseTypeHierarchy.Select(t => t.Name), Is.EqualTo(expectedNames));
        }

        [TestCase(typeof(Func<>), typeof(Action), ExpectedResult = false)]
        [TestCase(typeof(Action<>), typeof(Action), ExpectedResult = false)]
        [TestCase(typeof(Func<int, string>), typeof(Func<,>), ExpectedResult = false)]
        [TestCase(typeof(Action<int, string>), typeof(Action<int, string>), ExpectedResult = true)]
        public bool IsAssignableFrom_ReturnsExpectedResult(Type targetType, Type sourceType)
        {
            var targetMetadata = targetType.GetMetadata<IDelegateType>();
            var sourceMetadata = sourceType.GetMetadata();

            return targetMetadata.IsAssignableFrom(sourceMetadata);
        }

        [TestCase(typeof(Func<>), typeof(Action), ExpectedResult = false)]
        [TestCase(typeof(Action<>), typeof(Action), ExpectedResult = false)]
        [TestCase(typeof(Action<>), typeof(Action<>), ExpectedResult = true)]
        [TestCase(typeof(Func<int, string>), typeof(Func<,>), ExpectedResult = false)]
        [TestCase(typeof(Action<int, string>), typeof(Action<int, string>), ExpectedResult = true)]
        [TestCase(typeof(Action<,>), typeof(Action<int, string>), ExpectedResult = true)]
        [TestCase(typeof(Action<int, string>), typeof(Action<,>), ExpectedResult = false)]
        public bool IsSubstitutableBy_ReturnsExpectedResult(Type targetType, Type sourceType)
        {
            var targetMetadata = targetType.GetMetadata<IDelegateType>();
            var sourceMetadata = sourceType.GetMetadata();

            return targetMetadata.IsSubstitutableBy(sourceMetadata);
        }
    }
}

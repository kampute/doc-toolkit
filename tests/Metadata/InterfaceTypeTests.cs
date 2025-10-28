// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class InterfaceTypeTests
    {
        [TestCase(typeof(IDisposable), nameof(IDisposable))]
        [TestCase(typeof(IEnumerable<>), "IEnumerable`1")]
        [TestCase(typeof(IEnumerable<string>), "IEnumerable`1")]
        [TestCase(typeof(Acme.IProcess<>), "IProcess`1")]
        public void ImplementsInterfaceType(Type interfaceType, string expectedName)
        {
            var metadata = interfaceType.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IInterfaceType>());
            Assert.That(metadata.Name, Is.EqualTo(expectedName));
        }

        [Test]
        public void GenericTypeDefinition_HasCorrectGenericMetadata()
        {
            var metadata = typeof(IEnumerable<>).GetMetadata<IInterfaceType>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsGenericTypeDefinition, Is.True);
                Assert.That(metadata.TypeParameters, Has.Count.EqualTo(1));
                Assert.That(metadata.TypeParameters[0].Name, Is.EqualTo("T"));

                Assert.That(metadata.IsConstructedGenericType, Is.False);
                Assert.That(metadata.TypeArguments, Is.Empty);
                Assert.That(metadata.GenericTypeDefinition, Is.Null);
            }
        }

        [Test]
        public void ConstructedGenericType_HasCorrectGenericMetadata()
        {
            var metadata = typeof(IEnumerable<int>).GetMetadata<IInterfaceType>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsGenericTypeDefinition, Is.False);
                Assert.That(metadata.TypeParameters, Is.Empty);

                Assert.That(metadata.IsConstructedGenericType, Is.True);
                Assert.That(metadata.TypeArguments, Has.Count.EqualTo(1));
                Assert.That(metadata.TypeArguments[0].Name, Is.EqualTo("Int32"));
                Assert.That(metadata.GenericTypeDefinition, Is.Not.Null);
            }
        }

        [TestCase(typeof(IDisposable), 0, 0)]
        [TestCase(typeof(IEnumerable<>), 0, 1)]
        [TestCase(typeof(IEnumerable<int>), 0, 1)]
        [TestCase(typeof(IDictionary<string, int>), 0, 2)]
        [TestCase(typeof(Acme.IProcess<string>), 0, 1)]
        public void OwnGenericParameterRange_HasExpectedValue(Type interfaceType, int expectedOffset = 0, int expectedCount = 0)
        {
            var metadata = interfaceType.GetMetadata<IGenericCapableType>();

            Assert.That(metadata.OwnGenericParameterRange, Is.EqualTo((expectedOffset, expectedCount)));
        }

        [TestCase(typeof(IEnumerable<>))]
        public void Interfaces_HasExpectedValue(Type interfaceType)
        {
            var metadata = interfaceType.GetMetadata() as IInterfaceType;
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.Interfaces, Is.Not.Empty);
        }

        [TestCase(typeof(IEnumerable<>))]
        public void Methods_HasExpectedValue(Type interfaceType)
        {
            var metadata = interfaceType.GetMetadata<IInterfaceType>();

            Assert.That(metadata.Methods, Is.Not.Empty);
        }

        [TestCase(typeof(Acme.IProcess<>))]
        public void Properties_HasExpectedValue(Type interfaceType)
        {
            var metadata = interfaceType.GetMetadata<IInterfaceType>();

            Assert.That(metadata.Properties, Is.Not.Empty);
        }

        [TestCase(typeof(Acme.IProcess<>))]
        public void Events_HasExpectedValue(Type interfaceType)
        {
            var metadata = interfaceType.GetMetadata<IInterfaceType>();

            Assert.That(metadata.Events, Is.Not.Empty);
        }

        [TestCase(typeof(IDisposable), ExpectedResult = "T:System.IDisposable")]
        [TestCase(typeof(IEnumerable<>), ExpectedResult = "T:System.Collections.Generic.IEnumerable`1")]
        [TestCase(typeof(Acme.IProcess<>), ExpectedResult = "T:Acme.IProcess`1")]
        public string CodeReference_HasExpectedValue(Type interfaceType)
        {
            var metadata = interfaceType.GetMetadata<IInterfaceType>();

            return metadata.CodeReference;
        }

        [TestCase(typeof(ICloneable), ExpectedResult = new string[] { })]
        [TestCase(typeof(TestTypes.ITestInterface), ExpectedResult = new[] { nameof(TestTypes) })]
        public string[] DeclaringTypeHierarchy_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IInterfaceType>();

            return [.. metadata.DeclaringTypeHierarchy.Select(t => t.Name)];
        }

        [TestCase(typeof(TestTypes.ITestInterface))]
        [TestCase(typeof(TestTypes.IExtendedTestInterface))]
        public void BaseTypeHierarchy_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IInterfaceType>();

            Assert.That(metadata.BaseTypeHierarchy, Is.Empty);
        }

        [TestCase(typeof(TestTypes.IExtendedTestInterface), ExpectedResult = new[] { nameof(IDisposable), nameof(TestTypes.ITestInterface) })]
        public string[] ImplementedInterfaces_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IInterfaceType>();

            return [.. metadata.ImplementedInterfaces.Select(i => i.Name).OrderBy(n => n)];
        }

        [TestCase(typeof(TestTypes.ITestInterface), ExpectedResult = new[] {
            nameof(TestTypes.IExtendedTestInterface),              // Extends ITestInterface
            nameof(TestTypes.TestBaseClass),                       // Directly implements ITestInterface
            nameof(TestTypes.TestDerivedClass),                    // Inherits from TestBaseClass, indirectly
            nameof(TestTypes.TestGrandChildClass),                 // Inherits from TestDerivedClass, indirectly
            nameof(TestTypes.TestValueType),                       // Implements ITestInterface
        })]
        [TestCase(typeof(TestTypes.IGenericTestInterface<>), ExpectedResult = new[] {
            nameof(TestTypes.ConstructedGenericImplementedClass),  // Implements IGenericTestInterface<string>
            "GenericImplementedClass`1",                           // Directly implements IGenericTestInterface<T>
            "IExtendedGenericTestInterface`1",                     // Extends IGenericTestInterface<T>
        })]
        public string[] ImplementingTypes_HasExpectedValue(Type type)
        {
            var metadata = type.GetMetadata<IInterfaceType>();

            return [.. metadata.ImplementingTypes.Select(t => t.Name).OrderBy(n => n)];
        }

        [TestCase(typeof(TestTypes.ITestInterface), typeof(TestTypes.ITestInterface), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.ITestInterface), typeof(TestTypes.TestDerivedClass), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.IGenericTestInterface<>), typeof(TestTypes.GenericImplementedClass<>), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.IGenericTestInterface<string>), typeof(TestTypes.ConstructedGenericImplementedClass), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.IGenericTestInterface<>), typeof(TestTypes.IExtendedGenericTestInterface<>), ExpectedResult = true)]
        [TestCase(typeof(TestTypes.IGenericTestInterface<string>), typeof(TestTypes.IGenericTestInterface<int>), ExpectedResult = false)]
        [TestCase(typeof(TestTypes.IGenericTestInterface<string>), typeof(TestTypes.IGenericTestInterface<string>), ExpectedResult = true)]
        public bool IsAssignableFrom_ReturnsExpectedResult(Type targetType, Type sourceType)
        {
            var targetMetadata = targetType.GetMetadata<IInterfaceType>();
            var sourceMetadata = sourceType.GetMetadata();

            return targetMetadata.IsAssignableFrom(sourceMetadata);
        }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata.Reflection
{
    using Kampute.DocToolkit.Metadata.Reflection;
    using NUnit.Framework;
    using System;
    using System.Linq;

    [TestFixture]
    public class ExtensionContainerInfoTests
    {
        [Test]
        public void Constructor_WithValidStaticClass_CreatesInstance()
        {
            var containerType = typeof(Acme.SampleExtensions);

            var container = new ExtensionContainerInfo(containerType);

            Assert.That(container, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNonStaticClass_ThrowsArgumentException()
        {
            var containerType = typeof(Acme.SampleMethods);

            Assert.That(() => new ExtensionContainerInfo(containerType), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithNestedClass_ThrowsArgumentException()
        {
            var containerType = typeof(Acme.SampleGenericClass<object>.InnerGenericClass<int, string>);

            Assert.That(() => new ExtensionContainerInfo(containerType), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithGenericClass_ThrowsArgumentException()
        {
            var containerType = typeof(Acme.SampleGenericClass<>);

            Assert.That(() => new ExtensionContainerInfo(containerType), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new ExtensionContainerInfo(null!), Throws.ArgumentNullException);
        }

        [TestCase(typeof(Acme.SampleExtensions), ExpectedResult = 4)]
        public int ExtensionBlocks_HasExpectedNumberOfBlocks(Type containerType)
        {
            var container = new ExtensionContainerInfo(containerType);

            return container.ExtensionBlocks.Count;
        }

        [TestCase(typeof(Acme.SampleExtensions), ExpectedResult = 12)]
        public int ExtensionBlockMethods_HasExpectedNumberOfMethods(Type containerType)
        {
            var container = new ExtensionContainerInfo(containerType);

            return container.ExtensionBlockMethods.Count();
        }

        [TestCase(typeof(Acme.SampleExtensions), ExpectedResult = 9)]
        public int ExtensionBlockProperties_HasExpectedNumberOfProperties(Type containerType)
        {
            var container = new ExtensionContainerInfo(containerType);

            return container.ExtensionBlockProperties.Count();
        }

        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethod))]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethod))]
        public void GetNormalizedMethodInfo_WithExtensionBlockMethod_ReturnsNormalizedMethodInfo(Type containerType, string methodName)
        {
            var container = new ExtensionContainerInfo(containerType);
            var method = containerType.GetMethod(methodName, Acme.Bindings.AllDeclared)!;

            var canonical = container.GetNormalizedMethodInfo(method);

            Assert.That(canonical, Is.InstanceOf<IExtensionBlockMemberInfo>());
        }

        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.NonExtensionMethod))]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.ClassicExtensionMethod))]
        public void GetNormalizedMethodInfo_WithNonExtensionBockMethod_ReturnsOriginalMethodInfo(Type containerType, string methodName)
        {
            var container = new ExtensionContainerInfo(containerType);
            var method = containerType.GetMethod(methodName, Acme.Bindings.AllDeclared)!;

            var canonical = container.GetNormalizedMethodInfo(method);

            Assert.That(canonical, Is.SameAs(method));
        }

        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethod))]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethod))]
        public void GetExtensionMemberInfo_WithExtensionBlockMethod_ReturnsNormalizedMethodInfo(Type containerType, string methodName)
        {
            var container = new ExtensionContainerInfo(containerType);
            var method = containerType.GetMethod(methodName, Acme.Bindings.AllDeclared)!;

            var memberInfo = container.GetExtensionMemberInfo(method);

            Assert.That(memberInfo, Is.InstanceOf<System.Reflection.MethodInfo>());
            Assert.That(memberInfo, Is.InstanceOf<IExtensionBlockMethodInfo>());
            Assert.That(memberInfo, Is.Not.SameAs(method));
        }

        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_InstanceExtensionProperty))]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_StaticExtensionProperty))]
        public void GetExtensionMemberInfo_WithExtensionBlockPropertyAccessor_ReturnsPropertyInfo(Type containerType, string methodName)
        {
            var container = new ExtensionContainerInfo(containerType);
            var accessor = containerType.GetMethod(methodName, Acme.Bindings.AllDeclared)!;

            var memberInfo = container.GetExtensionMemberInfo(accessor);

            Assert.That(memberInfo, Is.InstanceOf<System.Reflection.PropertyInfo>());
            Assert.That(memberInfo, Is.InstanceOf<IExtensionBlockPropertyInfo>());
            Assert.That(memberInfo, Is.Not.SameAs(accessor));
        }

        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.NonExtensionMethod))]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.ClassicExtensionMethod))]
        public void GetExtensionMemberInfo_WithNonExtensionBlockMethod_ReturnsNull(Type containerType, string methodName)
        {
            var container = new ExtensionContainerInfo(containerType);
            var method = containerType.GetMethod(methodName, Acme.Bindings.AllDeclared)!;

            var memberInfo = container.GetExtensionMemberInfo(method);

            Assert.That(memberInfo, Is.Null);
        }
    }
}
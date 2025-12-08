// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata.Adapters;
    using Kampute.DocToolkit.Metadata.Reflection;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class ExtensionContainerTests
    {
        [Test]
        public void Constructor_WithValidStaticClass_CreatesInstance()
        {
            var containerType = typeof(Acme.SampleExtensions);

            var container = new ExtensionContainer(containerType);

            Assert.That(container, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNonStaticClass_ThrowsArgumentException()
        {
            var containerType = typeof(Acme.SampleMethods);

            Assert.That(() => new ExtensionContainer(containerType), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithNestedClass_ThrowsArgumentException()
        {
            var containerType = typeof(Acme.SampleGenericClass<object>.InnerGenericClass<int, string>);

            Assert.That(() => new ExtensionContainer(containerType), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithGenericClass_ThrowsArgumentException()
        {
            var containerType = typeof(Acme.SampleGenericClass<>);

            Assert.That(() => new ExtensionContainer(containerType), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new ExtensionContainer(null!), Throws.ArgumentNullException);
        }

        [TestCase(typeof(Acme.SampleExtensions), ExpectedResult = 16)]
        public int DeclaredExtensionMethods_HasExpectedNumberOfMethods(Type containerType)
        {
            var container = new ExtensionContainer(containerType);

            return container.DeclaredExtensionMethods.Count;
        }

        [TestCase(typeof(Acme.SampleExtensions), ExpectedResult = 9)]
        public int DeclaredExtensionProperties_HasExpectedNumberOfProperties(Type containerType)
        {
            var container = new ExtensionContainer(containerType);

            return container.DeclaredExtensionProperties.Count;
        }

        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethod))]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethod))]
        public void GetNormalizedMethodInfo_WithExtensionMethod_ReturnsCanonicalMethod(Type containerType, string methodName)
        {
            var container = new ExtensionContainer(containerType);
            var method = containerType.GetMethod(methodName, Acme.Bindings.AllDeclared)!;

            var canonical = container.GetNormalizedMethodInfo(method);

            Assert.That(canonical, Is.InstanceOf<IExtensionMemberInfo>());
        }

        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.NonExtensionMethod))]
        public void GetNormalizedMethodInfo_WithNonExtensionMethod_ReturnsOriginalMethod(Type containerType, string methodName)
        {
            var container = new ExtensionContainer(containerType);
            var method = containerType.GetMethod(methodName, Acme.Bindings.AllDeclared)!;

            var canonical = container.GetNormalizedMethodInfo(method);

            Assert.That(canonical, Is.SameAs(method));
        }

        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethod))]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethod))]
        public void GetExtensionMemberInfo_WithExtensionMethod_ReturnsMethodInfo(Type containerType, string methodName)
        {
            var container = new ExtensionContainer(containerType);
            var method = containerType.GetMethod(methodName, Acme.Bindings.AllDeclared)!;

            var memberInfo = container.GetExtensionMemberInfo(method!);

            Assert.That(memberInfo, Is.InstanceOf<System.Reflection.MethodInfo>());
        }

        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_InstanceExtensionProperty))]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_StaticExtensionProperty))]
        public void GetExtensionMemberInfo_WithExtensionPropertyAccessor_ReturnsPropertyInfo(Type containerType, string methodName)
        {
            var container = new ExtensionContainer(containerType);
            var accessor = containerType.GetMethod(methodName, Acme.Bindings.AllDeclared)!;

            var memberInfo = container.GetExtensionMemberInfo(accessor!);

            Assert.That(memberInfo, Is.InstanceOf<System.Reflection.PropertyInfo>());
        }

        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.NonExtensionMethod))]
        public void GetExtensionMemberInfo_WithNonExtensionMethod_ReturnsNull(Type containerType, string methodName)
        {
            var container = new ExtensionContainer(containerType);
            var method = containerType.GetMethod(methodName, Acme.Bindings.AllDeclared)!;

            var memberInfo = container.GetExtensionMemberInfo(method);

            Assert.That(memberInfo, Is.Null);
        }
    }
}
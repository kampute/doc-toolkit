// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata.Reflection
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Reflection;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A top-level generic static class, which is not a valid extension container.
    /// </summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    internal static class GenericStaticContainer<T>
    {
    }

    [TestFixture]
    public class ExtensionContainerInfoTests
    {
        /// <summary>
        /// A nested non-generic static class, which is not a valid extension container.
        /// </summary>
        private static class NestedStaticContainer
        {
        }

        [Test]
        public void Constructor_WithValidStaticClass_SetsContainerType()
        {
            var containerType = typeof(Acme.SampleExtensions);

            var container = new ExtensionContainerInfo(containerType);

            Assert.That(container.ContainerType, Is.SameAs(containerType));
        }

        [Test]
        public void Constructor_WithNonStaticClass_ThrowsArgumentException()
        {
            var containerType = typeof(Acme.SampleMethods);

            Assert.That(() => new ExtensionContainerInfo(containerType), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithNestedStaticClass_ThrowsArgumentException()
        {
            var containerType = typeof(NestedStaticContainer);

            Assert.That(() => new ExtensionContainerInfo(containerType), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithGenericStaticClass_ThrowsArgumentException()
        {
            var containerType = typeof(GenericStaticContainer<>);

            Assert.That(() => new ExtensionContainerInfo(containerType), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithNull_ThrowsArgumentNullException()
        {
            Assert.That(static () => new ExtensionContainerInfo(null!), Throws.ArgumentNullException);
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

        [Test]
        public void ExtensionMembers_WithContainerFromMetadataLoadContext_AreAllDiscovered()
        {
            var assemblyPath = typeof(Acme.SampleExtensions).Assembly.Location;
            using var universe = new MetadataUniverse([assemblyPath]);
            var containerType = universe.LoadFromPath(assemblyPath).GetType(typeof(Acme.SampleExtensions).FullName!)!;

            var container = new ExtensionContainerInfo(containerType);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(container.ExtensionBlocks, Has.Count.EqualTo(4));
                Assert.That(container.ExtensionBlockMethods.Count(), Is.EqualTo(12));
                Assert.That(container.ExtensionBlockProperties.Count(), Is.EqualTo(9));
            }
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
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.set_FullExtensionProperty))]
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

        [Test]
        public void GetNormalizedMethodInfo_WithInstanceExtensionBlockMethod_ExcludesReceiverFromParameters()
        {
            var containerType = typeof(Acme.SampleExtensions);
            var container = new ExtensionContainerInfo(containerType);
            var method = containerType.GetMethod(nameof(Acme.SampleExtensions.GenericExtensionMethod), Acme.Bindings.AllDeclared)!;

            var normalized = container.GetNormalizedMethodInfo(method);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(normalized.GetParameters().Select(static p => p.Name), Is.EqualTo(new[] { "value" }));
                Assert.That(((IExtensionBlockMethodInfo)normalized).DeclaredMethod, Is.SameAs(method));
                Assert.That(((IExtensionBlockMethodInfo)normalized).DeclaringBlock.ReceiverParameter.ParameterType, Is.EqualTo(typeof(Acme.ISampleInterface)));
            }
        }

        [Test]
        public void GetNormalizedMethodInfo_WithExtensionBlockPropertyGetter_ReturnsGetterOfExtensionProperty()
        {
            var containerType = typeof(Acme.SampleExtensions);
            var container = new ExtensionContainerInfo(containerType);
            var getter = containerType.GetMethod(nameof(Acme.SampleExtensions.get_InstanceExtensionProperty), Acme.Bindings.AllDeclared)!;

            var normalized = container.GetNormalizedMethodInfo(getter);
            var property = (PropertyInfo)container.GetExtensionMemberInfo(getter)!;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(normalized, Is.SameAs(property.GetMethod));
                Assert.That(normalized.GetParameters(), Is.Empty);
            }
        }

        [Test]
        public void GetNormalizedMethodInfo_WithNull_ThrowsArgumentNullException()
        {
            var container = new ExtensionContainerInfo(typeof(Acme.SampleExtensions));

            Assert.That(() => container.GetNormalizedMethodInfo(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("methodInfo"));
        }

        [Test]
        public void GetNormalizedMethodInfo_WithMethodOfAnotherType_ThrowsArgumentException()
        {
            var container = new ExtensionContainerInfo(typeof(Acme.SampleExtensions));
            var method = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.StaticMethod))!;

            Assert.That(() => container.GetNormalizedMethodInfo(method), Throws.ArgumentException);
        }

        [Test]
        public void GetExtensionMemberInfo_WithNull_ThrowsArgumentNullException()
        {
            var container = new ExtensionContainerInfo(typeof(Acme.SampleExtensions));

            Assert.That(() => container.GetExtensionMemberInfo(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("methodInfo"));
        }

        [Test]
        public void GetExtensionMemberInfo_WithMethodOfAnotherType_ThrowsArgumentException()
        {
            var container = new ExtensionContainerInfo(typeof(Acme.SampleExtensions));
            var method = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.StaticMethod))!;

            Assert.That(() => container.GetExtensionMemberInfo(method), Throws.ArgumentException);
        }
    }
}
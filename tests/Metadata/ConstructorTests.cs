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
    using System.Reflection;

    [TestFixture]
    public class ConstructorTests
    {
        private readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        [Test]
        public void ImplementsConstructor()
        {
            var constructorInfo = typeof(Acme.Widget).GetConstructor([typeof(int), typeof(int)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.InstanceOf<IConstructor>());
        }

        [Test]
        public void IsDefault_ReturnsTrueForPublicParameterlessConstructor()
        {
            var constructorInfo = typeof(Acme.Widget.NestedDerivedClass).GetConstructor(Type.EmptyTypes);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsDefaultConstructor, Is.True);
        }

        [Test]
        public void IsDefault_ReturnsFalseForNonPublicParameterlessConstructor()
        {
            var constructorInfo = typeof(Acme.Widget).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsDefaultConstructor, Is.False);
        }

        [Test]
        public void IsDefault_ReturnsFalseForPublicParameterizedConstructor()
        {
            var constructorInfo = typeof(Acme.Widget).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(string)], null);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsDefaultConstructor, Is.False);
        }

        [Test]
        public void BaseConstructor_ReturnsNullForBaseClass()
        {
            var constructorInfo = typeof(Acme.Widget).GetConstructor([typeof(int), typeof(int)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.BaseConstructor, Is.Null);
        }

        [Test]
        public void BaseConstructor_ReturnsBaseForDerivedClass()
        {
            var constructorInfo = typeof(Acme.Widget.NestedDerivedClass).GetConstructor(Type.EmptyTypes);
            Assert.That(constructorInfo, Is.Not.Null);

            var baseConstructorInfo = typeof(Acme.Widget.NestedClass).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            Assert.That(baseConstructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.BaseConstructor, Is.Not.Null);
            Assert.That(metadata.BaseConstructor.Represents(baseConstructorInfo), Is.True);
        }

        [Test]
        public void IsStatic_ReturnsTrueForStaticConstructor()
        {
            var constructorInfo = typeof(Acme.Widget).GetConstructor(BindingFlags.Static | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsStatic, Is.True);
        }

        [Test]
        public void IsStatic_ReturnsFalseForInstanceConstructor()
        {
            var constructorInfo = typeof(Acme.Widget).GetConstructor([typeof(int), typeof(int)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsStatic, Is.False);
        }

        [Test]
        public void Visibility_ReturnsPublicForPublicConstructor()
        {
            var constructorInfo = typeof(Acme.Widget).GetConstructor([typeof(int), typeof(int)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.Visibility, Is.EqualTo(MemberVisibility.Public));
        }

        [Test]
        public void Visibility_ReturnsInternalForInternalConstructor()
        {
            var constructorInfo = typeof(Acme.Widget).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(string)], null);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.Visibility, Is.EqualTo(MemberVisibility.Protected));
        }

        [Test]
        public void Visibility_ReturnsProtectedForProtectedConstructor()
        {
            var constructorInfo = typeof(Acme.Widget.NestedClass).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.Visibility, Is.EqualTo(MemberVisibility.Protected));
        }

        [Test]
        public void IsVisible_ReturnsTrueForPublicConstructor()
        {
            var constructorInfo = typeof(Acme.Widget).GetConstructor([typeof(int), typeof(int)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsVisible, Is.True);
        }

        [Test]
        public void IsVisible_ReturnsTrueForProtectedConstructor()
        {
            var constructorInfo = typeof(Acme.Widget.NestedClass).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsVisible, Is.True);
        }

        [Test]
        public void IsVisible_ReturnsFalseForInternalConstructor()
        {
            var constructorInfo = typeof(Acme.Widget).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsVisible, Is.False);
        }

        [Test]
        public void IsSpecialName_ReturnsTrue()
        {
            var constructorInfo = typeof(Acme.Widget).GetConstructor([typeof(int), typeof(int)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsSpecialName, Is.True);
        }

        [Test]
        public void CustomAttributes_ContainsExpectedAttribute()
        {
            var constructorInfo = typeof(Acme.Widget).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(string)], null);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            var attributeName = metadata.CustomAttributes.FirstOrDefault(static a => a.Type.FullName == "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute");
            Assert.That(attributeName, Is.Not.Null);
        }

        [TestCase(typeof(Uri), new[] { typeof(string) }, ExpectedResult = "M:System.Uri.#ctor(System.String)")]
        [TestCase(typeof(Acme.Widget), new[] { typeof(string) }, ExpectedResult = "M:Acme.Widget.#ctor(System.String)")]
        public string CodeReference_HasExpectedValue(Type declaringType, params Type[] parameterTypes)
        {
            var constructorInfo = declaringType.GetConstructor(bindingFlags, parameterTypes);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CodeReference;
        }
    }
}

// Copyright (C) Kampute
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
    public class ConstructorTests
    {
        [TestCase(typeof(Acme.SampleConstructors))]
        [TestCase(typeof(Acme.SampleGenericClass<>))]
        public void IsDefaultConstructor_ForInstanceParameterlessConstructor_ReturnsTrue(Type type)
        {
            var constructorInfo = type.GetConstructor(Acme.Bindings.AllDeclared, Type.EmptyTypes);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsDefaultConstructor, Is.True);
        }

        [Test]
        public void IsDefaultConstructor_ForInstanceParameterizedConstructor_ReturnsFalse()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, [typeof(int)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsDefaultConstructor, Is.False);
        }

        [Test]
        public void IsDefaultConstructor_ForStaticConstructor_ReturnsFalse()
        {
            var constructorInfo = typeof(Acme.ISampleInterface).TypeInitializer!;

            var metadata = constructorInfo.GetMetadata();

            Assert.That(metadata.IsDefaultConstructor, Is.False);
        }

        [Test]
        public void BaseConstructor_ReturnsNullForBaseClass()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, [typeof(object)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.BaseConstructor, Is.Null);
        }

        [Test]
        public void BaseConstructor_ReturnsBaseForDerivedClass()
        {
            var constructorInfo = typeof(Acme.SampleDerivedGenericClass<,,>).GetConstructor(Acme.Bindings.AllDeclared, Type.EmptyTypes);
            Assert.That(constructorInfo, Is.Not.Null);

            var baseConstructorInfo = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass).GetConstructor(Acme.Bindings.AllDeclared, Type.EmptyTypes);
            Assert.That(baseConstructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.BaseConstructor, Is.Not.Null);
            Assert.That(metadata.BaseConstructor.Represents(baseConstructorInfo), Is.True);
        }

        [Test]
        public void IsStatic_ReturnsTrueForStaticConstructor()
        {
            var constructorInfo = typeof(Acme.ISampleInterface).GetConstructor(Acme.Bindings.AllDeclared, Type.EmptyTypes);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsStatic, Is.True);
        }

        [Test]
        public void IsStatic_ReturnsFalseForInstanceConstructor()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, [typeof(int)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsStatic, Is.False);
        }

        [Test]
        public void Visibility_ReturnsPublicForPublicConstructor()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, [typeof(int)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.Visibility, Is.EqualTo(MemberVisibility.Public));
        }

        [Test]
        public void Visibility_ReturnsInternalForInternalConstructor()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, [typeof(string)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.Visibility, Is.EqualTo(MemberVisibility.Internal));
        }

        [Test]
        public void Visibility_ReturnsProtectedForProtectedConstructor()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, [typeof(object)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.Visibility, Is.EqualTo(MemberVisibility.Protected));
        }

        [Test]
        public void IsVisible_ReturnsTrueForPublicConstructor()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, [typeof(int)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsVisible, Is.True);
        }

        [Test]
        public void IsVisible_ReturnsTrueForProtectedConstructor()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, [typeof(object)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsVisible, Is.True);
        }

        [Test]
        public void IsVisible_ReturnsFalseForInternalConstructor()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, [typeof(string)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsVisible, Is.False);
        }

        [Test]
        public void IsSpecialName_ReturnsTrue()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, [typeof(int)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.IsSpecialName, Is.True);
        }

        [Test]
        public void CustomAttributes_ContainsExpectedAttribute()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, Type.EmptyTypes);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.CustomAttributes.Any(static a => a.Type.Name == "SetsRequiredMembersAttribute"), Is.True);
        }

        [Test]
        public void Overloads_ContainsOtherVisibleConstructorsOrderedByParameterCount()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, [typeof(int)])!;

            var metadata = constructorInfo.GetMetadata();

            // The internal constructor (string) is not visible, and the constructor itself is excluded.
            var overloadSignatures = metadata.Overloads.Cast<IConstructor>()
                .Select(static c => string.Join(",", c.Parameters.Select(static p => p.Type.Name)));
            Assert.That(overloadSignatures, Is.EqualTo(new[] { "", "Object", "String,Double" }));
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass))]
        public void Overloads_WithSingleConstructor_IsEmpty(Type declaringType)
        {
            var constructorInfo = declaringType.GetConstructor(Acme.Bindings.AllDeclared, Type.EmptyTypes)!;

            var metadata = constructorInfo.GetMetadata();

            Assert.That(metadata.Overloads, Is.Empty);
        }

        [Test]
        public void Overloads_ForStaticConstructor_IsEmpty()
        {
            var constructorInfo = typeof(Acme.SampleProperties).TypeInitializer!; // The type also has an instance constructor

            var metadata = constructorInfo.GetMetadata();

            Assert.That(metadata.Overloads, Is.Empty);
        }

        [TestCase(typeof(Uri), typeof(string), ExpectedResult = "M:System.Uri.#ctor(System.String)")]
        [TestCase(typeof(Acme.SampleConstructors), typeof(string), typeof(double), ExpectedResult = "M:Acme.SampleConstructors.#ctor(System.String,System.Double)")]
        public string CodeReference_HasExpectedValue(Type declaringType, params Type[] parameterTypes)
        {
            var constructorInfo = declaringType.GetConstructor(Acme.Bindings.AllDeclared, parameterTypes);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CodeReference;
        }
    }
}

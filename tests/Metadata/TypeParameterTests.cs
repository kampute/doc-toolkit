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
    public class TypeParameterTests
    {
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), 0, ExpectedResult = "T")]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), 1, ExpectedResult = "U")]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), 2, ExpectedResult = "V")]
        public string Name_ForGenericTypeParameter_HasExpectedValue(Type type, int parameterIndex)
        {
            var typeMetadata = type.GetMetadata() as IGenericCapableType;
            Assert.That(typeMetadata, Is.Not.Null);

            var typeParameter = typeMetadata.TypeParameters[parameterIndex];
            return typeParameter.Name;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints), 0, ExpectedResult = "T")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithoutTypeConstraints), 0, ExpectedResult = "T")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithoutTypeConstraints), 1, ExpectedResult = "U")]
        public string Name_ForGenericMethodParameter_HasExpectedValue(Type type, string methodName, int parameterIndex)
        {
            var methodInfo = type.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata() as IMethod;
            Assert.That(method, Is.Not.Null);

            var typeParameter = method.TypeParameters[parameterIndex];
            return typeParameter.Name;
        }

        [TestCase(typeof(Acme.SampleGenericClass<>))]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>))]
        [TestCase(typeof(Acme.SampleGenericStruct<>))]
        [TestCase(typeof(Acme.ISampleGenericInterface<>))]
        public void Position_ForGenericTypeParameter_HasExpectedValue(Type type)
        {
            var typeMetadata = type.GetMetadata() as IGenericCapableType;
            Assert.That(typeMetadata, Is.Not.Null);

            Assert.That(typeMetadata.TypeParameters.Select(tp => tp.Position), Is.EqualTo(Enumerable.Range(0, typeMetadata.TypeParameters.Count)));
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints))]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithoutTypeConstraints))]
        public void Position_ForGenericMethodParameter_HasExpectedValue(Type type, string methodName)
        {
            var methodInfo = type.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata() as IMethod;
            Assert.That(method, Is.Not.Null);

            Assert.That(method.TypeParameters.Select(tp => tp.Position), Is.EqualTo(Enumerable.Range(0, method.TypeParameters.Count)));
        }

        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), 0, ExpectedResult = TypeParameterVariance.Invariant)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), 1, ExpectedResult = TypeParameterVariance.Contravariant)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), 2, ExpectedResult = TypeParameterVariance.Covariant)]
        public TypeParameterVariance Variance_ForGenericTypeParameter_HasExpectedValue(Type type, int parameterIndex)
        {
            var typeMetadata = type.GetMetadata() as IGenericCapableType;
            Assert.That(typeMetadata, Is.Not.Null);

            var typeParameter = typeMetadata.TypeParameters[parameterIndex];
            return typeParameter.Variance;
        }

        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), 0,
            ExpectedResult = TypeParameterConstraints.ReferenceType)]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), 1,
            ExpectedResult = TypeParameterConstraints.NotNullableValueType | TypeParameterConstraints.DefaultConstructor)]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), 2,
            ExpectedResult = TypeParameterConstraints.None)]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>), 0,
            ExpectedResult = TypeParameterConstraints.ReferenceType)]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>), 1,
            ExpectedResult = TypeParameterConstraints.NotNullableValueType | TypeParameterConstraints.DefaultConstructor | TypeParameterConstraints.AllowByRefLike)]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>), 2,
            ExpectedResult = TypeParameterConstraints.AllowByRefLike)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), 0,
            ExpectedResult = TypeParameterConstraints.ReferenceType | TypeParameterConstraints.DefaultConstructor)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), 1,
            ExpectedResult = TypeParameterConstraints.None)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), 2,
            ExpectedResult = TypeParameterConstraints.None)]
        public TypeParameterConstraints Constraints_ForGenericTypeParameter_HasExpectedValue(Type type, int parameterIndex)
        {
            var typeMetadata = type.GetMetadata() as IGenericCapableType;
            Assert.That(typeMetadata, Is.Not.Null);

            var typeParameter = typeMetadata.TypeParameters[parameterIndex];
            return typeParameter.Constraints;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints), 0, ExpectedResult = TypeParameterConstraints.ReferenceType | TypeParameterConstraints.DefaultConstructor)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithoutTypeConstraints), 0, ExpectedResult = TypeParameterConstraints.ReferenceType)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithoutTypeConstraints), 1, ExpectedResult = TypeParameterConstraints.NotNullableValueType | TypeParameterConstraints.DefaultConstructor)]
        public TypeParameterConstraints Constraints_ForGenericMethodParameter_HasExpectedValue(Type type, string methodName, int parameterIndex)
        {
            var methodInfo = type.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata() as IMethod;
            Assert.That(method, Is.Not.Null);

            var typeParameter = method.TypeParameters[parameterIndex];
            return typeParameter.Constraints;
        }

        [TestCase(typeof(Acme.SampleGenericClass<>), 0)]
        [TestCase(typeof(Acme.SampleGenericStruct<>), 0, nameof(IDisposable))]
        public void TypeConstraints_ForGenericTypeParameter_HasExpectedTypes(Type type, int parameterIndex, params string[] expectedTypeNames)
        {
            var typeMetadata = type.GetMetadata<IGenericCapableType>();
            Assert.That(typeMetadata, Is.Not.Null);

            var typeParameter = typeMetadata.TypeParameters[parameterIndex];
            Assert.That(typeParameter.TypeConstraints.Select(t => t.Name), Is.EquivalentTo(expectedTypeNames));
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithoutTypeConstraints), 0)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints), 0, nameof(ICloneable))]
        public void TypeConstraints_ForGenericMethodParameter_HasExpectedTypes(Type type, string methodName, int parameterIndex, params string[] expectedTypeNames)
        {
            var methodInfo = type.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata() as IMethod;
            Assert.That(method, Is.Not.Null);

            var typeParameter = method.TypeParameters[parameterIndex];
            Assert.That(typeParameter.TypeConstraints.Select(t => t.Name), Is.EquivalentTo(expectedTypeNames));
        }

        [Test]
        public void DeclaringMember_ForGenericTypeParameter_ReturnsDeclaringType()
        {
            var typeMetadata = typeof(Acme.SampleGenericClass<>).GetMetadata<IClassType>();
            Assert.That(typeMetadata, Is.Not.Null);

            var typeParameter = typeMetadata.TypeParameters[0];
            Assert.That(typeParameter.DeclaringMember, Is.SameAs(typeMetadata));
        }

        [Test]
        public void DeclaringMember_ForGenericMethodParameter_ReturnsDeclaringMethod()
        {
            var methodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata() as IMethod;
            Assert.That(method, Is.Not.Null);

            var typeParameter = method.TypeParameters[0];
            Assert.That(typeParameter.DeclaringMember, Is.SameAs(method));
        }

        // where T: class
        [TestCase(typeof(Acme.SampleGenericClass<>), 0, typeof(object), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericClass<>), 0, typeof(int), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleGenericClass<>), 0, typeof(ReadOnlySpan<int>), ExpectedResult = false)]
        // where U: struct
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), 1, typeof(int), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), 1, typeof(ReadOnlySpan<int>), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), 1, typeof(string), ExpectedResult = false)]
        // where T: struct, IDisposable
        [TestCase(typeof(Acme.SampleGenericStruct<>), 0, typeof(System.IO.Stream), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericStruct<>), 0, typeof(object), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleGenericStruct<>), 0, typeof(int), ExpectedResult = false)]
        // where U: struct, allows ref struct
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>), 1, typeof(int), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>), 1, typeof(ReadOnlySpan<int>), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>), 1, typeof(string), ExpectedResult = false)]
        // where V: allows ref struct
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>), 2, typeof(int), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>), 2, typeof(ReadOnlySpan<int>), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>), 2, typeof(string), ExpectedResult = true)]
        // where T: class, new()
        [TestCase(typeof(Acme.ISampleGenericInterface<>), 0, typeof(object), ExpectedResult = true)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>), 0, typeof(string), ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>), 0, typeof(int), ExpectedResult = false)]
        // where U has no constraints
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), 1, typeof(int), ExpectedResult = true)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), 1, typeof(ReadOnlySpan<int>), ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), 1, typeof(string), ExpectedResult = true)]
        public bool IsSatisfiableBy_ChecksConstraintSatisfaction(Type declaringType, int parameterIndex, Type candidateType)
        {
            var declaringTypeMetadata = declaringType.GetMetadata<IGenericCapableType>();
            var typeParameter = declaringTypeMetadata.TypeParameters[parameterIndex];
            var candidateMetadata = candidateType.GetMetadata();

            return typeParameter.IsSatisfiableBy(candidateMetadata);
        }
    }
}
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
    public class TypeParameterTests
    {
        private readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        [TestCase(typeof(Acme.MyList<>), 0, ExpectedResult = "T")]
        [TestCase(typeof(Acme.MyList<>.Helper<,>), 0, ExpectedResult = "T")]
        [TestCase(typeof(Acme.MyList<>.Helper<,>), 1, ExpectedResult = "U")]
        public string Name_HasExpectedValue(Type type, int parameterIndex)
        {
            var typeMetadata = type.GetMetadata() as IGenericCapableType;
            Assert.That(typeMetadata, Is.Not.Null);

            var typeParameter = typeMetadata.TypeParameters[parameterIndex];
            return typeParameter.Name;
        }

        [TestCase(typeof(Acme.MyList<>), 0, ExpectedResult = 0)]
        [TestCase(typeof(Acme.MyList<>.Helper<,>), 0, ExpectedResult = 0)]
        [TestCase(typeof(Acme.MyList<>.Helper<,>), 1, ExpectedResult = 1)]
        [TestCase(typeof(Acme.MultipleConstraintClass<,>), 0, ExpectedResult = 0)]
        [TestCase(typeof(Acme.MultipleConstraintClass<,>), 1, ExpectedResult = 1)]
        public int Position_HasExpectedValue(Type type, int parameterIndex)
        {
            var typeMetadata = type.GetMetadata() as IGenericCapableType;
            Assert.That(typeMetadata, Is.Not.Null);

            var typeParameter = typeMetadata.TypeParameters[parameterIndex];
            return typeParameter.Position;
        }

        [TestCase(typeof(Acme.IProcess<>), 0, ExpectedResult = TypeParameterVariance.Covariant)]
        [TestCase(typeof(Acme.Widget.IMenuItem<>), 0, ExpectedResult = TypeParameterVariance.Contravariant)]
        [TestCase(typeof(Acme.MyList<>), 0, ExpectedResult = TypeParameterVariance.Invariant)]
        public TypeParameterVariance Variance_HasExpectedValue(Type type, int parameterIndex)
        {
            var typeMetadata = type.GetMetadata() as IGenericCapableType;
            Assert.That(typeMetadata, Is.Not.Null);

            var typeParameter = typeMetadata.TypeParameters[parameterIndex];
            return typeParameter.Variance;
        }

        [TestCase(typeof(Acme.MyList<>), 0, ExpectedResult = TypeParameterConstraints.ValueType | TypeParameterConstraints.DefaultConstructor)]
        [TestCase(typeof(Acme.MultipleConstraintClass<,>), 0, ExpectedResult = TypeParameterConstraints.ReferenceType)]
        [TestCase(typeof(Acme.MultipleConstraintClass<,>), 1, ExpectedResult = TypeParameterConstraints.ReferenceType | TypeParameterConstraints.DefaultConstructor)]
        // [TestCase(typeof(Acme.UnmanagedConstraintClass<>), 0, ExpectedResult = TypeParameterConstraints.ValueType | TypeParameterConstraints.DefaultConstructor | TypeParameterConstraints.UnmanagedType)]
        // [TestCase(typeof(Acme.NotNullConstraintClass<>), 0, ExpectedResult = TypeParameterConstraints.NotNull)]
        public TypeParameterConstraints Constraints_HasExpectedValue(Type type, int parameterIndex)
        {
            var typeMetadata = type.GetMetadata() as IGenericCapableType;
            Assert.That(typeMetadata, Is.Not.Null);

            var typeParameter = typeMetadata.TypeParameters[parameterIndex];
            return typeParameter.Constraints;
        }

        [Test]
        public void DeclaringMember_ForTypeParameter_ReturnsDeclaringType()
        {
            var typeMetadata = typeof(Acme.MyList<>).GetMetadata() as IGenericCapableType;
            Assert.That(typeMetadata, Is.Not.Null);

            var typeParameter = typeMetadata.TypeParameters[0];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(typeParameter.DeclaringMember, Is.Not.Null);
                Assert.That(typeParameter.DeclaringMember, Is.SameAs(typeMetadata));
            }
        }

        [Test]
        public void DeclaringMember_ForMethodParameter_ReturnsDeclaringMethod()
        {
            var methodInfo = typeof(Acme.Widget).GetMethod("M8", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata() as IMethod;
            Assert.That(method, Is.Not.Null);

            var typeParameter = method.TypeParameters[0];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(typeParameter.DeclaringMember, Is.Not.Null);
                Assert.That(typeParameter.DeclaringMember, Is.SameAs(method));
            }
        }

        [Test]
        public void TypeConstraints_WithNoTypeConstraints_ReturnsEmptyCollection()
        {
            var methodInfo = typeof(Acme.Widget).GetMethod("M7", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata() as IMethod;
            Assert.That(method, Is.Not.Null);

            var typeParameter = method.TypeParameters[0];
            Assert.That(typeParameter.TypeConstraints, Is.Empty);
        }

        [Test]
        public void TypeConstraints_WithTypeConstraints_ReturnsExpectedTypes()
        {
            var typeMetadata = typeof(Acme.MultipleConstraintClass<,>).GetMetadata() as IGenericCapableType;
            Assert.That(typeMetadata, Is.Not.Null);

            var typeParameter = typeMetadata.TypeParameters[0];
            Assert.That(typeParameter.TypeConstraints.Select(t => t.Name), Is.EqualTo(["ICloneable"]));
        }

        [Test]
        public void TypeConstraints_WithWidgetConstraint_ReturnsWidgetType()
        {
            var typeMetadata = typeof(Acme.Widget.IMenuItem<>).GetMetadata() as IGenericCapableType;
            Assert.That(typeMetadata, Is.Not.Null);

            var typeParameter = typeMetadata.TypeParameters[0];
            Assert.That(typeParameter.TypeConstraints.Select(t => t.Name), Is.EqualTo(["Widget"]));
        }

        [TestCase(typeof(Acme.Widget), "M8", 0, ExpectedResult = "T")]
        [TestCase(typeof(Acme.UseList), "GetValues", 0, ExpectedResult = "T")]
        public string MethodTypeParameter_Name_HasExpectedValue(Type type, string methodName, int parameterIndex)
        {
            var methodInfo = type.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata() as IMethod;
            Assert.That(method, Is.Not.Null);

            var typeParameter = method.TypeParameters[parameterIndex];
            return typeParameter.Name;
        }

        [TestCase(typeof(Acme.Widget), "M8", 0, ExpectedResult = TypeParameterConstraints.ReferenceType | TypeParameterConstraints.DefaultConstructor)]
        [TestCase(typeof(Acme.UseList), "GetValues", 0, ExpectedResult = TypeParameterConstraints.ValueType | TypeParameterConstraints.DefaultConstructor)]
        public TypeParameterConstraints MethodTypeParameter_Constraints_HasExpectedValue(Type type, string methodName, int parameterIndex)
        {
            var methodInfo = type.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata() as IMethod;
            Assert.That(method, Is.Not.Null);

            var typeParameter = method.TypeParameters[parameterIndex];
            return typeParameter.Constraints;
        }

        [TestCase(typeof(Acme.MyList<>), 0, typeof(int), ExpectedResult = true)]
        [TestCase(typeof(Acme.MyList<>), 0, typeof(string), ExpectedResult = false)]
        [TestCase(typeof(Acme.MultipleConstraintClass<,>), 0, typeof(string), ExpectedResult = true)]
        [TestCase(typeof(Acme.MultipleConstraintClass<,>), 0, typeof(Acme.Widget), ExpectedResult = false)]
        [TestCase(typeof(Acme.Widget.IMenuItem<>), 0, typeof(Acme.Widget), ExpectedResult = false)]
        [TestCase(typeof(Acme.Widget.IMenuItem<>), 0, typeof(Acme.DerivedWidget), ExpectedResult = true)]
        public bool IsSubstitutableBy_ChecksConstraintSatisfaction(Type declaringType, int parameterIndex, Type candidateType)
        {
            var declaringTypeMetadata = declaringType.GetMetadata<IGenericCapableType>();
            var typeParameter = declaringTypeMetadata.TypeParameters[parameterIndex];
            var candidateMetadata = candidateType.GetMetadata();

            return typeParameter.IsSubstitutableBy(candidateMetadata);
        }
    }
}
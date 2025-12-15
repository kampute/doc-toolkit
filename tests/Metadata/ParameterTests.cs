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
    public class ParameterTests
    {
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod))]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RefParamsMethod), "i", "s", "d")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.OptionalParamsMethod), "i", "s")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.ArrayParamsMethod), "args")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), "s", "day", "values")]
        public void Name_HasExpectedValue(Type declaringType, string methodName, params string[] expectedNames)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            Assert.That(method.Parameters.Select(p => p.Name), Is.EqualTo(expectedNames));
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RefParamsMethod))]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.OptionalParamsMethod))]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.ArrayParamsMethod))]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod))]
        public void Position_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            Assert.That(method.Parameters.Select(p => p.Position), Is.EqualTo(Enumerable.Range(0, method.Parameters.Count)));
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RefParamsMethod), 0, ExpectedResult = ParameterReferenceKind.In)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RefParamsMethod), 1, ExpectedResult = ParameterReferenceKind.Ref)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RefParamsMethod), 2, ExpectedResult = ParameterReferenceKind.Out)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 0, ExpectedResult = ParameterReferenceKind.None)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 1, ExpectedResult = ParameterReferenceKind.In)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 2, ExpectedResult = ParameterReferenceKind.None)]
        public ParameterReferenceKind ReferenceKind_HasExpectedValue(Type declaringType, string methodName, int parameterIndex)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.ReferenceKind;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RefParamsMethod), 0, ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RefParamsMethod), 1, ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RefParamsMethod), 2, ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 0, ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 1, ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 2, ExpectedResult = false)]
        public bool IsByRef_HasExpectedValue(Type declaringType, string methodName, int parameterIndex)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.IsByRef;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.OptionalParamsMethod), 0, ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.OptionalParamsMethod), 1, ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 0, ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 1, ExpectedResult = true)]
        public bool IsOptional_HasExpectedValue(Type declaringType, string methodName, int parameterIndex)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.IsOptional;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.ArrayParamsMethod), 0, ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 2, ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RefParamsMethod), 0, ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 0, ExpectedResult = false)]
        public bool IsParams_HasExpectedValue(Type declaringType, string methodName, int parameterIndex)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.IsParameterArray;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.OptionalParamsMethod), 0, ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.OptionalParamsMethod), 1, ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 0, ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 1, ExpectedResult = true)]
        public bool HasDefaultValue_HasExpectedValue(Type declaringType, string methodName, int parameterIndex)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.HasDefaultValue;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 0)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 2)]
        public void DefaultValue_WithoutDefault_ReturnsDBNull(Type declaringType, string methodName, int parameterIndex)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            Assert.That(parameter.DefaultValue, Is.EqualTo(DBNull.Value));
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.OptionalParamsMethod), 0, ExpectedResult = 42)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.OptionalParamsMethod), 1, ExpectedResult = "default")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 1, ExpectedResult = (int)DayOfWeek.Monday)]
        public object? DefaultValue_WithDefault_ReturnsDefaultValue(Type declaringType, string methodName, int parameterIndex)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var Parameter = method.Parameters[parameterIndex];
            return Parameter.DefaultValue;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.OptionalParamsMethod), 0, ExpectedResult = nameof(Int32))]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.OptionalParamsMethod), 1, ExpectedResult = nameof(String))]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), 0, ExpectedResult = nameof(String))]
        public string ParameterType_HasExpectedName(Type declaringType, string methodName, int parameterIndex)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.Type.Name;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints), 0, typeof(System.Diagnostics.CodeAnalysis.NotNullAttribute))]
        public void CustomAttributes_ReturnsExpectedAttributes(Type declaringType, string methodName, int parameterIndex, Type expectedAttribute)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            Assert.That(parameter.CustomAttributes.Any(a => a.Type.Represents(expectedAttribute)), Is.True);
        }

        [Test]
        public void IsSatisfiableBy_ReturnsTrueForSameParameter()
        {
            var methodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.RefParamsMethod), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[0];
            Assert.That(parameter.IsSatisfiableBy(parameter), Is.True);
        }

        [Test]
        public void IsSatisfiableBy_ReturnsTrueForMatchingParameters()
        {
            var methodName = nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.GenericMethod);

            var baseMethodInfo = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass).GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(baseMethodInfo, Is.Not.Null);

            var derivedMethodInfo = typeof(Acme.SampleDerivedGenericClass<,,>).GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(derivedMethodInfo, Is.Not.Null);

            var baseMethod = baseMethodInfo.GetMetadata();
            var derivedMethod = derivedMethodInfo.GetMetadata();

            for (var i = 0; i < baseMethod.Parameters.Count; i++)
            {
                var baseParam = baseMethod.Parameters[i];
                var derivedParam = derivedMethod.Parameters[i];

                Assert.That(baseParam.IsSatisfiableBy(derivedParam), Is.True);
            }
        }

        [Test]
        public void IsSatisfiableBy_ReturnsFalseForDifferentParameterDirections()
        {
            var methodName = nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.GenericMethod);

            var baseMethodInfo = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass).GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(baseMethodInfo, Is.Not.Null);

            var derivedMethodInfo = typeof(Acme.SampleDerivedGenericClass<,,>).GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(derivedMethodInfo, Is.Not.Null);

            var baseMethod = baseMethodInfo.GetMetadata();
            var derivedMethod = derivedMethodInfo.GetMetadata();

            for (var i = 0; i < baseMethod.Parameters.Count; i++)
            {
                var baseParam = baseMethod.Parameters[i];
                var derivedParam = derivedMethod.Parameters[i];

                Assert.That(derivedParam.IsSatisfiableBy(baseParam), Is.False);
            }
        }

        [Test]
        public void IsSatisfiableBy_ReturnsFalseForParametersWithDifferentTypes()
        {
            var firstMethodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.OptionalParamsMethod), Acme.Bindings.AllDeclared);
            Assert.That(firstMethodInfo, Is.Not.Null);

            var secondMethodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.UnsafeMethod), Acme.Bindings.AllDeclared);
            Assert.That(secondMethodInfo, Is.Not.Null);

            var firstMethod = firstMethodInfo.GetMetadata();
            var secondMethod = secondMethodInfo.GetMetadata();

            var firstMethodParam = firstMethod.Parameters[0]; // int
            var secondMethodParam = secondMethod.Parameters[0]; // int**

            Assert.That(firstMethodParam.IsSatisfiableBy(secondMethodParam), Is.False);
        }

        [Test]
        public void IsSatisfiableBy_ReturnsFalseForParametersWithDifferentModifiers()
        {
            var firstMethodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.OptionalParamsMethod), Acme.Bindings.AllDeclared);
            Assert.That(firstMethodInfo, Is.Not.Null);

            var secondMethodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.RefParamsMethod), Acme.Bindings.AllDeclared);
            Assert.That(secondMethodInfo, Is.Not.Null);

            var firstMethod = firstMethodInfo.GetMetadata();
            var secondMethod = secondMethodInfo.GetMetadata();

            var firstMethodParam = firstMethod.Parameters[0]; // int i
            var secondMethodParam = secondMethod.Parameters[0]; // in int i

            Assert.That(firstMethodParam.IsSatisfiableBy(secondMethodParam), Is.False);
        }

        [Test]
        public void IsSatisfiableBy_ReturnsFalseForParametersWithDifferentPositions()
        {
            var firstMethodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.OptionalParamsMethod), Acme.Bindings.AllDeclared);
            Assert.That(firstMethodInfo, Is.Not.Null);

            var secondMethodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.OverloadedMethod), Acme.Bindings.AllDeclared, [typeof(string), typeof(int)]);
            Assert.That(secondMethodInfo, Is.Not.Null);

            var firstMethod = firstMethodInfo.GetMetadata();
            var secondMethod = secondMethodInfo.GetMetadata();

            var firstMethodParam = firstMethod.Parameters[0]; // int i
            var secondMethodParam = secondMethod.Parameters[1]; // int i

            Assert.That(firstMethodParam.IsSatisfiableBy(secondMethodParam), Is.False);
        }

        [Test]
        public void IsSatisfiableBy_ReturnsTrueForOpenAndClosedMethodParameters()
        {
            var methodName = nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod);

            var baseMethodInfo = typeof(Acme.SampleDerivedGenericClass<,,>).GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(baseMethodInfo, Is.Not.Null);

            var derivedMethodInfo = typeof(Acme.SampleDerivedConstructedGenericClass).GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(derivedMethodInfo, Is.Not.Null);

            var baseMethod = baseMethodInfo.GetMetadata();
            var derivedMethod = derivedMethodInfo.GetMetadata();

            for (var i = 0; i < baseMethod.Parameters.Count; i++)
            {
                var baseParam = baseMethod.Parameters[i];
                var derivedParam = derivedMethod.Parameters[i];

                Assert.That(derivedParam.IsSatisfiableBy(baseParam), Is.False);
            }
        }

        [Test]
        public void ReturnParameter_HasExpectedProperties()
        {
            var methodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(method.Return.Member, Is.SameAs(method));
                Assert.That(method.Return.Name, Is.Empty);
                Assert.That(method.Return.Position, Is.EqualTo(-1));
                Assert.That(method.Return.IsByRef, Is.False);
                Assert.That(method.Return.IsOptional, Is.False);
                Assert.That(method.Return.IsParameterArray, Is.False);
                Assert.That(method.Return.HasDefaultValue, Is.False);
                Assert.That(method.Return.DefaultValue, Is.EqualTo(DBNull.Value));
                Assert.That(method.Return.Type.Name, Is.EqualTo("T"));
                Assert.That(method.Return.CustomAttributes.Select(a => a.Type.Name), Does.Contain("NotNullAttribute"));
            }
        }
    }
}
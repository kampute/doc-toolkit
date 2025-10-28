// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.Reflection;

    [TestFixture]
    public class ParameterTests
    {
        private readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        [TestCase("M1", 0, ExpectedResult = "c")]
        [TestCase("M1", 1, ExpectedResult = "f")]
        [TestCase("M1", 2, ExpectedResult = "v")]
        [TestCase("M1", 3, ExpectedResult = "i")]
        public string Parameter_Name_HasExpectedValue(string methodName, int parameterIndex)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.Name;
        }

        [TestCase("M1", 0, ExpectedResult = 0)]
        [TestCase("M1", 1, ExpectedResult = 1)]
        [TestCase("M1", 2, ExpectedResult = 2)]
        [TestCase("M1", 3, ExpectedResult = 3)]
        public int Parameter_Position_HasExpectedValue(string methodName, int parameterIndex)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.Position;
        }

        [TestCase("M1", 0, ExpectedResult = ParameterReferenceKind.None)]
        [TestCase("M1", 1, ExpectedResult = ParameterReferenceKind.Out)]
        [TestCase("M1", 2, ExpectedResult = ParameterReferenceKind.Ref)]
        [TestCase("M1", 3, ExpectedResult = ParameterReferenceKind.In)]
        public ParameterReferenceKind Parameter_ReferenceKind_HasExpectedValue(string methodName, int parameterIndex)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.ReferenceKind;
        }

        [TestCase("M1", 0, ExpectedResult = false)]
        [TestCase("M1", 1, ExpectedResult = true)]
        [TestCase("M1", 2, ExpectedResult = true)]
        [TestCase("M1", 3, ExpectedResult = true)]
        public bool Parameter_IsByRef_HasExpectedValue(string methodName, int parameterIndex)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.IsByRef;
        }

        [TestCase("M1", 0, ExpectedResult = false)]
        [TestCase("M4", 1, ExpectedResult = true)]
        [TestCase("M8", 1, ExpectedResult = true)]
        public bool Parameter_IsOptional_HasExpectedValue(string methodName, int parameterIndex)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.IsOptional;
        }

        [TestCase("M2", 0, ExpectedResult = false)]
        [TestCase("M6", 1, ExpectedResult = true)]
        public bool Parameter_IsParams_HasExpectedValue(string methodName, int parameterIndex)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.IsParameterArray;
        }

        [TestCase("M1", 0, ExpectedResult = false)]
        [TestCase("M4", 1, ExpectedResult = true)]
        [TestCase("M8", 1, ExpectedResult = true)]
        public bool Parameter_HasDefaultValue_HasExpectedValue(string methodName, int parameterIndex)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.HasDefaultValue;
        }

        [TestCase("M1", 3)]
        [TestCase("M6", 1)]
        public void Parameter_DefaultValue_WithoutDefault_ReturnsDBNull(string methodName, int parameterIndex)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            Assert.That(parameter.DefaultValue, Is.EqualTo(DBNull.Value));
        }

        [TestCase("M4", 1, ExpectedResult = null)]
        [TestCase("M8", 1, ExpectedResult = true)]
        public object? Parameter_DefaultValue_WithDefault_ReturnsDefaultValue(string methodName, int parameterIndex)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var Parameter = method.Parameters[parameterIndex];
            return Parameter.DefaultValue;
        }

        [TestCase("M1", 0, ExpectedResult = "Char")]
        [TestCase("M1", 1, ExpectedResult = "Single&")]
        [TestCase("M1", 2, ExpectedResult = "ValueType&")]
        [TestCase("M1", 3, ExpectedResult = "Int32&")]
        [TestCase("M6", 0, ExpectedResult = "Int32")]
        [TestCase("M6", 1, ExpectedResult = "Object[]")]
        public string Parameter_ParameterType_HasExpectedName(string methodName, int parameterIndex)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[parameterIndex];
            return parameter.Type.Name;
        }

        [Test]
        public void Parameter_CustomAttributes_ReturnsExpectedAttributes()
        {
            var methodInfo = typeof(Acme.MyList<int>).GetMethod("Test", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[0];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(parameter.CustomAttributes, Is.Not.Empty);
                Assert.That(parameter.HasCustomAttribute("Acme.CustomAttribute"), Is.True);
            }
        }

        [Test]
        public void Parameter_IsSubstitutableBy_ReturnsTrueForSameParameter()
        {
            var methodInfo = typeof(Acme.Widget).GetMethod("M8", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var method = methodInfo.GetMetadata();
            Assert.That(method, Is.Not.Null);

            var parameter = method.Parameters[0];
            Assert.That(parameter.IsSatisfiableBy(parameter), Is.True);
        }

        [Test]
        public void Parameter_IsSubstitutableBy_ReturnsTrueForMatchingParameters()
        {
            var widgetMethodInfo = typeof(Acme.Widget).GetMethod("M6", bindingFlags);
            Assert.That(widgetMethodInfo, Is.Not.Null);

            var valueTypeMethodInfo = typeof(Acme.ValueType).GetMethod("M1", bindingFlags);
            Assert.That(valueTypeMethodInfo, Is.Not.Null);

            var widgetMethod = widgetMethodInfo.GetMetadata();
            var valueTypeMethod = valueTypeMethodInfo.GetMetadata();

            var widgetParam = widgetMethod.Parameters[0];
            var valueTypeParam = valueTypeMethod.Parameters[0];

            // These should match as they are both "int" parameters without any modifiers
            Assert.That(widgetParam.IsSatisfiableBy(valueTypeParam), Is.True);
        }

        [Test]
        public void Parameter_IsSubstitutableBy_ReturnsFalseForDifferentParameterDirections()
        {
            var widgetMethodInfo = typeof(Acme.Widget).GetMethod("M1", bindingFlags);
            Assert.That(widgetMethodInfo, Is.Not.Null);

            var valueTypeMethodInfo = typeof(Acme.ValueType).GetMethod("M1", bindingFlags);
            Assert.That(valueTypeMethodInfo, Is.Not.Null);

            var widgetMethod = widgetMethodInfo.GetMetadata();
            var valueTypeMethod = valueTypeMethodInfo.GetMetadata();

            var widgetParam = widgetMethod.Parameters[3];
            var valueTypeParam = valueTypeMethod.Parameters[0];

            // These should not match as one is input "int" parameter and the other is a regular "int" parameter
            Assert.That(widgetParam.IsSatisfiableBy(valueTypeParam), Is.False);
        }

        [Test]
        public void Parameter_IsSubstitutableBy_ReturnsFalseForDifferentParameterTypes()
        {
            var widgetMethodInfo = typeof(Acme.Widget).GetMethod("M1", bindingFlags);
            Assert.That(widgetMethodInfo, Is.Not.Null);

            var valueTypeMethodInfo = typeof(Acme.ValueType).GetMethod("M2", bindingFlags);
            Assert.That(valueTypeMethodInfo, Is.Not.Null);

            var widgetMethod = widgetMethodInfo.GetMetadata();
            var valueTypeMethod = valueTypeMethodInfo.GetMetadata();

            var widgetParam = widgetMethod.Parameters[0];
            var valueTypeParam = valueTypeMethod.Parameters[0];

            // These should not match as one is "int" parameter and the other is a nullable "int" parameter
            Assert.That(widgetParam.IsSatisfiableBy(valueTypeParam), Is.False);
        }

        [Test]
        public void Parameter_IsSubstitutableBy_ReturnsTrueForOpenAndClosedMethodParameters()
        {
            var openMethodInfo = typeof(Acme.UseList).GetMethod("GetValues", bindingFlags);
            Assert.That(openMethodInfo, Is.Not.Null);

            var closedMethodInfo = openMethodInfo.MakeGenericMethod(typeof(int));
            Assert.That(closedMethodInfo, Is.Not.Null);

            var openMethod = openMethodInfo.GetMetadata();
            var closedMethod = closedMethodInfo.GetMetadata();

            var openParam = openMethod.Parameters[0];
            var closedParam = closedMethod.Parameters[0];

            // open generic parameter T should be substitutable by concrete int parameter
            Assert.That(openParam.IsSatisfiableBy(closedParam), Is.True);
        }
    }
}
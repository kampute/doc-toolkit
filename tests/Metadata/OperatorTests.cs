// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class OperatorTests
    {
        private static string GetMethodName(string operatorName) => operatorName.Insert(operatorName.LastIndexOf('.') + 1, "op_");

        [TestCase(typeof(Acme.SampleOperators), "UnaryPlus")]
        [TestCase(typeof(Acme.SampleOperators), "UnaryNegation")]
        [TestCase(typeof(Acme.SampleOperators), "Addition")]
        [TestCase(typeof(Acme.SampleOperators), "Subtraction")]
        [TestCase(typeof(Acme.SampleOperators), "AdditionAssignment")]
        [TestCase(typeof(Acme.SampleOperators), "SubtractionAssignment")]
        [TestCase(typeof(Acme.SampleOperators), "IncrementAssignment")]
        [TestCase(typeof(Acme.SampleOperators), "DecrementAssignment")]
        [TestCase(typeof(Acme.SampleOperators), "Implicit")]
        [TestCase(typeof(Acme.SampleOperators), "Explicit")]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False")]
        [TestCase(typeof(Acme.ISampleInterface), "False")]
        public void ImplementsOperator(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IOperator>());
            Assert.That(metadata.Name, Is.EqualTo(operatorName));
        }

        [TestCase(typeof(Acme.SampleOperators), "UnaryPlus", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "Addition", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "AdditionAssignment", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "IncrementAssignment", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "Implicit", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False", ExpectedResult = true)]
        [TestCase(typeof(Acme.ISampleInterface), "False", ExpectedResult = true)]
        public bool IsStatic_HasExpectedValue(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsStatic;
        }

        [TestCase(typeof(Acme.SampleOperators), "UnaryNegation", ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleOperators), "Subtraction", ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleOperators), "SubtractionAssignment", ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleOperators), "DecrementAssignment", ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleOperators), "Explicit", ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False", ExpectedResult = MemberVisibility.Private)]
        [TestCase(typeof(Acme.ISampleInterface), "False", ExpectedResult = MemberVisibility.Public)]
        public MemberVisibility Visibility_HasExpectedValue(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Visibility;
        }


        [TestCase(typeof(Acme.SampleOperators), "UnaryPlus", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "Addition", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "AdditionAssignment", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "IncrementAssignment", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "Implicit", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False", ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleInterface), "False", ExpectedResult = true)]
        public bool IsVisible_HasExpectedValue(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsVisible;
        }

        [TestCase(typeof(Acme.SampleOperators), "UnaryNegation", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "Subtraction", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "SubtractionAssignment", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "DecrementAssignment", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "Explicit", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False", ExpectedResult = false)] // Should be true like other operators?
        [TestCase(typeof(Acme.ISampleInterface), "False", ExpectedResult = true)]
        public bool IsSpecialName_HasExpectedValue(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsSpecialName;
        }

        [TestCase(typeof(Acme.SampleOperators), "UnaryPlus", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "Addition", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "AdditionAssignment", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "IncrementAssignment", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "Implicit", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False", ExpectedResult = true)]
        [TestCase(typeof(Acme.ISampleInterface), "False", ExpectedResult = false)]
        public bool IsExplicitInterfaceImplementation_HasExpectedValue(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsExplicitInterfaceImplementation;
        }

        [TestCase(typeof(Acme.SampleOperators), "UnaryNegation", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "Subtraction", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "SubtractionAssignment", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "DecrementAssignment", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "Explicit", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False", ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleInterface), "True", ExpectedResult = true)]
        [TestCase(typeof(Acme.ISampleInterface), "False", ExpectedResult = false)]
        public bool IsDefaultInterfaceImplementation_HasExpectedValue(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsDefaultInterfaceImplementation;
        }

        [TestCase(typeof(Acme.SampleOperators), "UnaryPlus", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "UnaryNegation", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "Addition", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "Subtraction", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "AdditionAssignment", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "SubtractionAssignment", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "IncrementAssignment", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "DecrementAssignment", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleOperators), "Implicit", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "Explicit", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False", ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleInterface), "True", ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleInterface), "False", ExpectedResult = false)]
        public bool IsConversionOperator_HasExpectedValue(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsConversionOperator;
        }

        [TestCase(typeof(Acme.SampleOperators), "UnaryPlus")]
        [TestCase(typeof(Acme.SampleOperators), "UnaryNegation")]
        [TestCase(typeof(Acme.SampleOperators), "Addition")]
        [TestCase(typeof(Acme.SampleOperators), "Subtraction")]
        [TestCase(typeof(Acme.SampleOperators), "AdditionAssignment")]
        [TestCase(typeof(Acme.SampleOperators), "SubtractionAssignment")]
        [TestCase(typeof(Acme.SampleOperators), "IncrementAssignment")]
        [TestCase(typeof(Acme.SampleOperators), "DecrementAssignment")]
        [TestCase(typeof(Acme.SampleOperators), "Implicit")]
        [TestCase(typeof(Acme.SampleOperators), "Explicit")]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False")]
        [TestCase(typeof(Acme.ISampleInterface), "True")]
        [TestCase(typeof(Acme.ISampleInterface), "False")]
        public void DeclaringType_HasExpectedType(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.DeclaringType.Represents(declaringType), Is.True);
        }

        [TestCase(typeof(Acme.SampleOperators), "UnaryNegation", ExpectedResult = MemberVirtuality.None)]
        [TestCase(typeof(Acme.SampleOperators), "Subtraction", ExpectedResult = MemberVirtuality.None)]
        [TestCase(typeof(Acme.SampleOperators), "SubtractionAssignment", ExpectedResult = MemberVirtuality.None)]
        [TestCase(typeof(Acme.SampleOperators), "DecrementAssignment", ExpectedResult = MemberVirtuality.None)]
        [TestCase(typeof(Acme.SampleOperators), "Explicit", ExpectedResult = MemberVirtuality.None)]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False", ExpectedResult = MemberVirtuality.None)]
        [TestCase(typeof(Acme.ISampleInterface), "True", ExpectedResult = MemberVirtuality.Virtual)]
        [TestCase(typeof(Acme.ISampleInterface), "False", ExpectedResult = MemberVirtuality.Abstract)]
        public MemberVirtuality Virtuality_HasExpectedValue(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Virtuality;
        }

        [TestCase(typeof(Acme.ISampleInterface), "False", null)]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False", null)]
        public void OverriddenOperator_HasExpectedValue(Type declaringType, string operatorName, Type? expectedBaseType)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.OverriddenOperator?.DeclaringType, Is.EqualTo(expectedBaseType?.GetMetadata()));
        }

        [TestCase(typeof(Acme.ISampleInterface), "False", null)]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False", typeof(Acme.ISampleInterface))]
        public void InterfaceOperator_HasExpectedValue(Type declaringType, string operatorName, Type? expectedInterfaceType)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.ImplementedOperator?.DeclaringType, Is.EqualTo(expectedInterfaceType?.GetMetadata()));
        }

        [TestCase(typeof(Acme.SampleOperators), "UnaryPlus", ExpectedResult = 1)]
        [TestCase(typeof(Acme.SampleOperators), "Addition", ExpectedResult = 2)]
        [TestCase(typeof(Acme.SampleOperators), "AdditionAssignment", ExpectedResult = 1)]
        [TestCase(typeof(Acme.SampleOperators), "IncrementAssignment", ExpectedResult = 0)]
        [TestCase(typeof(Acme.SampleOperators), "Implicit", ExpectedResult = 1)]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False", ExpectedResult = 1)]
        [TestCase(typeof(Acme.ISampleInterface), "False", ExpectedResult = 1)]
        public int Parameters_ReturnsExpectedCount(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Parameters.Count;
        }

        [TestCase(typeof(Acme.SampleOperators), "UnaryNegation", ExpectedResult = nameof(Acme.SampleOperators))]
        [TestCase(typeof(Acme.SampleOperators), "Subtraction", ExpectedResult = nameof(Acme.SampleOperators))]
        [TestCase(typeof(Acme.SampleOperators), "SubtractionAssignment", ExpectedResult = "Void")]
        [TestCase(typeof(Acme.SampleOperators), "DecrementAssignment", ExpectedResult = "Void")]
        [TestCase(typeof(Acme.SampleOperators), "Explicit", ExpectedResult = nameof(Int32))]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False", ExpectedResult = nameof(Boolean))]
        [TestCase(typeof(Acme.ISampleInterface), "False", ExpectedResult = nameof(Boolean))]
        public string ReturnParameter_ReturnsCorrectType(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Return.Type.Name;
        }

        [TestCase(typeof(Acme.SampleOperators), "UnaryPlus", ExpectedResult = "M:Acme.SampleOperators.op_UnaryPlus(Acme.SampleOperators)")]
        [TestCase(typeof(Acme.SampleOperators), "UnaryNegation", ExpectedResult = "M:Acme.SampleOperators.op_UnaryNegation(Acme.SampleOperators)")]
        [TestCase(typeof(Acme.SampleOperators), "Addition", ExpectedResult = "M:Acme.SampleOperators.op_Addition(Acme.SampleOperators,Acme.SampleOperators)")]
        [TestCase(typeof(Acme.SampleOperators), "Subtraction", ExpectedResult = "M:Acme.SampleOperators.op_Subtraction(Acme.SampleOperators,Acme.SampleOperators)")]
        [TestCase(typeof(Acme.SampleOperators), "AdditionAssignment", ExpectedResult = "M:Acme.SampleOperators.op_AdditionAssignment(Acme.SampleOperators)")]
        [TestCase(typeof(Acme.SampleOperators), "SubtractionAssignment", ExpectedResult = "M:Acme.SampleOperators.op_SubtractionAssignment(Acme.SampleOperators)")]
        [TestCase(typeof(Acme.SampleOperators), "IncrementAssignment", ExpectedResult = "M:Acme.SampleOperators.op_IncrementAssignment")]
        [TestCase(typeof(Acme.SampleOperators), "DecrementAssignment", ExpectedResult = "M:Acme.SampleOperators.op_DecrementAssignment")]
        [TestCase(typeof(Acme.SampleOperators), "Implicit", ExpectedResult = "M:Acme.SampleOperators.op_Implicit(Acme.SampleOperators)~System.String")]
        [TestCase(typeof(Acme.SampleOperators), "Explicit", ExpectedResult = "M:Acme.SampleOperators.op_Explicit(Acme.SampleOperators)~System.Int32")]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.False", ExpectedResult = "M:Acme.SampleOperators.Acme#ISampleInterface#op_False(Acme.ISampleInterface)")]
        [TestCase(typeof(Acme.ISampleInterface), "True", ExpectedResult = "M:Acme.ISampleInterface.op_True(Acme.ISampleInterface)")]
        [TestCase(typeof(Acme.ISampleInterface), "False", ExpectedResult = "M:Acme.ISampleInterface.op_False(Acme.ISampleInterface)")]
        public string CodeReference_HasExpectedValue(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod(GetMethodName(operatorName), Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.CodeReference;
        }
    }
}

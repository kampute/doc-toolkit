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
    public class OperatorTests
    {
        private readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        [TestCase("Addition", typeof(Acme.Widget))]
        [TestCase("Implicit", typeof(Acme.Widget))]
        [TestCase("Explicit", typeof(Acme.Widget))]
        public void ImplementsOperator(string operatorName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod($"op_{operatorName}", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IOperator>());
            Assert.That(metadata.Name, Is.EqualTo(operatorName));
        }

        [TestCase("Addition", typeof(Acme.Widget), ExpectedResult = MemberVisibility.Public)]
        [TestCase("Implicit", typeof(Acme.Widget), ExpectedResult = MemberVisibility.Public)]
        [TestCase("Explicit", typeof(Acme.Widget), ExpectedResult = MemberVisibility.Public)]
        public MemberVisibility Visibility_HasExpectedValue(string operatorName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod($"op_{operatorName}", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Visibility;
        }

        [TestCase("Addition", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("Implicit", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("Explicit", typeof(Acme.Widget), ExpectedResult = true)]
        public bool IsStatic_HasExpectedValue(string operatorName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod($"op_{operatorName}", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsStatic;
        }

        [TestCase("Addition", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("Implicit", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("Explicit", typeof(Acme.Widget), ExpectedResult = true)]
        public bool IsVisible_HasExpectedValue(string operatorName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod($"op_{operatorName}", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsVisible;
        }

        [TestCase("Addition", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("Implicit", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("Explicit", typeof(Acme.Widget), ExpectedResult = true)]
        public bool IsSpecialName_HasExpectedValue(string operatorName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod($"op_{operatorName}", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsSpecialName;
        }

        [TestCase("Addition", typeof(Acme.Widget), ExpectedResult = false)]
        [TestCase("Implicit", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("Explicit", typeof(Acme.Widget), ExpectedResult = true)]
        public bool IsConversionOperator_HasExpectedValue(string operatorName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod($"op_{operatorName}", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsConversionOperator;
        }

        [TestCase("Addition", typeof(Acme.Widget))]
        [TestCase("Implicit", typeof(Acme.Widget))]
        [TestCase("Explicit", typeof(Acme.Widget))]
        public void DeclaringType_HasExpectedType(string operatorName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod($"op_{operatorName}", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.DeclaringType.Represents(declaringType), Is.True);
        }

        [TestCase("Addition", ExpectedResult = 2)]
        [TestCase("Implicit", ExpectedResult = 1)]
        [TestCase("Explicit", ExpectedResult = 1)]
        public int Parameters_ReturnsExpectedCount(string operatorName)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod($"op_{operatorName}", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Parameters.Count;
        }

        [TestCase("Addition", typeof(Acme.Widget), ExpectedResult = "Widget")]
        [TestCase("Implicit", typeof(Acme.Widget), ExpectedResult = "Nullable`1")]
        [TestCase("Explicit", typeof(Acme.Widget), ExpectedResult = "Int32")]
        public string ReturnParameter_ReturnsCorrectType(string operatorName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod($"op_{operatorName}", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Return.Type.Name;
        }

        [TestCase(typeof(Acme.Widget), "Addition", ExpectedResult = "M:Acme.Widget.op_Addition(Acme.Widget,Acme.Widget)")]
        [TestCase(typeof(Acme.Widget), "Explicit", ExpectedResult = "M:Acme.Widget.op_Explicit(Acme.Widget)~System.Int32")]
        [TestCase(typeof(Acme.Widget), "Implicit", ExpectedResult = "M:Acme.Widget.op_Implicit(Acme.Widget)~System.Nullable{System.Int64}")]
        public string CodeReference_HasExpectedValue(Type declaringType, string operatorName)
        {
            var methodInfo = declaringType.GetMethod($"op_{operatorName}", bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IOperator;
            Assert.That(metadata, Is.Not.Null);

            return metadata.CodeReference;
        }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Support
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Capabilities;
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class CodeReferenceTests
    {
        [TestCase("N:System.IO", ExpectedResult = true)]
        [TestCase("T:System.String", ExpectedResult = true)]
        [TestCase("F:System.ConsoleColor.Black", ExpectedResult = true)]
        [TestCase("P:System.String.Length", ExpectedResult = true)]
        [TestCase("M:System.String.ToString", ExpectedResult = true)]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = true)]
        [TestCase("N:System.Collections.Generic", ExpectedResult = true)]
        [TestCase("T:System.String.Format(System.String,System.Object[])", ExpectedResult = true)]
        [TestCase("M:System.String.Substring(System.Int32,System.Int32)", ExpectedResult = true)]
        [TestCase("P:System.Collections.Generic.List`1.Item(System.Int32)", ExpectedResult = true)]
        [TestCase("T:System.Collections.Generic.Dictionary`2", ExpectedResult = true)]
        [TestCase("F:System.String.Empty", ExpectedResult = true)]
        [TestCase("E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged", ExpectedResult = true)]
        [TestCase("", ExpectedResult = false)]
        [TestCase("T", ExpectedResult = false)]
        [TestCase("T:", ExpectedResult = false)]
        [TestCase(":System.String", ExpectedResult = false)]
        [TestCase("X:System.String", ExpectedResult = false)]
        [TestCase("T;System.String", ExpectedResult = false)]
        public bool IsValid_ReturnsExpectedResult(string cref)
        {
            return CodeReference.IsValid(cref);
        }

        [TestCase("N:System.Collections.Generic", ExpectedResult = true)]
        [TestCase("T:System.String", ExpectedResult = false)]
        [TestCase("F:System.ConsoleColor.Black", ExpectedResult = false)]
        [TestCase("P:System.String.Length", ExpectedResult = false)]
        [TestCase("M:System.String.ToString", ExpectedResult = false)]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase("N", ExpectedResult = false)]
        [TestCase("N:", ExpectedResult = false)]
        public bool IsNamespace_ReturnsExpectedResult(string cref)
        {
            return CodeReference.IsNamespace(cref);
        }

        [Test]
        public void ResolveMember_WithNullCodeReference_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => CodeReference.ResolveMember(null!));
        }

        [TestCase("", ExpectedResult = null)]
        [TestCase("T", ExpectedResult = null)]
        [TestCase("T:", ExpectedResult = null)]
        [TestCase(":", ExpectedResult = null)]
        [TestCase(":System.String", ExpectedResult = null)]
        [TestCase("X:System.String", ExpectedResult = null)]
        [TestCase("T;System.String", ExpectedResult = null)]
        public IMember? ResolveMember_WithInvalidCodereference_ReturnsNull(string cref)
        {
            return CodeReference.ResolveMember(cref);
        }

        [Test]
        public void ResolveMember_ForType_ReturnsTypeMetadata()
        {
            var result = CodeReference.ResolveMember("T:System.String");

            if (result is not null)
            {
                Assert.That(result, Is.InstanceOf<IType>());
                Assert.That(result.Name, Is.EqualTo("String"));
            }
        }

        [Test]
        public void ResolveMember_ForNestedType_ReturnsTypeMetadata()
        {
            var result = CodeReference.ResolveMember("T:System.Collections.Generic.Dictionary`2.KeyCollection");

            if (result is not null)
            {
                Assert.That(result, Is.InstanceOf<IType>());
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(result.Name, Is.EqualTo("KeyCollection"));
                    Assert.That(result.DeclaringType, Is.Not.Null);
                }
            }
        }

        [Test]
        public void ResolveMember_ForGenericType_ReturnsTypeMetadata()
        {
            var result = CodeReference.ResolveMember("T:System.Collections.Generic.List`1");
            if (result is not null)
            {
                Assert.That(result, Is.InstanceOf<IType>());
                Assert.That(result, Is.InstanceOf<IWithTypeParameters>());
                Assert.That(result.Name, Is.EqualTo("List`1"));
            }
        }

        [Test]
        public void ResolveMember_ForConstructor_ReturnsConstructorMetadata()
        {
            var result = CodeReference.ResolveMember("M:System.String.#ctor(System.Char[])");

            if (result is not null)
            {
                Assert.That(result, Is.InstanceOf<IConstructor>());
                Assert.That(result.Name, Is.EqualTo(".ctor"));
            }
        }

        [Test]
        public void ResolveMember_ForField_ReturnsFieldMetadata()
        {
            var result = CodeReference.ResolveMember("F:System.String.Empty");

            if (result is not null)
            {
                Assert.That(result, Is.InstanceOf<IField>());
                Assert.That(result.Name, Is.EqualTo("Empty"));
            }
        }

        [Test]
        public void ResolveMember_ForMethod_ReturnsMethodMetadata()
        {
            var result = CodeReference.ResolveMember("M:System.String.Substring");

            if (result is not null)
            {
                Assert.That(result, Is.InstanceOf<IMethod>());
                Assert.That(result.Name, Is.EqualTo("Substring"));
            }
        }

        [Test]
        public void ResolveMember_ForGenericMethod_ReturnsMethodMetadata()
        {
            var result = CodeReference.ResolveMember("M:System.Array.Empty``1");

            Assert.That(result, Is.InstanceOf<IMethod>());
            Assert.That(result, Is.InstanceOf<IWithTypeParameters>());
            Assert.That(result.Name, Is.EqualTo("Empty"));
        }

        [Test]
        public void ResolveMember_ForProperty_ReturnsPropertyMetadata()
        {
            var result = CodeReference.ResolveMember("P:System.String.Length");

            if (result is not null)
            {
                Assert.That(result, Is.InstanceOf<IProperty>());
                Assert.That(result.Name, Is.EqualTo("Length"));
            }
        }

        [Test]
        public void ResolveMember_ForEvent_ReturnsEventMetadata()
        {
            var result = CodeReference.ResolveMember("E:System.AppDomain.AssemblyLoad");

            if (result is not null)
            {
                Assert.That(result, Is.InstanceOf<IEvent>());
                Assert.That(result.Name, Is.EqualTo("AssemblyLoad"));
            }
        }

        [Test]
        public void ResolveMember_WithNamespaceReference_ReturnsNull()
        {
            var result = CodeReference.ResolveMember("N:System");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ResolveMember_ForMember_WhenTypeDoesNotSupportMemberKind_ReturnsNull()
        {
            var result = CodeReference.ResolveMember("E:System.String.Event");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ResolveMember_ForMember_WhenTypeDoesNotContainMember_ReturnsNull()
        {
            var result = CodeReference.ResolveMember("M:System.String.NonExistingMethod");

            Assert.That(result, Is.Null);
        }
    }
}
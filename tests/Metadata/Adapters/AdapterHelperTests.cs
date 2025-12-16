// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Adapters;
    using Moq;
    using NUnit.Framework;
    using System.Collections.Generic;

    [TestFixture]
    public class AdapterHelperTests
    {
        [TestCase("IFoo.Bar", "IFoo", "Bar")]
        [TestCase("System.IDisposable.Dispose", "System.IDisposable", "Dispose")]
        [TestCase("Namespace.IInterface.Method", "Namespace.IInterface", "Method")]
        [TestCase("IFoo<T>.Bar", "IFoo`1", "Bar")]
        [TestCase("IFoo<T,U>.Bar", "IFoo`2", "Bar")]
        [TestCase("IFoo<T,U,V>.Method", "IFoo`3", "Method")]
        [TestCase("IFoo<T<U>>.Bar", "IFoo`1", "Bar")]
        [TestCase("IFoo<T<U,V>>.Bar", "IFoo`1", "Bar")]
        [TestCase("IFoo<T<U>,V>.Method", "IFoo`2", "Method")]
        [TestCase("IFoo<T<U<W>>,V>.Method", "IFoo`2", "Method")]
        public void SplitExplicitName_ReturnsCorrectComponents(string explicitName, string expectedInterface, string expectedMember)
        {
            var (interfaceName, memberName) = AdapterHelper.SplitExplicitName(explicitName);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(interfaceName, Is.EqualTo(expectedInterface));
                Assert.That(memberName, Is.EqualTo(expectedMember));
            }
        }

        [Test]
        public void EquivalentParameters_WithNullTargetParameters_ThrowsArgumentNullException()
        {
            var sourceParameters = new List<IParameter>();

            Assert.That(() => AdapterHelper.EquivalentParameters(null!, sourceParameters), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("targetParameters"));
        }

        [Test]
        public void EquivalentParameters_WithNullSourceParameters_ThrowsArgumentNullException()
        {
            var baseParameters = new List<IParameter>();

            Assert.That(() => AdapterHelper.EquivalentParameters(baseParameters, null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("sourceParameters"));
        }

        [Test]
        public void EquivalentParameters_WithDifferentCounts_ReturnsFalse()
        {
            var baseParameters = new List<IParameter> { Mock.Of<IParameter>() };
            var sourceParameters = new List<IParameter>();

            var result = AdapterHelper.EquivalentParameters(baseParameters, sourceParameters);

            Assert.That(result, Is.False);
        }

        [Test]
        public void EquivalentParameters_WithEquivalentParameters_ReturnsTrue()
        {
            var param1 = new Mock<IParameter>();
            param1.Setup(static p => p.IsSatisfiableBy(It.IsAny<IParameter>())).Returns(true);

            var param2 = new Mock<IParameter>();
            param2.Setup(static p => p.IsSatisfiableBy(It.IsAny<IParameter>())).Returns(true);

            var baseParameters = new List<IParameter> { param1.Object, param2.Object };
            var sourceParameters = new List<IParameter> { Mock.Of<IParameter>(), Mock.Of<IParameter>() };

            var result = AdapterHelper.EquivalentParameters(baseParameters, sourceParameters);

            Assert.That(result, Is.True);
        }

        [Test]
        public void EquivalentParameters_WithNonEquivalentParameters_ReturnsFalse()
        {
            var param1 = new Mock<IParameter>();
            param1.Setup(static p => p.IsSatisfiableBy(It.IsAny<IParameter>())).Returns(false);

            var param2 = new Mock<IParameter>();
            param2.Setup(static p => p.IsSatisfiableBy(It.IsAny<IParameter>())).Returns(true);

            var baseParameters = new List<IParameter> { param1.Object, param2.Object };
            var sourceParameters = new List<IParameter> { Mock.Of<IParameter>(), Mock.Of<IParameter>() };

            var result = AdapterHelper.EquivalentParameters(baseParameters, sourceParameters);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HaveSameDeclarationScope_WithBothNull_ReturnsTrue()
        {
            var result = AdapterHelper.HaveSameDeclarationScope(null, null);

            Assert.That(result, Is.True);
        }

        [Test]
        public void HaveSameDeclarationScope_WithOneNull_ReturnsFalse()
        {
            var result = AdapterHelper.HaveSameDeclarationScope(typeof(string), null);

            Assert.That(result, Is.False);
        }

        [Test]
        public void HaveSameDeclarationScope_WithSameReference_ReturnsTrue()
        {
            var type = typeof(string);

            var result = AdapterHelper.HaveSameDeclarationScope(type, type);

            Assert.That(result, Is.True);
        }

        [Test]
        public void HaveSameDeclarationScope_WithDifferentNames_ReturnsFalse()
        {
            var result = AdapterHelper.HaveSameDeclarationScope(typeof(string), typeof(int));

            Assert.That(result, Is.False);
        }

        [Test]
        public void HaveSameDeclarationScope_WithNestedTypesSameHierarchy_ReturnsTrue()
        {
            var result = AdapterHelper.HaveSameDeclarationScope(typeof(List<string>.Enumerator), typeof(List<int>.Enumerator));

            Assert.That(result, Is.True);
        }
    }
}

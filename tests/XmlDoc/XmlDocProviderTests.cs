// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.XmlDoc;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class XmlDocProviderTests
    {
        private XmlDocProvider xmlDocProvider = null!;

        [SetUp]
        public void SetUp()
        {
            var repository = new XmlDocRepository();
            repository.ImportFile(Path.ChangeExtension(GetType().Assembly.Location, ".xml"));
            xmlDocProvider = new XmlDocProvider(repository);
        }

        [TestCase("Acme", ExpectedResult = "Documentation for the Acme namespace.")]
        public string? TryGetNamespaceDoc_ReturnsCorrectDoc(string ns)
        {
            return xmlDocProvider.TryGetNamespaceDoc(ns, out var doc) ? doc.Summary.ToString().Trim() : null;
        }

        [TestCase(typeof(Acme.ISampleInterface), ExpectedResult = "A sample interface for testing purposes.")]
        [TestCase(typeof(Acme.ISampleGenericInterface<>), ExpectedResult = "A sample generic interface.")]
        [TestCase(typeof(Acme.SampleGenericClass<>), ExpectedResult = "A sample generic class.")]
        [TestCase(typeof(Acme.SampleGenericStruct<>), ExpectedResult = "A sample generic struct.")]
        public string? TryGetMemberDoc_ForTypes_ReturnsCorrectDoc(Type type)
        {
            return xmlDocProvider.TryGetMemberDoc(type.GetMetadata(), out var doc) ? doc.Summary.ToString().Trim() : null;
        }

        [TestCase(typeof(Acme.SampleConstructors), ExpectedResult = "Initializes a new instance of the SampleConstructors class with required members set.")]
        [TestCase(typeof(Acme.SampleConstructors), typeof(int), ExpectedResult = "Initializes a new instance of the SampleConstructors class with an integer parameter.")]
        [TestCase(typeof(Acme.ISampleInterface), ExpectedResult = "Initializes the static members of the ISampleInterface interface.")]
        public string? TryGetMemberDoc_ForConstructors_ReturnsCorrectDoc(Type type, params Type[] paramTypes)
        {
            var constructor = type.GetConstructor(Acme.Bindings.AllDeclared, null, paramTypes, null)?.GetMetadata();
            Assert.That(constructor, Is.InstanceOf<IConstructor>());

            return xmlDocProvider.TryGetMemberDoc(constructor, out var doc) ? doc.Summary.ToString().Trim() : null;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.ArrayParamsMethod), ExpectedResult = "A method with params array.")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithGenericParameter), ExpectedResult = "A generic method with a generic parameter.")]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethodWithOutParam", ExpectedResult = "Explicitly implements the interface method with out parameter.")]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceStaticMethod", ExpectedResult = "Explicitly implements the static interface method.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.NonExtensionMethod), ExpectedResult = "A non-extension method to verify correct classification.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.ClassicExtensionMethod), ExpectedResult = "A classic extension method.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethod), ExpectedResult = "An instance extension method.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethod), ExpectedResult = "A static extension method.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.GenericExtensionMethod), ExpectedResult = "A generic extension method.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.ClassicExtensionMethodForClass), ExpectedResult = "A classic extension method for generic class.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethodForClass), ExpectedResult = "An instance extension method for generic class.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethodForClass), ExpectedResult = "A static extension method for generic class.")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.GenericExtensionMethodForClass), ExpectedResult = "A generic extension method for generic class.")]
        public string? TryGetMemberDoc_ForMethod_ReturnsCorrectDoc(Type type, string methodName)
        {
            var method = type.GetMethod(methodName, Acme.Bindings.AllDeclared)?.GetMetadata();
            Assert.That(method, Is.InstanceOf<IMethod>());

            return xmlDocProvider.TryGetMemberDoc(method, out var doc) ? doc.Summary.ToString().Trim() : null;
        }

        [TestCase(typeof(Acme.SampleOperators), "op_UnaryPlus", ExpectedResult = "Unary plus operator.")]
        [TestCase(typeof(Acme.SampleOperators), "op_Addition", ExpectedResult = "Addition operator.")]
        [TestCase(typeof(Acme.SampleOperators), "op_IncrementAssignment", ExpectedResult = "Increment operator.")]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.op_False", ExpectedResult = "Explicitly implements the false operator for the interface.")]
        public string? TryGetMemberDoc_ForOperators_ReturnsCorrectDoc(Type type, string methodName)
        {
            var op = type.GetMethod(methodName, Acme.Bindings.AllDeclared)?.GetMetadata();
            Assert.That(op, Is.InstanceOf<IOperator>());

            return xmlDocProvider.TryGetMemberDoc(op, out var doc) ? doc.Summary.ToString().Trim() : null;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = "A regular property.")]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty), ExpectedResult = "A static property.")]
        [TestCase(typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty", ExpectedResult = "Explicitly implements the interface property.")]
        [TestCase(typeof(Acme.SampleExtensions), "InstanceExtensionProperty", ExpectedResult = "An instance extension property.")]
        [TestCase(typeof(Acme.SampleExtensions), "StaticExtensionProperty", ExpectedResult = "A static extension property.")]
        [TestCase(typeof(Acme.SampleExtensions), "InstanceExtensionPropertyForClass", ExpectedResult = "An instance extension property for generic class.")]
        [TestCase(typeof(Acme.SampleExtensions), "StaticExtensionPropertyForClass", ExpectedResult = "A static extension property for generic class.")]
        public string? TryGetMemberDoc_ForProperty_ReturnsCorrectDoc(Type type, string propertyName)
        {
            var property = propertyName.Contains("ExtensionProperty")
                ? type.GetMetadata<IClassType>().Properties.FirstOrDefault(p => p.Name == propertyName)
                : type.GetProperty(propertyName, Acme.Bindings.AllDeclared)?.GetMetadata();
            Assert.That(property, Is.InstanceOf<IProperty>());

            return xmlDocProvider.TryGetMemberDoc(property, out var doc) ? doc.Summary.ToString().Trim() : null;
        }

        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = "A regular event.")]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent), ExpectedResult = "A static event.")]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = "Explicitly implements the interface event.")]
        public string? TryGetMemberDoc_ForEvents_ReturnsCorrectDoc(Type type, string eventName)
        {
            var ev = type.GetEvent(eventName, Acme.Bindings.AllDeclared)?.GetMetadata();
            Assert.That(ev, Is.InstanceOf<IEvent>());

            return xmlDocProvider.TryGetMemberDoc(ev, out var doc) ? doc.Summary.ToString().Trim() : null;
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField), ExpectedResult = "A static readonly field.")]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ComplexField), ExpectedResult = "A complex unsafe field.")]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.FixedBuffer), ExpectedResult = "A fixed buffer field.")]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceField), ExpectedResult = "A static readonly field in interface.")]
        public string? TryGetMemberDoc_ForFields_ReturnsCorrectDoc(Type type, string fieldName)
        {
            var field = type.GetField(fieldName, Acme.Bindings.AllDeclared)?.GetMetadata();
            Assert.That(field, Is.InstanceOf<IField>());

            return xmlDocProvider.TryGetMemberDoc(field, out var doc) ? doc.Summary.ToString().Trim() : null;
        }

        [Test]
        public void TryGetMemberDoc_ForExtensionBlocks_ReturnsCorrectDoc()
        {
            var extensionBlock = typeof(Acme.SampleExtensions).GetMetadata<IClassType>()
                .ExtensionBlocks.First(static block => block.Receiver.Type.Name == nameof(Acme.ISampleInterface));

            var result = xmlDocProvider.TryGetMemberDoc(extensionBlock, out var doc);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(doc, Is.Not.Null);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(doc.Summary.ToString().Trim(), Is.EqualTo("Extension members for non-generic interface."));
                Assert.That(doc.Parameters["instance"].ToString().Trim(), Is.EqualTo("The instance."));
            }
        }

        [Test]
        public void TryGetMemberDoc_ForExtensionBlockMembers_ReturnsCorrectDoc()
        {
            var extensionBlock = typeof(Acme.SampleExtensions).GetMetadata<IClassType>()
                .ExtensionBlocks.First(static block => block.Receiver.Type.Name == nameof(Acme.ISampleInterface));

            var member = extensionBlock.Properties.First(static property => property.Name == "InstanceExtensionProperty");

            var result = xmlDocProvider.TryGetMemberDoc(member, out var doc);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(doc?.Summary.ToString().Trim(), Is.EqualTo("An instance extension property."));
            }
        }

        [Test]
        public void TryGetMemberDoc_ForExtensionProperties_ReturnsDocWithExtensionBlockDoc()
        {
            var extensionProperty = typeof(Acme.SampleExtensions).GetMetadata<IClassType>()
                .Properties.First(static property => property.Name == "InstanceExtensionProperty");

            var result = xmlDocProvider.TryGetMemberDoc(extensionProperty, out var doc);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(doc, Is.Not.Null);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(doc.Summary.ToString().Trim(), Is.EqualTo("An instance extension property."));
                Assert.That(doc.ExtensionBlock.Summary.ToString().Trim(), Is.EqualTo("Extension members for non-generic interface."));
            }
        }

        [Test]
        public void TryGetMemberDoc_ForExtensionMethods_ReturnsDocWithExtensionBlockDoc()
        {
            var extensionMethod = typeof(Acme.SampleExtensions).GetMetadata<IClassType>()
                .Methods.First(static method => method.Name == "StaticExtensionMethod");

            var result = xmlDocProvider.TryGetMemberDoc(extensionMethod, out var doc);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(doc, Is.Not.Null);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(doc.Summary.ToString().Trim(), Is.EqualTo("A static extension method."));
                Assert.That(doc.ExtensionBlock.Summary.ToString().Trim(), Is.EqualTo("Extension members for non-generic interface."));
            }
        }
    }
}

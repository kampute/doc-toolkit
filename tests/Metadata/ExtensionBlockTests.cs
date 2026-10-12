// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    [TestFixture]
    public class ExtensionBlockTests
    {
        private static IExtensionBlock GetBlock(Type receiverType)
        {
            var container = typeof(Acme.SampleExtensions).GetMetadata<IClassType>();
            return container.ExtensionBlocks.Single(b => b.Receiver.Type.Name == receiverType.Name);
        }

        private static HashSet<string> GetCompilerDocumentationIds()
        {
            var xmlPath = Path.ChangeExtension(typeof(Acme.SampleExtensions).Assembly.Location, ".xml");
            return [.. XDocument.Load(xmlPath).Descendants("member").Select(static m => m.Attribute("name")!.Value)];
        }

        [Test]
        public void ExtensionBlocks_OnePerReceiverType()
        {
            var container = typeof(Acme.SampleExtensions).GetMetadata<IClassType>();

            Assert.That(container.ExtensionBlocks.Select(static b => b.Receiver.Type.Name), Is.EquivalentTo(new[]
            {
                "SampleGenericClass`1",
                "SampleGenericStruct`1",
                "ISampleGenericInterface`1",
                nameof(Acme.ISampleInterface),
            }));
        }

        [Test]
        public void ExtensionBlock_WithNonGenericReceiver_HasNoTypeParameters()
        {
            var block = GetBlock(typeof(Acme.ISampleInterface));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(block.IsGenericBlock, Is.False);
                Assert.That(block.TypeParameters, Is.Empty);
            }
        }

        [Test]
        public void ExtensionBlock_WithGenericReceiver_ExposesBlockTypeParameters()
        {
            var block = GetBlock(typeof(Acme.SampleGenericClass<>));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(block.IsGenericBlock, Is.True);
                Assert.That(block.TypeParameters.Select(static t => t.Name), Is.EqualTo(new[] { "W" }));
            }
        }

        [Test]
        public void MethodsAndProperties_ContainOnlyMembersOfTheBlockOrderedByName()
        {
            var block = GetBlock(typeof(Acme.ISampleInterface));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(block.Methods.Select(static m => m.Name), Is.EqualTo(new[]
                {
                    nameof(Acme.SampleExtensions.GenericExtensionMethod),
                    nameof(Acme.SampleExtensions.InstanceExtensionMethod),
                    nameof(Acme.SampleExtensions.StaticExtensionMethod),
                }));
                Assert.That(block.Properties.Select(static p => p.Name), Is.EqualTo(new[]
                {
                    "FullExtensionProperty",
                    "InstanceExtensionProperty",
                    "StaticExtensionProperty",
                }));
            }
        }

        [Test]
        public void CodeReferences_MatchCompilerDocumentationIds()
        {
            var container = typeof(Acme.SampleExtensions).GetMetadata<IClassType>();
            var documentationIds = GetCompilerDocumentationIds();

            var codeReferences = container.ExtensionBlocks
                .SelectMany(static b => new[] { b.CodeReference }
                    .Concat(b.Methods.Select(static m => m.CodeReference))
                    .Concat(b.Properties.Select(static p => p.CodeReference)))
                .ToList();

            Assert.That(codeReferences, Has.Count.EqualTo(4 + 12 + 9));
            Assert.That(codeReferences, Is.SubsetOf(documentationIds));
        }

        [TestCase(typeof(Acme.SampleMethods), ExpectedResult = true)] // Implements the receiver interface
        [TestCase(typeof(Acme.ISampleInterface), ExpectedResult = true)]
        [TestCase(typeof(string), ExpectedResult = false)]
        public bool Extends_WithNonGenericInterfaceReceiver_ReturnsWhetherTypeIsAssignableToReceiver(Type type)
        {
            var block = GetBlock(typeof(Acme.ISampleInterface));

            return block.Extends(type.GetMetadata());
        }
    }
}

// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Metadata;
    using Moq;
    using NUnit.Framework;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class FileSystemDocumentationComposerTests
    {
        private string outputFolder = null!;

        [SetUp]
        public void SetUp()
        {
            MetadataProvider.RegisterRuntimeAssemblies(); // To ensure all code references are resolvable
            outputFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, recursive: true);
        }

        [Test]
        public void GenerateDocumentation_WritesEachRenderedPageToItsFileInOutputFolder()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>(typeof(Acme.SampleFields).Assembly.GetMetadata());
            Mock.Get(context.AddressProvider).SetupGet(static x => x.Granularity).Returns(PageGranularity.Namespace);
            var composer = new FileSystemDocumentationComposer(new CategoryRenderer());

            composer.GenerateDocumentation(context, outputFolder);

            var expectedFiles = context.Namespaces.Select(ns => Path.Combine(outputFolder, ns.Name.ToLowerInvariant()));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(Directory.GetFiles(outputFolder, "*", SearchOption.AllDirectories), Is.EquivalentTo(expectedFiles));
                Assert.That(expectedFiles.Select(File.ReadAllText), Is.All.EqualTo(nameof(PageCategory.NamespaceWithTypes)));
            }
        }

        private sealed class CategoryRenderer : DocumentPageRenderer
        {
            protected override void Render(TextWriter writer, PageCategory category, IDocumentModel model) => writer.Write(category);
        }
    }
}

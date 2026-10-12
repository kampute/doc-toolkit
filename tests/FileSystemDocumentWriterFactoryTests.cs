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
    public class FileSystemDocumentWriterFactoryTests
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
        public void CreateTextWriter_WithNestedRelativePath_CreatesMissingDirectoriesAndWritesFile()
        {
            var factory = new FileSystemDocumentWriterFactory(outputFolder);

            using (var writer = factory.CreateTextWriter("api/types/page.html"))
                writer.Write("content");

            Assert.That(File.ReadAllText(Path.Combine(outputFolder, "api", "types", "page.html")), Is.EqualTo("content"));
        }

        [Test]
        public void CreateWriter_WithModelWithoutDocumentationFile_ThrowsArgumentException()
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>();
            var modelMock = new Mock<IDocumentModel>();
            modelMock.SetupGet(static m => m.Context).Returns(context);
            modelMock.Setup(static m => m.TryGetDocumentationFile(out It.Ref<string?>.IsAny)).Returns(false);

            var factory = new FileSystemDocumentWriterFactory(outputFolder);

            Assert.That(() => factory.CreateWriter(modelMock.Object), Throws.ArgumentException.With.Property("ParamName").EqualTo("model"));
        }

        [TestCase(false, ExpectedResult = "<p>a b</p>")]
        [TestCase(true, ExpectedResult = "<p>  a   b  </p>")]
        public string CreateWriter_WritesModelFileMinifiedUnlessDisabled(bool disableMinification)
        {
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>(typeof(Acme.SampleFields).Assembly.GetMetadata());
            var ns = context.Namespaces.First(static n => n.Name == "Acme");
            var factory = new FileSystemDocumentWriterFactory(outputFolder, new OutputOptions { DisableMinification = disableMinification });

            using (var writer = factory.CreateWriter(ns))
                writer.Write("<p>  a   b  </p>");

            return File.ReadAllText(Path.Combine(outputFolder, "acme"));
        }
    }
}

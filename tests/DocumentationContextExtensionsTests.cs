// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.IO.Writers;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.XmlDoc;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    [TestFixture]
    public class DocumentationContextExtensionsTests
    {
        [SetUp]
        public void Setup()
        {
            MetadataProvider.RegisterRuntimeAssemblies(); // To ensure all code references are resolvable
        }

        [Test]
        public void FindMember_WhenMemberDoesNotExist_ReturnsNull()
        {
            var assembly = MockHelper.CreateAssembly("TestAssembly", ["Test.Namespace"]);
            using var docContext = MockHelper.CreateDocumentationContext<HtmlFormat>(assembly);

            var memberMock = new Mock<IMember>();
            memberMock.SetupGet(static m => m.Assembly).Returns(assembly);

            var result = docContext.FindMember(memberMock.Object);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void FindMember_WhenMemberExists_ReturnsMember()
        {
            var assembly = MockHelper.CreateAssembly("TestAssembly", ["Test.Namespace"]);
            using var docContext = MockHelper.CreateDocumentationContext<HtmlFormat>(assembly);

            var type = assembly.ExportedTypes.First();

            var result = docContext.FindMember(type);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(type.Name));
        }

        [Test]
        public void InspectDocumentations_WithNoMembers_ReturnsEmpty()
        {
            using var docContext = MockHelper.CreateDocumentationContext<HtmlFormat>();

            var result = docContext.InspectDocumentations().ToList();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void InspectDocumentations_WithMembers_YieldsIssues()
        {
            var assembly = MockHelper.CreateAssembly("TestAssembly", ["Test.Namespace"]);
            using var docContext = MockHelper.CreateDocumentationContext<HtmlFormat>(assembly);

            var issues = docContext.InspectDocumentations(XmlDocInspectionOptions.All).ToList();

            // The MockHelper.CreateAssembly creates an assembly with two exported classes that have no members.
            // These classes have XML documentation with only a <summary> tag. Based on this configuration, we expect that
            // the inspection finds no required tag issues but it should report that the optional <remarks>, <example>, and
            // <threadsafety> tags are missing.

            var requiredTagIssues = issues.Where(static i => i.IssueType == XmlDocInspectionIssueType.MissingRequiredTag).ToList();
            Assert.That(requiredTagIssues, Is.Empty, "No required tags should be reported as missing");

            var optionalTagIssues = issues.Where(static i => i.IssueType == XmlDocInspectionIssueType.MissingOptionalTag).ToList();
            Assert.That(optionalTagIssues, Has.Count.EqualTo(6), "The <remarks>, <example>, and <threadsafety> tags should be reported as missing");

            var referenceIssues = issues.Where(static i => i.IssueType == XmlDocInspectionIssueType.UndocumentedReference).ToList();
            Assert.That(referenceIssues, Is.Empty, "No references should be reported as undocumented");

            var seeAlsoIssues = issues.Where(static i => i.IssueType == XmlDocInspectionIssueType.UntitledSeeAlso).ToList();
            Assert.That(seeAlsoIssues, Is.Empty, "No see-also references should be reported as untitled");
        }

        [Test]
        public void IsFormatSupported_SupportedFormat_ReturnsTrue()
        {
            using var docContext = MockHelper.CreateDocumentationContext<TestFormatter>();

            var result = docContext.IsFormatSupported("document.xyz");

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsFormatSupported_SupportedFormat_ExtensionOnly_ReturnsTrue()
        {
            using var docContext = MockHelper.CreateDocumentationContext<TestFormatter>();

            var result = docContext.IsFormatSupported(".xyz");

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsFormatSupported_UnsupportedFormat_ReturnsFalse()
        {
            using var docContext = MockHelper.CreateDocumentationContext<TestFormatter>();

            var result = docContext.IsFormatSupported("document.unsupported");

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsFormatSupported_WithoutExtension_ReturnsFalse()
        {
            using var docContext = MockHelper.CreateDocumentationContext<TestFormatter>();

            var result = docContext.IsFormatSupported("document-without-extension");

            Assert.That(result, Is.False);
        }

        [Test]
        public void TryTransformText_EmptyText_ReturnsEmptyString()
        {
            using var docContext = MockHelper.CreateDocumentationContext<TestFormatter>();

            var result = docContext.TryTransformText(".unsupported", string.Empty, out var transformed);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformed, Is.Empty);
            }
        }

        [Test]
        public void TryTransformText_SupportedFormat_TransformsText()
        {
            using var docContext = MockHelper.CreateDocumentationContext<TestFormatter>();

            var result = docContext.TryTransformText(".xyz", "Original text", out var transformed);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformed, Is.EqualTo("Transformed: Original text"));
            }
        }

        [Test]
        public void TryTransformText_UnsupportedFormat_ReturnsFalse()
        {
            using var docContext = MockHelper.CreateDocumentationContext<TestFormatter>();

            var result = docContext.TryTransformText(".unknown", "Original text", out var transformed);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(transformed, Is.Null);
            }
        }

        private sealed class TestFormatter : IDocumentFormatter
        {
            public TestFormatter()
            {
                TextTransformers = new TextTransformerRegistry(".mock");
                TextTransformers.Register<TestTransformer>(".xyz");
            }

            public string FileExtension => ".mock";

            public TextTransformerRegistry TextTransformers { get; }

            public MarkupWriter CreateMarkupWriter(TextWriter writer, bool disposeWriter = false) => throw new NotImplementedException();
            public TextWriter CreateMinifier(TextWriter writer) => throw new NotImplementedException();
            public void Encode(ReadOnlySpan<char> text, TextWriter writer) => throw new NotImplementedException();
            public void Transform(TextWriter writer, XElement comment) => throw new NotImplementedException();
        }

        private sealed class TestTransformer : ITextTransformer
        {
            public void Transform(TextReader reader, TextWriter writer, IUrlTransformer? urlTransformer = null)
            {
                var text = reader.ReadToEnd();
                writer.Write($"Transformed: {text}");
            }
        }
    }
}

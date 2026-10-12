// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Topics;
    using Kampute.DocToolkit.XmlDoc;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class DocumentationContextBuilderTests
    {
        private string folder = null!;
        private string assemblyPath = null!;
        private string xmlDocPath = null!;

        [SetUp]
        public void SetUp()
        {
            folder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(folder);
            assemblyPath = typeof(Acme.SampleMethods).Assembly.Location;
            xmlDocPath = Path.ChangeExtension(assemblyPath, ".xml");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, recursive: true);
        }

        private string WriteFile(string name, string content)
        {
            var path = Path.Combine(folder, name);
            File.WriteAllText(path, content);
            return path;
        }

        [Test]
        public void FactoryMethods_ConfigureMatchingAddressingStrategy()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(DocumentationContextBuilder.DotNetApi().Strategy, Is.InstanceOf<DotNetApiStrategy>());
                Assert.That(DocumentationContextBuilder.DocFx().Strategy, Is.InstanceOf<DocFxStrategy>());
                Assert.That(DocumentationContextBuilder.DevOpsWiki().Strategy, Is.InstanceOf<DevOpsWikiStrategy>());
            }
        }

        [Test]
        public void Constructor_WithNullStrategy_ThrowsArgumentNullException()
        {
            Assert.That(static () => new DocumentationContextBuilder(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("strategy"));
        }

        [TestCase("https://example.com/docs", ExpectedResult = "https://example.com/docs/")]
        [TestCase("https://example.com/docs/", ExpectedResult = "https://example.com/docs/")]
        public string WithBaseUrl_WithString_EnsuresTrailingSlash(string baseUrl)
        {
            return DocumentationContextBuilder.DotNetApi().WithBaseUrl(baseUrl).BaseUrl!.ToString();
        }

        [Test]
        public void WithBaseUrl_WithInvalidArguments_Throws()
        {
            var builder = DocumentationContextBuilder.DotNetApi();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(() => builder.WithBaseUrl(" "), Throws.ArgumentException.With.Property("ParamName").EqualTo("baseUrl"));
                Assert.That(() => builder.WithBaseUrl("docs/api"), Throws.TypeOf<UriFormatException>());
                Assert.That(() => builder.WithBaseUrl(new Uri("docs/", UriKind.Relative)), Throws.ArgumentException.With.Property("ParamName").EqualTo("baseUrl"));
                Assert.That(() => builder.WithBaseUrl((Uri)null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("baseUrl"));
            }
        }

        [Test]
        public void WithLanguageAndErrorHandler_WithNull_ThrowArgumentNullException()
        {
            var builder = DocumentationContextBuilder.DotNetApi();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(() => builder.WithLanguage(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("language"));
                Assert.That(() => builder.WithXmlDocErrorHandler(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("errorHandler"));
            }
        }

        [Test]
        public void AddAssembly_WithSameFileTwice_KeepsOnePath()
        {
            var builder = DocumentationContextBuilder.DotNetApi().AddAssembly(assemblyPath).AddAssembly(assemblyPath);

            Assert.That(builder.AssemblyPaths, Is.EqualTo(new[] { assemblyPath }));
        }

        [Test]
        public void AddAssemblyAndAddXmlDoc_WithInvalidPaths_Throw()
        {
            var builder = DocumentationContextBuilder.DotNetApi();
            var missing = Path.Combine(folder, "missing.dll");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(() => builder.AddAssembly(""), Throws.ArgumentException.With.Property("ParamName").EqualTo("assemblyPath"));
                Assert.That(() => builder.AddAssembly(missing), Throws.TypeOf<FileNotFoundException>());
                Assert.That(() => builder.AddXmlDoc(""), Throws.ArgumentException.With.Property("ParamName").EqualTo("xmlDocPath"));
                Assert.That(() => builder.AddXmlDoc(missing), Throws.TypeOf<FileNotFoundException>());
            }
        }

        [Test]
        public void AddTopic_WithIdAndFilePath_AddsFileTopicWithThatId()
        {
            var topicPath = WriteFile("guide.md", "# Guide");

            var builder = DocumentationContextBuilder.DotNetApi().AddTopic("user-guide", topicPath);

            var topic = builder.Topics.Single();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(topic, Is.InstanceOf<FileTopic>());
                Assert.That(topic.Id, Is.EqualTo("user-guide"));
                Assert.That(((FileTopic)topic).FilePath, Is.EqualTo(topicPath));
            }
        }

        [Test]
        public void AddTopic_WithInvalidArguments_Throws()
        {
            var topicPath = WriteFile("guide.md", "# Guide");
            var builder = DocumentationContextBuilder.DotNetApi().AddTopic("guide", topicPath);
            var subtopic = MockTopicBuilder.Topic("parent").WithChild("child").Build().Subtopics.Single();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(() => builder.AddTopic("GUIDE", topicPath), Throws.ArgumentException, "Topic IDs are case-insensitive.");
                Assert.That(() => builder.AddTopic(" ", topicPath), Throws.ArgumentException.With.Property("ParamName").EqualTo("topicId"));
                Assert.That(() => builder.AddTopic("other", Path.Combine(folder, "missing.md")), Throws.TypeOf<FileNotFoundException>());
                Assert.That(() => builder.AddTopic(subtopic), Throws.ArgumentException.With.Property("ParamName").EqualTo("topic"));
                Assert.That(() => builder.AddTopic(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("topic"));
            }
        }

        [Test]
        public void Build_AssemblesContextFromConfiguration()
        {
            var language = new CSharp();
            var errorHandler = Mock.Of<IXmlDocErrorHandler>();
            var topicPath = WriteFile("guide.md", "# Guide");

            using var context = DocumentationContextBuilder.DotNetApi()
                .WithLanguage(language)
                .WithXmlDocErrorHandler(errorHandler)
                .WithBaseUrl("https://example.com/docs")
                .AddAssembly(assemblyPath)
                .AddXmlDoc(xmlDocPath)
                .AddTopic("guide", topicPath)
                .Build();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(context.Language, Is.SameAs(language));
                Assert.That(context.ContentFormatter, Is.InstanceOf<HtmlFormat>());
                Assert.That(context.Assemblies.Select(static a => a.Name), Is.EqualTo(new[] { typeof(Acme.SampleMethods).Assembly.GetName().Name }));
                Assert.That(context.Assemblies.Single().Metadata.IsReflectionOnly, Is.True, "Assemblies are loaded for metadata only.");
                Assert.That(context.Topics.Select(static t => t.Id), Is.EqualTo(new[] { "guide" }));
                Assert.That(context.AddressProvider.TryGetNamespaceUrl("Acme", out var url) ? url.ToString() : null, Is.EqualTo("https://example.com/docs/api/acme.html"));
                Assert.That(context.AddressProvider.TryGetNamespaceUrl("System", out var externalUrl) ? externalUrl.Host : null, Is.EqualTo("learn.microsoft.com"), "MicrosoftDocs is added by default.");
                Assert.That(context.Types.Any(static t => t.Metadata.FullName == typeof(Acme.SampleMethods).FullName && !t.Doc.Summary.IsEmpty), Is.True, "XML documentation is imported.");
            }
        }

        [Test]
        public void Build_WithContextAwareStrategy_BindsStrategyToContext()
        {
            var topicPath = WriteFile("home.md", "# Home");

            using var context = DocumentationContextBuilder.DevOpsWiki(new DevOpsWikiOptions { MainTopicId = "home" })
                .AddAssembly(assemblyPath)
                .AddTopic("home", topicPath)
                .Build();

            Assert.That(context.AddressProvider.TryGetNamespaceFile("Acme", out var path) ? path : null, Is.EqualTo("Home/Acme.md"));
        }
    }
}

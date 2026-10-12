// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Topics;
    using Moq;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class DocumentationComposerTests
    {
        private static readonly PageCategory[] MemberCategories =
        [
            PageCategory.Constructor, PageCategory.ConstructorOverloads, PageCategory.Field, PageCategory.Event,
            PageCategory.Property, PageCategory.PropertyOverloads, PageCategory.Method, PageCategory.MethodOverloads,
            PageCategory.Operator, PageCategory.OperatorOverloads,
        ];

        [SetUp]
        public void Setup()
        {
            MetadataProvider.RegisterRuntimeAssemblies(); // To ensure all code references are resolvable
        }

        [Test]
        public void GenerateDocumentation_WithNamespaceGranularity_WritesOnlyNamespacePagesWithTypes()
        {
            var pages = Compose(PageGranularity.Namespace).Pages;

            Assert.That(pages.Select(static p => p.Category), Is.Not.Empty.And.All.EqualTo(PageCategory.NamespaceWithTypes));
        }

        [Test]
        public void GenerateDocumentation_WithNamespaceTypeGranularity_WritesTypePagesWithMembersAndNoMemberPages()
        {
            var categories = Compose(PageGranularity.NamespaceType).Pages.Select(static p => p.Category).ToHashSet();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(categories, Is.SupersetOf(new[]
                {
                    PageCategory.Namespace, PageCategory.ClassWithMembers, PageCategory.StructWithMembers,
                    PageCategory.InterfaceWithMembers, PageCategory.Enum,
                }));
                Assert.That(categories, Has.None.AnyOf(PageCategory.NamespaceWithTypes, PageCategory.Class, PageCategory.Struct, PageCategory.Interface));
                Assert.That(categories.Intersect(MemberCategories), Is.Empty);
            }
        }

        [Test]
        public void GenerateDocumentation_WithNamespaceTypeMemberGranularity_WritesSeparateTypeAndMemberPages()
        {
            var categories = Compose(PageGranularity.NamespaceTypeMember).Pages.Select(static p => p.Category).ToHashSet();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(categories, Is.SupersetOf(new[]
                {
                    PageCategory.Namespace, PageCategory.Class, PageCategory.Struct, PageCategory.Interface, PageCategory.Enum,
                    PageCategory.Field, PageCategory.Property, PageCategory.Method, PageCategory.MethodOverloads,
                }));
                Assert.That(categories, Has.None.AnyOf(
                    PageCategory.NamespaceWithTypes, PageCategory.ClassWithMembers, PageCategory.StructWithMembers, PageCategory.InterfaceWithMembers));
            }
        }

        [Test]
        public void GenerateDocumentation_RendersEachPageToItsOwnDisposedWriterCreatedForThatPage()
        {
            var writerFactory = new InMemoryWriterFactory();
            var pages = Compose(PageGranularity.NamespaceTypeMember, writerFactory: writerFactory).Pages;

            using (Assert.EnterMultipleScope())
            {
                Assert.That(pages.Select(static p => p.Writer), Is.EquivalentTo(writerFactory.Writers.Keys));
                Assert.That(pages.Select(p => writerFactory.Writers[p.Writer]), Is.EqualTo(pages.Select(static p => PrimaryModel(p.Model))));
                Assert.That(writerFactory.Writers.Keys.Select(static w => w.IsDisposed), Is.All.True);
            }
        }

        [Test]
        public void GenerateDocumentation_WritesPageForEveryTopicIncludingSubtopics()
        {
            var topic = MockTopicBuilder.Topic("guide").WithChildren("install", "usage").Build();

            var (pages, flattenedTopics) = Compose(PageGranularity.Namespace, [topic]);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(flattenedTopics, Has.Count.EqualTo(3));
                Assert.That(pages.Where(static p => p.Category == PageCategory.Topic).Select(static p => p.Model), Is.EquivalentTo(flattenedTopics));
            }
        }

        private static IDocumentModel PrimaryModel(IDocumentModel model)
            => model is IEnumerable<IDocumentModel> overloads ? overloads.First() : model;

        private static (List<RenderedPage> Pages, List<IDocumentModel> FlattenedTopics) Compose(
            PageGranularity granularity, IEnumerable<ITopic>? topics = null, InMemoryWriterFactory? writerFactory = null)
        {
            var assembly = typeof(Acme.SampleFields).Assembly.GetMetadata();
            using var context = MockHelper.CreateDocumentationContext<HtmlFormat>([assembly], topics ?? []);
            Mock.Get(context.AddressProvider).SetupGet(static x => x.Granularity).Returns(granularity);

            var renderer = new RecordingRenderer();
            new TestComposer(renderer).GenerateDocumentation(writerFactory ?? new InMemoryWriterFactory(), context);
            return (renderer.Pages, [.. context.Topics.Flatten]);
        }

        private sealed record RenderedPage(PageCategory Category, IDocumentModel Model, TrackingWriter Writer);

        private sealed class RecordingRenderer : DocumentPageRenderer
        {
            public List<RenderedPage> Pages { get; } = [];

            protected override void Render(TextWriter writer, PageCategory category, IDocumentModel model)
                => Pages.Add(new RenderedPage(category, model, (TrackingWriter)writer));
        }

        private sealed class TestComposer(IDocumentRenderer renderer) : DocumentationComposer
        {
            protected override IDocumentRenderer Renderer { get; } = renderer;
        }

        private sealed class InMemoryWriterFactory : IDocumentWriterFactory
        {
            public Dictionary<TrackingWriter, IDocumentModel> Writers { get; } = [];

            public TextWriter CreateWriter(IDocumentModel model)
            {
                var writer = new TrackingWriter();
                Writers.Add(writer, model);
                return writer;
            }
        }

        private sealed class TrackingWriter : StringWriter
        {
            public bool IsDisposed { get; private set; }

            protected override void Dispose(bool disposing)
            {
                IsDisposed = true;
                base.Dispose(disposing);
            }
        }
    }
}

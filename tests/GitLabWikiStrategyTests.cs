// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Routing;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class GitLabWikiStrategyTests
    {
        [SetUp]
        public void Setup()
        {
            MetadataProvider.RegisterRuntimeAssemblies(); // To ensure all code references are resolvable
        }

        [TestCase("N:System", ExpectedResult = "System")]
        [TestCase("N:System.Collections.Generic", ExpectedResult = "System.Collections.Generic")]
        [TestCase("T:System.DateTime", ExpectedResult = "System/DateTime")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "System.Collections.Generic/List{T}")]
        [TestCase("T:System.Collections.Generic.List`1.Enumerator", ExpectedResult = "System.Collections.Generic/List{T}.Enumerator")]
        [TestCase("T:System.Collections.Generic.Dictionary`2", ExpectedResult = "System.Collections.Generic/Dictionary{TKey,-TValue}")]
        [TestCase("M:System.Uri.#ctor(System.String)", ExpectedResult = "System/Uri#uristring")]
        [TestCase("M:System.Console.WriteLine", ExpectedResult = "System/Console#writeline")]
        [TestCase("M:System.Console.WriteLine(System.String)", ExpectedResult = "System/Console#writelinestring")]
        [TestCase("M:System.Linq.Enumerable.Repeat``1(``0,System.Int32)", ExpectedResult = "System.Linq/Enumerable#repeattresulttresult-int")]
        [TestCase("M:System.DateTime.op_Addition(System.DateTime,System.TimeSpan)", ExpectedResult = "System/DateTime#additiondatetime-timespan")]
        [TestCase("P:System.Collections.Generic.List`1.Item(System.Int32)", ExpectedResult = "System.Collections.Generic/List{T}#itemint")]
        [TestCase("F:System.DateTime.MaxValue", ExpectedResult = "System/DateTime#maxvalue")]
        [TestCase("F:System.DayOfWeek.Monday", ExpectedResult = null)]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = "System/AppDomain#assemblyload")]
        public string? ResolveAddress_ReturnsExpectedUrl(string cref)
        {
            var strategy = new GitLabWikiStrategy(new GitLabWikiOptions { OmitExtensionInUrls = true });

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("N:System", ExpectedResult = "System.md")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "System.Collections.Generic/List{T}.md")]
        [TestCase("M:System.Console.WriteLine(System.String)", ExpectedResult = "System/Console.md#writelinestring")]
        public string? ResolveAddress_WithDefaultOptions_IncludesExtensionInUrl(string cref)
        {
            var strategy = new GitLabWikiStrategy();

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("N:System", ExpectedResult = "System.md")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "System.Collections.Generic/List{T}.md")]
        [TestCase("M:System.Console.WriteLine(System.String)", ExpectedResult = null)]
        public string? ResolveAddress_ReturnsExpectedFilePath(string cref)
        {
            var strategy = new GitLabWikiStrategy();

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("SimpleTitle", ExpectedResult = "Reference-Guide/SimpleTitle")]
        [TestCase("Title With Spaces", ExpectedResult = "Reference-Guide/Title-With-Spaces")]
        [TestCase("Title-With-Hyphens", ExpectedResult = "Reference-Guide/Title-With-Hyphens")]
        [TestCase("Title?<>:|Special", ExpectedResult = "Reference-Guide/Title_{}__Special")]
        public string? ResolveAddress_Topic_WithParent_ReturnsExpectedUrl(string topicTitle)
        {
            var strategy = new GitLabWikiStrategy(new GitLabWikiOptions { OmitExtensionInUrls = true });
            var topic = MockTopicBuilder.Topic("topic", topicTitle)
                .WithParent("parent-topic", "Reference Guide")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("SimpleTitle", ExpectedResult = "SimpleTitle")]
        [TestCase("Title With Spaces", ExpectedResult = "Title-With-Spaces")]
        [TestCase("Title.With.Dots", ExpectedResult = "Title.With.Dots")]
        [TestCase("Title_With_Underscores", ExpectedResult = "Title_With_Underscores")]
        [TestCase("Title-With-Hyphens", ExpectedResult = "Title-With-Hyphens")]
        [TestCase("List<T>", ExpectedResult = "List{T}")]
        [TestCase("Title*Special", ExpectedResult = "Title_Special")]
        [TestCase("Title?<>:|Special", ExpectedResult = "Title_{}__Special")]
        [TestCase("Title\"Quote", ExpectedResult = "Title_Quote")]
        [TestCase("Title#Slash/Back\\", ExpectedResult = "Title_Slash_Back_")]
        [TestCase("100% Done", ExpectedResult = "100_-Done")]
        [TestCase("MiXeD CaSe", ExpectedResult = "MiXeD-CaSe")]
        public string EncodeWikiPath_ReturnsExpectedPath(string input)
        {
            return GitLabWikiStrategy.EncodeWikiPath(input);
        }

        [Test]
        public void MainTopicIdAndApiTopicId_PlaceMainTopicAtHomePathAndPagesUnderIt()
        {
            var strategy = new GitLabWikiStrategy(new GitLabWikiOptions { MainTopicId = "welcome", ApiTopicId = "api", OmitExtensionInUrls = true });
            using var context = MockHelper.CreateDocumentationContext<MarkdownFormat>(
            [
                MockTopicBuilder.Topic("welcome", "Welcome").Build(),
                MockTopicBuilder.Topic("api", "API Reference").Build(),
                MockTopicBuilder.Topic("guide", "User Guide").Build(),
            ]);
            ((IContextualAddressingStrategy)strategy).InitializeWithContext(context);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(strategy.TryResolveTopicAddress(context.Topics.First(static t => t.Id == "welcome").Source, out var main) ? main.RelativeUrl : null, Is.EqualTo("home"));
                Assert.That(strategy.TryResolveTopicAddress(context.Topics.First(static t => t.Id == "api").Source, out var api) ? api.RelativeUrl : null, Is.EqualTo("home/API-Reference"));
                Assert.That(strategy.TryResolveTopicAddress(context.Topics.First(static t => t.Id == "guide").Source, out var guide) ? guide.RelativeUrl : null, Is.EqualTo("home/User-Guide"));
                Assert.That(strategy.TryResolveAddressByCodeReference("N:System", out var ns) ? ns.RelativeUrl : null, Is.EqualTo("home/API-Reference/System"));
                Assert.That(strategy.TryResolveAddressByCodeReference("T:System.DateTime", out var type) ? type.RelativeUrl : null, Is.EqualTo("home/API-Reference/System/DateTime"));
            }
        }

        [Test]
        public void MainTopicId_TopicNamedLikeHomePage_IsNestedUnderHomePage()
        {
            var strategy = new GitLabWikiStrategy(new GitLabWikiOptions { MainTopicId = "welcome", OmitExtensionInUrls = true });
            using var context = MockHelper.CreateDocumentationContext<MarkdownFormat>(
            [
                MockTopicBuilder.Topic("welcome", "Welcome").Build(),
                MockTopicBuilder.Topic("other", "home").Build(),
            ]);
            ((IContextualAddressingStrategy)strategy).InitializeWithContext(context);

            var other = context.Topics.First(static t => t.Id == "other").Source;

            Assert.That(strategy.TryResolveTopicAddress(other, out var address) ? address.RelativeUrl : null, Is.EqualTo("home/home"));
        }
    }
}

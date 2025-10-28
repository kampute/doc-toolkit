// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Routing;
    using NUnit.Framework;

    [TestFixture]
    public class DotNetApiStrategyTests
    {
        [SetUp]
        public void Setup()
        {
            MetadataProvider.RegisterRuntimeAssemblies(); // To ensure all code references are resolvable
        }

        [TestCase("N:System", ExpectedResult = "api/system.html")]
        [TestCase("N:System.Collections.Generic", ExpectedResult = "api/system.collections.generic.html")]
        [TestCase("T:System.DateTime", ExpectedResult = "api/system.datetime.html")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "api/system.collections.generic.list-1.html")]
        [TestCase("T:System.Collections.Generic.List`1.Enumerator", ExpectedResult = "api/system.collections.generic.list-1.enumerator.html")]
        [TestCase("M:System.Uri.#ctor(System.String)", ExpectedResult = "api/system.uri.-ctor.html#system-uri-ctor(system-string)")]
        [TestCase("M:System.Console.WriteLine", ExpectedResult = "api/system.console.writeline.html#system-console-writeline")]
        [TestCase("M:System.Console.WriteLine(System.String)", ExpectedResult = "api/system.console.writeline.html#system-console-writeline(system-string)")]
        [TestCase("M:System.Linq.Enumerable.Repeat``1(``0,System.Int32)", ExpectedResult = "api/system.linq.enumerable.repeat.html")]
        [TestCase("M:System.DateTime.op_Addition(System.DateTime,System.TimeSpan)", ExpectedResult = "api/system.datetime.op_addition.html")]
        [TestCase("P:System.Collections.Generic.List`1.Count", ExpectedResult = "api/system.collections.generic.list-1.count.html")]
        [TestCase("P:System.Collections.Generic.List`1.Item(System.Int32)", ExpectedResult = "api/system.collections.generic.list-1.item.html")]
        [TestCase("P:System.Collections.Generic.List`1.Enumerator.Current", ExpectedResult = "api/system.collections.generic.list-1.enumerator.current.html")]
        [TestCase("P:System.Collections.Generic.Dictionary`2.System#Collections#Generic#ICollection{System#Collections#Generic#KeyValuePair{TKey,TValue}}#IsReadOnly", ExpectedResult = "api/system.collections.generic.dictionary-2.system-collections-generic-icollection-system-collections-generic-keyvaluepair-tkey-tvalue---isreadonly.html")]
        [TestCase("F:System.DateTime.MaxValue", ExpectedResult = "api/system.datetime.maxvalue.html")]
        [TestCase("F:System.DayOfWeek.Monday", ExpectedResult = null)]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = "api/system.appdomain.assemblyload.html")]
        public string? TryResolveAddress_CodeReference_ReturnsExpectedUrl(string cref)
        {
            var strategy = new DotNetApiStrategy();

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("N:System", ExpectedResult = "api/system.html")]
        [TestCase("N:System.Collections.Generic", ExpectedResult = "api/system.collections.generic.html")]
        [TestCase("T:System.DateTime", ExpectedResult = "api/system.datetime.html")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "api/system.collections.generic.list-1.html")]
        [TestCase("T:System.Collections.Generic.List`1.Enumerator", ExpectedResult = "api/system.collections.generic.list-1.enumerator.html")]
        [TestCase("M:System.Uri.#ctor(System.String)", ExpectedResult = "api/system.uri.-ctor.html")]
        [TestCase("M:System.Console.WriteLine", ExpectedResult = "api/system.console.writeline.html")]
        [TestCase("M:System.Console.WriteLine(System.String)", ExpectedResult = "api/system.console.writeline.html")]
        [TestCase("M:System.Linq.Enumerable.Repeat``1(``0,System.Int32)", ExpectedResult = "api/system.linq.enumerable.repeat.html")]
        [TestCase("M:System.DateTime.op_Addition(System.DateTime,System.TimeSpan)", ExpectedResult = "api/system.datetime.op_addition.html")]
        [TestCase("P:System.Collections.Generic.List`1.Count", ExpectedResult = "api/system.collections.generic.list-1.count.html")]
        [TestCase("P:System.Collections.Generic.List`1.Item(System.Int32)", ExpectedResult = "api/system.collections.generic.list-1.item.html")]
        [TestCase("P:System.Collections.Generic.List`1.Enumerator.Current", ExpectedResult = "api/system.collections.generic.list-1.enumerator.current.html")]
        [TestCase("P:System.Collections.Generic.Dictionary`2.System#Collections#Generic#ICollection{System#Collections#Generic#KeyValuePair{TKey,TValue}}#IsReadOnly", ExpectedResult = "api/system.collections.generic.dictionary-2.system-collections-generic-icollection-system-collections-generic-keyvaluepair-tkey-tvalue---isreadonly.html")]
        [TestCase("F:System.DateTime.MaxValue", ExpectedResult = "api/system.datetime.maxvalue.html")]
        [TestCase("F:System.DayOfWeek.Monday", ExpectedResult = null)]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = "api/system.appdomain.assemblyload.html")]
        public string? TryResolveAddress_CodeReference_ReturnsExpectedFilePath(string cref)
        {
            var strategy = new DotNetApiStrategy();

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("Frequently Asked Questions", ExpectedResult = "frequently-asked-questions.html")]
        [TestCase("v1-to-v2", ExpectedResult = "v1-to-v2.html")]
        [TestCase("Files & Streams", ExpectedResult = "files-streams.html")]
        public string? TryResolveAddress_CodeReference_Topic_WithoutParent_ReturnsExpectedUrl(string topicId)
        {
            var strategy = new DotNetApiStrategy();
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("Frequently Asked Questions", ExpectedResult = "frequently-asked-questions.html")]
        [TestCase("v1-to-v2", ExpectedResult = "v1-to-v2.html")]
        [TestCase("Files & Streams", ExpectedResult = "files-streams.html")]
        public string? TryResolveAddress_CodeReference_Topic_WithoutParent_ReturnsExpectedFilePath(string topicId)
        {
            var strategy = new DotNetApiStrategy();
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("API Reference", ExpectedResult = "reference-guide/api-reference.html")]
        [TestCase("v2_Migration", ExpectedResult = "reference-guide/v2_migration.html")]
        public string? TryResolveAddress_CodeReference_Topic_WithParent_ReturnsExpectedUrl(string topicId)
        {
            var strategy = new DotNetApiStrategy();
            var topic = MockTopicBuilder.Topic(topicId)
                .WithParent("Reference Guide")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("API Reference", ExpectedResult = "reference-guide/api-reference.html")]
        [TestCase("v2_Migration", ExpectedResult = "reference-guide/v2_migration.html")]
        public string? TryResolveAddress_CodeReference_Topic_WithParent_ReturnsExpectedFilePath(string topicId)
        {
            var strategy = new DotNetApiStrategy();
            var topic = MockTopicBuilder.Topic(topicId)
                .WithParent("Reference Guide")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("User Guide", ExpectedResult = "user-guide/index.html")]
        [TestCase("Developer Reference", ExpectedResult = "developer-reference/index.html")]
        public string? TryResolveAddress_CodeReference_Topic_WithSubtopics_ReturnsExpectedUrl(string topicId)
        {
            var strategy = new DotNetApiStrategy();
            var topic = MockTopicBuilder.Topic(topicId)
                .WithChildren("Getting Started", "Advanced Topics")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("User Guide", ExpectedResult = "user-guide/index.html")]
        [TestCase("Developer Reference", ExpectedResult = "developer-reference/index.html")]
        public string? TryResolveAddress_CodeReference_Topic_WithSubtopics_ReturnsExpectedFilePath(string topicId)
        {
            var strategy = new DotNetApiStrategy();
            var topic = MockTopicBuilder.Topic(topicId)
                .WithChildren("Getting Started", "Advanced Topics")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("API Overview", ExpectedResult = "documentation/api-overview/index.html")]
        [TestCase("Best Practices", ExpectedResult = "documentation/best-practices/index.html")]
        public string? TryResolveAddress_CodeReference_Topic_WithParentAndSubtopics_ReturnsExpectedUrl(string topicId)
        {
            var strategy = new DotNetApiStrategy();
            var topic = MockTopicBuilder.Topic(topicId)
                .WithParent("Documentation")
                .WithChildren("Examples", "Tutorials")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("API Overview", ExpectedResult = "documentation/api-overview/index.html")]
        [TestCase("Best Practices", ExpectedResult = "documentation/best-practices/index.html")]
        public string? TryResolveAddress_CodeReference_Topic_WithParentAndSubtopics_ReturnsExpectedFilePath(string topicId)
        {
            var strategy = new DotNetApiStrategy();
            var topic = MockTopicBuilder.Topic(topicId)
                .WithParent("Documentation")
                .WithChildren("Examples", "Tutorials")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("T:System.DateTime", ExpectedResult = "reference/system.datetime")]
        [TestCase("N:System", ExpectedResult = "reference/system")]
        public string? TryResolveAddress_CodeReference_WithOmitExtensionInUrls_ReturnsExpectedUrl(string cref)
        {
            var options = new DotNetApiOptions
            {
                ApiPath = "reference",
                OmitExtensionInUrls = true
            };
            var strategy = new DotNetApiStrategy(options);

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("T:System.DateTime", ExpectedResult = "docs/system.datetime.xhtml")]
        [TestCase("N:System", ExpectedResult = "docs/system.xhtml")]
        public string? TryResolveAddress_CodeReference_WithCustomExtension_ReturnsExpectedUrl(string cref)
        {
            var options = new DotNetApiOptions
            {
                ApiPath = "docs",
                FileExtension = ".xhtml",
                OmitExtensionInUrls = false
            };
            var strategy = new DotNetApiStrategy(options);

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("T:System.DateTime", ExpectedResult = "docs/system.datetime.xhtml")]
        [TestCase("N:System", ExpectedResult = "docs/system.xhtml")]
        public string? TryResolveAddress_CodeReference_WithCustomExtension_ReturnsExpectedFilePath(string cref)
        {
            var options = new DotNetApiOptions
            {
                ApiPath = "docs",
                FileExtension = ".xhtml",
                OmitExtensionInUrls = true
            };
            var strategy = new DotNetApiStrategy(options);

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("Getting Started", ExpectedResult = "guides/getting-started.xhtml")]
        [TestCase("Advanced Topics", ExpectedResult = "guides/advanced-topics.xhtml")]
        public string? TryResolveAddress_CodeReference_WithCustomTopicPath_ReturnsExpectedFilePath(string topicId)
        {
            var options = new DotNetApiOptions
            {
                TopicPath = "guides",
                FileExtension = ".xhtml"
            };
            var strategy = new DotNetApiStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("Introduction", ExpectedResult = "guides/introduction")]
        [TestCase("Best Practices", ExpectedResult = "guides/best-practices")]
        public string? TryResolveAddress_CodeReference_WithCustomTopicPathAndOmitExtension_ReturnsExpectedUrl(string topicId)
        {
            var options = new DotNetApiOptions
            {
                TopicPath = "guides",
                OmitExtensionInUrls = true
            };
            var strategy = new DotNetApiStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("Overview", ExpectedResult = "docs/overview/main.html")]
        [TestCase("Tutorial", ExpectedResult = "docs/tutorial/main.html")]
        public string? TryResolveAddress_CodeReference_WithCustomIndexTopicName_ReturnsExpectedFilePath(string topicId)
        {
            var options = new DotNetApiOptions
            {
                TopicPath = "docs",
                IndexTopicName = "main"
            };
            var strategy = new DotNetApiStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId)
                .WithChildren("Examples")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("README", ExpectedResult = "index.xhtml")]
        [TestCase("API", ExpectedResult = "api/index.xhtml")]
        [TestCase("Getting Started", ExpectedResult = "docs/index.xhtml")]
        [TestCase("Best Practices", ExpectedResult = "guides/best-practices.xhtml")]
        public string? TryResolveAddress_CodeReference_WithPinnedTopics_ReturnsExpectedUrl(string topicId)
        {
            var options = new DotNetApiOptions
            {
                TopicPath = "guides",
                FileExtension = ".xhtml"
            };
            options.AddPinnedTopic("README", "");
            options.AddPinnedTopic("API", "api");
            options.AddPinnedTopic("Getting Started", "docs");

            var strategy = new DotNetApiStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("README", ExpectedResult = "index.xhtml")]
        [TestCase("API", ExpectedResult = "api/index.xhtml")]
        [TestCase("Getting Started", ExpectedResult = "docs/index.xhtml")]
        [TestCase("Best Practices", ExpectedResult = "guides/best-practices.xhtml")]
        public string? TryResolveAddress_CodeReference_WithPinnedTopics_ReturnsExpectedFilePath(string topicId)
        {
            var options = new DotNetApiOptions
            {
                TopicPath = "guides",
                FileExtension = ".xhtml"
            };
            options.AddPinnedTopic("README", "");
            options.AddPinnedTopic("API", "api");
            options.AddPinnedTopic("Getting Started", "docs");

            var strategy = new DotNetApiStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("Overview", ExpectedResult = "api/overview.html")]
        [TestCase("Examples", ExpectedResult = "api/examples.html")]
        public string? TryResolveAddress_CodeReference_PinnedTopicWithSubtopics_ReturnsExpectedUrl(string subtopicId)
        {
            var options = new DotNetApiOptions();
            options.AddPinnedTopic("API", "api");

            var strategy = new DotNetApiStrategy(options);
            var subtopic = MockTopicBuilder.Topic(subtopicId)
                .WithParent("API")
                .Build();

            return strategy.TryResolveTopicAddress(subtopic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("Overview", ExpectedResult = "api/overview.html")]
        [TestCase("Examples", ExpectedResult = "api/examples.html")]
        public string? TryResolveAddress_CodeReference_PinnedTopicWithSubtopics_ReturnsExpectedFilePath(string subtopicId)
        {
            var options = new DotNetApiOptions();
            options.AddPinnedTopic("API", "api");

            var strategy = new DotNetApiStrategy(options);
            var subtopic = MockTopicBuilder.Topic(subtopicId)
                .WithParent("API")
                .Build();

            return strategy.TryResolveTopicAddress(subtopic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("README", ExpectedResult = "index")]
        [TestCase("API Documentation", ExpectedResult = "docs/index")]
        public string? TryResolveAddress_CodeReference_PinnedTopicWithOmitExtension_ReturnsExpectedUrl(string topicId)
        {
            var options = new DotNetApiOptions
            {
                OmitExtensionInUrls = true
            };
            options.AddPinnedTopic("README", "");
            options.AddPinnedTopic("API Documentation", "docs");

            var strategy = new DotNetApiStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("Introduction", ExpectedResult = "docs/introduction/main.xhtml")]
        [TestCase("Tutorial", ExpectedResult = "docs/tutorial/main.xhtml")]
        public string? TryResolveAddress_CodeReference_PinnedTopicWithCustomIndexName_ReturnsExpectedFilePath(string topicId)
        {
            var options = new DotNetApiOptions
            {
                FileExtension = ".xhtml",
                IndexTopicName = "main"
            };
            options.AddPinnedTopic("Documentation", "docs");

            var strategy = new DotNetApiStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId)
                .WithParent("Documentation")
                .WithChildren("Examples")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }
    }
}

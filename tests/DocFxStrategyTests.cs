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
    public class DocFxStrategyTests
    {
        [SetUp]
        public void Setup()
        {
            MetadataProvider.RegisterRuntimeAssemblies(); // To ensure all code references are resolvable
        }

        [TestCase("N:System", ExpectedResult = "api/System.html")]
        [TestCase("N:System.Collections.Generic", ExpectedResult = "api/System.Collections.Generic.html")]
        [TestCase("T:System.DateTime", ExpectedResult = "api/System.DateTime.html")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "api/System.Collections.Generic.List_1.html")]
        [TestCase("T:System.Collections.Generic.List`1.Enumerator", ExpectedResult = "api/System.Collections.Generic.List_1.Enumerator.html")]
        [TestCase("M:System.Uri.#ctor(System.String)", ExpectedResult = "api/System.Uri.html#System_Uri__ctor_System_String_")]
        [TestCase("M:System.Console.WriteLine", ExpectedResult = "api/System.Console.html#System_Console_WriteLine")]
        [TestCase("M:System.Console.WriteLine(System.String)", ExpectedResult = "api/System.Console.html#System_Console_WriteLine_System_String_")]
        [TestCase("M:System.Linq.Enumerable.Repeat``1(``0,System.Int32)", ExpectedResult = "api/System.Linq.Enumerable.html#System_Linq_Enumerable_Repeat__1___0_System_Int32_")]
        [TestCase("M:System.DateTime.op_Addition(System.DateTime,System.TimeSpan)", ExpectedResult = "api/System.DateTime.html#System_DateTime_op_Addition_System_DateTime_System_TimeSpan_")]
        [TestCase("P:System.Collections.Generic.List`1.Count", ExpectedResult = "api/System.Collections.Generic.List_1.html#System_Collections_Generic_List_1_Count")]
        [TestCase("P:System.Collections.Generic.List`1.Item(System.Int32)", ExpectedResult = "api/System.Collections.Generic.List_1.html#System_Collections_Generic_List_1_Item_System_Int32_")]
        [TestCase("P:System.Collections.Generic.List`1.Enumerator.Current", ExpectedResult = "api/System.Collections.Generic.List_1.Enumerator.html#System_Collections_Generic_List_1_Enumerator_Current")]
        [TestCase("F:System.DateTime.MaxValue", ExpectedResult = "api/System.DateTime.html#System_DateTime_MaxValue")]
        [TestCase("F:System.DayOfWeek.Monday", ExpectedResult = null)]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = "api/System.AppDomain.html#System_AppDomain_AssemblyLoad")]
        public string? ResolveAddress_ReturnsExpectedUrl(string cref)
        {
            var strategy = new DocFxStrategy();

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("N:System", ExpectedResult = "api/System.html")]
        [TestCase("N:System.Collections.Generic", ExpectedResult = "api/System.Collections.Generic.html")]
        [TestCase("T:System.DateTime", ExpectedResult = "api/System.DateTime.html")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "api/System.Collections.Generic.List_1.html")]
        [TestCase("T:System.Collections.Generic.List`1.Enumerator", ExpectedResult = "api/System.Collections.Generic.List_1.Enumerator.html")]
        [TestCase("M:System.Uri.#ctor(System.String)", ExpectedResult = null)]
        [TestCase("M:System.Console.WriteLine", ExpectedResult = null)]
        [TestCase("M:System.Console.WriteLine(System.String)", ExpectedResult = null)]
        [TestCase("M:System.Linq.Enumerable.Repeat``1(``0,System.Int32)", ExpectedResult = null)]
        [TestCase("M:System.DateTime.op_Addition(System.DateTime,System.TimeSpan)", ExpectedResult = null)]
        [TestCase("P:System.Collections.Generic.List`1.Count", ExpectedResult = null)]
        [TestCase("P:System.Collections.Generic.List`1.Item(System.Int32)", ExpectedResult = null)]
        [TestCase("P:System.Collections.Generic.List`1.Enumerator.Current", ExpectedResult = null)]
        [TestCase("F:System.DateTime.MaxValue", ExpectedResult = null)]
        [TestCase("F:System.DayOfWeek.Monday", ExpectedResult = null)]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = null)]
        public string? ResolveAddress_ReturnsExpectedFilePath(string cref)
        {
            var strategy = new DocFxStrategy();

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("Frequently Asked Questions", ExpectedResult = "Frequently_Asked_Questions.html")]
        [TestCase("v1-to-v2", ExpectedResult = "v1-to-v2.html")]
        [TestCase("Files & Streams", ExpectedResult = "Files_Streams.html")]
        public string? ResolveAddress_Topic_WithoutParent_ReturnsExpectedUrl(string topicId)
        {
            var strategy = new DocFxStrategy();
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("Frequently Asked Questions", ExpectedResult = "Frequently_Asked_Questions.html")]
        [TestCase("v1-to-v2", ExpectedResult = "v1-to-v2.html")]
        [TestCase("Files & Streams", ExpectedResult = "Files_Streams.html")]
        public string? ResolveAddress_Topic_WithoutParent_ReturnsExpectedFilePath(string topicId)
        {
            var strategy = new DocFxStrategy();
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("API Reference", ExpectedResult = "Reference_Guide/API_Reference.html")]
        [TestCase("v2-Migration", ExpectedResult = "Reference_Guide/v2-Migration.html")]
        public string? ResolveAddress_Topic_WithParent_ReturnsExpectedUrl(string topicId)
        {
            var strategy = new DocFxStrategy();
            var topic = MockTopicBuilder.Topic(topicId)
                .WithParent("Reference Guide")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("API Reference", ExpectedResult = "Reference_Guide/API_Reference.html")]
        [TestCase("v2-Migration", ExpectedResult = "Reference_Guide/v2-Migration.html")]
        public string? ResolveAddress_Topic_WithParent_ReturnsExpectedFilePath(string topicId)
        {
            var strategy = new DocFxStrategy();
            var topic = MockTopicBuilder.Topic(topicId)
                .WithParent("Reference Guide")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("User Guide", ExpectedResult = "User_Guide/index.html")]
        [TestCase("Developer Reference", ExpectedResult = "Developer_Reference/index.html")]
        public string? ResolveAddress_Topic_WithSubtopics_ReturnsExpectedUrl(string topicId)
        {
            var strategy = new DocFxStrategy();
            var topic = MockTopicBuilder.Topic(topicId)
                .WithChildren("Getting Started", "Advanced Topics")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("User Guide", ExpectedResult = "User_Guide/index.html")]
        [TestCase("Developer Reference", ExpectedResult = "Developer_Reference/index.html")]
        public string? ResolveAddress_Topic_WithSubtopics_ReturnsExpectedFilePath(string topicId)
        {
            var strategy = new DocFxStrategy();
            var topic = MockTopicBuilder.Topic(topicId)
                .WithChildren("Getting Started", "Advanced Topics")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("API Overview", ExpectedResult = "Documentation/API_Overview/index.html")]
        [TestCase("Best Practices", ExpectedResult = "Documentation/Best_Practices/index.html")]
        public string? ResolveAddress_Topic_WithParentAndSubtopics_ReturnsExpectedUrl(string topicId)
        {
            var strategy = new DocFxStrategy();
            var topic = MockTopicBuilder.Topic(topicId)
                .WithParent("Documentation")
                .WithChildren("Examples", "Tutorials")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("API Overview", ExpectedResult = "Documentation/API_Overview/index.html")]
        [TestCase("Best Practices", ExpectedResult = "Documentation/Best_Practices/index.html")]
        public string? ResolveAddress_Topic_WithParentAndSubtopics_ReturnsExpectedFilePath(string topicId)
        {
            var strategy = new DocFxStrategy();
            var topic = MockTopicBuilder.Topic(topicId)
                .WithParent("Documentation")
                .WithChildren("Examples", "Tutorials")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("T:System.DateTime", ExpectedResult = "reference/System.DateTime")]
        [TestCase("N:System", ExpectedResult = "reference/System")]
        public string? ResolveAddress_WithOmitExtensionInUrls_ReturnsExpectedUrl(string cref)
        {
            var options = new DocFxOptions
            {
                ApiPath = "reference",
                OmitExtensionInUrls = true
            };
            var strategy = new DocFxStrategy(options);

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("T:System.DateTime", ExpectedResult = "docs/System.DateTime.xhtml")]
        [TestCase("N:System", ExpectedResult = "docs/System.xhtml")]
        public string? ResolveAddress_WithCustomExtension_ReturnsExpectedUrl(string cref)
        {
            var options = new DocFxOptions
            {
                ApiPath = "docs",
                FileExtension = ".xhtml",
                OmitExtensionInUrls = false
            };
            var strategy = new DocFxStrategy(options);

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("T:System.DateTime", ExpectedResult = "docs/System.DateTime.xhtml")]
        [TestCase("N:System", ExpectedResult = "docs/System.xhtml")]
        public string? ResolveAddress_WithCustomExtension_ReturnsExpectedFilePath(string cref)
        {
            var options = new DocFxOptions
            {
                ApiPath = "docs",
                FileExtension = ".xhtml",
                OmitExtensionInUrls = true
            };
            var strategy = new DocFxStrategy(options);

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("Getting Started", ExpectedResult = "Tutorials/Getting_Started.xhtml")]
        [TestCase("Advanced Topics", ExpectedResult = "Tutorials/Advanced_Topics.xhtml")]
        public string? ResolveAddress_WithCustomTopicPath_ReturnsExpectedFilePath(string topicId)
        {
            var options = new DocFxOptions
            {
                TopicPath = "Tutorials",
                FileExtension = ".xhtml"
            };
            var strategy = new DocFxStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("Introduction", ExpectedResult = "Guides/Introduction")]
        [TestCase("Best Practices", ExpectedResult = "Guides/Best_Practices")]
        public string? ResolveAddress_WithCustomTopicPathAndOmitExtension_ReturnsExpectedUrl(string topicId)
        {
            var options = new DocFxOptions
            {
                TopicPath = "Guides",
                OmitExtensionInUrls = true
            };
            var strategy = new DocFxStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("Overview", ExpectedResult = "docs/Overview/toc.html")]
        [TestCase("Tutorial", ExpectedResult = "docs/Tutorial/toc.html")]
        public string? ResolveAddress_WithCustomIndexTopicName_ReturnsExpectedFilePath(string topicId)
        {
            var options = new DocFxOptions
            {
                TopicPath = "docs",
                IndexTopicName = "toc"
            };
            var strategy = new DocFxStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId)
                .WithChildren("Examples")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("README", ExpectedResult = "index.xhtml")]
        [TestCase("API", ExpectedResult = "api/index.xhtml")]
        [TestCase("Getting Started", ExpectedResult = "docs/index.xhtml")]
        [TestCase("Best Practices", ExpectedResult = "Guides/Best_Practices.xhtml")]
        public string? ResolveAddress_WithPinnedTopics_ReturnsExpectedUrl(string topicId)
        {
            var options = new DocFxOptions
            {
                TopicPath = "Guides",
                FileExtension = ".xhtml"
            };
            options.AddPinnedTopic("README", "");
            options.AddPinnedTopic("API", "api");
            options.AddPinnedTopic("Getting Started", "docs");

            var strategy = new DocFxStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("README", ExpectedResult = "index.xhtml")]
        [TestCase("API", ExpectedResult = "api/index.xhtml")]
        [TestCase("Getting Started", ExpectedResult = "docs/index.xhtml")]
        [TestCase("Best Practices", ExpectedResult = "Guides/Best_Practices.xhtml")]
        public string? ResolveAddress_WithPinnedTopics_ReturnsExpectedFilePath(string topicId)
        {
            var options = new DocFxOptions
            {
                TopicPath = "Guides",
                FileExtension = ".xhtml"
            };
            options.AddPinnedTopic("README", "");
            options.AddPinnedTopic("API", "api");
            options.AddPinnedTopic("Getting Started", "docs");

            var strategy = new DocFxStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("Overview", ExpectedResult = "api/Overview.html")]
        [TestCase("Examples", ExpectedResult = "api/Examples.html")]
        public string? ResolveAddress_PinnedTopicWithSubtopics_ReturnsExpectedUrl(string subtopicId)
        {
            var options = new DocFxOptions();
            options.AddPinnedTopic("API", "api");

            var strategy = new DocFxStrategy(options);
            var subtopic = MockTopicBuilder.Topic(subtopicId)
                .WithParent("API")
                .Build();

            return strategy.TryResolveTopicAddress(subtopic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("Overview", ExpectedResult = "api/Overview.html")]
        [TestCase("Examples", ExpectedResult = "api/Examples.html")]
        public string? ResolveAddress_PinnedTopicWithSubtopics_ReturnsExpectedFilePath(string subtopicId)
        {
            var options = new DocFxOptions();
            options.AddPinnedTopic("API", "api");

            var strategy = new DocFxStrategy(options);
            var subtopic = MockTopicBuilder.Topic(subtopicId)
                .WithParent("API")
                .Build();

            return strategy.TryResolveTopicAddress(subtopic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("README", ExpectedResult = "index")]
        [TestCase("API Documentation", ExpectedResult = "docs/index")]
        public string? ResolveAddress_PinnedTopicWithOmitExtension_ReturnsExpectedUrl(string topicId)
        {
            var options = new DocFxOptions
            {
                OmitExtensionInUrls = true
            };
            options.AddPinnedTopic("README", "");
            options.AddPinnedTopic("API Documentation", "docs");

            var strategy = new DocFxStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("Introduction", ExpectedResult = "docs/Introduction/main.xhtml")]
        [TestCase("Tutorial", ExpectedResult = "docs/Tutorial/main.xhtml")]
        public string? ResolveAddress_PinnedTopicWithCustomIndexName_ReturnsExpectedFilePath(string topicId)
        {
            var options = new DocFxOptions
            {
                FileExtension = ".xhtml",
                IndexTopicName = "main"
            };
            options.AddPinnedTopic("Documentation", "docs");

            var strategy = new DocFxStrategy(options);
            var topic = MockTopicBuilder.Topic(topicId)
                .WithParent("Documentation")
                .WithChildren("Examples")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }
    }
}

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
    public class DevOpsWikiStrategyTests
    {
        [SetUp]
        public void Setup()
        {
            MetadataProvider.RegisterRuntimeAssemblies(); // To ensure all code references are resolvable
        }

        [TestCase("N:System", ExpectedResult = "System")]
        [TestCase("N:System.Collections.Generic", ExpectedResult = "System.Collections.Generic")]
        [TestCase("T:System.DateTime", ExpectedResult = "System/DateTime")]
        [TestCase("T:System.Action", ExpectedResult = "System/Action")]
        [TestCase("T:System.DayOfWeek", ExpectedResult = "System/DayOfWeek")]
        [TestCase("T:System.Collections.Generic.ICollection`1", ExpectedResult = "System.Collections.Generic/ICollection%3CT%3E")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "System.Collections.Generic/List%3CT%3E")]
        [TestCase("T:System.Collections.Generic.List`1.Enumerator", ExpectedResult = "System.Collections.Generic/List%3CT%3E.Enumerator")]
        [TestCase("M:System.Uri.#ctor(System.String)", ExpectedResult = "System/Uri#uri(string)")]
        [TestCase("M:System.Console.WriteLine", ExpectedResult = "System/Console#writeline()")]
        [TestCase("M:System.Console.WriteLine(System.String)", ExpectedResult = "System/Console#writeline(string)")]
        [TestCase("M:System.Linq.Enumerable.Repeat``1(``0,System.Int32)", ExpectedResult = "System.Linq/Enumerable#repeat%3Ctresult%3E(tresult,-int)")]
        [TestCase("M:System.DateTime.op_Addition(System.DateTime,System.TimeSpan)", ExpectedResult = "System/DateTime#addition(datetime,-timespan)")]
        [TestCase("P:System.Collections.Generic.List`1.Count", ExpectedResult = "System.Collections.Generic/List%3CT%3E#count")]
        [TestCase("P:System.Collections.Generic.List`1.Item(System.Int32)", ExpectedResult = "System.Collections.Generic/List%3CT%3E#item[int]")]
        [TestCase("P:System.Collections.Generic.List`1.Enumerator.Current", ExpectedResult = "System.Collections.Generic/List%3CT%3E.Enumerator#current")]
        [TestCase("F:System.DateTime.MaxValue", ExpectedResult = "System/DateTime#maxvalue")]
        [TestCase("F:System.DayOfWeek.Monday", ExpectedResult = null)]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = "System/AppDomain#assemblyload")]
        public string? ResolveAddress_ReturnsExpectedUrl(string cref)
        {
            var strategy = new DevOpsWikiStrategy(new DevOpsWikiOptions { OmitExtensionInUrls = true });

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("N:System", ExpectedResult = "System.md")]
        [TestCase("N:System.Collections.Generic", ExpectedResult = "System.Collections.Generic.md")]
        [TestCase("T:System.DateTime", ExpectedResult = "System/DateTime.md")]
        [TestCase("T:System.Action", ExpectedResult = "System/Action.md")]
        [TestCase("T:System.DayOfWeek", ExpectedResult = "System/DayOfWeek.md")]
        [TestCase("T:System.Collections.Generic.ICollection`1", ExpectedResult = "System.Collections.Generic/ICollection%3CT%3E.md")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "System.Collections.Generic/List%3CT%3E.md")]
        [TestCase("T:System.Collections.Generic.List`1.Enumerator", ExpectedResult = "System.Collections.Generic/List%3CT%3E.Enumerator.md")]
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
            var strategy = new DevOpsWikiStrategy(new DevOpsWikiOptions { OmitExtensionInUrls = true });

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeFilePath : null;
        }
        [TestCase("SimpleTitle", ExpectedResult = "SimpleTitle")]
        [TestCase("Title With Spaces", ExpectedResult = "Title-With-Spaces")]
        [TestCase("Title.With.Dots", ExpectedResult = "Title.With.Dots")]
        [TestCase("Title_With_Underscores", ExpectedResult = "Title_With_Underscores")]
        [TestCase("Title-With-Hyphens", ExpectedResult = "Title%2DWith%2DHyphens")]
        [TestCase("Title*Special", ExpectedResult = "Title%2ASpecial")]
        [TestCase("Title?<>:|Special", ExpectedResult = "Title%3F%3C%3E%3A%7CSpecial")]
        [TestCase("Title\"Quote", ExpectedResult = "Title%22Quote")]
        public string? ResolveAddress_Topic_WithoutParent_ReturnsExpectedUrl(string topicTitle)
        {
            var strategy = new DevOpsWikiStrategy(new DevOpsWikiOptions { OmitExtensionInUrls = true });
            var topic = MockTopicBuilder.Topic("topic", topicTitle).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("SimpleTitle", ExpectedResult = "SimpleTitle.md")]
        [TestCase("Title With Spaces", ExpectedResult = "Title-With-Spaces.md")]
        [TestCase("Title.With.Dots", ExpectedResult = "Title.With.Dots.md")]
        [TestCase("Title_With_Underscores", ExpectedResult = "Title_With_Underscores.md")]
        [TestCase("Title-With-Hyphens", ExpectedResult = "Title%2DWith%2DHyphens.md")]
        [TestCase("Title*Special", ExpectedResult = "Title%2ASpecial.md")]
        [TestCase("Title?<>:|Special", ExpectedResult = "Title%3F%3C%3E%3A%7CSpecial.md")]
        [TestCase("Title\"Quote\"", ExpectedResult = "Title%22Quote%22.md")]
        public string? ResolveAddress_Topic_WithoutParent_ReturnsExpectedFilePath(string topicTitle)
        {
            var strategy = new DevOpsWikiStrategy(new DevOpsWikiOptions { OmitExtensionInUrls = true });
            var topic = MockTopicBuilder.Topic("topic", topicTitle).Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("SimpleTitle", ExpectedResult = "Reference-Guide/SimpleTitle")]
        [TestCase("Title With Spaces", ExpectedResult = "Reference-Guide/Title-With-Spaces")]
        [TestCase("Title.With.Dots", ExpectedResult = "Reference-Guide/Title.With.Dots")]
        [TestCase("Title_With_Underscores", ExpectedResult = "Reference-Guide/Title_With_Underscores")]
        [TestCase("Title-With-Hyphens", ExpectedResult = "Reference-Guide/Title%2DWith%2DHyphens")]
        [TestCase("Title*Special", ExpectedResult = "Reference-Guide/Title%2ASpecial")]
        [TestCase("Title?<>:|Special", ExpectedResult = "Reference-Guide/Title%3F%3C%3E%3A%7CSpecial")]
        [TestCase("Title\"Quote", ExpectedResult = "Reference-Guide/Title%22Quote")]
        public string? ResolveAddress_Topic_WithParent_ReturnsExpectedUrl(string topicTitle)
        {
            var strategy = new DevOpsWikiStrategy(new DevOpsWikiOptions { OmitExtensionInUrls = true });
            var topic = MockTopicBuilder.Topic("topic", topicTitle)
                .WithParent("parent-topic", "Reference Guide")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("SimpleTitle", ExpectedResult = "Reference-Guide/SimpleTitle.md")]
        [TestCase("Title With Spaces", ExpectedResult = "Reference-Guide/Title-With-Spaces.md")]
        [TestCase("Title.With.Dots", ExpectedResult = "Reference-Guide/Title.With.Dots.md")]
        [TestCase("Title_With_Underscores", ExpectedResult = "Reference-Guide/Title_With_Underscores.md")]
        [TestCase("Title-With-Hyphens", ExpectedResult = "Reference-Guide/Title%2DWith%2DHyphens.md")]
        [TestCase("Title*Special", ExpectedResult = "Reference-Guide/Title%2ASpecial.md")]
        [TestCase("Title?<>:|Special", ExpectedResult = "Reference-Guide/Title%3F%3C%3E%3A%7CSpecial.md")]
        [TestCase("Title\"Quote\"", ExpectedResult = "Reference-Guide/Title%22Quote%22.md")]
        public string? ResolveAddress_Topic_WithParent_ReturnsExpectedFilePath(string topicTitle)
        {
            var strategy = new DevOpsWikiStrategy(new DevOpsWikiOptions { OmitExtensionInUrls = true });
            var topic = MockTopicBuilder.Topic("topic", topicTitle)
                .WithParent("parent-topic", "Reference Guide")
                .Build();

            return strategy.TryResolveTopicAddress(topic, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("T:System.DateTime", ExpectedResult = "System/DateTime.md")]
        [TestCase("N:System", ExpectedResult = "System.md")]
        public string? ResolveAddress_WithExtensionInUrls_ReturnsExpectedUrl(string cref)
        {
            var strategy = new DevOpsWikiStrategy(new DevOpsWikiOptions { OmitExtensionInUrls = false });

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("T:System.DateTime", ExpectedResult = "System/DateTime.wiki")]
        [TestCase("N:System", ExpectedResult = "System.wiki")]
        public string? ResolveAddress_WithCustomExtension_ReturnsExpectedUrl(string cref)
        {
            var options = new DevOpsWikiOptions
            {
                FileExtension = ".wiki",
                OmitExtensionInUrls = false
            };
            var strategy = new DevOpsWikiStrategy(options);

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeUrl : null;
        }

        [TestCase("T:System.DateTime", ExpectedResult = "System/DateTime.wiki")]
        [TestCase("N:System", ExpectedResult = "System.wiki")]
        public string? ResolveAddress_WithCustomExtension_ReturnsExpectedFilePath(string cref)
        {
            var options = new DevOpsWikiOptions
            {
                FileExtension = ".wiki",
                OmitExtensionInUrls = true
            };
            var strategy = new DevOpsWikiStrategy(options);

            return strategy.TryResolveAddressByCodeReference(cref, out var address) ? address.RelativeFilePath : null;
        }

        [TestCase("SimpleTitle", ExpectedResult = "SimpleTitle")]
        [TestCase("Title With Spaces", ExpectedResult = "Title-With-Spaces")]
        [TestCase("Title.With.Dots", ExpectedResult = "Title.With.Dots")]
        [TestCase("Title_With_Underscores", ExpectedResult = "Title_With_Underscores")]
        [TestCase("Title-With-Hyphens", ExpectedResult = "Title%2DWith%2DHyphens")]
        [TestCase("Title*Special", ExpectedResult = "Title%2ASpecial")]
        [TestCase("Title?<>:|Special", ExpectedResult = "Title%3F%3C%3E%3A%7CSpecial")]
        [TestCase("Title\"Quote", ExpectedResult = "Title%22Quote")]
        [TestCase("Title#Slash/Back\\", ExpectedResult = "Title_Slash_Back_")]
        [TestCase("MiXeD CaSe", ExpectedResult = "MiXeD-CaSe")]
        [TestCase("A-B_C#D/E\\F", ExpectedResult = "A%2DB_C_D_E_F")]
        public string EncodeWikiPath_ReturnsExpectedPath(string input)
        {
            return DevOpsWikiStrategy.EncodeWikiPath(input);
        }

        [TestCase("SimpleTitle", ExpectedResult = "simpletitle")]
        [TestCase("Title With Spaces", ExpectedResult = "title-with-spaces")]
        [TestCase("Title.With.Dots", ExpectedResult = "title.with.dots")]
        [TestCase("Title_With_Underscores", ExpectedResult = "title_with_underscores")]
        [TestCase("Title-With-Hyphens", ExpectedResult = "title%2Dwith%2Dhyphens")]
        [TestCase("Title*Special", ExpectedResult = "title%2Aspecial")]
        [TestCase("Title?<>:|Special", ExpectedResult = "title%3F%3C%3E%3A%7Cspecial")]
        [TestCase("Title\"Quote", ExpectedResult = "title%22quote")]
        [TestCase("Title#Slash/Back\\", ExpectedResult = "title_slash_back_")]
        [TestCase("MiXeD CaSe", ExpectedResult = "mixed-case")]
        [TestCase("A-B_C#D/E\\F", ExpectedResult = "a%2Db_c_d_e_f")]
        public string EncodeWikiFragmentIdentifier_ReturnsExpectedFragment(string input)
        {
            return DevOpsWikiStrategy.EncodeWikiFragmentIdentifier(input);
        }
    }
}

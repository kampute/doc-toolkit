// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Routing
{
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Routing;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class SearchBasedApiDocUrlResolverTests
    {
        [SetUp]
        public void Setup()
        {
            MetadataProvider.RegisterRuntimeAssemblies(); // To ensure all code references are resolvable
        }

        [TestCase("N:System", ExpectedResult = "https://example.com/?q=C%23+System")]
        [TestCase("N:System.Collections.Generic", ExpectedResult = "https://example.com/?q=C%23+System.Collections.Generic")]
        [TestCase("T:System.DateTime", ExpectedResult = "https://example.com/?q=C%23+System.DateTime")]
        [TestCase("T:System.Collections.Generic.List`1", ExpectedResult = "https://example.com/?q=C%23+System.Collections.Generic.List%3CT%3E")]
        [TestCase("T:System.Collections.Generic.List`1.Enumerator", ExpectedResult = "https://example.com/?q=C%23+System.Collections.Generic.List%3CT%3E.Enumerator")]
        [TestCase("M:System.Uri.#ctor(System.String)", ExpectedResult = "https://example.com/?q=C%23+System.Uri(string)")]
        [TestCase("M:System.Console.WriteLine", ExpectedResult = "https://example.com/?q=C%23+System.Console.WriteLine()")]
        [TestCase("M:System.Console.WriteLine(System.String)", ExpectedResult = "https://example.com/?q=C%23+System.Console.WriteLine(string)")]
        [TestCase("M:System.Linq.Enumerable.Repeat``1(``0,System.Int32)", ExpectedResult = "https://example.com/?q=C%23+System.Linq.Enumerable.Repeat%3CTResult%3E(TResult%2C+int)")]
        [TestCase("M:System.DateTime.op_Addition(System.DateTime,System.TimeSpan)", ExpectedResult = "https://example.com/?q=C%23+System.DateTime.Addition(DateTime%2C+TimeSpan)")]
        [TestCase("P:System.Collections.Generic.List`1.Count", ExpectedResult = "https://example.com/?q=C%23+System.Collections.Generic.List%3CT%3E.Count")]
        [TestCase("P:System.Collections.Generic.List`1.Item(System.Int32)", ExpectedResult = "https://example.com/?q=C%23+System.Collections.Generic.List%3CT%3E.Item%5Bint%5D")]
        [TestCase("P:System.Collections.Generic.List`1.Enumerator.Current", ExpectedResult = "https://example.com/?q=C%23+System.Collections.Generic.List%3CT%3E.Enumerator.Current")]
        [TestCase("F:System.DateTime.MaxValue", ExpectedResult = "https://example.com/?q=C%23+System.DateTime.MaxValue")]
        [TestCase("E:System.AppDomain.AssemblyLoad", ExpectedResult = "https://example.com/?q=C%23+System.AppDomain.AssemblyLoad")]
        [TestCase("M:System.Collections.Generic.List`1.System#Collections#IEnumerable#GetEnumerator", ExpectedResult = "https://example.com/?q=C%23+System.Collections.Generic.List%3CT%3E.System.Collections.IEnumerable.GetEnumerator()")]
        [TestCase("P:System.Collections.Generic.List`1.System#Collections#IList#Item(System.Int32)", ExpectedResult = "https://example.com/?q=C%23+System.Collections.Generic.List%3CT%3E.System.Collections.IList.Item%5Bint%5D")]
        public string? TryGetUrl_CodeReference_CSharp_ReturnsExpectedUrl(string cref)
        {
            var onlineSearch = new SearchBasedApiDocUrlResolver(new Uri("https://example.com/"))
            {
                Language = new CSharp(),
                NamespacePatterns = { "*" }
            };

            return onlineSearch.TryGetUrlByCodeReference(cref, out var url) ? url.ToString() : null;
        }
    }
}
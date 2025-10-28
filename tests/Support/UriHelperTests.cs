// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Support
{
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class UriHelperTests
    {
        [TestCase("https://example.com/", "param", "value", UriKind.Absolute, ExpectedResult = "https://example.com/?param=value")]
        [TestCase("https://example.com/?existing=yes", "param", "value", UriKind.Absolute, ExpectedResult = "https://example.com/?existing=yes&param=value")]
        [TestCase("https://example.com/#fragment", "param", "value", UriKind.Absolute, ExpectedResult = "https://example.com/?param=value#fragment")]
        [TestCase("https://example.com/?existing=yes#fragment", "param", "value", UriKind.Absolute, ExpectedResult = "https://example.com/?existing=yes&param=value#fragment")]
        [TestCase("page.html", "param", "value", UriKind.Relative, ExpectedResult = "page.html?param=value")]
        [TestCase("page.html?existing=yes", "param", "value", UriKind.Relative, ExpectedResult = "page.html?existing=yes&param=value")]
        [TestCase("https://example.com/", "param", "value with spaces", UriKind.Absolute, ExpectedResult = "https://example.com/?param=value+with+spaces")]
        [TestCase("https://example.com/", "param", "special:/?#[]@!$&'()*+,;=", UriKind.Absolute, ExpectedResult = "https://example.com/?param=special%3A%2F%3F%23%5B%5D%40!%24%26%27()*%2B%2C%3B%3D")]
        [TestCase("https://example.com/", "param", "", UriKind.Absolute, ExpectedResult = "https://example.com/?param=")]
        public string WithQueryParameter_ReturnsExpectedUri(string uriString, string paramName, string paramValue, UriKind uriKind)
        {
            var url = new Uri(uriString, uriKind);
            var result = url.WithQueryParameter(paramName, paramValue);
            return result.ToString();
        }

        [TestCase("https://example.com/", "section", UriKind.Absolute, ExpectedResult = "https://example.com/#section")]
        [TestCase("https://example.com/#old", "section", UriKind.Absolute, ExpectedResult = "https://example.com/#section")]
        [TestCase("https://example.com/?param=value", "section", UriKind.Absolute, ExpectedResult = "https://example.com/?param=value#section")]
        [TestCase("page.html", "section", UriKind.Relative, ExpectedResult = "page.html#section")]
        [TestCase("page.html#old", "section", UriKind.Relative, ExpectedResult = "page.html#section")]
        [TestCase("https://example.com/", "", UriKind.Absolute, ExpectedResult = "https://example.com/")]
        [TestCase("https://example.com/#old", "", UriKind.Absolute, ExpectedResult = "https://example.com/")]
        public string WithFragment_ReturnsExpectedUri(string uriString, string fragment, UriKind uriKind)
        {
            var url = new Uri(uriString, uriKind);
            var result = url.WithFragment(fragment);
            return result.ToString();
        }

        [TestCase("https://example.com/#section", UriKind.Absolute, ExpectedResult = "https://example.com/")]
        [TestCase("https://example.com/?param=value#section", UriKind.Absolute, ExpectedResult = "https://example.com/?param=value")]
        [TestCase("https://example.com/", UriKind.Absolute, ExpectedResult = "https://example.com/")]
        [TestCase("page.html#section", UriKind.Relative, ExpectedResult = "page.html")]
        [TestCase("page.html", UriKind.Relative, ExpectedResult = "page.html")]
        public string WithoutFragment_ReturnsExpectedUri(string uriString, UriKind uriKind)
        {
            var url = new Uri(uriString, uriKind);
            var result = url.WithoutFragment();
            return result.ToString();
        }

        // Path only
        [TestCase("https://example.com/", "page.html", UriKind.Absolute, ExpectedResult = "https://example.com/page.html")]
        [TestCase("https://example.com/docs/", "page.html", UriKind.Absolute, ExpectedResult = "https://example.com/docs/page.html")]
        [TestCase("https://example.com/docs", "page.html", UriKind.Absolute, ExpectedResult = "https://example.com/docs/page.html")]
        [TestCase("https://example.com/docs/", "/page.html", UriKind.Absolute, ExpectedResult = "https://example.com/page.html")]
        [TestCase("docs/", "page.html", UriKind.Relative, ExpectedResult = "docs/page.html")]
        [TestCase("docs", "page.html", UriKind.Relative, ExpectedResult = "docs/page.html")]
        [TestCase("docs/", "/page.html", UriKind.Relative, ExpectedResult = "/page.html")]
        [TestCase("docs", "", UriKind.Relative, ExpectedResult = "docs")]
        [TestCase("", "page.html", UriKind.Relative, ExpectedResult = "page.html")]
        // Query only
        [TestCase("docs/page", "?param=value", UriKind.Relative, ExpectedResult = "docs/page?param=value")]
        [TestCase("docs/page?base=value", "?param=value", UriKind.Relative, ExpectedResult = "docs/page?base=value&param=value")]
        [TestCase("docs/page?base=value", "", UriKind.Relative, ExpectedResult = "docs/page?base=value")]
        // Fragment only
        [TestCase("docs/page", "#section", UriKind.Relative, ExpectedResult = "docs/page#section")]
        [TestCase("docs/page#base", "#section", UriKind.Relative, ExpectedResult = "docs/page#section")]
        [TestCase("docs/page#base", "", UriKind.Relative, ExpectedResult = "docs/page#base")]
        // Multiple components
        [TestCase("docs/page?base=value#section", "other?param=value#fragment", UriKind.Relative, ExpectedResult = "docs/page/other?base=value&param=value#fragment")]
        [TestCase("docs/page?base=value#section", "/other?param=value#fragment", UriKind.Relative, ExpectedResult = "/other?base=value&param=value#fragment")]
        [TestCase("https://example.com/docs/page?base=value#section", "other?param=value#fragment", UriKind.Absolute, ExpectedResult = "https://example.com/docs/page/other?base=value&param=value#fragment")]
        [TestCase("https://example.com/docs/page?base=value#section", "/other?param=value#fragment", UriKind.Absolute, ExpectedResult = "https://example.com/other?base=value&param=value#fragment")]
        // Path normalization tests
        [TestCase("https://example.com/docs/", "../page.html", UriKind.Absolute, ExpectedResult = "https://example.com/page.html")]
        [TestCase("https://example.com/docs/sub/", "../../page.html", UriKind.Absolute, ExpectedResult = "https://example.com/page.html")]
        [TestCase("https://example.com/docs/", "./page.html", UriKind.Absolute, ExpectedResult = "https://example.com/docs/page.html")]
        [TestCase("https://example.com/docs/", "sub/../page.html", UriKind.Absolute, ExpectedResult = "https://example.com/docs/page.html")]
        [TestCase("https://example.com/docs/", "sub/./page.html", UriKind.Absolute, ExpectedResult = "https://example.com/docs/sub/page.html")]
        [TestCase("docs/", "../page.html", UriKind.Relative, ExpectedResult = "page.html")]
        [TestCase("docs/sub/", "../../page.html", UriKind.Relative, ExpectedResult = "page.html")]
        [TestCase("docs/", "./page.html", UriKind.Relative, ExpectedResult = "docs/page.html")]
        [TestCase("docs/", "sub/../page.html", UriKind.Relative, ExpectedResult = "docs/page.html")]
        [TestCase("docs/", "sub/./page.html", UriKind.Relative, ExpectedResult = "docs/sub/page.html")]
        [TestCase("", "../page.html", UriKind.Relative, ExpectedResult = "../page.html")]
        [TestCase("", "../../page.html", UriKind.Relative, ExpectedResult = "../../page.html")]
        public string Combine_ReturnsExpectedUri(string baseUriString, string relativeUri, UriKind uriKind)
        {
            var baseUri = new Uri(baseUriString, uriKind);
            var result = baseUri.Combine(relativeUri);
            return result.ToString();
        }

        [Test]
        public void ParseQueryString_WithSingleParameter_ReturnsExpectedPair()
        {
            var result = UriHelper.ParseQueryString("param=value").ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result[0].Key, Is.EqualTo("param"));
                Assert.That(result[0].Value, Is.EqualTo("value"));
            }
        }

        [Test]
        public void ParseQueryString_WithQuestionMarkPrefix_RemovesPrefix()
        {
            var result = UriHelper.ParseQueryString("?param=value").ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result[0].Key, Is.EqualTo("param"));
                Assert.That(result[0].Value, Is.EqualTo("value"));
            }
        }

        [Test]
        public void ParseQueryString_WithMultipleParameters_ReturnsAllPairs()
        {
            var result = UriHelper.ParseQueryString("param1=value1&param2=value2&param3=value3").ToList();

            Assert.That(result, Has.Count.EqualTo(3));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result[0].Key, Is.EqualTo("param1"));
                Assert.That(result[0].Value, Is.EqualTo("value1"));
                Assert.That(result[1].Key, Is.EqualTo("param2"));
                Assert.That(result[1].Value, Is.EqualTo("value2"));
                Assert.That(result[2].Key, Is.EqualTo("param3"));
                Assert.That(result[2].Value, Is.EqualTo("value3"));
            }
        }

        [Test]
        public void ParseQueryString_WithEmptyValues_ReturnsEmptyStrings()
        {
            var result = UriHelper.ParseQueryString("param1=&param2=value2&param3=").ToList();

            Assert.That(result, Has.Count.EqualTo(3));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result[0].Key, Is.EqualTo("param1"));
                Assert.That(result[0].Value, Is.Empty);
                Assert.That(result[1].Key, Is.EqualTo("param2"));
                Assert.That(result[1].Value, Is.EqualTo("value2"));
                Assert.That(result[2].Key, Is.EqualTo("param3"));
                Assert.That(result[2].Value, Is.Empty);
            }
        }

        [Test]
        public void ParseQueryString_WithNoEqualSign_ReturnsEmptyValue()
        {
            var result = UriHelper.ParseQueryString("param1&param2=value2").ToList();

            Assert.That(result, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result[0].Key, Is.EqualTo("param1"));
                Assert.That(result[0].Value, Is.Empty);
                Assert.That(result[1].Key, Is.EqualTo("param2"));
                Assert.That(result[1].Value, Is.EqualTo("value2"));
            }
        }

        [Test]
        public void ParseQueryString_WithUriEncodedValues_ReturnsDecodedValues()
        {
            var result = UriHelper.ParseQueryString("param1=value+with+spaces&param2=special%3A%2F%3F%23").ToList();

            Assert.That(result, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result[0].Key, Is.EqualTo("param1"));
                Assert.That(result[0].Value, Is.EqualTo("value with spaces"));
                Assert.That(result[1].Key, Is.EqualTo("param2"));
                Assert.That(result[1].Value, Is.EqualTo("special:/?#"));
            }
        }

        [Test]
        public void ToQueryString_WithSingleParameter_ReturnsExpectedString()
        {
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("param", "value")
            };

            var result = parameters.ToQueryString();

            Assert.That(result, Is.EqualTo("param=value"));
        }

        [Test]
        public void ToQueryString_WithMultipleParameters_ReturnsExpectedString()
        {
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("param1", "value1"),
                new("param2", "value2"),
                new("param3", "value3")
            };

            var result = parameters.ToQueryString();

            Assert.That(result, Is.EqualTo("param1=value1&param2=value2&param3=value3"));
        }

        [Test]
        public void ToQueryString_WithEmptyValues_ReturnsExpectedString()
        {
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("param1", ""),
                new("param2", "value2"),
                new("param3", "")
            };

            var result = parameters.ToQueryString();

            Assert.That(result, Is.EqualTo("param1=&param2=value2&param3="));
        }

        [Test]
        public void ToQueryString_WithSpecialCharacters_ReturnsEncodedString()
        {
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("param1", "value with spaces"),
                new("param2", "special:/?#[]@!$&'()*+,;=")
            };

            var result = parameters.ToQueryString();

            Assert.That(result, Is.EqualTo("param1=value+with+spaces&param2=special%3A%2F%3F%23%5B%5D%40!%24%26%27()*%2B%2C%3B%3D"));
        }

        [Test]
        public void ToQueryString_WithSpecialCharactersInKey_ReturnsEncodedString()
        {
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("key with spaces", "value"),
                new("special:/?#", "value")
            };

            var result = parameters.ToQueryString();

            Assert.That(result, Is.EqualTo("key+with+spaces=value&special%3A%2F%3F%23=value"));
        }

        [Test]
        public void ToQueryString_WithEmptyCollection_ReturnsEmptyString()
        {
            var parameters = new List<KeyValuePair<string, string>>();

            var result = parameters.ToQueryString();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ToQueryString_And_ParseQueryString_RoundTrip()
        {
            var originalParameters = new KeyValuePair<string, string>[]
            {
                new("param1", "value1"),
                new("param2", "value with spaces"),
                new("param3", "special:/?#")
            };

            var queryString = originalParameters.ToQueryString();
            var parsedParameters = UriHelper.ParseQueryString(queryString).ToList();

            Assert.That(parsedParameters, Is.EqualTo(originalParameters));
        }

        [TestCase("", ExpectedResult = "")]
        [TestCase("https://example.com/path", ExpectedResult = "https://example.com/path")]
        [TestCase("path/to/file.html", ExpectedResult = "path/to/file.html")]
        [TestCase("https://example.com/path?query=value", ExpectedResult = "https://example.com/path")]
        [TestCase("path/to/file.html?query=value", ExpectedResult = "path/to/file.html")]
        [TestCase("https://example.com/path#fragment", ExpectedResult = "https://example.com/path")]
        [TestCase("path/to/file.html#fragment", ExpectedResult = "path/to/file.html")]
        [TestCase("https://example.com/path?query=value#fragment", ExpectedResult = "https://example.com/path")]
        [TestCase("path/to/file.html?query=value#fragment", ExpectedResult = "path/to/file.html")]
        [TestCase("?query=value", ExpectedResult = "")]
        [TestCase("#fragment", ExpectedResult = "")]
        [TestCase("?query=value#fragment", ExpectedResult = "")]
        [TestCase("#fragment?query=value", ExpectedResult = "")]
        public string GetPathPart_ReturnsExpectedPath(string uriString)
        {
            return UriHelper.GetPathPart(uriString);
        }

        [TestCase("https://example.com/docs/page.html", ExpectedResult = new[] { "https://example.com/docs/page.html", "" })]
        [TestCase("https://example.com/docs/page.html?param=value", ExpectedResult = new[] { "https://example.com/docs/page.html", "?param=value" })]
        [TestCase("https://example.com/docs/page.html#section", ExpectedResult = new[] { "https://example.com/docs/page.html", "#section" })]
        [TestCase("https://example.com/docs/page.html?param=value#section", ExpectedResult = new[] { "https://example.com/docs/page.html", "?param=value#section" })]
        [TestCase("docs/page.html?param=value#section", ExpectedResult = new[] { "docs/page.html", "?param=value#section" })]
        [TestCase("?param=value", ExpectedResult = new[] { "", "?param=value" })]
        [TestCase("#fragment", ExpectedResult = new[] { "", "#fragment" })]
        [TestCase("", ExpectedResult = new[] { "", "" })]
        public string[] SplitPathAndSuffix_ReturnsExpectedComponents(string input)
        {
            var (path, suffix) = UriHelper.SplitPathAndSuffix(input);
            return [path, suffix];
        }

        [TestCase("https://example.com/docs/page.html", ExpectedResult = new[] { "https://example.com/docs/page.html", "", "" })]
        [TestCase("https://example.com/docs/page.html?param=value", ExpectedResult = new[] { "https://example.com/docs/page.html", "?param=value", "" })]
        [TestCase("https://example.com/docs/page.html#section", ExpectedResult = new[] { "https://example.com/docs/page.html", "", "#section" })]
        [TestCase("https://example.com/docs/page.html?param=value#section", ExpectedResult = new[] { "https://example.com/docs/page.html", "?param=value", "#section" })]
        [TestCase("docs/page.html?param=value#section", ExpectedResult = new[] { "docs/page.html", "?param=value", "#section" })]
        [TestCase("?param=value", ExpectedResult = new[] { "", "?param=value", "" })]
        [TestCase("#fragment", ExpectedResult = new[] { "", "", "#fragment" })]
        [TestCase("?param=value#fragment", ExpectedResult = new[] { "", "?param=value", "#fragment" })]
        [TestCase("", ExpectedResult = new[] { "", "", "" })]
        public string[] SplitPathQueryAndFragment_ReturnsExpectedComponents(string input)
        {
            var (path, queryString, fragment) = UriHelper.SplitPathQueryAndFragment(input);
            return [path, queryString, fragment];
        }

        [TestCase("", ExpectedResult = false)]
        [TestCase("/", ExpectedResult = true)]
        [TestCase("/path", ExpectedResult = true)]
        [TestCase("/path/to/file", ExpectedResult = true)]
        [TestCase("http://example.com", ExpectedResult = true)]
        [TestCase("https://example.com/path", ExpectedResult = true)]
        [TestCase("ftp://example.com", ExpectedResult = true)]
        [TestCase("file:///path", ExpectedResult = true)]
        [TestCase("mailto:user@example.com", ExpectedResult = true)]
        [TestCase("scheme:path", ExpectedResult = true)]
        [TestCase("scheme:", ExpectedResult = true)]
        [TestCase("C:/path", ExpectedResult = true)]
        [TestCase("C:path", ExpectedResult = true)]
        [TestCase("relative/path", ExpectedResult = false)]
        [TestCase("relative", ExpectedResult = false)]
        [TestCase("scheme", ExpectedResult = false)]
        [TestCase("path/with/scheme:inside", ExpectedResult = false)]
        [TestCase("\\\\server\\share", ExpectedResult = false)]
        [TestCase("data:text/plain;base64,SGVsbG8=", ExpectedResult = true)]
        public bool IsAbsoluteOrRooted_ReturnsExpectedResult(string uriString)
        {
            return UriHelper.IsAbsoluteOrRooted(uriString);
        }

        [TestCase("?param=value", ExpectedResult = true)]
        [TestCase("#fragment", ExpectedResult = true)]
        [TestCase("?param=value#fragment", ExpectedResult = true)]
        [TestCase("#fragment?param=value", ExpectedResult = true)]
        [TestCase("https://example.com/", ExpectedResult = false)]
        [TestCase("page.html", ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase("param=value", ExpectedResult = false)]
        [TestCase("https://example.com/?param=value", ExpectedResult = false)]
        [TestCase("https://example.com/#fragment", ExpectedResult = false)]
        public bool IsQueryOrFragmentOnly_ReturnsExpectedResult(string uriString)
        {
            return UriHelper.IsQueryOrFragmentOnly(uriString);
        }
    }
}

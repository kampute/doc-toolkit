// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Support
{
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class RawUriTests
    {
        [TestCase("http://example.com/%2D", ExpectedResult = "http://example.com/%2D")]
        [TestCase("http://example.com/%2Dpath", ExpectedResult = "http://example.com/%2Dpath")]
        [TestCase("http://example.com/path%2D", ExpectedResult = "http://example.com/path%2D")]
        [TestCase("http://example.com/path%2D/resource", ExpectedResult = "http://example.com/path%2D/resource")]
        public string ToString_WithEscapedDash_PreservesEscapeSequence(string uriString)
        {
            var uri = new RawUri(uriString);

            var standardUri = new Uri(uriString);
            Assert.That(standardUri.ToString(), Is.Not.EqualTo(uriString),
                "Standard Uri should decode %2D but didn't, which makes this test invalid");

            return uri.ToString();
        }

        [TestCase("http://example.com/%2D", ExpectedResult = "http://example.com/%2D")]             // dash
        [TestCase("http://example.com/%3F", ExpectedResult = "http://example.com/%3F")]             // question mark
        [TestCase("http://example.com/%23", ExpectedResult = "http://example.com/%23")]             // hash
        [TestCase("http://example.com/%25", ExpectedResult = "http://example.com/%25")]             // percent
        [TestCase("http://example.com/%20path", ExpectedResult = "http://example.com/%20path")]     // space
        [TestCase("http://example.com/caf%C3%A9", ExpectedResult = "http://example.com/caf%C3%A9")] // é (e-acute)
        [TestCase("http://example.com/%C3%BC", ExpectedResult = "http://example.com/%C3%BC")]       // ü (u-umlaut)
        [TestCase("http://example.com/%E2%82%AC", ExpectedResult = "http://example.com/%E2%82%AC")] // € (euro)
        public string ToString_WithEscapedCharacters_PreservesEscapeSequences(string uriString)
        {
            var uri = new RawUri(uriString);

            return uri.ToString();
        }

        [TestCase("http://example.com/", "path/%2Dresource", ExpectedResult = "http://example.com/path/%2Dresource")]
        [TestCase("http://example.com/api/", "item/%23123", ExpectedResult = "http://example.com/api/item/%23123")]
        [TestCase("http://example.com/path/", "../other/%3Fquery=%26value", ExpectedResult = "http://example.com/other/%3Fquery=%26value")]
        public string ToString_WithBaseAndRelativeUri_PreservesEscapeSequences(string baseUriString, string relativeUriString)
        {
            var baseUri = new Uri(baseUriString);
            var uri = new RawUri(baseUri, relativeUriString);

            return uri.ToString();
        }

        [TestCase("http://example.com/%2D", UriKind.Absolute, ExpectedResult = "http://example.com/%2D")]
        [TestCase("/path/%2Dresource", UriKind.Relative, ExpectedResult = "/path/%2Dresource")]
        [TestCase("http://example.com/%2D", UriKind.RelativeOrAbsolute, ExpectedResult = "http://example.com/%2D")]
        [TestCase("/path/%2Dresource", UriKind.RelativeOrAbsolute, ExpectedResult = "/path/%2Dresource")]
        [TestCase("http://example.com/%3Fquery=%26value", UriKind.Absolute, ExpectedResult = "http://example.com/%3Fquery=%26value")]
        [TestCase("http://example.com/path#fragment%2Did", UriKind.Absolute, ExpectedResult = "http://example.com/path#fragment%2Did")]
        public string ToString_WithUriKinds_PreservesEscapeSequences(string uriString, UriKind uriKind)
        {
            var uri = new RawUri(uriString, uriKind);

            return uri.ToString();
        }

        [Test]
        public void ToString_ComparedToStandardUri_PreservesEscapeSequencesWhenStandardUriNormalizes()
        {
            var testUris = new[]
            {
                "http://example.com/%2D",                  // Escaped dash
                "http://example.com/path%2Dto%2Dresource", // Multiple escaped dashes
                "http://example.com/path?query=%2Dvalue",  // Escaped dash in query
                "http://example.com/path#fragment%2Did"    // Escaped dash in fragment
            };

            foreach (var uriString in testUris)
            {
                var rawUri = new RawUri(uriString);
                var standardUri = new Uri(uriString);

                var rawResult = rawUri.ToString();
                var standardResult = standardUri.ToString();

                Assert.That(rawResult, Is.EqualTo(uriString), $"RawUri should preserve the exact original string: {uriString}");

                if (standardResult != uriString)
                    Assert.Pass($"Confirmed different behavior: RawUri preserves escapes that Uri normalizes in {uriString}");
            }

            Assert.Inconclusive("Could not demonstrate a difference between RawUri and Uri behavior with the test URIs");
        }
    }
}

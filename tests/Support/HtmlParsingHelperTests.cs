// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Support
{
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;

    [TestFixture]
    public class HtmlParsingHelperTests
    {
        [TestCase("<tag>content</tag>", "tag", ExpectedResult = "content")]
        [TestCase("<TAG>content</TAG>", "tag", ExpectedResult = "content")]
        [TestCase("<tag attribute='value'>content</tag>", "tag", ExpectedResult = "content")]
        [TestCase("<tag>nested <span>content</span> here</tag>", "tag", ExpectedResult = "nested <span>content</span> here")] //
        [TestCase("<tag>  whitespace  </tag>", "tag", ExpectedResult = "  whitespace  ")]
        [TestCase("<tag></tag>", "tag", ExpectedResult = "")]
        [TestCase("<tag>content</tag><tag>more content</tag>", "tag", ExpectedResult = "content")]
        [TestCase("<outer><tag>content</tag></outer>", "tag", ExpectedResult = "content")]
        [TestCase("<tag \nclass='multiline'>content</tag>", "tag", ExpectedResult = "content")]
        [TestCase("<tag-name>content</tag-name>", "tag-name", ExpectedResult = "content")]
        [TestCase("<tag-name>content</tag-name>", "tag", ExpectedResult = null)]
        [TestCase("<tag-name> <tag>content</tag> </tag-name>", "tag", ExpectedResult = "content")]
        [TestCase("", "tag", ExpectedResult = null)]
        [TestCase("<tag>content</tag>", "", ExpectedResult = null)]
        [TestCase("<tag>content</tag", "tag", ExpectedResult = null)]
        [TestCase("<tag>content<tag>", "tag", ExpectedResult = null)]
        [TestCase("tag>content</tag>", "tag", ExpectedResult = null)]
        public string? TryExtractTagContent_ReturnsExpectedContent(string html, string tagName)
        {
            HtmlParsingHelper.TryExtractTagContent(html, tagName, out var content);
            return content;
        }
    }
}

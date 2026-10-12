// Copyright (C) Kampute
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
        [TestCase("<tag>outer <TAG class='x'>inner</TAG> after</tag>", "tag", ExpectedResult = "outer <TAG class='x'>inner</TAG> after")]
        [TestCase("<tag>a<tag-name>b</tag-name>c</tag>", "tag", ExpectedResult = "a<tag-name>b</tag-name>c")]
        [TestCase("<body><!-- <body> --><p>Content</p></body>", "body", ExpectedResult = "<!-- <body> --><p>Content</p>")]
        [TestCase("<body><!-- </body> --><p>Content</p></body>", "body", ExpectedResult = "<!-- </body> --><p>Content</p>")]
        [TestCase("<!-- <body>fake</body> --><body>Content</body>", "body", ExpectedResult = "Content")]
        [TestCase("<!-- <body>fake</body> -->", "body", ExpectedResult = null)]
        [TestCase("<tag><!-- <tag> --><tag>nested</tag> after</tag>", "tag", ExpectedResult = "<!-- <tag> --><tag>nested</tag> after")]
        [TestCase("<body><span title='<body>'>Content</span></body>", "body", ExpectedResult = "<span title='<body>'>Content</span>")]
        [TestCase("<body><script>const tag = '<body>';</script>Content</body>", "body", ExpectedResult = "<script>const tag = '<body>';</script>Content")]
        [TestCase("<title>Example <title> text</title>", "title", ExpectedResult = "Example <title> text")]
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

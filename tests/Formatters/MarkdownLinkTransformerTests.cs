// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Formatters
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Routing;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class MarkdownLinkTransformerTests
    {
        [TestCase("[Home](home.md)", ExpectedResult = "[Home](index.md)")]
        [TestCase("[Home](home.md#section)", ExpectedResult = "[Home](index.md#section)")]
        [TestCase("[Home](home.md?query=param)", ExpectedResult = "[Home](index.md?query=param)")]
        [TestCase("[Image](image.jpg)", ExpectedResult = "[Image](images/image.jpg)")]
        [TestCase("[Home](home.md \"Home Page\")", ExpectedResult = "[Home](index.md \"Home Page\")")]
        [TestCase("[Image](image.jpg 'Image')", ExpectedResult = "[Image](images/image.jpg 'Image')")]
        [TestCase("[Other](other.md)", ExpectedResult = "[Other](other.md)")]
        [TestCase("[home-link]: home.md", ExpectedResult = "[home-link]: index.md")]
        [TestCase("[image-link]: image.jpg", ExpectedResult = "[image-link]: images/image.jpg")]
        [TestCase("[home-link]: home.md \"Home Page\"", ExpectedResult = "[home-link]: index.md \"Home Page\"")]
        [TestCase("[image-link]: image.jpg 'Image'", ExpectedResult = "[image-link]: images/image.jpg 'Image'")]
        [TestCase("[other-link]: other.md", ExpectedResult = "[other-link]: other.md")]
        [TestCase("[Home](home.md)\n[Image](image.jpg)", ExpectedResult = "[Home](index.md)\n[Image](images/image.jpg)")]
        [TestCase("[home]: home.md\n[image]: image.jpg", ExpectedResult = "[home]: index.md\n[image]: images/image.jpg")]
        [TestCase("[Home](home.md)\n[home]: home.md", ExpectedResult = "[Home](index.md)\n[home]: index.md")]
        [TestCase("[Home](index.md)", ExpectedResult = "[Home](pages/index.md)")]
        [TestCase(@"
            # Introduction
            This is a [link to home](home.md).
            # Images
            Here's an ![image](image.jpg).
            # References
            [home]: home.md ""Home Page""
            [image]: image.jpg 'Image Description'
            [other]: other.md
            For more information, see the [about page](about.md).",
            ExpectedResult = @"
            # Introduction
            This is a [link to home](index.md).
            # Images
            Here's an ![image](images/image.jpg).
            # References
            [home]: index.md ""Home Page""
            [image]: images/image.jpg 'Image Description'
            [other]: other.md
            For more information, see the [about page](pages/about.md).")]
        public string Transform_ReplacesUrlsAsExpected(string markdownContent)
        {
            var linkTransformer = new MarkdownLinkTransformer();
            var reader = new StringReader(markdownContent);
            var writer = new StringWriter();
            var urlMapper = new PathToUrlMapper
            {
                { "home.md", new Uri("index.md", UriKind.Relative) },
                { "index.md", new Uri("pages/index.md", UriKind.Relative) },
                { "image.jpg", new Uri("images/image.jpg", UriKind.Relative) },
                { "about.md", new Uri("pages/about.md", UriKind.Relative) },
            };

            linkTransformer.Transform(reader, writer, urlMapper);

            return writer.ToString();
        }
    }
}
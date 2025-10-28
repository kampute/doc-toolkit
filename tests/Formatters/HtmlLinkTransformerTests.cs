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
    public class HtmlLinkTransformerTests
    {
        [TestCase("<a href=\"home.html\">Home</a>", ExpectedResult = "<a href=\"index.html\">Home</a>")]
        [TestCase("<a href='home.html#section'>Home</a>", ExpectedResult = "<a href='index.html#section'>Home</a>")]
        [TestCase("<a href='home.html?query=param'>Home</a>", ExpectedResult = "<a href='index.html?query=param'>Home</a>")]
        [TestCase("<img src=\"image.jpg\" alt=\"Image\">", ExpectedResult = "<img src=\"images/image.jpg\" alt=\"Image\">")]
        [TestCase("<img src='image.jpg' alt='Image'>", ExpectedResult = "<img src='images/image.jpg' alt='Image'>")]
        [TestCase("<a href=index.html>Home</a>", ExpectedResult = "<a href=pages/index.html>Home</a>")]
        [TestCase("<img src=image.jpg alt=\"Image\">", ExpectedResult = "<img src=images/image.jpg alt=\"Image\">")]
        [TestCase("<a href=\"other.html\">Other</a>", ExpectedResult = "<a href=\"other.html\">Other</a>")]
        [TestCase("<img src='other.jpg' alt='Image'>", ExpectedResult = "<img src='other.jpg' alt='Image'>")]
        [TestCase("<div data-url=\"index.html\">Content</div>", ExpectedResult = "<div data-url=\"index.html\">Content</div>")]
        [TestCase(@"
                <!DOCTYPE html>
                <html>
                <head>
                    <link href=styles.css rel=""stylesheet"">
                    <script src=""app.js""></script>
                </head>
                <body>
                    <div class=""container"">
                        <a href=index.html>Home</a>
                        <a href=""about.html"">About</a>
                        <img src='image.jpg' alt='Image'>
                    </div>
                </body>
                </html>",
                ExpectedResult = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <link href=css/styles.css rel=""stylesheet"">
                    <script src=""js/app.js""></script>
                </head>
                <body>
                    <div class=""container"">
                        <a href=pages/index.html>Home</a>
                        <a href=""pages/about.html"">About</a>
                        <img src='images/image.jpg' alt='Image'>
                    </div>
                </body>
                </html>")]
        public string Transform_ReplacesUrlsAsExpected(string htmlContent)
        {
            var linkTransformer = new HtmlLinkTransformer();
            var reader = new StringReader(htmlContent);
            var writer = new StringWriter();
            var urlMapper = new PathToUrlMapper
            {
                { "styles.css", new Uri("css/styles.css", UriKind.Relative) },
                { "app.js", new Uri("js/app.js", UriKind.Relative) },
                { "home.html", new Uri("index.html", UriKind.Relative) },
                { "index.html", new Uri("pages/index.html", UriKind.Relative) },
                { "about.html", new Uri("pages/about.html", UriKind.Relative) },
                { "image.jpg", new Uri("images/image.jpg", UriKind.Relative) },
            };

            linkTransformer.Transform(reader, writer, urlMapper);

            return writer.ToString();
        }

    }
}
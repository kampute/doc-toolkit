// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Topics
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Topics;
    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class AdHocTopicTests
    {
        [Test]
        public void Render_OutputsExpectedContent()
        {
            var adHocTopic = new AdHocTopic("TestTopic", static (w, c) => w.WriteLink(new("https://example.com/"), "Example"));

            using (var textWriter = new StringWriter())
            using (var context = MockHelper.CreateDocumentationContext<HtmlFormat>())
            {
                adHocTopic.Render(textWriter, context);

                Assert.That(textWriter.ToString(), Is.EqualTo("<a href=\"https://example.com/\">Example</a>"));
            }

            using (var textWriter = new StringWriter())
            using (var context = MockHelper.CreateDocumentationContext<MarkdownFormat>())
            {
                adHocTopic.Render(textWriter, context);

                Assert.That(textWriter.ToString(), Is.EqualTo("[Example](https://example.com/)"));
            }
        }
    }
}

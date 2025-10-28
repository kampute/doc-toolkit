// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc
{
    using Kampute.DocToolkit.XmlDoc;
    using NUnit.Framework;

    [TestFixture]
    public class XmlDocToMarkdownTransformerTests : XmlDocTransformerTester<XmlDocToMarkdownTransformer>
    {
        [TestCase("<c>code</c>", ExpectedResult = "`code`")]
        [TestCase("<code>Console.WriteLine(\"&lt;Hello&gt;\");</code>", ExpectedResult = "\n```csharp\nConsole.WriteLine(\"<Hello>\");\n```\n")]
        [TestCase("<code language=\"python\">print(\"Hello, World!\")</code>", ExpectedResult = "\n```python\nprint(\"Hello, World!\")\n```\n")]
        [TestCase("<paramref name=\"value\"/>", ExpectedResult = "`value`")]
        [TestCase("<typeparamref name=\"T\"/>", ExpectedResult = "`T`")]
        [TestCase("<see langword=\"null\"/>", ExpectedResult = "[`null`](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null)")]
        [TestCase("<see cref=\"T:System.String\"/>", ExpectedResult = "[System.String](https://learn.microsoft.com/dotnet/api/system.string)")]
        [TestCase("<see href=\"https://example.com\">Example</see>", ExpectedResult = "[Example](https://example.com)")]
        [TestCase("<see href=\"topic\"/>", ExpectedResult = "[Title of topic](topic)")]
        [TestCase("<list type=\"bullet\"><item><description>First</description></item><item><description>Second</description></item></list>", ExpectedResult = "\n\n- First\n- Second\n\n")]
        [TestCase("<list type=\"bullet\"><item><term>First</term><description>First description</description></item><item><term>Second</term><description>Second description</description></item></list>", ExpectedResult = "\n\n- **First** \u2013 First description\n- **Second** \u2013 Second description\n\n")]
        [TestCase("<list type=\"number\"><item><description>First</description></item><item><description>Second</description></item></list>", ExpectedResult = "\n\n1. First\n2. Second\n\n")]
        [TestCase("<list type=\"number\"><item><term>First</term><description>First description</description></item><item><term>Second</term><description>Second description</description></item></list>", ExpectedResult = "\n\n1. **First** \u2013 First description\n2. **Second** \u2013 Second description\n\n")]
        [TestCase("<list type=\"table\"><listheader><term>Term</term><description>Description</description></listheader><item><term>Term 1</term><description>Description 1</description></item><item><term>Term 2</term><description>Description 2</description></item></list>", ExpectedResult = "\n\n|Term|Description|\n|---|---|\n|Term 1|Description 1|\n|Term 2|Description 2|\n\n")]
        [TestCase("<note>This is a note.</note>", ExpectedResult = "\n\n> This is a note.\n\n")]
        [TestCase("<note>This is an <c>example</c> note.</note>", ExpectedResult = "\n\n> This is an `example` note.\n\n")]
        [TestCase("<note>This is a note with two paragraphs.<para>This is the <c>second</c> paragraph.</para></note>", ExpectedResult = "\n\n> This is a note with two paragraphs.\n> \n> This is the `second` paragraph.\n\n")]
        [TestCase("<note type=\"warning\">This is a warning note.</note>", ExpectedResult = "\n\n> This is a warning note.\n\n")]
        [TestCase("<note type=\"important\" title=\"Important\">This is an important note.</note>", ExpectedResult = "\n\n> **Important** \\\n> This is an important note.\n\n")]
        [TestCase("<para>This is <c>inline code</c> and a <see href=\"https://example.com\">link</see>.</para>", ExpectedResult = "This is `inline code` and a [link](https://example.com).")]
        [TestCase("<para>Single paragraph.</para>", ExpectedResult = "Single paragraph.")]
        [TestCase("<para>First paragraph.</para><para>Second paragraph.</para>", ExpectedResult = "First paragraph.\n\nSecond paragraph.")]
        [TestCase("<para>First paragraph.</para><para>Second paragraph.</para><para>Third paragraph.</para>", ExpectedResult = "First paragraph.\n\nSecond paragraph.\n\nThird paragraph.")]
        [TestCase("<para>First paragraph.</para>Second paragraph.<para>Third paragraph.</para>", ExpectedResult = "First paragraph.\n\nSecond paragraph.\n\nThird paragraph.")]
        [TestCase("First paragraph.<para>Second paragraph.</para>Third paragraph.", ExpectedResult = "First paragraph.\n\nSecond paragraph.\n\nThird paragraph.")]
        public string Transform_WritesExpectedMarkdown(string xmlContent)
        {
            return Transform(xmlContent);
        }
    }
}

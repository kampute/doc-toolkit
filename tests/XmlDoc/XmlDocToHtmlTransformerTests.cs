// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc
{
    using Kampute.DocToolkit.XmlDoc;
    using NUnit.Framework;

    [TestFixture]
    public class XmlDocToHtmlTransformerTests : XmlDocTransformerTester<XmlDocToHtmlTransformer>
    {
        [TestCase("<c>code</c>", ExpectedResult = "<code>code</code>")]
        [TestCase("<code>Console.WriteLine(\"&lt;Hello&gt;\");</code>", ExpectedResult = "<pre dir=\"ltr\" aria-label=\"Code snippet\"><code class=\"language-csharp\">Console.WriteLine(\"&lt;Hello&gt;\");</code></pre>")]
        [TestCase("<code language=\"python\">print(\"Hello, World!\")</code>", ExpectedResult = "<pre dir=\"ltr\" aria-label=\"Code snippet\"><code class=\"language-python\">print(\"Hello, World!\")</code></pre>")]
        [TestCase("<paramref name=\"value\"/>", ExpectedResult = "<code class=\"param-name\">value</code>")]
        [TestCase("<typeparamref name=\"T\"/>", ExpectedResult = "<code class=\"type-param-name\">T</code>")]
        [TestCase("<see langword=\"null\"/>", ExpectedResult = "<a href=\"https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/null\" rel=\"language-keyword\" class=\"see-link\"><code class=\"langword\">null</code></a>")]
        [TestCase("<see cref=\"T:System.String\"/>", ExpectedResult = "<a href=\"https://learn.microsoft.com/dotnet/api/system.string\" rel=\"code-reference\" class=\"see-link\">System.String</a>")]
        [TestCase("<see href=\"https://example.com\">Example</see>", ExpectedResult = "<a href=\"https://example.com\" class=\"see-link\">Example</a>")]
        [TestCase("<see href=\"topic\"/>", ExpectedResult = "<a href=\"topic\" class=\"see-link\">Title of topic</a>")]
        [TestCase("<list type=\"bullet\"><item><description>First</description></item><item><description>Second</description></item></list>", ExpectedResult = "<ul class=\"list-items\"><li>First</li><li>Second</li></ul>")]
        [TestCase("<list type=\"bullet\"><item><term>First</term><description>First description</description></item><item><term>Second</term><description>Second description</description></item></list>", ExpectedResult = "<ul class=\"list-items\"><li><span class=\"term\">First</span><span class=\"term-separator\"> \u2013 </span>First description</li><li><span class=\"term\">Second</span><span class=\"term-separator\"> \u2013 </span>Second description</li></ul>")]
        [TestCase("<list type=\"number\"><item><description>First</description></item><item><description>Second</description></item></list>", ExpectedResult = "<ol class=\"list-items\"><li>First</li><li>Second</li></ol>")]
        [TestCase("<list type=\"number\"><item><term>First</term><description>First description</description></item><item><term>Second</term><description>Second description</description></item></list>", ExpectedResult = "<ol class=\"list-items\"><li><span class=\"term\">First</span><span class=\"term-separator\"> \u2013 </span>First description</li><li><span class=\"term\">Second</span><span class=\"term-separator\"> \u2013 </span>Second description</li></ol>")]
        [TestCase("<list type=\"table\"><listheader><term>Header1</term><description>Header2</description></listheader><item><term>Row1Col1</term><description>Row1Col2</description></item><item><term>Row2Col1</term><description>Row2Col2</description></item></list>", ExpectedResult = "<table class=\"table-list\"><thead><tr><th class=\"term\">Header1</th><th>Header2</th></tr></thead><tbody><tr><td class=\"term\">Row1Col1</td><td>Row1Col2</td></tr><tr><td class=\"term\">Row2Col1</td><td>Row2Col2</td></tr></tbody></table>")]
        [TestCase("<note>This is a note.</note>", ExpectedResult = "<blockquote class=\"note\" role=\"note\"><div class=\"note-content\">This is a note.</div></blockquote>")]
        [TestCase("<note type=\"important\" title=\"Important\">This is an important note.</note>", ExpectedResult = "<blockquote class=\"note\" role=\"note\" data-type=\"important\" aria-label=\"Important\"><div class=\"note-title\" aria-hidden=\"true\">Important</div><div class=\"note-content\">This is an important note.</div></blockquote>")]
        [TestCase("<para>This is <c>inline code</c> and a <see href=\"https://example.com\">link</see>.</para>", ExpectedResult = "<p>This is <code>inline code</code> and a <a href=\"https://example.com\" class=\"see-link\">link</a>.</p>")]
        [TestCase("<para>Single paragraph.</para>", ExpectedResult = "<p>Single paragraph.</p>")]
        [TestCase("<para>First paragraph.</para><para>Second paragraph.</para><para>Third paragraph.</para>", ExpectedResult = "<p>First paragraph.</p><p>Second paragraph.</p><p>Third paragraph.</p>")]
        [TestCase("<para>First paragraph.</para>Second paragraph.<para>Third paragraph.</para>", ExpectedResult = "<p>First paragraph.</p>Second paragraph.<p>Third paragraph.</p>")]
        [TestCase("First paragraph.<para>Second paragraph.</para>Third paragraph.", ExpectedResult = "First paragraph.<p>Second paragraph.</p>Third paragraph.")]
        public string Transform_WritesExpectedHtml(string xmlContent)
        {
            return Transform(xmlContent);
        }
    }
}

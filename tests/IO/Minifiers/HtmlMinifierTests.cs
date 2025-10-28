// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.IO.Minifiers
{
    using Kampute.DocToolkit.IO.Minifiers;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class HtmlMinifierTests
    {
        // Empty and Whitespace-only Inputs
        [TestCase("", ExpectedResult = "")]
        [TestCase("   \n\r\t", ExpectedResult = "")]

        // Leading/Trailing Whitespace Around Elements
        [TestCase("   <div>Hello</div>", ExpectedResult = "<div>Hello</div>")]
        [TestCase("<div>Hello</div>   ", ExpectedResult = "<div>Hello</div>")]
        [TestCase("   <div>Hello</div>   ", ExpectedResult = "<div>Hello</div>")]

        // Whitespace Surrounding Tag Names
        [TestCase("<  div  >Hello<  /  div  >", ExpectedResult = "<div>Hello</div>")]
        [TestCase("<  br /  >Hello", ExpectedResult = "<br/>Hello")]

        // Basic Text Normalization
        [TestCase("Hello World", ExpectedResult = "Hello World")]
        [TestCase("  Hello  World  ", ExpectedResult = "Hello World")]
        [TestCase("Hello\nWorld", ExpectedResult = "Hello World")]
        [TestCase("Hello\tWorld", ExpectedResult = "Hello World")]
        [TestCase("Hello\r\nWorld", ExpectedResult = "Hello World")]

        // Comment Removal in Text Nodes
        [TestCase("Hello<!--comment-->World", ExpectedResult = "HelloWorld")]
        [TestCase("<!--comment-->Hello World", ExpectedResult = "Hello World")]
        [TestCase("   <!--comment-->   ", ExpectedResult = "")]
        [TestCase(" <!--leading comment--> <div>Hello</div>", ExpectedResult = "<div>Hello</div>")]
        [TestCase("<div>Hello</div> <!--trailing comment--> ", ExpectedResult = "<div>Hello</div>")]
        [TestCase("<!-- comment -->Text", ExpectedResult = "Text")]
        [TestCase("Text<!-- comment -->", ExpectedResult = "Text")]

        // Inline Element Whitespace Normalization
        [TestCase("   <span>   Hello  </span>   ", ExpectedResult = "<span>Hello</span>")]
        [TestCase("Text   <span>   Hello  </span>   ", ExpectedResult = "Text <span>Hello</span>")]
        [TestCase("   <span>   Hello  </span>   Text", ExpectedResult = "<span>Hello</span> Text")]
        [TestCase("Text <!-- comment --> <span>  Hello  </span>", ExpectedResult = "Text <span>Hello</span>")]
        [TestCase(" <strong>Hello</strong>  <strong>World</strong> ", ExpectedResult = "<strong>Hello</strong> <strong>World</strong>")]

        // Block-Level and Nested Element Normalization
        [TestCase("  <!--comment-->  <div>  <p>   Hello   </p>  <p>   World   </p>  </div>  <!--comment-->", ExpectedResult = "<div><p>Hello</p><p>World</p></div>")]
        [TestCase("Text <!-- comment --> <div>   <span>  A  </span>  B   </div> C <!-- comment -->", ExpectedResult = "Text<div><span>A</span> B</div>C")]
        [TestCase(" Text  <div>  Text  </div>  Text ", ExpectedResult = "Text<div>Text</div>Text")]
        [TestCase(" Text  <code>  Code  </code>  Text ", ExpectedResult = "Text <code>Code</code> Text")]
        [TestCase(" <code>  Code  </code>  <div>  Text  </div>  Text ", ExpectedResult = "<code>Code</code><div>Text</div>Text")]

        // Attribute Normalization
        [TestCase("<img   src  =  'https://example.com/image.png'  />", ExpectedResult = "<img src='https://example.com/image.png'/>")]
        [TestCase("<a href=\"  https://example.com  \" rel = external>Click\nHere</a>", ExpectedResult = "<a href=\"https://example.com\" rel=external>Click Here</a>")]
        [TestCase("<pre  class  = 'test  code'   />Hello\nWorld!</pre>", ExpectedResult = "<pre class='test code'/>Hello World!</pre>")]
        [TestCase("<pre  dir =  ltr > <code  lang = \"csharp\" >   int x = 10;   </code> </pre>", ExpectedResult = "<pre dir=ltr> <code lang=\"csharp\">   int x = 10;   </code> </pre>")]

        // Preformatted Elements
        [TestCase("<pre>  Hello  World  </pre>", ExpectedResult = "<pre>  Hello  World  </pre>")]
        [TestCase("   <pre>  Hello  World  </pre>   ", ExpectedResult = "<pre>  Hello  World  </pre>")]
        [TestCase("<div><pre>  Hello  </pre>Text<pre>  World  </pre></div>", ExpectedResult = "<div><pre>  Hello  </pre>Text<pre>  World  </pre></div>")]
        [TestCase("<pre>Some text <!-- comment inside pre --> More text</pre>", ExpectedResult = "<pre>Some text <!-- comment inside pre --> More text</pre>")]
        [TestCase("<script  src='script.js' />   Hello   World", ExpectedResult = "<script src='script.js'/>Hello World")]
        public string Write_OutputsMinifiedHtml(string text)
        {
            using var writer = new HtmlMinifier(new StringWriter());

            writer.Write(text);

            return writer.ToString();
        }

        [TestCase("Hello<!--comment-->World", ExpectedResult = "Hello<!--comment-->World")]
        [TestCase("<!--comment-->Hello World", ExpectedResult = "<!--comment-->Hello World")]
        [TestCase("   <!--comment-->   ", ExpectedResult = "<!--comment-->")]
        [TestCase(" <!--leading comment--> <div>Hello</div>", ExpectedResult = "<!--leading comment--><div>Hello</div>")]
        [TestCase("<div>Hello</div> <!--trailing comment--> ", ExpectedResult = "<div>Hello</div><!--trailing comment-->")]
        [TestCase("<!--  comment  -->Text", ExpectedResult = "<!--  comment  -->Text")]
        [TestCase("Text<!--  comment  -->", ExpectedResult = "Text<!--  comment  -->")]
        public string Write_PreserveComments_KeepsCommentsAsIs(string text)
        {
            using var writer = new HtmlMinifier(new StringWriter())
            {
                PreserveComments = true
            };

            writer.Write(text);

            return writer.ToString();
        }

        [TestCase("<a href='https://example.com' title=' Example\nVisit it! '>Click\nHere</a>", ExpectedResult = "<a href='https://example.com\' title=' Example\nVisit it! '>Click Here</a>")]
        public string Write_PreserveAttributeWhitespace_KeepsAttributeValuesAsIs(string text)
        {
            using var writer = new HtmlMinifier(new StringWriter())
            {
                PreserveAttributeWhitespace = true
            };

            writer.Write(text);

            return writer.ToString();
        }

        [Test]
        public void Dispose_WhenLeaveOpenIsFalse_ClosesUnderlyingWriter()
        {
            var stringWriter = new StringWriter();
            var writer = new HtmlMinifier(stringWriter, leaveOpen: false);

            writer.Dispose();

            Assert.Throws<ObjectDisposedException>(() => stringWriter.Write("Test"));
        }

        [Test]
        public void Dispose_WhenLeaveOpenIsTrue_DoesNotCloseUnderlyingWriter()
        {
            var stringWriter = new StringWriter();
            var writer = new HtmlMinifier(stringWriter, leaveOpen: true);

            writer.Dispose();

            Assert.DoesNotThrow(() => stringWriter.Write("Test"));
        }
    }
}

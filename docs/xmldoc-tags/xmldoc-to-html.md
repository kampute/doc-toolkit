# HTML Representation of XML Documentation Elements

## Overview
This reference describes how DocToolkit transforms XML documentation elements to their corresponding HTML representations.

**Note:** New lines and indentation in the output examples are provided solely for readability and are not present in the generated HTML.

## Text and Paragraphs

### Whitespace and Special Characters
Whitespace in text is normalized and any special HTML characters are encoded to prevent unintended formatting.

### Paragraphs
Paragraphs (`para` elements) are wrapped in `<p>` elements.

###### XML Input:
```xml
<para>This is the first paragraph.</para>
<para>This is the second paragraph.</para>
```

###### HTML Output:
```html
<p>This is the first paragraph.</p>
<p>This is the second paragraph.</p>
```

## Code Elements

### Inline Code
Inline code (`c` elements) is wrapped in `<code>` tags to visually distinguish it from regular text.

###### XML Input:
```xml
Use <c>Console.WriteLine()</c> to print to the console.
```

###### HTML Output:
```html
Use <code>Console.WriteLine()</code> to print to the console.
```

### Code Blocks
Code blocks (`code` elements) are wrapped in `<pre><code>` elements. When a language is specified via the `language` attribute, it is applied as a class to facilitate syntax highlighting.

###### XML Input:
```xml
<code language="javascript">
function sayHello() {
    console.log("Hello");
}
</code>
```

###### HTML Output:
```html
<pre dir="ltr" aria-label="Code snippet"><code class="language-javascript">function sayHello() {
    console.log("Hello");
}
</code></pre>
```

When no language is specified, the language specified by the documentation context will be used.

###### XML Input:
```xml
<code>
public void HelloWorld()
{
    Console.WriteLine("Hello, World!");
}
</code>
```

###### HTML Output:
```html
<pre dir="ltr" aria-label="Code snippet"><code class="language-csharp">public void HelloWorld()
{
    Console.WriteLine("Hello, World!");
}
</code></pre>
```

The content of a code block is normalized by performing the following operations:
- Leading and trailing empty lines are removed.
- Consecutive empty lines are collapsed into a single empty line.
- Tabs are converted to four spaces, and trailing whitespace is removed from each line.
- The minimum indentation across all non-empty lines is determined and removed from each line.

## Parameters and Type References

### Type Parameter Reference
Generic type parameters (`typeparamref` elements) are enclosed within a `<code>` element with the class `type-param-name`.

###### XML Input:
```xml
The <typeparamref name="T"/> parameter defines the element type.
```

###### HTML Output:
```html
The <code class="type-param-name">T</code> parameter defines the element type.
```

### Parameter Reference
References to parameters (`paramref` elements) are enclosed in a `<code>` element with the class `param-name`.

###### XML Input:
```xml
The <paramref name="input"/> parameter must not be null.
```

###### HTML Output:
```html
The <code class="param-name">input</code> parameter must not be null.
```

## Links and References

### Language Keyword Reference
Language keywords (`see` elements with `langword` attribute) are transformed into hyperlinks with the class `see-link` and the `rel` attribute set to `language-keyword`, containing a specially styled code element.

###### XML Input:
```xml
Use <see langword="null"/> to represent a null reference.
```

###### HTML Output:
```html
Use <a href="https://docs.example.com/keywords/null" rel="language-keyword" class="see-link"><code class="langword">null</code></a> to represent a null reference.
```

### Code Reference
Code references (`see` elements with `cref` attribute) are transformed into hyperlinks with the class `see-link` and the `rel` attribute set to `code-reference` that point to the corresponding documentation. If no explicit link text is provided, the title is automatically generated based on the referenced type or member.

###### XML Input:
```xml
See <see cref="T:System.DateTime"/> type for date and time operations.
```

###### HTML Output:
```html
See <a href="/docs/System.DateTime" rel="code-reference" class="see-link">DateTime</a> type for date and time operations.
```

When explicit text is provided within a code reference, it is used as the link text.

###### XML Input:
```xml
<see cref="M:System.IO.File.ReadAllText">Read text from a file</see>
```

###### HTML Output:
```html
<a href="/docs/System.IO.File.ReadAllText" rel="code-reference" class="see-link">Read text from a file</a>
```

### External URL Reference
External URL references (`see` elements with `href` attribute) are transformed into standard anchor elements with the class `see-link`.

###### XML Input:
```xml
Visit <see href="https://docs.example.com">our documentation</see> for more details.
```

###### HTML Output:
```html
Visit <a class="see-link" href="https://docs.example.com">our documentation</a> for more details.
```

If no explicit text is provided for an external URL, the URL itself is used as the link text.

###### XML Input:
```xml
<see href="https://docs.example.com" />
```

###### HTML Output:
```html
<a class="see-link" href="https://docs.example.com">https://docs.example.com</a>
```

## Lists

### Bullet List
Standard lists (`list` elements without a type or with `type="bullet"`) are transformed into unordered lists with the class `list-items`.

###### XML Input:
```xml
<list>
  <item>First item</item>
  <item>Second item</item>
  <item>Third item</item>
</list>
```

###### HTML Output:
```html
<ul class="list-items">
  <li>First item</li>
  <li>Second item</li>
  <li>Third item</li>
</ul>
```

### Numbered List
Numbered lists (`list` elements with `type="number"`) are transformed into ordered lists with the class `list-items`.

###### XML Input:
```xml
<list type="number">
  <item>First item</item>
  <item>Second item</item>
  <item>Third item</item>
</list>
```

###### HTML Output:
```html
<ol class="list-items">
  <li>First item</li>
  <li>Second item</li>
  <li>Third item</li>
</ol>
```

### List with Term/Description Pairs
When list items contain term-description pairs, the term is displayed with a special `term` class followed by an en dash separator and then the description.

###### XML Input:
```xml
<list type="bullet">
  <item>
    <term>Term 1</term>
    <description>Description for term 1</description>
  </item>
  <item>
    <term>Term 2</term>
    <description>Description for term 2</description>
  </item>
</list>
```

###### HTML Output:
```html
<ul class="list-items">
  <li>
    <span class="term">Term 1</span>
    <span class="term-separator"> – </span>
    Description for term 1
  </li>
  <li>
    <span class="term">Term 2</span>
    <span class="term-separator"> – </span>
    Description for term 2
  </li>
</ul>
```

### Table-structured List
Table-structured lists (`list` elements with `type="table"`) are transformed into HTML tables with headers and body rows.

###### XML Input:
```xml
<list type="table">
  <listheader>
    <term>Parameter</term>
    <description>Description</description>
  </listheader>
  <item>
    <term>name</term>
    <description>The user's name</description>
  </item>
  <item>
    <term>age</term>
    <description>The user's age</description>
  </item>
</list>
```

###### HTML Output:
```html
<table class="table-list">
  <thead>
    <tr>
      <th class="term">Parameter</th>
      <th>Description</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td class="term">name</td>
      <td>The user's name</td>
    </tr>
    <tr>
      <td class="term">age</td>
      <td>The user's age</td>
    </tr>
  </tbody>
</table>
```

## Notes and Admonitions

### Simple Note
Notes (`note` elements) are wrapped in a `<blockquote>` element with the class `note`. The content is enclosed in a nested `<div>` element with the class `note-content`.

###### XML Input:
```xml
<note>This is an important detail to remember.</note>
```

###### HTML Output:
```html
<blockquote class="note" role="note">
  <div class="note-content">This is an important detail to remember.</div>
</div>
```

### Note with Type
Notes can have different types (info, tip, security, etc.) specified via the `type` attribute. The type is added as a data attribute to the `<div>` element.

###### XML Input:
```xml
<note type="important">This operation cannot be undone.</note>
```

###### HTML Output:
```html
<blockquote class="note" role="note" data-type="important">
  <div class="note-content">This operation cannot be undone.</div>
</blockquote>
```

### Note with Title
Notes can have titles specified via the `title` attribute. The title is displayed in an `<div>` element with the class `note-title`.

###### XML Input:
```xml
<note type="security" title="Security Warning">Do not share your password.</note>
```

###### HTML Output:
```html
<blockquote class="note" role="note" data-type="security" aria-label="Security Warning">
  <div class="note-title" aria-hidden="true">Security Warning</div>
  <div class="note-content">Do not share your password.</div>
</blockquote>
```

### Note with Complex Content
Notes can contain complex content including paragraphs, lists, and other elements.

###### XML Input:
```xml
<note type="tip" title="Tip">
  <para>You can optimize performance by:</para>
  <list>
    <item>Using indexes</item>
    <item>Caching results</item>
  </list>
</note>
```

###### HTML Output:
```html
<blockquote class="note" role="note" data-type="tip" aria-label="Tip">
  <div class="note-title" aria-hidden="true">Tip</div>
  <div class="note-content">
    You can optimize performance by:
    <ul class="list-items">
      <li>Using indexes</li>
      <li>Caching results</li>
    </ul>
  </div>
</blockquote>
```
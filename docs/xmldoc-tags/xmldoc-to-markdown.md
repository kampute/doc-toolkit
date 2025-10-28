# Markdown Representation of XML Documentation Elements

## Overview
This reference describes how DocToolkit transforms XML documentation elements to their corresponding Markdown representations.

## Basic Text Handling

### Whitespace and Special Characters
Whitespace in text is normalized and any special Markdown characters are escaped to prevent unintended formatting.

### Paragraphs
Paragraphs (`para` elements) are converted into Markdown paragraphs by inserting an empty line before and after their content when appropriate. When paragraphs are interleaved with other text nodes, the stylesheet ensures proper separation by inserting empty lines around the `para` elements.

###### XML Input:
```xml
<para>This is the first paragraph.</para>
<para>This is the second paragraph.</para>
```

###### Markdown Output:
```md
This is the first paragraph.

This is the second paragraph.
```

###### XML Input:
```xml
Some text before.
<para>Paragraph content.</para>
Some text after.
```

###### Markdown Output:
```md
Some text before.

Paragraph content.

Some text after.
```

## Code Elements

### Inline Code
Inline code segments wrapped in `c` elements are rendered as inline code in Markdown by surrounding the text with backticks.

###### XML Input:
```xml
Use <c>Console.WriteLine()</c> to print to the console.
```

###### Markdown Output:
```md
Use `Console.WriteLine()` to print to the console.
```

### Code Blocks
Code blocks (`code` elements) are transformed into fenced code blocks. If a `language` attribute is provided, its value is appended after the opening backticks to enable syntax highlighting. Otherwise, the language specified by the documentation context will be used.

###### XML Input (With Language):
```xml
<code language="javascript">
function sayHello() {
    console.log("Hello");
}
</code>
```

###### Markdown Output:
````md
```javascript
function sayHello() {
    console.log("Hello");
}
```
````

###### XML Input (Without Language):
```xml
<code>
public void HelloWorld()
{
    Console.WriteLine("Hello, World!");
}
</code>
```

###### Markdown Output:
````md
```csharp
public void HelloWorld()
{
    Console.WriteLine("Hello, World!");
}
```
````

The content of a code block is normalized by removing leading and trailing empty lines, collapsing consecutive empty lines, converting tabs to spaces, removing trailing whitespace, and eliminating common indentation.

## Parameter and Type Parameter References
Both parameter (`paramref`) and generic type parameter (`typeparamref`) references are rendered as inline code by enclosing the name in backticks.

###### XML Input:
```xml
The <paramref name="input"/> parameter must not be null.
The <typeparamref name="T"/> parameter defines the element type.
```

###### Markdown Output:
```md
The `input` parameter must not be null.
The `T` parameter defines the element type.
```

## Links and References

### Language Keyword Reference
When a `see` element has a `langword` attribute, the stylesheet attempts to generate a Markdown link. If a URL is available, the keyword is wrapped in backticks and linked; otherwise, it is output as inline code.

###### XML Input:
```xml
Use <see langword="null"/> to represent a null reference.
```

###### Markdown Output:
```md
Use [`null`](https://docs.example.com/keywords/null) to represent a null reference.
```

### Code References
A `see` element with a `cref` attribute is transformed into a Markdown link that points to the corresponding documentation. If no explicit link text is provided, the title is automatically generated based on the referenced type or member.

###### XML Input (Auto Text):
```xml
See <see cref="T:System.DateTime"/> type for date and time operations.
```

###### Markdown Output:
```md
See [DateTime](/docs/System.DateTime) type for date and time operations.
```

###### XML Input (Custom Text):
```xml
<see cref="M:System.IO.File.ReadAllText">Read text from a file</see>
```

###### Markdown Output:
```md
[Read text from a file](/docs/System.IO.File.ReadAllText)
```

### External URL References
For `see` elements with an `href` attribute, the stylesheet creates a standard Markdown link. If no explicit text is provided, the URL itself is used as the link text.

###### XML Input (With Text):
```xml
Visit <see href="https://docs.example.com">our documentation</see> for more details.
```

###### Markdown Output:
```md
Visit [our documentation](https://docs.example.com) for more details.
```

###### XML Input (Without Text):
```xml
<see href="https://docs.example.com" />
```

###### Markdown Output:
```md
[https://docs.example.com](https://docs.example.com)
```

## Lists

### Bullet and Numbered Lists
Standard `list` elements (with no type or with `type="bullet"`) are transformed into unordered lists in Markdown. Lists with `type="number"` are converted into numbered lists.

###### XML Input (Bullet):
```xml
<list>
  <item>First item</item>
  <item>Second item</item>
  <item>Third item</item>
</list>
```

###### Markdown Output:
```md
- First item
- Second item
- Third item
```

###### XML Input (Numbered):
```xml
<list type="number">
  <item>First item</item>
  <item>Second item</item>
  <item>Third item</item>
</list>
```

###### Markdown Output:
```md
1. First item
2. Second item
3. Third item
```

When list items contain term-description pairs, the term is emphasized and separated from its description by an en-dash.

###### XML Input (Term/Description):
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

###### Markdown Output:
```md
- **Term 1** – Description for term 1
- **Term 2** – Description for term 2
```

### Tables
For table-structured lists (`type="table"`), the stylesheet outputs a Markdown table.

###### XML Input (Table):
```xml
<list type="table">
  <listheader>
    <term>Parameter</term>
    <description>Required</description>
    <description>Description</description>
  </listheader>
  <item>
    <term>name</term>
    <description>Yes</description>
    <description>The user's name</description>
  </item>
  <item>
    <term>age</term>
    <description>No</description>
    <description>The user's age</description>
  </item>
</list>
```

###### Markdown Output:
```md
|Parameter|Required|Description|
|---|---|---|
|name|Yes|The user's name|
|age|No|The user's age|
```

**Important:** For proper rendering, Markdown tables must have a header row and contain only single-paragraph text in each cell.

## Notes and Admonitions
A `note` element is rendered as a Markdown blockquote. Notes can include titles, types, and complex content including paragraphs and lists.

###### XML Input (Simple):
```xml
<note>This is an important detail to remember.</note>
```

###### Markdown Output:
```md
> This is an important detail to remember.
```

###### XML Input (With Title):
```xml
<note type="security" title="Security Warning">Do not share your password.</note>
```

###### Markdown Output:
```md
> **Security Warning**
> Do not share your password.
```

###### XML Input (Complex):
```xml
<note type="tip" title="Tip">
  <para>You can optimize performance by:</para>
  <list>
    <item>Using indexes</item>
    <item>Caching results</item>
  </list>
</note>
```

###### Markdown Output:
```md
> **Tip**
> You can optimize performance by:
> - Using indexes
> - Caching results
```
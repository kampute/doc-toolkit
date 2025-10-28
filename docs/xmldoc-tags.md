# Supported XML Documentation Tags

This reference guide details XML documentation tags supported by DocToolkit, and their usage.

## Standard XML Documentation Tags

The following standard XML documentation tags are fully supported:

| Tag | Top-Level | Description | Example |
|-----|-----------|-------------|---------|
| [`<c>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#c) | No | Used for inline code snippets within documentation. It renders short code fragments inline with text. | `Use <c>int</c> for integer values` |
| [`<code>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#code) | No | Represents a block of code formatted in a pre-formatted style, typically for multi-line code samples. | `<code>var x = new Example();</code>` |
| [`<example>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#example) | Yes | Provides an example demonstrating how a type or member is used in practice. | `<example>This shows basic usage...</example>` |
| [`<exception>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#exception) | Yes | Documents an exception that a method or property may throw. | `<exception cref="ArgumentNullException">...</exception>` |
| [`<include>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#include) | Yes | Allows external XML documentation to be included within the current documentation file. | `<include file="docs.xml" path="doc/member[@name='T:MyType']/*" />` |
| [`<inheritdoc>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#inheritdoc) | Yes | Inherits documentation from a base class or implemented interface. | `<inheritdoc cref="BaseClass.Method" />` |
| [`<list>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#list) | Yes | Defines a list within documentation, such as bullet points, numbered lists, or table to structure information. | `<list type="bullet">...</list>` |
| [`<para>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#para) | No | Inserts a paragraph break within documentation. | `<para>This is a new paragraph.</para>` |
| [`<param>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#param) | Yes | Describes a parameter of a constructor, method, or index property. | `<param name="value">The input value</param>` |
| [`<paramref>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#paramref) | No | Refers to a specific method parameter within documentation. | `When <paramref name="count"/> is negative...` |
| [`<permission>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#permission) | Yes | Declares required security permission for accessing a type or member. | `<permission cref="SecurityPermission">...</permission>` |
| [`<remarks>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#remarks) | Yes | Provides additional information or context about an element that may not fit within the summary. | `<remarks>This is used internally...</remarks>` |
| [`<returns>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#returns) | Yes | Describes the return value of a method or delegate. | `<returns>The calculated value</returns>` |
| [`<see>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#see) | No | Creates hyperlinks to related types, members, language-specific keywords, or external pages within the documentation. | `See <see cref="OtherClass"/> for details` |
| [`<seealso>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#seealso) | Yes | Adds a 'See Also' reference linking to related documentation or external resources. | `<seealso cref="RelatedMethod"/>` |
| [`<summary>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#summary) | Yes | Provides a brief description summarizing the purpose of a type or member. | `<summary>Represents a configuration...</summary>` |
| [`<typeparam>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#typeparam) | Yes | Describes a generic type parameter used in methods or types. | `<typeparam name="T">The type of items</typeparam>` |
| [`<typeparamref>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#typeparamref) | No | References a generic type parameter within documentation. | `The <typeparamref name="T"/> parameter must be...` |
| [`<value>`](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#value) | Yes | Specifies a detailed description of a property value. | `<value>The current connection state</value>` |

## Extended XML Documentation Tags

Additionally, the following extended tags from [Sandcastle](https://github.com/EWSoftware/SHFB) are supported:

| Tag | Top-Level | Description | Example |
|-----|-----------|-------------|---------|
| [`<event>`](https://ewsoftware.github.io/XMLCommentsGuide/html/81bf7ad3-45dc-452f-90d5-87ce2494a182.htm) | Yes | Describes an event that a method or property may raise. | `<event cref="MyEvent">Raised when...</event>` |
| [`<note>`](https://ewsoftware.github.io/XMLCommentsGuide/html/4302a60f-e4f4-4b8d-a451-5f453c4ebd46.htm) | No | Creates a note-like section within a topic to draw attention to some important information. | `<note type="warning" title="Important">Be careful to...</note>` |
| [`<overloads>`](https://ewsoftware.github.io/XMLCommentsGuide/html/5b11b235-2b6c-4dfc-86b0-2e7dd98f2716.htm) | Yes | Provides common documentation for a set of overloaded members. | `<overloads>Common description for all overloads</overloads>` |
| [`<threadsafety>`](https://ewsoftware.github.io/XMLCommentsGuide/html/fb4625cb-52d0-428e-9c7c-7a0d88e1b692.htm) | Yes | Specifies thread safety of a class or structure, detailing whether static and instance members are safe for concurrent use. | `<threadsafety static="true" instance="false"/>` |

## Understanding Top-Level Tags

Tags marked as "Top-Level" in the tables above represent major documentation sections. These sections typically appear as distinct segments in the generated documentation with their own headings. Non-top-level tags are used for inline formatting or for referencing within other sections.

## Markdown Considerations

When authoring documentation intended for Markdown output, ensure that all tables include a header row. Omitting the `<listheader>` element in a `<list type="table">` will result in an invalid table that may not render correctly.

Furthermore, Markdown renderers do not support block elements or line breaks within table cells; restrict content in these cells to a single paragraph of text and inline formatting only.

## Documenting Namespaces

.NET compilers do not natively support XML documentation comments for namespaces. To provide documentation for a namespace, define a class named `NamespaceDoc` within the desired namespace and add XML comments to that class. DocToolkit recognizes this convention and uses the documentation from `NamespaceDoc` as the namespace-level documentation.

```csharp
namespace MyNamespace
{
    /// <summary>
    /// Summary for MyNamespace.
    /// </summary>
    /// <remarks>
    /// Remarks for MyNamespace.
    /// </remarks>
    /// <example>
    /// Example usage of MyNamespace.
    /// </example>
    /// <seealso cref="MyOtherNamespace"/>
    internal static class NamespaceDoc
    {
        // This class will not be exported into the assembly.
        // It exists only to hold the namespace documentation.
    }
}
```

## Related Resources

- [C# XML Documentation Guide](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/)
- [Sandcastle Documentation](https://ewsoftware.github.io/XMLCommentsGuide/html/4268757F-CE8D-4E6D-8502-4F7F2E22DDA3.htm)
- [DocFX Documentation](https://dotnet.github.io/docfx/)

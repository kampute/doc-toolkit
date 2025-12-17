# Copilot Instructions for Kampute.DocToolkit

## Project Overview
This is a .NET library that extracts metadata from .NET assemblies and transforms XML documentation comments into structured documentation models. It provides a **pipeline architecture** rather than fixed output - you implement custom renderers while the library handles reflection, XML processing, and cross-reference resolution.

## Core Architecture Pattern
The library follows a **three-phase pipeline**:
1. **Metadata Extraction**: Reflects over .NET assemblies to build object models (`src/Models/`, `src/Collections/`)
2. **XML Processing**: XSLT-based transformation of XML documentation (`src/XmlDoc/`, `src/Xslt/`)
3. **Output Generation**: Custom renderers process models into final formats (`DocumentPageRenderer`)

## Key Components & Workflows

### Documentation Context Setup
Always use the builder pattern via `DocumentationContextBuilder`:
```csharp
var context = DocumentationContextBuilder
    .DevOpsWiki()  // or .DocFx(), .DotNetApi()
    .AddAssembly("path/to/assembly.dll")
    .AddXmlDoc("path/to/assembly.xml")
    .Build();
```

### Custom Renderers
Extend `DocumentPageRenderer` and implement the abstract `Render` method:
- Input: `PageCategory` enum + `IModel` (different model types for types vs members)
- Output: Write to `TextWriter` in your target format
- The `PageCategory` enum drives different rendering logic (see `src/PageCategory.cs`)

### Models Architecture
The library uses distinct model hierarchies in `src/Models/`:
- **Base**: `MemberModel` (implements `IDocumentModel`) - base for types and type members
- **Types**: `TypeModel` → `CompositeTypeModel` (classes, structs) - represent .NET types with metadata
- **Members**: `TypeMemberModel` (methods, properties, fields, etc.) - represent type members with signatures
- **Namespace/Topic**: `NamespaceModel`, `TopicModel` - implement `IDocumentModel` directly
- **Assembly**: `AssemblyModel` - container for types, does not implement `IDocumentModel`
- **Documentation**: `XmlDocEntry` objects contain structured XML documentation (summary, remarks, parameters, etc.)
- **Collections**: Specialized collections (`TopicCollection`, `TypesCollection`, `OverloadCollection`, `PatternCollection`) with filtering/grouping

Concrete model classes: `ClassModel`, `MethodModel`, `PropertyModel`, `FieldModel`, etc.

### XSLT Integration Pattern
XML doc transformation uses embedded XSLT resources:
- HTML: `src/Xslt/Resources/html.xslt`
- Markdown: `src/Xslt/Resources/md.xslt`
- Access via `XsltCompiler.CompileEmbeddedResource("html"|"md")`
- Custom transformers extend `XmlDocTransformer` class

## Testing Patterns
- Uses **NUnit** with **Moq** for mocking
- Use `[TestCase(..., ExpectedResult=...)]` for parameterized tests.
- Name test methods: `MemberName_StateUnderTest_ExpectedBehavior`.
- When applicable, use types and members defined in `Acme.cs` for test cases.
- Prefer constraint-based assertions.
- Write only tests for public members and business logic (skip private methods and parameter validation).
- `tests/MockHelper.cs` provides common test utilities
- Mock contexts via `MockHelper.CreateDocumentationContext<TFormat>(assemblies, topics)`
- Test files mirror `src/` structure in `tests/`

## Build & Development
- **Target**: netstandard2.1
- **Signed assemblies** (when `SigningKey.snk` exists)
- Build: `dotnet build DocToolkit.sln`
- Test: `dotnet test`
- Package: Auto-generates NuGet on build (`GeneratePackageOnBuild=true`)

## File Organization Conventions
- **Namespace folders**: Each namespace gets its own folder (`Collections/`, `Formatters/`, etc.)
- **NamespaceDoc.cs**: Every namespace folder contains this file with namespace-level XML documentation
- **Embedded resources**: XSLT files in `src/Xslt/Resources/` are embedded as resources
- **Interface segregation**: Small, focused interfaces (e.g., `IXmlDocTransformer`, `IDocumentRenderer`)

## Configuration & Target Platforms
Built-in support for common documentation platforms via addressing strategies:
- **Azure DevOps Wiki**: Markdown with `.md` extensions in URLs
- **DocFx**: HTML-based with specific URL patterns
- **Microsoft Docs**: .NET API browser conventions
- Each has corresponding `*Options` and `*Page` classes

## Critical Dependencies
- **System.Xml.Xsl**: Core XSLT transformation engine
- **System.Reflection**: Assembly metadata extraction
- **Embedded XSLT**: Never modify XSLT files without rebuilding - they're embedded resources

When adding new output formats, follow the `HtmlFormat`/`MarkdownFormat` pattern: implement `IDocumentFormatter`, create corresponding XSLT transformer, and register text transformers for link processing.

## Development Guidelines
- **SOLID principles**: Prioritize Single Responsibility and Open/Closed principles
- **Simplicity over complexity**: Avoid excessive helper methods and unnecessary abstractions
- **Self-documenting code**: Code should be readable without redundant inline comments
- **Interface pragmatism**: Use interfaces judiciously - avoid "Ravioli" (too many small interfaces) and "Lasagna" (too many layers)
- **Performance & clarity**: Optimize for both execution speed and code understanding
- **Problem-solving approach**: Question existing solutions, propose improvements, document limitations when needed

## Documentation Guidelines
- Avoid promotional, flowery, or overly embellished language, and use adjectives and adverbs only when strictly necessary.
- Emphasize purpose and usage; do not document implementation details or obvious information.
- Provide meaningful context and call out important behavioral nuances or edge cases.
- Keep content concise and focused by using short sentences and brief paragraphs to convey information clearly.
- Organize content using bullet points, numbered lists, or tables when appropriate, and explain the context or purpose of the list or table in at least one paragraph.
- Numbered lists should be used for steps in a process or sequence only.
- When writing XML documentation comments, use appropriate XML tags for references, language keywords, and for organizing information in lists and tables. Ensure tags are used to clarify context, structure information, and improve readability for consumers of the documentation.
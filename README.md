# DocToolkit

DocToolkit is a .NET library that builds structured documentation models from assemblies and their XML comments. It separates metadata extraction, XML processing, and output generation, enabling you to create custom documentation formats with full control over rendering and presentation.

## How It Works

DocToolkit separates metadata extraction from output generation, giving you full control over the final documentation format:

1. **Metadata Extraction**: Uses reflection-only assemblies via MetadataLoadContext to discover types, members, and relationships
2. **XML Processing**: Parses XML documentation files and transforms them using XSLT stylesheets
3. **Output Generation**: Passes structured models to your custom renderer for final output

## Key Features

- **Specialized TextWriters**: Built-in HTML and Markdown TextWriter wrappers with minification support
- **Customizable URL Management**: Built-in support for Azure DevOps Wiki, Microsoft Docs, and DocFx
- **Conceptual Topics**: Support for standalone documentation topics beyond API reference
- **Extensible**: Add custom URL schemes, XML transformations, and output formats

Explore the [API documentation](https://kampute.github.io/doc-toolkit/) to learn about all available features of DocToolkit.

## Quick Start

Install via NuGet:
```bash
dotnet add package Kampute.DocToolkit
```

Basic usage:
```csharp
// 1. Create a documentation renderer
public class MyDocumentRenderer : DocumentPageRenderer
{
    protected override void Render(TextWriter writer, PageCategory category, IModel model)
    {
        // Your custom rendering logic here
    }
}

// 2. Configure the documentation context
using var context = DocumentationContextBuilder
    .DevOpsWiki()
    .AddAssembly("path/to/MyLibrary.dll")
    .AddXmlDoc("path/to/MyLibrary.xml")
    .Build();

// 3. Generate documentation
var renderer = new MyDocumentRenderer();
var composer = new FileSystemDocumentationComposer(renderer);
composer.GenerateDocumentation(context, "/output/directory");
```

## Contributing

Contributions welcome! Please follow the existing coding and documentation conventions to maintain consistency across the codebase.

1. Fork the repository
2. Create a feature branch: `git checkout -b feature-name`
3. Commit changes: `git commit -m 'Add feature'`
4. Push branch: `git push origin feature-name`
5. Open a pull request

## License

Licensed under the [MIT License](LICENSE).
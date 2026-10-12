# AGENTS.md

Kampute.DocToolkit is a .NET library, published on NuGet, that builds documentation models from assemblies and their XML documentation comments. It does not render pages; consumers such as Kampose supply the renderers.

## Build and test

- Library: `src/Kampute.DocToolkit.csproj`, targeting `netstandard2.1`. Tests: `tests/Kampute.DocToolkit.Test.csproj`, targeting `net10.0`, with NUnit and Moq.
- CI runs `dotnet build -c Release` and then `dotnet test --no-build -c Release`. Use the same configuration locally.
- Run a focused subset with `dotnet test --filter "FullyQualifiedName~ClassName"`.
- CI also runs `kampose build` (configured in `kampose.json`), which audits the XML documentation of the built assembly and stops on the issues it finds.

## Code conventions

- Follow `.editorconfig`: the copyright header from `file_header_template`, block-scoped namespaces, `using` directives inside the namespace, and LF line endings.
- Document every member with XML documentation comments, except for private fields.
- Keep the build free of warnings and analyzer issues. Use `#pragma warning disable` only for false positives, and add a comment with the justification.
- Tests mirror the `src` folder structure in the `Kampute.DocToolkit.Test` namespace. Prefer `[TestCase(..., ExpectedResult = ...)]` for input-to-output cases.

## Compatibility

- The public and protected API is a published contract. Do not remove or change a member's signature, or add a member to a public interface, without a major version.
- When a change alters documented behavior, state the old and new behavior in the commit message.

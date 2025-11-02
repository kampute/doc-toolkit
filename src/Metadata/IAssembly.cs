// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    /// Provides a unified interface for accessing assembly metadata.
    /// </summary>
    /// <remarks>
    /// This interface abstracts assembly metadata access, allowing consistent operations regardless of
    /// how the assembly is loaded (e.g., Common Language Runtime or MetadataLoadContext).
    /// </remarks>
    public interface IAssembly : IMetadataAdapter<Assembly>, IWithCustomAttributes
    {
        /// <summary>
        /// Gets the simple name of the assembly.
        /// </summary>
        /// <value>
        /// The simple name of the assembly.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets the identity of the assembly.
        /// </summary>
        /// <value>
        /// The <see cref="AssemblyName"/> representing the assembly's identity.
        /// </value>
        AssemblyName Identity { get; }

        /// <summary>
        /// Gets the modules defined in the assembly.
        /// </summary>
        /// <value>
        /// A read-only collection of modules defined in the assembly.
        /// </value>
        IReadOnlyCollection<Module> Modules { get; }

        /// <summary>
        /// Gets the namespaces exported by the assembly and their associated types.
        /// </summary>
        /// <value>
        /// A lookup mapping namespace names to their corresponding types. The namespaces are ordered by their names,
        /// and the types within each namespace are ordered by their full names.
        /// </value>
        IReadOnlyDictionary<string, IReadOnlyList<IType>> Namespaces { get; }

        /// <summary>
        /// Gets the types exported by the assembly.
        /// </summary>
        /// <value>
        /// A read-only collection of top-level types exported by the assembly, ordered by their full names.
        /// </value>
        IReadOnlyCollection<IType> ExportedTypes { get; }

        /// <summary>
        /// Gets the identity of assemblies referenced by this assembly.
        /// </summary>
        /// <value>
        /// A read-only collection of referenced assembly names.
        /// </value>
        IReadOnlyCollection<AssemblyName> ReferencedAssemblies { get; }

        /// <summary>
        /// Gets a dictionary of metadata attributes of the assembly.
        /// </summary>
        /// <value>
        /// A read-only dictionary where keys are attribute names and values are their corresponding values.
        /// </value>
        IReadOnlyDictionary<string, object?> Attributes { get; }

        /// <summary>
        /// Gets a value indicating whether the assembly is strongly named.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the assembly has a public key indicating it is strongly named; otherwise, <see langword="false"/>.
        /// </value>
        bool IsStronglyNamed { get; }

        /// <summary>
        /// Gets a value indicating whether the assembly is loaded in reflection-only context.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the assembly is loaded in reflection-only context; otherwise, <see langword="false"/>.
        /// </value>
        bool IsReflectionOnly { get; }

        /// <summary>
        /// Gets the repository used to create and cache member metadata of the assembly.
        /// </summary>
        /// <value>
        /// The <see cref="IMemberAdapterRepository"/> instance that manages member metadata for the assembly.
        /// </value>
        IMemberAdapterRepository Repository { get; }

        /// <summary>
        /// Attempts to retrieve metadata of a type exported by the assembly using its full name.
        /// </summary>
        /// <param name="fullName">The full name of the type to retrieve.</param>
        /// <param name="type">When this method returns, contains the type if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the type is found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fullName"/> is <see langword="null"/>.</exception>
        bool TryGetType(string fullName, [NotNullWhen(true)] out IType? type);
    }
}

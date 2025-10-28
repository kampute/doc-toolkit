// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// An adapter that wraps an <see cref="Assembly"/> and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="Assembly"/> and the metadata
    /// representation defined by the <see cref="IAssembly"/> interface. It provides access to assembly-level
    /// information regardless of whether the assembly was loaded via Common Language Runtime (CLR) or
    /// Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class AssemblyAdapter : AttributeAwareMetadataAdapter<Assembly>, IAssembly
    {
        private readonly Lazy<ILookup<string, IType>> namespaces;
        private readonly Lazy<SortedDictionary<string, IType>> exportedTypes;
        private readonly Lazy<IReadOnlyDictionary<string, object?>> attributes;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyAdapter"/> class.
        /// </summary>
        /// <param name="assembly">The reflection information of the assembly to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> is <see langword="null"/>.</exception>
        public AssemblyAdapter(Assembly assembly)
            : base(assembly)
        {
            Repository = new MemberAdapterRepository(this);
            Identity = assembly.GetName();

            exportedTypes = new(() =>
            {
                var factory = Repository.GetTypeMetadata;
                var types = new SortedDictionary<string, IType>(StringComparer.Ordinal);
                foreach (var type in GetExportedTypes())
                {
                    var metadata = factory(type);
                    types[NormalizeTypeName(metadata.FullName)] = metadata;
                }
                return types;
            });

            namespaces = new(() => ExportedTypes.ToLookup(type => type.Namespace, StringComparer.Ordinal));

            attributes = new(GetMetadataAttributes);
        }

        /// <inheritdoc/>
        public override string Name => Identity.Name ?? Reflection.ManifestModule.Name;

        /// <inheritdoc/>
        public virtual AssemblyName Identity { get; }

        /// <inheritdoc/>
        public virtual IReadOnlyCollection<Module> Modules => Reflection.GetModules();

        /// <inheritdoc/>
        public ILookup<string, IType> Namespaces => namespaces.Value;

        /// <inheritdoc/>
        public IReadOnlyCollection<IType> ExportedTypes => exportedTypes.Value.Values;

        /// <inheritdoc/>
        public virtual IReadOnlyCollection<AssemblyName> ReferencedAssemblies => Reflection.GetReferencedAssemblies();

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, object?> Attributes => attributes.Value;

        /// <inheritdoc/>
        public virtual bool IsStronglyNamed => Identity.Flags.HasFlag(AssemblyNameFlags.PublicKey);

        /// <inheritdoc/>
        public virtual bool IsReflectionOnly => Reflection.ReflectionOnly || Reflection.GetType().Assembly != typeof(Assembly).Assembly;

        /// <inheritdoc/>
        public virtual IMemberAdapterRepository Repository { get; }

        /// <inheritdoc/>
        public bool TryGetType(string fullName, [NotNullWhen(true)] out IType? type)
        {
            if (fullName is null)
                throw new ArgumentNullException(nameof(fullName));

            return exportedTypes.Value.TryGetValue(NormalizeTypeName(fullName), out type);
        }

        /// <summary>
        /// Retrieves the types exported by the assembly.
        /// </summary>
        /// <returns>An enumerable collection of types exported by the assembly.</returns>
        protected virtual IEnumerable<Type> GetExportedTypes() => Reflection.ExportedTypes;

        /// <summary>
        /// Retrieves the assembly's metadata attributes as key-value pairs.
        /// </summary>
        /// <returns>A read-only dictionary containing the assembly's metadata attributes.</returns>
        /// <remarks>
        /// This method extracts custom attributes from the assembly that follow the naming convention <c>System.Reflection.Assembly*Attribute</c>
        /// and maps them to key-value pairs.
        /// <para>
        /// The attributes of type <see cref="AssemblyMetadataAttribute"/> are treated specially, where the first constructor argument is used as
        /// the key and the second as the value.
        /// </para>
        /// The method uses case-insensitive keys and ignores duplicate attribute names, keeping the first occurrence.
        /// </remarks>
        protected virtual IReadOnlyDictionary<string, object?> GetMetadataAttributes()
        {
            const string NamePrefix = "System.Reflection.Assembly";
            const string NameSuffix = "Attribute";
            const string MetadataName = "Metadata";

            var attributes = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var attr in Reflection.CustomAttributes)
            {
                var arguments = attr.ConstructorArguments;
                if (arguments.Count == 0)
                    continue;

                var attrName = attr.AttributeType.FullName;
                if
                (
                    attrName is not null &&
                    attrName.Length > NamePrefix.Length + NameSuffix.Length &&
                    attrName.StartsWith(NamePrefix, StringComparison.Ordinal) &&
                    attrName.EndsWith(NameSuffix, StringComparison.Ordinal)
                )
                {
                    attrName = attrName[NamePrefix.Length..^NameSuffix.Length];
                    if (attrName != MetadataName)
                        attributes.TryAdd(attrName, arguments[0].Value);
                    else if (arguments.Count > 1 && arguments[0].Value is string name)
                        attributes.TryAdd(name, arguments[1].Value);
                }
            }

            return attributes;
        }

        /// <summary>
        /// Normalizes a type name by replacing '+' with '.' to handle nested types.
        /// </summary>
        /// <param name="name">The type name to normalize.</param>
        /// <returns>The normalized type name.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static string NormalizeTypeName(string name) => name.Replace('+', '.');

        /// <inheritdoc/>
        protected override IEnumerable<CustomAttributeData> GetCustomAttributes() => Reflection.CustomAttributes;

        /// <inheritdoc/>
        protected override ICustomAttribute CreateAttributeMetadata(CustomAttributeData attribute)
            => Repository.GetCustomAttributeMetadata(attribute, AttributeTarget.Assembly);
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Models
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents an assembly in the documentation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="AssemblyModel"/> class represents the top-level organizational unit in the documentation model.
    /// Assemblies serve as containers for all types and namespaces in a .NET project, defining the deployment boundary.
    /// </para>
    /// This class provides properties and methods to:
    /// <list type="bullet">
    ///   <item><description>Retrieve assembly metadata such as title, description, and version</description></item>
    ///   <item><description>Access all types exported by the assembly</description></item>
    ///   <item><description>Organize types by their namespaces</description></item>
    ///   <item><description>Create appropriate documentation model for types based on their categories</description></item>
    ///   <item><description>Find specific members by their reflection metadata</description></item>
    /// </list>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class AssemblyModel : IEquatable<AssemblyModel>
    {
        private readonly SortedDictionary<string, TypeModel> exportedTypes;
        private readonly Lazy<TypeCollection> types;
        private readonly Lazy<SortedDictionary<string, NamespaceModel>> namespaces;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyModel"/> class.
        /// </summary>
        /// <param name="context">The documentation context.</param>
        /// <param name="assembly">The assembly represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> is <see langword="null"/>.</exception>
        public AssemblyModel(IDocumentationContext context, IAssembly assembly)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Metadata = assembly ?? throw new ArgumentNullException(nameof(assembly));

            exportedTypes = CollectExportedTypes();

            types = new(() => new(exportedTypes.Values));
            namespaces = new(CollectNamespaces);
        }

        /// <summary>
        /// Gets the documentation context.
        /// </summary>
        /// <value>
        /// The <see cref="IDocumentationContext"/> object representing the documentation context.
        /// </value>
        public IDocumentationContext Context { get; }

        /// <summary>
        /// Gets the metadata of the assembly.
        /// </summary>
        /// <value>
        /// The <see cref="IAssembly"/> object representing the documented assembly.
        /// </value>
        public IAssembly Metadata { get; }

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <value>
        /// The name of the assembly, derived from the "Title" attribute if available; otherwise, the assembly's simple name.
        /// </value>
        public string Name => Metadata.Attributes.GetValueOrDefault("Title") is string { Length: > 0 } title ? title : Metadata.Name;

        /// <summary>
        /// Gets the namespaces exported by the assembly.
        /// </summary>
        /// <value>
        /// A read-only dictionary of <see cref="NamespaceModel"/> objects representing the namespaces exported by the assembly.
        /// The namespaces in the dictionary are sorted and keyed by their names.
        /// </value>
        public IReadOnlyDictionary<string, NamespaceModel> Namespaces => namespaces.Value;

        /// <summary>
        /// Gets all the types exported by the assembly.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="TypeModel"/> objects representing all the types exported by the assembly.
        /// The types in the collection are ordered by their full names and categorized by their kinds.
        /// </value>
        public IReadOnlyTypeCollection ExportedTypes => types.Value;

        /// <summary>
        /// Finds a member in the assembly by its reflection metadata.
        /// </summary>
        /// <param name="member">The metadata of the member to find.</param>
        /// <returns>The documentation model representing the member, or <see langword="null"/> if the member is not found or does not belong to this assembly.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> is <see langword="null"/>.</exception>
        public MemberModel? FindMember(IMember member)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            if (!ReferenceEquals(Metadata, member.Assembly))
                return null;

            return member switch
            {
                IType type => exportedTypes.TryGetValue(type.FullName, out var typeElement) ? typeElement : null,
                ITypeMember typeMember => exportedTypes.TryGetValue(typeMember.DeclaringType.FullName, out var declaringType) ? declaringType.FindMember(typeMember) : null,
                _ => null,
            };
        }

        /// <summary>
        /// Determines whether the specified assembly is equal to the current assembly.
        /// </summary>
        /// <param name="other">The assembly to compare with the current assembly.</param>
        /// <returns><see langword="true"/> if the specified assembly is equal to the current assembly; otherwise, <see langword="false"/>.</returns>
        public bool Equals(AssemblyModel? other) => other is not null && Metadata.Equals(other.Metadata) && Context.Equals(other.Context);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj) => obj is AssemblyModel other && Equals(other);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => HashCode.Combine(Metadata, Context);

        /// <summary>
        /// Creates the documentation model elements for all exported types in the assembly.
        /// </summary>
        /// <returns>A sorted dictionary of <see cref="TypeModel"/> objects representing the exported types.</returns>
        private SortedDictionary<string, TypeModel> CollectExportedTypes()
        {
            var types = new SortedDictionary<string, TypeModel>(StringComparer.Ordinal);

            foreach (var type in Metadata.ExportedTypes)
                AddType(type);

            return types;

            TypeModel? AddType(IType type)
            {
                if (type.DeclaringType is null)
                {
                    var typeElement = CreateTopLevelType(type);
                    if (typeElement is null)
                        return null;

                    types[type.FullName] = typeElement;
                    return typeElement;
                }

                if (!types.TryGetValue(type.DeclaringType.FullName, out var declaringType))
                {
                    declaringType = AddType(type.DeclaringType);
                    if (declaringType is null)
                        return null;
                }

                var nestedTypeElement = CreateNestedType(declaringType, type);
                if (nestedTypeElement is null)
                    return null;

                types[type.FullName] = nestedTypeElement;
                return nestedTypeElement;
            }

            TypeModel? CreateTopLevelType(IType type) => type switch
            {
                IClassType classType => new ClassModel(this, classType),
                IStructType structType => new StructModel(this, structType),
                IInterfaceType interfaceType => new InterfaceModel(this, interfaceType),
                IEnumType enumType => new EnumModel(this, enumType),
                IDelegateType delegateType => new DelegateModel(this, delegateType),
                _ => null,
            };

            TypeModel? CreateNestedType(TypeModel declaringType, IType type) => type switch
            {
                IClassType classType => new ClassModel(declaringType, classType),
                IStructType structType => new StructModel(declaringType, structType),
                IInterfaceType interfaceType => new InterfaceModel(declaringType, interfaceType),
                IEnumType enumType => new EnumModel(declaringType, enumType),
                IDelegateType delegateType => new DelegateModel(declaringType, delegateType),
                _ => null
            };
        }

        /// <summary>
        /// Collects the namespaces exported by the assembly.
        /// </summary>
        /// <returns>A sorted dictionary of <see cref="NamespaceModel"/> objects representing the exported namespaces.</returns>
        private SortedDictionary<string, NamespaceModel> CollectNamespaces()
        {
            var namespaces = new SortedDictionary<string, NamespaceModel>(StringComparer.Ordinal);

            foreach (var nsTypes in exportedTypes.Values.GroupBy(static type => type.Metadata.Namespace))
                namespaces.Add(nsTypes.Key, new NamespaceModel(Context, nsTypes.Key, nsTypes));

            return namespaces;
        }
    }
}
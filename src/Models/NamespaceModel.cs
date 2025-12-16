// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Models
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.XmlDoc;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Represents a documentation model for a .NET namespace.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="NamespaceModel"/> class represents a logical container for related types in the documentation model.
    /// Namespaces serve as the primary organizational unit in .NET projects, grouping related functionality together.
    /// </para>
    /// This class provides methods to:
    /// <list type="bullet">
    ///   <item><description>Access all types defined within the namespace</description></item>
    ///   <item><description>Find specific members by their reflection metadata</description></item>
    ///   <item><description>Handle namespace merging across multiple assemblies</description></item>
    /// </list>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class NamespaceModel : IDocumentModel, IEquatable<NamespaceModel>
    {
        private readonly TypeCollection types = TypeCollection.Empty;
        private XmlDocEntry? doc;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceModel"/> class.
        /// </summary>
        /// <param name="context">The documentation context.</param>
        /// <param name="ns">The namespace represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ns"/> is <see langword="null"/>, empty, or contains only whitespace.</exception>
        public NamespaceModel(IDocumentationContext context, string ns)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrWhiteSpace(ns))
                throw new ArgumentException($"'{nameof(ns)}' cannot be null, empty, or whitespace.", nameof(ns));

            Context = context;
            Name = ns;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamespaceModel"/> class with the specified types.
        /// </summary>
        /// <param name="context">The documentation context.</param>
        /// <param name="ns">The namespace represented by this instance.</param>
        /// <param name="types">The types defined in the namespace. The types in different namespaces are ignored.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="types"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ns"/> is <see langword="null"/>, empty, or contains only whitespace.</exception>
        /// <remarks>
        /// The constructor does not validate whether the provided types actually belong to the specified namespace. It is the responsibility of the caller
        /// to ensure that the types are correctly associated with the namespace.
        /// </remarks>
        public NamespaceModel(IDocumentationContext context, string ns, IEnumerable<TypeModel> types)
            : this(context, ns)
        {
            if (types is null)
                throw new ArgumentNullException(nameof(types));

            this.types = new(types);
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is always <see cref="DocumentationModelType.Namespace"/> for this model.
        /// </value>
        public DocumentationModelType ModelType => DocumentationModelType.Namespace;

        /// <summary>
        /// Gets the documentation context associated with the namespace.
        /// </summary>
        /// <value>
        /// The <see cref="IDocumentationContext"/> object representing the documentation context associated with the namespace.
        /// </value>
        public IDocumentationContext Context { get; }

        /// <summary>
        /// Gets the name of the namespace.
        /// </summary>
        /// <value>
        /// The name of the namespace.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the types defined in the namespace.
        /// </summary>
        /// <value>
        /// The read-only collection of <see cref="TypeModel"/> objects representing the types defined in the namespace.
        /// The types in the collection are ordered by their full names and categorized by their kinds.
        /// </value>
        public IReadOnlyTypeCollection Types => types;

        /// <summary>
        /// Gets the documentation for the namespace.
        /// </summary>
        /// <value>
        /// The <see cref="XmlDocEntry"/> object representing the documentation of the namespace.
        /// </value>
        public XmlDocEntry Doc => doc ??= Context.ContentProvider.TryGetNamespaceDoc(Name, out var namespaceDoc) ? namespaceDoc.WithContext(Context) : XmlDocEntry.Empty;

        /// <summary>
        /// Get the URL for the namespace documentation.
        /// </summary>
        /// <value>
        /// The URL for the namespace documentation.
        /// </value>
        /// <inheritdoc/>
        public Uri Url => Context.AddressProvider.TryGetNamespaceUrl(Name, out var url) ? url : UriHelper.EmptyUri;

        /// <summary>
        /// Gets the hierarchy of parent models that lead to this namespace.
        /// </summary>
        /// <value>
        /// An enumerable collection of <see cref="IDocumentModel"/> objects representing the parent hierarchy.
        /// </value>
        public IEnumerable<IDocumentModel> HierarchyPath => [];

        /// <summary>
        /// Attempts to get the relative path of the documentation file for the namespace.
        /// </summary>
        /// <param name="relativePath">When this method returns, contains the relative path of the documentation file if applicable; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a documentation file is applicable; otherwise, <see langword="false"/>.</returns>
        public bool TryGetDocumentationFile([NotNullWhen(true)] out string? relativePath) => Context.AddressProvider.TryGetNamespaceFile(Name, out relativePath);

        /// <summary>
        /// Determines whether the specified namespace is equal to the current namespace.
        /// </summary>
        /// <param name="other">The namespace to compare with the current namespace.</param>
        /// <returns><see langword="true"/> if the specified namespace is equal to the current namespace; otherwise, <see langword="false"/>.</returns>
        public bool Equals(NamespaceModel? other) => other is not null && Name.Equals(other.Name, StringComparison.Ordinal) && Context.Equals(other.Context);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj) => obj is NamespaceModel other && Equals(other);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => HashCode.Combine(Name, Context);

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>The name of the namespace.</returns>
        public override string ToString() => Name;

        /// <summary>
        /// Merges duplicate namespace entries by combining their type information.
        /// </summary>
        /// <param name="namespaces">The collection of namespace information to examine.</param>
        /// <returns>A collection of namespace information where duplicate entries have been merged into single entries.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="namespaces"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method is implemented by using deferred execution. The immediate return value is an object that stores all the information
        /// that is required to perform the action.
        /// </remarks>
        public static IEnumerable<NamespaceModel> MergeDuplicates(IEnumerable<NamespaceModel> namespaces)
        {
            if (namespaces is null)
                throw new ArgumentNullException(nameof(namespaces));

            return namespaces.GroupBy(static ns => ns.Name, Combine, StringComparer.Ordinal);

            static NamespaceModel Combine(string name, IEnumerable<NamespaceModel> namespaces)
            {
                using var iterator = namespaces.GetEnumerator();

                iterator.MoveNext();
                var first = iterator.Current;

                if (!iterator.MoveNext())
                    return first;

                var types = first.types.AsEnumerable();
                do { types = types.Concat(iterator.Current.types); } while (iterator.MoveNext());
                return new NamespaceModel(first.Context, name, types.OrderBy(static t => t.Name, StringComparer.Ordinal));
            }
        }
    }
}

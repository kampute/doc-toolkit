// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Collections
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.XmlDoc;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a collection of members that are overloads of each other.
    /// </summary>
    /// <typeparam name="T">The type of members in the collection.</typeparam>
    /// <remarks>
    /// The <see cref="OverloadCollection{T}"/> class provides a way to group and organize members that represent different overloads
    /// of the same method, constructor, or other member. This collection ensures that all contained members share the same name and
    /// declaring type, providing a consistent view of related members.
    /// <para>
    /// The collection preserves the documentation context of the overloaded members and provides common properties such as name,
    /// declaring type, and documentation URL. It is particularly useful for documentation generators that need to represent
    /// multiple overloads of a member together while still providing access to each individual overload.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public class OverloadCollection<T> : IDocumentModel, IReadOnlyCollection<T>
        where T : TypeMemberModel
    {
        private readonly List<T> overloads = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="OverloadCollection{T}"/> class.
        /// </summary>
        /// <param name="members">The members that are overloads of each other.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="members"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="members"/> contains elements that are not overloads of each other.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="members"/> contains fewer than two elements.</exception>
        public OverloadCollection(IEnumerable<T> members)
        {
            if (members is null)
                throw new ArgumentNullException(nameof(members));

            if (members is IReadOnlyCollection<T> collection)
                overloads.Capacity = collection.Count;

            var representative = default(T);
            var doc = default(XmlDocEntry);

            foreach (var member in members)
            {
                if (member is null)
                    continue;

                if (representative is null)
                    representative = member;
                else if (member.Name != representative.Name || !ReferenceEquals(member.DeclaringType, representative.DeclaringType))
                    throw new ArgumentException("All members must be overloads of each other.", nameof(members));

                overloads.Add(member);

                if (doc is null && member.Doc.Overloads != XmlDocEntry.Empty)
                    doc = member.Doc.Overloads;
            }

            if (overloads.Count < 2)
                throw new ArgumentException("The overload collection must contain at least two elements.", nameof(members));

            Doc = doc ?? XmlDocEntry.Empty;
        }

        /// <summary>
        /// Gets the number of members in the collection.
        /// </summary>
        /// <value>
        /// The number of members in the collection.
        /// </value>
        public int Count => overloads.Count;

        /// <summary>
        /// Gets the member at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the member to get.</param>
        /// <returns>The member at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
        public T this[int index] => overloads[index];

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is the same for all overloaded members.
        /// </value>
        public DocumentationModelType ModelType => overloads[0].ModelType;

        /// <summary>
        /// Gets the documentation context.
        /// </summary>
        /// <value>
        /// The <see cref="IDocumentationContext"/> object representing the documentation context.
        /// </value>
        public IDocumentationContext Context => overloads[0].Context;

        /// <summary>
        /// Gets the assembly that contains the overloaded members.
        /// </summary>
        /// <value>
        /// The assembly that contains the overloaded members.
        /// </value>
        public AssemblyModel Assembly => overloads[0].Assembly;

        /// <summary>
        /// Gets the namespace of the overloaded members.
        /// </summary>
        /// <value>
        /// The namespace of the overloaded members.
        /// </value>
        public NamespaceModel Namespace => overloads[0].Namespace;

        /// <summary>
        /// Gets the declaring type of the overloaded members.
        /// </summary>
        /// <value>
        /// The declaring type of the overloaded members.
        /// </value>
        public TypeModel DeclaringType => overloads[0].DeclaringType!;

        /// <summary>
        /// Gets the name of the overloaded members.
        /// </summary>
        /// <value>
        /// The name of the overloaded members.
        /// </value>
        public string Name => overloads[0].Name;

        /// <summary>
        /// Gets the common documentation URL for the overloaded members.
        /// </summary>
        /// <value>
        /// The common documentation URL for the overloaded members.
        /// </value>
        /// <inheritdoc cref="IDocumentModel.Url"/>
        public Uri Url => overloads[0].Url.WithoutFragment();

        /// <summary>
        /// Gets the hierarchy path of the parent models leading to this model.
        /// </summary>
        /// <value>
        /// An enumerable of <see cref="IDocumentModel"/> objects representing the hierarchy path of this model.
        /// </value>
        public IEnumerable<IDocumentModel> HierarchyPath => overloads[0].HierarchyPath;

        /// <summary>
        /// Get the common relative path of the documentation file for the overloaded members.
        /// </summary>
        /// <param name="relativePath">The common relative path of the documentation file for the overloaded members, or <see langword="null"/> if not applicable.</param>
        /// <returns><see langword="true"/> if a common documentation file is applicable; otherwise, <see langword="false"/>.</returns>
        public bool TryGetDocumentationFile([NotNullWhen(true)] out string? relativePath) => overloads[0].TryGetDocumentationFile(out relativePath);

        /// <summary>
        /// Gets the common documentation for overloaded members.
        /// </summary>
        /// <value>
        /// The <see cref="XmlDocEntry"/> object representing the common documentation of the members.
        /// </value>
        public XmlDocEntry Doc { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>The full name of the overloaded members.</returns>
        public override string ToString() => overloads[0].ToString()!;

        /// <summary>
        /// Returns an enumerator that iterates through the collection of members.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection of members.</returns>
        public IEnumerator<T> GetEnumerator() => overloads.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the types collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

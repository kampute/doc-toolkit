// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Models
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.XmlDoc;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents the documentation model for a type or type member.
    /// </summary>
    /// <remarks>
    /// The <see cref="MemberModel"/> class provides a lightweight wrapper that associates reflection metadata with documentation
    /// context. It serves as a base class for all documentation model elements.
    /// <para>
    /// The class provides access to reflection metadata through the <see cref="Metadata"/> property, and to documentation-specific
    /// functionality such as documentation comments and URL.
    /// </para>
    /// </remarks>
    public abstract class MemberModel : IDocumentModel, IEquatable<MemberModel>
    {
        private string? name;
        private XmlDocEntry? doc;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberModel"/> class.
        /// </summary>
        /// <param name="member">The metadata of the member represented by this instance.</param>
        protected MemberModel(IMember member)
        {
            Metadata = member ?? throw new ArgumentNullException(nameof(member));
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model for this member.
        /// </value>
        public abstract DocumentationModelType ModelType { get; }

        /// <summary>
        /// Gets the documentation context associated with this member.
        /// </summary>
        /// <value>
        /// The <see cref="IDocumentationContext"/> object representing the documentation context associated with this member.
        /// </value>
        /// <inheritdoc/>
        public IDocumentationContext Context => Assembly.Context;

        /// <summary>
        /// Gets the reflection metadata for this member.
        /// </summary>
        /// <value>
        /// The reflection metadata object that provides detailed information about the member.
        /// </value>
        public IMember Metadata { get; }

        /// <summary>
        /// Gets the assembly that contains the member.
        /// </summary>
        /// <value>
        /// The <see cref="AssemblyModel"/> object representing the assembly that contains this member.
        /// </value>
        public abstract AssemblyModel Assembly { get; }

        /// <summary>
        /// Gets the namespace that contains the member.
        /// </summary>
        /// <value>
        /// The <see cref="NamespaceModel"/> object representing the namespace that contains this member.
        /// </value>
        public NamespaceModel Namespace => Assembly.Namespaces[Metadata.Namespace];

        /// <summary>
        /// Gets the declaring type of the member.
        /// </summary>
        /// <value>
        /// The <see cref="TypeModel"/> object representing the type that declares this member, or <see langword="null"/> if the member is not declared within a type.
        /// </value>
        public abstract TypeModel? DeclaringType { get; }

        /// <summary>
        /// Gets the formatted name of the member.
        /// </summary>
        /// <value>
        /// A string representing the name of the member according to the language specified in the documentation context.
        /// </value>
        /// <inheritdoc/>
        public string Name => name ??= Context.Language.FormatName(Metadata, MemberNameQualifier);

        /// <summary>
        /// Gets the documentation comments associated with the member.
        /// </summary>
        /// <value>
        /// A <see cref="XmlDocEntry"/> object containing the documentation comments for the member.
        /// </value>
        public XmlDocEntry Doc => doc ??= Context.ContentProvider.TryGetMemberDoc(Metadata, out var memberDoc) ? memberDoc.WithContext(Context) : XmlDocEntry.Empty;

        /// <summary>
        /// Gets the URL of the member's documentation page.
        /// </summary>
        /// <value>
        /// A <see cref="Uri"/> object representing the URL of the member's documentation page.
        /// </value>
        /// <inheritdoc/>
        public Uri Url => Context.AddressProvider.TryGetMemberUrl(Metadata, out var url) ? url : UriHelper.EmptyUri;

        /// <summary>
        /// Gets the hierarchy of parent document models that lead to this member.
        /// </summary>
        /// <value>
        /// An enumerable collection of <see cref="IDocumentModel"/> objects representing the parent hierarchy.
        /// </value>
        public abstract IEnumerable<IDocumentModel> HierarchyPath { get; }

        /// <summary>
        /// Gets the name qualifier to use for formatting the member's name.
        /// </summary>
        /// <value>
        /// The <see cref="NameQualifier"/> value representing the name qualifier to use for formatting the member's name.
        /// </value>
        protected virtual NameQualifier MemberNameQualifier { get; }

        /// <summary>
        /// Attempts to get the relative path of the documentation file for the member.
        /// </summary>
        /// <param name="relativePath">When this method returns, contains the relative path of the documentation file if applicable; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a documentation file is applicable; otherwise, <see langword="false"/>.</returns>
        public bool TryGetDocumentationFile([NotNullWhen(true)] out string? relativePath) => Context.AddressProvider.TryGetMemberFile(Metadata, out relativePath);

        /// <summary>
        /// Determines whether the specified member definition is equal to the current member definition.
        /// </summary>
        /// <param name="other">The member definition to compare with the current member definition.</param>
        /// <returns><see langword="true"/> if the specified member definition is equal to the current member definition; otherwise, <see langword="false"/>.</returns>
        public bool Equals(MemberModel? other) => other is not null && Metadata.Equals(other.Metadata) && Context.Equals(other.Context);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj) => obj is MemberModel other && Equals(other);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => HashCode.Combine(Metadata, Context);

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => Name;
    }

    /// <summary>
    /// Represents a documentation model for a member of a specific kind.
    /// </summary>
    /// <typeparam name="T">The type of the member, which must implement <see cref="IMember"/>.</typeparam>
    public abstract class MemberModel<T> : MemberModel
        where T : class, IMember
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberModel{T}"/> class.
        /// </summary>
        /// <param name="member">The metadata of the member represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> is <see langword="null"/>.</exception>
        protected MemberModel(T member)
            : base(member)
        {
        }

        /// <summary>
        /// Gets the reflection metadata for the member.
        /// </summary>
        /// <value>
        /// The reflection metadata object that provides detailed information about the member.
        /// </value>
        public new T Metadata => (T)base.Metadata;
    }
}

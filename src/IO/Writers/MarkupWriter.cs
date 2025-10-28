// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.IO.Writers
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides a text writer for writing markup language content with encoding of special characters and structured document elements.
    /// </summary>
    /// <remarks>
    /// The <see cref="MarkupWriter"/> class is an abstract class that defines a common API for generating documentation in various markup formats
    /// This abstraction is built on the principle of separation of concerns:
    /// <list type="bullet">
    ///   <item><description>The <em>semantic structure</em> of the content (headings, paragraphs, lists, tables)</description></item>
    ///   <item><description>The <em>character escaping</em> required by each markup language</description></item>
    ///   <item><description>The <em>structural syntax</em> for the target markup language</description></item>
    /// </list>
    /// The class also provides link handling through specialized documentation link methods, which manage cross-references between
    /// code elements. These methods create links at different levels of granularity, from direct element links to component-based linking.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public abstract class MarkupWriter : WrappedTextWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarkupWriter"/> class.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write encoded text to.</param>
        /// <param name="leaveOpen">A value indicating whether the underlying writer should be left open when the <see cref="MarkupWriter"/> is disposed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <see langword="null"/>.</exception>
        protected MarkupWriter(TextWriter writer, bool leaveOpen)
            : base(writer, leaveOpen)
        {
        }

        /// <summary>
        /// Writes a character to the text stream without encoding special characters.
        /// </summary>
        /// <param name="value">The character to write.</param>
        public virtual void WriteSafe(char value) => UnderlyingWriter.Write(value);

        /// <summary>
        /// Writes a string to the text stream without encoding special characters.
        /// </summary>
        /// <param name="value">The string to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSafe(ReadOnlySpan<char> value)
        {
            foreach (var c in value)
                WriteSafe(c);
        }

        /// <summary>
        /// Writes a strong emphasis in the format of the target document using an optional delegate to write the content.
        /// </summary>
        /// <param name="contentHandler">The action to write the content of the strong emphasis.</param>
        public abstract void WriteStrong(Action<MarkupWriter> contentHandler);

        /// <summary>
        /// Writes a strong emphasis in the format of the target document using the specified text.
        /// </summary>
        /// <param name="text">The text of the strong emphasis.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteStrong(string text) => WriteStrong(w => w.Write(text));

        /// <summary>
        /// Writes an emphasis in the format of the target document using an optional delegate to write the content.
        /// </summary>
        /// <param name="contentHandler">The action to write the content of the emphasis.</param>
        public abstract void WriteEmphasis(Action<MarkupWriter> contentHandler);

        /// <summary>
        /// Writes an emphasis in the format of the target document using the specified text.
        /// </summary>
        /// <param name="text">The text of the emphasis.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteEmphasis(string text) => WriteEmphasis(w => w.Write(text ?? string.Empty));

        /// <summary>
        /// Writes a hyperlink in the format of the target document using an optional delegate to write the link's text.
        /// </summary>
        /// <param name="url">The <see cref="Uri"/> representing the link's destination.</param>
        /// <param name="linkTextHandler">The action to write the text of the link, or <see langword="null"/> to use the URL as the text.</param>
        public abstract void WriteLink(Uri url, Action<MarkupWriter>? linkTextHandler = null);

        /// <summary>
        /// Writes a hyperlink with the specified text in the format of the target document.
        /// </summary>
        /// <param name="url">The <see cref="Uri"/> representing the link's destination.</param>
        /// <param name="text">The text of the link.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="url"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLink(Uri url, string text) => WriteLink(url, string.IsNullOrEmpty(text) ? null : w => w.Write(text));

        /// <summary>
        /// Writes an image as an embedded link in the format of the target document using the specified URL and an optional title.
        /// </summary>
        /// <param name="imageUrl">The <see cref="Uri"/> representing the image's URL.</param>
        /// <param name="title">The title of the image, or <see langword="null"/> to omit the title.</param>
        public abstract void WriteImage(Uri imageUrl, string? title = null);

        /// <summary>
        /// Writes a code span in the format of the target document using the specified text.
        /// </summary>
        /// <param name="text">The text of the code span.</param>
        public abstract void WriteInlineCode(string text);

        /// <summary>
        /// Writes a code block in the format of the target document.
        /// </summary>
        /// <param name="code">The code to write.</param>
        /// <param name="language">The language of the code block, or <see langword="null"/> to omit the language specifier.</param>
        public abstract void WriteCodeBlock(string code, string? language = null);

        /// <summary>
        /// Writes a heading in the format of the target document using an optional delegate to write the heading content.
        /// </summary>
        /// <param name="level">The heading level (1-6).</param>
        /// <param name="contentHandler">The action to write the content of the heading.</param>
        public abstract void WriteHeading(int level, Action<MarkupWriter> contentHandler);

        /// <summary>
        /// Writes a heading in the format of the target document using the specified text.
        /// </summary>
        /// <param name="level">The heading level (1-6).</param>
        /// <param name="text">The text of the heading.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteHeading(int level, string text) => WriteHeading(level, w => w.Write(text ?? string.Empty));

        /// <summary>
        /// Writes a paragraph in the format of the target document using an optional delegate to write the paragraph content.
        /// </summary>
        /// <param name="contentHandler">The action to write the content of the paragraph.</param>
        public abstract void WriteParagraph(Action<MarkupWriter> contentHandler);

        /// <summary>
        /// Writes a paragraph in the format of the target document using the specified text.
        /// </summary>
        /// <param name="text">The text of the paragraph.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteParagraph(string text) => WriteParagraph(w => w.Write(text ?? string.Empty));

        /// <summary>
        /// Writes a blockquote in the format of the target document using an optional delegate to write the content.
        /// </summary>
        /// <param name="contentHandler">The action to write the content of the blockquote.</param>
        public abstract void WriteBlockquote(Action<MarkupWriter> contentHandler);

        /// <summary>
        /// Writes a blockquote in the format of the target document using the specified text.
        /// </summary>
        /// <param name="text">The text of the blockquote.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBlockquote(string text) => WriteBlockquote(w => w.Write(text ?? string.Empty));

        /// <summary>
        /// Writes a list in the format of the target document using an optional delegate to write the list items.
        /// </summary>
        /// <param name="count">The number of items in the list.</param>
        /// <param name="itemContentHandler">The action to write the content of each list item.</param>
        /// <param name="isOrdered">Indicates whether the list is ordered (numbered) or unordered (bulleted).</param>
        public abstract void WriteList(int count, Action<MarkupWriter, int> itemContentHandler, bool isOrdered = false);

        /// <summary>
        /// Writes a list in the format of the target document using the specified items.
        /// </summary>
        /// <param name="items">The items to include in the list.</param>
        /// <param name="isOrdered">Indicates whether the list is ordered (numbered) or unordered (bulleted).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteList(IReadOnlyList<object> items, bool isOrdered = false)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            WriteList(items.Count, (w, idx) => w.Write(items[idx]?.ToString() ?? string.Empty), isOrdered);
        }

        /// <summary>
        /// Writes a table in the format of the target document using an optional delegate to write the content of each cell.
        /// </summary>
        /// <param name="columns">The column headers of the table.</param>
        /// <param name="rowCount">The number of rows in the table.</param>
        /// <param name="cellContentHandler">The action to write the content of each cell.</param>
        public abstract void WriteTable(IReadOnlyList<string> columns, int rowCount, Action<MarkupWriter, int, int> cellContentHandler);

        /// <summary>
        /// Writes a table in the format of the target document using the specified header and row columns.
        /// </summary>
        /// <param name="columns">The column headers of the table.</param>
        /// <param name="rows">The rows of the table.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="columns"/> or <paramref name="rows"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteTable(IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyList<object>> rows)
        {
            if (columns is null)
                throw new ArgumentNullException(nameof(columns));
            if (rows is null)
                throw new ArgumentNullException(nameof(rows));

            WriteTable(columns, rows.Count, (w, row, col) => w.Write(rows[row][col]?.ToString() ?? string.Empty));
        }

        /// <summary>
        /// Writes a horizontal rule in the format of the target document.
        /// </summary>
        public abstract void WriteHorizontalRule();

        /// <summary>
        /// Writes a hyperlink to the documentation of a code element with the specified URL and delegate for writing the link text.
        /// </summary>
        /// <param name="docUrl">The URI of the documentation page to link to.</param>
        /// <param name="docLinkTextHandler">The action to write the text of the link, typically the name of the code element.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="docUrl"/> or <paramref name="docLinkTextHandler"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method writes a hyperlink to the documentation of a code element with the specified text. The text is typically the
        /// name of the code element, and the hyperlink points to the documentation page for the code element.
        /// <para>
        /// The default implementation of this method calls <see cref="WriteLink(Uri, Action{MarkupWriter}?)"/> to write the hyperlink.
        /// Derived classes can override this method to provide custom behavior for writing links to documentation.
        /// </para>
        /// </remarks>
        protected virtual void WriteDocLink(Uri docUrl, Action<MarkupWriter> docLinkTextHandler)
        {
            if (docUrl is null)
                throw new ArgumentNullException(nameof(docUrl));
            if (docLinkTextHandler is null)
                throw new ArgumentNullException(nameof(docLinkTextHandler));

            WriteLink(docUrl, docLinkTextHandler);
        }

        /// <summary>
        /// Writes a hyperlink to the documentation of the specified member model.
        /// </summary>
        /// <param name="member">The member for which the link is written.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method writes a hyperlink to the documentation for the specified member. If the member has a valid documentation
        /// URL, a hyperlink is written using the member's signature formatted according to the rules of the programming language
        /// specified in the member's context. If no URL exists, the member's signature is written as plain text.
        /// <para>
        /// The qualification level for the member's name is determined based on the member's type and whether it is part of the
        /// current documentation scope.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void WriteDocLink(MemberModel member)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            WriteDocLink(member.Metadata, member.Context);
        }

        /// <summary>
        /// Writes a hyperlink to the documentation of the specified namespace model.
        /// </summary>
        /// <param name="ns">The namespace for which the link is written.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ns"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method writes a hyperlink to the documentation for the specified namespace. If the namespace has has a valid documentation
        /// URL, a hyperlink is written using the namespace's name. If the namespace does not have documentation URL, the namespace's name
        /// is written as plain text.
        /// </remarks>
        public virtual void WriteDocLink(NamespaceModel ns)
        {
            if (ns is null)
                throw new ArgumentNullException(nameof(ns));

            var url = ns.Url;
            if (url != UriHelper.EmptyUri)
                WriteDocLink(url, w => w.Write(ns.Name));
            else
                Write(ns.Name);
        }

        /// <summary>
        /// Writes a hyperlink to the documentation of the specified member.
        /// </summary>
        /// <param name="member">The member for which the link is written.</param>
        /// <param name="context">The documentation context used to resolve the link and format the member's signature.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method writes a hyperlink to the documentation for the specified member. It uses the provided <paramref name="context"/> to resolve the
        /// member's documentation URL and format its signature. The signature is formatted according to the rules of the programming language specified
        /// in the <paramref name="context"/>. Depending on type of the member and whether it is part of the current documentation scope, the method
        /// determines an appropriate level of name qualification to use when formatting the member's signature.
        /// <para>
        /// When a valid documentation URL is available for the specified member, a hyperlink is written using the member’s signature. For example, the hyperlink
        /// for <c>ICollection&lt;T&gt;.Count</c> will display as "ICollection&lt;T&gt;.Count" with a hyperlink to the documentation of the <c>Count</c> property
        /// in the <c>ICollection&lt;T&gt;</c> interface.
        /// </para>
        /// If no direct URL exists for a member, this method attempts to generate links for individual components of the member when possible. For example, the
        /// writing of a hyperlink for <c>ICollection&lt;DateTime&gt;.Count</c> will result in separate links for "ICollection" linked to <c>ICollection&lt;T&gt;</c>,
        /// "DateTime" linked to <c>DateTime</c>, and "Count" linked to <c>ICollection&lt;T&gt;.Count</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteDocLink(IMember member, IDocumentationContext context)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            WriteDocLink(member, context, context.DetermineNameQualifier(member));
        }

        /// <summary>
        /// Writes a hyperlink to the documentation of the specified member with a specific name qualification level.
        /// </summary>
        /// <param name="member">The member for which the link is written.</param>
        /// <param name="context">The documentation context used to resolve the link and format the member's signature.</param>
        /// <param name="qualifier">The level of qualification to use for the member's name.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method writes a hyperlink to the documentation for the specified member. It uses the provided <paramref name="context"/> to resolve the
        /// member's documentation URL and format its signature. The signature is formatted according to the rules of the programming language specified
        /// in the <paramref name="context"/>. For the member's name, the method uses the level of qualification specified by <paramref name="qualifier"/>.
        /// <para>
        /// When a valid documentation URL is available for the specified member, a hyperlink is written using the member’s signature. For example, the hyperlink
        /// for <c>ICollection&lt;T&gt;.Count</c> will display as "ICollection&lt;T&gt;.Count" with a hyperlink to the documentation of the <c>Count</c> property
        /// in the <c>ICollection&lt;T&gt;</c> interface.
        /// </para>
        /// If no direct URL exists for a member, this method attempts to generate links for individual components of the member when possible. For example, the
        /// writing of a hyperlink for <c>ICollection&lt;DateTime&gt;.Count</c> will result in separate links for "ICollection" linked to <c>ICollection&lt;T&gt;</c>,
        /// "DateTime" linked to <c>DateTime</c>, and "Count" linked to <c>ICollection&lt;T&gt;.Count</c>.
        /// </remarks>
        public virtual void WriteDocLink(IMember member, IDocumentationContext context, NameQualifier qualifier)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (context.AddressProvider.TryGetMemberUrl(member, out var url))
                WriteDocLink(url, w => context.Language.WriteSignature(w, member, qualifier));
            else
                context.Language.WriteSignature(this, member, qualifier, CreateDocLinker(context.AddressProvider));
        }

        /// <summary>
        /// Writes a hyperlink to the documentation of the specified code reference.
        /// </summary>
        /// <param name="cref">The code reference for which the link is written.</param>
        /// <param name="context">The documentation context used to resolve the link and format the code reference.</param>
        /// <param name="qualifierSelector">An optional function to determine the level of qualification to use for the member's name if the code reference resolves to a member.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cref"/> or <paramref name="context"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method writes a hyperlink to the documentation for the specified code reference. It uses the provided <paramref name="context"/> to resolve
        /// the documentation URL of the reference and format its signature. The signature is formatted according to the rules of the programming language
        /// specified in the <paramref name="context"/>.
        /// <para>
        /// If the code reference resolves to a member, the method uses the <paramref name="qualifierSelector"/> function to determine the appropriate level
        /// of name qualification to use when formatting the member's signature. If no function is provided, the method uses the most suitable qualification
        /// level based on the current documentation scope.
        /// </para>
        /// If the code reference cannot be resolved to a member, but represents a namespace, the method attempts to retrieve the documentation URL for the
        /// namespace and writes a link using the namespace name as the link text.
        /// <para>
        /// If the code reference cannot be resolved to either a member or a namespace, the method writes the code reference as plain text without a hyperlink.
        /// </para>
        /// </remarks>
        public virtual void WriteDocLink(string cref, IDocumentationContext context, Func<IMember, NameQualifier>? qualifierSelector = null)
        {
            if (cref is null)
                throw new ArgumentNullException(nameof(cref));
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (CodeReference.IsNamespace(cref))
            {
                var ns = cref[2..];
                if (context.AddressProvider.TryGetNamespaceUrl(ns, out var url))
                    WriteDocLink(url, w => w.Write(ns));
                else
                    Write(ns);
            }
            else if (CodeReference.ResolveMember(cref) is IMember member)
                WriteDocLink(member, context, (qualifierSelector ?? context.DetermineNameQualifier)(member));
            else
                Write(cref);
        }

        /// <summary>
        /// Writes a hyperlink to the documentation of the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute for which the link is written.</param>
        /// <param name="context">The documentation context used to resolve the link and format the attribute and its parameters.</param>
        /// <param name="qualifier">The level of qualification to use for the attribute's name.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="attribute"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method writes the specified attribute as a hyperlink to the documentation of the attribute's type. It uses the provided <paramref name="context"/>
        /// to resolve the documentation URL of the attribute and format it. The output includes the attribute's type and any parameters and properties it may have.
        /// </remarks>
        public virtual void WriteDocLink(ICustomAttribute attribute, IDocumentationContext context, NameQualifier qualifier)
        {
            if (attribute is null)
                throw new ArgumentNullException(nameof(attribute));
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            context.Language.WriteAttribute(this, attribute, qualifier, CreateDocLinker(context.AddressProvider));
        }

        /// <summary>
        /// Creates a delegate for linking to the documentation of a type or type's member.
        /// </summary>
        /// <param name="urlResolver">The URL resolver used to retrieve documentation URLs for members.</param>
        /// <returns>A <see cref="MemberDocLinker"/> delegate for writing links to documentation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="urlResolver"/> is <see langword="null"/>.</exception>
        public MemberDocLinker CreateDocLinker(IApiDocUrlProvider urlResolver)
        {
            if (urlResolver is null)
                throw new ArgumentNullException(nameof(urlResolver));

            return (_, member, memberName) =>
            {
                if (urlResolver.TryGetMemberUrl(member, out var memberUrl))
                    WriteDocLink(memberUrl, w => w.Write(memberName));
                else
                    Write(memberName);
            };
        }
    }
}

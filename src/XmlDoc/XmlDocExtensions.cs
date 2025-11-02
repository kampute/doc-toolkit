// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides extension methods for various XML documentation related types to simplify common tasks.
    /// </summary>
    public static class XmlDocExtensions
    {
        /// <summary>
        /// Determines whether any inspection checks are enabled.
        /// </summary>
        /// <param name="options">The inspection options to check.</param>
        /// <returns><see langword="true"/> if any inspection checks are enabled; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyChecks(this XmlDocInspectionOptions options) => (options & XmlDocInspectionOptions.All) != XmlDocInspectionOptions.None;

        /// <summary>
        /// Wraps the specified <see cref="IXmlDocProvider"/> instance in a caching layer if it is not already cached.
        /// </summary>
        /// <param name="xmlDocProvider">The XML documentation provider to wrap.</param>
        /// <returns>A cached version of the XML documentation provider.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="xmlDocProvider"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IXmlDocProvider WithCaching(this IXmlDocProvider xmlDocProvider)
        {
            if (xmlDocProvider is null)
                throw new ArgumentNullException(nameof(xmlDocProvider));

            return xmlDocProvider is XmlDocProviderCache ? xmlDocProvider : new XmlDocProviderCache(xmlDocProvider);
        }

        /// <summary>
        /// Attempts to retrieves the XML documentation for the specified namespace.
        /// </summary>
        /// <param name="xmlDocProvider">The XML documentation provider to use.</param>
        /// <param name="ns">The namespace to retrieve the documentation for.</param>
        /// <param name="doc">
        /// When this method returns, contains the <see cref="XmlDocEntry"/> representing the documentation for the namespace,
        /// if the documentation is available; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if the documentation is available; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="xmlDocProvider"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ns"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
        /// <remarks>
        /// The XML documentation for a namespace is represented by a special type named "NamespaceDoc" within the namespace.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetNamespaceDoc(this IXmlDocProvider xmlDocProvider, string ns, [NotNullWhen(true)] out XmlDocEntry? doc)
        {
            if (xmlDocProvider is null)
                throw new ArgumentNullException(nameof(xmlDocProvider));
            if (string.IsNullOrWhiteSpace(ns))
                throw new ArgumentException("Namespace cannot be null or whitespace.", nameof(ns));

            return xmlDocProvider.TryGetDoc($"T:{ns}.{nameof(NamespaceDoc)}", out doc);
        }

        /// <summary>
        /// Attempts to retrieves the XML documentation for the specified member.
        /// </summary>
        /// <param name="xmlDocProvider">The XML documentation provider to use.</param>
        /// <param name="member">The member to retrieve the documentation for.</param>
        /// <param name="doc">
        /// When this method returns, contains the <see cref="XmlDocEntry"/> representing the documentation for the member,
        /// if the documentation is available; otherwise, <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if the documentation is available; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="xmlDocProvider"/> or <paramref name="member"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetMemberDoc(this IXmlDocProvider xmlDocProvider, IMember member, [NotNullWhen(true)] out XmlDocEntry? doc)
        {
            if (xmlDocProvider is null)
                throw new ArgumentNullException(nameof(xmlDocProvider));
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            if (member.IsDirectDeclaration)
                return xmlDocProvider.TryGetDoc(member.CodeReference, out doc);

            doc = null;
            return false;
        }

        /// <summary>
        /// Inspects the XML documentation for the specified member.
        /// </summary>
        /// <param name="xmlDocProvider">The XML documentation provider to use.</param>
        /// <param name="member">The member to inspect.</param>
        /// <param name="options">The options to determine which tags to inspect.</param>
        /// <returns>An enumerable of <see cref="XmlDocInspectionIssue"/> instances representing the issues found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="xmlDocProvider"/> or <paramref name="member"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<XmlDocInspectionIssue> InspectDocumentation(this IXmlDocProvider xmlDocProvider, IMember member, XmlDocInspectionOptions options = XmlDocInspectionOptions.Required)
        {
            if (xmlDocProvider is null)
                throw new ArgumentNullException(nameof(xmlDocProvider));
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            return options.HasAnyChecks() ? GetMemberDoc(member).Inspect(member, options, GetMemberDoc) : [];

            XmlDocEntry GetMemberDoc(IMember member) => xmlDocProvider.TryGetMemberDoc(member, out var memberDoc) ? memberDoc : XmlDocEntry.Empty;
        }

        /// <summary>
        /// Inspects the XML documentation entry for missing or incomplete elements based on the provided member and inspection options.
        /// </summary>
        /// <param name="doc">The XML documentation entry to inspect.</param>
        /// <param name="member">The member whose documentation is being inspected.</param>
        /// <param name="options">The options to determine which tags to inspect.</param>
        /// <param name="overloadDocProvider">An optional function to provide documentation for overload members when checking overload documentation.</param>
        /// <returns>An enumerable of <see cref="XmlDocInspectionIssue"/> instances representing the issues found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="doc"/> or <paramref name="member"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>
        /// This method inspects the XML documentation entry for completeness based on the member's characteristics and the specified inspection options.
        /// </para>
        /// The following elements are inspected based on the <paramref name="options"/> flags:
        /// <list type="table">
        ///   <item>
        ///     <term><see cref="XmlDocInspectionOptions.Summary"/></term>
        ///     <description>Checks for the presence of a non-empty <c>&lt;summary&gt;</c> tag.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="XmlDocInspectionOptions.Remarks"/></term>
        ///     <description>Checks for the presence of the <c>&lt;remarks&gt;</c> tag with non-empty content.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="XmlDocInspectionOptions.Example"/></term>
        ///     <description>Checks for the presence of the <c>&lt;example&gt;</c> tag with non-empty content.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="XmlDocInspectionOptions.TypeParam"/></term>
        ///     <description>Checks that each type parameter of generic members has a corresponding <c>&lt;typeparam&gt;</c> tag with non-empty description.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="XmlDocInspectionOptions.Param"/></term>
        ///     <description>Checks that each parameter for members with parameters has a corresponding <c>&lt;param&gt;</c> tag with non-empty description.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="XmlDocInspectionOptions.Returns"/></term>
        ///     <description>Checks for the presence of the <c>&lt;returns&gt;</c> tag with non-empty description for members with return values.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="XmlDocInspectionOptions.Value"/></term>
        ///     <description>For properties, checks for the presence of the <c>&lt;value&gt;</c> tag with non-empty content.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="XmlDocInspectionOptions.Exception"/></term>
        ///     <description>Validates that all <c>&lt;exception&gt;</c> tags have non-empty descriptions.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="XmlDocInspectionOptions.Permission"/></term>
        ///     <description>Validates that all <c>&lt;permission&gt;</c> tags have non-empty descriptions.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="XmlDocInspectionOptions.Event"/></term>
        ///     <description>Validates that all <c>&lt;event&gt;</c> tags have non-empty descriptions.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="XmlDocInspectionOptions.ThreadSafety"/></term>
        ///     <description>
        ///     For composite types (classes, structs), checks for the presence of the <c>&lt;threadsafety&gt;</c> tag with either
        ///     non-empty content or having values for at least one of <c>instance</c> and <c>static</c> attributes.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="XmlDocInspectionOptions.SeeAlso"/></term>
        ///     <description>Checks that all <c>&lt;seealso&gt;</c> tags referencing hyperlinks have descriptive text.</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="XmlDocInspectionOptions.Overloads"/></term>
        ///     <description>
        ///     For members with overloads, checks for the presence of the <c>&lt;overloads&gt;</c> tag. If the tag is missing or empty, uses
        ///     the provided <paramref name="overloadDocProvider"/> function to check documentation of each overload. If the <c>&lt;overloads&gt;</c>
        ///     tag is missing or empty in all overloads, an issue is reported. If <paramref name="overloadDocProvider"/> is <see langword="null"/>,
        ///     only the presence of the <c>&lt;overloads&gt;</c> tag in the current member is checked.
        ///     </description>
        ///   </item>
        /// </list>
        /// <note type="note">
        /// Enum value fields are only checked for the <c>&lt;summary&gt;</c> tag if the <see cref="XmlDocInspectionOptions.Summary"/> option is specified;
        /// other tags are not applicable and thus not inspected.
        /// </note>
        /// </remarks>
        public static IEnumerable<XmlDocInspectionIssue> Inspect
        (
            this XmlDocEntry doc,
            IMember member,
            XmlDocInspectionOptions options = XmlDocInspectionOptions.Required,
            Func<IMember, XmlDocEntry?>? overloadDocProvider = null
        )
        {
            if (doc is null)
                throw new ArgumentNullException(nameof(doc));
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            return options.HasAnyChecks() ? EnumerateInspectionIssues() : [];

            IEnumerable<XmlDocInspectionIssue> EnumerateInspectionIssues()
            {
                if (options.HasFlag(XmlDocInspectionOptions.Summary) && doc.Summary.IsEmpty)
                {
                    if (options.HasFlag(XmlDocInspectionOptions.OmitImplicitlyCreatedConstructors) && member is IConstructor { IsDefaultConstructor: true, HasOverloads: false })
                        yield break;

                    yield return XmlDocInspectionIssue.MissingRequiredTag(member, XmlDocTag.Summary);
                }

                if (member is IField { IsEnumValue: true })
                    yield break;

                if (options.HasFlag(XmlDocInspectionOptions.TypeParam) && member.TryGetOwnTypeParameters(out var typeParameters))
                {
                    foreach (var typeParameter in typeParameters)
                    {
                        if (doc.TypeParameters.TryGetValue(typeParameter.Name, out var typeParamDoc) && !typeParamDoc.IsEmpty)
                            continue;

                        yield return XmlDocInspectionIssue.UndocumentedTypeParameter(typeParameter);
                    }
                }

                if (options.HasFlag(XmlDocInspectionOptions.Param) && member is IWithParameters { HasParameters: true } memberWithParameters)
                {
                    foreach (var parameter in memberWithParameters.Parameters)
                    {
                        if (doc.Parameters.TryGetValue(parameter.Name, out var paramDoc) && !paramDoc.IsEmpty)
                            continue;

                        yield return XmlDocInspectionIssue.UndocumentedParameter(parameter);
                    }
                }

                if (options.HasFlag(XmlDocInspectionOptions.Returns) && member is IWithReturnParameter { HasReturnValue: true } memberWithReturnValue && doc.ReturnDescription.IsEmpty)
                    yield return XmlDocInspectionIssue.UndocumentedReturnParameter(memberWithReturnValue.Return);

                if (options.HasFlag(XmlDocInspectionOptions.Example) && doc.Example.IsEmpty)
                    yield return XmlDocInspectionIssue.MissingOptionalTag(member, XmlDocTag.Example);

                if (options.HasFlag(XmlDocInspectionOptions.Remarks) && doc.Remarks.IsEmpty)
                    yield return XmlDocInspectionIssue.MissingOptionalTag(member, XmlDocTag.Remarks);

                if (options.HasFlag(XmlDocInspectionOptions.ThreadSafety) && member is ICompositeType && doc.ThreadSafety.IsEmpty)
                    yield return XmlDocInspectionIssue.MissingOptionalTag(member, XmlDocTag.ThreadSafety);

                if (options.HasFlag(XmlDocInspectionOptions.Value) && member is IProperty && doc.ValueDescription.IsEmpty)
                    yield return XmlDocInspectionIssue.MissingOptionalTag(member, XmlDocTag.Value);

                if (options.HasFlag(XmlDocInspectionOptions.Exception))
                {
                    foreach (var descriptor in doc.Exceptions)
                    {
                        if (descriptor.IsEmpty)
                            yield return XmlDocInspectionIssue.UndocumentedReference(member, XmlDocTag.Exception, descriptor.Reference);
                    }
                }

                if (options.HasFlag(XmlDocInspectionOptions.Permission))
                {
                    foreach (var descriptor in doc.Permissions)
                    {
                        if (descriptor.IsEmpty)
                            yield return XmlDocInspectionIssue.UndocumentedReference(member, XmlDocTag.Permission, descriptor.Reference);
                    }
                }

                if (options.HasFlag(XmlDocInspectionOptions.Event))
                {
                    foreach (var descriptor in doc.Events)
                    {
                        if (descriptor.IsEmpty)
                            yield return XmlDocInspectionIssue.UndocumentedReference(member, XmlDocTag.Event, descriptor.Reference);
                    }
                }

                if (options.HasFlag(XmlDocInspectionOptions.SeeAlso))
                {
                    foreach (var seeAlso in doc.SeeAlso)
                    {
                        if (seeAlso.IsEmpty && seeAlso.IsHyperlink)
                            yield return XmlDocInspectionIssue.UntitledSeeAlso(member, seeAlso.Target);
                    }
                }

                if (options.HasFlag(XmlDocInspectionOptions.Overloads) && member is IWithOverloads { HasOverloads: true } memberWithOverloads && doc.Overloads.IsEmpty)
                {
                    if (overloadDocProvider is not null)
                    {
                        foreach (var overloadMember in memberWithOverloads.Overloads)
                        {
                            var overloadDoc = overloadDocProvider(overloadMember);
                            if (overloadDoc is not null && !overloadDoc.Overloads.IsEmpty)
                                yield break;
                        }
                    }

                    yield return XmlDocInspectionIssue.MissingOptionalTag(member, XmlDocTag.Overloads);
                }
            }
        }
    }
}

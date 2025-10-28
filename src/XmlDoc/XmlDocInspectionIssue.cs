// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using Kampute.DocToolkit.Metadata;
    using System;

    /// <summary>
    /// Represents an issue found during the inspection of XML documentation comments.
    /// </summary>
    public readonly struct XmlDocInspectionIssue
    {
        // Private constructor to enforce use of static factory methods
        private XmlDocInspectionIssue
        (
            XmlDocInspectionIssueType issueType,
            XmlDocTag xmlTag,
            IMember member,
            ITypeParameter? typeParameter = null,
            IParameter? parameter = null,
            string? codeReference = null,
            string? hyperlink = null
        )
        {
            IssueType = issueType;
            XmlTag = xmlTag;
            Member = member;
            TypeParameter = typeParameter;
            Parameter = parameter;
            CodeReference = codeReference;
            Hyperlink = hyperlink;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocInspectionIssue"/> for a missing required tag.
        /// </summary>
        /// <param name="member">The member that is missing a required XML documentation tag.</param>
        /// <param name="xmlTag">The XML documentation tag that is missing.</param>
        /// <returns>A new <see cref="XmlDocInspectionIssue"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> is <see langword="null"/>.</exception>
        public static XmlDocInspectionIssue MissingRequiredTag(IMember member, XmlDocTag xmlTag)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            return new(XmlDocInspectionIssueType.MissingRequiredTag, xmlTag, member);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocInspectionIssue"/> for a missing optional tag.
        /// </summary>
        /// <param name="member">The member that is missing an optional XML documentation tag.</param>
        /// <param name="xmlTag">The XML documentation tag that is missing.</param>
        /// <returns>A new <see cref="XmlDocInspectionIssue"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> is <see langword="null"/>.</exception>
        public static XmlDocInspectionIssue MissingOptionalTag(IMember member, XmlDocTag xmlTag)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            return new(XmlDocInspectionIssueType.MissingOptionalTag, xmlTag, member);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocInspectionIssue"/> for an undocumented type parameter.
        /// </summary>
        /// <param name="typeParameter">The undocumented type parameter.</param>
        /// <returns>A new <see cref="XmlDocInspectionIssue"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeParameter"/> is <see langword="null"/>.</exception>
        public static XmlDocInspectionIssue UndocumentedTypeParameter(ITypeParameter typeParameter)
        {
            if (typeParameter is null)
                throw new ArgumentNullException(nameof(typeParameter));

            return new
            (
                issueType: XmlDocInspectionIssueType.MissingRequiredTag,
                xmlTag: XmlDocTag.TypeParam,
                member: typeParameter.DeclaringMember,
                typeParameter: typeParameter
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocInspectionIssue"/> for an undocumented parameter.
        /// </summary>
        /// <param name="parameter">The undocumented parameter.</param>
        /// <returns>A new <see cref="XmlDocInspectionIssue"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameter"/> is <see langword="null"/>.</exception>
        public static XmlDocInspectionIssue UndocumentedParameter(IParameter parameter)
        {
            if (parameter is null)
                throw new ArgumentNullException(nameof(parameter));

            return new
            (
                issueType: XmlDocInspectionIssueType.MissingRequiredTag,
                xmlTag: XmlDocTag.Param,
                member: parameter.Member,
                parameter: parameter
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocInspectionIssue"/> for an undocumented return parameter.
        /// </summary>
        /// <param name="returnParameter">The undocumented return parameter.</param>
        /// <returns>A new <see cref="XmlDocInspectionIssue"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="returnParameter"/> is <see langword="null"/>.</exception>
        public static XmlDocInspectionIssue UndocumentedReturnParameter(IParameter returnParameter)
        {
            if (returnParameter is null)
                throw new ArgumentNullException(nameof(returnParameter));

            return new
            (
                issueType: XmlDocInspectionIssueType.MissingRequiredTag,
                xmlTag: XmlDocTag.Returns,
                member: returnParameter.Member,
                parameter: returnParameter
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocInspectionIssue"/> for an undocumented reference.
        /// </summary>
        /// <param name="member">The member with the undocumented reference.</param>
        /// <param name="xmlTag">The XML documentation tag containing the reference.</param>
        /// <param name="cref">The code reference that is not described.</param>
        /// <returns>A new <see cref="XmlDocInspectionIssue"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> or <paramref name="cref"/> is <see langword="null"/>.</exception>
        public static XmlDocInspectionIssue UndocumentedReference(IMember member, XmlDocTag xmlTag, string cref)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));
            if (cref is null)
                throw new ArgumentNullException(nameof(cref));

            return new(XmlDocInspectionIssueType.UndocumentedReference, xmlTag, member, codeReference: cref);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocInspectionIssue"/> for an untitled see-also reference.
        /// </summary>
        /// <param name="member">The member that has the empty see-also element.</param>
        /// <param name="href">The URI string of the see-also topic that is not described.</param>
        /// <returns>A new <see cref="XmlDocInspectionIssue"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> or <paramref name="href"/> is <see langword="null"/>.</exception>
        public static XmlDocInspectionIssue UntitledSeeAlso(IMember member, string href)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));
            if (href is null)
                throw new ArgumentNullException(nameof(href));

            return new(XmlDocInspectionIssueType.UntitledSeeAlso, XmlDocTag.SeeAlso, member, hyperlink: href);
        }

        /// <summary>
        /// Gets the type of issue.
        /// </summary>
        /// <value>
        /// The type of issue found during the inspection.
        /// </value>
        public readonly XmlDocInspectionIssueType IssueType { get; }

        /// <summary>
        /// Gets the XML documentation tag associated with the issue.
        /// </summary>
        /// <value>
        /// The XML documentation tag associated with the issue.
        /// </value>
        public readonly XmlDocTag XmlTag { get; }

        /// <summary>
        /// Gets the member associated with the issue.
        /// </summary>
        /// <value>
        /// The member that has the documentation issue.
        /// </value>
        public readonly IMember Member { get; }

        /// <summary>
        /// Gets the type parameter associated with the issue, if applicable.
        /// </summary>
        /// <value>
        /// The type parameter, or <see langword="null"/> if not applicable to this issue type.
        /// </value>
        /// <remarks>
        /// When the issue type is <see cref="XmlDocInspectionIssueType.MissingRequiredTag"/> and the tag is <see cref="XmlDocTag.TypeParam"/>,
        /// this property contains the type parameter that is not documented.
        /// </remarks>
        public readonly ITypeParameter? TypeParameter { get; }

        /// <summary>
        /// Gets the parameter associated with the issue, if applicable.
        /// </summary>
        /// <value>
        /// The parameter, or <see langword="null"/> if not applicable to this issue type.
        /// </value>
        /// <remarks>
        /// When the issue type is <see cref="XmlDocInspectionIssueType.MissingRequiredTag"/> and the tag is <see cref="XmlDocTag.Param"/>
        /// or <see cref="XmlDocTag.Returns"/>, this property contains the parameter that is not documented.
        /// </remarks>
        public readonly IParameter? Parameter { get; }

        /// <summary>
        /// Gets the code reference associated with the issue, if applicable.
        /// </summary>
        /// <value>
        /// The code reference, or <see langword="null"/> if not applicable to this issue type.
        /// </value>
        /// <remarks>
        /// When the issue type is <see cref="XmlDocInspectionIssueType.UndocumentedReference"/>, this property contains the code reference
        /// that is not described.
        /// </remarks>
        public readonly string? CodeReference { get; }

        /// <summary>
        /// Gets the hyperlink reference associated with the issue, if applicable.
        /// </summary>
        /// <value>
        /// The hyperlink reference, or <see langword="null"/> if not applicable to this issue type.
        /// </value>
        /// <remarks>
        /// When the issue type is <see cref="XmlDocInspectionIssueType.UntitledSeeAlso"/>, this property contains the URI string of the
        /// see-also topic that is not described.
        /// </remarks>
        public readonly string? Hyperlink { get; }

        /// <summary>
        /// Returns a string that represents the current issue.
        /// </summary>
        /// <returns>A string that represents the current issue.</returns>
        public override string ToString() => $"{Member.Name}: {IssueType} ({XmlTag})";
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    /// <summary>
    /// Represents a top-level XML documentation tag.
    /// </summary>
    /// <remarks>
    /// The <see cref="XmlDocTag"/> enum represents the standard and extended XML documentation tags that appear at the top
    /// level of .NET documentation comments.
    /// <para>
    /// Most of these tags are defined in the official .NET documentation standards, though some (like <c>event</c>, <c>threadsafety</c>,
    /// and <c>overloads</c>) are extensions that are commonly used but not part of the official standard.
    /// </para>
    /// </remarks>
    public enum XmlDocTag
    {
        /// <summary>
        /// Represents the <see href="https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#summary">summary</see> tag.
        /// </summary>
        /// <remarks>
        /// The <c>summary</c> tag is the most important tag in XML documentation comments. It provides a brief description of the member.
        /// </remarks>
        Summary,

        /// <summary>
        /// Represents the <see href="https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#typeparam">typeparam</see> tag.
        /// </summary>
        /// <remarks>
        /// The <c>typeparam</c> tag provides information about a type parameter of a generic member.
        /// </remarks>
        TypeParam,

        /// <summary>
        /// Represents the <see href="https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#param">param</see> tag.
        /// </summary>
        /// <remarks>
        /// The <c>param</c> tag provides information about a parameter of a member.
        /// </remarks>
        Param,

        /// <summary>
        /// Represents the <see href="https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#returns">returns</see> tag.
        /// </summary>
        /// <remarks>
        /// The <c>returns</c> tag provides information about the return value of a member.
        /// </remarks>
        Returns,

        /// <summary>
        /// Represents the <see href="https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#value">value</see> tag.
        /// </summary>
        /// <remarks>
        /// The <c>value</c> tag provides information about the value of a property.
        /// </remarks>
        Value,

        /// <summary>
        /// Represents the <see href="https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#remarks">remarks</see> tag.
        /// </summary>
        /// <remarks>
        /// The <c>remarks</c> tag provides additional information about the member.
        /// </remarks>
        Remarks,

        /// <summary>
        /// Represents the <see href="https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#example">example</see> tag.
        /// </summary>
        /// <remarks>
        /// The <c>example</c> tag provides an example of how to use the member.
        /// </remarks>
        Example,

        /// <summary>
        /// Represents the <see href="https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#exception">exception</see> tag.
        /// </summary>
        /// <remarks>
        /// The <c>exception</c> tag provides information about exceptions that the member can throw.
        /// </remarks>
        Exception,

        /// <summary>
        /// Represents the <see href="https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#permission">permission</see> tag.
        /// </summary>
        /// <remarks>
        /// The <c>permission</c> tag provides information about the permissions that the caller must have to call the member.
        /// </remarks>
        Permission,

        /// <summary>
        /// Represents the <see href="https://ewsoftware.github.io/XMLCommentsGuide/html/81bf7ad3-45dc-452f-90d5-87ce2494a182.htm">event</see> tag.
        /// </summary>
        /// <remarks>
        /// The <c>event</c> tag provides information about the event that the member raises.
        /// <note type="info" title="Information">
        /// The <c>event</c> tag is not part of the standard set of XML documentation tags.
        /// </note>
        /// </remarks>
        Event,

        /// <summary>
        /// Represents the <see href="https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags#seealso">seealso</see> tag.
        /// </summary>
        /// <remarks>
        /// The <c>seealso</c> tag provides a reference to a member or topic that is related to the member.
        /// </remarks>
        SeeAlso,

        /// <summary>
        /// Represents the <see href="https://ewsoftware.github.io/XMLCommentsGuide/html/fb4625cb-52d0-428e-9c7c-7a0d88e1b692.htm">threadsafety</see> tag.
        /// </summary>
        /// <remarks>
        /// The <c>threadsafety</c> tag provides information about the thread safety of the member.
        /// <note type="info" title="Information">
        /// The <c>threadsafety</c> tag is not part of the standard set of XML documentation tags.
        /// </note>
        /// </remarks>
        ThreadSafety,

        /// <summary>
        /// Represents the <see href="https://ewsoftware.github.io/XMLCommentsGuide/html/5b11b235-2b6c-4dfc-86b0-2e7dd98f2716.htm">overloads</see> tag.
        /// </summary>
        /// <remarks>
        /// The <c>overloads</c> tag provides information about the shared behavior of overloaded members. Unlike other tags, its content consists of top-level
        /// XML documentation elements that collectively document all overloads as a single group.
        /// <note type="tip" title="Tip">
        /// There is no need to repeat the <c>overloads</c> tag for each overloaded member, documenting the common behavior once for any one of them is sufficient.
        /// </note>
        /// <note type="info" title="Information">
        /// The <c>overloads</c> tag is not part of the standard set of XML documentation tags.
        /// </note>
        /// </remarks>
        Overloads,
    }
}

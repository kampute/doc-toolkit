// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Languages
{
    using Kampute.DocToolkit.Metadata;
    using System.IO;

    /// <summary>
    /// Represents a delegate for linking to the documentation of a type or type's member.
    /// </summary>
    /// <param name="textWriter">The <see cref="TextWriter"/> to which the link is written.</param>
    /// <param name="member">The member to link to.</param>
    /// <param name="memberName">The member's name to display in the link.</param>
    /// <remarks>
    /// The <see cref="MemberDocLinker"/> delegate is used to write links to the documentation of specified types or members.
    /// This delegate is typically used in conjunction with methods that format and write code elements.
    /// <para>
    /// By using this delegate, developers can ensure that links to documentation are consistently formatted and integrated
    /// within the generated output.
    /// </para>
    /// </remarks>
    /// <seealso cref="IProgrammingLanguage"/>
    public delegate void MemberDocLinker(TextWriter textWriter, IMember member, string memberName);
}

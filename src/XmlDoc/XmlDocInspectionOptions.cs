// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    using System;

    /// <summary>
    /// Specifies the options for inspecting XML documentation of members.
    /// </summary>
    /// <remarks>
    /// The <see cref="XmlDocInspectionOptions"/> enum provides a flexible way to configure which types
    /// of XML documentation issues should be reported during documentation inspection and validation.
    /// </remarks>
    [Flags]
    public enum XmlDocInspectionOptions
    {
        /// <summary>
        /// No issues will be reported.
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// Report missing or empty <c>summary</c> tags.
        /// </summary>
        Summary = 0x0001,

        /// <summary>
        /// Report missing or empty <c>typeparam</c> tags.
        /// </summary>
        TypeParam = 0x0002,

        /// <summary>
        /// Report missing or empty <c>param</c> tags.
        /// </summary>
        Param = 0x0004,

        /// <summary>
        /// Report missing or empty <c>returns</c> tags.
        /// </summary>
        Returns = 0x0008,

        /// <summary>
        /// Report missing documentation for a property's value.
        /// </summary>
        Value = 0x0010,

        /// <summary>
        /// Report missing descriptions for exceptions thrown by a member.
        /// </summary>
        Exception = 0x0020,

        /// <summary>
        /// Report missing descriptions for permissions required by a member.
        /// </summary>
        Permission = 0x0040,

        /// <summary>
        /// Report missing descriptions for events triggered by a member.
        /// </summary>
        Event = 0x0080,

        /// <summary>
        /// Report <c>see-also</c> hyperlinks that lack descriptive titles.
        /// </summary>
        SeeAlso = 0x0100,

        /// <summary>
        /// Report missing or empty <c>remarks</c> tags.
        /// </summary>
        Remarks = 0x0200,

        /// <summary>
        /// Report missing or empty <c>example</c> tags.
        /// </summary>
        Example = 0x0400,

        /// <summary>
        /// Report missing documentation regarding thread safety for class and struct types.
        /// </summary>
        ThreadSafety = 0x0800,

        /// <summary>
        /// Report missing shared summary documentation for overloaded members.
        /// </summary>
        Overloads = 0x1000,

        /// <summary>
        /// Omit reporting missing <c>summary</c> tags for default constructors that have no overloads.
        /// </summary>
        OmitImplicitlyCreatedConstructors = 0x2000,

        /// <summary>
        /// Report issues related to the minimum required documentation.
        /// These include missing or empty <c>summary</c>, <c>typeparam</c>, <c>param</c>, and <c>returns</c> tags.
        /// </summary>
        Required = Summary | TypeParam | Param | Returns,

        /// <summary>
        /// Report issues related to the recommended documentation.
        /// These include all required documentation issues plus <c>value</c>, <c>exception</c>, <c>permission</c>, <c>event</c>, and <c>see-also</c> tags.
        /// </summary>
        Recommended = Required | Value | Exception | Permission | Event | SeeAlso,

        /// <summary>
        /// Report all issues.
        /// </summary>
        All = Recommended | Remarks | Example | ThreadSafety | Overloads
    }
}

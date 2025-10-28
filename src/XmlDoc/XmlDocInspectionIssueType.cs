// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc
{
    /// <summary>
    /// Specifies the type of issue found during the inspection of XML documentation comments.
    /// </summary>
    public enum XmlDocInspectionIssueType
    {
        /// <summary>
        /// A required XML documentation tag is missing or has no content.
        /// </summary>
        MissingRequiredTag,

        /// <summary>
        /// An optional XML documentation tag is missing or has no content.
        /// </summary>
        MissingOptionalTag,

        /// <summary>
        /// A reference in an XML documentation tag is not properly described.
        /// </summary>
        UndocumentedReference,

        /// <summary>
        /// A see-also reference to a URI has no description.
        /// </summary>
        UntitledSeeAlso
    }
}

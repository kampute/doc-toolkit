// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc.Comments
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    /// <summary>
    /// Represents a documentation content that is associated with a code reference.
    /// </summary>
    /// <remarks>
    /// The <see cref="ReferenceComment"/> class processes XML documentation tags that contain references to other code elements,
    /// such as <c>&lt;exception&gt;</c> tags. These references allow documentation to create links between related elements in the
    /// generated documentation.
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class ReferenceComment : Comment
    {
        private readonly List<XElement> alternateElements = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceComment"/> class.
        /// </summary>
        /// <param name="cref">The code reference being described.</param>
        /// <param name="element">The XML element containing the description.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cref"/> or <paramref name="element"/> is <see langword="null"/>.</exception>
        protected ReferenceComment(string cref, XElement element)
            : base(element)
        {
            Reference = cref ?? throw new ArgumentNullException(nameof(cref));
        }

        /// <summary>
        /// Gets the code reference being described.
        /// </summary>
        /// <value>
        /// The code reference being described.
        /// </value>
        public string Reference { get; }

        /// <summary>
        /// Gets any additional XML elements that provide variants or supplementary documentation content for this reference.
        /// </summary>
        /// <value>
        /// The read-only list of additional XML elements associated with this reference that provide variants or supplementary documentation content.
        /// </value>
        public IReadOnlyList<XElement> Variants => alternateElements;

        /// <summary>
        /// Attempts to create a new instance of the <see cref="ReferenceComment"/> class from the specified XML element.
        /// </summary>
        /// <param name="element">The XML element containing the code reference and description.</param>
        /// <param name="comment">When this method returns, contains the created reference comment if successful; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a reference comment was successfully created; otherwise, <see langword="false"/>.</returns>
        public static bool TryCreate(XElement element, [NotNullWhen(true)] out ReferenceComment? comment)
        {
            if (element is not null && element.TryGetAttributeValue("cref", out var cref))
            {
                comment = new ReferenceComment(cref, element);
                return true;
            }

            comment = null;
            return false;
        }

        /// <summary>
        /// Collects all references from the specified XML elements, preserving the order of first occurrence.
        /// </summary>
        /// <param name="elements">The XML elements to collect references from.</param>
        /// <returns>An list of <see cref="ReferenceComment"/> instances in the order they first appear.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
        public static new List<ReferenceComment> Collect(IEnumerable<XElement> elements)
        {
            if (elements is null)
                throw new ArgumentNullException(nameof(elements));

            var seenReferences = new Dictionary<string, int>(StringComparer.Ordinal);
            var comments = new List<ReferenceComment>();

            foreach (var element in elements)
            {
                if (element is null || !element.TryGetAttributeValue("cref", out var cref))
                    continue;

                if (seenReferences.TryGetValue(cref, out var index))
                    comments[index].alternateElements.Add(element);
                else
                {
                    seenReferences[cref] = comments.Count;
                    comments.Add(new ReferenceComment(cref, element));
                }
            }

            return comments;
        }
    }
}

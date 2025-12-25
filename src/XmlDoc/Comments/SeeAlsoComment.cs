// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc.Comments
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    /// <summary>
    /// Represents the documentation content for a see-also reference, which is associated with a code reference or hyperlink.
    /// </summary>
    /// <remarks>
    /// The <see cref="SeeAlsoComment"/> class represents a <c>&lt;seealso&gt;</c> XML documentation tag, which is used to create
    /// cross-references between documentation topics. These can link to other code elements using a code reference (cref) or to
    /// external resources using hyperlinks (href).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class SeeAlsoComment : Comment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SeeAlsoComment"/> class for a code reference.
        /// </summary>
        /// <param name="link">The code reference (cref) or hyperlink (href) being referenced.</param>
        /// <param name="element">The XML element containing the description of the see-also reference.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="link"/> or <paramref name="element"/> is <see langword="null"/>.</exception>
        protected SeeAlsoComment(string link, XElement element)
            : base(element)
        {
            Target = link ?? throw new ArgumentNullException(nameof(link));
        }

        /// <summary>
        /// Gets the code reference (cref) or hyperlink (href) being referenced.
        /// </summary>
        /// <value>
        /// A string representing the code reference (cref) or hyperlink (href) being described.
        /// </value>
        public string Target { get; }

        /// <summary>
        /// Gets a value indicating whether the reference is a hyperlink.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the reference is a hyperlink; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsHyperlink => Content.Attribute("href") is not null;

        /// <summary>
        /// Gets a value indicating whether the reference is a code reference.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the reference is a code reference; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsCodeReference => Content.Attribute("cref") is not null;

        /// <summary>
        /// Returns a new version of this see-also comment with resolved cross-reference and description, if its reference is a relative URL.
        /// </summary>
        /// <param name="context">The documentation context that provides topic resolution and URL services.</param>
        /// <returns>A new <see cref="SeeAlsoComment"/> with a properly addressed cross-reference and descriptive text if applicable; otherwise, the original instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method returns a new version of the see-also comment if its reference is a relative URL. The refinement process includes:
        /// <list type="bullet">
        ///   <item><description>Applying the context's URL addressing strategy and translations to the reference</description></item>
        ///   <item><description>Using the target topic's title when the reference lacks descriptive text</description></item>
        /// </list>
        /// <note type="info" title="Implementation Note">
        /// You typically do not need to call this method directly, as it is automatically invoked during the processing of XML documentation comments.
        /// </note>
        /// </remarks>
        public SeeAlsoComment WithResolvedHyperlink(IDocumentationContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (IsCodeReference || Uri.IsWellFormedUriString(Target, UriKind.Absolute))
                return this;

            if (!context.UrlTransformer.TryTransformUrl(Target, out var adjustedUrl))
                return this;

            if (IsEmpty && context.Topics.TryResolve(UriHelper.GetPathPart(Target), out var topic))
                Content.Value = topic.Name;

            return new SeeAlsoComment(adjustedUrl.ToString(), Content);
        }

        /// <summary>
        /// Attempts to create a new instance of the <see cref="SeeAlsoComment"/> class from the specified XML element.
        /// </summary>
        /// <param name="element">The XML element containing the see-also reference.</param>
        /// <param name="comment">When this method returns, contains the created see-also comment if successful; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a comment was successfully created; otherwise, <see langword="false"/>.</returns>
        public static bool TryCreate(XElement element, [NotNullWhen(true)] out SeeAlsoComment? comment)
        {
            if (element is null)
            {
                // Nothing to do if the element is null.
            }
            else if (element.TryGetAttributeValue("cref", out var cref))
            {
                comment = new SeeAlsoComment(cref, element);
                return true;
            }
            else if (element.TryGetAttributeValue("href", out var href))
            {
                comment = new SeeAlsoComment(href, element);
                return true;
            }

            comment = null;
            return false;
        }

        /// <summary>
        /// Collects all see-also references from the specified XML elements.
        /// </summary>
        /// <param name="elements">The XML elements to collect see-also references from.</param>
        /// <returns>An enumerable collection of <see cref="SeeAlsoComment"/> instances.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
        public static new IEnumerable<SeeAlsoComment> Collect(IEnumerable<XElement> elements)
        {
            if (elements is null)
                throw new ArgumentNullException(nameof(elements));

            foreach (var element in elements)
            {
                if (TryCreate(element, out var comment))
                    yield return comment;
            }
        }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc.Comments
{
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.XmlDoc;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    /// <summary>
    /// Represents the documentation content for a top-level XML comment.
    /// </summary>
    /// <remarks>
    /// The <see cref="Comment"/> class serves as the fundamental representation of XML documentation content for
    /// a top-level XML comment. It encapsulates the XML element that contains the documentation and provides
    /// methods for transforming the content into a string representation.
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class Comment
    {
        private readonly XElement element;

        /// <summary>
        /// Represents an empty comment.
        /// </summary>
        public static readonly Comment Empty = new(new("none"));

        /// <summary>
        /// Initializes a new instance of the <see cref="Comment"/> class.
        /// </summary>
        /// <param name="element">The XML element containing the description.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> is <see langword="null"/>.</exception>
        protected Comment(XElement element)
        {
            this.element = element ?? throw new ArgumentNullException(nameof(element));
        }

        /// <summary>
        /// Gets the underlying XML element containing the documentation content.
        /// </summary>
        /// <value>
        /// The underlying XML element containing the documentation content.
        /// </value>
        public XElement Content => element;

        /// <summary>
        /// Gets a value indicating whether the comment is without content.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the comment is without content; otherwise, <see langword="false"/>.
        /// </value>
        public virtual bool IsEmpty => element.IsEmpty || string.IsNullOrWhiteSpace(element.Value);

        /// <summary>
        /// Converts the comment to its string representation without formatting.
        /// </summary>
        /// <returns>The string representation of the comment.</returns>
        public override string ToString() => element.Value;

        /// <summary>
        /// Converts the XML comment to its string representation using the specified formatter.
        /// </summary>
        /// <param name="formatter">The formatter to use for converting the XML comment to a string.</param>
        /// <returns>The string representation of the comment.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formatter"/> is <see langword="null"/>.</exception>
        public virtual string ToString(IXmlDocTransformer formatter)
        {
            if (formatter is null)
                throw new ArgumentNullException(nameof(formatter));

            using var writer = StringBuilderPool.Shared.GetWriter();
            formatter.Transform(writer, element);
            return writer.ToString();
        }

        /// <summary>
        /// Creates a comment from the specified XML element.
        /// </summary>
        /// <param name="element">The XML element containing the description.</param>
        /// <returns>A comment representing the specified XML element.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Comment Create(XElement? element) => element is null || element.IsEmpty ? Empty : new(element);

        /// <summary>
        /// Collects all comments that are associated with a name from the specified XML elements.
        /// </summary>
        /// <param name="elements">The XML elements to collect the comments from.</param>
        /// <returns>A dictionary of named comments.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
        public static Dictionary<string, Comment> Collect(IEnumerable<XElement> elements)
        {
            if (elements is null)
                throw new ArgumentNullException(nameof(elements));

            var dictionary = new Dictionary<string, Comment>(StringComparer.Ordinal);
            foreach (var element in elements)
            {
                if (element.TryGetAttributeValue("name", out var name))
                    dictionary[name] = Create(element);
            }
            return dictionary;
        }
    }
}

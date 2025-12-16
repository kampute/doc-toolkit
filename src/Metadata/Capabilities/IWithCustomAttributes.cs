// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Capabilities
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines a contract for metadata elements that can have custom attributes.
    /// </summary>
    public interface IWithCustomAttributes
    {
        /// <summary>
        /// Gets a value indicating whether the element has any custom attributes applied.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the element has one or more custom attributes; otherwise, <see langword="false"/>.
        /// </value>
        bool HasCustomAttributes => CustomAttributes.Count > 0;

        /// <summary>
        /// Gets the custom attributes applied to the element.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="ICustomAttribute"/> instances representing the custom attributes applied to the element.
        /// </value>
        IReadOnlyList<ICustomAttribute> CustomAttributes { get; }

        /// <summary>
        /// Gets the custom attributes explicitly applied to the member.
        /// </summary>
        /// <value>
        /// An enumerable collection of <see cref="ICustomAttribute"/> objects representing the custom attributes explicitly applied to the member.
        /// </value>
        IEnumerable<ICustomAttribute> ExplicitCustomAttributes => CustomAttributes.Where(static a => !a.IsImplicitlyApplied);

        /// <summary>
        /// Determines whether the element has a specific custom attribute applied.
        /// </summary>
        /// <param name="attributeFullName">The full name of the attribute type to check for.</param>
        /// <returns>
        /// <see langword="true"/> if the element has the specified attribute applied; otherwise, <see langword="false"/>.
        /// </returns>
        bool HasCustomAttribute(string attributeFullName);
    }
}

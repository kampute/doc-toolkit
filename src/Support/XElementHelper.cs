// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    /// <summary>
    /// Provides extension methods for <see cref="XElement"/> instances.
    /// </summary>
    public static class XElementHelper
    {
        /// <summary>
        /// Determines whether the element has an attribute with the specified name.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> instance.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns><see langword="true"/> if the attribute exists; otherwise, <see langword="false"/>.</returns>
        public static bool HasAttribute(this XElement element, string attributeName)
        {
            return element?.Attribute(attributeName) is not null;
        }

        /// <summary>
        /// Attempts to get the value of the attribute with the specified name.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> instance.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <param name="value">When this method returns, contains the value of the attribute, if the attribute is found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the attribute is found and has a value; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetAttributeValue(this XElement element, string attributeName, [NotNullWhen(true)] out string? value)
        {
            value = element?.Attribute(attributeName)?.Value;
            return !string.IsNullOrEmpty(value);
        }
    }
}

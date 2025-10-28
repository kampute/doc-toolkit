// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;

    /// <summary>
    /// Defines a contract for accessing enum-specific metadata.
    /// </summary>
    public interface IEnumType : IType, IWithFields
    {
        /// <summary>
        /// Gets the underlying type of the enum.
        /// </summary>
        /// <value>
        /// The underlying type (e.g., <see cref="int"/>, <see cref="byte"/>, <see cref="long"/>, etc.) of the enum.
        /// </value>
        IType UnderlyingType { get; }

        /// <summary>
        /// Gets a value indicating whether the enum is decorated with the <c>Flags</c> attribute.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the enum is decorated with the <c>Flags</c> attribute; otherwise, <see langword="false"/>.
        /// </value>
        bool IsFlagsEnum => HasCustomAttribute("System.FlagsAttribute");

        /// <summary>
        /// Retrieves the name of the enum member corresponding to the specified value.
        /// </summary>
        /// <param name="value">The value to look up.</param>
        /// <returns>The name of the enum member if found; otherwise, <see langword ="null"/>.</returns>
        string? GetEnumName(object value);
    }
}

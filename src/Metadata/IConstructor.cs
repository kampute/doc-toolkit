// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;

    /// <summary>
    /// Defines a contract for accessing constructor metadata.
    /// </summary>
    public interface IConstructor : ITypeMember, IWithParameters, IWithOverloads
    {
        /// <summary>
        /// Gets a value indicating whether this constructor is the default public constructor of a type.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this constructor is a default public constructor; otherwise, <see langword="false"/>.
        /// </value>
        bool IsDefaultConstructor => !IsStatic && Visibility == MemberVisibility.Public && Parameters.Count == 0;

        /// <summary>
        /// Gets the constructor in the base class that has the same signature as this constructor, if any.
        /// </summary>
        /// <value>
        /// The constructor in the base class that has the same signature as this constructor or <see langword="null"/> if none exists.
        /// </value>
        IConstructor? BaseConstructor { get; }
    }
}

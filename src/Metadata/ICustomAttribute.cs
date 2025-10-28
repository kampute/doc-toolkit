// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Defines a contract for accessing custom attribute metadata.
    /// </summary>
    public interface ICustomAttribute : IMetadataAdapter<CustomAttributeData>
    {
        /// <summary>
        /// Gets the target of the attribute.
        /// </summary>
        /// <value>
        /// The <see cref="AttributeTarget"/> indicating where the attribute is applied.
        /// </value>
        AttributeTarget Target { get; }

        /// <summary>
        /// Gets the type of the attribute.
        /// </summary>
        /// <value>The attribute type metadata.</value>
        IClassType Type { get; }

        /// <summary>
        /// Gets the constructor arguments used to create the attribute.
        /// </summary>
        /// <value>
        /// A read-only list of constructor argument values.
        /// </value>
        IReadOnlyList<TypedValue> ConstructorArguments { get; }

        /// <summary>
        /// Gets the named arguments (properties and fields) set on the attribute.
        /// </summary>
        /// <value>
        /// A read-only dictionary of named argument values.
        /// </value>
        IReadOnlyDictionary<string, TypedValue> NamedArguments { get; }

        /// <summary>
        /// Gets a value indicating whether the attribute is applied by the compiler.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the attribute is applied by the compiler; otherwise, <see langword="false"/>.
        /// </value>
        bool IsImplicitlyApplied { get; }
    }
}

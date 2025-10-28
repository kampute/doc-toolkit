// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;

    /// <summary>
    /// Defines a contract for accessing metadata for types that are composite types.
    /// </summary>
    /// <remarks>
    /// This interface provides access to metadata specific to composite types (classes and structs).
    /// </remarks>
    public interface ICompositeType : IGenericCapableType, IInterfaceCapableType,
        IWithConstructors, IWithFields, IWithProperties, IWithMethods, IWithEvents, IWithOperators, IWithNestedTypes, IWithExplicitInterfaceMembers
    {
    }
}

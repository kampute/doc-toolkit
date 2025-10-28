// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;

    /// <summary>
    /// Defines a contract for accessing primitive type-specific metadata.
    /// </summary>
    public interface IPrimitiveType : IInterfaceCapableType, IWithFields, IWithMethods, IWithOperators
    {
    }
}

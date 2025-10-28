// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Defines a contract for accessing interface-specific metadata.
    /// </summary>
    public interface IInterfaceType : IGenericCapableType, IInterfaceCapableType, IWithMethods, IWithProperties, IWithEvents
    {
        /// <summary>
        /// Gets the types that implement the interface either directly or indirectly within the same assembly.
        /// </summary>
        /// <value>
        /// An enumerable of <see cref="IInterfaceCapableType"/> representing the types that implement the interface.
        /// </value>
        /// <remarks>
        /// This property includes all types that implement the interface, whether they do so directly or through inheritance. The types in the
        /// returned collection only include exported types from the assembly.
        /// <para>
        /// This property is implemented by using deferred execution. The immediate return value is an object that stores all the information
        /// that is required to perform the action.
        /// </para>
        /// </remarks>
        IEnumerable<IInterfaceCapableType> ImplementingTypes
        {
            get
            {
                return Assembly.ExportedTypes
                    .OfType<IInterfaceCapableType>()
                    .Where(IsGenericTypeDefinition ? ImplementsThisGenericInterface : ImplementsThisNonGenericInterface);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                bool ImplementsThisGenericInterface(IInterfaceCapableType type) => type.Interfaces.Any(i => Equals(i) || (i.IsConstructedGenericType && Equals(i.GenericTypeDefinition!)));

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                bool ImplementsThisNonGenericInterface(IInterfaceCapableType type) => type.Interfaces.Contains(this);
            }
        }
    }
}

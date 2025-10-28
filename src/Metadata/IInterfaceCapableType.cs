// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines a contract for accessing metadata for types that can extend or implement interfaces.
    /// </summary>
    public interface IInterfaceCapableType : IType, IWithInterfaces
    {
        /// <summary>
        /// Gets the interfaces that are directly implemented by the type, excluding those inherited from base types or other interfaces.
        /// </summary>
        /// <value>
        /// An enumerable of <see cref="IInterfaceType"/> representing the directly implemented interfaces.
        /// </value>
        /// <remarks>
        /// This property filters out interfaces that are inherited from base types or other interfaces, providing a clear view of the
        /// interfaces that are explicitly implemented by the type itself.
        /// <para>
        /// This property is implemented by using deferred execution. The immediate return value is an object that stores all the
        /// information that is required to perform the action.
        /// </para>
        /// </remarks>
        IEnumerable<IInterfaceType> ImplementedInterfaces
        {
            get
            {
                if (Interfaces.Count == 0)
                    return [];

                var inheritedInterfaces = Interfaces.SelectMany(i => i.Interfaces);

                if (BaseType is IWithInterfaces baseType)
                    inheritedInterfaces = inheritedInterfaces.Concat(baseType.Interfaces);

                return Interfaces.Except(inheritedInterfaces, ReferenceEqualityComparer<IInterfaceType>.Instance);
            }
        }

        /// <summary>
        /// Determines whether the current type implements the specified interface.
        /// </summary>
        /// <param name="interfaceType">The interface type to check for implementation.</param>
        /// <returns><see langword="true"/> if the current type implements the specified interface; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="interfaceType"/> is <see langword="null"/>.</exception>
        bool Implements(IInterfaceType interfaceType)
        {
            if (interfaceType is null)
                throw new ArgumentNullException(nameof(interfaceType));

            if (!interfaceType.IsGenericTypeDefinition)
                return Interfaces.Contains(interfaceType, ReferenceEqualityComparer<IInterfaceType>.Instance);

            foreach (var i in Interfaces)
            {
                if (ReferenceEquals(interfaceType, i))
                    return true;

                if (i.IsConstructedGenericType && ReferenceEquals(interfaceType, i.GenericTypeDefinition))
                    return true;
            }

            return false;
        }
    }
}

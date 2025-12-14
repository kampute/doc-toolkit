// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines a contract for accessing class-specific metadata.
    /// </summary>
    public interface IClassType : ICompositeType
    {
        /// <summary>
        /// Gets a value indicating whether the class is abstract.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the class is abstract; otherwise, <see langword="false"/>.
        /// </value>
        bool IsAbstract { get; }

        /// <summary>
        /// Gets a value indicating whether the class is sealed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the class is sealed; otherwise, <see langword="false"/>.
        /// </value>
        bool IsSealed { get; }

        /// <summary>
        /// Gets a value indicating whether the class may contain extension members.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the class may contain extension members; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// A class can declare extension members if it is static, not nested, and not a generic type.
        /// </remarks>
        bool MayContainExtensionMembers { get; }

        /// <summary>
        /// Gets the collection of extension blocks defined in the class.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="IExtensionBlock"/> objects representing the extension blocks defined in the class.
        /// </value>
        IReadOnlyCollection<IExtensionBlock> ExtensionBlocks { get; }

        /// <summary>
        /// Gets the types that directly derive from the class in the same assembly.
        /// </summary>
        /// <value>
        /// An enumerable collection of <see cref="IClassType"/> objects representing the types that directly derive from the class.
        /// </value>
        /// <remarks>
        /// This property collects types that are direct subclasses of the current class within the same assembly. The types in the returned
        /// collection only include exported types from the assembly.
        /// <para>
        /// This property is implemented by using deferred execution. The immediate return value is an object that stores all the information
        /// that is required to perform the action.
        /// </para>
        /// </remarks>
        IEnumerable<IClassType> DerivedTypes
        {
            get
            {
                Func<IClassType, bool> isSameType = IsGenericTypeDefinition
                    ? t => ReferenceEquals(this, t) || (t.IsConstructedGenericType && ReferenceEquals(this, t.GenericTypeDefinition))
                    : t => ReferenceEquals(this, t);

                return Assembly.ExportedTypes.OfType<IClassType>().Where(t => t.BaseType is not null && isSameType(t.BaseType));
            }
        }

        /// <summary>
        /// Determines whether the type is a subclass of the given type, including indirect inheritance through the type hierarchy.
        /// </summary>
        /// <param name="type">The type to check against.</param>
        /// <returns><see langword="true"/> if the type is a subclass of the specified type; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        bool IsSubclassOf(IClassType type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            for (var current = BaseType; current is not null; current = current.BaseType)
            {
                if (type.Equals(current))
                    return true;
            }

            return false;
        }
    }
}

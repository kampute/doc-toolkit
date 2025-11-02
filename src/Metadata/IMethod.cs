// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for accessing method metadata.
    /// </summary>
    public interface IMethod : IVirtualTypeMember, IMethodBase
    {
        /// <summary>
        /// Gets the type parameters declared by the method if it is generic.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="ITypeParameter"/> instances representing the type parameters declared by the method.
        /// </value>
        IReadOnlyList<ITypeParameter> TypeParameters { get; }

        /// <summary>
        /// Gets a value indicating whether the method is generic.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the method is generic; otherwise, <see langword="false"/>.
        /// </value>
        bool IsGenericMethod { get; }

        /// <summary>
        /// Gets a value indicating whether the method is read-only.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the method is read-only; otherwise, <see langword="false"/>.
        /// </value>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets a value indicating whether the method is asynchronous.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the method is asynchronous; otherwise, <see langword="false"/>.
        /// </value>
        bool IsAsync { get; }

        /// <summary>
        /// Gets a value indicating whether the method is an extension method.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the method is an extension method; otherwise, <see langword="false"/>.
        /// </value>
        bool IsExtension { get; }

        /// <summary>
        /// Gets the base method that this method overrides, if any.
        /// </summary>
        /// <value>
        /// The base method that this method overrides, or <see langword="null"/> if none.
        /// </value>
        IMethod? OverriddenMethod { get; }

        /// <summary>
        /// Gets the interface method that this method implements, if any.
        /// </summary>
        /// <value>
        /// The interface method that this method implements, or <see langword="null"/> if none.
        /// </value>
        IMethod? ImplementedMethod { get; }

        /// <summary>
        /// Determines whether the method is an extension method for the given type.
        /// </summary>
        /// <param name="type">The type to check against.</param>
        /// <returns><see langword="true"/> if the method is an extension method for the specified type; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        bool IsExtensionMethodFor(IType type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return IsExtension && Parameters[0].Type.IsAssignableFrom(type);
        }
    }
}

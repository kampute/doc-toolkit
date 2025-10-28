// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System;

    /// <summary>
    /// Defines a contract for types that decorate or wrap another type (arrays, pointers, by-ref, or nullable).
    /// </summary>
    public interface ITypeDecorator : IType
    {
        /// <summary>
        /// Gets the element type.
        /// </summary>
        /// <value>
        /// The element type of the current type.
        /// </value>
        IType ElementType { get; }

        /// <summary>
        /// Gets the type modifier that indicates how the type decorates or wraps an element type.
        /// </summary>
        /// <value>
        /// A <see cref="TypeModifier"/> value indicating how the type decorates or wraps an element type (array, pointer, nullable, or by-ref).
        /// </value>
        TypeModifier Modifier { get; }

        /// <summary>
        /// Gets the rank of the array if the type is an array.
        /// </summary>
        /// <value>
        /// The rank of the array, or 0 if the type is not an array.
        /// </value>
        int ArrayRank { get; }

        /// <summary>
        /// Retrieves the underlying type by unwrapping any decorators such as arrays, pointers, by-ref, or nullable types.
        /// </summary>
        /// <param name="decoratorVisitor">An optional action to invoke for each decorator encountered during unwrapping.</param>
        /// <returns>The underlying type without any decorators.</returns>
        IType Unwrap(Action<ITypeDecorator>? decoratorVisitor = null)
        {
            for (var decorator = this; ;)
            {
                decoratorVisitor?.Invoke(decorator);

                if (decorator.ElementType is not ITypeDecorator innerDecorator)
                    return decorator.ElementType;

                decorator = innerDecorator;
            }
        }
    }
}

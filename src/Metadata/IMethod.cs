// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a contract for accessing method metadata.
    /// </summary>
    /// <remarks>
    /// For extension methods, this interface provides a logical view that reflects the extended type rather than the underlying implementation:
    /// <list type="bullet">
    ///   <item><see cref="IMember.IsStatic"/> indicates whether the extension method is static or instance-based.</item>
    ///   <item><see cref="IWithParameters.Parameters"/> excludes the first parameter (the <c>this</c> parameter).</item>
    ///   <item><see cref="TypeParameters"/> excludes type parameters from the extended type.</item>
    ///   <item><see cref="IsGenericMethod"/> indicates whether the method declares its own type parameters.</item>
    /// </list>
    /// </remarks>
    public interface IMethod : IMethodBase, IWithExtensionBehavior
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
    }
}

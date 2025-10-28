// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit;

    /// <summary>
    /// Represents an addressing strategy that requires documentation context information for initialization.
    /// </summary>
    /// <remarks>
    /// This interface is implemented by addressing strategies that need access to documentation context during
    /// initialization to resolve topic names, configure paths, or establish other context-dependent settings.
    /// Such strategies cannot be fully configured at construction time because the required context information
    /// is not yet available.
    /// <para>
    /// The binding must be performed after the documentation context is loaded and before the strategy
    /// is used for address resolution. Implementations typically use lazy initialization to ensure
    /// context-dependent operations occur at the appropriate time, avoiding coordination problems with
    /// component startup order.
    /// </para>
    /// </remarks>
    public interface IContextualAddressingStrategy : IDocumentAddressingStrategy
    {
        /// <summary>
        /// Binds the addressing strategy to the specified documentation context.
        /// </summary>
        /// <param name="context">The documentation context to bind to.</param>
        void InitializeWithContext(IDocumentationContext context);
    }
}
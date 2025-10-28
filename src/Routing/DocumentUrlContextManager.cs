// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;

    /// <summary>
    /// Represents the base class for managing document-specific URL contexts for adjusting cross-document relative links.
    /// </summary>
    /// <remarks>
    /// This abstract class implements the Template Method pattern to handle relative URL adjustment in generated documentation.
    /// It maintains a context hierarchy that ensures proper URL resolution regardless of a document's location in the documentation
    /// structure.
    /// <para>
    /// When extending this class, implement the <see cref="CreateScope"/> method to provide a custom <see cref="DocumentUrlContext"/>
    /// that defines how URLs are adjusted based on your specific requirements.
    /// </para>
    /// The class manages nested scopes in a thread-safe manner, supporting concurrent documentation processing. Scopes are cached
    /// by directory path to minimize overhead when frequently accessing the same directories in the documentation structure.
    /// <para>
    /// Implementations of the scope should be stateless, thread-safe, and extend <see cref="DocumentUrlContext"/> to ensure proper scope
    /// life cycle management through the hierarchy.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    /// <seealso cref="DocumentUrlContext"/>
    public abstract class DocumentUrlContextManager : IDocumentUrlContextProvider
    {
        private readonly AsyncLocal<Stack<UrlContext>> activeScopes = new();
        private readonly Lazy<UrlContext> rootScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentUrlContextManager"/> class.
        /// </summary>
        protected DocumentUrlContextManager()
        {
            rootScope = new(() => CreateScope(string.Empty, null));
        }

        /// <summary>
        /// Gets the currently active URL context.
        /// </summary>
        /// <value>
        /// The currently active URL context.
        /// </value>
        public DocumentUrlContext ActiveScope
        {
            get
            {
                var scopes = activeScopes.Value;
                return scopes is not null && scopes.TryPeek(out var current) ? current : rootScope.Value;
            }
        }

        /// <summary>
        /// Creates a scoped URL context for the specified document path and model.
        /// </summary>
        /// <param name="directory">The relative directory path of the document being rendered within the documentation structure.</param>
        /// <param name="model">The document model being processed, or <see langword="null"/> if not applicable.</param>
        /// <returns>An <see cref="DocumentUrlContext"/> object that represents the URL context scope. When disposed, the context will be reset.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="directory"/> is an absolute path.</exception>
        public DocumentUrlContext BeginScope(string directory, IDocumentModel? model)
        {
            if (Path.IsPathRooted(directory))
                throw new ArgumentException("The directory path must be relative to the documentation root directory.", nameof(directory));

            var scopes = activeScopes.Value;
            if (scopes is null)
            {
                scopes = new Stack<UrlContext>();
                activeScopes.Value = scopes;
            }

            if (string.IsNullOrEmpty(directory) && model is null)
            {
                var root = rootScope.Value;
                scopes.Push(root);
                return root;
            }

            var scope = CreateScope(directory, model);
            scopes.Push(scope);
            return scope;
        }

        /// <summary>
        /// Ends the current URL context and restores the previous context.
        /// </summary>
        /// <param name="scope">The URL context scope to end.</param>
        /// <exception cref="InvalidOperationException">Thrown when the specified scope is not the current active scope.</exception>
        protected void EndScope(UrlContext scope)
        {
            var scopes = activeScopes.Value;
            if (scopes is not null && scopes.TryPeek(out var current) && ReferenceEquals(current, scope))
                scopes.Pop();
            else
                throw new InvalidOperationException("URL scopes must be ended in the reverse order they were started.");
        }

        /// <summary>
        /// Creates a new scope for the specified directory path and document model.
        /// </summary>
        /// <param name="directory">The relative directory path of the document being rendered within the documentation structure.</param>
        /// <param name="model">The document model being processed, or <see langword="null"/> if not applicable.</param>
        /// <returns>A new <see cref="UrlContext"/> for the specified directory and model.</returns>
        protected abstract UrlContext CreateScope(string directory, IDocumentModel? model);

        /// <summary>
        /// Provides the base functionality for a scoped URL context that is managed by the <see cref="DocumentUrlContextManager"/>.
        /// </summary>
        protected abstract class UrlContext : DocumentUrlContext
        {
            private bool isDisposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="UrlContext"/> class.
            /// </summary>
            /// <param name="owner">The owning URL manager.</param>
            /// <param name="directory">The directory path of the document being rendered relative to the documentation root.</param>
            /// <param name="model">The document model associated with the current context or <see langword="null"/> if not applicable.</param>
            protected UrlContext(DocumentUrlContextManager owner, string directory, IDocumentModel? model)
                : base(directory, model)
            {
                Owner = owner;
            }

            /// <summary>
            /// Gets the owner of this scope.
            /// </summary>
            /// <value>
            /// The <see cref="DocumentUrlContextManager"/> that owns this scope.
            /// </value>
            protected DocumentUrlContextManager Owner { get; }

            /// <summary>
            /// Restores the previous document context.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public sealed override void Dispose()
            {
                if (!isDisposed)
                {
                    Owner.EndScope(this);
                    isDisposed = true;
                }
            }
        }
    }
}
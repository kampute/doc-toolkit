// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata.Reflection;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An adapter that wraps an <see cref="ExtensionBlockInfo"/> and provides metadata access.
    /// </summary>
    public class ExtensionBlockAdapter : IExtensionBlock
    {
        private readonly Lazy<IParameter> receiver;
        private readonly Lazy<IReadOnlyList<ITypeParameter>> typeParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionBlockAdapter"/> class.
        /// </summary>
        /// <param name="assembly">The assembly that contains the extension block.</param>
        /// <param name="extensionBlock">The extension block information to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> or <paramref name="extensionBlock"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="extensionBlock"/> does not belong to the specified assembly.</exception>
        public ExtensionBlockAdapter(IAssembly assembly, ExtensionBlockInfo extensionBlock)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            Block = extensionBlock ?? throw new ArgumentNullException(nameof(extensionBlock));

            if (!assembly.Represents(extensionBlock.Assembly))
                throw new ArgumentException("The provided assembly does not declare the extension block.", nameof(assembly));

            receiver = new(() => Assembly.Repository.GetParameterMetadata(Block.Receiver));
            typeParameters = new(() => [.. Block.TypeParameters.Select(Assembly.Repository.GetTypeMetadata<ITypeParameter>)]);
        }

        /// <summary>
        /// Gets the underlying extension block information.
        /// </summary>
        /// <value>
        /// The underlying extension block information.
        /// </value>
        protected ExtensionBlockInfo Block { get; }

        /// <inheritdoc/>
        public IAssembly Assembly { get; }

        /// <inheritdoc/>
        public IParameter Receiver => receiver.Value;

        /// <inheritdoc/>
        public bool IsSynthetic => Block.IsSynthetic;

        /// <inheritdoc/>
        public bool IsGenericBlock => Block.IsGenericBlock;

        /// <inheritdoc/>
        public IReadOnlyList<ITypeParameter> TypeParameters => typeParameters.Value;

        /// <inheritdoc/>
        public string? CodeReference => Block.BlockType is not null ? $"T:{Block.BlockType.FullName}" : null;

        /// <inheritdoc/>
        public bool Extends(IType type)
        {
            if (Receiver.Type.IsAssignableFrom(type))
                return true;

            if (IsGenericBlock && type is IGenericCapableType { IsGenericTypeDefinition: true })
                return Receiver.Type.IsSubstitutableBy(type);

            return false;
        }
    }
}
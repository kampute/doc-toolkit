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
        private readonly Lazy<IReadOnlyList<IProperty>> properties;
        private readonly Lazy<IReadOnlyList<IMethod>> methods;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionBlockAdapter"/> class.
        /// </summary>
        /// <param name="declaringType">The class type that declares the extension block.</param>
        /// <param name="extensionBlock">The extension block information to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="extensionBlock"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="extensionBlock"/> does not belong to the <paramref name="declaringType"/>.</exception>
        public ExtensionBlockAdapter(IClassType declaringType, ExtensionBlockInfo extensionBlock)
        {
            DeclaringType = declaringType ?? throw new ArgumentNullException(nameof(declaringType));
            Block = extensionBlock ?? throw new ArgumentNullException(nameof(extensionBlock));

            if (!DeclaringType.Represents(extensionBlock.BlockType.DeclaringType))
                throw new ArgumentException("The extension block does not belong to the declaring type.", nameof(extensionBlock));

            receiver = new(() => Assembly.Repository.GetParameterMetadata(Block.ReceiverParameter));
            typeParameters = new(() => IsGenericBlock ? [.. Block.TypeParameters.Select(MetadataProvider.GetMetadata<ITypeParameter>)] : []);
            properties = new(() => [.. Block.Properties.Select(Assembly.Repository.GetPropertyMetadata).OrderByName()]);
            methods = new(() => [.. Block.Methods.Select(Assembly.Repository.GetMethodMetadata<IMethod>).OrderByName()]);
        }

        /// <summary>
        /// Gets the underlying extension block information.
        /// </summary>
        /// <value>
        /// The underlying extension block information.
        /// </value>
        protected ExtensionBlockInfo Block { get; }

        /// <inheritdoc/>
        public IAssembly Assembly => DeclaringType.Assembly;

        /// <inheritdoc/>
        public IClassType DeclaringType { get; }

        /// <inheritdoc/>
        public IParameter Receiver => receiver.Value;

        /// <inheritdoc/>
        public bool IsGenericBlock => Block.BlockType.IsGenericTypeDefinition;

        /// <inheritdoc/>
        public IReadOnlyList<ITypeParameter> TypeParameters => typeParameters.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IProperty> Properties => properties.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IMethod> Methods => methods.Value;

        /// <inheritdoc/>
        public string CodeReference => $"T:{Block.BlockType.FullName}";

        /// <inheritdoc/>
        public bool Extends(IType type) => Receiver.Type.IsAssignableFrom(type);
    }
}
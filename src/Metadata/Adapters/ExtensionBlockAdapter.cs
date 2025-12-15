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
    public class ExtensionBlockAdapter : TypeMemberAdapter<ExtensionBlockInfo>, IExtensionBlock
    {
        private readonly Lazy<IParameter> receiver;
        private readonly Lazy<IReadOnlyList<ITypeParameter>> typeParameters;
        private readonly Lazy<IReadOnlyList<IProperty>> properties;
        private readonly Lazy<IReadOnlyList<IMethod>> methods;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionBlockAdapter"/> class.
        /// </summary>
        /// <param name="declaringType">The type that declares the extension block.</param>
        /// <param name="extensionBlock">The extension block information to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="extensionBlock"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="extensionBlock"/> does not belong to the <paramref name="declaringType"/>.</exception>
        public ExtensionBlockAdapter(IType declaringType, ExtensionBlockInfo extensionBlock)
            : base(declaringType, extensionBlock)
        {
            receiver = new(() => Assembly.Repository.GetParameterMetadata(Reflection.ReceiverParameter));
            typeParameters = new(() => IsGenericBlock ? [.. Reflection.TypeParameters.Select(MetadataProvider.GetMetadata<ITypeParameter>)] : []);
            properties = new(() => [.. Reflection.ExtensionProperties.Select(Assembly.Repository.GetPropertyMetadata).OrderByName()]);
            methods = new(() => [.. Reflection.ExtensionMethods.Select(Assembly.Repository.GetMethodMetadata<IMethod>).OrderByName()]);
        }

        /// <inheritdoc/>
        public IParameter Receiver => receiver.Value;

        /// <inheritdoc/>
        public override bool IsStatic => Reflection.BlockType.IsSealed && Reflection.BlockType.IsAbstract;

        /// <inheritdoc/>
        public override bool IsUnsafe => false;

        /// <inheritdoc/>
        public override bool IsSpecialName => Reflection.BlockType.IsSpecialName;

        /// <inheritdoc/>
        public bool IsGenericBlock => Reflection.BlockType.IsGenericTypeDefinition;

        /// <inheritdoc/>
        public IReadOnlyList<ITypeParameter> TypeParameters => typeParameters.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IProperty> Properties => properties.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IMethod> Methods => methods.Value;

        /// <inheritdoc/>
        public bool Extends(IType type) => Receiver.Type.IsAssignableFrom(type);

        /// <inheritdoc/>
        protected override MemberVisibility GetMemberVisibility() => MemberVisibility.Public;

        /// <inheritdoc/>
        protected override (char, string) GetCodeReferenceParts() => ('T', Reflection.BlockType.FullName!);
    }
}
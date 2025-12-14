// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Reflection
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Represents reflection information about an extension block.
    /// </summary>
    /// <remarks>
    /// This class encapsulates details about extension members that share a common receiver type.
    /// It provides information about the block's name, receiver parameter, and any generic type parameters.
    /// <para>
    /// For sake of monotony, this class is also used to represent classic (non-block) extension methods.
    /// In such cases, the <see cref="BlockType"/> will be <see langword="null"/>.
    /// </para>
    /// </remarks>
    public sealed class ExtensionBlockInfo : IEquatable<ExtensionBlockInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionBlockInfo"/> class for an explicit extension block.
        /// </summary>
        /// <param name="blockType">The type of the extension block.</param>
        /// <param name="receiver">The receiver parameter of the extension block.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockType"/> or <paramref name="receiver"/> is <see langword="null"/>.</exception>
        public ExtensionBlockInfo(Type blockType, ParameterInfo receiver)
        {
            BlockType = blockType ?? throw new ArgumentNullException(nameof(blockType));
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));

            if (blockType.IsGenericType)
                TypeParameters = receiver.Member.DeclaringType.GetGenericArguments();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionBlockInfo"/> class for a classic (non-block) extension method.
        /// </summary>
        /// <param name="receiver">The receiver parameter of the extension method.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="receiver"/> is <see langword="null"/>.</exception>
        public ExtensionBlockInfo(ParameterInfo receiver)
        {
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            if (receiver.ParameterType.IsGenericType)
                TypeParameters = receiver.ParameterType.GetGenericArguments();
        }

        /// <summary>
        /// Gets a value indicating whether the extension block is synthetic (i.e., represents a classic extension method).
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the extension block is synthetic; otherwise, <see langword="false"/>.
        /// </value>
        /// <remakrs>
        /// A synthetic extension block represents an aggregation of classic (non-block) extension methods that share the same 
        /// receiver type. Such blocks do not have an explicit block type, hence <see cref="BlockType"/> is <see langword="null"/>.
        /// </remakrs>
        public bool IsSynthetic => BlockType is null;

        /// <summary>
        /// Gets a value indicating whether the extension block is generic.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the extension block is generic; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsGenericBlock => TypeParameters.Length > 0;

        /// <summary>
        /// Gets the assembly that contains the extension block.
        /// </summary>
        /// <value>
        /// The assembly that contains the extension block.
        /// </value>
        public Assembly Assembly => Receiver.Member.Module.Assembly;

        /// <summary>
        /// Gets the type of the extension block, if applicable.
        /// </summary>
        /// <value>
        /// The type of the extension block, or <see langword="null"/> if the extension block represents a classic (non-block) 
        /// extension method.
        /// </value>
        public Type? BlockType { get; }

        /// <summary>
        /// Gets the receiver parameter of the extension block.
        /// </summary>
        /// <value>
        /// The receiver parameter of the extension block.
        /// </value>
        public ParameterInfo Receiver { get; }

        /// <summary>
        /// Gets the generic type parameters of the extension block, if any.
        /// </summary>
        /// <value>
        /// The generic type parameters of the extension block.
        /// </value>
        public Type[] TypeParameters { get; } = [];

        /// <summary>
        /// Determines whether the current instance is equal to the specified <see cref="ExtensionBlockInfo"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ExtensionBlockInfo"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ExtensionBlockInfo other) => other is not null && Receiver.Equals(other.Receiver);

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj) => obj is ExtensionBlockInfo other && Equals(other);

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode() => Receiver.GetHashCode();
    }
}

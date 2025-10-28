// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Collections;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// An adapter that wraps a <see cref="CustomAttributeData"/> and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="CustomAttributeData"/> and the metadata
    /// representation defined by the <see cref="ICustomAttribute"/> interface. It provides access to attribute-level
    /// information regardless of whether the assembly containing the attribute was loaded via Common Language
    /// Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class CustomAttributeAdapter : ICustomAttribute
    {
        /// <summary>
        /// The collection of patterns that identify attributes applied by the system/compiler.
        /// </summary>
        protected static readonly IReadOnlyPatternCollection SystemAttributePatterns = new PatternCollection('.')
        {
            "System.Runtime.*",
            "System.Reflection.*",
            "System.CodeDom.*",
            "System.ParamArrayAttribute",
            "System.Diagnostics.DebuggerHiddenAttribute",
            "System.Diagnostics.DebuggerNonUserCodeAttribute",
            "System.Diagnostics.DebuggerStepThroughAttribute",
        };

        private readonly Lazy<IClassType> attributeType;
        private readonly Lazy<IReadOnlyList<TypedValue>> constructorArguments;
        private readonly Lazy<IReadOnlyDictionary<string, TypedValue>> namedArguments;
        private bool? isImplicit;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomAttributeAdapter"/> class.
        /// </summary>
        /// <param name="attribute">The runtime custom attribute data to wrap.</param>
        /// <param name="target">The target where the attribute is applied.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="attribute"/> is <see langword="null"/>.</exception>
        public CustomAttributeAdapter(CustomAttributeData attribute, AttributeTarget target)
        {
            Native = attribute ?? throw new ArgumentNullException(nameof(attribute));
            Target = target;

            attributeType = new(Native.AttributeType.GetMetadata<IClassType>);

            constructorArguments = new(() => [.. Native.ConstructorArguments.Select(
                arg => new TypedValue(arg.ArgumentType.GetMetadata(), GetArgumentValue(arg)))]);

            namedArguments = new(() => Native.NamedArguments.ToDictionary(
                arg => arg.MemberName,
                arg => new TypedValue(arg.TypedValue.ArgumentType.GetMetadata(), GetArgumentValue(arg.TypedValue))));
        }

        /// <summary>
        /// Gets the underlying <see cref="CustomAttributeData"/> instance.
        /// </summary>
        /// <value>
        /// The <see cref="CustomAttributeData"/> instance wrapped by this adapter.
        /// </value>
        protected CustomAttributeData Native { get; }

        /// <inheritdoc/>
        public virtual IClassType Type => attributeType.Value;

        /// <inheritdoc/>
        public virtual IReadOnlyList<TypedValue> ConstructorArguments => constructorArguments.Value;

        /// <inheritdoc/>
        public virtual IReadOnlyDictionary<string, TypedValue> NamedArguments => namedArguments.Value;

        /// <inheritdoc/>
        public virtual bool IsImplicitlyApplied => isImplicit ??= SystemAttributePatterns.Matches(Native.AttributeType.FullName);

        /// <inheritdoc/>
        public AttributeTarget Target { get; }

        /// <inheritdoc/>
        public virtual bool Represents(CustomAttributeData reflection) => ReferenceEquals(Native, reflection);

        /// <summary>
        /// Gets the value of the specified argument, handling array arguments appropriately.
        /// </summary>
        /// <param name="typedArgument">The argument whose value is to be retrieved.</param>
        /// <returns>The value of the argument.</returns>
        protected virtual object? GetArgumentValue(CustomAttributeTypedArgument typedArgument)
        {
            if (typedArgument.Value is IReadOnlyCollection<CustomAttributeTypedArgument> args)
            {
                var elementType = typedArgument.ArgumentType.GetElementType() ?? typeof(object);
                var array = Array.CreateInstance(elementType, args.Count);

                var index = 0;
                foreach (var arg in args)
                    array.SetValue(GetArgumentValue(arg), index++);

                return array;
            }

            return typedArgument.Value;
        }
    }
}

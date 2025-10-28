// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// An adapter that wraps a reflection <see cref="Type"/> representing a delegate type and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="Type"/> and the metadata
    /// representation defined by the <see cref="IDelegateType"/> interface. It provides access to delegate-level
    /// information regardless of whether the assembly containing the delegate was loaded via Common Language
    /// Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class DelegateTypeAdapter : GenericCapableTypeAdapter, IDelegateType
    {
        private readonly Lazy<IMethod> invokeMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateTypeAdapter"/> class.
        /// </summary>
        /// <param name="declaringEntity">The assembly or type that declares the delegate type.</param>
        /// <param name="delegateType">The reflection information of the delegate type to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringEntity"/> or <paramref name="delegateType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is neither an <see cref="IAssembly"/> nor an <see cref="IType"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="delegateType"/> is not declared by the <paramref name="declaringEntity"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="delegateType"/> is a nested type but <paramref name="declaringEntity"/> is an assembly, or when <paramref name="delegateType"/> is a top-level type but <paramref name="declaringEntity"/> is a type.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="delegateType"/> is not a delegate.</exception>
        public DelegateTypeAdapter(object declaringEntity, Type delegateType)
            : base(declaringEntity, delegateType)
        {
            var invoker = GetInvokeMethod() ?? throw new ArgumentException("Type must be a delegate.", nameof(delegateType));
            invokeMethod = new(() => (IMethod)Assembly.Repository.GetMethodMetadata(invoker));
        }

        /// <inheritdoc/>
        public IMethod InvokeMethod => invokeMethod.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IParameter> Parameters => InvokeMethod.Parameters;

        /// <inheritdoc/>
        public IParameter Return => InvokeMethod.Return;

        /// <inheritdoc/>
        public override bool IsUnsafe => InvokeMethod.IsUnsafe;

        /// <summary>
        /// Retrieves the invoke method of the delegate type.
        /// </summary>
        /// <returns>The <see cref="MethodInfo"/> representing the invoke method, or <see langword="null"/> if not found.</returns>
        protected virtual MethodInfo? GetInvokeMethod() => Reflection.GetMethod("Invoke");
    }
}

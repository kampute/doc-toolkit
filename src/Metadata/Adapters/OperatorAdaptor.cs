// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// An adapter that wraps a <see cref="MethodInfo"/> representing an operator overload and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="MethodInfo"/> and the metadata
    /// representation defined by the <see cref="IOperator"/> interface. It provides access to operator-level
    /// information regardless of whether the assembly containing the operator's type was loaded via Common
    /// Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class OperatorAdapter : TypeMemberAdapter<MethodInfo>, IOperator
    {
        private readonly Lazy<IReadOnlyList<IParameter>> parameters;
        private readonly Lazy<IParameter> returnParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorAdapter"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the operator method.</param>
        /// <param name="method">The operator method to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="method"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="method"/> is not an operator method of <paramref name="declaringType"/>.</exception>
        public OperatorAdapter(IType declaringType, MethodInfo method)
            : base(declaringType, method)
        {
            Name = method.Name[3..];
            parameters = new(() => [.. GetParameters()]);
            returnParameter = new(GetReturnParameter);
        }

        /// <inheritdoc/>
        public override string Name { get; }

        /// <inheritdoc/>
        public IReadOnlyList<IParameter> Parameters => parameters.Value;

        /// <inheritdoc/>
        public IParameter Return => returnParameter.Value;

        /// <inheritdoc/>
        public override bool IsSpecialName => true;

        /// <inheritdoc/>
        public override bool IsStatic => true;

        /// <inheritdoc/>
        public virtual bool IsConversionOperator => Name is "Implicit" or "Explicit";

        /// <inheritdoc/>
        public override bool IsUnsafe => Return.Type.IsUnsafe || Parameters.Any(static p => p.Type.IsUnsafe);

        /// <inheritdoc/>
        public virtual IEnumerable<IMember> Overloads => ((IWithOperators)DeclaringType).Operators.Where(m => m.Name == Name && !ReferenceEquals(m, this));

        /// <inheritdoc/>
        protected override MemberVisibility GetMemberVisibility()
        {
            if (Reflection.IsPublic)
                return MemberVisibility.Public;
            else if (Reflection.IsFamily)
                return MemberVisibility.Protected;
            else if (Reflection.IsAssembly)
                return MemberVisibility.Internal;
            else if (Reflection.IsFamilyAndAssembly)
                return MemberVisibility.PrivateProtected;
            else if (Reflection.IsFamilyOrAssembly)
                return MemberVisibility.ProtectedInternal;
            else
                return MemberVisibility.Private;
        }

        /// <inheritdoc/>
        protected sealed override (char, string) GetCodeReferenceParts()
        {
            var signature = $"op_{Name}({string.Join(',', Parameters.Select(p => p.Type.ParametericSignature))})";
            if (IsConversionOperator)
                signature = $"{signature}~{Return.Type.ParametericSignature}";

            return ('M', signature);
        }

        /// <summary>
        /// Retrieves the parameters of the operator overload.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IParameter"/> objects representing the parameters of the operator overload.</returns>
        protected virtual IEnumerable<IParameter> GetParameters() => Reflection.GetParameters().Select(Assembly.Repository.GetParameterMetadata);

        /// <summary>
        /// Retrieves the return parameter of the operator overload.
        /// </summary>
        /// <returns>An <see cref="IParameter"/> object representing the return parameter of the operator overload.</returns>
        protected virtual IParameter GetReturnParameter() => Assembly.Repository.GetParameterMetadata(Reflection.ReturnParameter);
    }
}

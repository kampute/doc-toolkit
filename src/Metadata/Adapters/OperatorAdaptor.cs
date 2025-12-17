// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Capabilities;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

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
    public class OperatorAdapter : MethodBaseAdapter, IOperator
    {
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
            var i = method.Name.IndexOf("op_", StringComparison.Ordinal);
            if (i == 0)
                Name = method.Name[3..]; // Standard operator overload
            else if (i > 0)
                Name = method.Name.Remove(i, 3); // Explicit interface implementation of operator overload
            else
                throw new ArgumentException("The provided method is not an operator overload.", nameof(method));
        }

        /// <inheritdoc/>
        public override string Name { get; }

        /// <inheritdoc/>
        public string MethodName => Reflection.Name;

        /// <inheritdoc/>
        public virtual bool IsConversionOperator => Name is "Implicit" or "Explicit"; // Note: Conversion operators are not supported in interfaces

        /// <inheritdoc/>
        public override bool IsUnsafe => Return.Type.IsUnsafe || Parameters.Any(static p => p.Type.IsUnsafe);

        /// <inheritdoc/>
        public IOperator? OverriddenOperator => (IOperator?)OverriddenMember;

        /// <inheritdoc/>
        public IOperator? ImplementedOperator => (IOperator?)ImplementedMember;

        /// <inheritdoc/>
        public override IEnumerable<IMember> Overloads => GetOperatorsWithSameName(DeclaringType, preserveOrder: true).Where(m => !ReferenceEquals(m, this));

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindImplementedMember()
        {
            if (IsPublic)
            {
                return ((IInterfaceCapableType)DeclaringType).Interfaces
                    .SelectMany(i => i.Operators.WhereName(Name))
                    .FirstOrDefault(HasMatchingSignature);
            }

            if (IsExplicitInterfaceImplementation)
            {
                var (interfaceFullName, memberName) = AdapterHelper.SplitExplicitName(Name);

                return ((IInterfaceCapableType)DeclaringType)
                    .Interfaces.FindByFullName(interfaceFullName)?
                    .Operators.WhereName(memberName).FirstOrDefault(HasMatchingSignature);
            }

            return null;
        }

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindGenericDefinition()
        {
            return DeclaringType is IGenericCapableType { IsConstructedGenericType: true, GenericTypeDefinition: IType genericType }
                ? GetOperatorsWithSameName(genericType).FirstOrDefault(HasMatchingSignature)
                : (IVirtualTypeMember?)null;
        }

        /// <inheritdoc/>
        protected sealed override (char, string) GetCodeReferenceParts()
        {
            using var reusable = StringBuilderPool.Shared.GetBuilder();
            var sb = reusable.Builder;

            sb.Append(MethodName);

            if (MethodName.Contains('.'))
                sb.Replace('.', '#').Replace('<', '{').Replace('>', '}');

            if (Parameters.Count > 0)
            {
                sb.Append('(');
                sb.AppendJoin(',', Parameters.Select(static p => p.Type.ParametricSignature));
                sb.Append(')');
            }

            if (IsConversionOperator)
            {
                sb.Append('~');
                sb.Append(Return.Type.ParametricSignature);
            }

            return ('M', sb.ToString());
        }

        /// <summary>
        /// Retrieves operators from the specified type that have the same name as this operator.
        /// </summary>
        /// <param name="type">The type to search for similar property names.</param>
        /// <param name="preserveOrder">Indicates whether to preserve the order of operators as they appear in the type.</param>
        /// <returns>An enumerable collection of <see cref="IOperator"/> objects with same name as this operator.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IEnumerable<IOperator> GetOperatorsWithSameName(IType type, bool preserveOrder = false)
        {
            return IsExplicitInterfaceImplementation
                ? type is IWithExplicitInterfaceOperators withExplicitOperators
                    ? withExplicitOperators.ExplicitInterfaceOperators.WhereName(Name, preserveOrder)
                    : []
                : type is IWithOperators withOperators
                    ? withOperators.Operators.WhereName(Name, preserveOrder)
                    : [];
        }
    }
}
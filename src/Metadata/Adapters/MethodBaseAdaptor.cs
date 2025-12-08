// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// An abstract adapter that wraps a <see cref="MethodInfo"/> and provides metadata access for methods
    /// and operators.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="MethodInfo"/> and the metadata
    /// representation defined by the <see cref="IMethod"/> interface. It provides access to method-level
    /// information regardless of whether the assembly containing the method's type was loaded via Common
    /// Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public abstract class MethodBaseAdapter : VirtualTypeMemberAdapter<MethodInfo>, IMethodBase
    {
        private readonly Lazy<IReadOnlyList<IParameter>> parameters;
        private readonly Lazy<IParameter> returnParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodAdapter"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the method.</param>
        /// <param name="method">The method to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="method"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="method"/> is not a method of <paramref name="declaringType"/>.</exception>
        public MethodBaseAdapter(IType declaringType, MethodInfo method)
            : base(declaringType, method)
        {
            parameters = new(() => [.. GetParameters()]);
            returnParameter = new(GetReturnParameter);
        }

        /// <inheritdoc/>
        public IReadOnlyList<IParameter> Parameters => parameters.Value;

        /// <inheritdoc/>
        public IParameter Return => returnParameter.Value;

        /// <inheritdoc/>
        public override bool IsSpecialName => Reflection.IsSpecialName;

        /// <inheritdoc/>
        public override bool IsStatic => Reflection.IsStatic;

        /// <inheritdoc/>
        public override bool IsUnsafe => Return.Type.IsUnsafe || Parameters.Any(static p => p.Type.IsUnsafe);

        /// <inheritdoc/>
        public virtual bool IsReadOnly => HasCustomAttribute(AttributeNames.IsReadOnly);

        /// <inheritdoc/>
        public abstract IEnumerable<IMember> Overloads { get; }

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
        protected override MemberVirtuality GetMemberVirtuality()
        {
            if (!Reflection.IsVirtual)
                return MemberVirtuality.None;
            else if (Reflection.IsAbstract)
                return MemberVirtuality.Abstract;
            else if (Reflection.IsFinal)
                return OverriddenMember is not null ? MemberVirtuality.SealedOverride : MemberVirtuality.None;
            else
                return OverriddenMember is not null ? MemberVirtuality.Override : MemberVirtuality.Virtual;
        }

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindOverriddenMember()
        {
            if (!Reflection.IsVirtual)
                return null;

            var methodName = Reflection.Name;
            var searchFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic
                            | (IsStatic ? BindingFlags.Static : BindingFlags.Instance);

            for (var baseType = Reflection.DeclaringType.BaseType; baseType is not null; baseType = baseType.BaseType)
            {
                if (baseType.IsConstructedGenericType)
                    baseType = baseType.GetGenericTypeDefinition();

                foreach (var member in baseType.GetMember(methodName, searchFlags))
                {
                    if (member is not MethodInfo method || method.IsFinal || method.IsPrivate || method.IsFamilyAndAssembly)
                        continue;

                    var candidate = MetadataProvider.GetMetadata(method) as IMethod;
                    if (HasMatchingSignature(candidate))
                        return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether the given method can be considered a base declaration of this method.
        /// </summary>
        /// <param name="baseCandidate">The other method to compare against.</param>
        /// <returns><see langword="true"/> if the given method can be considered a base declaration; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method compares only the signature of the methods. It assumes that the caller has already verified other necessary conditions,
        /// such as matching names and accessibility.
        /// </remarks>
        protected virtual bool HasMatchingSignature(IMethodBase? baseCandidate)
        {
            return baseCandidate is not null
                && baseCandidate.IsStatic == IsStatic
                && baseCandidate.Parameters.Count == Parameters.Count
                && baseCandidate.Return.IsSatisfiableBy(Return)
                && AdapterHelper.EquivalentParameters(baseCandidate.Parameters, Parameters);
        }

        /// <summary>
        /// Retrieves the parameters of the method.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IParameter"/> objects representing the parameters of the method.</returns>
        protected virtual IEnumerable<IParameter> GetParameters() => Reflection.GetParameters().Select(Assembly.Repository.GetParameterMetadata);

        /// <summary>
        /// Retrieves the return parameter of the method.
        /// </summary>
        /// <returns>An <see cref="IParameter"/> object representing the return parameter of the method.</returns>
        protected virtual IParameter GetReturnParameter() => Assembly.Repository.GetParameterMetadata(Reflection.ReturnParameter);
    }
}

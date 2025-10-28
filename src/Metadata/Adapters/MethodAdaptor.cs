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
    /// An adapter that wraps a <see cref="MethodInfo"/> and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="MethodInfo"/> and the metadata
    /// representation defined by the <see cref="IMethod"/> interface. It provides access to method-level
    /// information regardless of whether the assembly containing the method's type was loaded via Common
    /// Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class MethodAdapter : VirtualTypeMemberAdapter<MethodInfo>, IMethod
    {
        private readonly Lazy<IReadOnlyList<ITypeParameter>> typeParameters;
        private readonly Lazy<IReadOnlyList<IParameter>> parameters;
        private readonly Lazy<IParameter> returnParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodAdapter"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the method.</param>
        /// <param name="method">The method to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="method"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="method"/> is not a method of <paramref name="declaringType"/>.</exception>
        public MethodAdapter(IType declaringType, MethodInfo method)
            : base(declaringType, method)
        {
            typeParameters = new(() => [.. GetTypeParameters()]);
            parameters = new(() => [.. GetParameters()]);
            returnParameter = new(GetReturnParameter);
        }

        /// <inheritdoc/>
        public IReadOnlyList<ITypeParameter> TypeParameters => typeParameters.Value;

        /// <inheritdoc/>
        public IReadOnlyList<IParameter> Parameters => parameters.Value;

        /// <inheritdoc/>
        public IParameter Return => returnParameter.Value;

        /// <inheritdoc/>
        public override bool IsSpecialName => Reflection.IsSpecialName;

        /// <inheritdoc/>
        public override bool IsStatic => Reflection.IsStatic;

        /// <inheritdoc/>
        public virtual bool IsGenericMethod => Reflection.IsGenericMethod;

        /// <inheritdoc/>
        public virtual bool IsExtension => IsStatic && HasCustomAttribute("System.Runtime.CompilerServices.ExtensionAttribute");

        /// <inheritdoc/>
        public virtual bool IsAsync => HasCustomAttribute("System.Runtime.CompilerServices.AsyncStateMachineAttribute");

        /// <inheritdoc/>
        public virtual bool IsReadOnly => HasCustomAttribute("System.Runtime.CompilerServices.IsReadOnlyAttribute");

        /// <inheritdoc/>
        public override bool IsUnsafe => Return.Type.IsUnsafe || Parameters.Any(static p => p.Type.IsUnsafe);

        /// <inheritdoc/>
        public IMethod? OverriddenMethod => (IMethod?)OverriddenMember;

        /// <inheritdoc/>
        public IMethod? ImplementedMethod => (IMethod?)ImplementedMember;

        /// <inheritdoc/>
        public virtual IEnumerable<IMember> Overloads => ((IWithMethods)DeclaringType)
            .Methods.WhereName(Name, preserveOrder: true).Where(m => !ReferenceEquals(m, this));

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
        protected sealed override (char, string) GetCodeReferenceParts()
        {
            var signature = Name;
            if (signature.Contains('.'))
                signature = signature.Replace('.', '#').Replace('<', '{').Replace('>', '}');
            if (IsGenericMethod && TypeParameters.Count > 0)
                signature += $"``{TypeParameters.Count}";
            if (Parameters.Count > 0)
                signature += $"({string.Join(',', Parameters.Select(p => p.Type.Signature))})";

            return ('M', signature);
        }

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindOverriddenMember()
        {
            if (IsStatic || IsExplicitInterfaceImplementation || !Reflection.IsVirtual)
                return null;

            for (var baseType = Reflection.DeclaringType.BaseType; baseType is not null; baseType = baseType.BaseType)
            {
                if (baseType.IsConstructedGenericType)
                    baseType = baseType.GetGenericTypeDefinition();

                foreach (var member in baseType.GetMember(Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
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

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindImplementedMember()
        {
            if (IsPublic && !IsStatic)
            {
                return ((IInterfaceCapableType)DeclaringType).Interfaces
                    .SelectMany(i => i.Methods.WhereName(Name))
                    .FirstOrDefault(HasMatchingSignature);
            }

            if (IsExplicitInterfaceImplementation)
            {
                var (interfaceFullName, memberName) = AdapterHelper.DecodeExplicitName(Name);

                return ((IInterfaceCapableType)DeclaringType)
                    .Interfaces.FindByFullName(interfaceFullName)?
                    .Methods.WhereName(memberName).FirstOrDefault(HasMatchingSignature);
            }

            return null;
        }

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindGenericDefinition()
        {
            return DeclaringType is IGenericCapableType { IsConstructedGenericType: true, GenericTypeDefinition: IWithMethods typeDef }
                ? typeDef.Methods.WhereName(Name).FirstOrDefault(HasMatchingSignature)
                : (IVirtualTypeMember?)null;
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
        protected virtual bool HasMatchingSignature(IMethod? baseCandidate)
        {
            return baseCandidate is not null
                && baseCandidate.Parameters.Count == Parameters.Count
                && baseCandidate.TypeParameters.Count == TypeParameters.Count
                && baseCandidate.Return.IsSatisfiableBy(Return)
                && AdapterHelper.AreParameterSignaturesMatching(baseCandidate.Parameters, Parameters);
        }

        /// <summary>
        /// Retrieves the type parameters defined by the method.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="ITypeParameter"/> objects representing the type parameters defined by the method.</returns>
        protected virtual IEnumerable<ITypeParameter> GetTypeParameters() => Reflection.GetGenericArguments().Select(Assembly.Repository.GetTypeMetadata<ITypeParameter>);

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

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Capabilities;
    using Kampute.DocToolkit.Metadata.Reflection;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

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
    public class MethodAdapter : MethodBaseAdapter, IMethod
    {
        private readonly Lazy<IReadOnlyList<ITypeParameter>> typeParameters;
        private readonly Lazy<IExtensionBlock?> extensionBlock;

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
            extensionBlock = new(GetExtensionBlock);
            IsClassicExtensionMethod = IsStatic && HasCustomAttribute(AttributeNames.Extension);
        }

        /// <inheritdoc/>
        public IReadOnlyList<ITypeParameter> TypeParameters => typeParameters.Value;

        /// <inheritdoc/>
        public virtual bool IsGenericMethod => Reflection.IsGenericMethod;

        /// <inheritdoc/>
        public virtual bool IsExtension => IsClassicExtensionMethod || Reflection is IExtensionBlockMethodInfo;

        /// <inheritdoc/>
        public virtual bool IsClassicExtensionMethod { get; }

        /// <inheritdoc/>
        public virtual IParameter? ExtensionReceiver => IsClassicExtensionMethod ? Parameters[0] : ExtensionBlock?.Receiver;

        /// <inheritdoc/>
        public IExtensionBlock? ExtensionBlock => extensionBlock.Value;

        /// <inheritdoc/>
        public IMethod? OverriddenMethod => (IMethod?)OverriddenMember;

        /// <inheritdoc/>
        public IMethod? ImplementedMethod => (IMethod?)ImplementedMember;

        /// <inheritdoc/>
        public override IEnumerable<IMember> Overloads
            => GetMethodsWithSameName(DeclaringType, preserveOrder: true).Where(m => !ReferenceEquals(m, this));

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindImplementedMember()
        {
            if (IsPublic)
            {
                return ((IInterfaceCapableType)DeclaringType).Interfaces
                    .SelectMany(i => i.Methods.WhereName(Name))
                    .FirstOrDefault(HasMatchingSignature);
            }

            if (IsExplicitInterfaceImplementation)
            {
                var (interfaceFullName, memberName) = AdapterHelper.SplitExplicitName(Name);

                return ((IInterfaceCapableType)DeclaringType)
                    .Interfaces.FindByFullName(interfaceFullName)?
                    .Methods.WhereName(memberName).FirstOrDefault(HasMatchingSignature);
            }

            return null;
        }

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindGenericDefinition()
        {
            return DeclaringType is IGenericCapableType { IsConstructedGenericType: true, GenericTypeDefinition: IType genericType }
                ? GetMethodsWithSameName(genericType).FirstOrDefault(HasMatchingSignature)
                : (IVirtualTypeMember?)null;
        }

        /// <inheritdoc/>
        protected sealed override (char, string) GetCodeReferenceParts()
        {
            using var reusable = StringBuilderPool.Shared.GetBuilder();
            var sb = reusable.Builder;

            sb.Append(Name);

            if (Name.Contains('.'))
                sb.Replace('.', '#').Replace('<', '{').Replace('>', '}');

            if (IsGenericMethod && TypeParameters.Count > 0)
                sb.Append("``").Append(TypeParameters.Count);

            if (Parameters.Count > 0)
            {
                sb.Append('(');
                sb.AppendJoin(',', Parameters.Select(p => p.Type.ParametricSignature));
                sb.Append(')');
            }

            return ('M', sb.ToString());
        }

        /// <inheritdoc/>
        protected override string ConstructCodeReference()
        {
            return Reflection is IExtensionBlockMethodInfo extension
                ? Assembly.Repository.GetMethodMetadata(extension.DeclaredMethod, asDeclared: true).CodeReference
                : base.ConstructCodeReference();
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
        protected override bool HasMatchingSignature(IMethodBase? baseCandidate)
        {
            return baseCandidate is IMethod baseCandidateMethod
                && baseCandidateMethod.IsGenericMethod == IsGenericMethod
                && baseCandidateMethod.TypeParameters.Count == TypeParameters.Count
                && base.HasMatchingSignature(baseCandidate);
        }

        /// <summary>
        /// Retrieves the extension block associated with the method, if it is an extension method.
        /// </summary>
        /// <returns>An <see cref="IExtensionBlock"/> representing the extension block, or <see langword="null"/> if the method is not an extension method.</returns>
        protected virtual IExtensionBlock? GetExtensionBlock() => Reflection is IExtensionBlockMemberInfo extensionMember
            ? Assembly.Repository.GetExtensionBlockMetadata(extensionMember.DeclaringBlock)
            : null;

        /// <summary>
        /// Retrieves the type parameters defined by the method.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="ITypeParameter"/> objects representing the type parameters defined by the method.</returns>
        protected virtual IEnumerable<ITypeParameter> GetTypeParameters() => Reflection.GetGenericArguments().Select(Assembly.Repository.GetTypeMetadata<ITypeParameter>);

        /// <summary>
        /// Retrieves methods from the specified type that have the same name as this method.
        /// </summary>
        /// <param name="type">The type to search for similar property names.</param>
        /// <param name="preserveOrder">Indicates whether to preserve the order of methods as they appear in the type.</param>
        /// <returns>An enumerable collection of <see cref="IMethod"/> objects with same name as this method.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IEnumerable<IMethod> GetMethodsWithSameName(IType type, bool preserveOrder = false)
        {
            return IsExplicitInterfaceImplementation
                ? type is IWithExplicitInterfaceMethods withExplicitMethods
                    ? withExplicitMethods.ExplicitInterfaceMethods.WhereName(Name, preserveOrder)
                    : []
                : type is IWithMethods withMethods
                    ? withMethods.Methods.WhereName(Name, preserveOrder)
                    : [];
        }
    }
}

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
    /// An adapter that wraps a <see cref="PropertyInfo"/> and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="PropertyInfo"/> and the metadata
    /// representation defined by the <see cref="IProperty"/> interface. It provides access to property-level
    /// information regardless of whether the assembly containing the property's type was loaded via Common
    /// Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class PropertyAdapter : VirtualTypeMemberAdapter<PropertyInfo>, IProperty
    {
        private readonly Lazy<IReadOnlyList<IParameter>> indexParameters;
        private readonly Lazy<IType> propertyType;
        private readonly Lazy<IMethod?> getMethod;
        private readonly Lazy<IMethod?> setMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAdapter"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the property.</param>
        /// <param name="property">The property to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="property"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="property"/> is not a property of <paramref name="declaringType"/>.</exception>
        public PropertyAdapter(IType declaringType, PropertyInfo property)
            : base(declaringType, property)
        {
            propertyType = new(GetPropertyType);
            getMethod = new(GetGetterMethod);
            setMethod = new(GetSetterMethod);
            indexParameters = new(() => [.. GetIndexParameters()]);
        }

        /// <inheritdoc/>
        public virtual IType Type => propertyType.Value;

        /// <inheritdoc/>
        public override bool IsSpecialName => Reflection.IsSpecialName;

        /// <inheritdoc/>
        public override bool IsStatic => GetAccessors().Any(static m => m.IsStatic);

        /// <inheritdoc/>
        public override bool IsUnsafe => GetAccessors().Any(static m => m.IsUnsafe);

        /// <inheritdoc/>
        public virtual bool IsReadOnly => GetAccessors().Any(static m => m.IsReadOnly);

        /// <inheritdoc/>
        public virtual bool IsInitOnly => SetMethod?.Return.HasRequiredCustomModifier("System.Runtime.CompilerServices.IsExternalInit") == true;

        /// <inheritdoc/>
        public virtual bool IsRequired => HasCustomAttribute("System.Runtime.CompilerServices.RequiredMemberAttribute");

        /// <inheritdoc/>
        public virtual bool IsIndexer => Reflection.GetIndexParameters().Length > 0;

        /// <inheritdoc/>
        public virtual bool CanRead => Reflection.CanRead;

        /// <inheritdoc/>
        public virtual bool CanWrite => Reflection.CanWrite;

        /// <inheritdoc/>
        public IReadOnlyList<IParameter> Parameters => indexParameters.Value;

        /// <inheritdoc/>
        public virtual IMethod? GetMethod => getMethod.Value;

        /// <inheritdoc/>
        public IMethod? SetMethod => setMethod.Value;

        /// <inheritdoc/>
        public IProperty? OverriddenProperty => (IProperty?)OverriddenMember;

        /// <inheritdoc/>
        public IProperty? ImplementedProperty => (IProperty?)ImplementedMember;

        /// <inheritdoc/>
        public virtual IEnumerable<IMember> Overloads => !IsIndexer ? [] : ((IWithProperties)DeclaringType)
            .Properties.WhereName(Name, preserveOrder: true).Where(p => !ReferenceEquals(p, this));

        /// <inheritdoc/>
        public virtual bool HasRequiredCustomModifier(string modifierFullName)
        {
            foreach (var modifier in Reflection.GetRequiredCustomModifiers())
            {
                if (modifier.FullName == modifierFullName)
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual bool HasOptionalCustomModifier(string modifierFullName)
        {
            foreach (var modifier in Reflection.GetOptionalCustomModifiers())
            {
                if (modifier.FullName == modifierFullName)
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<IMethod> GetAccessors()
        {
            if (GetMethod is not null)
                yield return GetMethod;
            if (SetMethod is not null)
                yield return SetMethod;
        }

        /// <inheritdoc/>
        protected override MemberVisibility GetMemberVisibility()
        {
            var visibility = MemberVisibility.Private;
            foreach (var accessor in GetAccessors())
            {
                if (accessor.Visibility > visibility)
                    visibility = accessor.Visibility;
            }
            return visibility;
        }

        /// <inheritdoc/>
        protected override MemberVirtuality GetMemberVirtuality()
        {
            var virtuality = MemberVirtuality.None;
            foreach (var accessor in GetAccessors())
            {
                if (accessor.Virtuality > virtuality)
                    virtuality = accessor.Virtuality;
            }
            return virtuality;
        }

        /// <inheritdoc/>
        protected sealed override (char, string) GetCodeReferenceParts()
        {
            var signature = Name;
            if (signature.Contains('.'))
                signature = signature.Replace('.', '#').Replace('<', '{').Replace('>', '}');
            if (IsIndexer)
                signature += $"({string.Join(',', Parameters.Select(p => p.Type.ParametericSignature))})";

            return ('P', signature);
        }

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindOverriddenMember()
        {
            if (IsStatic || IsExplicitInterfaceImplementation || Virtuality == MemberVirtuality.None)
                return null;

            for (var baseType = DeclaringType.BaseType; baseType is not null; baseType = baseType.BaseType)
            {
                if (baseType.IsConstructedGenericType)
                    baseType = (IClassType)baseType.GenericTypeDefinition!;

                foreach (var candidate in baseType.Properties.WhereName(Name))
                {
                    if (!candidate.IsStatic && candidate.IsOverridable && HasMatchingSignature(candidate))
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
                    .SelectMany(i => i.Properties.WhereName(Name))
                    .FirstOrDefault(HasMatchingSignature);
            }

            if (IsExplicitInterfaceImplementation)
            {
                var (interfaceFullName, memberName) = AdapterHelper.DecodeExplicitName(Name);

                return ((IInterfaceCapableType)DeclaringType)
                    .Interfaces.FindByFullName(interfaceFullName)?
                    .Properties.WhereName(memberName).FirstOrDefault(HasMatchingSignature);
            }

            return null;
        }

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindGenericDefinition()
        {
            return DeclaringType is IGenericCapableType { IsConstructedGenericType: true, GenericTypeDefinition: IWithProperties typeDef }
                ? typeDef.Properties.WhereName(Name).FirstOrDefault(HasMatchingSignature)
                : (IVirtualTypeMember?)null;
        }

        /// <summary>
        /// Determines whether the given property can be considered a base declaration of this property.
        /// </summary>
        /// <param name="baseCandidate">The other property to compare against.</param>
        /// <returns><see langword="true"/> if the given property can be considered a base declaration; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method checks whether the candidate property's type is substitutable by this property's type, and whether their parameter signatures
        /// match (if applicable). It assumes that the caller has already verified other necessary conditions, such as matching names.
        /// </remarks>
        protected virtual bool HasMatchingSignature(IProperty? baseCandidate)
        {
            return baseCandidate is not null
                && baseCandidate.Type.IsSubstitutableBy(Type)
                && (!IsIndexer || AdapterHelper.AreParameterSignaturesMatching(baseCandidate.Parameters, Parameters));
        }

        /// <summary>
        /// Retrieves the type of the property.
        /// </summary>
        /// <returns>An <see cref="IType"/> representing the type of the property.</returns>
        protected virtual IType GetPropertyType() => MetadataProvider.GetMetadata(Reflection.PropertyType);

        /// <summary>
        /// Retrieves the getter method of the property, if it exists.
        /// </summary>
        /// <returns>An <see cref="IMethod"/> representing the getter method, or <see langword="null"/> if the property does not have a getter.</returns>
        protected virtual IMethod? GetGetterMethod() => Reflection.GetMethod is MethodInfo getter ? (IMethod)Assembly.Repository.GetMethodMetadata(getter) : null;

        /// <summary>
        /// Retrieves the setter method of the property, if it exists.
        /// </summary>
        /// <returns>An <see cref="IMethod"/> representing the setter method, or <see langword="null"/> if the property does not have a setter.</returns>
        protected virtual IMethod? GetSetterMethod() => Reflection.SetMethod is MethodInfo setter ? (IMethod)Assembly.Repository.GetMethodMetadata(setter) : null;

        /// <summary>
        /// Retrieves the index parameters of the property.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IParameter"/> objects representing the index parameters of the property.</returns>
        protected virtual IEnumerable<IParameter> GetIndexParameters() => Reflection.GetIndexParameters().Select(Assembly.Repository.GetParameterMetadata);
    }
}

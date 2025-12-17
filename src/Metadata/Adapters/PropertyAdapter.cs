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
        private readonly Lazy<IExtensionBlock?> extensionBlock;

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
            extensionBlock = new(GetExtensionBlock);
        }

        /// <inheritdoc/>
        public virtual IType Type => propertyType.Value;

        /// <inheritdoc/>
        public override bool IsSpecialName => Reflection.IsSpecialName;

        /// <inheritdoc/>
        public override bool IsStatic => AnyAccessor.IsStatic;

        /// <inheritdoc/>
        public override bool IsUnsafe => AnyAccessor.IsUnsafe;

        /// <inheritdoc/>
        public virtual bool IsReadOnly => AnyAccessor.IsReadOnly;

        /// <inheritdoc/>
        public virtual bool IsInitOnly => SetMethod?.Return.HasRequiredCustomModifier(ModifierNames.IsExternalInit) is true;

        /// <inheritdoc/>
        public virtual bool IsRequired => HasCustomAttribute(AttributeNames.RequiredMember);

        /// <inheritdoc/>
        public virtual bool IsIndexer => Reflection.GetIndexParameters().Length > 0;

        /// <inheritdoc/>
        public virtual bool CanRead => Reflection.CanRead;

        /// <inheritdoc/>
        public virtual bool CanWrite => Reflection.CanWrite;

        /// <inheritdoc/>
        public IReadOnlyList<IParameter> Parameters => indexParameters.Value;

        /// <inheritdoc/>
        public IExtensionBlock? ExtensionBlock => extensionBlock.Value;

        /// <inheritdoc/>
        public virtual IMethod? GetMethod => getMethod.Value;

        /// <inheritdoc/>
        public IMethod? SetMethod => setMethod.Value;

        /// <inheritdoc/>
        public IProperty? OverriddenProperty => (IProperty?)OverriddenMember;

        /// <inheritdoc/>
        public IProperty? ImplementedProperty => (IProperty?)ImplementedMember;

        /// <inheritdoc/>
        public virtual IEnumerable<IMember> Overloads
            => IsIndexer ? GetPropertiesWithSameName(DeclaringType, preserveOrder: true).Where(p => !ReferenceEquals(p, this)) : [];

        /// <summary>
        /// Gets either the getter or setter method of the property, whichever is available.
        /// </summary>
        /// <value>
        /// The getter method if it exists; otherwise, the setter method.
        /// </value>
        protected IMethod AnyAccessor => GetMethod ?? SetMethod!;

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
            if (!IsIndexer && !Name.Contains('.'))
                return ('P', Name);

            using var reusable = StringBuilderPool.Shared.GetBuilder();
            var sb = reusable.Builder;

            sb.Append(Name);

            if (Name.Contains('.'))
                sb.Replace('.', '#').Replace('<', '{').Replace('>', '}');

            if (IsIndexer)
            {
                sb.Append('(');
                sb.AppendJoin(',', Parameters.Select(static p => p.Type.ParametricSignature));
                sb.Append(')');
            }

            return ('P', sb.ToString());
        }

        /// <inheritdoc/>
        protected override string ConstructCodeReference()
        {
            return Reflection is IExtensionBlockPropertyInfo
                ? AnyAccessor.CodeReference
                : base.ConstructCodeReference();
        }

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindOverriddenMember()
        {
            if (Virtuality == MemberVirtuality.None)
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
            if (IsPublic)
            {
                return ((IInterfaceCapableType)DeclaringType).Interfaces
                    .SelectMany(i => i.Properties.WhereName(Name))
                    .FirstOrDefault(HasMatchingSignature);
            }

            if (IsExplicitInterfaceImplementation)
            {
                var (interfaceFullName, memberName) = AdapterHelper.SplitExplicitName(Name);

                return ((IInterfaceCapableType)DeclaringType)
                    .Interfaces.FindByFullName(interfaceFullName)?
                    .Properties.WhereName(memberName).FirstOrDefault(HasMatchingSignature);
            }

            return null;
        }

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindGenericDefinition()
        {
            return DeclaringType is IGenericCapableType { IsConstructedGenericType: true, GenericTypeDefinition: IType genericType }
                ? GetPropertiesWithSameName(genericType).FirstOrDefault(HasMatchingSignature)
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
                && baseCandidate.IsStatic == IsStatic
                && TypesAreCompatible(baseCandidate.Type, Type)
                && (!IsIndexer || AdapterHelper.EquivalentParameters(baseCandidate.Parameters, Parameters));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool TypesAreCompatible(IType expectedType, IType actualType)
            {
                return expectedType is ITypeParameter { IsGenericTypeParameter: true } typeParameter
                    ? typeParameter.IsSatisfiableBy(actualType)
                    : expectedType.Equals(actualType);
            }
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
        protected virtual IMethod? GetGetterMethod() => Reflection.GetMethod is MethodInfo getter ? Assembly.Repository.GetMethodMetadata<IMethod>(getter) : null;

        /// <summary>
        /// Retrieves the setter method of the property, if it exists.
        /// </summary>
        /// <returns>An <see cref="IMethod"/> representing the setter method, or <see langword="null"/> if the property does not have a setter.</returns>
        protected virtual IMethod? GetSetterMethod() => Reflection.SetMethod is MethodInfo setter ? Assembly.Repository.GetMethodMetadata<IMethod>(setter) : null;

        /// <summary>
        /// Retrieves the index parameters of the property.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IParameter"/> objects representing the index parameters of the property.</returns>
        protected virtual IEnumerable<IParameter> GetIndexParameters() => Reflection.GetIndexParameters().Select(Assembly.Repository.GetParameterMetadata);

        /// <summary>
        /// Retrieves the extension block associated with the property, if it is an extension property.
        /// </summary>
        /// <returns>An <see cref="IExtensionBlock"/> representing the extension block, or <see langword="null"/> if the property is not an extension property.</returns>
        protected virtual IExtensionBlock? GetExtensionBlock() => Reflection is IExtensionBlockMemberInfo extensionMember
            ? Assembly.Repository.GetExtensionBlockMetadata(extensionMember.DeclaringBlock)
            : null;

        /// <summary>
        /// Retrieves properties from the specified type that have same name as this property.
        /// </summary>
        /// <param name="type">The type to search for similar property names.</param>
        /// <param name="preserveOrder">Indicates whether to preserve the order of properties as they appear in the type.</param>
        /// <returns>An enumerable collection of <see cref="IProperty"/> objects with same name as this property.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected IEnumerable<IProperty> GetPropertiesWithSameName(IType type, bool preserveOrder = false)
        {
            return IsExplicitInterfaceImplementation
                ? type is IWithExplicitInterfaceProperties withExplicitProperties
                    ? withExplicitProperties.ExplicitInterfaceProperties.WhereName(Name, preserveOrder)
                    : []
                : type is IWithProperties withProperties
                    ? withProperties.Properties.WhereName(Name, preserveOrder)
                    : [];
        }
    }
}

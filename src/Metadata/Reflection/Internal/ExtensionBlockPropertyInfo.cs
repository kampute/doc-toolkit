// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Reflection.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Represents a logical view of an extension property.
    /// </summary>
    internal sealed class ExtensionBlockPropertyInfo : PropertyInfo, IExtensionBlockPropertyInfo
    {
        private readonly MethodInfo? getter;
        private readonly MethodInfo? setter;

        public ExtensionBlockPropertyInfo(ExtensionBlockInfo block, PropertyInfo stubProperty, MethodInfo? declaredGetter, MethodInfo? declaredSetter)
        {
            if (declaredGetter is null && declaredSetter is null)
                throw new ArgumentException("At least one of getter or setter must be provided.", nameof(declaredGetter));

            DeclaringBlock = block ?? throw new ArgumentNullException(nameof(block));
            ReceivedProperty = stubProperty ?? throw new ArgumentNullException(nameof(stubProperty));

            if (declaredGetter is not null)
                getter = new ExtensionBlockMethodInfo(block, stubProperty.GetMethod, declaredGetter);
            if (declaredSetter is not null)
                setter = new ExtensionBlockMethodInfo(block, stubProperty.SetMethod, declaredSetter);
        }

        public ExtensionBlockInfo DeclaringBlock { get; }
        public PropertyInfo ReceivedProperty { get; }

        #region PropertyInfo Members

        public override PropertyAttributes Attributes => ReceivedProperty.Attributes;
        public override bool CanRead => ReceivedProperty.CanRead;
        public override bool CanWrite => ReceivedProperty.CanWrite;
        public override Type PropertyType => ReceivedProperty.PropertyType;

        public override ParameterInfo[] GetIndexParameters() => ReceivedProperty.GetIndexParameters();
        public override MethodInfo? GetGetMethod(bool nonPublic) => getter;
        public override MethodInfo? GetSetMethod(bool nonPublic) => setter;
        public override MethodInfo[] GetAccessors(bool nonPublic)
            => getter is not null && setter is not null ? [getter, setter] : getter is not null ? [getter] : [setter!];

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) => throw new NotImplementedException();
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) => throw new NotImplementedException();

        #endregion

        #region MemberInfo Members

        public override string Name => ReceivedProperty.Name;
        public override Module Module => ReceivedProperty.Module;
        public override int MetadataToken => ReceivedProperty.MetadataToken;
        public override MemberTypes MemberType => ReceivedProperty.MemberType;
        public override Type DeclaringType => DeclaringBlock.BlockType.DeclaringType;
        public override Type? ReflectedType => DeclaringBlock.BlockType.DeclaringType;
        public override IEnumerable<CustomAttributeData> CustomAttributes => ReceivedProperty.CustomAttributes;

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => ReceivedProperty.GetCustomAttributes(attributeType, inherit);
        public override object[] GetCustomAttributes(bool inherit) => ReceivedProperty.GetCustomAttributes(inherit);
        public override IList<CustomAttributeData> GetCustomAttributesData() => ReceivedProperty.GetCustomAttributesData();
        public override bool HasSameMetadataDefinitionAs(MemberInfo other) => throw new NotSupportedException();
        public override bool IsDefined(Type attributeType, bool inherit) => throw new NotSupportedException();
        public override bool Equals(object obj) => obj is ExtensionBlockPropertyInfo other && ReceivedProperty.Equals(other.ReceivedProperty);
        public override int GetHashCode() => HashCode.Combine(DeclaringBlock, ReceivedProperty);

        #endregion
    }
}

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
    internal sealed class ExtensionPropertyInfo : PropertyInfo, IExtensionPropertyInfo
    {
        private readonly MethodInfo? getter;
        private readonly MethodInfo? setter;

        public ExtensionPropertyInfo(ExtensionBlockInfo block, PropertyInfo stub, MethodInfo? getter, MethodInfo? setter)
        {
            if (getter is null && setter is null)
                throw new ArgumentException("At least one of getter or setter must be provided.", nameof(getter));

            ExtensionBlock = block ?? throw new ArgumentNullException(nameof(block));
            ReceiverProperty = stub ?? throw new ArgumentNullException(nameof(stub));

            if (getter is not null)
                this.getter = new ExtensionMethodInfo(block, stub.GetMethod, getter);
            if (setter is not null)
                this.setter = new ExtensionMethodInfo(block, stub.SetMethod, setter);
        }

        public ExtensionBlockInfo ExtensionBlock { get; }
        public PropertyInfo ReceiverProperty { get; }

        #region PropertyInfo Members

        public override PropertyAttributes Attributes => ReceiverProperty.Attributes;
        public override bool CanRead => ReceiverProperty.CanRead;
        public override bool CanWrite => ReceiverProperty.CanWrite;
        public override Type PropertyType => ReceiverProperty.PropertyType;

        public override ParameterInfo[] GetIndexParameters() => ReceiverProperty.GetIndexParameters();
        public override MethodInfo? GetGetMethod(bool nonPublic) => getter;
        public override MethodInfo? GetSetMethod(bool nonPublic) => setter;
        public override MethodInfo[] GetAccessors(bool nonPublic)
            => getter is not null && setter is not null ? [getter, setter] : getter is not null ? [getter] : [setter!];

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) => throw new NotImplementedException();
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) => throw new NotImplementedException();

        #endregion

        #region MemberInfo Members

        public override string Name => ReceiverProperty.Name;
        public override Module Module => ReceiverProperty.Module;
        public override int MetadataToken => ReceiverProperty.MetadataToken;
        public override MemberTypes MemberType => ReceiverProperty.MemberType;
        public override Type DeclaringType => (getter ?? setter)!.DeclaringType!;
        public override Type? ReflectedType => (getter ?? setter)!.ReflectedType;
        public override IEnumerable<CustomAttributeData> CustomAttributes => ReceiverProperty.CustomAttributes;

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => ReceiverProperty.GetCustomAttributes(attributeType, inherit);
        public override object[] GetCustomAttributes(bool inherit) => ReceiverProperty.GetCustomAttributes(inherit);
        public override IList<CustomAttributeData> GetCustomAttributesData() => ReceiverProperty.GetCustomAttributesData();
        public override bool HasSameMetadataDefinitionAs(MemberInfo other) => throw new NotSupportedException();
        public override bool IsDefined(Type attributeType, bool inherit) => throw new NotSupportedException();
        public override bool Equals(object obj) => obj is ExtensionPropertyInfo other && ReceiverProperty.Equals(other.ReceiverProperty);
        public override int GetHashCode() => HashCode.Combine(ExtensionBlock, ReceiverProperty);
        public override string ToString() => $"Extension property for {ExtensionBlock.Receiver.ParameterType}: {ReceiverProperty}";

        #endregion
    }
}

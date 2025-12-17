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
    /// Represents a logical view of an extension property defined in an extension block.
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

        /// <inheritdoc/>
        public ExtensionBlockInfo DeclaringBlock { get; }

        /// <inheritdoc/>
        public PropertyInfo ReceivedProperty { get; }

        #region PropertyInfo Members

        /// <summary>
        /// Gets the property attributes of the received property.
        /// </summary>
        /// <value>
        /// The property attributes.
        /// </value>
        public override PropertyAttributes Attributes => ReceivedProperty.Attributes;

        /// <summary>
        /// Gets a value indicating whether the received property can be read.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the property can be read; otherwise, <see langword="false"/>.
        /// </value>
        public override bool CanRead => ReceivedProperty.CanRead;

        /// <summary>
        /// Gets a value indicating whether the received property can be written.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the property can be written; otherwise, <see langword="false"/>.
        /// </value>
        public override bool CanWrite => ReceivedProperty.CanWrite;

        /// <summary>
        /// Gets the type of the received property.
        /// </summary>
        /// <value>
        /// The property type.
        /// </value>
        public override Type PropertyType => ReceivedProperty.PropertyType;

        /// <summary>
        /// Returns the index parameters of the received property.
        /// </summary>
        /// <returns>An array of parameter information for the index parameters.</returns>
        public override ParameterInfo[] GetIndexParameters() => ReceivedProperty.GetIndexParameters();

        /// <summary>
        /// Returns the getter method for this property.
        /// </summary>
        /// <param name="nonPublic">Whether to return non-public methods.</param>
        /// <returns>The getter method, or <see langword="null"/> if none exists.</returns>
        public override MethodInfo? GetGetMethod(bool nonPublic) => getter;

        /// <summary>
        /// Returns the setter method for this property.
        /// </summary>
        /// <param name="nonPublic">Whether to return non-public methods.</param>
        /// <returns>The setter method, or <see langword="null"/> if none exists.</returns>
        public override MethodInfo? GetSetMethod(bool nonPublic) => setter;

        /// <summary>
        /// Returns the accessor methods for this property.
        /// </summary>
        /// <param name="nonPublic">Whether to return non-public methods.</param>
        /// <returns>An array of accessor methods.</returns>
        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return getter is not null && setter is not null
                ? [getter, setter]
                : getter is not null
                    ? [getter]
                    : [setter!];
        }

        /// <summary>
        /// Not supported for extension block properties.
        /// </summary>
        /// <param name="obj">The object whose property value will be returned.</param>
        /// <param name="invokeAttr">The invocation attributes.</param>
        /// <param name="binder">The binder to use for binding.</param>
        /// <param name="index">The index parameters.</param>
        /// <param name="culture">The culture to use for the invocation.</param>
        /// <returns>This method always throws <see cref="NotSupportedException"/>.</returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) => throw new NotSupportedException();

        /// <summary>
        /// Not supported for extension block properties.
        /// </summary>
        /// <param name="obj">The object whose property value will be set.</param>
        /// <param name="value">The new property value.</param>
        /// <param name="invokeAttr">The invocation attributes.</param>
        /// <param name="binder">The binder to use for binding.</param>
        /// <param name="index">The index parameters.</param>
        /// <param name="culture">The culture to use for the invocation.</param>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) => throw new NotSupportedException();

        #endregion

        #region MemberInfo Members

        /// <summary>
        /// Gets the name of the received property.
        /// </summary>
        /// <value>
        /// The property name.
        /// </value>
        public override string Name => ReceivedProperty.Name;

        /// <summary>
        /// Gets the module of the received property.
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        public override Module Module => ReceivedProperty.Module;

        /// <summary>
        /// Gets the metadata token of the received property.
        /// </summary>
        /// <value>
        /// The metadata token.
        /// </value>
        public override int MetadataToken => ReceivedProperty.MetadataToken;

        /// <summary>
        /// Gets the member type of the received property.
        /// </summary>
        /// <value>
        /// The member type.
        /// </value>
        public override MemberTypes MemberType => ReceivedProperty.MemberType;

        /// <summary>
        /// Gets the declaring type of the extension block.
        /// </summary>
        /// <value>
        /// The declaring type.
        /// </value>
        public override Type DeclaringType => DeclaringBlock.BlockType.DeclaringType;

        /// <summary>
        /// Gets the reflected type of the extension block.
        /// </summary>
        /// <value>
        /// The reflected type.
        /// </value>
        public override Type? ReflectedType => DeclaringBlock.BlockType.DeclaringType;

        /// <summary>
        /// Gets the custom attributes of the received property.
        /// </summary>
        /// <value>
        /// An enumerable collection of custom attribute data.
        /// </value>
        public override IEnumerable<CustomAttributeData> CustomAttributes => ReceivedProperty.CustomAttributes;

        /// <summary>
        /// Returns the custom attributes of the specified type from the received property.
        /// </summary>
        /// <param name="attributeType">The type of attribute to retrieve.</param>
        /// <param name="inherit">Whether to search the inheritance chain.</param>
        /// <returns>An array of custom attributes.</returns>
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => ReceivedProperty.GetCustomAttributes(attributeType, inherit);

        /// <summary>
        /// Returns all custom attributes from the received property.
        /// </summary>
        /// <param name="inherit">Whether to search the inheritance chain.</param>
        /// <returns>An array of custom attributes.</returns>
        public override object[] GetCustomAttributes(bool inherit) => ReceivedProperty.GetCustomAttributes(inherit);

        /// <summary>
        /// Returns the custom attribute data from the received property.
        /// </summary>
        /// <returns>A list of custom attribute data.</returns>
        public override IList<CustomAttributeData> GetCustomAttributesData() => ReceivedProperty.GetCustomAttributesData();

        /// <summary>
        /// Determines whether the specified attribute type is defined on the received property.
        /// </summary>
        /// <param name="attributeType">The type of attribute to check.</param>
        /// <param name="inherit">Whether to search the inheritance chain.</param>
        /// <returns><see langword="true"/> if the attribute is defined; otherwise, <see langword="false"/>.</returns>
        public override bool IsDefined(Type attributeType, bool inherit) => ReceivedProperty.IsDefined(attributeType, inherit);

        /// <summary>
        /// Determines whether this property has the same metadata definition as the specified member.
        /// </summary>
        /// <param name="other">The member to compare with.</param>
        /// <returns><see langword="true"/> if the metadata definitions are the same; otherwise, <see langword="false"/>.</returns>
        public override bool HasSameMetadataDefinitionAs(MemberInfo other) => ReceivedProperty.HasSameMetadataDefinitionAs(other);

        /// <summary>
        /// Determines whether this instance is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns><see langword="true"/> if the objects are equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj) => obj is ExtensionBlockPropertyInfo other && ReceivedProperty.Equals(other.ReceivedProperty);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine(DeclaringBlock, ReceivedProperty);

        #endregion
    }
}

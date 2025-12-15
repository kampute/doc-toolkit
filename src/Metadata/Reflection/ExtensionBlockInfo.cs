// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Reflection
{
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Represents reflection information about an extension block.
    /// </summary>
    public sealed class ExtensionBlockInfo : MemberInfo, IEquatable<ExtensionBlockInfo>
    {
        private const BindingFlags AllDeclaredMembers =
            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionBlockInfo"/> class.
        /// </summary>
        /// <param name="blockType">The type representing the extension block.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="blockType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="blockType"/> does not represent a valid extension block.</exception>
        public ExtensionBlockInfo(Type blockType)
        {
            BlockType = blockType ?? throw new ArgumentNullException(nameof(blockType));

            ReceiverParameter = GetReceiverParameter(blockType)
                ?? throw new ArgumentException("The specified type does not represent a valid extension block.", nameof(blockType));

            var markerName = ReceiverParameter.Member.DeclaringType.Name;

            ExtensionMethods = GetExtensionMethods(blockType, markerName);
            ExtensionProperties = GetExtensionProperties(blockType, markerName);

            if (blockType.IsGenericType)
                TypeParameters = ReceiverParameter.ParameterType.GetGenericArguments();
        }

        /// <summary>
        /// Gets the type representing the extension block.
        /// </summary>
        /// <value>
        /// The type representing the extension block.
        /// </value>
        public Type BlockType { get; }

        /// <summary>
        /// Gets the type parameters of the extension block, if the block is generic.
        /// </summary>
        /// <value>
        /// The type parameters of the extension block.
        /// </value>
        public Type[] TypeParameters { get; } = [];

        /// <summary>
        /// Gets the receiver parameter of the extension block.
        /// </summary>
        /// <value>
        /// The receiver parameter of the extension block.
        /// </value>
        public ParameterInfo ReceiverParameter { get; }

        /// <summary>
        /// Gets the extension methods defined within the extension block.
        /// </summary>
        /// <value>
        /// The extension methods defined within the extension block.
        /// </value>
        public MethodInfo[] ExtensionMethods { get; }

        /// <summary>
        /// Gets the extension properties defined within the extension block.
        /// </summary>
        /// <value>
        /// The extension properties defined within the extension block.
        /// </value>
        public PropertyInfo[] ExtensionProperties { get; }

        /// <summary>
        /// Determines whether the current instance is equal to the specified <see cref="ExtensionBlockInfo"/> object.
        /// </summary>
        /// <param name="other">The <see cref="ExtensionBlockInfo"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the current instance is equal to <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public bool Equals(ExtensionBlockInfo other) => other is not null && BlockType.Equals(other.BlockType);

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the specified object is equal to the current instance; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj) => obj is ExtensionBlockInfo other && Equals(other);

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode() => HashCode.Combine(BlockType, ReceiverParameter);

        /// <summary>
        /// Determines whether two <see cref="ExtensionBlockInfo"/> instances are equal.
        /// </summary>
        /// <param name="left">The left <see cref="ExtensionBlockInfo"/> instance.</param>
        /// <param name="right">The right <see cref="ExtensionBlockInfo"/> instance.</param>
        /// <returns><see langword="true"/> if the instances are equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ExtensionBlockInfo? left, ExtensionBlockInfo? right) => Equals(left, right);

        /// <summary>
        /// Determines whether two <see cref="ExtensionBlockInfo"/> instances are not equal.
        /// </summary>
        /// <param name="left">The left <see cref="ExtensionBlockInfo"/> instance.</param>
        /// <param name="right">The right <see cref="ExtensionBlockInfo"/> instance.</param>
        /// <returns><see langword="true"/> if the instances are not equal; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(ExtensionBlockInfo? left, ExtensionBlockInfo? right) => !(left == right);

        /// <summary>
        /// Retrieves the receiver parameter for the specified extension block type.
        /// </summary>
        /// <param name="extensionBlockType">The type representing the extension block.</param>
        /// <returns>A <see cref="ParameterInfo"/> representing the receiver parameter, or <see langword="null"/> if not found.</returns>
        private static ParameterInfo? GetReceiverParameter(Type extensionBlockType)
        {
            foreach (var markerType in extensionBlockType.GetNestedTypes())
            {
                if (!markerType.IsSpecialName)
                    continue;

                var markerMethod = markerType.GetMethod("<Extension>$", BindingFlags.Public | BindingFlags.Static);
                if (markerMethod is not null)
                {
                    var parameters = markerMethod.GetParameters();
                    if (parameters.Length > 0)
                        return parameters[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the extension methods defined within the specified block type that match the given marker name.
        /// </summary>
        /// <param name="extensionBlockType">The type representing the extension block.</param>
        /// <param name="markerName">The marker name to filter methods.</param>
        /// <returns>An array of <see cref="MethodInfo"/> representing the extension methods.</returns>
        private static MethodInfo[] GetExtensionMethods(Type extensionBlockType, string markerName)
        {
            var methods = extensionBlockType.GetMethods(AllDeclaredMembers);

            var count = 0;
            for (var i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (!method.IsSpecialName && GetMarkerName(method) == markerName)
                    methods[count++] = method;
            }

            if (count != methods.Length)
                Array.Resize(ref methods, count);

            return methods;
        }

        /// <summary>
        /// Retrieves the extension properties defined within the specified block type that match the given marker name.
        /// </summary>
        /// <param name="extensionBlockType">The type representing the extension block.</param>
        /// <param name="markerName">The marker name to filter properties.</param>
        /// <returns>An array of <see cref="PropertyInfo"/> representing the extension properties.</returns>
        private static PropertyInfo[] GetExtensionProperties(Type extensionBlockType, string markerName)
        {
            var properties = extensionBlockType.GetProperties(AllDeclaredMembers);

            var count = 0;
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                if (GetMarkerName(property) == markerName)
                    properties[count++] = property;
            }

            if (count != properties.Length)
                Array.Resize(ref properties, count);

            return properties;
        }

        /// <summary>
        /// Gets the marker name from the specified member's custom attributes.
        /// </summary>
        /// <param name="member">The member to inspect.</param>
        /// <returns>A string representing the marker name, or <see langword="null"/> if not found.</returns>
        private static string? GetMarkerName(MemberInfo member)
        {
            foreach (var attribute in member.CustomAttributes)
            {
                if (attribute.AttributeType.FullName != AttributeNames.ExtensionMarker)
                    continue;

                if (attribute.ConstructorArguments.Count == 1)
                    return attribute.ConstructorArguments[0].Value as string;

                foreach (var namedArg in attribute.NamedArguments)
                {
                    if (namedArg.MemberName == "Name")
                        return namedArg.TypedValue.Value as string;
                }
            }

            return null;
        }

        #region MemberInfo Members

        /// <summary>
        /// Gets the name of the extension block.
        /// </summary>
        /// <value>
        /// The name of the extension block.
        /// </value>
        public override string Name => BlockType.FullName!;

        /// <summary>
        /// Gets the module in which the extension block is defined.
        /// </summary>
        /// <value>
        /// The module in which the extension block is defined.
        /// </value>
        public override Module Module => BlockType.Module;

        /// <summary>
        /// Gets the metadata token for the extension block.
        /// </summary>
        /// <value>
        /// The metadata token for the extension block.
        /// </value>
        public override int MetadataToken => BlockType.MetadataToken;

        /// <summary>
        /// Gets the member type of the extension block.
        /// </summary>
        /// <value>
        /// The member type of the extension block.
        /// </value>
        public override MemberTypes MemberType => BlockType.MemberType;

        /// <summary>
        /// Gets the type that declares the extension block.
        /// </summary>
        /// <value>
        /// The type that declares the extension block.
        /// </value>
        public override Type DeclaringType => BlockType.DeclaringType;

        /// <summary>
        /// Gets the type that was used to obtain the extension block.
        /// </summary>
        /// <value>
        /// The type that was used to obtain the extension block.
        /// </value>
        public override Type? ReflectedType => BlockType.DeclaringType;

        /// <summary>
        /// Gets the custom attributes applied to the extension block.
        /// </summary>
        /// <value>
        /// The custom attributes applied to the extension block.
        /// </value>
        public override IEnumerable<CustomAttributeData> CustomAttributes => BlockType.CustomAttributes;

        /// <summary>
        /// Returns custom attributes of the specified type applied to the extension block.
        /// </summary>
        /// <param name="attributeType">The type of attributes to retrieve.</param>
        /// <param name="inherit"><see langword="true"/> to search the inheritance chain; otherwise, <see langword="false"/>.</param>
        /// <returns>An array of custom attributes applied to the extension block.</returns>
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => BlockType.GetCustomAttributes(attributeType, inherit);

        /// <summary>
        /// Returns all custom attributes applied to the extension block.
        /// </summary>
        /// <param name="inherit"><see langword="true"/> to search the inheritance chain; otherwise, <see langword="false"/>.</param>
        /// <returns>An array of custom attributes applied to the extension block.</returns>
        public override object[] GetCustomAttributes(bool inherit) => BlockType.GetCustomAttributes(inherit);

        /// <summary>
        /// Returns custom attribute data for the extension block.
        /// </summary>
        /// <returns>A list of custom attribute data for the extension block.</returns>
        public override IList<CustomAttributeData> GetCustomAttributesData() => BlockType.GetCustomAttributesData();

        /// <summary>
        /// Determines whether the extension block has the same metadata definition as the specified member.
        /// </summary>
        /// <param name="other">The member to compare.</param>
        /// <returns><see langword="true"/> if the extension block has the same metadata definition as <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public override bool HasSameMetadataDefinitionAs(MemberInfo other) => BlockType.HasSameMetadataDefinitionAs(other);

        /// <summary>
        /// Determines whether the specified attribute type is applied to the extension block.
        /// </summary>
        /// <param name="attributeType">The type of attribute to check.</param>
        /// <param name="inherit"><see langword="true"/> to search the inheritance chain; otherwise, <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if the attribute is applied to the extension block; otherwise, <see langword="false"/>.</returns>
        public override bool IsDefined(Type attributeType, bool inherit) => BlockType.IsDefined(attributeType, inherit);

        #endregion
    }
}

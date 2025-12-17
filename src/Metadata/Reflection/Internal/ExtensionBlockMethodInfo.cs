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
    /// Represents a logical view of a extension method defined in an extension block.
    /// </summary>
    internal sealed class ExtensionBlockMethodInfo : MethodInfo, IExtensionBlockMethodInfo
    {
        public ExtensionBlockMethodInfo(ExtensionBlockInfo block, MethodInfo stubMethod, MethodInfo declaredMethod)
        {
            DeclaringBlock = block ?? throw new ArgumentNullException(nameof(block));
            ReceivedMethod = stubMethod ?? throw new ArgumentNullException(nameof(stubMethod));
            DeclaredMethod = declaredMethod ?? throw new ArgumentNullException(nameof(declaredMethod));
        }

        /// <inheritdoc/>
        public ExtensionBlockInfo DeclaringBlock { get; }

        /// <inheritdoc/>
        public MethodInfo DeclaredMethod { get; }

        /// <inheritdoc/>
        public MethodInfo ReceivedMethod { get; }

        #region MethodInfo Members

        /// <summary>
        /// Gets the runtime method handle of the declared method.
        /// </summary>
        /// <value>
        /// The runtime method handle.
        /// </value>
        public override RuntimeMethodHandle MethodHandle => DeclaredMethod.MethodHandle;

        /// <summary>
        /// Gets the return parameter of the received method.
        /// </summary>
        /// <value>
        /// The return parameter.
        /// </value>
        public override ParameterInfo ReturnParameter => ReceivedMethod.ReturnParameter;

        /// <summary>
        /// Gets the return type of the received method.
        /// </summary>
        /// <value>
        /// The return type.
        /// </value>
        public override Type ReturnType => ReceivedMethod.ReturnType;

        /// <summary>
        /// Gets the custom attributes provider for the return type of the received method.
        /// </summary>
        /// <value>
        /// The custom attributes provider for the return type.
        /// </value>
        public override ICustomAttributeProvider ReturnTypeCustomAttributes => ReceivedMethod.ReturnTypeCustomAttributes;

        /// <summary>
        /// Gets the method attributes of the received method.
        /// </summary>
        /// <value>
        /// The method attributes.
        /// </value>
        public override MethodAttributes Attributes => ReceivedMethod.Attributes;

        /// <summary>
        /// Gets a value indicating whether the received method is a generic method.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the method is generic; otherwise, <see langword="false"/>.
        /// </value>
        public override bool IsGenericMethod => ReceivedMethod.IsGenericMethod;

        /// <summary>
        /// Gets a value indicating whether the received method is a generic method definition.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the method is a generic method definition; otherwise, <see langword="false"/>.
        /// </value>
        public override bool IsGenericMethodDefinition => ReceivedMethod.IsGenericMethodDefinition;

        /// <summary>
        /// Gets a value indicating whether the received method contains generic parameters.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the method contains generic parameters; otherwise, <see langword="false"/>.
        /// </value>
        public override bool ContainsGenericParameters => ReceivedMethod.ContainsGenericParameters;

        /// <summary>
        /// Returns the generic arguments of the received method.
        /// </summary>
        /// <returns>An array of types representing the generic arguments.</returns>
        public override Type[] GetGenericArguments() => ReceivedMethod.GetGenericArguments();

        /// <summary>
        /// Returns the parameters of the received method.
        /// </summary>
        /// <returns>An array of parameter information.</returns>
        public override ParameterInfo[] GetParameters() => ReceivedMethod.GetParameters();

        /// <summary>
        /// Returns the method implementation flags of the declared method.
        /// </summary>
        /// <returns>The method implementation flags.</returns>
        public override MethodImplAttributes GetMethodImplementationFlags() => DeclaredMethod.GetMethodImplementationFlags();

        /// <summary>
        /// Returns the base definition of the declared method.
        /// </summary>
        /// <returns>The base method definition.</returns>
        public override MethodInfo GetBaseDefinition() => DeclaredMethod.GetBaseDefinition();

        /// <summary>
        /// Not supported for extension block methods.
        /// </summary>
        /// <param name="delegateType">The type of delegate to create.</param>
        /// <returns>This method always throws <see cref="NotSupportedException"/>.</returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override Delegate CreateDelegate(Type delegateType) => throw new NotSupportedException();

        /// <summary>
        /// Not supported for extension block methods.
        /// </summary>
        /// <param name="delegateType">The type of delegate to create.</param>
        /// <param name="target">The object to bind the delegate to.</param>
        /// <returns>This method always throws <see cref="NotSupportedException"/>.</returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override Delegate CreateDelegate(Type delegateType, object target) => throw new NotSupportedException();

        /// <summary>
        /// Not supported for extension block methods.
        /// </summary>
        /// <returns>This method always throws <see cref="NotSupportedException"/>.</returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override MethodInfo GetGenericMethodDefinition() => throw new NotSupportedException();

        /// <summary>
        /// Not supported for extension block methods.
        /// </summary>
        /// <param name="typeArguments">The type arguments to substitute.</param>
        /// <returns>This method always throws <see cref="NotSupportedException"/>.</returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override MethodInfo MakeGenericMethod(params Type[] typeArguments) => throw new NotSupportedException();

        /// <summary>
        /// Not supported for extension block methods.
        /// </summary>
        /// <param name="obj">The object on which to invoke the method.</param>
        /// <param name="invokeAttr">The invocation attributes.</param>
        /// <param name="binder">The binder to use for binding.</param>
        /// <param name="parameters">The parameters to pass to the method.</param>
        /// <param name="culture">The culture to use for the invocation.</param>
        /// <returns>This method always throws <see cref="NotSupportedException"/>.</returns>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        public override object? Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo? culture) => throw new NotSupportedException();

        #endregion

        #region MemberInfo Members

        /// <summary>
        /// Gets the name of the declared method.
        /// </summary>
        /// <value>
        /// The method name.
        /// </value>
        public override string Name => DeclaredMethod.Name;

        /// <summary>
        /// Gets the module of the declared method.
        /// </summary>
        /// <value>
        /// The module.
        /// </value>
        public override Module Module => DeclaredMethod.Module;

        /// <summary>
        /// Gets the metadata token of the declared method.
        /// </summary>
        /// <value>
        /// The metadata token.
        /// </value>
        public override int MetadataToken => DeclaredMethod.MetadataToken;

        /// <summary>
        /// Gets the member type of the declared method.
        /// </summary>
        /// <value>
        /// The member type.
        /// </value>
        public override MemberTypes MemberType => DeclaredMethod.MemberType;

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
        /// Gets the custom attributes of the received method.
        /// </summary>
        /// <value>
        /// An enumerable collection of custom attribute data.
        /// </value>
        public override IEnumerable<CustomAttributeData> CustomAttributes => ReceivedMethod.CustomAttributes;

        /// <summary>
        /// Returns the custom attributes of the specified type from the received method.
        /// </summary>
        /// <param name="attributeType">The type of attribute to retrieve.</param>
        /// <param name="inherit">Whether to search the inheritance chain.</param>
        /// <returns>An array of custom attributes.</returns>
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => ReceivedMethod.GetCustomAttributes(attributeType, inherit);

        /// <summary>
        /// Returns all custom attributes from the received method.
        /// </summary>
        /// <param name="inherit">Whether to search the inheritance chain.</param>
        /// <returns>An array of custom attributes.</returns>
        public override object[] GetCustomAttributes(bool inherit) => ReceivedMethod.GetCustomAttributes(inherit);

        /// <summary>
        /// Returns the custom attribute data from the received method.
        /// </summary>
        /// <returns>A list of custom attribute data.</returns>
        public override IList<CustomAttributeData> GetCustomAttributesData() => ReceivedMethod.GetCustomAttributesData();

        /// <summary>
        /// Determines whether the specified attribute type is defined on the received method.
        /// </summary>
        /// <param name="attributeType">The type of attribute to check.</param>
        /// <param name="inherit">Whether to search the inheritance chain.</param>
        /// <returns><see langword="true"/> if the attribute is defined; otherwise, <see langword="false"/>.</returns>
        public override bool IsDefined(Type attributeType, bool inherit) => ReceivedMethod.IsDefined(attributeType, inherit);

        /// <summary>
        /// Determines whether this method has the same metadata definition as the specified member.
        /// </summary>
        /// <param name="other">The member to compare with.</param>
        /// <returns><see langword="true"/> if the metadata definitions are the same; otherwise, <see langword="false"/>.</returns>
        public override bool HasSameMetadataDefinitionAs(MemberInfo other) => ReceivedMethod.HasSameMetadataDefinitionAs(other);

        /// <summary>
        /// Determines whether this instance is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns><see langword="true"/> if the objects are equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj) => obj is ExtensionBlockMethodInfo other && DeclaredMethod.Equals(other.DeclaredMethod);

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode() => HashCode.Combine(DeclaringBlock, DeclaredMethod);

        #endregion
    }
}

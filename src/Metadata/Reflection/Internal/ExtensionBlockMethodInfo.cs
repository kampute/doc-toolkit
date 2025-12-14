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
    /// Represents a logical view of an extension method.
    /// </summary>
    internal sealed class ExtensionBlockMethodInfo : MethodInfo, IExtensionBlockMethodInfo
    {

        public ExtensionBlockMethodInfo(ExtensionBlockInfo block, MethodInfo stubMethod, MethodInfo declaredMethod)
        {
            DeclaringBlock = block ?? throw new ArgumentNullException(nameof(block));
            ReceivedMethod = stubMethod ?? throw new ArgumentNullException(nameof(stubMethod));
            DeclaredMethod = declaredMethod ?? throw new ArgumentNullException(nameof(declaredMethod));
        }

        public ExtensionBlockInfo DeclaringBlock { get; }
        public MethodInfo DeclaredMethod { get; }
        public MethodInfo ReceivedMethod { get; }

        #region MethodInfo Members

        public override RuntimeMethodHandle MethodHandle => DeclaredMethod.MethodHandle;
        public override ParameterInfo ReturnParameter => ReceivedMethod.ReturnParameter;
        public override Type ReturnType => ReceivedMethod.ReturnType;
        public override ICustomAttributeProvider ReturnTypeCustomAttributes => ReceivedMethod.ReturnTypeCustomAttributes;
        public override MethodAttributes Attributes => ReceivedMethod.Attributes;
        public override bool IsGenericMethod => ReceivedMethod.IsGenericMethod;
        public override bool IsGenericMethodDefinition => ReceivedMethod.IsGenericMethodDefinition;
        public override bool ContainsGenericParameters => ReceivedMethod.ContainsGenericParameters;
        public override Type[] GetGenericArguments() => ReceivedMethod.GetGenericArguments();
        public override ParameterInfo[] GetParameters() => ReceivedMethod.GetParameters();

        public override MethodImplAttributes GetMethodImplementationFlags() => DeclaredMethod.GetMethodImplementationFlags();
        public override MethodInfo GetBaseDefinition() => DeclaredMethod.GetBaseDefinition();

        public override Delegate CreateDelegate(Type delegateType) => throw new NotImplementedException();
        public override Delegate CreateDelegate(Type delegateType, object target) => throw new NotImplementedException();
        public override MethodInfo GetGenericMethodDefinition() => throw new NotImplementedException();
        public override MethodInfo MakeGenericMethod(params Type[] typeArguments) => throw new NotImplementedException();
        public override object? Invoke(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? parameters, CultureInfo? culture) => throw new NotImplementedException();

        #endregion

        #region MemberInfo Members

        public override string Name => DeclaredMethod.Name;
        public override Module Module => DeclaredMethod.Module;
        public override int MetadataToken => DeclaredMethod.MetadataToken;
        public override MemberTypes MemberType => DeclaredMethod.MemberType;
        public override Type DeclaringType => DeclaringBlock.BlockType.DeclaringType;
        public override Type? ReflectedType => DeclaringBlock.BlockType.DeclaringType;
        public override IEnumerable<CustomAttributeData> CustomAttributes => ReceivedMethod.CustomAttributes;

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => ReceivedMethod.GetCustomAttributes(attributeType, inherit);
        public override object[] GetCustomAttributes(bool inherit) => ReceivedMethod.GetCustomAttributes(inherit);
        public override IList<CustomAttributeData> GetCustomAttributesData() => ReceivedMethod.GetCustomAttributesData();
        public override bool IsDefined(Type attributeType, bool inherit) => throw new NotImplementedException();
        public override bool HasSameMetadataDefinitionAs(MemberInfo other) => throw new NotImplementedException();
        public override bool Equals(object obj) => obj is ExtensionBlockMethodInfo other && DeclaredMethod.Equals(other.DeclaredMethod);
        public override int GetHashCode() => HashCode.Combine(DeclaringBlock, DeclaredMethod);

        #endregion
    }
}

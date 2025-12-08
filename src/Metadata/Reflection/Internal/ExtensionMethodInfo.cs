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
    internal sealed class ExtensionMethodInfo : MethodInfo, IExtensionMethodInfo
    {
        private readonly Lazy<Type[]> ownedGenericArguments;

        public ExtensionMethodInfo(MethodInfo classicMethod)
        {
            DeclaredMethod = classicMethod ?? throw new ArgumentNullException(nameof(classicMethod));
            ReceiverParameter = classicMethod.GetParameters()[0];
            ownedGenericArguments = new(GetOwnedGenericArguments);
        }

        public ExtensionMethodInfo(MethodInfo method, ParameterInfo receiver, MethodInfo stub)
        {
            DeclaredMethod = method ?? throw new ArgumentNullException(nameof(method));
            ReceiverParameter = receiver ?? throw new ArgumentNullException(nameof(receiver));
            ReceiverMethod = stub ?? throw new ArgumentNullException(nameof(stub));
            ownedGenericArguments = new(ReceiverMethod.GetGenericArguments);
        }

        public MethodInfo DeclaredMethod { get; }
        public MethodInfo? ReceiverMethod { get; }
        public MethodInfo? MarkerMethod { get; }
        public ParameterInfo ReceiverParameter { get; }

        #region MethodInfo Members

        public override ParameterInfo ReturnParameter => DeclaredMethod.ReturnParameter;
        public override Type ReturnType => DeclaredMethod.ReturnType;
        public override ICustomAttributeProvider ReturnTypeCustomAttributes => DeclaredMethod.ReturnTypeCustomAttributes;
        public override RuntimeMethodHandle MethodHandle => DeclaredMethod.MethodHandle;

        public override MethodAttributes Attributes => ReceiverMethod is not null
            ? ReceiverMethod.Attributes
            : DeclaredMethod.Attributes & ~MethodAttributes.Static; // Remove static flag for classic extension methods

        public override bool IsGenericMethod => ReceiverMethod is not null
            ? ReceiverMethod.IsGenericMethod
            : DeclaredMethod.IsGenericMethod && ownedGenericArguments.Value.Length > 0;

        public override bool IsGenericMethodDefinition => ReceiverMethod is not null
            ? ReceiverMethod.IsGenericMethodDefinition
            : DeclaredMethod.IsGenericMethodDefinition && ownedGenericArguments.Value.Length > 0;

        public override bool ContainsGenericParameters => ReceiverMethod is not null
            ? ReceiverMethod.ContainsGenericParameters
            : DeclaredMethod.ContainsGenericParameters && ownedGenericArguments.Value.Length > 0;

        public override Type[] GetGenericArguments() => ReceiverMethod is not null
            ? ReceiverMethod.GetGenericArguments()
            : ownedGenericArguments.Value;

        public override ParameterInfo[] GetParameters()
        {
            if (ReceiverMethod is not null)
                return ReceiverMethod.GetParameters();

            var parameters = DeclaredMethod.GetParameters();

            // Remove the first parameter (receiver) for classic extension methods
            return parameters.Length > 1 ? parameters[1..] : [];
        }

        private Type[] GetOwnedGenericArguments()
        {
            if (!DeclaredMethod.IsGenericMethodDefinition)
                return [];

            var genericArgs = DeclaredMethod.GetGenericArguments();
            if (!ReceiverParameter.ParameterType.IsGenericType)
                return genericArgs;

            var receiverGenericArgs = ReceiverParameter.ParameterType.GetGenericArguments();
            if (receiverGenericArgs.Length == 0)
                return genericArgs;

            // Remove receiver generic arguments for classic extension methods
            return genericArgs.Length > receiverGenericArgs.Length ? genericArgs[receiverGenericArgs.Length..] : [];
        }

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
        public override Type DeclaringType => DeclaredMethod.DeclaringType!;
        public override Type? ReflectedType => DeclaredMethod.ReflectedType;
        public override IEnumerable<CustomAttributeData> CustomAttributes => DeclaredMethod.CustomAttributes;

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => DeclaredMethod.GetCustomAttributes(attributeType, inherit);
        public override object[] GetCustomAttributes(bool inherit) => DeclaredMethod.GetCustomAttributes(inherit);
        public override IList<CustomAttributeData> GetCustomAttributesData() => DeclaredMethod.GetCustomAttributesData();
        public override bool IsDefined(Type attributeType, bool inherit) => throw new NotImplementedException();
        public override bool HasSameMetadataDefinitionAs(MemberInfo other) => throw new NotImplementedException();
        public override bool Equals(object obj) => obj is ExtensionMethodInfo other && DeclaredMethod.Equals(other.DeclaredMethod);
        public override int GetHashCode() => HashCode.Combine(DeclaredMethod, ReceiverParameter);
        public override string ToString() => $"Extension method for {ReceiverParameter.ParameterType}: {ReceiverMethod ?? DeclaredMethod}";

        #endregion
    }
}

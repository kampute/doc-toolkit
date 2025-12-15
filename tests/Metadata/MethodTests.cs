// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class MethodTests
    {
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.StaticMethod))]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod))]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.VirtualMethod))]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints))]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithGenericParameter))]
        public void ImplementsMethod(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;

            Assert.That(metadata, Is.InstanceOf<IMethod>());
            Assert.That(metadata.Name, Is.EqualTo(methodName));
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithoutTypeConstraints), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.VirtualMethod), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.StaticMethod), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithGenericParameter), ExpectedResult = true)]
        public bool IsGeneric_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsGenericMethod;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.UnsafeMethod), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), ExpectedResult = true)]
        public bool IsUnsafe_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsUnsafe;
        }

        [TestCase(typeof(Acme.SampleGenericStruct<MemoryStream>.InnerGenericStruct<int, string>.DeepInnerGenericStruct), "Method", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleGenericStruct<MemoryStream>.InnerGenericStruct<int, string>.DeepInnerGenericStruct), "GenericMethod", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.VirtualMethod), ExpectedResult = false)]
        public bool IsReadOnly_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsReadOnly;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.InternalMethod), ExpectedResult = MemberVisibility.Internal)]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethod", ExpectedResult = MemberVisibility.Private)]
        public MemberVisibility Visibility_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Visibility;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.StaticMethod), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.VirtualMethod), ExpectedResult = false)]
        public bool IsStatic_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsStatic;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.InternalMethod), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.VirtualMethod), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethod", ExpectedResult = false)]
        public bool IsVisible_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsVisible;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.VirtualMethod), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.ClassicExtensionMethodForClass), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethodForClass), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethodForClass), ExpectedResult = false)]
        public bool IsSpecialName_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsSpecialName;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceMethod), ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceStaticDefaultMethod), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethod", ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceStaticMethod", ExpectedResult = true)]
        public bool IsExplicitInterfaceImplementation_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsExplicitInterfaceImplementation;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceMethod), ExpectedResult = true)]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceStaticDefaultMethod), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethod", ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceStaticMethod", ExpectedResult = false)]
        public bool IsDefaultInterfaceImplementation_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsDefaultInterfaceImplementation;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod))]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethod")]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceStaticMethod")]
        public void DeclaringType_HasExpectedType(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.DeclaringType.Name, Is.EqualTo(declaringType.Name));
        }

        [TestCase(typeof(Acme.SampleDerivedGenericClass<,,>), nameof(Acme.SampleDerivedConstructedGenericClass.GenericMethod), ExpectedResult = MemberVirtuality.Override)]
        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass), nameof(Acme.SampleDerivedConstructedGenericClass.GenericMethod), ExpectedResult = MemberVirtuality.SealedOverride)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.AbstractMethod), ExpectedResult = MemberVirtuality.Abstract)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.VirtualMethod), ExpectedResult = MemberVirtuality.Virtual)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = MemberVirtuality.None)]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethod", ExpectedResult = MemberVirtuality.None)]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceStaticMethod", ExpectedResult = MemberVirtuality.None)]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceStaticMethod), ExpectedResult = MemberVirtuality.Abstract)]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceStaticDefaultMethod), ExpectedResult = MemberVirtuality.Virtual)]
        public MemberVirtuality Virtuality_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Virtuality;
        }

        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass), nameof(Acme.SampleDerivedConstructedGenericClass.Method), typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass))]
        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass), nameof(Acme.SampleDerivedConstructedGenericClass.GenericMethod), typeof(Acme.SampleDerivedGenericClass<,,>))]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.VirtualMethod), null)]
        public void OverriddenMethod_HasExpectedValue(Type declaringType, string methodName, Type? expectedBaseType)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.OverriddenMethod?.DeclaringType, Is.EqualTo(expectedBaseType?.GetMetadata()));
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), null)]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceMethod), null)]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethod", typeof(Acme.ISampleInterface))]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceStaticMethod", typeof(Acme.ISampleInterface))]
        public void InterfaceMethod_HasExpectedValue(Type declaringType, string methodName, Type? expectedInterfaceType)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.ImplementedMethod?.DeclaringType, Is.EqualTo(expectedInterfaceType?.GetMetadata()));
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints), ExpectedResult = 1)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithoutTypeConstraints), ExpectedResult = 2)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithGenericParameter), ExpectedResult = 1)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = 0)]
        public int TypeParameters_HasExpectedCount(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.TypeParameters.Count;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = 0)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RefParamsMethod), ExpectedResult = 3)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.ArrayParamsMethod), ExpectedResult = 1)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.OptionalParamsMethod), ExpectedResult = 2)]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithGenericParameter), ExpectedResult = 1)]
        public int Parameters_HasExpectedCount(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Parameters.Count;
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = "Void")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints), ExpectedResult = "T")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.VirtualMethod), ExpectedResult = "Void")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithGenericParameter), ExpectedResult = "S")]
        public string ReturnParameter_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Return.Type.Name;
        }

        [TestCase(typeof(Acme.SampleDerivedGenericClass<,,>), nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod), ExpectedResult = "M:Acme.SampleDerivedGenericClass`3.GenericMethod``1(`0,`1,`2,``0)")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = "M:Acme.SampleMethods.RegularMethod")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints), ExpectedResult = "M:Acme.SampleMethods.GenericMethodWithTypeConstraints``1(``0)")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithGenericParameter), ExpectedResult = "M:Acme.SampleMethods.GenericMethodWithGenericParameter``1(System.Collections.Generic.List{``0})")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RefParamsMethod), ExpectedResult = "M:Acme.SampleMethods.RefParamsMethod(System.Int32@,System.String@,System.Double@)")]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethod", ExpectedResult = "M:Acme.SampleMethods.Acme#ISampleInterface#InterfaceMethod")]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceStaticMethod", ExpectedResult = "M:Acme.SampleMethods.Acme#ISampleInterface#InterfaceStaticMethod")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.ClassicExtensionMethodForClass), ExpectedResult = "M:Acme.SampleExtensions.ClassicExtensionMethodForClass``1(Acme.SampleGenericClass{``0})")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethodForClass), ExpectedResult = "M:Acme.SampleExtensions.InstanceExtensionMethodForClass``1(Acme.SampleGenericClass{``0})")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethodForClass), ExpectedResult = "M:Acme.SampleExtensions.StaticExtensionMethodForClass``1")]
        public string CodeReference_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, Acme.Bindings.AllDeclared);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CodeReference;
        }
    }
}

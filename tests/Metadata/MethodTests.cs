// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.Reflection;

    [TestFixture]
    public class MethodTests
    {
        private readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        [TestCase("M0", typeof(Acme.Widget))]
        [TestCase("M1", typeof(Acme.Widget))]
        [TestCase("M7", typeof(Acme.Widget))]
        [TestCase("VirtualMethod", typeof(Acme.DerivedClass))]
        public void ImplementsMethod(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;

            Assert.That(metadata, Is.InstanceOf<IMethod>());
            Assert.That(metadata.Name, Is.EqualTo(methodName));
        }

        [TestCase("M0", typeof(Acme.Widget), ExpectedResult = false)]
        [TestCase("M7", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("VirtualMethod", typeof(Acme.DerivedClass), ExpectedResult = false)]
        public bool IsGeneric_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsGenericMethod;
        }

        [TestCase("M3", typeof(Acme.Widget), ExpectedResult = false)]
        [TestCase("M4", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("M5", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("M6", typeof(Acme.Widget), ExpectedResult = false)]
        public bool IsUnsafe_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsUnsafe;
        }

        [TestCase("M2", typeof(Acme.ValueType), ExpectedResult = true)]
        [TestCase("Clone", typeof(Acme.ValueType), ExpectedResult = true)]
        [TestCase("M0", typeof(Acme.Widget), ExpectedResult = false)]
        [TestCase("VirtualMethod", typeof(Acme.DerivedClass), ExpectedResult = false)]
        public bool IsReadOnly_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsReadOnly;
        }

        [TestCase("VirtualAsyncMethod", typeof(Acme.BaseClass), ExpectedResult = true)]
        [TestCase("VirtualAsyncMethod", typeof(Acme.DerivedClass), ExpectedResult = true)]
        [TestCase("ProcessAsync", typeof(Acme.UseList), ExpectedResult = true)]
        [TestCase("M0", typeof(Acme.Widget), ExpectedResult = false)]
        [TestCase("VirtualMethod", typeof(Acme.DerivedClass), ExpectedResult = false)]
        public bool IsAsync_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsAsync;
        }

        [TestCase("Hello", typeof(Acme.Extensions), ExpectedResult = true)]
        [TestCase("M0", typeof(Acme.Widget), ExpectedResult = false)]
        [TestCase("VirtualMethod", typeof(Acme.DerivedClass), ExpectedResult = false)]
        public bool IsExtension_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsExtension;
        }

        [TestCase("M0", typeof(Acme.Widget), ExpectedResult = MemberVisibility.Public)]
        [TestCase("M7", typeof(Acme.Widget), ExpectedResult = MemberVisibility.Public)]
        [TestCase("VirtualMethod", typeof(Acme.DerivedClass), ExpectedResult = MemberVisibility.Public)]
        [TestCase("Acme.IProcess<System.String>.GetStatus", typeof(Acme.DerivedClass), ExpectedResult = MemberVisibility.Private)]
        public MemberVisibility Visibility_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Visibility;
        }

        [TestCase("M0", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("M1", typeof(Acme.Widget), ExpectedResult = false)]
        [TestCase("VirtualMethod", typeof(Acme.DerivedClass), ExpectedResult = false)]
        public bool IsStatic_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsStatic;
        }

        [TestCase("M0", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("M1", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("M7", typeof(Acme.Widget), ExpectedResult = true)]
        [TestCase("VirtualMethod", typeof(Acme.DerivedClass), ExpectedResult = true)]
        [TestCase("Acme.IProcess<System.String>.GetStatus", typeof(Acme.DerivedClass), ExpectedResult = false)]
        public bool IsVisible_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsVisible;
        }

        [TestCase("M0", typeof(Acme.Widget), ExpectedResult = false)]
        [TestCase("M1", typeof(Acme.Widget), ExpectedResult = false)]
        [TestCase("VirtualMethod", typeof(Acme.DerivedClass), ExpectedResult = false)]
        public bool IsSpecialName_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsSpecialName;
        }

        [TestCase("M0", typeof(Acme.Widget), ExpectedResult = false)]
        [TestCase("VirtualMethod", typeof(Acme.DerivedClass), ExpectedResult = false)]
        [TestCase("InterfaceMethod", typeof(Acme.DerivedClass), ExpectedResult = false)]
        [TestCase("Acme.IProcess<System.String>.GetStatus", typeof(Acme.DerivedClass), ExpectedResult = true)]
        public bool IsExplicitInterfaceImplementation_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsExplicitInterfaceImplementation;
        }

        [TestCase("InterfaceMethod", typeof(Acme.DerivedClass), ExpectedResult = false)]
        [TestCase("InterfaceMethod", typeof(Acme.ITestInterface), ExpectedResult = false)]
        [TestCase("InterfaceDefaultMethod", typeof(Acme.ITestInterface), ExpectedResult = true)]
        public bool IsDefaultInterfaceImplementation_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsDefaultInterfaceImplementation;
        }

        [TestCase("M0")]
        public void DeclaringType_HasExpectedType(string methodName)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.DeclaringType.Name, Is.EqualTo(nameof(Acme.Widget)));
        }

        [TestCase("VirtualMethod", typeof(Acme.BaseClass), ExpectedResult = MemberVirtuality.Virtual)]
        [TestCase("VirtualMethod", typeof(Acme.DerivedClass), ExpectedResult = MemberVirtuality.SealedOverride)]
        [TestCase("M0", typeof(Acme.Widget), ExpectedResult = MemberVirtuality.None)]
        [TestCase("M2", typeof(Acme.Widget.NestedClass), ExpectedResult = MemberVirtuality.Abstract)]
        [TestCase("M2", typeof(Acme.Widget.NestedDerivedClass), ExpectedResult = MemberVirtuality.SealedOverride)]
        [TestCase("InterfaceMethod", typeof(Acme.DerivedClass), ExpectedResult = MemberVirtuality.None)]
        [TestCase("Acme.IProcess<System.String>.GetStatus", typeof(Acme.DerivedClass), ExpectedResult = MemberVirtuality.None)]
        public MemberVirtuality Virtuality_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Virtuality;
        }

        [TestCase(nameof(TestTypes.TestDerivedClass.RegularTestMethod), typeof(TestTypes.TestDerivedClass), ExpectedResult = null)]
        [TestCase(nameof(TestTypes.TestDerivedClass.VirtualTestMethod), typeof(TestTypes.TestDerivedClass), ExpectedResult = nameof(TestTypes.TestBaseClass))]
        [TestCase(nameof(TestTypes.TestDerivedClass.VirtualGenericMethod), typeof(TestTypes.TestDerivedClass), ExpectedResult = nameof(TestTypes.TestBaseClass))]
        [TestCase("GetValueOrDefault", typeof(TestTypes.GenericDerivedClass<>), ExpectedResult = "GenericBaseClass`1")]
        [TestCase("GetValueOrDefault", typeof(TestTypes.GenericDerivedClass<int>), ExpectedResult = "GenericBaseClass`1")]
        [TestCase("GetValueOrDefault", typeof(TestTypes.ConstructedGenericDerivedClass), ExpectedResult = "GenericBaseClass`1")]
        //[TestCase("GetValueOrDefault", typeof(TestTypes.DrivedFromConstructedGeneric), ExpectedResult = "ConstructedGenericDerivedClass")]
        public string? OverriddenMethod_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.OverriddenMethod?.DeclaringType.Name;
        }

        [TestCase(nameof(TestTypes.TestDerivedClass.RegularTestMethod), typeof(TestTypes.TestDerivedClass), ExpectedResult = null)]
        [TestCase(nameof(TestTypes.TestBaseClass.InterfaceTestMethod), typeof(TestTypes.TestBaseClass), ExpectedResult = nameof(TestTypes.ITestInterface))]
        [TestCase(nameof(TestTypes.TestBaseClass.InterfaceGenericTestMethod), typeof(TestTypes.TestBaseClass), ExpectedResult = nameof(TestTypes.ITestInterface))]
        [TestCase("Acme.IProcess<System.String>.GetStatus", typeof(Acme.DerivedClass), ExpectedResult = "IProcess`1")]
        public string? InterfaceMethod_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.ImplementedMethod?.DeclaringType.Name;
        }

        [TestCase("M7", ExpectedResult = 1)]
        [TestCase("M0", ExpectedResult = 0)]
        public int TypeParameters_HasExpectedCount(string methodName)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.TypeParameters.Count;
        }

        [TestCase("M0", ExpectedResult = 0)]
        [TestCase("M1", ExpectedResult = 4)]
        [TestCase("M6", ExpectedResult = 2)]
        [TestCase("M7", ExpectedResult = 0)]
        public int Parameters_HasExpectedCount(string methodName)
        {
            var methodInfo = typeof(Acme.Widget).GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Parameters.Count;
        }

        [TestCase("M0", typeof(Acme.Widget), ExpectedResult = "Void")]
        [TestCase("M7", typeof(Acme.Widget), ExpectedResult = "T")]
        [TestCase("VirtualMethod", typeof(Acme.DerivedClass), ExpectedResult = "String")]
        public string ReturnParameter_HasExpectedValue(string methodName, Type declaringType)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Return.Type.Name;
        }

        [TestCase(typeof(Acme.ValueType), nameof(Acme.ValueType.M1), ExpectedResult = "M:Acme.ValueType.M1(System.Int32)")]
        [TestCase(typeof(Acme.ValueType), nameof(Acme.ValueType.M2), ExpectedResult = "M:Acme.ValueType.M2(System.Nullable{System.Int32})")]
        [TestCase(typeof(Acme.Widget.NestedClass), nameof(Acme.Widget.NestedClass.M1), ExpectedResult = "M:Acme.Widget.NestedClass.M1")]
        [TestCase(typeof(Acme.Widget.NestedClass), nameof(Acme.Widget.NestedClass.M2), ExpectedResult = "M:Acme.Widget.NestedClass.M2(System.Int32)")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M0), ExpectedResult = "M:Acme.Widget.M0")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M1), ExpectedResult = "M:Acme.Widget.M1(System.Char,System.Single@,Acme.ValueType@,System.Int32@)")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M2), ExpectedResult = "M:Acme.Widget.M2(System.Int16[],System.Int32[0:,0:],System.Int64[][])")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M3), ExpectedResult = "M:Acme.Widget.M3(System.Int64[][],Acme.Widget[0:,0:,0:][])")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M4), ExpectedResult = "M:Acme.Widget.M4(System.Char*,Acme.Widget.Direction**)")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M5), ExpectedResult = "M:Acme.Widget.M5(System.Void*,System.Nullable{System.Double}*[0:,0:,0:][0:,0:][])")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M6), ExpectedResult = "M:Acme.Widget.M6(System.Int32,System.Object[])")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M7), ExpectedResult = "M:Acme.Widget.M7``1")]
        [TestCase(typeof(Acme.MyList<>), nameof(Acme.MyList<int>.Test), ExpectedResult = "M:Acme.MyList`1.Test(`0)")]
        [TestCase(typeof(Acme.MyList<>), nameof(Acme.MyList<int>.EndScope), ExpectedResult = "M:Acme.MyList`1.EndScope(Acme.MyList{`0}.Scope)")]
        [TestCase(typeof(Acme.MyList<>.Scope), "op_Implicit", ExpectedResult = "M:Acme.MyList`1.Scope.op_Implicit(Acme.MyList{`0}.Scope)~System.Collections.Generic.List{`0}")]
        [TestCase(typeof(Acme.UseList), nameof(Acme.UseList.ProcessAsync), ExpectedResult = "M:Acme.UseList.ProcessAsync(Acme.MyList{System.Int32})")]
        [TestCase(typeof(Acme.UseList), nameof(Acme.UseList.GetValues), ExpectedResult = "M:Acme.UseList.GetValues``1(``0)")]
        [TestCase(typeof(Acme.UseList), nameof(Acme.UseList.Intercept), ExpectedResult = "M:Acme.UseList.Intercept``1(Acme.MyList{``0}.Helper{System.Char,System.String}[]@)")]
        [TestCase(typeof(Acme.UseList), "Acme.IProcess<System.String>.GetStatus", ExpectedResult = "M:Acme.UseList.Acme#IProcess{System#String}#GetStatus(System.Boolean)")]
        public string CodeReference_HasExpectedValue(Type declaringType, string methodName)
        {
            var methodInfo = declaringType.GetMethod(methodName, bindingFlags);
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CodeReference;
        }
    }
}

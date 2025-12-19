// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.Linq;

    [TestFixture]
    public class PropertyTests
    {
        [TestCase(typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty")]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.VirtualProperty))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.InitOnlyProperty))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RequiredProperty))]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(int))]
        public void ImplementsProperty(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IProperty>());
            Assert.That(metadata.Name, Is.EqualTo(propertyName));
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.VirtualProperty), ExpectedResult = nameof(Int32))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty), ExpectedResult = nameof(String))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = nameof(Int32))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty), ExpectedResult = nameof(Int32))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty), ExpectedResult = nameof(Int32))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.InitOnlyProperty), ExpectedResult = nameof(Int32))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RequiredProperty), ExpectedResult = nameof(Int32))]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(int), ExpectedResult = nameof(Int32))]
        public string PropertyType_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Type.Name;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(int), ExpectedResult = true)]
        public bool IsIndexer_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsIndexer;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.InitOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct), nameof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct.Property), ExpectedResult = true)]
        public bool IsReadOnly_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsReadOnly;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.InitOnlyProperty), ExpectedResult = true)]
        public bool IsInitOnly_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsInitOnly;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.InitOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RequiredProperty), ExpectedResult = true)]
        public bool IsRequired_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsRequired;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.VirtualProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.InitOnlyProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(int), ExpectedResult = true)]
        public bool CanRead_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CanRead;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.VirtualProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.InitOnlyProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(int), ExpectedResult = false)]
        public bool CanWrite_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CanWrite;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.VirtualProperty))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty))]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(int))]
        public void GetMethod_ForReadableIsNotNull(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.GetMethod, Is.InstanceOf<IMethod>());
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty))]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty))]
        public void SetMethod_ForWritableIsNotNull(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.SetMethod, Is.InstanceOf<IMethod>());
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty))]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(int))]
        public void SetMethod_ForReadOnlyIsNull(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.SetMethod, Is.Null);
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty), ExpectedResult = 1)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = 2)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty), ExpectedResult = 2)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty), ExpectedResult = 1)]
        public int GetAccessors_ReturnsExpectedCount(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.GetAccessors().Count();
        }

        [TestCase(typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty", ExpectedResult = MemberVisibility.Private)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.VirtualProperty), ExpectedResult = MemberVisibility.ProtectedInternal)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty), ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty), ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty), ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.InternalProperty), ExpectedResult = MemberVisibility.Internal)]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(int), ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty", ExpectedResult = MemberVisibility.Private)]
        public MemberVisibility Visibility_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Visibility;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.VirtualProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(int), ExpectedResult = false)]
        public bool IsStatic_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsStatic;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.VirtualProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.InternalProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(int), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty", ExpectedResult = false)]
        public bool IsVisible_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsVisible;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.VirtualProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(int), ExpectedResult = false)]
        public bool IsSpecialName_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsSpecialName;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty", ExpectedResult = true)]
        public bool IsExplicitInterfaceImplementation_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsExplicitInterfaceImplementation;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceProperty), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty", ExpectedResult = false)]
        public bool IsDefaultInterfaceImplementation_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsDefaultInterfaceImplementation;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.VirtualProperty))]
        [TestCase(typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty")]
        public void DeclaringType_HasExpectedType(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.DeclaringType, Is.Not.Null);
            Assert.That(metadata.DeclaringType.Name, Is.EqualTo(nameof(Acme.SampleProperties)));
        }

        [TestCase(typeof(Acme.SampleDerivedGenericClass<,,>), nameof(Acme.SampleDerivedGenericClass<,,>.Property), ExpectedResult = MemberVirtuality.Override)]
        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass), nameof(Acme.SampleDerivedConstructedGenericClass.Property), ExpectedResult = MemberVirtuality.SealedOverride)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.AbstractProperty), ExpectedResult = MemberVirtuality.Abstract)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.VirtualProperty), ExpectedResult = MemberVirtuality.Virtual)]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = MemberVirtuality.None)]
        [TestCase(typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty", ExpectedResult = MemberVirtuality.None)]
        public MemberVirtuality Virtuality_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Virtuality;
        }

        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass), nameof(Acme.SampleDerivedConstructedGenericClass.Property), ExpectedResult = "SampleDerivedGenericClass`3")]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = null)]
        public string? OverriddenProperty_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.OverriddenProperty?.DeclaringType.Name;
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = null)]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceProperty), ExpectedResult = null)]
        [TestCase(typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty", ExpectedResult = nameof(Acme.ISampleInterface))]
        public string? InterfaceProperty_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.ImplementedProperty?.DeclaringType.Name;
        }

        [TestCase(typeof(Acme.SampleProperties), typeof(int))]
        [TestCase(typeof(Acme.SampleProperties), typeof(string), typeof(int))]
        public void Indexer_HasCorrectParameterCount(Type declaringType, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty("Item", Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsIndexer, Is.True);
                Assert.That(metadata.Parameters.Select(static p => p.Type.Name), Is.EqualTo(parameterTypes.Select(static t => t.Name)));
            }
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty))]
        public void NonIndexer_HasNoParameters(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsIndexer, Is.False);
                Assert.That(metadata.Parameters, Is.Empty);
            }
        }

        [TestCase(typeof(Acme.SampleProperties), ExpectedResult = 1)]
        [TestCase(typeof(Acme.ISampleExtendedConstructedGenericInterface), ExpectedResult = 0)]
        public int Overloads_HasExpectedCount(Type declaringType)
        {
            var propertyInfo = declaringType.GetMember("Item", Acme.Bindings.AllDeclared).FirstOrDefault();
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata() as IProperty;
            Assert.That(metadata, Is.Not.Null);

            return metadata.Overloads.Count();
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = "P:Acme.SampleProperties.RegularProperty")]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(int), ExpectedResult = "P:Acme.SampleProperties.Item(System.Int32)")]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(string), typeof(int), ExpectedResult = "P:Acme.SampleProperties.Item(System.String,System.Int32)")]
        [TestCase(typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty", ExpectedResult = "P:Acme.SampleProperties.Acme#ISampleInterface#InterfaceProperty")]
        public string CodeReference_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CodeReference;
        }
    }
}

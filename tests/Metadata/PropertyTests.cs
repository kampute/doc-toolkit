// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    [TestFixture]
    public class PropertyTests
    {
        private readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        [TestCase("VirtualProperty")]
        [TestCase("Item")]
        [TestCase("StaticProperty")]
        public void ImplementsProperty(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IProperty>());
            Assert.That(metadata.Name, Is.EqualTo(propertyName));
        }

        [TestCase("VirtualProperty", ExpectedResult = "String")]
        [TestCase("Item", ExpectedResult = "Int32")]
        [TestCase("StaticProperty", ExpectedResult = "String")]
        public string PropertyType_HasExpectedValue(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Type.Name;
        }

        [TestCase("Count", ExpectedResult = false)]
        [TestCase("Item", ExpectedResult = true)]
        public bool IsIndexer_HasExpectedValue(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsIndexer;
        }

        [TestCase("HasTotal", ExpectedResult = true)]
        [TestCase("HalfTotal", ExpectedResult = true)]
        public bool IsReadOnly_HasExpectedValue(string propertyName)
        {
            var propertyInfo = typeof(Acme.ValueType).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsReadOnly;
        }

        [TestCase("InitOnlyProperty", ExpectedResult = true)]
        [TestCase("RequiredProperty", ExpectedResult = false)]
        [TestCase("Count", ExpectedResult = false)]
        public bool IsInitOnly_HasExpectedValue(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsInitOnly;
        }

        [TestCase("InitOnlyProperty", ExpectedResult = false)]
        [TestCase("RequiredProperty", ExpectedResult = true)]
        [TestCase("Count", ExpectedResult = false)]
        public bool IsRequired_HasExpectedValue(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsRequired;
        }

        [TestCase("VirtualProperty", ExpectedResult = true)]
        [TestCase("Count", ExpectedResult = true)]
        [TestCase("Item", ExpectedResult = true)]
        [TestCase("InitOnlyProperty", ExpectedResult = true)]
        [TestCase("StaticProperty", ExpectedResult = true)]
        public bool CanRead_HasExpectedValue(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CanRead;
        }

        [TestCase("VirtualProperty", ExpectedResult = true)]
        [TestCase("Count", ExpectedResult = false)]
        [TestCase("Item", ExpectedResult = false)]
        [TestCase("InitOnlyProperty", ExpectedResult = true)]
        [TestCase("StaticProperty", ExpectedResult = true)]
        public bool CanWrite_HasExpectedValue(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CanWrite;
        }

        [TestCase("VirtualProperty")]
        [TestCase("Count")]
        [TestCase("Item")]
        [TestCase("InitOnlyProperty")]
        [TestCase("StaticProperty")]
        public void GetMethod_ForReadableIsNotNull(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.GetMethod, Is.InstanceOf<IMethod>());
        }

        [TestCase("VirtualProperty")]
        [TestCase("InitOnlyProperty")]
        [TestCase("StaticProperty")]
        public void SetMethod_ForWritableIsNotNull(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.SetMethod, Is.InstanceOf<IMethod>());
        }

        [TestCase("Count")]
        [TestCase("Item")]
        public void SetMethod_ForReadOnlyIsNull(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.SetMethod, Is.Null);
        }

        [TestCase("Count", ExpectedResult = 1)]
        [TestCase("VirtualProperty", ExpectedResult = 2)]
        [TestCase("InitOnlyProperty", ExpectedResult = 2)]
        [TestCase("StaticProperty", ExpectedResult = 2)]
        public int GetAccessors_ReturnsExpectedCount(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.GetAccessors().Count();
        }

        [TestCase("VirtualProperty", ExpectedResult = MemberVisibility.Public)]
        [TestCase("Count", ExpectedResult = MemberVisibility.Public)]
        [TestCase("Item", ExpectedResult = MemberVisibility.Public)]
        [TestCase("InitOnlyProperty", ExpectedResult = MemberVisibility.Public)]
        [TestCase("StaticProperty", ExpectedResult = MemberVisibility.Public)]
        [TestCase("Acme.IProcess<System.String>.IsCompleted", ExpectedResult = MemberVisibility.Private)]
        public MemberVisibility Visibility_HasExpectedValue(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Visibility;
        }

        [TestCase("VirtualProperty", ExpectedResult = false)]
        [TestCase("Count", ExpectedResult = false)]
        [TestCase("Item", ExpectedResult = false)]
        [TestCase("InitOnlyProperty", ExpectedResult = false)]
        [TestCase("StaticProperty", ExpectedResult = true)]
        public bool IsStatic_HasExpectedValue(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsStatic;
        }

        [TestCase("VirtualProperty", ExpectedResult = true)]
        [TestCase("Count", ExpectedResult = true)]
        [TestCase("Item", ExpectedResult = true)]
        [TestCase("InitOnlyProperty", ExpectedResult = true)]
        [TestCase("StaticProperty", ExpectedResult = true)]
        [TestCase("Acme.IProcess<System.String>.IsCompleted", ExpectedResult = false)]
        public bool IsVisible_HasExpectedValue(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsVisible;
        }

        [TestCase("VirtualProperty", ExpectedResult = false)]
        [TestCase("Count", ExpectedResult = false)]
        [TestCase("Item", ExpectedResult = false)] // Indexers are not special name properties!
        [TestCase("InitOnlyProperty", ExpectedResult = false)]
        [TestCase("StaticProperty", ExpectedResult = false)]
        public bool IsSpecialName_HasExpectedValue(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsSpecialName;
        }

        [TestCase("Count", ExpectedResult = false)]
        [TestCase("Acme.IProcess<System.String>.IsCompleted", ExpectedResult = true)]
        public bool IsExplicitInterfaceImplementation_HasExpectedValue(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsExplicitInterfaceImplementation;
        }

        [TestCase("Count", ExpectedResult = false)]
        [TestCase("IsEmpty", ExpectedResult = true)]
        public bool IsDefaultInterfaceImplementation_HasExpectedValue(string propertyName)
        {
            var propertyInfo = typeof(Acme.ITestInterface).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsDefaultInterfaceImplementation;
        }

        [TestCase("VirtualProperty")]
        public void DeclaringType_HasExpectedType(string propertyName)
        {
            var propertyInfo = typeof(Acme.DerivedClass).GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.DeclaringType, Is.Not.Null);
            Assert.That(metadata.DeclaringType.Name, Is.EqualTo(nameof(Acme.DerivedClass)));
        }

        [TestCase("VirtualProperty", typeof(Acme.BaseClass), ExpectedResult = MemberVirtuality.Virtual)]
        [TestCase("VirtualProperty", typeof(Acme.DerivedClass), ExpectedResult = MemberVirtuality.SealedOverride)]
        [TestCase("VirtualReadOnly", typeof(Acme.DerivedClass), ExpectedResult = MemberVirtuality.Override)]
        [TestCase("StaticProperty", typeof(Acme.DerivedClass), ExpectedResult = MemberVirtuality.None)]
        [TestCase("Acme.IProcess<System.String>.IsCompleted", typeof(Acme.DerivedClass), ExpectedResult = MemberVirtuality.None)]
        public MemberVirtuality Virtuality_HasExpectedValue(string propertyName, Type declaringType)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Virtuality;
        }

        [TestCase("VirtualProperty", typeof(Acme.DerivedClass), ExpectedResult = nameof(Acme.BaseClass))]
        [TestCase("RegularProperty", typeof(Acme.DerivedClass), ExpectedResult = null)]
        [TestCase("Value", typeof(TestTypes.GenericDerivedClass<>), ExpectedResult = "GenericBaseClass`1")]
        [TestCase("Value", typeof(TestTypes.GenericDerivedClass<int>), ExpectedResult = "GenericBaseClass`1")]
        [TestCase("Value", typeof(TestTypes.ConstructedGenericDerivedClass), ExpectedResult = "GenericBaseClass`1")]
        //[TestCase("Value", typeof(TestTypes.DrivedFromConstructedGeneric), ExpectedResult = "ConstructedGenericDerivedClass")]
        public string? OverriddenProperty_HasExpectedValue(string propertyName, Type declaringType)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.OverriddenProperty?.DeclaringType.Name;
        }

        [TestCase("Acme.IProcess<System.String>.IsCompleted", typeof(Acme.DerivedClass), ExpectedResult = "IProcess`1")]
        [TestCase("RegularProperty", typeof(Acme.DerivedClass), ExpectedResult = null)]
        public string? InterfaceProperty_HasExpectedValue(string propertyName, Type declaringType)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.ImplementedProperty?.DeclaringType.Name;
        }

        [Test]
        public void Indexer_HasCorrectParameterCount()
        {
            var propertyInfo = typeof(Acme.Widget).GetProperty("Item", bindingFlags, null, typeof(int), [typeof(string), typeof(int)], null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsIndexer, Is.True);
                Assert.That(metadata.Parameters, Has.Count.EqualTo(2));
            }
            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.Parameters[0].Type.Name, Is.EqualTo("String"));
                Assert.That(metadata.Parameters[1].Type.Name, Is.EqualTo("Int32"));
            }
        }

        [Test]
        public void NonIndexer_HasNoParameters()
        {
            var propertyInfo = typeof(Acme.Widget).GetProperty("Width", bindingFlags);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsIndexer, Is.False);
                Assert.That(metadata.Parameters, Is.Empty);
            }
        }

        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.Width), ExpectedResult = "P:Acme.Widget.Width")]
        [TestCase(typeof(Acme.Widget), "Item", new[] { typeof(int) }, ExpectedResult = "P:Acme.Widget.Item(System.Int32)")]
        [TestCase(typeof(Acme.Widget), "Item", new[] { typeof(string), typeof(int) }, ExpectedResult = "P:Acme.Widget.Item(System.String,System.Int32)")]
        [TestCase(typeof(Acme.UseList), "Acme.IProcess<System.String>.IsCompleted", ExpectedResult = "P:Acme.UseList.Acme#IProcess{System#String}#IsCompleted")]
        [TestCase(typeof(Dictionary<,>), "System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey,TValue>>.IsReadOnly", ExpectedResult = "P:System.Collections.Generic.Dictionary`2.System#Collections#Generic#ICollection{System#Collections#Generic#KeyValuePair{TKey,TValue}}#IsReadOnly")]
        public string CodeReference_HasExpectedValue(Type declaringType, string propertyName, params Type[] parameterTypes)
        {
            var propertyInfo = declaringType.GetProperty(propertyName, bindingFlags, null, null, parameterTypes, null);
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CodeReference;
        }
    }
}

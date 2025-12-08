// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class EventTests
    {
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.VirtualEvent))]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent))]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent))]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent")]
        public void ImplementsEvent(Type declaringType, string eventName)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.InstanceOf<IEvent>());
            Assert.That(metadata.Name, Is.EqualTo(eventName));
        }

        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.VirtualEvent), ExpectedResult = "EventHandler")]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = "EventHandler")]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent), ExpectedResult = "EventHandler")]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = "EventHandler")]
        public string EventHandlerType_HasExpectedValue(Type declaringType, string eventName)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Type.Name;
        }

        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.VirtualEvent))]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent))]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent))]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent")]
        public void AddMethod_IsNotNull(Type declaringType, string eventName)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.AddMethod, Is.InstanceOf<IMethod>());
        }

        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.VirtualEvent))]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent))]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent))]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent")]
        public void RemoveMethod_IsNotNull(Type declaringType, string eventName)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.RemoveMethod, Is.InstanceOf<IMethod>());
        }

        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.VirtualEvent), ExpectedResult = MemberVisibility.ProtectedInternal)]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent), ExpectedResult = MemberVisibility.Public)]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = MemberVisibility.Private)]
        public MemberVisibility Visibility_HasExpectedValue(Type declaringType, string eventName)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Visibility;
        }

        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.VirtualEvent), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = false)]
        public bool IsStatic_HasExpectedValue(Type declaringType, string eventName)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsStatic;
        }

        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.VirtualEvent), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent), ExpectedResult = true)]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = false)]
        public bool IsVisible_HasExpectedValue(Type declaringType, string eventName)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsVisible;
        }

        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.VirtualEvent), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = false)]
        public bool IsSpecialName_HasExpectedValue(Type declaringType, string eventName)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsSpecialName;
        }

        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = false)]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceEvent), ExpectedResult = false)]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = true)]
        public bool IsExplicitInterfaceImplementation_HasExpectedValue(Type declaringType, string eventName)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsExplicitInterfaceImplementation;
        }

        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent))]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent")]
        public void DeclaringType_HasExpectedType(Type declaringType, string eventName)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.DeclaringType.Represents(declaringType), Is.True);
        }

        [TestCase(typeof(Acme.SampleDerivedGenericClass<,,>), nameof(Acme.SampleDerivedConstructedGenericClass.Event), ExpectedResult = MemberVirtuality.Override)]
        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass), nameof(Acme.SampleDerivedConstructedGenericClass.Event), ExpectedResult = MemberVirtuality.SealedOverride)]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.AbstractEvent), ExpectedResult = MemberVirtuality.Abstract)]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.VirtualEvent), ExpectedResult = MemberVirtuality.Virtual)]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = MemberVirtuality.None)]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = MemberVirtuality.None)]
        public MemberVirtuality Virtuality_HasExpectedValue(Type declaringType, string eventName)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Virtuality;
        }

        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass), nameof(Acme.SampleDerivedConstructedGenericClass.Event), typeof(Acme.SampleDerivedGenericClass<,,>))]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), null)]
        public void OverriddenEvent_HasExpectedValue(Type declaringType, string eventName, Type? expectedBaseType)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.OverriddenEvent?.DeclaringType, Is.EqualTo(expectedBaseType?.GetMetadata()));
        }

        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), null)]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceEvent), null)]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", typeof(Acme.ISampleInterface))]
        public void InterfaceEvent_HasExpectedValue(Type declaringType, string eventName, Type? expectedInterfaceType)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.ImplementedEvent?.DeclaringType, Is.EqualTo(expectedInterfaceType?.GetMetadata()));
        }

        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = "E:Acme.SampleEvents.RegularEvent")]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = "E:Acme.SampleEvents.Acme#ISampleInterface#InterfaceEvent")]
        public string CodeReference_HasExpectedValue(Type declaringType, string eventName)
        {
            var eventInfo = declaringType.GetEvent(eventName, Acme.Bindings.AllDeclared);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CodeReference;
        }
    }
}

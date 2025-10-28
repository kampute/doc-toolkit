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
    public class EventTests
    {
        private readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        [TestCase("VirtualEvent")]
        [TestCase("RegularEvent")]
        [TestCase("InterfaceEvent")]
        [TestCase("StaticEvent")]
        public void ImplementsEvent(string eventName)
        {
            var eventInfo = typeof(Acme.DerivedClass).GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.InstanceOf<IEvent>());
            Assert.That(metadata.Name, Is.EqualTo(eventName));
        }

        [TestCase("VirtualEvent", ExpectedResult = "EventHandler")]
        [TestCase("RegularEvent", ExpectedResult = "Del")]
        [TestCase("InterfaceEvent", ExpectedResult = "EventHandler")]
        [TestCase("StaticEvent", ExpectedResult = "EventHandler`1")]
        public string EventHandlerType_HasExpectedValue(string eventName)
        {
            var eventInfo = typeof(Acme.DerivedClass).GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Type.Name;
        }

        [TestCase("VirtualEvent")]
        [TestCase("RegularEvent")]
        [TestCase("InterfaceEvent")]
        [TestCase("StaticEvent")]
        public void AddMethod_IsNotNull(string eventName)
        {
            var eventInfo = typeof(Acme.DerivedClass).GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.AddMethod, Is.InstanceOf<IMethod>());
        }

        [TestCase("VirtualEvent")]
        [TestCase("RegularEvent")]
        [TestCase("InterfaceEvent")]
        [TestCase("StaticEvent")]
        public void RemoveMethod_IsNotNull(string eventName)
        {
            var eventInfo = typeof(Acme.DerivedClass).GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.RemoveMethod, Is.InstanceOf<IMethod>());
        }

        [TestCase("VirtualEvent", ExpectedResult = MemberVisibility.Protected)]
        [TestCase("RegularEvent", ExpectedResult = MemberVisibility.Internal)]
        [TestCase("StaticEvent", ExpectedResult = MemberVisibility.Public)]
        [TestCase("Acme.IProcess<System.String>.Completed", ExpectedResult = MemberVisibility.Private)]
        public MemberVisibility Visibility_HasExpectedValue(string eventName)
        {
            var eventInfo = typeof(Acme.DerivedClass).GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Visibility;
        }

        [TestCase("VirtualEvent", ExpectedResult = false)]
        [TestCase("RegularEvent", ExpectedResult = false)]
        [TestCase("InterfaceEvent", ExpectedResult = false)]
        [TestCase("StaticEvent", ExpectedResult = true)]
        public bool IsStatic_HasExpectedValue(string eventName)
        {
            var eventInfo = typeof(Acme.DerivedClass).GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsStatic;
        }

        [TestCase("VirtualEvent", ExpectedResult = true)]
        [TestCase("RegularEvent", ExpectedResult = false)]
        [TestCase("StaticEvent", ExpectedResult = true)]
        [TestCase("InterfaceEvent", ExpectedResult = true)]
        [TestCase("Acme.IProcess<System.String>.Completed", ExpectedResult = false)]
        public bool IsVisible_HasExpectedValue(string eventName)
        {
            var eventInfo = typeof(Acme.DerivedClass).GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsVisible;
        }

        [TestCase("VirtualEvent", ExpectedResult = false)]
        [TestCase("RegularEvent", ExpectedResult = false)]
        [TestCase("InterfaceEvent", ExpectedResult = false)]
        [TestCase("StaticEvent", ExpectedResult = false)]
        public bool IsSpecialName_HasExpectedValue(string eventName)
        {
            var eventInfo = typeof(Acme.DerivedClass).GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsSpecialName;
        }

        [TestCase("InterfaceEvent", ExpectedResult = false)]
        [TestCase("Acme.IProcess<System.String>.Completed", ExpectedResult = true)]
        public bool IsExplicitInterfaceImplementation_HasExpectedValue(string eventName)
        {
            var eventInfo = typeof(Acme.DerivedClass).GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.IsExplicitInterfaceImplementation;
        }

        [TestCase("RegularEvent", typeof(Acme.DerivedClass))]
        [TestCase("Acme.IProcess<System.String>.Completed", typeof(Acme.DerivedClass))]
        public void DeclaringType_HasExpectedType(string eventName, Type declaringType)
        {
            var eventInfo = declaringType.GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            Assert.That(metadata.DeclaringType.Represents(declaringType), Is.True);
        }

        [TestCase("VirtualEvent", typeof(Acme.BaseClass), ExpectedResult = MemberVirtuality.Virtual)]
        [TestCase("VirtualEvent", typeof(Acme.DerivedClass), ExpectedResult = MemberVirtuality.Override)]
        [TestCase("RegularEvent", typeof(Acme.DerivedClass), ExpectedResult = MemberVirtuality.None)]
        [TestCase("StaticEvent", typeof(Acme.DerivedClass), ExpectedResult = MemberVirtuality.None)]
        [TestCase("InterfaceEvent", typeof(Acme.DerivedClass), ExpectedResult = MemberVirtuality.None)]
        [TestCase("Acme.IProcess<System.String>.Completed", typeof(Acme.DerivedClass), ExpectedResult = MemberVirtuality.None)]
        public MemberVirtuality Virtuality_HasExpectedValue(string eventName, Type declaringType)
        {
            var eventInfo = declaringType.GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.Virtuality;
        }

        [TestCase("VirtualEvent", typeof(Acme.DerivedClass), ExpectedResult = "BaseClass")]
        [TestCase("RegularEvent", typeof(Acme.DerivedClass), ExpectedResult = null)]
        public string? OverriddenEvent_HasExpectedValue(string eventName, Type declaringType)
        {
            var eventInfo = declaringType.GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.OverriddenEvent?.DeclaringType.Name;
        }

        [TestCase("Acme.IProcess<System.String>.Completed", typeof(Acme.DerivedClass), ExpectedResult = "IProcess`1")]
        [TestCase("RegularEvent", typeof(Acme.DerivedClass), ExpectedResult = null)]
        public string? InterfaceEvent_HasExpectedValue(string eventName, Type declaringType)
        {
            var eventInfo = declaringType.GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.ImplementedEvent?.DeclaringType.Name;
        }

        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.AnEvent), ExpectedResult = "E:Acme.Widget.AnEvent")]
        [TestCase(typeof(Acme.UseList), "Acme.IProcess<System.String>.Completed", ExpectedResult = "E:Acme.UseList.Acme#IProcess{System#String}#Completed")]
        public string CodeReference_HasExpectedValue(Type declaringType, string eventName)
        {
            var eventInfo = declaringType.GetEvent(eventName, bindingFlags);
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();
            Assert.That(metadata, Is.Not.Null);

            return metadata.CodeReference;
        }
    }
}

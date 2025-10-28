// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class MetadataExtensionsTests
    {
        [Test]
        public void GetInheritedMember_WithNullMember_ThrowsArgumentNullException()
        {
            Assert.That(() => default(IMember)!.GetInheritedMember(), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("member"));
        }

        [Test]
        public void GetInheritedMember_WithRootType_ReturnsNull()
        {
            var objectType = typeof(object).GetMetadata();
            Assert.That(objectType, Is.Not.Null);

            var inheritedMember = objectType.GetInheritedMember();
            Assert.That(inheritedMember, Is.Null);
        }

        [Test]
        public void GetInheritedMember_WithRegularType_ReturnsBaseType()
        {
            var baseType = typeof(TestTypes.TestBaseClass).GetMetadata();
            Assert.That(baseType, Is.Not.Null);

            var objectType = typeof(object).GetMetadata();
            Assert.That(objectType, Is.Not.Null);

            var inheritedMember = baseType.GetInheritedMember();
            Assert.That(inheritedMember, Is.EqualTo(objectType));
        }

        [Test]
        public void GetInheritedMember_WithDerivedGenericType_ReturnsBaseType()
        {
            var genericType = typeof(TestTypes.GenericDerivedClass<>).GetMetadata();
            Assert.That(genericType, Is.Not.Null);

            var baseType = typeof(TestTypes.GenericBaseClass<>).GetMetadata();
            Assert.That(baseType, Is.Not.Null);

            var inheritedMember = genericType.GetInheritedMember();
            Assert.That(inheritedMember, Is.EqualTo(baseType));
        }

        [Test]
        public void GetInheritedMember_WithConstructedGenericType_ReturnsGenericTypeDefinition()
        {
            var constructedType = typeof(TestTypes.GenericBaseClass<int>).GetMetadata();
            Assert.That(constructedType, Is.Not.Null);

            var genericTypeDefinition = typeof(TestTypes.GenericBaseClass<>).GetMetadata();
            Assert.That(genericTypeDefinition, Is.InstanceOf<IGenericCapableType>());

            var inheritedMember = constructedType.GetInheritedMember();
            Assert.That(inheritedMember, Is.EqualTo(genericTypeDefinition));
        }

        [Test]
        public void GetInheritedMember_WithConstructor_ReturnsBaseConstructor()
        {
            var baseType = typeof(TestTypes.TestBaseClass).GetMetadata<IClassType>();
            Assert.That(baseType, Is.Not.Null);

            var baseConstructor = baseType.Constructors.FirstOrDefault(c => c.Parameters.Count == 1);
            Assert.That(baseConstructor, Is.InstanceOf<IConstructor>());

            var derivedType = typeof(TestTypes.TestDerivedClass).GetMetadata<IClassType>();
            Assert.That(derivedType, Is.Not.Null);

            var derivedConstructor = derivedType.Constructors.FirstOrDefault(c => c.Parameters.Count == 1);
            Assert.That(derivedConstructor, Is.InstanceOf<IConstructor>());

            var inheritedMember = derivedConstructor.GetInheritedMember();
            Assert.That(inheritedMember, Is.EqualTo(baseConstructor));
        }

        [Test]
        public void GetInheritedMember_WithConstructorNoBaseMatch_ReturnsNull()
        {
            var derivedType = typeof(TestTypes.TestDerivedClass).GetMetadata<IClassType>();
            Assert.That(derivedType, Is.Not.Null);

            var derivedConstructor = derivedType.Constructors.FirstOrDefault(c => c.Parameters.Count == 0);
            Assert.That(derivedConstructor, Is.InstanceOf<IConstructor>());

            var inheritedMember = derivedConstructor.GetInheritedMember();
            Assert.That(inheritedMember, Is.Null);
        }

        [Test]
        public void GetInheritedMember_WithOverriddenMethod_ReturnsOverriddenMember()
        {
            var baseType = typeof(TestTypes.TestBaseClass).GetMetadata<IClassType>();
            Assert.That(baseType, Is.Not.Null);

            var baseMethod = baseType.Methods.FirstOrDefault(m => m.Name == nameof(TestTypes.TestBaseClass.VirtualTestMethod));
            Assert.That(baseMethod, Is.InstanceOf<IMethod>());

            var derivedType = typeof(TestTypes.TestDerivedClass).GetMetadata<IClassType>();
            Assert.That(derivedType, Is.Not.Null);

            var overriddenMethod = derivedType.Methods.FirstOrDefault(m => m.Name == nameof(TestTypes.TestDerivedClass.VirtualTestMethod));
            Assert.That(overriddenMethod, Is.InstanceOf<IMethod>());

            var inheritedMember = overriddenMethod.GetInheritedMember();

            Assert.That(inheritedMember, Is.EqualTo(baseMethod));
        }

        [Test]
        public void GetInheritedMember_WithOverriddenGenericMethod_ReturnsOverriddenMember()
        {
            var baseType = typeof(TestTypes.TestBaseClass).GetMetadata<IClassType>();
            Assert.That(baseType, Is.Not.Null);

            var baseMethod = baseType.Methods.FirstOrDefault(m => m.Name == nameof(TestTypes.TestBaseClass.VirtualGenericMethod));
            Assert.That(baseMethod, Is.InstanceOf<IMethod>());

            var derivedType = typeof(TestTypes.TestDerivedClass).GetMetadata<IClassType>();
            Assert.That(derivedType, Is.Not.Null);

            var overriddenMethod = derivedType.Methods.FirstOrDefault(m => m.Name == nameof(TestTypes.TestDerivedClass.VirtualGenericMethod));
            Assert.That(overriddenMethod, Is.InstanceOf<IMethod>());

            var inheritedMember = overriddenMethod.GetInheritedMember();

            Assert.That(inheritedMember, Is.EqualTo(baseMethod));
        }

        [Test]
        public void GetInheritedMember_WithInterfaceMethod_ReturnsInterfaceMember()
        {
            var interfaceType = typeof(TestTypes.ITestInterface).GetMetadata<IInterfaceType>();
            Assert.That(interfaceType, Is.Not.Null);

            var interfaceMethod = interfaceType.Methods.FirstOrDefault(m => m.Name == nameof(TestTypes.ITestInterface.InterfaceTestMethod));
            Assert.That(interfaceMethod, Is.InstanceOf<IMethod>());

            var implementingType = typeof(TestTypes.TestBaseClass).GetMetadata<IClassType>();
            Assert.That(implementingType, Is.Not.Null);

            var implementedMethod = implementingType.Methods.FirstOrDefault(m => m.Name == nameof(TestTypes.TestBaseClass.InterfaceTestMethod));
            Assert.That(implementedMethod, Is.InstanceOf<IMethod>());

            var inheritedMember = implementedMethod.GetInheritedMember();

            Assert.That(inheritedMember, Is.EqualTo(interfaceMethod));
        }

        [Test]
        public void GetInheritedMember_WithInterfaceGenericMethod_ReturnsInterfaceMember()
        {
            var interfaceType = typeof(TestTypes.ITestInterface).GetMetadata<IInterfaceType>();
            Assert.That(interfaceType, Is.Not.Null);

            var interfaceMethod = interfaceType.Methods.FirstOrDefault(m => m.Name == nameof(TestTypes.ITestInterface.InterfaceGenericTestMethod));
            Assert.That(interfaceMethod, Is.InstanceOf<IMethod>());

            var implementingType = typeof(TestTypes.TestBaseClass).GetMetadata<IClassType>();
            Assert.That(implementingType, Is.Not.Null);

            var implementedMethod = implementingType.Methods.FirstOrDefault(m => m.Name == nameof(TestTypes.TestBaseClass.InterfaceGenericTestMethod));
            Assert.That(implementedMethod, Is.InstanceOf<IMethod>());

            var inheritedMember = implementedMethod.GetInheritedMember();

            Assert.That(inheritedMember, Is.EqualTo(interfaceMethod));
        }

        [Test]
        public void GetInheritedMember_WithRegularMethod_ReturnsNull()
        {
            var type = typeof(TestTypes.TestDerivedClass).GetMetadata<IClassType>();
            Assert.That(type, Is.Not.Null);

            var regularMethod = type.Methods.FirstOrDefault(m => m.Name == nameof(TestTypes.TestDerivedClass.RegularTestMethod));
            Assert.That(regularMethod, Is.InstanceOf<IMethod>());

            var inheritedMember = regularMethod.GetInheritedMember();
            Assert.That(inheritedMember, Is.Null);
        }

        [Test]
        public void GetInheritedMember_WithOverriddenProperty_ReturnsOverriddenMember()
        {
            var baseType = typeof(TestTypes.TestBaseClass).GetMetadata<IClassType>();
            Assert.That(baseType, Is.Not.Null);

            var baseProperty = baseType.Properties.FirstOrDefault(p => p.Name == nameof(TestTypes.TestBaseClass.VirtualTestProperty));
            Assert.That(baseProperty, Is.InstanceOf<IProperty>());

            var derivedType = typeof(TestTypes.TestDerivedClass).GetMetadata<IClassType>();
            Assert.That(derivedType, Is.Not.Null);

            var overriddenProperty = derivedType.Properties.FirstOrDefault(p => p.Name == nameof(TestTypes.TestDerivedClass.VirtualTestProperty));
            Assert.That(overriddenProperty, Is.InstanceOf<IProperty>());

            var inheritedMember = overriddenProperty.GetInheritedMember();

            Assert.That(inheritedMember, Is.EqualTo(baseProperty));
        }

        [Test]
        public void GetInheritedMember_WithInterfaceProperty_ReturnsInterfaceMember()
        {
            var interfaceType = typeof(TestTypes.ITestInterface).GetMetadata<IInterfaceType>();
            Assert.That(interfaceType, Is.Not.Null);

            var interfaceProperty = interfaceType.Properties.FirstOrDefault(p => p.Name == nameof(TestTypes.ITestInterface.InterfaceTestProperty));
            Assert.That(interfaceProperty, Is.InstanceOf<IProperty>());

            var implementingType = typeof(TestTypes.TestBaseClass).GetMetadata<IClassType>();
            Assert.That(implementingType, Is.Not.Null);

            var implementedProperty = implementingType.Properties.FirstOrDefault(p => p.Name == nameof(TestTypes.TestBaseClass.InterfaceTestProperty));
            Assert.That(implementedProperty, Is.InstanceOf<IProperty>());

            var inheritedMember = implementedProperty.GetInheritedMember();

            Assert.That(inheritedMember, Is.EqualTo(interfaceProperty));
        }

        [Test]
        public void GetInheritedMember_WithRegularProperty_ReturnsNull()
        {
            var type = typeof(TestTypes.TestDerivedClass).GetMetadata<IClassType>();
            Assert.That(type, Is.Not.Null);

            var regularProperty = type.Properties.FirstOrDefault(p => p.Name == nameof(TestTypes.TestDerivedClass.RegularTestProperty));
            Assert.That(regularProperty, Is.InstanceOf<IProperty>());

            var inheritedMember = regularProperty.GetInheritedMember();
            Assert.That(inheritedMember, Is.Null);
        }

        [Test]
        public void GetInheritedMember_WithOverriddenEvent_ReturnsOverriddenMember()
        {
            var baseType = typeof(TestTypes.TestBaseClass).GetMetadata<IClassType>();
            Assert.That(baseType, Is.Not.Null);

            var baseEvent = baseType.Events.FirstOrDefault(e => e.Name == nameof(TestTypes.TestBaseClass.VirtualTestEvent));
            Assert.That(baseEvent, Is.InstanceOf<IEvent>());

            var derivedType = typeof(TestTypes.TestDerivedClass).GetMetadata<IClassType>();
            Assert.That(derivedType, Is.Not.Null);

            var overriddenEvent = derivedType.Events.FirstOrDefault(e => e.Name == nameof(TestTypes.TestDerivedClass.VirtualTestEvent));
            Assert.That(overriddenEvent, Is.InstanceOf<IEvent>());

            var inheritedMember = overriddenEvent.GetInheritedMember();

            Assert.That(inheritedMember, Is.EqualTo(baseEvent));
        }

        [Test]
        public void GetInheritedMember_WithInterfaceEvent_ReturnsInterfaceMember()
        {
            var interfaceType = typeof(TestTypes.ITestInterface).GetMetadata<IInterfaceType>();
            Assert.That(interfaceType, Is.Not.Null);

            var interfaceEvent = interfaceType.Events.FirstOrDefault(e => e.Name == nameof(TestTypes.ITestInterface.InterfaceTestEvent));
            Assert.That(interfaceEvent, Is.InstanceOf<IEvent>());

            var implementingType = typeof(TestTypes.TestBaseClass).GetMetadata<IClassType>();
            Assert.That(implementingType, Is.Not.Null);

            var implementedEvent = implementingType.Events.FirstOrDefault(e => e.Name == nameof(TestTypes.TestBaseClass.InterfaceTestEvent));
            Assert.That(implementedEvent, Is.InstanceOf<IEvent>());

            var inheritedMember = implementedEvent.GetInheritedMember();

            Assert.That(inheritedMember, Is.EqualTo(interfaceEvent));
        }

        [Test]
        public void GetInheritedMember_WithRegularEvent_ReturnsNull()
        {
            var type = typeof(TestTypes.TestDerivedClass).GetMetadata<IClassType>();
            Assert.That(type, Is.Not.Null);

            var regularEvent = type.Events.FirstOrDefault(e => e.Name == nameof(TestTypes.TestDerivedClass.RegularTestEvent));
            Assert.That(regularEvent, Is.InstanceOf<IEvent>());

            var inheritedMember = regularEvent.GetInheritedMember();
            Assert.That(inheritedMember, Is.Null);
        }

        [Test]
        public void GetInheritedMember_WithField_ReturnsNull()
        {
            var type = typeof(TestTypes.TestBaseClass).GetMetadata<IClassType>();
            Assert.That(type, Is.Not.Null);

            var field = type.Fields[0];
            Assert.That(field, Is.InstanceOf<IField>());

            var inheritedMember = field.GetInheritedMember();
            Assert.That(inheritedMember, Is.Null);
        }

        [TestCase("P:Kampute.DocToolkit.Metadata.Adapters.AssemblyAdapter.Name", ExpectedResult = "P:Kampute.DocToolkit.Metadata.Adapters.MetadataAdapter`1.Name")]
        [TestCase("P:Kampute.DocToolkit.Metadata.Adapters.TypeAdapter.IsStatic", ExpectedResult = "P:Kampute.DocToolkit.Metadata.Adapters.MemberAdapter`1.IsStatic")]
        [TestCase("P:Kampute.DocToolkit.Topics.Abstracts.TopicSource.Kampute#DocToolkit#Topics#ITopic#ParentTopic", ExpectedResult = "P:Kampute.DocToolkit.Topics.ITopic.ParentTopic")]
        [TestCase("M:Kampute.DocToolkit.Metadata.Adapters.CustomAttributeAdapter.Represents(System.Reflection.CustomAttributeData)", ExpectedResult = "M:Kampute.DocToolkit.Metadata.IMetadataAdapter`1.Represents(`0)")]
        [TestCase("M:Kampute.DocToolkit.Metadata.Adapters.MetadataAdapter`1.Represents(`0)", ExpectedResult = "M:Kampute.DocToolkit.Metadata.IMetadataAdapter`1.Represents(`0)")]
        [TestCase("M:Kampute.DocToolkit.Metadata.Adapters.MemberAdapter`1.Kampute#DocToolkit#Metadata#IMetadataAdapter{System#Reflection#MemberInfo}#Represents(System.Reflection.MemberInfo)", ExpectedResult = "M:Kampute.DocToolkit.Metadata.IMetadataAdapter`1.Represents(`0)")]
        [TestCase("M:Kampute.DocToolkit.Topics.Abstracts.TopicSource.Render(System.IO.TextWriter,Kampute.DocToolkit.IDocumentationContext)", ExpectedResult = "M:Kampute.DocToolkit.Topics.ITopic.Render(System.IO.TextWriter,Kampute.DocToolkit.IDocumentationContext)")]
        public string? GetMemberDefinition_ReturnsDefinitionOfInheritedMember(string cref)
        {
            MetadataProvider.RegisterRuntimeAssemblies();

            var member = CodeReference.ResolveMember(cref);
            Assert.That(member, Is.Not.Null);

            var inheritedMember = member.GetInheritedMember();
            Assert.That(inheritedMember, Is.Not.Null);

            var memberDefinition = inheritedMember.GetMemberDefinition();
            return memberDefinition?.CodeReference;
        }

        [Test]
        public void GetMembers_WithNullType_ThrowsArgumentNullException()
        {
            Assert.That(() => default(IType)!.GetMembers(), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("type"));
        }

        [Test]
        public void GetMembers_WithClassType_ReturnsAllDeclaredMembers()
        {
            var type = typeof(TestTypes.TestBaseClass).GetMetadata();

            var members = type.GetMembers().ToList();

            Assert.That(members, Has.Some.InstanceOf<IConstructor>());
            Assert.That(members, Has.Some.InstanceOf<IMethod>());
            Assert.That(members, Has.Some.InstanceOf<IProperty>());
            Assert.That(members, Has.Some.InstanceOf<IEvent>());
            Assert.That(members, Has.Some.InstanceOf<IField>());
            Assert.That(members, Has.Some.InstanceOf<IOperator>());
        }

        [Test]
        public void GetMembers_WithStructType_ReturnsDeclaredMembers()
        {
            var type = typeof(TestTypes.TestValueType).GetMetadata();

            var members = type.GetMembers().ToList();

            Assert.That(members, Has.Some.InstanceOf<IConstructor>());
            Assert.That(members, Has.Some.InstanceOf<IMethod>());
            Assert.That(members, Has.Some.InstanceOf<IProperty>());
            Assert.That(members, Has.Some.InstanceOf<IEvent>());
            Assert.That(members, Has.Some.InstanceOf<IField>());
            Assert.That(members, Has.Some.InstanceOf<IOperator>());
        }

        [Test]
        public void GetMembers_WithInterfaceType_ReturnsInterfaceMembers()
        {
            var type = typeof(TestTypes.ITestInterface).GetMetadata();

            var members = type.GetMembers().ToList();

            Assert.That(members, Has.Some.InstanceOf<IMethod>());
            Assert.That(members, Has.Some.InstanceOf<IProperty>());
            Assert.That(members, Has.Some.InstanceOf<IEvent>());
        }

        [Test]
        public void GetMembers_WithTypeWithExplicitInterfaceMembers_ReturnsExplicitMembers()
        {
            var type = typeof(TestTypes.TestDerivedClass).GetMetadata();

            var members = type.GetMembers().ToList();

            var explicitMember = members.OfType<IVirtualTypeMember>().FirstOrDefault(m => m.IsExplicitInterfaceImplementation);
            Assert.That(explicitMember, Is.Not.Null);
        }

        [Test]
        public void GetMembers_WithTypeWithoutMembers_ReturnsOnlyImplicitConstructor()
        {
            var type = typeof(TestTypes.TestExtensionTarget).GetMetadata();

            var members = type.GetMembers().ToList();

            Assert.That(members, Has.Count.EqualTo(1));
            Assert.That(members[0], Is.InstanceOf<IConstructor>());
        }

        [Test]
        public void GetMembersIncludingNested_WithNullType_ThrowsArgumentNullException()
        {
            Assert.That(() => default(IType)!.GetMembersIncludingNested(), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("type"));
        }

        [Test]
        public void GetMembersIncludingNested_WithTypeWithoutNestedTypes_ReturnsSameMembersAsGetMembers()
        {
            var type = typeof(TestTypes.TestBaseClass).GetMetadata();

            var members = type.GetMembers().ToList();
            var membersDeep = type.GetMembersIncludingNested().ToList();

            Assert.That(membersDeep, Is.EquivalentTo(members));
        }

        [Test]
        public void GetMembersIncludingNested_WithTypeWithNestedTypes_IncludesNestedTypes()
        {
            var type = typeof(TestTypes.TestNestedClass).GetMetadata();

            var membersDeep = type.GetMembersIncludingNested().ToList();

            Assert.That(membersDeep, Has.Some.InstanceOf<IType>());
        }

        [Test]
        public void GetMembersIncludingNested_WithTypeWithNestedTypes_IncludesNestedTypeMembers()
        {
            var type = typeof(TestTypes.TestValueType).GetMetadata();

            var membersDeep = type.GetMembersIncludingNested().ToList();

            var nestedMember = membersDeep.OfType<IMethod>().FirstOrDefault(m => m.Name == nameof(TestTypes.TestValueType.NestedValueType.NestedMethod));
            Assert.That(nestedMember, Is.Not.Null);
        }

        [Test]
        public void GetMembersIncludingNested_WithTypeWithoutMembers_ReturnsOnlyImplicitConstructor()
        {
            var type = typeof(TestTypes.TestExtensionTarget).GetMetadata();

            var membersDeep = type.GetMembersIncludingNested().ToList();

            Assert.That(membersDeep, Has.Count.EqualTo(1));
            Assert.That(membersDeep[0], Is.InstanceOf<IConstructor>());
        }
    }
}
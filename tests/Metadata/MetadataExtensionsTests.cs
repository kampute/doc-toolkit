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
            var baseType = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass).GetMetadata();
            Assert.That(baseType, Is.Not.Null);

            var objectType = typeof(object).GetMetadata();
            Assert.That(objectType, Is.Not.Null);

            var inheritedMember = baseType.GetInheritedMember();
            Assert.That(inheritedMember, Is.SameAs(objectType));
        }

        [Test]
        public void GetInheritedMember_WithDerivedGenericType_ReturnsBaseType()
        {
            var genericType = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata();
            Assert.That(genericType, Is.Not.Null);

            var baseType = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass).GetMetadata();
            Assert.That(baseType, Is.Not.Null);

            var inheritedMember = genericType.GetInheritedMember();
            Assert.That(inheritedMember, Is.SameAs(baseType));
        }

        [Test]
        public void GetInheritedMember_WithConstructedGenericType_ReturnsGenericTypeDefinition()
        {
            var constructedType = typeof(Acme.SampleDerivedConstructedGenericClass).GetMetadata();
            Assert.That(constructedType, Is.Not.Null);

            var genericType = typeof(Acme.SampleDerivedGenericClass<object, int, string>).GetMetadata();
            Assert.That(genericType, Is.InstanceOf<IGenericCapableType>());

            var inheritedMember = constructedType.GetInheritedMember();
            Assert.That(inheritedMember, Is.SameAs(genericType));
        }

        [Test]
        public void GetInheritedMember_WithConstructor_ReturnsBaseConstructor()
        {
            var baseType = typeof(Acme.SampleDerivedGenericClass<object, int, string>).GetMetadata<IClassType>();
            Assert.That(baseType, Is.Not.Null);

            var baseConstructor = baseType.Constructors.FirstOrDefault(c => c.Parameters.Count == 0);
            Assert.That(baseConstructor, Is.InstanceOf<IConstructor>());

            var derivedType = typeof(Acme.SampleDerivedConstructedGenericClass).GetMetadata<IClassType>();
            Assert.That(derivedType, Is.Not.Null);

            var derivedConstructor = derivedType.Constructors.FirstOrDefault(c => c.Parameters.Count == 0);
            Assert.That(derivedConstructor, Is.InstanceOf<IConstructor>());

            var inheritedMember = derivedConstructor.GetInheritedMember();
            Assert.That(inheritedMember, Is.SameAs(baseConstructor));
        }

        [Test]
        public void GetInheritedMember_WithConstructorNoBaseMatch_ReturnsNull()
        {
            var derivedType = typeof(Acme.SampleDerivedConstructedGenericClass).GetMetadata<IClassType>();
            Assert.That(derivedType, Is.Not.Null);

            var derivedConstructor = derivedType.Constructors.FirstOrDefault(c => c.Parameters.Count == 1);
            Assert.That(derivedConstructor, Is.InstanceOf<IConstructor>());

            var inheritedMember = derivedConstructor.GetInheritedMember();
            Assert.That(inheritedMember, Is.Null);
        }

        [Test]
        public void GetInheritedMember_WithOverriddenMethod_ReturnsOverriddenMember()
        {
            var methodName = nameof(Acme.SampleDerivedConstructedGenericClass.Method);

            var baseType = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass).GetMetadata<IClassType>();
            Assert.That(baseType, Is.Not.Null);

            var baseMethod = baseType.Methods.FirstOrDefault(m => m.Name == methodName);
            Assert.That(baseMethod, Is.InstanceOf<IMethod>());

            var derivedType = typeof(Acme.SampleDerivedConstructedGenericClass).GetMetadata<IClassType>();
            Assert.That(derivedType, Is.Not.Null);

            var overriddenMethod = derivedType.Methods.FirstOrDefault(m => m.Name == methodName);
            Assert.That(overriddenMethod, Is.InstanceOf<IMethod>());

            var inheritedMember = overriddenMethod.GetInheritedMember();

            Assert.That(inheritedMember, Is.SameAs(baseMethod));
        }

        [Test]
        public void GetInheritedMember_WithOverriddenGenericMethod_ReturnsOverriddenMember()
        {
            var methodName = nameof(Acme.SampleDerivedConstructedGenericClass.GenericMethod);

            var baseType = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            Assert.That(baseType, Is.Not.Null);

            var baseMethod = baseType.Methods.FirstOrDefault(m => m.Name == methodName);
            Assert.That(baseMethod, Is.InstanceOf<IMethod>());

            var derivedType = typeof(Acme.SampleDerivedConstructedGenericClass).GetMetadata<IClassType>();
            Assert.That(derivedType, Is.Not.Null);

            var overriddenMethod = derivedType.Methods.FirstOrDefault(m => m.Name == methodName);
            Assert.That(overriddenMethod, Is.InstanceOf<IMethod>());

            var inheritedMember = overriddenMethod.GetInheritedMember();

            Assert.That(inheritedMember, Is.SameAs(baseMethod));
        }

        [Test]
        public void GetInheritedMember_WithInterfaceMethod_ReturnsInterfaceMember()
        {
            var methodName = nameof(Acme.ISampleInterface.InterfaceMethodWithRefParam);

            var interfaceType = typeof(Acme.ISampleInterface).GetMetadata<IInterfaceType>();
            Assert.That(interfaceType, Is.Not.Null);

            var interfaceMethod = interfaceType.Methods.FirstOrDefault(m => m.Name == methodName);
            Assert.That(interfaceMethod, Is.InstanceOf<IMethod>());

            var implementingType = typeof(Acme.SampleMethods).GetMetadata<IClassType>();
            Assert.That(implementingType, Is.Not.Null);

            var implementedMethod = implementingType.Methods.FirstOrDefault(m => m.Name == methodName);
            Assert.That(implementedMethod, Is.InstanceOf<IMethod>());

            var inheritedMember = implementedMethod.GetInheritedMember();

            Assert.That(inheritedMember, Is.SameAs(interfaceMethod));
        }

        [Test]
        public void GetInheritedMember_WithInterfaceGenericMethod_ReturnsInterfaceMember()
        {
            var methodName = nameof(Acme.ISampleInterface.InterfaceGenericMethod);

            var interfaceType = typeof(Acme.ISampleInterface).GetMetadata<IInterfaceType>();
            Assert.That(interfaceType, Is.Not.Null);

            var interfaceMethod = interfaceType.Methods.FirstOrDefault(m => m.Name == methodName);
            Assert.That(interfaceMethod, Is.InstanceOf<IMethod>());

            var implementingType = typeof(Acme.SampleMethods).GetMetadata<IClassType>();
            Assert.That(implementingType, Is.Not.Null);

            var implementedMethod = implementingType.Methods.FirstOrDefault(m => m.Name == methodName);
            Assert.That(implementedMethod, Is.InstanceOf<IMethod>());

            var inheritedMember = implementedMethod.GetInheritedMember();

            Assert.That(inheritedMember, Is.SameAs(interfaceMethod));
        }

        [Test]
        public void GetInheritedMember_WithRegularMethod_ReturnsNull()
        {
            var type = typeof(Acme.SampleMethods).GetMetadata<IClassType>();
            Assert.That(type, Is.Not.Null);

            var regularMethod = type.Methods.FirstOrDefault(m => m.Name == nameof(Acme.SampleMethods.RegularMethod));
            Assert.That(regularMethod, Is.InstanceOf<IMethod>());

            var inheritedMember = regularMethod.GetInheritedMember();
            Assert.That(inheritedMember, Is.Null);
        }

        [Test]
        public void GetInheritedMember_WithOverriddenProperty_ReturnsOverriddenMember()
        {
            var propertyName = nameof(Acme.SampleDerivedConstructedGenericClass.Property);

            var baseType = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            Assert.That(baseType, Is.Not.Null);

            var baseProperty = baseType.Properties.FirstOrDefault(p => p.Name == propertyName);
            Assert.That(baseProperty, Is.InstanceOf<IProperty>());

            var derivedType = typeof(Acme.SampleDerivedConstructedGenericClass).GetMetadata<IClassType>();
            Assert.That(derivedType, Is.Not.Null);

            var overriddenProperty = derivedType.Properties.FirstOrDefault(p => p.Name == propertyName);
            Assert.That(overriddenProperty, Is.InstanceOf<IProperty>());

            var inheritedMember = overriddenProperty.GetInheritedMember();

            Assert.That(inheritedMember, Is.SameAs(baseProperty));
        }

        [Test]
        public void GetInheritedMember_WithInterfaceProperty_ReturnsInterfaceMember()
        {
            var propertyName = nameof(Acme.ISampleInterface.InterfaceProperty);

            var interfaceType = typeof(Acme.ISampleInterface).GetMetadata<IInterfaceType>();
            Assert.That(interfaceType, Is.Not.Null);

            var interfaceProperty = interfaceType.Properties.FirstOrDefault(p => p.Name == propertyName);
            Assert.That(interfaceProperty, Is.InstanceOf<IProperty>());

            var implementingType = typeof(Acme.SampleProperties).GetMetadata<IClassType>();
            Assert.That(implementingType, Is.Not.Null);

            var implementedProperty = implementingType.ExplicitInterfaceProperties.FirstOrDefault(p => p.Name.EndsWith(propertyName));
            Assert.That(implementedProperty, Is.InstanceOf<IProperty>());

            var inheritedMember = implementedProperty.GetInheritedMember();

            Assert.That(inheritedMember, Is.SameAs(interfaceProperty));
        }

        [Test]
        public void GetInheritedMember_WithRegularProperty_ReturnsNull()
        {
            var type = typeof(Acme.SampleProperties).GetMetadata<IClassType>();
            Assert.That(type, Is.Not.Null);

            var regularProperty = type.Properties.FirstOrDefault(p => p.Name == nameof(Acme.SampleProperties.RegularProperty));
            Assert.That(regularProperty, Is.InstanceOf<IProperty>());

            var inheritedMember = regularProperty.GetInheritedMember();
            Assert.That(inheritedMember, Is.Null);
        }

        [Test]
        public void GetInheritedMember_WithOverriddenEvent_ReturnsOverriddenMember()
        {
            var eventName = nameof(Acme.SampleDerivedConstructedGenericClass.Event);

            var baseType = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            Assert.That(baseType, Is.Not.Null);

            var baseEvent = baseType.Events.FirstOrDefault(e => e.Name == eventName);
            Assert.That(baseEvent, Is.InstanceOf<IEvent>());

            var derivedType = typeof(Acme.SampleDerivedConstructedGenericClass).GetMetadata<IClassType>();
            Assert.That(derivedType, Is.Not.Null);

            var overriddenEvent = derivedType.Events.FirstOrDefault(e => e.Name == eventName);
            Assert.That(overriddenEvent, Is.InstanceOf<IEvent>());

            var inheritedMember = overriddenEvent.GetInheritedMember();

            Assert.That(inheritedMember, Is.SameAs(baseEvent));
        }

        [Test]
        public void GetInheritedMember_WithInterfaceEvent_ReturnsInterfaceMember()
        {
            var eventName = nameof(Acme.ISampleInterface.InterfaceEvent);

            var interfaceType = typeof(Acme.ISampleInterface).GetMetadata<IInterfaceType>();
            Assert.That(interfaceType, Is.Not.Null);

            var interfaceEvent = interfaceType.Events.FirstOrDefault(e => e.Name == eventName);
            Assert.That(interfaceEvent, Is.InstanceOf<IEvent>());

            var implementingType = typeof(Acme.SampleEvents).GetMetadata<ICompositeType>();
            Assert.That(implementingType, Is.Not.Null);

            var implementedEvent = implementingType.ExplicitInterfaceEvents.FirstOrDefault(e => e.Name.EndsWith(eventName));
            Assert.That(implementedEvent, Is.InstanceOf<IEvent>());

            var inheritedMember = implementedEvent.GetInheritedMember();

            Assert.That(inheritedMember, Is.SameAs(interfaceEvent));
        }

        [Test]
        public void GetInheritedMember_WithRegularEvent_ReturnsNull()
        {
            var type = typeof(Acme.SampleEvents).GetMetadata<IClassType>();
            Assert.That(type, Is.Not.Null);

            var regularEvent = type.Events.FirstOrDefault(e => e.Name == nameof(Acme.SampleEvents.RegularEvent));
            Assert.That(regularEvent, Is.InstanceOf<IEvent>());

            var inheritedMember = regularEvent.GetInheritedMember();
            Assert.That(inheritedMember, Is.Null);
        }

        [Test]
        public void GetInheritedMember_WithField_ReturnsNull()
        {
            var type = typeof(Acme.SampleFields).GetMetadata<ICompositeType>();
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
            var type = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass).GetMetadata();

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
            var type = typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct).GetMetadata();

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
            var type = typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface).GetMetadata();

            var members = type.GetMembers().ToList();

            Assert.That(members, Has.Some.InstanceOf<IMethod>());
            Assert.That(members, Has.Some.InstanceOf<IProperty>());
            Assert.That(members, Has.Some.InstanceOf<IEvent>());
            Assert.That(members, Has.Some.InstanceOf<IField>());
            Assert.That(members, Has.Some.InstanceOf<IOperator>());
        }

        [Test]
        public void GetMembers_WithTypeWithExplicitInterfaceMembers_ReturnsExplicitMembers()
        {
            var type = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass).GetMetadata();

            var members = type.GetMembers().ToList();

            Assert.That(members.OfType<IVirtualTypeMember>().Any(m => m.IsExplicitInterfaceImplementation), Is.True);
        }

        [Test]
        public void GetMembersIncludingNested_WithNullType_ThrowsArgumentNullException()
        {
            Assert.That(() => default(IType)!.GetMembersIncludingNested(), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("type"));
        }

        [Test]
        public void GetMembersIncludingNested_WithTypeWithoutNestedTypes_ReturnsSameMembersAsGetMembers()
        {
            var type = typeof(Acme.SampleDerivedConstructedGenericClass).GetMetadata();

            var members = type.GetMembers().ToList();
            var membersDeep = type.GetMembersIncludingNested().ToList();

            Assert.That(membersDeep, Is.EquivalentTo(members));
        }

        [Test]
        public void GetMembersIncludingNested_WithTypeWithNestedTypes_IncludesNestedTypes()
        {
            var type = typeof(Acme.SampleGenericClass<>).GetMetadata();

            var membersDeep = type.GetMembersIncludingNested().ToList();

            Assert.That(membersDeep, Has.Some.InstanceOf<IType>());
        }

        [Test]
        public void GetMembersIncludingNested_WithTypeWithNestedTypes_IncludesNestedTypeMembers()
        {
            var type = typeof(Acme.SampleGenericClass<>).GetMetadata();

            var membersDeep = type.GetMembersIncludingNested().ToList();

            Assert.That(membersDeep.OfType<ITypeMember>().Any(m => !ReferenceEquals(m.DeclaringType, type)), Is.True);
        }

        [Test]
        public void TryGetOwnTypeParameters_WithNullMember_ThrowsArgumentNullException()
        {
            Assert.That(() => default(IMember)!.TryGetOwnTypeParameters(out _), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("member"));
        }

        [Test]
        public void TryGetOwnTypeParameters_WithNonGenericType_ReturnsFalse()
        {
            var type = typeof(Acme.ISampleInterface).GetMetadata();

            var result = type.TryGetOwnTypeParameters(out var parameters);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(parameters, Is.Empty);
            }
        }

        [Test]
        public void TryGetOwnTypeParameters_WithGenericTypeDefinition_ReturnsTrueAndParameters()
        {
            var type = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>).GetMetadata();

            var result = type.TryGetOwnTypeParameters(out var parameters);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(parameters.Select(p => p.Name), Is.EqualTo(["U", "V"]));
            }
        }

        [Test]
        public void TryGetOwnTypeParameters_WithConstructedGenericType_ReturnsTrueAndEmpty()
        {
            var type = typeof(Acme.SampleGenericClass<object>).GetMetadata();

            var result = type.TryGetOwnTypeParameters(out var parameters);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(parameters, Is.Empty);
            }
        }

        [Test]
        public void TryGetOwnTypeParameters_WithDerivedGenericTypeDefinition_ReturnsTrueAndParameters()
        {
            var type = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata();
            var result = type.TryGetOwnTypeParameters(out var parameters);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(parameters.Select(p => p.Name), Is.EqualTo(["T", "U", "V"]));
            }
        }

        [Test]
        public void TryGetOwnTypeParameters_WithGenericMethod_ReturnsTrueAndParameters()
        {
            var type = typeof(Acme.SampleMethods).GetMetadata<IClassType>();
            var method = type.Methods.First(m => m.Name == nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints));

            var result = method.TryGetOwnTypeParameters(out var parameters);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(parameters.Select(p => p.Name), Is.EqualTo(["T"]));
            }
        }

        [Test]
        public void TryGetOwnTypeParameters_WithNonGenericMethod_ReturnsFalse()
        {
            var type = typeof(Acme.SampleMethods).GetMetadata<IClassType>();
            var method = type.Methods.First(m => m.Name == nameof(Acme.SampleMethods.RegularMethod));

            var result = method.TryGetOwnTypeParameters(out var parameters);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(parameters, Is.Empty);
            }
        }

        [Test]
        public void TryGetOwnTypeParameters_WithGenericInterface_ReturnsTrueAndParameters()
        {
            var type = typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>).GetMetadata();

            var result = type.TryGetOwnTypeParameters(out var parameters);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(parameters.Select(p => p.Name), Is.EqualTo(["U", "V"]));
            }
        }
    }
}
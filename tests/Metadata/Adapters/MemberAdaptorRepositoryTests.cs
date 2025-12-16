// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Adapters;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    [TestFixture]
    public class MemberAdaptorRepositoryTests
    {
        private MemberAdapterRepository repository = null!;
        private Mock<IAssembly> assemblyMock = null!;
        private Mock<IMemberAdapterFactory> factoryMock = null!;

        [SetUp]
        public void SetUp()
        {
            assemblyMock = new Mock<IAssembly>();
            assemblyMock.Setup(a => a.Represents(It.IsAny<Assembly>())).Returns(true);
            assemblyMock.Setup(a => a.Equals(It.IsAny<object>())).Returns((object obj) => ReferenceEquals(obj, assemblyMock.Object));

            factoryMock = new Mock<IMemberAdapterFactory>();
            factoryMock.Setup(f => f.CreateTypeMetadata(It.IsAny<IAssembly>(), It.IsAny<Type>())).Returns(new Mock<IType>().Object);
            factoryMock.Setup(f => f.CreateTypeMetadata(It.IsAny<IType>(), It.IsAny<Type>())).Returns(new Mock<IType>().Object);
            factoryMock.Setup(f => f.CreateConstructorMetadata(It.IsAny<IType>(), It.IsAny<ConstructorInfo>())).Returns(new Mock<IConstructor>().Object);
            factoryMock.Setup(f => f.CreateMethodMetadata(It.IsAny<IType>(), It.IsAny<MethodInfo>())).Returns(new Mock<IMethodBase>().Object);
            factoryMock.Setup(f => f.CreatePropertyMetadata(It.IsAny<IType>(), It.IsAny<PropertyInfo>())).Returns(new Mock<IProperty>().Object);
            factoryMock.Setup(f => f.CreateEventMetadata(It.IsAny<IType>(), It.IsAny<EventInfo>())).Returns(new Mock<IEvent>().Object);
            factoryMock.Setup(f => f.CreateFieldMetadata(It.IsAny<IType>(), It.IsAny<FieldInfo>())).Returns(new Mock<IField>().Object);
            factoryMock.Setup(f => f.CreateParameterMetadata(It.IsAny<IMember>(), It.IsAny<ParameterInfo>())).Returns(new Mock<IParameter>().Object);
            factoryMock.Setup(f => f.CreateCustomAttributeMetadata(It.IsAny<CustomAttributeData>(), It.IsAny<AttributeTarget>())).Returns(new Mock<ICustomAttribute>().Object);

            repository = new MemberAdapterRepository(assemblyMock.Object, factoryMock.Object);
        }

        [Test]
        public void Assembly_ReturnsProvidedAssembly()
        {
            Assert.That(repository.Assembly, Is.SameAs(assemblyMock.Object));
        }

        [Test]
        public void ResolveCanonicalType_WithRegularType_ReturnsSameInstance()
        {
            var type = typeof(string);

            var result = repository.ResolveCanonicalType(type);

            Assert.That(result, Is.SameAs(type));
        }

        [Test]
        public void ResolveCanonicalType_WithGenericNestedType_ReturnsDirectInstance()
        {
            var direct = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass);
            var viaReflection = typeof(Acme.SampleGenericClass<>)
                .GetNestedType("InnerGenericClass`2")!
                .GetNestedType("DeepInnerGenericClass")!;

            Assert.That(repository.ResolveCanonicalType(viaReflection), Is.SameAs(direct));
        }

        [Test]
        public void ResolveCanonicalType_WithGenericBaseType_ReturnsDirectInstance()
        {
            var direct = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass);
            var viaReflection = typeof(Acme.SampleDerivedGenericClass<,,>).BaseType!;

            Assert.That(repository.ResolveCanonicalType(viaReflection), Is.SameAs(direct));
        }

        [Test]
        public void ResolveCanonicalType_WithConstructedGenericBaseType_ReturnsDirectInstance()
        {
            var direct = typeof(Acme.SampleDerivedGenericClass<object, int, string>);
            var viaReflection = typeof(Acme.SampleDerivedConstructedGenericClass).BaseType!;

            Assert.That(repository.ResolveCanonicalType(viaReflection), Is.SameAs(direct));
        }

        [Test]
        public void ResolveCanonicalType_WithGenericImplementedInterfacesType_ReturnsDirectInstance()
        {
            var direct = typeof(IEnumerable<string>);
            var viaReflection = typeof(Acme.SampleDerivedConstructedGenericClass).GetInterfaces()
                .First(static i => i.Name == "IEnumerable`1");

            Assert.That(repository.ResolveCanonicalType(viaReflection), Is.SameAs(direct));
        }

        [Test]
        public void ResolveCanonicalType_WithConstructedGenericImplementedInterfaceType_ReturnsDirectInstance()
        {
            var direct = typeof(IEnumerable<string>);
            var viaReflection = typeof(Acme.SampleDerivedConstructedGenericClass).GetInterfaces()
                .First(static i => i.Name == "IEnumerable`1");

            Assert.That(repository.ResolveCanonicalType(viaReflection), Is.SameAs(direct));
        }

        [Test]
        public void ResolveCanonicalType_WithGenericConstructedGenericImplementedInterfaceType_ReturnsSameInstance()
        {
            var type = typeof(Dictionary<,>.KeyCollection.Enumerator).GetInterfaces()
                .First(static i => i.Name == "IEnumerator`1");

            Assert.That(repository.ResolveCanonicalType(type), Is.SameAs(type));
        }

        [Test]
        public void ResolveCanonicalType_WithGenericTypeAsParameter_ReturnsSameInstance()
        {
            var type = typeof(Acme.SampleExtensions)
                .GetMethod(nameof(Acme.SampleExtensions.ClassicExtensionMethodForClass))!
                .GetParameters()[0].ParameterType;

            Assert.That(repository.ResolveCanonicalType(type), Is.SameAs(type));
        }

        [Test]
        public void GetTypeMetadata_WithValidType_ReturnsTypeMetadata()
        {
            var type = typeof(string);

            var result = repository.GetTypeMetadata(type);

            Assert.That(result, Is.InstanceOf<IType>());
        }

        [Test]
        public void GetTypeMetadata_WithNullType_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetTypeMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("type"));
        }

        [Test]
        public void GetConstructorMetadata_WithValidConstructor_ReturnsConstructorMetadata()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, Type.EmptyTypes)!;

            var result = repository.GetConstructorMetadata(constructorInfo);

            Assert.That(result, Is.InstanceOf<IConstructor>());
        }

        [Test]
        public void GetConstructorMetadata_WithNullConstructor_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetConstructorMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("constructorInfo"));
        }

        [Test]
        public void GetMethodMetadata_WithValidMethod_ReturnsMethodBaseMetadata()
        {
            var methodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.RegularMethod), Acme.Bindings.AllDeclared)!;

            var result = repository.GetMethodMetadata(methodInfo);

            Assert.That(result, Is.InstanceOf<IMethodBase>());
        }

        [Test]
        public void GetMethodMetadata_WithValidOperatorMethod_ReturnsMethodBaseMetadata()
        {
            var methodInfo = typeof(Acme.SampleOperators).GetMethod("op_UnaryPlus", Acme.Bindings.AllDeclared)!;

            var result = repository.GetMethodMetadata(methodInfo);

            Assert.That(result, Is.InstanceOf<IMethodBase>());
        }

        [Test]
        public void GetMethodMetadata_WithNullMethod_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetMethodMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("methodInfo"));
        }

        [Test]
        public void GetPropertyMetadata_WithValidProperty_ReturnsPropertyMetadata()
        {
            var propertyInfo = typeof(Acme.SampleProperties).GetProperty(nameof(Acme.SampleProperties.RegularProperty), Acme.Bindings.AllDeclared)!;

            var result = repository.GetPropertyMetadata(propertyInfo);

            Assert.That(result, Is.InstanceOf<IProperty>());
        }

        [Test]
        public void GetPropertyMetadata_WithNullProperty_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetPropertyMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("propertyInfo"));
        }

        [Test]
        public void GetEventMetadata_WithValidEvent_ReturnsEventMetadata()
        {
            var eventInfo = typeof(Acme.SampleEvents).GetEvent(nameof(Acme.SampleEvents.RegularEvent), Acme.Bindings.AllDeclared)!;

            var result = repository.GetEventMetadata(eventInfo);

            Assert.That(result, Is.InstanceOf<IEvent>());
        }

        [Test]
        public void GetEventMetadata_WithNullEvent_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetEventMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("eventInfo"));
        }

        [Test]
        public void GetFieldMetadata_WithValidField_ReturnsFieldMetadata()
        {
            var fieldInfo = typeof(Acme.SampleFields).GetField(nameof(Acme.SampleFields.VolatileField), Acme.Bindings.AllDeclared)!;

            var result = repository.GetFieldMetadata(fieldInfo);

            Assert.That(result, Is.InstanceOf<IField>());
        }

        [Test]
        public void GetFieldMetadata_WithNullField_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetFieldMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("fieldInfo"));
        }

        [Test]
        public void GetMemberMetadata_WithType_ReturnsTypeMetadata()
        {
            var memberInfo = typeof(string);

            var result = ((IMemberAdapterRepository)repository).GetMemberMetadata(memberInfo);

            Assert.That(result, Is.InstanceOf<IType>());
        }

        [Test]
        public void GetMemberMetadata_WithConstructor_ReturnsConstructorMetadata()
        {
            var memberInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, Type.EmptyTypes)!;

            var result = ((IMemberAdapterRepository)repository).GetMemberMetadata(memberInfo);

            Assert.That(result, Is.InstanceOf<IConstructor>());
        }

        [Test]
        public void GetMemberMetadata_WithMethod_ReturnsMethodBaseMetadata()
        {
            var memberInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.RegularMethod), Acme.Bindings.AllDeclared)!;

            var result = ((IMemberAdapterRepository)repository).GetMemberMetadata(memberInfo);

            Assert.That(result, Is.InstanceOf<IMethodBase>());
        }

        [Test]
        public void GetMemberMetadata_WithOperatorMethod_ReturnsMethodBaseMetadata()
        {
            var memberInfo = typeof(Acme.SampleOperators).GetMethod("op_UnaryPlus", Acme.Bindings.AllDeclared)!;

            var result = ((IMemberAdapterRepository)repository).GetMemberMetadata(memberInfo);

            Assert.That(result, Is.InstanceOf<IMethodBase>());
        }

        [Test]
        public void GetMemberMetadata_WithProperty_ReturnsPropertyMetadata()
        {
            var memberInfo = typeof(Acme.SampleProperties).GetProperty(nameof(Acme.SampleProperties.RegularProperty), Acme.Bindings.AllDeclared)!;

            var result = ((IMemberAdapterRepository)repository).GetMemberMetadata(memberInfo);

            Assert.That(result, Is.InstanceOf<IProperty>());
        }

        [Test]
        public void GetMemberMetadata_WithEvent_ReturnsEventMetadata()
        {
            var memberInfo = typeof(Acme.SampleEvents).GetEvent(nameof(Acme.SampleEvents.RegularEvent), Acme.Bindings.AllDeclared)!;

            var result = ((IMemberAdapterRepository)repository).GetMemberMetadata(memberInfo);

            Assert.That(result, Is.InstanceOf<IEvent>());
        }

        [Test]
        public void GetMemberMetadata_WithField_ReturnsFieldMetadata()
        {
            var memberInfo = typeof(Acme.SampleFields).GetField(nameof(Acme.SampleFields.VolatileField), Acme.Bindings.AllDeclared)!;

            var result = ((IMemberAdapterRepository)repository).GetMemberMetadata(memberInfo);

            Assert.That(result, Is.InstanceOf<IField>());
        }

        [Test]
        public void GetMemberMetadata_WithNullMember_ThrowsArgumentNullException()
        {
            Assert.That(() => ((IMemberAdapterRepository)repository).GetMemberMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("memberInfo"));
        }

        [Test]
        public void GetParameterMetadata_WithValidParameter_ReturnsParameterMetadata()
        {
            var methodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.RefParamsMethod), Acme.Bindings.AllDeclared)!;
            var parameterInfo = methodInfo.GetParameters()[0];

            var result = repository.GetParameterMetadata(parameterInfo);

            Assert.That(result, Is.InstanceOf<IParameter>());
        }

        [Test]
        public void GetParameterMetadata_WithNullParameter_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetParameterMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("parameterInfo"));
        }

        [Test]
        public void GetCustomAttributeMetadata_WithValidAttribute_ReturnsCustomAttributeMetadata()
        {
            var attributeData = typeof(Acme.SampleAttribute).GetCustomAttributesData()[0];

            var result = repository.GetCustomAttributeMetadata(attributeData, AttributeTarget.Type);

            Assert.That(result, Is.InstanceOf<ICustomAttribute>());
        }

        [Test]
        public void GetCustomAttributeMetadata_WithNullAttribute_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetCustomAttributeMetadata(null!, AttributeTarget.Type), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("attributeData"));
        }
    }
}

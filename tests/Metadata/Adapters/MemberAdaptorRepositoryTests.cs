// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Adapters;
    using Kampute.DocToolkit.Metadata.Reflection;
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

            // The factory creates a new adapter on every call, so a repeated lookup returns the same instance only if the repository caches it.
            factoryMock = new Mock<IMemberAdapterFactory>();
            factoryMock.Setup(f => f.CreateTypeMetadata(It.IsAny<IAssembly>(), It.IsAny<Type>())).Returns(() => new Mock<IType>().Object);
            factoryMock.Setup(f => f.CreateTypeMetadata(It.IsAny<IType>(), It.IsAny<Type>())).Returns(() => new Mock<IType>().Object);
            factoryMock.Setup(f => f.CreateConstructorMetadata(It.IsAny<IType>(), It.IsAny<ConstructorInfo>())).Returns(() => new Mock<IConstructor>().Object);
            factoryMock.Setup(f => f.CreateMethodMetadata(It.IsAny<IType>(), It.IsAny<MethodInfo>())).Returns(() => new Mock<IMethodBase>().Object);
            factoryMock.Setup(f => f.CreatePropertyMetadata(It.IsAny<IType>(), It.IsAny<PropertyInfo>())).Returns(() => new Mock<IProperty>().Object);
            factoryMock.Setup(f => f.CreateEventMetadata(It.IsAny<IType>(), It.IsAny<EventInfo>())).Returns(() => new Mock<IEvent>().Object);
            factoryMock.Setup(f => f.CreateFieldMetadata(It.IsAny<IType>(), It.IsAny<FieldInfo>())).Returns(() => new Mock<IField>().Object);
            factoryMock.Setup(f => f.CreateParameterMetadata(It.IsAny<IMember>(), It.IsAny<ParameterInfo>())).Returns(() => new Mock<IParameter>().Object);
            factoryMock.Setup(f => f.CreateCustomAttributeMetadata(It.IsAny<CustomAttributeData>(), It.IsAny<AttributeTarget>())).Returns(() => new Mock<ICustomAttribute>().Object);
            factoryMock.Setup(f => f.CreateExtensionBlockMetadata(It.IsAny<IType>(), It.IsAny<ExtensionBlockInfo>())).Returns(() => new Mock<IExtensionBlock>().Object);

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
            var genericType = typeof(Acme.SampleDerivedGenericClass<,,>);
            var direct = typeof(IEnumerable<>).MakeGenericType(genericType.GetGenericArguments()[2]);
            var viaReflection = genericType.GetInterfaces()
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
        public void GetTypeMetadata_WithTopLevelType_CreatesMetadataOnceForAssembly()
        {
            var type = typeof(string);

            var first = repository.GetTypeMetadata(type);
            var second = repository.GetTypeMetadata(type);

            Assert.That(second, Is.SameAs(first));
            factoryMock.Verify(f => f.CreateTypeMetadata(assemblyMock.Object, type), Times.Once);
        }

        [Test]
        public void GetTypeMetadata_WithNestedType_CreatesMetadataForDeclaringTypeMetadata()
        {
            var type = typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>);

            repository.GetTypeMetadata(type);

            var declaringType = repository.GetTypeMetadata(typeof(Acme.SampleGenericClass<>));
            factoryMock.Verify(f => f.CreateTypeMetadata(declaringType, type), Times.Once);
        }

        [Test]
        public void GetTypeMetadata_WithTypeOfAnotherAssembly_ThrowsArgumentException()
        {
            var repositoryOfAnotherAssembly = new MemberAdapterRepository(typeof(string).Assembly.GetMetadata());

            Assert.That(() => repositoryOfAnotherAssembly.GetTypeMetadata(typeof(Acme.SampleMethods)), Throws.ArgumentException);
        }

        [Test]
        public void GetTypeMetadata_WithNullType_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetTypeMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("type"));
        }

        [Test]
        public void GetConstructorMetadata_WithValidConstructor_CreatesMetadataOnceForDeclaringType()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, Type.EmptyTypes)!;

            var first = repository.GetConstructorMetadata(constructorInfo);
            var second = repository.GetConstructorMetadata(constructorInfo);

            Assert.That(second, Is.SameAs(first));
            var declaringType = repository.GetTypeMetadata(typeof(Acme.SampleConstructors));
            factoryMock.Verify(f => f.CreateConstructorMetadata(declaringType, constructorInfo), Times.Once);
        }

        [Test]
        public void GetConstructorMetadata_WithNullConstructor_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetConstructorMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("constructorInfo"));
        }

        [Test]
        public void GetMethodMetadata_WithValidMethod_CreatesMetadataOnceForDeclaringType()
        {
            var methodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.RegularMethod), Acme.Bindings.AllDeclared)!;

            var first = repository.GetMethodMetadata(methodInfo);
            var second = repository.GetMethodMetadata(methodInfo);

            Assert.That(second, Is.SameAs(first));
            var declaringType = repository.GetTypeMetadata(typeof(Acme.SampleMethods));
            factoryMock.Verify(f => f.CreateMethodMetadata(declaringType, methodInfo), Times.Once);
        }

        [Test]
        public void GetMethodMetadata_WithExtensionBlockMethod_CreatesMetadataForNormalizedMethod()
        {
            var methodInfo = typeof(Acme.SampleExtensions).GetMethod(nameof(Acme.SampleExtensions.InstanceExtensionMethod), Acme.Bindings.AllDeclared)!;

            repository.GetMethodMetadata(methodInfo);

            factoryMock.Verify(f => f.CreateMethodMetadata(It.IsAny<IType>(), It.Is<MethodInfo>(m => m is IExtensionBlockMethodInfo && ((IExtensionBlockMethodInfo)m).DeclaredMethod == methodInfo)), Times.Once);
        }

        [Test]
        public void GetMethodMetadata_WithExtensionBlockMethodAsDeclared_CreatesMetadataForDeclaredMethod()
        {
            var methodInfo = typeof(Acme.SampleExtensions).GetMethod(nameof(Acme.SampleExtensions.InstanceExtensionMethod), Acme.Bindings.AllDeclared)!;

            repository.GetMethodMetadata(methodInfo, asDeclared: true);

            factoryMock.Verify(f => f.CreateMethodMetadata(It.IsAny<IType>(), methodInfo), Times.Once);
        }

        [Test]
        public void GetMethodMetadata_WithNullMethod_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetMethodMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("methodInfo"));
        }

        [Test]
        public void GetPropertyMetadata_WithValidProperty_CreatesMetadataOnceForDeclaringType()
        {
            var propertyInfo = typeof(Acme.SampleProperties).GetProperty(nameof(Acme.SampleProperties.RegularProperty), Acme.Bindings.AllDeclared)!;

            var first = repository.GetPropertyMetadata(propertyInfo);
            var second = repository.GetPropertyMetadata(propertyInfo);

            Assert.That(second, Is.SameAs(first));
            var declaringType = repository.GetTypeMetadata(typeof(Acme.SampleProperties));
            factoryMock.Verify(f => f.CreatePropertyMetadata(declaringType, propertyInfo), Times.Once);
        }

        [Test]
        public void GetPropertyMetadata_WithNullProperty_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetPropertyMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("propertyInfo"));
        }

        [Test]
        public void GetEventMetadata_WithValidEvent_CreatesMetadataOnceForDeclaringType()
        {
            var eventInfo = typeof(Acme.SampleEvents).GetEvent(nameof(Acme.SampleEvents.RegularEvent), Acme.Bindings.AllDeclared)!;

            var first = repository.GetEventMetadata(eventInfo);
            var second = repository.GetEventMetadata(eventInfo);

            Assert.That(second, Is.SameAs(first));
            var declaringType = repository.GetTypeMetadata(typeof(Acme.SampleEvents));
            factoryMock.Verify(f => f.CreateEventMetadata(declaringType, eventInfo), Times.Once);
        }

        [Test]
        public void GetEventMetadata_WithNullEvent_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetEventMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("eventInfo"));
        }

        [Test]
        public void GetFieldMetadata_WithValidField_CreatesMetadataOnceForDeclaringType()
        {
            var fieldInfo = typeof(Acme.SampleFields).GetField(nameof(Acme.SampleFields.VolatileField), Acme.Bindings.AllDeclared)!;

            var first = repository.GetFieldMetadata(fieldInfo);
            var second = repository.GetFieldMetadata(fieldInfo);

            Assert.That(second, Is.SameAs(first));
            var declaringType = repository.GetTypeMetadata(typeof(Acme.SampleFields));
            factoryMock.Verify(f => f.CreateFieldMetadata(declaringType, fieldInfo), Times.Once);
        }

        [Test]
        public void GetFieldMetadata_WithNullField_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetFieldMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("fieldInfo"));
        }

        [Test]
        public void GetExtensionBlockMetadata_WithValidBlock_CreatesMetadataOnceForContainerType()
        {
            var block = repository.ExtensionReflection.GetDeclaredExtensionBlocks(typeof(Acme.SampleExtensions))[0];

            var first = repository.GetExtensionBlockMetadata(block);
            var second = repository.GetExtensionBlockMetadata(block);

            Assert.That(second, Is.SameAs(first));
            var declaringType = repository.GetTypeMetadata(typeof(Acme.SampleExtensions));
            factoryMock.Verify(f => f.CreateExtensionBlockMetadata(declaringType, block), Times.Once);
        }

        [Test]
        public void GetExtensionBlockMetadata_WithBlockOfAnotherAssembly_ThrowsArgumentException()
        {
            var block = new ExtensionContainerInfo(typeof(Acme.SampleExtensions)).ExtensionBlocks[0];
            assemblyMock.Setup(a => a.Represents(It.IsAny<Assembly>())).Returns(false);

            Assert.That(() => repository.GetExtensionBlockMetadata(block), Throws.ArgumentException
                .With.Property("ParamName").EqualTo("extensionBlock"));
        }

        [Test]
        public void GetExtensionBlockMetadata_WithNullBlock_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetExtensionBlockMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("extensionBlock"));
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
        public void GetMemberMetadata_WithExtensionBlockPropertyAccessor_ReturnsPropertyMetadata()
        {
            var accessor = typeof(Acme.SampleExtensions).GetMethod(nameof(Acme.SampleExtensions.get_InstanceExtensionProperty), Acme.Bindings.AllDeclared)!;

            var result = ((IMemberAdapterRepository)repository).GetMemberMetadata(accessor);

            Assert.That(result, Is.InstanceOf<IProperty>());
            factoryMock.Verify(f => f.CreatePropertyMetadata(It.IsAny<IType>(), It.Is<PropertyInfo>(p => p is IExtensionBlockPropertyInfo && p.Name == "InstanceExtensionProperty")), Times.Once);
        }

        [Test]
        public void GetMemberMetadata_WithUnsupportedMember_ThrowsNotSupportedException()
        {
            var memberInfo = new Mock<MemberInfo>().Object;

            Assert.That(() => ((IMemberAdapterRepository)repository).GetMemberMetadata(memberInfo), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void GetMemberMetadata_WithNullMember_ThrowsArgumentNullException()
        {
            Assert.That(() => ((IMemberAdapterRepository)repository).GetMemberMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("memberInfo"));
        }

        [Test]
        public void GetParameterMetadata_WithValidParameter_CreatesMetadataForDeclaringMember()
        {
            var methodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.RefParamsMethod), Acme.Bindings.AllDeclared)!;
            var parameterInfo = methodInfo.GetParameters()[0];

            repository.GetParameterMetadata(parameterInfo);

            var member = repository.GetMethodMetadata(methodInfo);
            factoryMock.Verify(f => f.CreateParameterMetadata(member, parameterInfo), Times.Once);
        }

        [Test]
        public void GetParameterMetadata_WithNullParameter_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetParameterMetadata(null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("parameterInfo"));
        }

        [Test]
        public void GetCustomAttributeMetadata_WithValidAttribute_CreatesMetadataForTarget()
        {
            var attributeData = typeof(Acme.SampleAttribute).GetCustomAttributesData()[0];

            repository.GetCustomAttributeMetadata(attributeData, AttributeTarget.Type);

            factoryMock.Verify(f => f.CreateCustomAttributeMetadata(attributeData, AttributeTarget.Type), Times.Once);
        }

        [Test]
        public void GetCustomAttributeMetadata_WithNullAttribute_ThrowsArgumentNullException()
        {
            Assert.That(() => repository.GetCustomAttributeMetadata(null!, AttributeTarget.Type), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("attributeData"));
        }
    }
}
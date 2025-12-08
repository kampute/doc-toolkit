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
    using System.Reflection;

    [TestFixture]
    public class MemberAdapterFactoryTests
    {
        private readonly MemberAdapterFactory factory = MemberAdapterFactory.Instance;
        private Mock<IAssembly> assemblyMock = null!;
        private Mock<IType> typeMock = null!;
        private Mock<IMember> memberMock = null!;

        [SetUp]
        public void SetUp()
        {
            assemblyMock = new Mock<IAssembly>();
            assemblyMock.Setup(a => a.Represents(It.IsAny<Assembly>())).Returns(true);
            assemblyMock.Setup(a => a.Equals(It.IsAny<object>())).Returns((object obj) => ReferenceEquals(obj, assemblyMock.Object));

            typeMock = new Mock<IType>();
            typeMock.Setup(t => t.Assembly).Returns(assemblyMock.Object);
            typeMock.Setup(t => t.IsNested).Returns(false);
            typeMock.Setup(t => t.Represents(It.IsAny<Type>())).Returns(true);

            memberMock = new Mock<IMember>();
            memberMock.Setup(m => m.DeclaringType).Returns(typeMock.Object);
        }

        [Test]
        public void CreateTypeMetadata_WithNullAssembly_ThrowsArgumentNullException()
        {
            Assert.That(() => factory.CreateTypeMetadata((IAssembly)null!, typeof(string)), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("assembly"));
        }

        [Test]
        public void CreateTypeMetadata_WithNullType_ThrowsArgumentNullException()
        {
            Assert.That(() => factory.CreateTypeMetadata(assemblyMock.Object, null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("type"));
        }

        [Test]
        public void CreateTypeMetadata_WithGenericParameterType_ReturnsTypeParameterMetadata()
        {
            var type = typeof(Acme.SampleGenericClass<>).GetGenericArguments()[0];

            var result = factory.CreateTypeMetadata(assemblyMock.Object, type);

            Assert.That(result, Is.InstanceOf<ITypeParameter>());
        }

        [Test]
        public void CreateTypeMetadata_WithArrayType_ReturnsTypeDecoratorMetadata()
        {
            var type = typeof(int[]);

            var result = factory.CreateTypeMetadata(assemblyMock.Object, type);

            Assert.That(result, Is.InstanceOf<ITypeDecorator>());
        }

        [Test]
        public void CreateTypeMetadata_WithPointerType_ReturnsTypeDecoratorMetadata()
        {
            var type = typeof(int*);

            var result = factory.CreateTypeMetadata(assemblyMock.Object, type);

            Assert.That(result, Is.InstanceOf<ITypeDecorator>());
        }

        [Test]
        public void CreateTypeMetadata_WithByRefType_ReturnsTypeDecoratorMetadata()
        {
            var type = typeof(int).MakeByRefType();

            var result = factory.CreateTypeMetadata(assemblyMock.Object, type);

            Assert.That(result, Is.InstanceOf<ITypeDecorator>());
        }

        [Test]
        public void CreateTypeMetadata_WithNullableType_ReturnsTypeDecoratorMetadata()
        {
            var type = typeof(int?);

            var result = factory.CreateTypeMetadata(assemblyMock.Object, type);

            Assert.That(result, Is.InstanceOf<ITypeDecorator>());
        }

        [Test]
        public void CreateTypeMetadata_WithEnumType_ReturnsEnumTypeMetadata()
        {
            var type = typeof(DayOfWeek);

            var result = factory.CreateTypeMetadata(assemblyMock.Object, type);

            Assert.That(result, Is.InstanceOf<IEnumType>());
        }

        [Test]
        public void CreateTypeMetadata_WithValueType_ReturnsStructTypeMetadata()
        {
            var type = typeof(int);

            var result = factory.CreateTypeMetadata(assemblyMock.Object, type);

            Assert.That(result, Is.InstanceOf<IStructType>());
        }

        [Test]
        public void CreateTypeMetadata_WithInterfaceType_ReturnsInterfaceTypeMetadata()
        {
            var type = typeof(Acme.ISampleInterface);

            var result = factory.CreateTypeMetadata(assemblyMock.Object, type);

            Assert.That(result, Is.InstanceOf<IInterfaceType>());
        }

        [Test]
        public void CreateTypeMetadata_WithDelegateType_ReturnsDelegateTypeMetadata()
        {
            var type = typeof(Action);

            var result = factory.CreateTypeMetadata(assemblyMock.Object, type);

            Assert.That(result, Is.InstanceOf<IDelegateType>());
        }

        [Test]
        public void CreateTypeMetadata_WithClassType_ReturnsClassTypeMetadata()
        {
            var type = typeof(Uri);

            var result = factory.CreateTypeMetadata(assemblyMock.Object, type);

            Assert.That(result, Is.InstanceOf<IClassType>());
        }

        [Test]
        public void CreateTypeMetadata_WithDeclaringTypeNull_ThrowsArgumentNullException()
        {
            Assert.That(() => factory.CreateTypeMetadata((IType)null!, typeof(Uri)), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("declaringType"));
        }

        [Test]
        public void CreateTypeMetadata_WithDeclaringTypeAndNullType_ThrowsArgumentNullException()
        {
            Assert.That(() => factory.CreateTypeMetadata(typeMock.Object, null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("type"));
        }

        [Test]
        public void CreateConstructorMetadata_ReturnsConstructorMetadata()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, Type.EmptyTypes)!;

            var result = factory.CreateConstructorMetadata(typeMock.Object, constructorInfo);

            Assert.That(result, Is.InstanceOf<IConstructor>());
        }

        [Test]
        public void CreateConstructorMetadata_WithNullDeclaringType_ThrowsArgumentNullException()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor(Acme.Bindings.AllDeclared, Type.EmptyTypes)!;

            Assert.That(() => factory.CreateConstructorMetadata(null!, constructorInfo), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("declaringType"));
        }

        [Test]
        public void CreateConstructorMetadata_WithNullConstructorInfo_ThrowsArgumentNullException()
        {
            Assert.That(() => factory.CreateConstructorMetadata(typeMock.Object, null!), Throws.ArgumentNullException.With
                .Property("ParamName").EqualTo("constructorInfo"));
        }

        [Test]
        public void CreatePropertyMetadata_ReturnsPropertyMetadata()
        {
            var propertyInfo = typeof(Acme.SampleProperties).GetProperty(nameof(Acme.SampleProperties.RegularProperty), Acme.Bindings.AllDeclared)!;

            var result = factory.CreatePropertyMetadata(typeMock.Object, propertyInfo);

            Assert.That(result, Is.InstanceOf<IProperty>());
        }

        [Test]
        public void CreatePropertyMetadata_WithNullDeclaringType_ThrowsArgumentNullException()
        {
            var propertyInfo = typeof(Acme.SampleProperties).GetProperty(nameof(Acme.SampleProperties.RegularProperty), Acme.Bindings.AllDeclared)!;

            Assert.That(() => factory.CreatePropertyMetadata(null!, propertyInfo), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("declaringType"));
        }

        [Test]
        public void CreatePropertyMetadata_WithNullPropertyInfo_ThrowsArgumentNullException()
        {
            Assert.That(() => factory.CreatePropertyMetadata(typeMock.Object, null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("propertyInfo"));
        }

        [Test]
        public void CreateMethodMetadata_WithRegularMethod_ReturnsMethodMetadata()
        {
            var methodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.RegularMethod), Acme.Bindings.AllDeclared)!;

            var result = factory.CreateMethodMetadata(typeMock.Object, methodInfo);

            Assert.That(result, Is.InstanceOf<IMethod>());
        }

        [Test]
        public void CreateMethodMetadata_WithOperatorMethod_ReturnsOperatorMetadata()
        {
            var methodInfo = typeof(Acme.SampleOperators).GetMethod("op_UnaryPlus", Acme.Bindings.AllDeclared)!;

            var result = factory.CreateMethodMetadata(typeMock.Object, methodInfo);

            Assert.That(result, Is.InstanceOf<IOperator>());
        }

        [Test]
        public void CreateMethodMetadata_WithExplicitInterfaceOperatorMethod_ReturnsOperatorMetadata()
        {
            var methodInfo = typeof(Acme.SampleOperators).GetMethod("Acme.ISampleInterface.op_False", Acme.Bindings.AllDeclared)!;

            var result = factory.CreateMethodMetadata(typeMock.Object, methodInfo);

            Assert.That(result, Is.InstanceOf<IOperator>());
        }

        [Test]
        public void CreateMethodMetadata_WithNullDeclaringType_ThrowsArgumentNullException()
        {
            var methodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.RegularMethod), Acme.Bindings.AllDeclared)!;

            Assert.That(() => factory.CreateMethodMetadata(null!, methodInfo), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("declaringType"));
        }

        [Test]
        public void CreateMethodMetadata_WithNullMethodInfo_ThrowsArgumentNullException()
        {
            Assert.That(() => factory.CreateMethodMetadata(typeMock.Object, null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("methodInfo"));
        }

        [Test]
        public void CreateEventMetadata_ReturnsEventMetadata()
        {
            var eventInfo = typeof(Acme.SampleEvents).GetEvent(nameof(Acme.SampleEvents.RegularEvent), Acme.Bindings.AllDeclared)!;

            var result = factory.CreateEventMetadata(typeMock.Object, eventInfo);

            Assert.That(result, Is.InstanceOf<IEvent>());
        }

        [Test]
        public void CreateEventMetadata_WithNullDeclaringType_ThrowsArgumentNullException()
        {
            var eventInfo = typeof(Acme.SampleEvents).GetEvent(nameof(Acme.SampleEvents.RegularEvent), Acme.Bindings.AllDeclared)!;

            Assert.That(() => factory.CreateEventMetadata(null!, eventInfo), Throws
                .ArgumentNullException.With.Property("ParamName").EqualTo("declaringType"));
        }

        [Test]
        public void CreateEventMetadata_WithNullEventInfo_ThrowsArgumentNullException()
        {
            Assert.That(() => factory.CreateEventMetadata(typeMock.Object, null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("eventInfo"));
        }

        [Test]
        public void CreateFieldMetadata_ReturnsFieldMetadata()
        {
            var fieldInfo = typeof(Acme.SampleFields).GetField(nameof(Acme.SampleFields.VolatileField), Acme.Bindings.AllDeclared)!;

            var result = factory.CreateFieldMetadata(typeMock.Object, fieldInfo);

            Assert.That(result, Is.InstanceOf<IField>());
        }

        [Test]
        public void CreateFieldMetadata_WithNullDeclaringType_ThrowsArgumentNullException()
        {
            var fieldInfo = typeof(Acme.SampleFields).GetField(nameof(Acme.SampleFields.VolatileField), Acme.Bindings.AllDeclared)!;

            Assert.That(() => factory.CreateFieldMetadata(null!, fieldInfo), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("declaringType"));
        }

        [Test]
        public void CreateFieldMetadata_WithNullFieldInfo_ThrowsArgumentNullException()
        {
            Assert.That(() => factory.CreateFieldMetadata(typeMock.Object, null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("fieldInfo"));
        }

        [Test]
        public void CreateParameterMetadata_ReturnsParameterMetadata()
        {
            var methodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.ArrayParamsMethod), Acme.Bindings.AllDeclared)!;
            var parameterInfo = methodInfo.GetParameters()[0];

            var result = factory.CreateParameterMetadata(memberMock.Object, parameterInfo);

            Assert.That(result, Is.InstanceOf<IParameter>());
        }

        [Test]
        public void CreateParameterMetadata_WithNullMember_ThrowsArgumentNullException()
        {
            var methodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.ArrayParamsMethod), Acme.Bindings.AllDeclared)!;
            var parameterInfo = methodInfo.GetParameters()[0];

            Assert.That(() => factory.CreateParameterMetadata(null!, parameterInfo), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("member"));
        }

        [Test]
        public void CreateParameterMetadata_WithNullParameterInfo_ThrowsArgumentNullException()
        {
            Assert.That(() => factory.CreateParameterMetadata(memberMock.Object, null!), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("parameterInfo"));
        }

        [Test]
        public void CreateCustomAttributeMetadata_ReturnsCustomAttributeMetadata()
        {
            var attributeData = typeof(Acme.SampleAttribute).GetCustomAttributesData()[0];

            var result = factory.CreateCustomAttributeMetadata(attributeData, AttributeTarget.Type);

            Assert.That(result, Is.InstanceOf<ICustomAttribute>());
        }

        [Test]
        public void CreateCustomAttributeMetadata_WithNullAttributeData_ThrowsArgumentNullException()
        {
            Assert.That(() => factory.CreateCustomAttributeMetadata(null!, AttributeTarget.Type), Throws.ArgumentNullException
                .With.Property("ParamName").EqualTo("attributeData"));
        }
    }
}
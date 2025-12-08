// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    [TestFixture]
    public class MetadataProviderTests
    {
        [SetUp]
        public void SetUp()
        {
            MetadataProvider.ClearCache();
        }

        [Test]
        public void GetMetadataAssembly_WithSameAssembly_ReturnsCachedInstance()
        {
            var assembly = typeof(Acme.ISampleInterface).Assembly;

            var first = assembly.GetMetadata();
            var second = assembly.GetMetadata();

            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void ClearCache_WhenCalled_RemovesAllAvailableAssemblies()
        {
            typeof(Acme.ISampleInterface).Assembly.GetMetadata();

            Assert.That(MetadataProvider.AvailableAssemblies, Is.Not.Empty);

            MetadataProvider.ClearCache();

            Assert.That(MetadataProvider.AvailableAssemblies, Is.Empty);
        }

        [Test]
        public void GetMetadata_ForUnsupportedMember_ThrowsNotSupportedException()
        {
            MemberInfo member = new UnsupportedMemberInfo();

            Assert.That(member.GetMetadata, Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void GetMetadata_ForType_NonGeneric_ReturnsTypedMetadata()
        {
            var type = typeof(Acme.ISampleInterface);

            var metadata = type.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IInterfaceType>());
            Assert.That(metadata.Represents(type), Is.True);
        }

        [Test]
        public void GetMetadata_ForType_Generic_ReturnsTypedMetadata()
        {
            var metadata = typeof(Acme.ISampleInterface).GetMetadata<IInterfaceType>();

            Assert.That(metadata, Is.InstanceOf<IInterfaceType>());
            Assert.That(metadata.Represents(typeof(Acme.ISampleInterface)), Is.True);
        }

        [Test]
        public void GetMetadata_ForConstructor_ReturnsSameInstanceAsConstructorAccessor()
        {
            var constructorInfo = typeof(Acme.SampleConstructors).GetConstructor([typeof(int)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IConstructor>());
            Assert.That(metadata.Represents(constructorInfo), Is.True);
        }

        [Test]
        public void GetMetadata_ForMethod_ReturnsSameInstanceAsMethodAccessor()
        {
            var methodInfo = typeof(Acme.SampleMethods).GetMethod(nameof(Acme.SampleMethods.RegularMethod));
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IMethod>());
            Assert.That(metadata.Represents(methodInfo), Is.True);
        }

        [Test]
        public void GetMetadata_ForProperty_ReturnsSameInstanceAsPropertyAccessor()
        {
            var propertyInfo = typeof(Acme.SampleProperties).GetProperty(nameof(Acme.SampleProperties.RegularProperty));
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IProperty>());
            Assert.That(metadata.Represents(propertyInfo), Is.True);
        }

        [Test]
        public void GetMetadata_ForEvent_ReturnsSameInstanceAsEventAccessor()
        {
            var eventInfo = typeof(Acme.SampleEvents).GetEvent(nameof(Acme.SampleEvents.RegularEvent));
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IEvent>());
            Assert.That(metadata.Represents(eventInfo), Is.True);
        }

        [Test]
        public void GetMetadata_ForField_ReturnsSameInstanceAsFieldAccessor()
        {
            var fieldInfo = typeof(Acme.SampleFields).GetField(nameof(Acme.SampleFields.ComplexField));
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();

            Assert.That(metadata, Is.InstanceOf<IField>());
            Assert.That(metadata.Represents(fieldInfo), Is.True);
        }

        [Test]
        public void GetMetadata_ForInstanceExtensionMethod_ReturnsExtensionMethodMetadata()
        {
            var methodInfo = typeof(Acme.SampleExtensions).GetMethod(nameof(Acme.SampleExtensions.InstanceExtensionMethodForClass));
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;

            Assert.That(metadata, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsStatic, Is.False);
                Assert.That(metadata.IsExtension, Is.True);
                Assert.That(metadata.ReceiverParameter?.Type.Name, Is.EqualTo(typeof(Acme.SampleGenericClass<>).Name));
                Assert.That(metadata.Parameters, Is.Empty);
                Assert.That(metadata.IsGenericMethod, Is.False);
                Assert.That(metadata.TypeParameters, Is.Empty);
                Assert.That(metadata.DeclaringType?.Name, Is.EqualTo(nameof(Acme.SampleExtensions)));
            }
        }

        [Test]
        public void GetMetadata_ForStaticExtensionMethod_ReturnsExtensionMethodMetadata()
        {
            var methodInfo = typeof(Acme.SampleExtensions).GetMethod(nameof(Acme.SampleExtensions.StaticExtensionMethodForClass));
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;

            Assert.That(metadata, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsStatic, Is.True);
                Assert.That(metadata.IsExtension, Is.True);
                Assert.That(metadata.ReceiverParameter?.Type.Name, Is.EqualTo(typeof(Acme.SampleGenericClass<>).Name));
                Assert.That(metadata.Parameters, Is.Empty);
                Assert.That(metadata.IsGenericMethod, Is.False);
                Assert.That(metadata.TypeParameters, Is.Empty);
                Assert.That(metadata.DeclaringType?.Name, Is.EqualTo(nameof(Acme.SampleExtensions)));
            }
        }

        [Test]
        public void GetMetadata_ForGenericExtensionMethod_ReturnsExtensionMethodMetadata()
        {
            var methodInfo = typeof(Acme.SampleExtensions).GetMethod(nameof(Acme.SampleExtensions.GenericExtensionMethod));
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata() as IMethod;

            Assert.That(metadata, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsExtension, Is.True);
                Assert.That(metadata.ReceiverParameter?.Type.Name, Is.EqualTo(nameof(Acme.ISampleInterface)));
                Assert.That(metadata.Parameters, Has.Count.EqualTo(1));
                Assert.That(metadata.IsGenericMethod, Is.True);
                Assert.That(metadata.TypeParameters.Select(tp => tp.Name), Is.EquivalentTo(["U"]));
                Assert.That(metadata.DeclaringType?.Name, Is.EqualTo(nameof(Acme.SampleExtensions)));
            }
        }

        [Test]
        public void GetMetadata_ForInstanceExtensionProperty_ReturnsExtensionPropertyMetadata()
        {
            var accessorInfo = typeof(Acme.SampleExtensions).GetMethod(nameof(Acme.SampleExtensions.get_InstanceExtensionProperty));
            Assert.That(accessorInfo, Is.Not.Null);

            var metadata = ((MemberInfo)accessorInfo).GetMetadata() as IProperty;

            Assert.That(metadata, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsStatic, Is.False);
                Assert.That(metadata.IsExtension, Is.True);
                Assert.That(metadata.ReceiverParameter?.Type.Name, Is.EqualTo(nameof(Acme.ISampleInterface)));
                Assert.That(metadata.DeclaringType?.Name, Is.EqualTo(nameof(Acme.SampleExtensions)));
            }
        }

        [Test]
        public void GetMetadata_ForStaticExtensionProperty_ReturnsExtensionPropertyMetadata()
        {
            var accessorInfo = typeof(Acme.SampleExtensions).GetMethod(nameof(Acme.SampleExtensions.get_StaticExtensionProperty));
            Assert.That(accessorInfo, Is.Not.Null);

            var metadata = ((MemberInfo)accessorInfo).GetMetadata() as IProperty;

            Assert.That(metadata, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(metadata.IsStatic, Is.True);
                Assert.That(metadata.IsExtension, Is.True);
                Assert.That(metadata.ReceiverParameter?.Type.Name, Is.EqualTo(nameof(Acme.ISampleInterface)));
                Assert.That(metadata.DeclaringType?.Name, Is.EqualTo(nameof(Acme.SampleExtensions)));
            }
        }

        [Test]
        public void FindTypeByFullName_WithRegisteredAssembly_ReturnsMatchingType()
        {
            typeof(Acme.ISampleInterface).Assembly.GetMetadata(); // Register the assembly

            var targetType = typeof(Acme.ISampleInterface);

            var result = MetadataProvider.FindTypeByFullName(targetType.FullName!);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Represents(targetType), Is.True);
        }

        [Test]
        public void FindTypeByFullName_WithoutAvailableAssemblies_SearchesCoreLibrary_ReturnsMatchingType()
        {
            var targetType = typeof(string);

            var result = MetadataProvider.FindTypeByFullName(targetType.FullName!);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Represents(targetType), Is.True);
        }

        [Test]
        public void FindTypeByFullName_WhenTypeDoesNotExist_ReturnsNull()
        {
            var result = MetadataProvider.FindTypeByFullName("System.NonExisting.TypeName");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetMetadataAssembly_WhenAssemblyIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => MetadataProvider.GetMetadata((Assembly)null!), Throws.ArgumentNullException);
        }

        [Test]
        public void GetMetadataType_WhenTypeIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => MetadataProvider.GetMetadata((System.Type)null!), Throws.ArgumentNullException);
        }

        [Test]
        public void GetMetadataConstructor_WhenConstructorIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => MetadataProvider.GetMetadata((ConstructorInfo)null!), Throws.ArgumentNullException);
        }

        [Test]
        public void GetMetadataMethod_WhenMethodIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => MetadataProvider.GetMetadata((MethodInfo)null!), Throws.ArgumentNullException);
        }

        [Test]
        public void GetMetadataProperty_WhenPropertyIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => MetadataProvider.GetMetadata((PropertyInfo)null!), Throws.ArgumentNullException);
        }

        [Test]
        public void GetMetadataEvent_WhenEventIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => MetadataProvider.GetMetadata((EventInfo)null!), Throws.ArgumentNullException);
        }

        [Test]
        public void GetMetadataField_WhenFieldIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => MetadataProvider.GetMetadata((FieldInfo)null!), Throws.ArgumentNullException);
        }

        [Test]
        public void AssemblyCache_WhenAssemblyIsGarbageCollected_RemovesFromAvailableAssemblies()
        {
            RegisterAndForgetAssembly();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Assert.That(MetadataProvider.AvailableAssemblies, Is.Empty);

            static void RegisterAndForgetAssembly()
            {
                var mockAssembly = new Mock<Assembly>();
                mockAssembly.Setup(a => a.GetName()).Returns(new AssemblyName("TestAssembly"));
                mockAssembly.Setup(a => a.Location).Returns(Path.GetTempFileName());
                mockAssembly.Setup(a => a.Modules).Returns([]);
                mockAssembly.Setup(a => a.CustomAttributes).Returns([]);
                mockAssembly.Setup(a => a.GetExportedTypes()).Returns([]);

                var _ = mockAssembly.Object.GetMetadata();
                Assert.That(MetadataProvider.AvailableAssemblies, Is.Not.Empty);
            }
        }

        private sealed class UnsupportedMemberInfo : MemberInfo
        {
            public override Type? DeclaringType => typeof(Acme.ISampleInterface);
            public override MemberTypes MemberType => MemberTypes.Custom;
            public override string Name => "UnsupportedMember";
            public override Type ReflectedType => typeof(Acme.ISampleInterface);
            public override Module Module => DeclaringType!.Module;

            public override object[] GetCustomAttributes(bool inherit) => [];
            public override object[] GetCustomAttributes(Type attributeType, bool inherit) => [];
            public override bool IsDefined(Type attributeType, bool inherit) => false;
        }
    }
}

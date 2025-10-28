// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Test;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.IO;
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
            var assembly = typeof(TestTypes.TestBaseClass).Assembly;

            var first = assembly.GetMetadata();
            var second = assembly.GetMetadata();

            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void ClearCache_WhenCalled_RemovesAllAvailableAssemblies()
        {
            typeof(TestTypes.TestBaseClass).Assembly.GetMetadata();

            Assert.That(MetadataProvider.AvailableAssemblies, Is.Not.Empty);

            MetadataProvider.ClearCache();

            Assert.That(MetadataProvider.AvailableAssemblies, Is.Empty);
        }

        [Test]
        public void GetMetadataType_WithRuntimeType_ReturnsRepresentingAdapter()
        {
            var type = typeof(TestTypes.TestDerivedClass);

            var metadata = type.GetMetadata();

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.Represents(type), Is.True);
        }

        [Test]
        public void GetMetadataGeneric_WithSupportedTargetInterface_ReturnsTypedMetadata()
        {
            var metadata = typeof(TestTypes.TestDerivedClass).GetMetadata<IClassType>();

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.Represents(typeof(TestTypes.TestDerivedClass)), Is.True);
        }

        [Test]
        public void GetMetadataMember_WithUnsupportedMember_ThrowsNotSupportedException()
        {
            MemberInfo member = new UnsupportedMemberInfo();

            Assert.That(member.GetMetadata, Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void GetMetadataMember_ForConstructor_ReturnsSameInstanceAsConstructorAccessor()
        {
            var constructorInfo = typeof(TestTypes.TestDerivedClass).GetConstructor([typeof(int)]);
            Assert.That(constructorInfo, Is.Not.Null);

            var metadata = constructorInfo.GetMetadata();

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.Represents(constructorInfo), Is.True);
        }

        [Test]
        public void GetMetadataMember_ForMethod_ReturnsSameInstanceAsMethodAccessor()
        {
            var methodInfo = typeof(TestTypes.TestDerivedClass).GetMethod(nameof(TestTypes.TestDerivedClass.RegularTestMethod));
            Assert.That(methodInfo, Is.Not.Null);

            var metadata = methodInfo.GetMetadata();

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.Represents(methodInfo), Is.True);
        }

        [Test]
        public void GetMetadataMember_ForProperty_ReturnsSameInstanceAsPropertyAccessor()
        {
            var propertyInfo = typeof(TestTypes.TestDerivedClass).GetProperty(nameof(TestTypes.TestDerivedClass.RegularTestProperty));
            Assert.That(propertyInfo, Is.Not.Null);

            var metadata = propertyInfo.GetMetadata();

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.Represents(propertyInfo), Is.True);
        }

        [Test]
        public void GetMetadataMember_ForEvent_ReturnsSameInstanceAsEventAccessor()
        {
            var eventInfo = typeof(TestTypes.TestDerivedClass).GetEvent(nameof(TestTypes.TestDerivedClass.RegularTestEvent));
            Assert.That(eventInfo, Is.Not.Null);

            var metadata = eventInfo.GetMetadata();

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.Represents(eventInfo), Is.True);
        }

        [Test]
        public void GetMetadataMember_ForField_ReturnsSameInstanceAsFieldAccessor()
        {
            var fieldInfo = typeof(TestTypes.TestBaseClass).GetField(nameof(TestTypes.TestBaseClass.TestField));
            Assert.That(fieldInfo, Is.Not.Null);

            var metadata = fieldInfo.GetMetadata();

            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.Represents(fieldInfo), Is.True);
        }

        [Test]
        public void FindTypeByFullName_WithRegisteredAssembly_ReturnsMatchingType()
        {
            typeof(TestTypes.TestBaseClass).Assembly.GetMetadata(); // Register the assembly

            var targetType = typeof(TestTypes.TestDerivedClass);

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
            public override Type? DeclaringType => typeof(TestTypes.TestBaseClass);
            public override MemberTypes MemberType => MemberTypes.Custom;
            public override string Name => "UnsupportedMember";
            public override Type ReflectedType => typeof(TestTypes.TestBaseClass);
            public override Module Module => DeclaringType!.Module;

            public override object[] GetCustomAttributes(bool inherit) => [];
            public override object[] GetCustomAttributes(Type attributeType, bool inherit) => [];
            public override bool IsDefined(Type attributeType, bool inherit) => false;
        }
    }
}

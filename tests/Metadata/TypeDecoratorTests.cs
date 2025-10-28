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
    public class TypeDecoratorTests
    {
        [Test]
        public void ArrayType_IsTypeDecorator()
        {
            var metadata = typeof(int[]).GetMetadata();

            Assert.That(metadata, Is.InstanceOf<ITypeDecorator>());
        }

        [Test]
        public void ArrayType_HasCorrectElementModifier()
        {
            var decorator = typeof(int[]).GetMetadata<ITypeDecorator>();

            Assert.That(decorator.Modifier, Is.EqualTo(TypeModifier.Array));
        }

        [Test]
        public void ArrayType_HasCorrectElementType()
        {
            var decorator = typeof(int[]).GetMetadata<ITypeDecorator>();

            Assert.That(decorator.ElementType.Name, Is.EqualTo("Int32"));
        }

        [Test]
        public void ArrayType_IsNotDirectDeclaration()
        {
            var decorator = typeof(int[]).GetMetadata<ITypeDecorator>();

            Assert.That(decorator.IsDirectDeclaration, Is.False);
        }

        [Test]
        public void ArrayType_HasCorrectArrayRank()
        {
            var decorator = typeof(int[]).GetMetadata<ITypeDecorator>();

            Assert.That(decorator.ArrayRank, Is.EqualTo(1));
        }

        [Test]
        public void MultidimensionalArrayType_IsTypeDecorator()
        {
            var metadata = typeof(int[,]).GetMetadata();

            Assert.That(metadata, Is.InstanceOf<ITypeDecorator>());
        }

        [Test]
        public void MultidimensionalArrayType_HasCorrectElementModifier()
        {
            var decorator = typeof(int[,]).GetMetadata<ITypeDecorator>();

            Assert.That(decorator.Modifier, Is.EqualTo(TypeModifier.Array));
        }

        [Test]
        public void MultidimensionalArrayType_IsNotDirectDeclaration()
        {
            var decorator = typeof(int[,]).GetMetadata<ITypeDecorator>();

            Assert.That(decorator.IsDirectDeclaration, Is.False);
        }

        [Test]
        public void MultidimensionalArrayType_HasCorrectArrayRank()
        {
            var decorator = typeof(int[,]).GetMetadata<ITypeDecorator>();

            Assert.That(decorator.ArrayRank, Is.EqualTo(2));
        }

        [Test]
        public void JaggedArrayType_IsTypeDecorator()
        {
            var metadata = typeof(int[][]).GetMetadata();

            Assert.That(metadata, Is.InstanceOf<ITypeDecorator>());
        }

        [Test]
        public void JaggedArrayType_HasCorrectElementModifier()
        {
            var decorator = typeof(int[][]).GetMetadata<ITypeDecorator>();

            Assert.That(decorator.Modifier, Is.EqualTo(TypeModifier.Array));
        }

        [Test]
        public void JaggedArrayType_IsNotDirectDeclaration()
        {
            var decorator = typeof(int[][]).GetMetadata<ITypeDecorator>();

            Assert.That(decorator.IsDirectDeclaration, Is.False);
        }

        [Test]
        public void JaggedArrayType_HasCorrectArrayRank()
        {
            var decorator = typeof(int[][]).GetMetadata<ITypeDecorator>();

            Assert.That(decorator.ArrayRank, Is.EqualTo(1));
        }

        [Test]
        public void PointerType_IsTypeDecorator()
        {
            var metadata = typeof(int).MakePointerType().GetMetadata();

            Assert.That(metadata, Is.InstanceOf<ITypeDecorator>());
        }

        [Test]
        public void PointerType_HasCorrectElementModifier()
        {
            var decorator = typeof(int).MakePointerType().GetMetadata<ITypeDecorator>();

            Assert.That(decorator.Modifier, Is.EqualTo(TypeModifier.Pointer));
        }

        [Test]
        public void PointerType_HasCorrectElementType()
        {
            var decorator = typeof(int).MakePointerType().GetMetadata<ITypeDecorator>();

            Assert.That(decorator.ElementType.Name, Is.EqualTo("Int32"));
        }

        [Test]
        public void PointerType_IsNotDirectDeclaration()
        {
            var decorator = typeof(int).MakePointerType().GetMetadata<ITypeDecorator>();

            Assert.That(decorator.IsDirectDeclaration, Is.False);
        }

        [Test]
        public void ByRefType_IsTypeDecorator()
        {
            var metadata = typeof(int).MakeByRefType().GetMetadata();

            Assert.That(metadata, Is.InstanceOf<ITypeDecorator>());
        }

        [Test]
        public void ByRefType_HasCorrectElementModifier()
        {
            var decorator = typeof(int).MakeByRefType().GetMetadata<ITypeDecorator>();

            Assert.That(decorator.Modifier, Is.EqualTo(TypeModifier.ByRef));
        }

        [Test]
        public void ByRefType_HasCorrectElementType()
        {
            var decorator = typeof(int).MakeByRefType().GetMetadata<ITypeDecorator>();

            Assert.That(decorator.ElementType.Name, Is.EqualTo("Int32"));
        }

        [Test]
        public void ByRefType_IsNotDirectDeclaration()
        {
            var decorator = typeof(int).MakeByRefType().GetMetadata<ITypeDecorator>();

            Assert.That(decorator.IsDirectDeclaration, Is.False);
        }

        [Test]
        public void NullableType_IsTypeDecorator()
        {
            var metadata = typeof(int?).GetMetadata();

            Assert.That(metadata, Is.InstanceOf<ITypeDecorator>());
        }

        [Test]
        public void NullableType_HasCorrectElementModifier()
        {
            var decorator = typeof(int?).GetMetadata<ITypeDecorator>();

            Assert.That(decorator.Modifier, Is.EqualTo(TypeModifier.Nullable));
        }

        [Test]
        public void NullableType_HasCorrectElementType()
        {
            var decorator = typeof(int?).GetMetadata<ITypeDecorator>();

            Assert.That(decorator.ElementType.Name, Is.EqualTo("Int32"));
        }

        [Test]
        public void NullableType_IsNotDirectDeclaration()
        {
            var decorator = typeof(int?).GetMetadata<ITypeDecorator>();

            Assert.That(decorator.IsDirectDeclaration, Is.False);
        }

        [Test]
        public void ComplexNestedDecorator_IsCorrectlyHandled()
        {
            // Represents ref int?*[,]: by-ref of array of pointers to nullable int
            var type = typeof(int?).MakePointerType().MakeArrayType(2).MakeByRefType();

            var byRefDecorator = (ITypeDecorator)type.GetMetadata();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(byRefDecorator.IsUnsafe, Is.True); // Because it contains a pointer
                Assert.That(byRefDecorator.Modifier, Is.EqualTo(TypeModifier.ByRef));
                Assert.That(byRefDecorator.ElementType, Is.InstanceOf<ITypeDecorator>());
                Assert.That(byRefDecorator.IsDirectDeclaration, Is.False);
            }

            var arrayDecorator = (ITypeDecorator)byRefDecorator.ElementType;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(arrayDecorator.IsUnsafe, Is.True); // Because it contains a pointer
                Assert.That(arrayDecorator.Modifier, Is.EqualTo(TypeModifier.Array));
                Assert.That(arrayDecorator.ElementType, Is.InstanceOf<ITypeDecorator>());
                Assert.That(arrayDecorator.ArrayRank, Is.EqualTo(2));
                Assert.That(arrayDecorator.IsDirectDeclaration, Is.False);
            }

            var pointerDecorator = (ITypeDecorator)arrayDecorator.ElementType;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(pointerDecorator.IsUnsafe, Is.True); // Because it is a pointer
                Assert.That(pointerDecorator.Modifier, Is.EqualTo(TypeModifier.Pointer));
                Assert.That(pointerDecorator.ElementType, Is.InstanceOf<ITypeDecorator>());
                Assert.That(pointerDecorator.IsDirectDeclaration, Is.False);
            }

            var nullableDecorator = (ITypeDecorator)pointerDecorator.ElementType;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(nullableDecorator.IsUnsafe, Is.False);
                Assert.That(nullableDecorator.Modifier, Is.EqualTo(TypeModifier.Nullable));
                Assert.That(nullableDecorator.ElementType.Name, Is.EqualTo("Int32"));
                Assert.That(nullableDecorator.IsDirectDeclaration, Is.False);
            }

            var underlyingType = nullableDecorator.ElementType;
            Assert.That(underlyingType, Is.Not.Null.And.Not.InstanceOf<ITypeDecorator>());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(underlyingType.IsUnsafe, Is.False);
                Assert.That(underlyingType.Name, Is.EqualTo("Int32"));
                Assert.That(underlyingType.IsDirectDeclaration, Is.True);
                Assert.That(underlyingType.CodeReference, Is.EqualTo("T:System.Int32"));
            }
        }

        [Test]
        public void Unwrap_ReturnsInnerMostElementType()
        {
            // Represents ref int?*[]: by-ref of array of pointers to nullable int
            var decorator = typeof(int?).MakePointerType().MakeArrayType().MakeByRefType().GetMetadata<ITypeDecorator>();

            Assert.That(decorator.Unwrap().Name, Is.EqualTo("Int32"));
        }

        [Test]
        public void Unwrap_CallsVisitorForEachDecorator()
        {
            // Represents ref int?*[]: by-ref of array of pointers to nullable int
            var decorator = typeof(int?).MakePointerType().MakeArrayType().MakeByRefType().GetMetadata<ITypeDecorator>();

            var decoratorsVisited = 0;
            var unwrappedType = decorator.Unwrap(_ => ++decoratorsVisited);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(decoratorsVisited, Is.EqualTo(4));
                Assert.That(unwrappedType.Name, Is.EqualTo("Int32"));
            }
        }

        [TestCase(typeof(int?), typeof(int?), ExpectedResult = true)]
        [TestCase(typeof(int?), typeof(bool?), ExpectedResult = false)]
        [TestCase(typeof(int?), typeof(int), ExpectedResult = true)]
        [TestCase(typeof(int?), typeof(bool), ExpectedResult = false)]
        [TestCase(typeof(int[]), typeof(int[]), ExpectedResult = true)]
        [TestCase(typeof(int[]), typeof(bool[]), ExpectedResult = false)]
        [TestCase(typeof(int*), typeof(int*), ExpectedResult = true)]
        [TestCase(typeof(int*), typeof(bool*), ExpectedResult = false)]
        [TestCase(typeof(void*), typeof(void*), ExpectedResult = true)]
        [TestCase(typeof(void*), typeof(bool*), ExpectedResult = false)]
        public bool IsAssignableFrom_ReturnsExpectedResults(Type targetType, Type sourceType)
        {
            var targetMetadata = targetType.GetMetadata();
            var sourceMetadata = sourceType.GetMetadata();

            return targetMetadata.IsAssignableFrom(sourceMetadata);
        }
    }
}

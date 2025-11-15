// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test
{
    using System;

    public static class TestTypes
    {
        public static string? ExtensionMethod(this TestExtensionTarget target) => target.ToString();

        public interface ITestInterface : ICloneable
        {
            int InterfaceTestProperty { get; }
            DateTime InterfaceTestDefaultProperty => DateTime.Now;

            void InterfaceTestMethod();
            int InterfaceTestDefaultMethod() => 42;
            T InterfaceGenericTestMethod<T>(T value);
            void InterfaceByRefParamTestMethod(in int value) { }

            event EventHandler InterfaceTestEvent;
        }

        public interface IExtendedTestInterface : ITestInterface, IDisposable { }

        public class TestBaseClass : ITestInterface
        {
            public TestBaseClass(int value) => TestField = value;

            public int TestField;

            public int InterfaceTestProperty => 0;
            public virtual bool VirtualTestProperty { get; set; }

            public void InterfaceTestMethod() { }
            public T InterfaceGenericTestMethod<T>(T value) => value;
            public void InterfaceByRefParamTestMethod(in int value) => TestField += value;
            public virtual void VirtualTestMethod() { }
            public virtual T VirtualGenericMethod<T>(T value) => value;
            public virtual object Clone() => MemberwiseClone();

            public event EventHandler? InterfaceTestEvent;
            public virtual event EventHandler? VirtualTestEvent { add { } remove { } }

            public static explicit operator int(TestBaseClass operand) => operand.TestField;
        }

        public class TestDerivedClass : TestBaseClass, IExtendedTestInterface
        {
            public TestDerivedClass() : base(0) { }
            public TestDerivedClass(int value) : base(value) { }

            public override bool VirtualTestProperty { get; set; }
            public int RegularTestProperty => TestField;

            public override void VirtualTestMethod() => TestField++;
            public override T VirtualGenericMethod<T>(T value) => value;
            public void RegularTestMethod() => TestField++;
            void IDisposable.Dispose() => GC.SuppressFinalize(this);

            public override event EventHandler? VirtualTestEvent;
            public event TestDelegate? RegularTestEvent;
        }

        public class TestGrandChildClass : TestDerivedClass { }

        public class GenericBaseClass<T> where T : struct
        {
            public GenericBaseClass() { }

            public virtual T? Value { get; } = default;
            public virtual T GetValueOrDefault() => Value ?? default;
        }

        public class GenericDerivedClass<T> : GenericBaseClass<T> where T : struct
        {
            public GenericDerivedClass() => Value = default(T);
            public GenericDerivedClass(T value) => Value = value;

            public override T? Value { get; }
            public override T GetValueOrDefault() => Value!.Value;
        }

        public class ConstructedGenericDerivedClass : GenericBaseClass<int>
        {
            public ConstructedGenericDerivedClass() { }

            public sealed override int? Value { get; } = 42;
            public sealed override int GetValueOrDefault() => 42;
        }

        public interface IGenericTestInterface<T> { }

        public interface IExtendedGenericTestInterface<T> : IGenericTestInterface<T> { }

        public class GenericImplementedClass<T> : IGenericTestInterface<T> { }

        public class ConstructedGenericImplementedClass : IGenericTestInterface<string> { }

        public class IndependentGenericClass<T> { }

        public class AnotherIndependentGenericClass<T> { }

        public class GenericMethodHost
        {
            public static T FirstMethod<T>(T x) => x;
            public static T SecondMethod<T>() => default!;
            public static T SecondMethod<T>(T x) => x;
        }

        public class TestExtensionTarget { }

        public class TestNestedClass
        {
            public class InnerClass
            {
                internal class DeepestClass
                {
                    public static int DeepestMethod() => 42;
                }
            }
        }

        public struct TestValueType : ITestInterface
        {
            public int TestField;

            public TestValueType(int value) => TestField = value;

            public readonly int RegularTestProperty => TestField;
            public readonly int InterfaceTestProperty => TestField;

            public readonly void RegularTestMethod() => RegularTestEvent?.Invoke(TestField);
            public void InterfaceTestMethod() => ++TestField;

            public readonly object Clone() => MemberwiseClone();

            readonly T ITestInterface.InterfaceGenericTestMethod<T>(T value) => value;
            void ITestInterface.InterfaceByRefParamTestMethod(in int value) => TestField += value;

            public event TestDelegate? RegularTestEvent;
            public event EventHandler? InterfaceTestEvent { add { } remove { } }

            public static implicit operator int(TestValueType operand) => operand.TestField;

            public readonly struct NestedValueType
            {
                public static int NestedMethod() => 42;
            }

            internal struct NonVisibleNestedValueType { }
        }

        public enum TestEnum
        {
            Value
        }

        public delegate void TestDelegate(int value);
    }
}

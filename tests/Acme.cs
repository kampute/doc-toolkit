// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1070 // Do not declare event fields as virtual
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value null
#pragma warning disable CS0067 // The event is never used
namespace Acme
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    public static class Bindings
    {
        public const BindingFlags AllDeclared = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class SampleAttribute : Attribute
    {
        public SampleAttribute(Type type) => Type = type;

        protected Type Type { get; }
        public DayOfWeek[] Days { get; set; } = [];
    }

    public interface ISampleInterface
    {
        static ISampleInterface()
        {
            InterfaceField = DateTime.Now;
        }

        public static readonly DateTime InterfaceField;

        int InterfaceProperty => 42;

        void InterfaceMethod() { }
        T InterfaceGenericMethod<T>(T value) where T : struct => value;
        void InterfaceMethodWithInParam(in int i) { }
        void InterfaceMethodWithRefParam(ref string s) { }
        void InterfaceMethodWithOutParam(out double d) { d = default; }

        static abstract void InterfaceStaticMethod();
        static virtual void InterfaceStaticDefaultMethod() { }

        static virtual bool operator true(ISampleInterface instance) => true;
        static abstract bool operator false(ISampleInterface instance);

        event EventHandler? InterfaceEvent { add { } remove { } }
    }

    public class SampleConstructors
    {
        [SetsRequiredMembers] protected SampleConstructors() { }
        public SampleConstructors(int i) { }
        public SampleConstructors(string s, double d) { }
        protected SampleConstructors([NotNull] object o) { }
        internal SampleConstructors(string s) { }
    }

    public class SampleOperators : ISampleInterface
    {
        public static SampleOperators operator +(SampleOperators x) => x;
        public static SampleOperators operator -(SampleOperators x) => x;

        public static SampleOperators operator +(SampleOperators x, SampleOperators y) => x;
        public static SampleOperators operator -(SampleOperators x, SampleOperators y) => x;

        public void operator +=(SampleOperators x) { }
        public void operator -=(SampleOperators x) { }

        public void operator ++() { }
        public void operator --() { }

        public static implicit operator string(SampleOperators x) => "Sample";
        public static explicit operator int(SampleOperators x) => 42;

        static bool ISampleInterface.operator false(ISampleInterface instance) => false;

        static void ISampleInterface.InterfaceStaticMethod() { }
    }

    public abstract class SampleMethods : ISampleInterface
    {
        public static void StaticMethod() { }
        public void RegularMethod() { }
        internal protected abstract void AbstractMethod();
        internal protected virtual void VirtualMethod(int i) { }
        public unsafe void UnsafeMethod(int** p) { }
        [return: NotNull] public T GenericMethodWithTypeConstraints<T>([NotNull] T value) where T : class, ICloneable, new() => value;
        public U GenericMethodWithoutTypeConstraints<T, U>(T t, U u) where T : class where U : struct => u;
        public S GenericMethodWithGenericParameter<S>(List<S> list) => list[0];
        public void RefParamsMethod(in int i, ref string s, out double d) { d = 0.0; }
        public void ArrayParamsMethod(params IEnumerable<string> args) { }
        public void OptionalParamsMethod(int i = 42, string s = "default") { }
        public unsafe void MixedParamsMethod(string s, in DayOfWeek day = DayOfWeek.Monday, params double?*[][,][,,,] values) { }
        public void OverloadedMethod() { }
        public void OverloadedMethod(string s, int i) { }
        void ISampleInterface.InterfaceMethod() { }
        public T InterfaceGenericMethod<T>(T value) where T : struct => value;
        void ISampleInterface.InterfaceMethodWithInParam(in int i) { }
        void ISampleInterface.InterfaceMethodWithOutParam(out double d) => d = default;
        public void InterfaceMethodWithRefParam(ref string s) { }
        static void ISampleInterface.InterfaceStaticMethod() { }
        public sealed override string ToString() => "SampleMethods";
        internal void InternalMethod() { }

        static bool ISampleInterface.operator false(ISampleInterface instance) => false;
    }

    public abstract class SampleProperties : ISampleInterface
    {
        public static string StaticProperty { get; set; } = "Static";

        public int RegularProperty { get; set; }
        internal protected abstract int AbstractProperty { get; }
        internal protected virtual int VirtualProperty { get; }
        int ISampleInterface.InterfaceProperty => 42;

        public int ReadOnlyProperty { get; }
        public int WriteOnlyProperty { set { } }
        public int InitOnlyProperty { get; init; }
        public required int RequiredProperty { get; set; }

        public int this[int i] => i;
        public string this[string s, int i] => $"{s}:{i}";

        internal int InternalProperty { get; set; }

        static void ISampleInterface.InterfaceStaticMethod() => throw new NotImplementedException();
        static bool ISampleInterface.operator false(ISampleInterface instance) => throw new NotImplementedException();
    }

    public abstract class SampleEvents : ISampleInterface
    {
        public static event EventHandler? StaticEvent;
        public event EventHandler? RegularEvent;
        internal protected abstract event EventHandler? AbstractEvent;
        internal protected virtual event EventHandler? VirtualEvent;
        event EventHandler? ISampleInterface.InterfaceEvent { add { } remove { } }

        internal event EventHandler? InternalEvent;

        static void ISampleInterface.InterfaceStaticMethod() => throw new NotImplementedException();
        static bool ISampleInterface.operator false(ISampleInterface instance) => throw new NotImplementedException();
    }

    public struct SampleFields
    {
        public static readonly int StaticReadonlyField = 10;
        public const string ConstField = "Constant";

        public volatile int VolatileField;
        public unsafe fixed byte FixedBuffer[16];
        public unsafe double?*[][,][,,] ComplexField;
        public int[] ArrayField;

        internal int InternalField;
    }

    public class SampleGenericClass<T>
        where T : class
    {
        public class InnerGenericClass<U, V>()
            where U : struct
            where V : notnull
        {
            public abstract class DeepInnerGenericClass : ISampleInterface, IEnumerable<V>
            {
                public const string Field = "DeepClass";

                public DeepInnerGenericClass() { }

                public virtual T? Property { get; set; }
                int ISampleInterface.InterfaceProperty => 42;

                public abstract void Method(T t, U u, V v);
                public virtual W GenericMethod<W>(T t, U u, V v, W w) where W : class, new() => w;
                void ISampleInterface.InterfaceMethod() { }
                static void ISampleInterface.InterfaceStaticMethod() { }

                IEnumerator<V> IEnumerable<V>.GetEnumerator() => throw new NotImplementedException();
                IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

                public static implicit operator T?(DeepInnerGenericClass instance) => instance.Property;
                static bool ISampleInterface.operator false(ISampleInterface instance) => false;

                public virtual event EventHandler? Event;
                event EventHandler? ISampleInterface.InterfaceEvent { add { } remove { } }
            }
        }

        internal class NonVisibleInnerClass { }
    }

    public abstract class SampleDerivedGenericClass<T, U, V> : SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass
        where T : class, new()
        where U : struct
        where V : notnull
    {
        public SampleDerivedGenericClass() : base() { }

        public override T? Property { get; set; }

        public override W GenericMethod<W>(T t, U u, V v, W w) => w;
        public override event EventHandler? Event;
    }

    public class SampleDerivedConstructedGenericClass : SampleDerivedGenericClass<object, int, string>
    {
        public SampleDerivedConstructedGenericClass() : base() { }
        public SampleDerivedConstructedGenericClass(object o) : base() => Property = o;

        public sealed override object? Property { get; set; }
        public override void Method(object t, int u, string v) { }
        public sealed override W GenericMethod<W>(object t, int u, string v, W w) => w;
        public sealed override event EventHandler? Event;
    }

    public class SampleDirectDerivedConstructedGenericClass : SampleGenericClass<object>.InnerGenericClass<int, string>.DeepInnerGenericClass
    {
        public sealed override void Method(object t, int u, string v) { }
    }

    public struct SampleGenericStruct<T>
        where T : class, IDisposable
    {
        public readonly struct InnerGenericStruct<U, V>
            where U : struct, allows ref struct
            where V : allows ref struct
        {
            public struct DeepInnerGenericStruct : ISampleInterface, IEnumerable<V>
            {
                public const string Field = "DeepStruct";

                public DeepInnerGenericStruct(T value) => Property = value;

                public readonly T? Property { get; }
                readonly int ISampleInterface.InterfaceProperty => 42;

                public readonly void Method(T t, U u, V v) { }
                public readonly W GenericMethod<W>(T t, U u, V v, W w) where W : class, new() => w;
                readonly void ISampleInterface.InterfaceMethod() { }
                static void ISampleInterface.InterfaceStaticMethod() { }

                readonly IEnumerator<V> IEnumerable<V>.GetEnumerator() => throw new NotImplementedException();
                readonly IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

                public static implicit operator T?(DeepInnerGenericStruct instance) => instance.Property;
                static bool ISampleInterface.operator false(ISampleInterface instance) => throw new NotImplementedException();

                public event EventHandler? Event;
                event EventHandler? ISampleInterface.InterfaceEvent { add { } remove { } }
            }
        }

        internal struct NonVisibleInnerStruct { }
    }

    public interface ISampleGenericInterface<T>
        where T : class, new()
    {
        public interface IInnerGenericInterface<in U, out V>
        {
            public interface IDeepInnerGenericInterface : ISampleInterface, IEnumerable<V>
            {
                const string Field = "DeepInterface";

                T? Property { get; set; }
                int ISampleInterface.InterfaceProperty => 42;

                V Method(T t, U u);
                W GenericMethod<W>(T t, U u, W w) where W : class;
                void ISampleInterface.InterfaceMethod() { }

                void operator ++();

                event EventHandler? Event;
                event EventHandler? ISampleInterface.InterfaceEvent { add { } remove { } }
            }
        }

        internal interface INonVisibleInnerInterface { }
    }

    public interface ISampleExtendedGenericInterface<T, in U, out V> : ISampleGenericInterface<T>.IInnerGenericInterface<U, V>.IDeepInnerGenericInterface
        where T : class, new()
    {
    }

    public interface ISampleExtendedConstructedGenericInterface : ISampleExtendedGenericInterface<object, int, string>
    {
    }

    public interface ISampleDirectExtendedConstructedGenericInterface : ISampleGenericInterface<object>.IInnerGenericInterface<int, string>.IDeepInnerGenericInterface
    {
    }

    public interface ISampleDirectAndIndirectExtendedInterface : IReadOnlyCollection<int>, IEnumerable<string>
    {
    }

    public static class SampleExtensions
    {
        /// <summary>
        /// A classic extension method for generic class.
        /// </summary>
        /// <typeparam name="T">The type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        public static void ClassicExtensionMethodForClass<T>(this SampleGenericClass<T> instance) where T : class { }

        /// <summary>
        /// Extension members for generic class.
        /// </summary>
        /// <typeparam name="T">The receiver type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        extension<T>(SampleGenericClass<T> instance)
            where T : class
        {
            /// <summary>
            /// An instance extension property for generic class.
            /// </summary>
            public int InstanceExtensionPropertyForClass => 42;

            /// <summary>
            /// A static extension property for generic class.
            /// </summary>
            public static bool StaticExtensionPropertyForClass => true;

            /// <summary>
            /// An instance extension method for generic class.
            /// </summary>
            public void InstanceExtensionMethodForClass() { }

            /// <summary>
            /// A static extension method for generic class.
            /// </summary>
            public static void StaticExtensionMethodForClass() { }

            /// <summary>
            /// A generic extension method for generic class.
            /// </summary>
            /// <typeparam name="U">The method type parameter.</typeparam>
            /// <param name="value">The parameter.</param>
            public void GenericExtensionMethodForClass<U>(U value) where U : struct { }
        }

        /// <summary>
        /// A classic extension method for generic struct.
        /// </summary>
        /// <typeparam name="T">The type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        public static void ClassicExtensionMethodForStruct<T>(this SampleGenericStruct<T> instance) where T : class, IDisposable { }

        /// <summary>
        /// Extension members for generic struct.
        /// </summary>
        /// <typeparam name="T">The receiver type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        extension<T>(SampleGenericStruct<T> instance)
            where T : class, IDisposable
        {
            /// <summary>
            /// An instance extension property for generic struct.
            /// </summary>
            public int InstanceExtensionPropertyForStruct => 42;

            /// <summary>
            /// A static extension property for generic struct.
            /// </summary>
            public static bool StaticExtensionPropertyForStruct => true;

            /// <summary>
            /// An instance extension method for generic struct.
            /// </summary>
            public void InstanceExtensionMethodForStruct() { }

            /// <summary>
            /// A static extension method for generic struct.
            /// </summary>
            public static void StaticExtensionMethodForStruct() { }

            /// <summary>
            /// A generic extension method for generic struct.
            /// </summary>
            /// <typeparam name="U">The method type parameter.</typeparam>
            /// <param name="value">The parameter.</param>
            public void GenericExtensionMethodForStruct<U>(U value) where U : struct { }
        }

        /// <summary>
        /// A classic extension method for generic interface.
        /// </summary>
        /// <typeparam name="T">The type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        public static void ClassicExtensionMethodForInterface<T>(this ISampleGenericInterface<T> instance) where T : class, new() { }

        /// <summary>
        /// Extension members for generic interface.
        /// </summary>
        /// <typeparam name="T">The receiver type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        extension<T>(ISampleGenericInterface<T> instance)
            where T : class, new()
        {
            /// <summary>
            /// An instance extension property for generic interface.
            /// </summary>
            public int InstanceExtensionPropertyForInterface => 42;

            /// <summary>
            /// A static extension property for generic interface.
            /// </summary>
            public static bool StaticExtensionPropertyForInterface => true;

            /// <summary>
            /// An instance extension method for generic interface.
            /// </summary>
            public void InstanceExtensionMethodForInterface() { }

            /// <summary>
            /// A static extension method for generic interface.
            /// </summary>
            public static void StaticExtensionMethodForInterface() { }

            /// <summary>
            /// A generic extension method for generic interface.
            /// </summary>
            /// <typeparam name="U">The method type parameter.</typeparam>
            /// <param name="value">The parameter.</param>
            public void GenericExtensionMethodForInterface<U>(U value) where U : struct { }
        }

        /// <summary>
        /// A classic extension method.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public static void ClassicExtensionMethod(this ISampleInterface instance) { }

        /// <summary>
        /// Extension members for non-generic interface.
        /// </summary>
        /// <param name="instance">The instance.</param>
        extension(ISampleInterface instance)
        {
            /// <summary>
            /// An instance extension property.
            /// </summary>
            public int InstanceExtensionProperty => 42;

            /// <summary>
            /// A static extension property.
            /// </summary>
            public static bool StaticExtensionProperty => true;

            /// <summary>
            /// A full extension property with getter and setter.
            /// </summary>
            public string FullExtensionProperty { get => string.Empty; set { } }

            /// <summary>
            /// An instance extension method.
            /// </summary>
            public void InstanceExtensionMethod() { }

            /// <summary>
            /// A static extension method.
            /// </summary>
            public static void StaticExtensionMethod() { }

            /// <summary>
            /// A generic extension method.
            /// </summary>
            /// <typeparam name="U">The type parameter.</typeparam>
            /// <param name="value">The parameter.</param>
            public void GenericExtensionMethod<U>(U value) where U : struct { }
        }

        /// <summary>
        /// A non-extension method to verify correct classification.
        /// </summary>
        public static void NonExtensionMethod() { }
    }
}
#pragma warning restore CS0067 // The event is never used
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CA1070 // Do not declare event fields as virtual
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0130 // Namespace does not match folder structure

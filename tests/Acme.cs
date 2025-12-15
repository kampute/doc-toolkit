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

    /// <summary>
    /// Binding flags for reflection tests.
    /// </summary>
    public static class Bindings
    {
        /// <summary>
        /// Binding flags to get all members declared/implemented on a type.
        /// </summary>
        public const BindingFlags AllDeclared = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    }

    /// <summary>
    /// A sample attribute for testing purposes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class SampleAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleAttribute">SampleAttribute</see> class.
        /// </summary>
        /// <param name="type">The constructor parameter.</param>
        public SampleAttribute(Type type) => Type = type;

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type value.</value>
        protected Type Type { get; }

        /// <summary>
        /// Gets or sets the days of the week.
        /// </summary>
        /// <value>The days of the week.</value>
        public DayOfWeek[] Days { get; set; } = [];
    }

    /// <summary>
    /// A sample interface for testing purposes.
    /// </summary>
    public interface ISampleInterface
    {
        /// <summary>
        /// Initializes the static members of the <see cref="ISampleInterface">ISampleInterface</see> interface.
        /// </summary>
        static ISampleInterface()
        {
            InterfaceField = DateTime.Now;
        }

        /// <summary>
        /// A static readonly field in interface.
        /// </summary>
        public static readonly DateTime InterfaceField;

        /// <summary>
        /// Gets the interface property value.
        /// </summary>
        /// <value>The interface property value.</value>
        int InterfaceProperty => 42;

        /// <summary>
        /// A sample interface method.
        /// </summary>
        void InterfaceMethod() { }

        /// <summary>
        /// A generic method in the interface.
        /// </summary>
        /// <typeparam name="T">The type parameter.</typeparam>
        /// <param name="value">The value parameter.</param>
        /// <returns>The value.</returns>
        T InterfaceGenericMethod<T>(T value) where T : struct => value;

        /// <summary>
        /// A method with an in parameter.
        /// </summary>
        /// <param name="i">The in parameter.</param>
        void InterfaceMethodWithInParam(in int i) { }

        /// <summary>
        /// A method with a ref parameter.
        /// </summary>
        /// <param name="s">The ref parameter.</param>
        void InterfaceMethodWithRefParam(ref string s) { }

        /// <summary>
        /// A method with an out parameter.
        /// </summary>
        /// <param name="d">The out parameter.</param>
        void InterfaceMethodWithOutParam(out double d) { d = default; }

        /// <summary>
        /// A static abstract method in the interface.
        /// </summary>
        static abstract void InterfaceStaticMethod();

        /// <summary>
        /// A static virtual method with default implementation in the interface.
        /// </summary>
        static virtual void InterfaceStaticDefaultMethod() { }

        /// <summary>
        /// Defines the true operator for the interface.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>Always true.</returns>
        static virtual bool operator true(ISampleInterface instance) => true;

        /// <summary>
        /// Defines the false operator for the interface.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>The result.</returns>
        static abstract bool operator false(ISampleInterface instance);

        /// <summary>
        /// An event in the interface.
        /// </summary>
        event EventHandler? InterfaceEvent { add { } remove { } }
    }

    /// <summary>
    /// A sample class demonstrating various constructor signatures.
    /// </summary>
    public class SampleConstructors
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleConstructors">SampleConstructors</see> class with required members set.
        /// </summary>
        [SetsRequiredMembers] protected SampleConstructors() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleConstructors">SampleConstructors</see> class with an integer parameter.
        /// </summary>
        /// <param name="i">The integer parameter.</param>
        public SampleConstructors(int i) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleConstructors">SampleConstructors</see> class with string and double parameters.
        /// </summary>
        /// <param name="s">The string parameter.</param>
        /// <param name="d">The double parameter.</param>
        public SampleConstructors(string s, double d) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleConstructors">SampleConstructors</see> class with an object parameter.
        /// </summary>
        /// <param name="o">The object parameter.</param>
        protected SampleConstructors([NotNull] object o) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleConstructors">SampleConstructors</see> class with a string parameter.
        /// </summary>
        /// <param name="s">The string parameter.</param>
        internal SampleConstructors(string s) { }
    }

    /// <summary>
    /// A sample class demonstrating operator overloading.
    /// </summary>
    public class SampleOperators : ISampleInterface
    {
        /// <summary>
        /// Unary plus operator.
        /// </summary>
        /// <param name="x">The operand.</param>
        /// <returns>The result.</returns>
        public static SampleOperators operator +(SampleOperators x) => x;

        /// <summary>
        /// Unary minus operator.
        /// </summary>
        /// <param name="x">The operand.</param>
        /// <returns>The result.</returns>
        public static SampleOperators operator -(SampleOperators x) => x;

        /// <summary>
        /// Addition operator.
        /// </summary>
        /// <param name="x">The left operand.</param>
        /// <param name="y">The right operand.</param>
        /// <returns>The result.</returns>
        public static SampleOperators operator +(SampleOperators x, SampleOperators y) => x;

        /// <summary>
        /// Subtraction operator.
        /// </summary>
        /// <param name="x">The left operand.</param>
        /// <param name="y">The right operand.</param>
        /// <returns>The result.</returns>
        public static SampleOperators operator -(SampleOperators x, SampleOperators y) => x;

        /// <summary>
        /// Addition assignment operator.
        /// </summary>
        /// <param name="x">The operand.</param>
        public void operator +=(SampleOperators x) { }

        /// <summary>
        /// Subtraction assignment operator.
        /// </summary>
        /// <param name="x">The operand.</param>
        public void operator -=(SampleOperators x) { }

        /// <summary>
        /// Increment operator.
        /// </summary>
        public void operator ++() { }

        /// <summary>
        /// Decrement operator.
        /// </summary>
        public void operator --() { }

        /// <summary>
        /// Implicit conversion to string.
        /// </summary>
        /// <param name="x">The instance.</param>
        /// <returns>The string representation.</returns>
        public static implicit operator string(SampleOperators x) => "Sample";

        /// <summary>
        /// Explicit conversion to int.
        /// </summary>
        /// <param name="x">The instance.</param>
        /// <returns>The integer value.</returns>
        public static explicit operator int(SampleOperators x) => 42;

        /// <summary>
        /// Explicitly implements the false operator for the interface.
        /// </summary>
        /// <inheritdoc/>
        static bool ISampleInterface.operator false(ISampleInterface instance) => false;

        /// <summary>
        /// Explicitly implements the static method for the interface.
        /// </summary>
        /// <inheritdoc/>
        static void ISampleInterface.InterfaceStaticMethod() { }
    }

    /// <summary>
    /// A sample abstract class demonstrating various method signatures.
    /// </summary>
    public abstract class SampleMethods : ISampleInterface
    {
        /// <summary>
        /// A static method.
        /// </summary>
        public static void StaticMethod() { }

        /// <summary>
        /// A regular instance method.
        /// </summary>
        public void RegularMethod() { }

        /// <summary>
        /// An abstract method.
        /// </summary>
        internal protected abstract void AbstractMethod();

        /// <summary>
        /// A virtual method.
        /// </summary>
        /// <param name="i">The parameter.</param>
        internal protected virtual void VirtualMethod(int i) { }

        /// <summary>
        /// An unsafe method.
        /// </summary>
        /// <param name="p">The pointer parameter.</param>
        public unsafe void UnsafeMethod(int** p) { }

        /// <summary>
        /// A generic method with type constraints.
        /// </summary>
        /// <typeparam name="T">The type parameter.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The value.</returns>
        [return: NotNull] public T GenericMethodWithTypeConstraints<T>([NotNull] T value) where T : class, ICloneable, new() => value;

        /// <summary>
        /// A generic method without type constraints.
        /// </summary>
        /// <typeparam name="T">The first type parameter.</typeparam>
        /// <typeparam name="U">The second type parameter.</typeparam>
        /// <param name="t">The first parameter.</param>
        /// <param name="u">The second parameter.</param>
        /// <returns>The second parameter.</returns>
        public U GenericMethodWithoutTypeConstraints<T, U>(T t, U u) where T : class where U : struct => u;

        /// <summary>
        /// A generic method with a generic parameter.
        /// </summary>
        /// <typeparam name="S">The type parameter.</typeparam>
        /// <param name="list">The list.</param>
        /// <returns>The first element.</returns>
        public S GenericMethodWithGenericParameter<S>(List<S> list) => list[0];

        /// <summary>
        /// A method with ref, in, and out parameters.
        /// </summary>
        /// <param name="i">The in parameter.</param>
        /// <param name="s">The ref parameter.</param>
        /// <param name="d">The out parameter.</param>
        public void RefParamsMethod(in int i, ref string s, out double d) { d = 0.0; }

        /// <summary>
        /// A method with params array.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void ArrayParamsMethod(params IEnumerable<string> args) { }

        /// <summary>
        /// A method with optional parameters.
        /// </summary>
        /// <param name="i">The integer parameter.</param>
        /// <param name="s">The string parameter.</param>
        public void OptionalParamsMethod(int i = 42, string s = "default") { }

        /// <summary>
        /// A method with mixed parameter types.
        /// </summary>
        /// <param name="s">The string parameter.</param>
        /// <param name="day">The day parameter.</param>
        /// <param name="values">The values array.</param>
        public unsafe void MixedParamsMethod(string s, in DayOfWeek day = DayOfWeek.Monday, params double?*[][,][,,,] values) { }

        /// <summary>
        /// An overloaded method.
        /// </summary>
        public void OverloadedMethod() { }

        /// <summary>
        /// An overloaded method with parameters.
        /// </summary>
        /// <param name="s">The string parameter.</param>
        /// <param name="i">The integer parameter.</param>
        public void OverloadedMethod(string s, int i) { }

        /// <summary>
        /// Explicitly implements the interface method.
        /// </summary>
        /// <inheritdoc/>
        void ISampleInterface.InterfaceMethod() { }

        /// <summary>
        /// Implements the generic interface method.
        /// </summary>
        /// <inheritdoc/>
        public T InterfaceGenericMethod<T>(T value) where T : struct => value;

        /// <summary>
        /// Explicitly implements the interface method with in parameter.
        /// </summary>
        /// <inheritdoc/>
        void ISampleInterface.InterfaceMethodWithInParam(in int i) { }

        /// <summary>
        /// Explicitly implements the interface method with out parameter.
        /// </summary>
        /// <inheritdoc/>
        void ISampleInterface.InterfaceMethodWithOutParam(out double d) => d = default;

        /// <summary>
        /// Implements the interface method with ref parameter.
        /// </summary>
        /// <inheritdoc/>
        public void InterfaceMethodWithRefParam(ref string s) { }

        /// <summary>
        /// Explicitly implements the static interface method.
        /// </summary>
        /// <inheritdoc/>
        static void ISampleInterface.InterfaceStaticMethod() { }

        /// <summary>
        /// Returns a string representation of the object.
        /// </summary>
        /// <returns>The string.</returns>
        public sealed override string ToString() => "SampleMethods";

        /// <summary>
        /// An internal method.
        /// </summary>
        internal void InternalMethod() { }

        /// <summary>
        /// Implements the false operator.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>Always false.</returns>
        static bool ISampleInterface.operator false(ISampleInterface instance) => false;
    }

    /// <summary>
    /// A sample abstract class demonstrating various property signatures.
    /// </summary>
    public abstract class SampleProperties : ISampleInterface
    {
        /// <summary>
        /// A static property.
        /// </summary>
        /// <value>The static property value.</value>
        public static string StaticProperty { get; set; } = "Static";

        /// <summary>
        /// A regular property.
        /// </summary>
        /// <value>The regular property value.</value>
        public int RegularProperty { get; set; }

        /// <summary>
        /// An abstract property.
        /// </summary>
        /// <value>The abstract property value.</value>
        internal protected abstract int AbstractProperty { get; }

        /// <summary>
        /// A virtual property.
        /// </summary>
        /// <value>The virtual property value.</value>
        internal protected virtual int VirtualProperty { get; }

        /// <summary>
        /// Explicitly implements the interface property.
        /// </summary>
        /// <inheritdoc/>
        int ISampleInterface.InterfaceProperty => 42;

        /// <summary>
        /// A read-only property.
        /// </summary>
        /// <value>The read-only property value.</value>
        public int ReadOnlyProperty { get; }

        /// <summary>
        /// A write-only property.
        /// </summary>
        /// <value>The write-only property value.</value>
        public int WriteOnlyProperty { set { } }

        /// <summary>
        /// An init-only property.
        /// </summary>
        /// <value>The init-only property value.</value>
        public int InitOnlyProperty { get; init; }

        /// <summary>
        /// A required property.
        /// </summary>
        /// <value>The required property value.</value>
        public required int RequiredProperty { get; set; }

        /// <summary>
        /// An indexer with one parameter.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <returns>The value.</returns>
        /// <value>The indexed value.</value>
        public int this[int i] => i;

        /// <summary>
        /// An indexer with two parameters.
        /// </summary>
        /// <param name="s">The string parameter.</param>
        /// <param name="i">The integer parameter.</param>
        /// <returns>The value.</returns>
        /// <value>The indexed value.</value>
        public string this[string s, int i] => $"{s}:{i}";

        /// <summary>
        /// An internal property.
        /// </summary>
        /// <value>The internal property value.</value>
        internal int InternalProperty { get; set; }

        /// <summary>
        /// Explicitly implements the static interface method.
        /// </summary>
        /// <inheritdoc/>
        static void ISampleInterface.InterfaceStaticMethod() => throw new NotImplementedException();

        /// <summary>
        /// Explicitly implements the false operator.
        /// </summary>
        /// <inheritdoc/>
        static bool ISampleInterface.operator false(ISampleInterface instance) => throw new NotImplementedException();
    }

    /// <summary>
    /// A sample abstract class demonstrating various event signatures.
    /// </summary>
    public abstract class SampleEvents : ISampleInterface
    {
        /// <summary>
        /// A static event.
        /// </summary>
        public static event EventHandler? StaticEvent;

        /// <summary>
        /// A regular event.
        /// </summary>
        public event EventHandler? RegularEvent;

        /// <summary>
        /// An abstract event.
        /// </summary>
        internal protected abstract event EventHandler? AbstractEvent;

        /// <summary>
        /// A virtual event.
        /// </summary>
        internal protected virtual event EventHandler? VirtualEvent;

        /// <summary>
        /// Explicitly implements the interface event.
        /// </summary>
        /// <inheritdoc/>
        event EventHandler? ISampleInterface.InterfaceEvent { add { } remove { } }

        /// <summary>
        /// An internal event.
        /// </summary>
        internal event EventHandler? InternalEvent;

        /// <summary>
        /// Explicitly implements the static interface method.
        /// </summary>
        /// <inheritdoc/>
        static void ISampleInterface.InterfaceStaticMethod() => throw new NotImplementedException();

        /// <summary>
        /// Explicitly implements the false operator.
        /// </summary>
        /// <inheritdoc/>
        static bool ISampleInterface.operator false(ISampleInterface instance) => throw new NotImplementedException();
    }

    /// <summary>
    /// A sample struct demonstrating various field types.
    /// </summary>
    public struct SampleFields
    {
        /// <summary>
        /// A static readonly field.
        /// </summary>
        public static readonly int StaticReadonlyField = 10;

        /// <summary>
        /// A constant field.
        /// </summary>
        public const string ConstField = "Constant";

        /// <summary>
        /// A volatile field.
        /// </summary>
        public volatile int VolatileField;

        /// <summary>
        /// A fixed buffer field.
        /// </summary>
        public unsafe fixed byte FixedBuffer[16];

        /// <summary>
        /// A complex unsafe field.
        /// </summary>
        public unsafe double?*[][,][,,] ComplexField;

        /// <summary>
        /// An array field.
        /// </summary>
        public int[] ArrayField;

        /// <summary>
        /// An internal field.
        /// </summary>
        internal int InternalField;
    }

    /// <summary>
    /// A sample generic class.
    /// </summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    public class SampleGenericClass<T>
        where T : class
    {
        /// <summary>
        /// An inner generic class.
        /// </summary>
        /// <typeparam name="U">The U type parameter.</typeparam>
        /// <typeparam name="V">The V type parameter.</typeparam>
        public class InnerGenericClass<U, V>()
            where U : struct
            where V : notnull
        {
            /// <summary>
            /// A deep inner generic abstract class.
            /// </summary>
            public abstract class DeepInnerGenericClass : ISampleInterface, IEnumerable<V>
            {
                /// <summary>
                /// A constant field.
                /// </summary>
                public const string Field = "DeepClass";

                /// <summary>
                /// Initializes a new instance of the <see cref="DeepInnerGenericClass">DeepInnerGenericClass</see> class.
                /// </summary>
                public DeepInnerGenericClass() { }

                /// <summary>
                /// A virtual property.
                /// </summary>
                /// <value>The virtual property value.</value>
                public virtual T? Property { get; set; }

                /// <summary>
                /// Explicitly implements the interface property.
                /// </summary>
                /// <inheritdoc/>
                int ISampleInterface.InterfaceProperty => 42;

                /// <summary>
                /// An abstract method.
                /// </summary>
                /// <param name="t">The t parameter.</param>
                /// <param name="u">The u parameter.</param>
                /// <param name="v">The v parameter.</param>
                public abstract void Method(T t, U u, V v);

                /// <summary>
                /// A virtual generic method.
                /// </summary>
                /// <typeparam name="W">The W parameter.</typeparam>
                /// <param name="t">The t parameter.</param>
                /// <param name="u">The u parameter.</param>
                /// <param name="v">The v parameter.</param>
                /// <param name="w">The w parameter.</param>
                /// <returns>The <paramref name="w">w</paramref> parameter.</returns>
                public virtual W GenericMethod<W>(T t, U u, V v, W w) where W : class, new() => w;

                /// <summary>
                /// Implements the interface method.
                /// </summary>
                /// <inheritdoc/>
                void ISampleInterface.InterfaceMethod() { }

                /// <summary>
                /// Explicitly implements the static interface method.
                /// </summary>
                /// <inheritdoc/>
                static void ISampleInterface.InterfaceStaticMethod() { }

                /// <summary>
                /// Explicitly implements the GetEnumerator method.
                /// </summary>
                /// <returns>The enumerator.</returns>
                IEnumerator<V> IEnumerable<V>.GetEnumerator() => throw new NotImplementedException();

                /// <summary>
                /// Explicitly implements the GetEnumerator method.
                /// </summary>
                /// <returns>The enumerator.</returns>
                IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

                /// <summary>
                /// Implicit conversion operator.
                /// </summary>
                /// <param name="instance">The instance.</param>
                /// <returns>The result.</returns>
                public static implicit operator T?(DeepInnerGenericClass instance) => instance.Property;

                /// <summary>
                /// Explicitly implements the false operator.
                /// </summary>
                /// <inheritdoc/>
                static bool ISampleInterface.operator false(ISampleInterface instance) => false;

                /// <summary>
                /// A virtual event.
                /// </summary>
                public virtual event EventHandler? Event;

                /// <summary>
                /// Explicitly implements the interface event.
                /// </summary>
                /// <inheritdoc/>
                event EventHandler? ISampleInterface.InterfaceEvent { add { } remove { } }
            }
        }

        /// <summary>
        /// A non-visible inner class.
        /// </summary>
        internal class NonVisibleInnerClass { }
    }

    /// <summary>
    /// A sample derived generic class.
    /// </summary>
    /// <typeparam name="T">The T parameter.</typeparam>
    /// <typeparam name="U">The U parameter.</typeparam>
    /// <typeparam name="V">The V parameter.</typeparam>
    public abstract class SampleDerivedGenericClass<T, U, V> : SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass
        where T : class, new()
        where U : struct
        where V : notnull
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleDerivedGenericClass{T,U,V}">SampleDerivedGenericClass&lt;T,U,V&gt;</see> class.
        /// </summary>
        public SampleDerivedGenericClass() : base() { }

        /// <summary>
        /// Overrides the virtual property.
        /// </summary>
        /// <inheritdoc/>
        public override T? Property { get; set; }

        /// <summary>
        /// Overrides the virtual generic method.
        /// </summary>
        /// <inheritdoc/>
        public override W GenericMethod<W>(T t, U u, V v, W w) => w;

        /// <summary>
        /// Overrides the virtual event.
        /// </summary>
        /// <inheritdoc/>
        public override event EventHandler? Event;
    }

    /// <summary>
    /// A sample derived constructed generic class.
    /// </summary>
    public class SampleDerivedConstructedGenericClass : SampleDerivedGenericClass<object, int, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SampleDerivedConstructedGenericClass">SampleDerivedConstructedGenericClass</see> class.
        /// </summary>
        public SampleDerivedConstructedGenericClass() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleDerivedConstructedGenericClass">SampleDerivedConstructedGenericClass</see> class with an object parameter.
        /// </summary>
        /// <param name="o">The object parameter.</param>
        public SampleDerivedConstructedGenericClass(object o) : base() => Property = o;

        /// <summary>
        /// Overrides the property.
        /// </summary>
        /// <inheritdoc/>
        public sealed override object? Property { get; set; }

        /// <summary>
        /// Overrides the abstract method.
        /// </summary>
        /// <inheritdoc/>
        public override void Method(object t, int u, string v) { }

        /// <summary>
        /// Overrides the generic method.
        /// </summary>
        /// <inheritdoc/>
        public sealed override W GenericMethod<W>(object t, int u, string v, W w) => w;

        /// <summary>
        /// Overrides the event.
        /// </summary>
        /// <inheritdoc/>
        public sealed override event EventHandler? Event;
    }

    /// <summary>
    /// A sample direct derived constructed generic class.
    /// </summary>
    public class SampleDirectDerivedConstructedGenericClass : SampleGenericClass<object>.InnerGenericClass<int, string>.DeepInnerGenericClass
    {
        /// <summary>
        /// Overrides the abstract method.
        /// </summary>
        /// <inheritdoc/>
        public sealed override void Method(object t, int u, string v) { }
    }

    /// <summary>
    /// A sample generic struct.
    /// </summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    public struct SampleGenericStruct<T>
        where T : class, IDisposable
    {
        /// <summary>
        /// An inner generic readonly struct.
        /// </summary>
        /// <typeparam name="U">The U parameter.</typeparam>
        /// <typeparam name="V">The V parameter.</typeparam>
        public readonly struct InnerGenericStruct<U, V>
            where U : struct, allows ref struct
            where V : allows ref struct
        {
            /// <summary>
            /// A deep inner generic struct.
            /// </summary>
            public struct DeepInnerGenericStruct : ISampleInterface, IEnumerable<V>
            {
                /// <summary>
                /// A constant field.
                /// </summary>
                public const string Field = "DeepStruct";

                /// <summary>
                /// Initializes a new instance of the <see cref="DeepInnerGenericStruct">DeepInnerGenericStruct</see> struct.
                /// </summary>
                /// <param name="value">The value.</param>
                public DeepInnerGenericStruct(T value) => Property = value;

                /// <summary>
                /// A readonly property.
                /// </summary>
                /// <value>The readonly property value.</value>
                public readonly T? Property { get; }

                /// <summary>
                /// Explicitly implements the interface property.
                /// </summary>
                /// <inheritdoc/>
                readonly int ISampleInterface.InterfaceProperty => 42;

                /// <summary>
                /// A readonly method.
                /// </summary>
                /// <param name="t">The t parameter.</param>
                /// <param name="u">The u parameter.</param>
                /// <param name="v">The v parameter.</param>
                public readonly void Method(T t, U u, V v) { }

                /// <summary>
                /// A readonly generic method.
                /// </summary>
                /// <typeparam name="W">The W parameter.</typeparam>
                /// <param name="t">The t parameter.</param>
                /// <param name="u">The u parameter.</param>
                /// <param name="v">The v parameter.</param>
                /// <param name="w">The w parameter.</param>
                /// <returns>The <paramref name="w"/> parameter.</returns>
                public readonly W GenericMethod<W>(T t, U u, V v, W w) where W : class, new() => w;

                /// <summary>
                /// Implements the interface method.
                /// </summary>
                /// <inheritdoc/>
                readonly void ISampleInterface.InterfaceMethod() { }

                /// <summary>
                /// Explicitly implements the static interface method.
                /// </summary>
                /// <inheritdoc/>
                static void ISampleInterface.InterfaceStaticMethod() { }

                /// <summary>
                /// Explicitly implements the GetEnumerator method.
                /// </summary>
                /// <returns>The enumerator.</returns>
                readonly IEnumerator<V> IEnumerable<V>.GetEnumerator() => throw new NotImplementedException();

                /// <summary>
                /// Explicitly implements the GetEnumerator method.
                /// </summary>
                /// <returns>The enumerator.</returns>
                readonly IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

                /// <summary>
                /// Implicit conversion operator.
                /// </summary>
                /// <param name="instance">The instance.</param>
                /// <returns>The result.</returns>
                public static implicit operator T?(DeepInnerGenericStruct instance) => instance.Property;

                /// <summary>
                /// Explicitly implements the false operator.
                /// </summary>
                /// <inheritdoc/>
                static bool ISampleInterface.operator false(ISampleInterface instance) => false;

                /// <summary>
                /// An event.
                /// </summary>
                public event EventHandler? Event;

                /// <summary>
                /// Explicitly implements the interface event.
                /// </summary>
                /// <inheritdoc/>
                event EventHandler? ISampleInterface.InterfaceEvent { add { } remove { } }
            }
        }

        /// <summary>
        /// A non-visible inner struct.
        /// </summary>
        internal struct NonVisibleInnerStruct { }
    }

    /// <summary>
    /// A sample generic interface.
    /// </summary>
    /// <typeparam name="T">The type parameter.</typeparam>
    public interface ISampleGenericInterface<T>
        where T : class, new()
    {
        /// <summary>
        /// An inner generic interface.
        /// </summary>
        /// <typeparam name="U">The U parameter.</typeparam>
        /// <typeparam name="V">The V parameter.</typeparam>
        public interface IInnerGenericInterface<in U, out V>
        {
            /// <summary>
            /// A deep inner generic interface.
            /// </summary>
            public interface IDeepInnerGenericInterface : ISampleInterface, IEnumerable<V>
            {
                /// <summary>
                /// A constant field.
                /// </summary>
                const string Field = "DeepInterface";

                /// <summary>
                /// A property.
                /// </summary>
                /// <value>The property value.</value>
                T? Property { get; set; }

                /// <summary>
                /// Explicitly implements the interface property.
                /// </summary>
                /// <inheritdoc/>
                int ISampleInterface.InterfaceProperty => 42;

                /// <summary>
                /// A method.
                /// </summary>
                /// <param name="t">The t parameter.</param>
                /// <param name="u">The u parameter.</param>
                /// <returns>The result.</returns>
                V Method(T t, U u);

                /// <summary>
                /// A generic method.
                /// </summary>
                /// <typeparam name="W">The W parameter.</typeparam>
                /// <param name="t">The t parameter.</param>
                /// <param name="u">The u parameter.</param>
                /// <param name="w">The w parameter.</param>
                /// <returns>The result.</returns>
                W GenericMethod<W>(T t, U u, W w) where W : class;

                /// <summary>
                /// Explicitly implements the interface method.
                /// </summary>
                /// <inheritdoc/>
                void ISampleInterface.InterfaceMethod() { }

                /// <summary>
                /// Increment operator.
                /// </summary>
                void operator ++();

                /// <summary>
                /// An event.
                /// </summary>
                event EventHandler? Event;

                /// <summary>
                /// Explicitly implements the interface event.
                /// </summary>
                /// <inheritdoc/>
                event EventHandler? ISampleInterface.InterfaceEvent { add { } remove { } }
            }
        }

        /// <summary>
        /// A non-visible inner interface.
        /// </summary>
        internal interface INonVisibleInnerInterface { }
    }

    /// <summary>
    /// A sample extended generic interface.
    /// </summary>
    /// <typeparam name="T">The T parameter.</typeparam>
    /// <typeparam name="U">The U parameter.</typeparam>
    /// <typeparam name="V">The V parameter.</typeparam>
    public interface ISampleExtendedGenericInterface<T, in U, out V> : ISampleGenericInterface<T>.IInnerGenericInterface<U, V>.IDeepInnerGenericInterface
        where T : class, new()
    {
    }

    /// <summary>
    /// A sample extended constructed generic interface.
    /// </summary>
    public interface ISampleExtendedConstructedGenericInterface : ISampleExtendedGenericInterface<object, int, string>
    {
    }

    /// <summary>
    /// A sample direct extended constructed generic interface.
    /// </summary>
    public interface ISampleDirectExtendedConstructedGenericInterface : ISampleGenericInterface<object>.IInnerGenericInterface<int, string>.IDeepInnerGenericInterface
    {
    }

    /// <summary>
    /// A sample interface extending directly and indirectly.
    /// </summary>
    public interface ISampleDirectAndIndirectExtendedInterface : IReadOnlyCollection<int>, IEnumerable<string>
    {
    }

    /// <summary>
    /// Defines some extension members for testing purposes.
    /// </summary>
    public static class SampleExtensions
    {
        /// <summary>
        /// A classic extension method for generic class.
        /// </summary>
        /// <typeparam name="W">The type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        public static void ClassicExtensionMethodForClass<W>(this SampleGenericClass<W> instance) where W : class { }

        /// <summary>
        /// Extension members for generic class.
        /// </summary>
        /// <typeparam name="W">The receiver type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        extension<W>(SampleGenericClass<W> instance)
            where W : class
        {
            /// <summary>
            /// An instance extension property for generic class.
            /// </summary>
            /// <value>The value of the instance extension property.</value>
            public int InstanceExtensionPropertyForClass => 42;

            /// <summary>
            /// A static extension property for generic class.
            /// </summary>
            /// <value>The value of the static extension property.</value>
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
        /// <typeparam name="W">The type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        public static void ClassicExtensionMethodForStruct<W>(this SampleGenericStruct<W> instance) where W : class, IDisposable { }

        /// <summary>
        /// Extension members for generic struct.
        /// </summary>
        /// <typeparam name="W">The receiver type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        extension<W>(SampleGenericStruct<W> instance)
            where W : class, IDisposable
        {
            /// <summary>
            /// An instance extension property for generic struct.
            /// </summary>
            /// <value>The value of the instance extension property.</value>
            public int InstanceExtensionPropertyForStruct => 42;

            /// <summary>
            /// A static extension property for generic struct.
            /// </summary>
            /// <value>The value of the static extension property.</value>
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
        /// <typeparam name="W">The type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        public static void ClassicExtensionMethodForInterface<W>(this ISampleGenericInterface<W> instance) where W : class, new() { }

        /// <summary>
        /// Extension members for generic interface.
        /// </summary>
        /// <typeparam name="W">The receiver type parameter.</typeparam>
        /// <param name="instance">The instance.</param>
        extension<W>(ISampleGenericInterface<W> instance)
            where W : class, new()
        {
            /// <summary>
            /// An instance extension property for generic interface.
            /// </summary>
            /// <value>The value of the instance extension property.</value>
            public int InstanceExtensionPropertyForInterface => 42;

            /// <summary>
            /// A static extension property for generic interface.
            /// </summary>
            /// <value>The value of the static extension property.</value>
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
            /// <value>The value of the instance extension property.</value>
            public int InstanceExtensionProperty => 42;

            /// <summary>
            /// A static extension property.
            /// </summary>
            /// <value>The value of the static extension property.</value>
            public static bool StaticExtensionProperty => true;

            /// <summary>
            /// An extension property with both getter and setter.
            /// </summary>
            /// <value>The value of the full extension property.</value>
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

    /// <summary>
    /// Documentation for the Acme namespace.
    /// </summary>
    internal static class NamespaceDoc { }
}
#pragma warning restore CS0067 // The event is never used
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CA1070 // Do not declare event fields as virtual
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0130 // Namespace does not match folder structure

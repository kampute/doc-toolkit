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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    [AttributeUsage(AttributeTargets.Parameter)]
    public class CustomAttribute : Attribute { }

    public interface IProcess<out T>
    {
        bool IsCompleted { get; }

        event EventHandler? Completed;

        T GetStatus(bool state);
    }

    public struct ValueType : ICloneable
    {
        public volatile int total;
        public unsafe fixed int buffer[10];

        public ValueType(int initial) => total = initial;

        public readonly bool HasTotal => total != 0;

        public int HalfTotal
        {
            readonly get => total / 2;
            set => total = value * 2;
        }

        public unsafe void M1(int i) => buffer[i % 10] = ++total;
        public readonly string M2(int? i = null) => $"{i}";

        public readonly object Clone() => new ValueType(total);

        public event EventHandler? E1;

        public enum State
        {
            Empty,
            NotEmpty
        }
    }

    /// <seealso href="https://ecma-international.org/wp-content/uploads/ECMA-334_7th_edition_december_2023.pdf"/>
    public class Widget
    {
        public string message;
        internal static Direction defaultDirection;
        public const double PI = 3.14159;
        protected internal readonly double monthlyAverage;
        public long[] array1;
        public Widget[,] array2;
        public unsafe int* pCount;
        public unsafe float** ppValues;
        private volatile bool valid;

        static Widget() { }

        [SetsRequiredMembers]
        internal Widget() => Width = 0;

        [SetsRequiredMembers]
        protected Widget(string s) => Width = s.Length;

        [SetsRequiredMembers]
        public Widget(int width, int height)
        {
            Width = width;
            Height = height;
            valid = true;
            Validated?.Invoke(this, EventArgs.Empty);
        }

        public static void M0() { }
        public void M1(char c, out float f, ref ValueType v, in int i) { f = 0; }
        public void M2(short[] x1, int[,] x2, long[][] x3) { }
        public void M3(long[][] x3, Widget[][,,] x4) { }
        public unsafe void M4(char* pc, Direction** pd = null) { }
        public unsafe void M5(void* pv, double?*[][,][,,] pd) { }
        public void M6(int i, params object[] args) { valid = i % 2 == 0; }
        public T? M7<T>() => valid ? default : throw new InvalidOperationException();
        public void M8<T>(T t, bool isTrusted = true) where T : class, new() { }
        private bool Valid() => valid;

        [return: NotNullIfNotNull(nameof(x))]
        public static implicit operator long?(Widget? x) => x?.Width;
        public static explicit operator int(Widget x) => x.Width;
        public static Widget operator +(Widget x) => x;
        public static Widget operator +(Widget x1, Widget x2) => x1;

        public static Direction DefaultDirection
        {
            get => defaultDirection;
            set => defaultDirection = value;
        }

        public required int Width { get; init; }
        public int Height { get; set; }
        public int Depth { get; private set; }
        private bool IsValid => Valid();

        public int this[int i] => IsValid ? i : -1;
        public int this[string s, int i] => i;

        protected internal event Del AnEvent;
        internal event EventHandler? Validated;

        public abstract class NestedClass
        {
            protected NestedClass() => value = 999;

            public int value;

            public void M1() { }
            public abstract void M2(int i = 123);

            public enum DeepNestedEnum
            {
                Value1,
                Value2
            }
        }

        public class NestedDerivedClass : NestedClass
        {
            public NestedDerivedClass() : base() { }
            public NestedDerivedClass(int initialValue) => value = initialValue;
            public NestedDerivedClass(string name) => value = name.Length;

            public new void M1() { }
            public sealed override void M2(int i = 123) { }
        }

        public interface IMenuItem<in T> where T : Widget, new()
        {
        }

        protected internal delegate void Del(int i);

        public enum Direction
        {
            North,
            South,
            East,
            West
        }

        private struct PrivateType { }
    }

    public class DerivedWidget : Widget
    {
        [SetsRequiredMembers]
        public DerivedWidget() : base() { }
    }

    public class MyList<T>(IEnumerable<T> items)
        where T : struct
    {
        public IEnumerable<T> Items { get; protected set; } = items;

        protected internal void Test([Custom] T t = default) { }

        public class Helper<U, V> { }
    }

    public class MyExtendedList<T> : MyList<T>
        where T : struct
    {
        public MyExtendedList() : base([]) { }
    }

    public sealed class UseList : IProcess<string>
    {
        bool IProcess<string>.IsCompleted => true;

        event EventHandler? IProcess<string>.Completed
        {
            add { }
            remove { }
        }

        string IProcess<string>.GetStatus(bool state) => string.Empty;

        public async Task<bool> ProcessAsync(MyList<int> list) => await Task.FromResult(list.Items is not null);
        public MyList<T> GetValues<T>(T value) where T : struct => new([value]);

        [Obsolete("For sake of testing", DiagnosticId = "XYZ")]
        public T Intercept<T>(in MyList<T>.Helper<char, string>[] helper) where T : struct => default;
    }

    public static class Extensions
    {
        public static void Hello(this Widget widget) => widget.message = "Hello, World!";
    }

    public interface ITestInterface
    {
        int Count { get; }
        bool IsEmpty => Count == 0;

        int this[int i] { get; }

        void InterfaceMethod();
        void InterfaceDefaultMethod() { }

        event EventHandler? InterfaceEvent;
    }

    public class BaseClass
    {
        public virtual string VirtualProperty { get; set; } = "Base";
        public string RegularProperty { get; set; } = "Regular";
        public virtual int VirtualReadOnly { get; } = 42;

        protected virtual event EventHandler? VirtualEvent;

        public virtual string VirtualMethod() => "Base";
        public virtual async Task<string> VirtualAsyncMethod() => await Task.FromResult("Base Async");
    }

    public class DerivedClass : BaseClass, ITestInterface, IProcess<string>
    {
        public sealed override string VirtualProperty { get; set; } = "Derived";
        public override int VirtualReadOnly { get; } = 99;
        public int Count => 10;
        public int this[int i] => i * 2;
        public bool InitOnlyProperty { get; init; }
        public required int RequiredProperty { get; set; }
        public static string StaticProperty { get; set; } = "Static";

        bool IProcess<string>.IsCompleted => true;

        public void RegularMethod() { }
        public void InterfaceMethod() { }
        public sealed override string VirtualMethod() => "Derived";
        public override async Task<string> VirtualAsyncMethod() => await Task.FromResult("Derived Async");
        string IProcess<string>.GetStatus(bool state) => state ? "Complete" : "Incomplete";

        protected override event EventHandler? VirtualEvent;
        public static event EventHandler<int>? StaticEvent;
        internal event Widget.Del? RegularEvent;
        public event EventHandler? InterfaceEvent;
        event EventHandler? IProcess<string>.Completed
        {
            add { }
            remove { }
        }
    }

    public class TestClass
    {
        public TestClass() { }
        public TestClass(int value) { }
        public TestClass(string name) { }
        public TestClass(int value, string name) { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class ExampleAttribute : Attribute
    {
        public ExampleAttribute(Type type) => Type = type;

        protected Type Type { get; }
        public DayOfWeek Day { get; set; } = DayOfWeek.Monday;
    }

    public class UnmanagedConstraintClass<T> where T : unmanaged
    {
        public void TestMethod<U>(U value) where U : unmanaged { }
    }

    public class NotNullConstraintClass<T> where T : notnull
    {
        public void TestMethod<U>(U value) where U : notnull { }
    }

    public class MultipleConstraintClass<T, U> where T : class, ICloneable where U : class, new()
    {
        public T? Value { get; set; }
    }
}
#pragma warning restore CS0067 // The event is never used
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value null
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CA1070 // Do not declare event fields as virtual
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0130 // Namespace does not match folder structure

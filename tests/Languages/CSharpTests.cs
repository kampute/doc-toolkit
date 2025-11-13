// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Languages
{
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    [TestFixture]
    public class CSharpTests
    {
        private readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        private readonly CSharp csharp = new();

        [SetUp]
        public void Setup()
        {
            MetadataProvider.RegisterRuntimeAssemblies(); // To ensure all code references are resolvable
        }

        [TestCase("abstract", ExpectedResult = "https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/abstract")]
        [TestCase("as", ExpectedResult = "https://learn.microsoft.com/dotnet/csharp/language-reference/operators/type-testing-and-cast#as-operator")]
        [TestCase("nonexistent", ExpectedResult = null)]
        public string? TryGetUrl_ReturnsExpectedDocumentationUrl(string keyword)
        {
            return csharp.TryGetUrl(keyword, out var url) ? url.ToString() : null;
        }

        [TestCase(null, ExpectedResult = "null")]
        [TestCase(true, ExpectedResult = "true")]
        [TestCase(false, ExpectedResult = "false")]
        [TestCase(42, ExpectedResult = "42")]
        [TestCase(42u, ExpectedResult = "42")]
        [TestCase(42L, ExpectedResult = "42")]
        [TestCase(42uL, ExpectedResult = "42")]
        [TestCase(42.25f, ExpectedResult = "42.25F")]
        [TestCase(42.25d, ExpectedResult = "42.25")]
        [TestCase(DayOfWeek.Monday, ExpectedResult = "DayOfWeek.Monday")]
        [TestCase(BindingFlags.Static | BindingFlags.Public, ExpectedResult = "BindingFlags.Static | BindingFlags.Public")]
        [TestCase('C', ExpectedResult = "'C'")]
        [TestCase('\\', ExpectedResult = "'\\\\'")]
        [TestCase('\0', ExpectedResult = "'\\0'")]
        [TestCase('\a', ExpectedResult = "'\\a'")]
        [TestCase('\b', ExpectedResult = "'\\b'")]
        [TestCase('\e', ExpectedResult = "'\\e'")]
        [TestCase('\f', ExpectedResult = "'\\f'")]
        [TestCase('\n', ExpectedResult = "'\\n'")]
        [TestCase('\r', ExpectedResult = "'\\r'")]
        [TestCase('\t', ExpectedResult = "'\\t'")]
        [TestCase('\v', ExpectedResult = "'\\v'")]
        [TestCase('\'', ExpectedResult = "'\\''")]
        [TestCase('\u2022', ExpectedResult = "'\\u2022'")]
        [TestCase("OK", ExpectedResult = "\"OK\"")]
        [TestCase("\"OK\"", ExpectedResult = "\"\\\"OK\\\"\"")]
        public string FormatValue_ReturnsExpectedString(object? value)
        {
            return csharp.FormatLiteral(value);
        }

        [TestCase(NameQualifier.None, typeof(Acme.IProcess<>), ExpectedResult = "IProcess<T>")]
        [TestCase(NameQualifier.None, typeof(Acme.ValueType), ExpectedResult = "ValueType")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), ExpectedResult = "Widget")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.NestedClass), ExpectedResult = "NestedClass")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.IMenuItem<>), ExpectedResult = "IMenuItem<T>")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.Del), ExpectedResult = "Del")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.Direction), ExpectedResult = "Direction")]
        [TestCase(NameQualifier.None, typeof(Acme.MyList<>), ExpectedResult = "MyList<T>")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.MyList<>.Helper<,>), ExpectedResult = "MyList<T>.Helper<U, V>")]
        [TestCase(NameQualifier.None, typeof(Acme.UseList), ExpectedResult = "UseList")]
        [TestCase(NameQualifier.DeclaringType, typeof(Dictionary<,>.KeyCollection.Enumerator), ExpectedResult = "Dictionary<TKey, TValue>.KeyCollection.Enumerator")]
        public string FormatName_ForTypes_ReturnsExpectedString(NameQualifier qualifier, Type type)
        {
            return csharp.FormatName(type.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.Widget), new[] { typeof(string) }, ExpectedResult = "")]
        [TestCase(NameQualifier.Full, typeof(Acme.Widget), new[] { typeof(string) }, ExpectedResult = "Acme.Widget")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.NestedClass), ExpectedResult = "")]
        [TestCase(NameQualifier.Full, typeof(Acme.Widget.NestedClass), ExpectedResult = "Acme.Widget.NestedClass")]
        [TestCase(NameQualifier.None, typeof(Acme.MyList<>), new[] { typeof(IEnumerable<>) }, ExpectedResult = "")]
        [TestCase(NameQualifier.Full, typeof(Acme.MyList<>), new[] { typeof(IEnumerable<>) }, ExpectedResult = "Acme.MyList<T>")]
        public string FormatName_ForConstructors_ReturnsExpectedString(NameQualifier qualifier, Type type, params Type[] paramTypes)
        {
            var constructor = type.GetMetadata<IClassType>().Constructors.First(
                c => c.Parameters.Count == paramTypes.Length
                  && c.Parameters.All(p => p.Type.Name == paramTypes[p.Position].Name));

            return csharp.FormatName(constructor, qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.ValueType), nameof(Acme.ValueType.M1), ExpectedResult = "M1")]
        [TestCase(NameQualifier.None, typeof(Acme.ValueType), nameof(Acme.ValueType.M2), ExpectedResult = "M2")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M0), ExpectedResult = "M0")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M1), ExpectedResult = "M1")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M2), ExpectedResult = "M2")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M3), ExpectedResult = "M3")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M4), ExpectedResult = "M4")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M5), ExpectedResult = "M5")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M6), ExpectedResult = "M6")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.NestedClass), nameof(Acme.Widget.NestedClass.M1), ExpectedResult = "M1")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.NestedClass), nameof(Acme.Widget.NestedClass.M2), ExpectedResult = "M2")]
        [TestCase(NameQualifier.Full, typeof(Acme.MyList<>), nameof(Acme.MyList<int>.Test), ExpectedResult = "Acme.MyList<T>.Test")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.UseList), nameof(Acme.UseList.ProcessAsync), ExpectedResult = "UseList.ProcessAsync")]
        [TestCase(NameQualifier.None, typeof(Acme.UseList), nameof(Acme.UseList.GetValues), ExpectedResult = "GetValues")]
        [TestCase(NameQualifier.None, typeof(Acme.UseList), nameof(Acme.UseList.Intercept), ExpectedResult = "Intercept")]
        [TestCase(NameQualifier.None, typeof(Acme.Extensions), nameof(Acme.Extensions.Hello), ExpectedResult = "Hello")]
        [TestCase(NameQualifier.DeclaringType, typeof(Dictionary<,>.KeyCollection.Enumerator), "MoveNext", ExpectedResult = "Dictionary<TKey, TValue>.KeyCollection.Enumerator.MoveNext")]
        public string FormatName_ForMethods_ReturnsExpectedString(NameQualifier qualifier, Type type, string methodName, Type[]? paramTypes = null)
        {
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, bindingFlags) : type.GetMethod(methodName, bindingFlags, paramTypes);

            return csharp.FormatName(methodInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.Widget), "UnaryPlus", ExpectedResult = "UnaryPlus")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), "Addition", ExpectedResult = "Addition")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), "Explicit", ExpectedResult = "Explicit")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), "Implicit", ExpectedResult = "Implicit")]
        public string FormatName_ForOperators_ReturnsExpectedString(NameQualifier qualifier, Type type, string operatorName, Type[]? paramTypes = null)
        {
            var methodName = $"op_{operatorName}";
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, bindingFlags) : type.GetMethod(methodName, bindingFlags, paramTypes);

            return csharp.FormatName(methodInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.Width), ExpectedResult = "Width")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), "Item", new[] { typeof(int) }, ExpectedResult = "Item[]")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), "Item", new[] { typeof(string), typeof(int) }, ExpectedResult = "Item[]")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.MyList<>), nameof(Acme.MyList<int>.Items), ExpectedResult = "MyList<T>.Items")]
        [TestCase(NameQualifier.DeclaringType, typeof(Dictionary<,>.KeyCollection.Enumerator), "Current", ExpectedResult = "Dictionary<TKey, TValue>.KeyCollection.Enumerator.Current")]
        public string FormatName_ForProperties_ReturnsExpectedString(NameQualifier qualifier, Type type, string propertyName, params Type[] paramTypes)
        {
            var propertyInfo = type.GetProperty(propertyName, bindingFlags, null, null, paramTypes, null);

            return csharp.FormatName(propertyInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.AnEvent), ExpectedResult = "AnEvent")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.Widget), nameof(Acme.Widget.AnEvent), ExpectedResult = "Widget.AnEvent")]
        [TestCase(NameQualifier.Full, typeof(Acme.Widget), nameof(Acme.Widget.AnEvent), ExpectedResult = "Acme.Widget.AnEvent")]
        public string FormatName_ForEvents_ReturnsExpectedString(NameQualifier qualifier, Type type, string eventName)
        {
            var eventInfo = type.GetEvent(eventName, bindingFlags);

            return csharp.FormatName(eventInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.ValueType), nameof(Acme.ValueType.total), ExpectedResult = "total")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.message), ExpectedResult = "message")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.defaultDirection), ExpectedResult = "defaultDirection")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.PI), ExpectedResult = "PI")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.monthlyAverage), ExpectedResult = "monthlyAverage")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.array1), ExpectedResult = "array1")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.array2), ExpectedResult = "array2")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.pCount), ExpectedResult = "pCount")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.ppValues), ExpectedResult = "ppValues")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.NestedClass), nameof(Acme.Widget.NestedClass.value), ExpectedResult = "value")]
        public string FormatName_ForFields_ReturnsExpectedString(NameQualifier qualifier, Type type, string fieldName)
        {
            var fieldInfo = type.GetField(fieldName, bindingFlags);

            return csharp.FormatName(fieldInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.IProcess<>), ExpectedResult = "IProcess<T>")]
        [TestCase(NameQualifier.None, typeof(Acme.ValueType), ExpectedResult = "ValueType")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), ExpectedResult = "Widget")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.NestedClass), ExpectedResult = "NestedClass")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.IMenuItem<>), ExpectedResult = "IMenuItem<T>")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.Del), ExpectedResult = "Del")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.Direction), ExpectedResult = "Direction")]
        [TestCase(NameQualifier.None, typeof(Acme.MyList<>), ExpectedResult = "MyList<T>")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.MyList<>.Helper<,>), ExpectedResult = "MyList<T>.Helper<U, V>")]
        [TestCase(NameQualifier.None, typeof(Acme.UseList), ExpectedResult = "UseList")]
        public string FormatSignature_ForTypes_ReturnsExpectedString(NameQualifier qualifier, Type type)
        {
            return csharp.FormatSignature(type.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.Widget), new[] { typeof(string) }, ExpectedResult = "Widget(string)")]
        [TestCase(NameQualifier.Full, typeof(Acme.Widget), new[] { typeof(string) }, ExpectedResult = "Acme.Widget(string)")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.NestedClass), ExpectedResult = "NestedClass()")]
        [TestCase(NameQualifier.Full, typeof(Acme.Widget.NestedClass), ExpectedResult = "Acme.Widget.NestedClass()")]
        [TestCase(NameQualifier.None, typeof(Acme.MyList<>), new[] { typeof(IEnumerable<>) }, ExpectedResult = "MyList<T>(IEnumerable<T>)")]
        [TestCase(NameQualifier.Full, typeof(Acme.MyList<>), new[] { typeof(IEnumerable<>) }, ExpectedResult = "Acme.MyList<T>(IEnumerable<T>)")]
        public string FormatSignature_ForConstructors_ReturnsExpectedString(NameQualifier qualifier, Type type, params Type[] paramTypes)
        {
            var constructor = type.GetMetadata<IClassType>().Constructors.First(
                c => c.Parameters.Count == paramTypes.Length
                  && c.Parameters.All(p => p.Type.Name == paramTypes[p.Position].Name));

            return csharp.FormatSignature(constructor, qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.ValueType), nameof(Acme.ValueType.M1), ExpectedResult = "M1(int)")]
        [TestCase(NameQualifier.None, typeof(Acme.ValueType), nameof(Acme.ValueType.M2), ExpectedResult = "M2(int?)")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M0), ExpectedResult = "M0()")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M1), ExpectedResult = "M1(char, out float, ref ValueType, in int)")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M2), ExpectedResult = "M2(short[], int[,], long[][])")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M3), ExpectedResult = "M3(long[][], Widget[][,,])")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M4), ExpectedResult = "M4(char*, Widget.Direction**)")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M5), ExpectedResult = "M5(void*, double?*[][,][,,])")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.M6), ExpectedResult = "M6(int, params object[])")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.NestedClass), nameof(Acme.Widget.NestedClass.M1), ExpectedResult = "M1()")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.NestedClass), nameof(Acme.Widget.NestedClass.M2), ExpectedResult = "M2(int)")]
        [TestCase(NameQualifier.Full, typeof(Acme.MyList<>), nameof(Acme.MyList<int>.Test), ExpectedResult = "Acme.MyList<T>.Test(T)")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.UseList), nameof(Acme.UseList.ProcessAsync), ExpectedResult = "UseList.ProcessAsync(MyList<int>)")]
        [TestCase(NameQualifier.None, typeof(Acme.UseList), nameof(Acme.UseList.GetValues), ExpectedResult = "GetValues<T>(T)")]
        [TestCase(NameQualifier.None, typeof(Acme.UseList), nameof(Acme.UseList.Intercept), ExpectedResult = "Intercept<T>(in MyList<T>.Helper<char, string>[])")]
        [TestCase(NameQualifier.None, typeof(Acme.Extensions), nameof(Acme.Extensions.Hello), ExpectedResult = "Hello(this Widget)")]
        public string FormatSignature_ForMethods_ReturnsExpectedString(NameQualifier qualifier, Type type, string methodName, Type[]? paramTypes = null)
        {
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, bindingFlags) : type.GetMethod(methodName, bindingFlags, paramTypes);

            return csharp.FormatSignature(methodInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.Widget), "UnaryPlus", ExpectedResult = "UnaryPlus(Widget)")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), "Addition", ExpectedResult = "Addition(Widget, Widget)")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), "Explicit", ExpectedResult = "Explicit(Widget)")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), "Implicit", ExpectedResult = "Implicit(Widget)")]
        public string FormatSignature_ForOperators_ReturnsExpectedString(NameQualifier qualifier, Type type, string operatorName, Type[]? paramTypes = null)
        {
            var methodName = $"op_{operatorName}";
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, bindingFlags) : type.GetMethod(methodName, bindingFlags, paramTypes);

            return csharp.FormatSignature(methodInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.Width), ExpectedResult = "Width")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), "Item", new[] { typeof(int) }, ExpectedResult = "Item[int]")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), "Item", new[] { typeof(string), typeof(int) }, ExpectedResult = "Item[string, int]")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.MyList<>), nameof(Acme.MyList<int>.Items), ExpectedResult = "MyList<T>.Items")]
        public string FormatSignature_ForProperties_ReturnsExpectedString(NameQualifier qualifier, Type type, string propertyName, params Type[] paramTypes)
        {
            var propertyInfo = type.GetProperty(propertyName, bindingFlags, null, null, paramTypes, null);

            return csharp.FormatSignature(propertyInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.AnEvent), ExpectedResult = "AnEvent")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.Widget), nameof(Acme.Widget.AnEvent), ExpectedResult = "Widget.AnEvent")]
        [TestCase(NameQualifier.Full, typeof(Acme.Widget), nameof(Acme.Widget.AnEvent), ExpectedResult = "Acme.Widget.AnEvent")]
        public string FormatSignature_ForEvents_ReturnsExpectedString(NameQualifier qualifier, Type type, string eventName)
        {
            var eventInfo = type.GetEvent(eventName, bindingFlags);

            return csharp.FormatSignature(eventInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.ValueType), nameof(Acme.ValueType.total), ExpectedResult = "total")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.message), ExpectedResult = "message")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.defaultDirection), ExpectedResult = "defaultDirection")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.PI), ExpectedResult = "PI")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.monthlyAverage), ExpectedResult = "monthlyAverage")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.array1), ExpectedResult = "array1")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.array2), ExpectedResult = "array2")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.pCount), ExpectedResult = "pCount")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget), nameof(Acme.Widget.ppValues), ExpectedResult = "ppValues")]
        [TestCase(NameQualifier.None, typeof(Acme.Widget.NestedClass), nameof(Acme.Widget.NestedClass.value), ExpectedResult = "value")]
        public string FormatSignature_ForFields_ReturnsExpectedString(NameQualifier qualifier, Type type, string fieldName)
        {
            var fieldInfo = type.GetField(fieldName, bindingFlags);

            return csharp.FormatSignature(fieldInfo!.GetMetadata(), qualifier);
        }

        [TestCase(typeof(Acme.IProcess<>), ExpectedResult = "public interface IProcess<out T>")]
        [TestCase(typeof(Acme.ValueType), ExpectedResult = "public struct ValueType : ICloneable")]
        [TestCase(typeof(Acme.Widget), ExpectedResult = "public class Widget")]
        [TestCase(typeof(Acme.Widget.NestedClass), ExpectedResult = "public abstract class Widget.NestedClass")]
        [TestCase(typeof(Acme.Widget.NestedDerivedClass), ExpectedResult = "public class Widget.NestedDerivedClass : Widget.NestedClass")]
        [TestCase(typeof(Acme.Widget.IMenuItem<>), ExpectedResult = "public interface Widget.IMenuItem<in T>\n\twhere T : Widget, new()")]
        [TestCase(typeof(Acme.Widget.Del), ExpectedResult = "protected delegate void Widget.Del(int i)")]
        [TestCase(typeof(Acme.Widget.Direction), ExpectedResult = "public enum Widget.Direction")]
        [TestCase(typeof(Acme.MyList<>), ExpectedResult = "public class MyList<T>\n\twhere T : struct")]
        [TestCase(typeof(Acme.MyList<>.Helper<,>), ExpectedResult = "public class MyList<T>.Helper<U, V>\n\twhere T : struct")]
        [TestCase(typeof(Acme.UseList), ExpectedResult = "public sealed class UseList : IProcess<string>")]
        [TestCase(typeof(Predicate<>), ExpectedResult = "public delegate bool Predicate<in T>(T obj)")]
        public string FormatDefinition_ForTypes_ReturnsExpectedString(Type type)
        {
            return csharp.FormatDefinition(type.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.Widget), new[] { typeof(string) }, ExpectedResult = "[SetsRequiredMembers]\nprotected Widget(string s)")]
        [TestCase(typeof(Acme.Widget.NestedClass), ExpectedResult = "protected NestedClass()")]
        [TestCase(typeof(Acme.MyList<>), new[] { typeof(IEnumerable<>) }, ExpectedResult = "public MyList<T>(IEnumerable<T> items)")]
        public string FormatDefinition_ForConstructors_ReturnsExpectedString(Type type, params Type[] paramTypes)
        {
            var constructor = type.GetMetadata<IClassType>().Constructors.First(
                c => c.Parameters.Count == paramTypes.Length
                  && c.Parameters.All(p => p.Type.Name == paramTypes[p.Position].Name));

            return csharp.FormatDefinition(constructor).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.Extensions), nameof(Acme.Extensions.Hello), ExpectedResult = "public static void Hello(this Widget widget)")]
        [TestCase(typeof(Acme.ValueType), nameof(Acme.ValueType.M1), ExpectedResult = "public void M1(int i)")]
        [TestCase(typeof(Acme.ValueType), nameof(Acme.ValueType.M2), ExpectedResult = "public readonly string M2(int? i = null)")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M0), ExpectedResult = "public static void M0()")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M1), ExpectedResult = "public void M1(\n\tchar c,\n\tout float f,\n\tref ValueType v,\n\tin int i)")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M2), ExpectedResult = "public void M2(short[] x1, int[,] x2, long[][] x3)")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M3), ExpectedResult = "public void M3(long[][] x3, Widget[][,,] x4)")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M4), ExpectedResult = "public unsafe void M4(char* pc, Widget.Direction** pd = null)")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M5), ExpectedResult = "public unsafe void M5(void* pv, double?*[][,][,,] pd)")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.M6), ExpectedResult = "public void M6(int i, params object[] args)")]
        [TestCase(typeof(Acme.Widget.NestedClass), nameof(Acme.Widget.NestedClass.M1), ExpectedResult = "public void M1()")]
        [TestCase(typeof(Acme.Widget.NestedClass), nameof(Acme.Widget.NestedClass.M2), ExpectedResult = "public abstract void M2(int i = 123)")]
        [TestCase(typeof(Acme.Widget.NestedDerivedClass), nameof(Acme.Widget.NestedDerivedClass.M1), ExpectedResult = "public void M1()")]
        [TestCase(typeof(Acme.Widget.NestedDerivedClass), nameof(Acme.Widget.NestedDerivedClass.M2), ExpectedResult = "public sealed override void M2(int i = 123)")]
        [TestCase(typeof(Acme.MyList<>), nameof(Acme.MyList<int>.Test), ExpectedResult = "protected void Test([Custom] T t = default)")]
        [TestCase(typeof(Acme.UseList), nameof(Acme.UseList.ProcessAsync), ExpectedResult = "public Task<bool> ProcessAsync(MyList<int> list)")]
        [TestCase(typeof(Acme.UseList), nameof(Acme.UseList.GetValues), ExpectedResult = "public MyList<T> GetValues<T>(T value)\n\twhere T : struct")]
        [TestCase(typeof(Acme.UseList), nameof(Acme.UseList.Intercept), ExpectedResult = "[Obsolete(\"For sake of testing\", DiagnosticId = \"XYZ\")]\npublic T Intercept<T>(in MyList<T>.Helper<char, string>[] helper)\n\twhere T : struct")]
        [TestCase(typeof(Acme.UseList), "Acme.IProcess<System.String>.GetStatus", ExpectedResult = "string IProcess<string>.GetStatus(bool state)")]
        public string FormatDefinition_ForMethods_ReturnsExpectedString(Type type, string methodName, Type[]? paramTypes = null)
        {
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, bindingFlags) : type.GetMethod(methodName, bindingFlags, paramTypes);

            return csharp.FormatDefinition(methodInfo!.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.Widget), "UnaryPlus", ExpectedResult = "public static Widget operator +(Widget x)")]
        [TestCase(typeof(Acme.Widget), "Addition", ExpectedResult = "public static Widget operator +(Widget x1, Widget x2)")]
        [TestCase(typeof(Acme.Widget), "Explicit", ExpectedResult = "public static explicit operator int(Widget x)")]
        [TestCase(typeof(Acme.Widget), "Implicit", ExpectedResult = "[return: NotNullIfNotNull(\"x\")]\npublic static implicit operator long?(Widget x)")]
        public string FormatDefinition_ForOperators_ReturnsExpectedString(Type type, string operatorName, Type[]? paramTypes = null)
        {
            var methodName = $"op_{operatorName}";
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, bindingFlags) : type.GetMethod(methodName, bindingFlags, paramTypes);

            return csharp.FormatDefinition(methodInfo!.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.Width), ExpectedResult = "public required int Width { get; init; }")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.Height), ExpectedResult = "public int Height { get; set; }")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.Depth), ExpectedResult = "public int Depth { get; }")]
        [TestCase(typeof(Acme.Widget), "Item", new[] { typeof(int) }, ExpectedResult = "public int this[int i] { get; }")]
        [TestCase(typeof(Acme.Widget), "Item", new[] { typeof(string), typeof(int) }, ExpectedResult = "public int this[string s, int i] { get; }")]
        [TestCase(typeof(Acme.MyList<>), nameof(Acme.MyList<int>.Items), ExpectedResult = "public IEnumerable<T> Items { get; protected set; }")]
        [TestCase(typeof(Acme.UseList), "Acme.IProcess<System.String>.IsCompleted", ExpectedResult = "bool IProcess<string>.IsCompleted { get; }")]
        public string FormatDefinition_ForProperties_ReturnsExpectedString(Type type, string propertyName, params Type[] paramTypes)
        {
            var propertyInfo = type.GetProperty(propertyName, bindingFlags, null, null, paramTypes, null);

            return csharp.FormatDefinition(propertyInfo!.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.AnEvent), ExpectedResult = "protected event Widget.Del AnEvent")]
        [TestCase(typeof(Acme.UseList), "Acme.IProcess<System.String>.Completed", ExpectedResult = "event EventHandler IProcess<string>.Completed")]
        public string FormatDefinition_ForEvents_ReturnsExpectedString(Type type, string eventName)
        {
            var eventInfo = type.GetEvent(eventName, bindingFlags);

            return csharp.FormatDefinition(eventInfo!.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.ValueType), nameof(Acme.ValueType.total), ExpectedResult = "public volatile int total")]
        [TestCase(typeof(Acme.ValueType), nameof(Acme.ValueType.buffer), ExpectedResult = "public unsafe fixed int buffer[10]")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.message), ExpectedResult = "public string message")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.defaultDirection), ExpectedResult = "internal static Widget.Direction defaultDirection")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.PI), ExpectedResult = "public const double PI = 3.14159")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.monthlyAverage), ExpectedResult = "protected readonly double monthlyAverage")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.array1), ExpectedResult = "public long[] array1")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.array2), ExpectedResult = "public Widget[,] array2")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.pCount), ExpectedResult = "public unsafe int* pCount")]
        [TestCase(typeof(Acme.Widget), nameof(Acme.Widget.ppValues), ExpectedResult = "public unsafe float** ppValues")]
        [TestCase(typeof(Acme.Widget.NestedClass), nameof(Acme.Widget.NestedClass.value), ExpectedResult = "public int value")]
        public string FormatDefinition_ForFields_ReturnsExpectedString(Type type, string fieldName)
        {
            var fieldInfo = type.GetField(fieldName, bindingFlags);

            return csharp.FormatDefinition(fieldInfo!.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase("N:Acme", ExpectedResult = "Acme")]
        [TestCase("T:Acme.Widget", ExpectedResult = "Widget")]
        [TestCase("T:Acme.MyList`1+Helper`2", ExpectedResult = "MyList<T>.Helper<U, V>")]
        [TestCase("F:Acme.Widget.PI", ExpectedResult = "Widget.PI")]
        [TestCase("P:Acme.Widget.Item(System.String,System.Int32)", ExpectedResult = "Widget.Item[string, int]")]
        [TestCase("M:Acme.Widget.M1(System.Char,System.Single@,Acme.ValueType@,System.Int32@)", ExpectedResult = "Widget.M1(char, out float, ref ValueType, in int)")]
        [TestCase("M:Acme.Widget.op_Addition(Acme.Widget,Acme.Widget)", ExpectedResult = "Widget.Addition(Widget, Widget)")]
        [TestCase("E:Acme.Widget.AnEvent", ExpectedResult = "Widget.AnEvent")]
        [TestCase("E:Acme.UseList.Acme#IProcess{System#String}#Completed", ExpectedResult = "IProcess<string>.Completed")]
        [TestCase("T:Unresolvable.Type", ExpectedResult = "T:Unresolvable.Type")]
        public string FormatCodeReference_WithoutQualifierSelector_ReturnsExpectedString(string cref)
        {
            return csharp.FormatCodeReference(cref);
        }

        [TestCase("N:Acme", ExpectedResult = "Acme")]
        [TestCase("T:Acme.Widget", ExpectedResult = "Widget")]
        [TestCase("T:Acme.MyList`1+Helper`2", ExpectedResult = "MyList<T>.Helper<U, V>")]
        [TestCase("F:Acme.Widget.PI", ExpectedResult = "PI")]
        [TestCase("P:Acme.Widget.Item(System.String,System.Int32)", ExpectedResult = "Item[string, int]")]
        [TestCase("M:Acme.Widget.M1(System.Char,System.Single@,Acme.ValueType@,System.Int32@)", ExpectedResult = "M1(char, out float, ref ValueType, in int)")]
        [TestCase("M:Acme.Widget.op_Addition(Acme.Widget,Acme.Widget)", ExpectedResult = "Addition(Widget, Widget)")]
        [TestCase("E:Acme.Widget.AnEvent", ExpectedResult = "AnEvent")]
        [TestCase("E:Acme.UseList.Acme#IProcess{System#String}#Completed", ExpectedResult = "IProcess<string>.Completed")]
        [TestCase("T:Unresolvable.Type", ExpectedResult = "T:Unresolvable.Type")]
        public string FormatCodeReference_WithQualifierSelector_ReturnsExpectedString(string cref)
        {
            return csharp.FormatCodeReference(cref, member => member is IType ? NameQualifier.DeclaringType : NameQualifier.None);
        }
    }
}
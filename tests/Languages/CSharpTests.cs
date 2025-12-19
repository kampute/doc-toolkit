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
        private readonly CSharp cs = new();

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
            return cs.TryGetUrl(keyword, out var url) ? url.ToString() : null;
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
            return cs.FormatLiteral(value);
        }

        [TestCase(NameQualifier.None, typeof(Action), ExpectedResult = "Action")]
        [TestCase(NameQualifier.None, typeof(Action<>), ExpectedResult = "Action<T>")]
        [TestCase(NameQualifier.None, typeof(Action<,>), ExpectedResult = "Action<T1, T2>")]
        [TestCase(NameQualifier.None, typeof(DayOfWeek), ExpectedResult = "DayOfWeek")]
        [TestCase(NameQualifier.None, typeof(DateTime), ExpectedResult = "DateTime")]
        [TestCase(NameQualifier.None, typeof(ArraySegment<>), ExpectedResult = "ArraySegment<T>")]
        [TestCase(NameQualifier.None, typeof(List<>), ExpectedResult = "List<T>")]
        [TestCase(NameQualifier.None, typeof(IReadOnlyList<>), ExpectedResult = "IReadOnlyList<T>")]
        [TestCase(NameQualifier.None, typeof(Dictionary<,>.KeyCollection.Enumerator), ExpectedResult = "Enumerator")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleGenericClass<>), ExpectedResult = "SampleGenericClass<T>")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), ExpectedResult = "SampleGenericClass<T>.InnerGenericClass<U, V>")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), ExpectedResult = "SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass")]
        public string FormatName_ForTypes_ReturnsExpectedString(NameQualifier qualifier, Type type)
        {
            return cs.FormatName(type.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(DateTime), typeof(long), ExpectedResult = "")]
        [TestCase(NameQualifier.Full, typeof(DateTime), typeof(long), ExpectedResult = "System.DateTime")]
        [TestCase(NameQualifier.None, typeof(List<>), typeof(IEnumerable<>), ExpectedResult = "")]
        [TestCase(NameQualifier.Full, typeof(List<>), typeof(IEnumerable<>), ExpectedResult = "System.Collections.Generic.List<T>")]
        [TestCase(NameQualifier.None, typeof(Dictionary<,>.KeyCollection), typeof(Dictionary<,>), ExpectedResult = "")]
        [TestCase(NameQualifier.Full, typeof(Dictionary<,>.KeyCollection), typeof(Dictionary<,>), ExpectedResult = "System.Collections.Generic.Dictionary<TKey, TValue>.KeyCollection")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), ExpectedResult = "")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), ExpectedResult = "Acme.SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass")]
        public string FormatName_ForConstructors_ReturnsExpectedString(NameQualifier qualifier, Type type, params Type[] paramTypes)
        {
            var constructor = type.GetMetadata<ICompositeType>().Constructors.First(
                c => c.Parameters.Count == paramTypes.Length
                  && c.Parameters.All(p => p.Type.Name == paramTypes[p.Position].Name));

            return cs.FormatName(constructor, qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = "RegularMethod")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.StaticMethod), ExpectedResult = "StaticMethod")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethod", ExpectedResult = "ISampleInterface.InterfaceMethod")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.Method), ExpectedResult = "Acme.SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass.Method")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.GenericMethod), ExpectedResult = "Acme.SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass.GenericMethod")]
        public string FormatName_ForMethods_ReturnsExpectedString(NameQualifier qualifier, Type type, string methodName, Type[]? paramTypes = null)
        {
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, Acme.Bindings.AllDeclared) : type.GetMethod(methodName, Acme.Bindings.AllDeclared, paramTypes);

            return cs.FormatName(methodInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.ClassicExtensionMethodForClass), ExpectedResult = "ClassicExtensionMethodForClass")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethodForClass), ExpectedResult = "extension<W>(SampleGenericClass<W>).InstanceExtensionMethodForClass")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethodForClass), ExpectedResult = "SampleExtensions.extension<W>(SampleGenericClass<W>).StaticExtensionMethodForClass")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.GenericExtensionMethodForClass), ExpectedResult = "Acme.SampleExtensions.extension<W>(SampleGenericClass<W>).GenericExtensionMethodForClass")]
        public string FormatName_ForExtensionMethods_ReturnsExpectedString(NameQualifier qualifier, Type type, string methodName, Type[]? paramTypes = null)
        {
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, Acme.Bindings.AllDeclared) : type.GetMethod(methodName, Acme.Bindings.AllDeclared, paramTypes);

            return cs.FormatName(methodInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_UnaryPlus", ExpectedResult = "UnaryPlus")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_UnaryNegation", ExpectedResult = "UnaryNegation")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_Addition", ExpectedResult = "Addition")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_Subtraction", ExpectedResult = "Subtraction")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_AdditionAssignment", ExpectedResult = "AdditionAssignment")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_SubtractionAssignment", ExpectedResult = "SubtractionAssignment")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_IncrementAssignment", ExpectedResult = "IncrementAssignment")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_DecrementAssignment", ExpectedResult = "DecrementAssignment")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_Explicit", ExpectedResult = "Explicit")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_Implicit", ExpectedResult = "Implicit")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "Acme.ISampleInterface.op_False", ExpectedResult = "ISampleInterface.False")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct), "Acme.ISampleInterface.op_False", ExpectedResult = "ISampleInterface.False")]
        public string FormatName_ForOperators_ReturnsExpectedString(NameQualifier qualifier, Type type, string methodName, Type[]? paramTypes = null)
        {
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, Acme.Bindings.AllDeclared) : type.GetMethod(methodName, Acme.Bindings.AllDeclared, paramTypes);

            return cs.FormatName(methodInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = "RegularProperty")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty), ExpectedResult = "StaticProperty")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleProperties), "Item", typeof(int), ExpectedResult = "Item[]")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty", ExpectedResult = "ISampleInterface.InterfaceProperty")]
        [TestCase(NameQualifier.DeclaringType, typeof(Dictionary<,>.KeyCollection.Enumerator), "Current", ExpectedResult = "Dictionary<TKey, TValue>.KeyCollection.Enumerator.Current")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.Property), ExpectedResult = "Acme.SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass.Property")]
        public string FormatName_ForProperties_ReturnsExpectedString(NameQualifier qualifier, Type type, string propertyName, params Type[] paramTypes)
        {
            var propertyInfo = type.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, paramTypes, null);

            return cs.FormatName(propertyInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_InstanceExtensionProperty), ExpectedResult = "extension(ISampleInterface).InstanceExtensionProperty")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_StaticExtensionProperty), ExpectedResult = "SampleExtensions.extension(ISampleInterface).StaticExtensionProperty")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_FullExtensionProperty), ExpectedResult = "Acme.SampleExtensions.extension(ISampleInterface).FullExtensionProperty")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_InstanceExtensionPropertyForClass), ExpectedResult = "extension<W>(SampleGenericClass<W>).InstanceExtensionPropertyForClass")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_StaticExtensionPropertyForClass), ExpectedResult = "SampleExtensions.extension<W>(SampleGenericClass<W>).StaticExtensionPropertyForClass")]
        public string FormatName_ForExtensionProperties_ReturnsExpectedString(NameQualifier qualifier, Type type, string accessorName)
        {
            MemberInfo accessorInfo = type.GetMethod(accessorName, Acme.Bindings.AllDeclared)!;

            return cs.FormatName(accessorInfo.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = "RegularEvent")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = "SampleEvents.RegularEvent")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent), ExpectedResult = "StaticEvent")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent), ExpectedResult = "SampleEvents.StaticEvent")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = "ISampleInterface.InterfaceEvent")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = "SampleEvents.Acme.ISampleInterface.InterfaceEvent")]
        public string FormatName_ForEvents_ReturnsExpectedString(NameQualifier qualifier, Type type, string eventName)
        {
            var eventInfo = type.GetEvent(eventName, Acme.Bindings.AllDeclared);

            return cs.FormatName(eventInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(DateTime), nameof(DateTime.MinValue), ExpectedResult = "MinValue")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField), ExpectedResult = "StaticReadonlyField")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField), ExpectedResult = "VolatileField")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = "ConstField")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField), ExpectedResult = "VolatileField")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.FixedBuffer), ExpectedResult = "FixedBuffer")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.ComplexField), ExpectedResult = "ComplexField")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.FixedBuffer), ExpectedResult = "FixedBuffer")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.ComplexField), ExpectedResult = "ComplexField")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField), ExpectedResult = "StaticReadonlyField")]
        public string FormatName_ForFields_ReturnsExpectedString(NameQualifier qualifier, Type type, string fieldName)
        {
            var fieldInfo = type.GetField(fieldName, Acme.Bindings.AllDeclared);

            return cs.FormatName(fieldInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Action), ExpectedResult = "Action")]
        [TestCase(NameQualifier.None, typeof(Action<>), ExpectedResult = "Action<T>")]
        [TestCase(NameQualifier.None, typeof(Action<,>), ExpectedResult = "Action<T1, T2>")]
        [TestCase(NameQualifier.None, typeof(DayOfWeek), ExpectedResult = "DayOfWeek")]
        [TestCase(NameQualifier.None, typeof(DateTime), ExpectedResult = "DateTime")]
        [TestCase(NameQualifier.None, typeof(ArraySegment<>), ExpectedResult = "ArraySegment<T>")]
        [TestCase(NameQualifier.None, typeof(List<>), ExpectedResult = "List<T>")]
        [TestCase(NameQualifier.None, typeof(IReadOnlyList<>), ExpectedResult = "IReadOnlyList<T>")]
        [TestCase(NameQualifier.DeclaringType, typeof(Dictionary<,>.KeyCollection.Enumerator), ExpectedResult = "Dictionary<TKey, TValue>.KeyCollection.Enumerator")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleGenericClass<>), ExpectedResult = "SampleGenericClass<T>")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), ExpectedResult = "SampleGenericClass<T>.InnerGenericClass<U, V>")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), ExpectedResult = "SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass")]
        public string FormatSignature_ForTypes_ReturnsExpectedString(NameQualifier qualifier, Type type)
        {
            return cs.FormatSignature(type.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleConstructors), typeof(string), typeof(double), ExpectedResult = "SampleConstructors(string, double)")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleConstructors), typeof(string), typeof(double), ExpectedResult = "Acme.SampleConstructors(string, double)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleConstructors), typeof(object), ExpectedResult = "SampleConstructors(object)")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleConstructors), typeof(object), ExpectedResult = "Acme.SampleConstructors(object)")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleGenericClass<>), ExpectedResult = "Acme.SampleGenericClass<T>()")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), ExpectedResult = "Acme.SampleGenericClass<T>.InnerGenericClass<U, V>()")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), ExpectedResult = "Acme.SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass()")]
        public string FormatSignature_ForConstructors_ReturnsExpectedString(NameQualifier qualifier, Type type, params Type[] paramTypes)
        {
            var constructor = type.GetMetadata<IClassType>().Constructors.First(
                c => c.Parameters.Count == paramTypes.Length
                  && c.Parameters.All(p => p.Type.Name == paramTypes[p.Position].Name));

            return cs.FormatSignature(constructor, qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = "RegularMethod()")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.StaticMethod), ExpectedResult = "StaticMethod()")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RefParamsMethod), ExpectedResult = "RefParamsMethod(in int, ref string, out double)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.ArrayParamsMethod), ExpectedResult = "ArrayParamsMethod(params IEnumerable<string>)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), ExpectedResult = "MixedParamsMethod(string, in DayOfWeek, params double?*[][,][,,,])")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.UnsafeMethod), ExpectedResult = "UnsafeMethod(int**)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.OptionalParamsMethod), ExpectedResult = "OptionalParamsMethod(int, string)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithGenericParameter), ExpectedResult = "GenericMethodWithGenericParameter<S>(List<S>)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethod", ExpectedResult = "ISampleInterface.InterfaceMethod()")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.Method), ExpectedResult = "Method(T, U, V)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.GenericMethod), ExpectedResult = "GenericMethod<W>(T, U, V, W)")]
        public string FormatSignature_ForMethods_ReturnsExpectedString(NameQualifier qualifier, Type type, string methodName, Type[]? paramTypes = null)
        {
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, Acme.Bindings.AllDeclared) : type.GetMethod(methodName, Acme.Bindings.AllDeclared, paramTypes);

            return cs.FormatSignature(methodInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.ClassicExtensionMethodForClass), ExpectedResult = "ClassicExtensionMethodForClass<W>(this SampleGenericClass<W>)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethodForClass), ExpectedResult = "extension<W>(SampleGenericClass<W>).InstanceExtensionMethodForClass()")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethodForClass), ExpectedResult = "SampleExtensions.extension<W>(SampleGenericClass<W>).StaticExtensionMethodForClass()")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.GenericExtensionMethodForClass), ExpectedResult = "Acme.SampleExtensions.extension<W>(SampleGenericClass<W>).GenericExtensionMethodForClass<U>(U)")]
        public string FormatSignature_ForExtensionMethods_ReturnsExpectedString(NameQualifier qualifier, Type type, string methodName, Type[]? paramTypes = null)
        {
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, Acme.Bindings.AllDeclared) : type.GetMethod(methodName, Acme.Bindings.AllDeclared, paramTypes);

            return cs.FormatSignature(methodInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_UnaryPlus", ExpectedResult = "UnaryPlus(SampleOperators)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_UnaryNegation", ExpectedResult = "UnaryNegation(SampleOperators)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_Addition", ExpectedResult = "Addition(SampleOperators, SampleOperators)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_Subtraction", ExpectedResult = "Subtraction(SampleOperators, SampleOperators)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_AdditionAssignment", ExpectedResult = "AdditionAssignment(SampleOperators)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_SubtractionAssignment", ExpectedResult = "SubtractionAssignment(SampleOperators)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_IncrementAssignment", ExpectedResult = "IncrementAssignment()")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_DecrementAssignment", ExpectedResult = "DecrementAssignment()")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_Explicit", ExpectedResult = "Explicit(SampleOperators)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "op_Implicit", ExpectedResult = "Implicit(SampleOperators)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleOperators), "Acme.ISampleInterface.op_False", ExpectedResult = "ISampleInterface.False(ISampleInterface)")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct), "Acme.ISampleInterface.op_False", ExpectedResult = "ISampleInterface.False(ISampleInterface)")]
        public string FormatSignature_ForOperators_ReturnsExpectedString(NameQualifier qualifier, Type type, string methodName, Type[]? paramTypes = null)
        {
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, Acme.Bindings.AllDeclared) : type.GetMethod(methodName, Acme.Bindings.AllDeclared, paramTypes);

            return cs.FormatSignature(methodInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = "RegularProperty")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty), ExpectedResult = "StaticProperty")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleProperties), "Item", typeof(int), ExpectedResult = "Item[int]")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleProperties), "Item", typeof(string), typeof(int), ExpectedResult = "Item[string, int]")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty", ExpectedResult = "ISampleInterface.InterfaceProperty")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.Property), ExpectedResult = "Acme.SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass.Property")]
        public string FormatSignature_ForProperties_ReturnsExpectedString(NameQualifier qualifier, Type type, string propertyName, params Type[] paramTypes)
        {
            var propertyInfo = type.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, paramTypes, null);

            return cs.FormatSignature(propertyInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_InstanceExtensionProperty), ExpectedResult = "extension(ISampleInterface).InstanceExtensionProperty")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_StaticExtensionProperty), ExpectedResult = "SampleExtensions.extension(ISampleInterface).StaticExtensionProperty")]
        [TestCase(NameQualifier.Full, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_FullExtensionProperty), ExpectedResult = "Acme.SampleExtensions.extension(ISampleInterface).FullExtensionProperty")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_InstanceExtensionPropertyForClass), ExpectedResult = "extension<W>(SampleGenericClass<W>).InstanceExtensionPropertyForClass")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_StaticExtensionPropertyForClass), ExpectedResult = "SampleExtensions.extension<W>(SampleGenericClass<W>).StaticExtensionPropertyForClass")]
        public string FormatSignature_ForExtensionProperties_ReturnsExpectedString(NameQualifier qualifier, Type type, string accessorName)
        {
            MemberInfo accessorInfo = type.GetMethod(accessorName, Acme.Bindings.AllDeclared)!;

            return cs.FormatSignature(accessorInfo.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = "RegularEvent")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = "SampleEvents.RegularEvent")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent), ExpectedResult = "StaticEvent")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent), ExpectedResult = "SampleEvents.StaticEvent")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = "ISampleInterface.InterfaceEvent")]
        [TestCase(NameQualifier.DeclaringType, typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = "SampleEvents.Acme.ISampleInterface.InterfaceEvent")]
        public string FormatSignature_ForEvents_ReturnsExpectedString(NameQualifier qualifier, Type type, string eventName)
        {
            var eventInfo = type.GetEvent(eventName, Acme.Bindings.AllDeclared);

            return cs.FormatSignature(eventInfo!.GetMetadata(), qualifier);
        }

        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField), ExpectedResult = "StaticReadonlyField")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = "ConstField")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField), ExpectedResult = "VolatileField")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = "ConstField")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField), ExpectedResult = "VolatileField")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.FixedBuffer), ExpectedResult = "FixedBuffer")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.ComplexField), ExpectedResult = "ComplexField")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.FixedBuffer), ExpectedResult = "FixedBuffer")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.ComplexField), ExpectedResult = "ComplexField")]
        [TestCase(NameQualifier.None, typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField), ExpectedResult = "StaticReadonlyField")]
        public string FormatSignature_ForFields_ReturnsExpectedString(NameQualifier qualifier, Type type, string fieldName)
        {
            var fieldInfo = type.GetField(fieldName, Acme.Bindings.AllDeclared);

            return cs.FormatSignature(fieldInfo!.GetMetadata(), qualifier);
        }

        [TestCase(typeof(Acme.SampleFields), ExpectedResult = "public struct SampleFields")]
        [TestCase(typeof(Acme.SampleMethods), ExpectedResult = "public abstract class SampleMethods : ISampleInterface")]
        [TestCase(typeof(Acme.SampleConstructors), ExpectedResult = "public class SampleConstructors")]
        [TestCase(typeof(Acme.SampleAttribute), ExpectedResult = "[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]\npublic class SampleAttribute : Attribute")]
        [TestCase(typeof(Acme.SampleGenericClass<>), ExpectedResult = "public class SampleGenericClass<T>\n\twhere T : class")]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>), ExpectedResult = "public class SampleGenericClass<T>.InnerGenericClass<U, V>\n\twhere T : class\n\twhere U : struct")]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), ExpectedResult = "public abstract class SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass : ISampleInterface, IEnumerable<V>\n\twhere T : class\n\twhere U : struct")]
        [TestCase(typeof(Acme.SampleDerivedGenericClass<,,>), ExpectedResult = "public abstract class SampleDerivedGenericClass<T, U, V> : SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass\n\twhere T : class, new()\n\twhere U : struct")]
        [TestCase(typeof(Acme.SampleDerivedConstructedGenericClass), ExpectedResult = "public class SampleDerivedConstructedGenericClass : SampleDerivedGenericClass<object, int, string>")]
        [TestCase(typeof(Acme.SampleDirectDerivedConstructedGenericClass), ExpectedResult = "public class SampleDirectDerivedConstructedGenericClass : SampleGenericClass<object>.InnerGenericClass<int, string>.DeepInnerGenericClass")]
        [TestCase(typeof(Acme.SampleGenericStruct<>), ExpectedResult = "public struct SampleGenericStruct<T>\n\twhere T : class, IDisposable")]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>), ExpectedResult = "public readonly struct SampleGenericStruct<T>.InnerGenericStruct<U, V>\n\twhere T : class, IDisposable\n\twhere U : struct, allows ref struct\n\twhere V : allows ref struct")]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct), ExpectedResult = "public struct SampleGenericStruct<T>.InnerGenericStruct<U, V>.DeepInnerGenericStruct : ISampleInterface, IEnumerable<V>\n\twhere T : class, IDisposable\n\twhere U : struct, allows ref struct\n\twhere V : allows ref struct")]
        [TestCase(typeof(Acme.ISampleInterface), ExpectedResult = "public interface ISampleInterface")]
        [TestCase(typeof(Acme.ISampleGenericInterface<>), ExpectedResult = "public interface ISampleGenericInterface<T>\n\twhere T : class, new()")]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>), ExpectedResult = "interface ISampleGenericInterface<T>.IInnerGenericInterface<in U, out V>\n\twhere T : class, new()")]
        [TestCase(typeof(Acme.ISampleGenericInterface<>.IInnerGenericInterface<,>.IDeepInnerGenericInterface), ExpectedResult = "interface ISampleGenericInterface<T>.IInnerGenericInterface<in U, out V>.IDeepInnerGenericInterface : ISampleInterface, IEnumerable<V>\n\twhere T : class, new()")]
        [TestCase(typeof(Acme.ISampleExtendedGenericInterface<,,>), ExpectedResult = "public interface ISampleExtendedGenericInterface<T, in U, out V> : ISampleGenericInterface<T>.IInnerGenericInterface<U, V>.IDeepInnerGenericInterface\n\twhere T : class, new()")]
        [TestCase(typeof(Acme.ISampleExtendedConstructedGenericInterface), ExpectedResult = "public interface ISampleExtendedConstructedGenericInterface : ISampleExtendedGenericInterface<object, int, string>")]
        [TestCase(typeof(Acme.ISampleDirectExtendedConstructedGenericInterface), ExpectedResult = "public interface ISampleDirectExtendedConstructedGenericInterface : ISampleGenericInterface<object>.IInnerGenericInterface<int, string>.IDeepInnerGenericInterface")]
        [TestCase(typeof(Acme.ISampleDirectAndIndirectExtendedInterface), ExpectedResult = "public interface ISampleDirectAndIndirectExtendedInterface : IEnumerable<string>, IReadOnlyCollection<int>")]
        [TestCase(typeof(BindingFlags), ExpectedResult = "[Flags]\npublic enum BindingFlags")]
        [TestCase(typeof(Predicate<>), ExpectedResult = "public delegate bool Predicate<in T>(T obj)")]
        [TestCase(typeof(Dictionary<,>.KeyCollection.Enumerator), ExpectedResult = "public struct Dictionary<TKey, TValue>.KeyCollection.Enumerator : IEnumerator<TKey>")]
        public string FormatDefinition_ForTypes_ReturnsExpectedString(Type type)
        {
            return cs.FormatDefinition(type.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.SampleConstructors), typeof(object), ExpectedResult = "protected SampleConstructors([NotNull] object o)")]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), ExpectedResult = "public DeepInnerGenericClass()")]
        [TestCase(typeof(Acme.SampleGenericClass<>), ExpectedResult = "public SampleGenericClass<T>()")]
        public string FormatDefinition_ForConstructors_ReturnsExpectedString(Type type, params Type[] paramTypes)
        {
            var constructor = type.GetMetadata<ICompositeType>().Constructors.First(
                c => c.Parameters.Count == paramTypes.Length
                  && c.Parameters.All(p => p.Type.Name == paramTypes[p.Position].Name));

            return cs.FormatDefinition(constructor).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RegularMethod), ExpectedResult = "public void RegularMethod()")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.StaticMethod), ExpectedResult = "public static void StaticMethod()")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.RefParamsMethod), ExpectedResult = "public void RefParamsMethod(in int i, ref string s, out double d)")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.ArrayParamsMethod), ExpectedResult = "public void ArrayParamsMethod(params IEnumerable<string> args)")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.MixedParamsMethod), ExpectedResult = "public unsafe void MixedParamsMethod(string s, in DayOfWeek day = DayOfWeek.Monday, params double?*[][,][,,,] values)")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.UnsafeMethod), ExpectedResult = "public unsafe void UnsafeMethod(int** p)")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.OptionalParamsMethod), ExpectedResult = "public void OptionalParamsMethod(int i = 42, string s = \"default\")")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithTypeConstraints), ExpectedResult = "[return: NotNull]\npublic T GenericMethodWithTypeConstraints<T>([NotNull] T value)\n\twhere T : class, ICloneable, new()")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.GenericMethodWithGenericParameter), ExpectedResult = "public S GenericMethodWithGenericParameter<S>(List<S> list)")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.AbstractMethod), ExpectedResult = "protected abstract void AbstractMethod()")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.VirtualMethod), ExpectedResult = "protected virtual void VirtualMethod(int i)")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.ToString), ExpectedResult = "public sealed override string ToString()")]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethod", ExpectedResult = "void ISampleInterface.InterfaceMethod()")]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethodWithInParam", ExpectedResult = "void ISampleInterface.InterfaceMethodWithInParam(in decimal dec)")]
        [TestCase(typeof(Acme.SampleMethods), "Acme.ISampleInterface.InterfaceMethodWithOutParam", ExpectedResult = "void ISampleInterface.InterfaceMethodWithOutParam(out double d)")]
        [TestCase(typeof(Acme.SampleMethods), nameof(Acme.SampleMethods.InterfaceMethodWithRefParam), ExpectedResult = "public void InterfaceMethodWithRefParam(ref string s)")]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.Method), ExpectedResult = "public abstract object Method(T t, U u, V v)")]
        [TestCase(typeof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass), nameof(Acme.SampleGenericClass<>.InnerGenericClass<,>.DeepInnerGenericClass.GenericMethod), ExpectedResult = "public virtual W GenericMethod<W>(\n\tT t,\n\tU u,\n\tV v,\n\tW w)\n\twhere W : class, new()")]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct), nameof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct.Method), ExpectedResult = "public readonly void Method(T t, U u, V v)")]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct), "Acme.ISampleInterface.InterfaceStaticMethod", ExpectedResult = "static void ISampleInterface.InterfaceStaticMethod()")]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceStaticMethod), ExpectedResult = "static abstract void InterfaceStaticMethod()")]
        [TestCase(typeof(Acme.ISampleInterface), nameof(Acme.ISampleInterface.InterfaceStaticDefaultMethod), ExpectedResult = "static virtual void InterfaceStaticDefaultMethod()")]
        public string FormatDefinition_ForMethods_ReturnsExpectedString(Type type, string methodName, Type[]? paramTypes = null)
        {
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, Acme.Bindings.AllDeclared) : type.GetMethod(methodName, Acme.Bindings.AllDeclared, paramTypes);

            return cs.FormatDefinition(methodInfo!.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.ClassicExtensionMethod), ExpectedResult = "public static void ClassicExtensionMethod(this ISampleInterface instance)")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethod), ExpectedResult = "public void extension(ISampleInterface).InstanceExtensionMethod()")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethod), ExpectedResult = "public static void extension(ISampleInterface).StaticExtensionMethod()")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.GenericExtensionMethod), ExpectedResult = "public void extension(ISampleInterface).GenericExtensionMethod<U>(U value)\n\twhere U : struct")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.ClassicExtensionMethodForClass), ExpectedResult = "public static void ClassicExtensionMethodForClass<W>(this SampleGenericClass<W> instance)\n\twhere W : class")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.InstanceExtensionMethodForClass), ExpectedResult = "public void extension<W>(SampleGenericClass<W>).InstanceExtensionMethodForClass()\n\twhere W : class")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.StaticExtensionMethodForClass), ExpectedResult = "public static void extension<W>(SampleGenericClass<W>).StaticExtensionMethodForClass()\n\twhere W : class")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.GenericExtensionMethodForClass), ExpectedResult = "public void extension<W>(SampleGenericClass<W>).GenericExtensionMethodForClass<U>(U value)\n\twhere W : class\n\twhere U : struct")]
        public string FormatDefinition_ForExtensionMethods_ReturnsExpectedString(Type type, string methodName, Type[]? paramTypes = null)
        {
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, Acme.Bindings.AllDeclared) : type.GetMethod(methodName, Acme.Bindings.AllDeclared, paramTypes);

            return cs.FormatDefinition(methodInfo!.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.SampleOperators), "op_UnaryPlus", ExpectedResult = "public static SampleOperators operator +(SampleOperators x)")]
        [TestCase(typeof(Acme.SampleOperators), "op_UnaryNegation", ExpectedResult = "public static SampleOperators operator -(SampleOperators x)")]
        [TestCase(typeof(Acme.SampleOperators), "op_Addition", ExpectedResult = "public static SampleOperators operator +(SampleOperators x, SampleOperators y)")]
        [TestCase(typeof(Acme.SampleOperators), "op_Subtraction", ExpectedResult = "public static SampleOperators operator -(SampleOperators x, SampleOperators y)")]
        [TestCase(typeof(Acme.SampleOperators), "op_AdditionAssignment", ExpectedResult = "public void operator +=(SampleOperators x)")]
        [TestCase(typeof(Acme.SampleOperators), "op_SubtractionAssignment", ExpectedResult = "public void operator -=(SampleOperators x)")]
        [TestCase(typeof(Acme.SampleOperators), "op_IncrementAssignment", ExpectedResult = "public void operator ++()")]
        [TestCase(typeof(Acme.SampleOperators), "op_DecrementAssignment", ExpectedResult = "public void operator --()")]
        [TestCase(typeof(Acme.SampleOperators), "op_Explicit", ExpectedResult = "public static explicit operator int(SampleOperators x)")]
        [TestCase(typeof(Acme.SampleOperators), "op_Implicit", ExpectedResult = "public static implicit operator string(SampleOperators x)")]
        [TestCase(typeof(Acme.SampleOperators), "Acme.ISampleInterface.op_False", ExpectedResult = "static bool ISampleInterface.operator false(ISampleInterface instance)")]
        [TestCase(typeof(Acme.ISampleInterface), "op_True", ExpectedResult = "static virtual bool operator true(ISampleInterface instance)")]
        [TestCase(typeof(Acme.ISampleInterface), "op_False", ExpectedResult = "static abstract bool operator false(ISampleInterface instance)")]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct), "Acme.ISampleInterface.op_False", ExpectedResult = "static bool ISampleInterface.operator false(ISampleInterface instance)")]
        public string FormatDefinition_ForOperators_ReturnsExpectedString(Type type, string methodName, Type[]? paramTypes = null)
        {
            var methodInfo = paramTypes is null ? type.GetMethod(methodName, Acme.Bindings.AllDeclared) : type.GetMethod(methodName, Acme.Bindings.AllDeclared, paramTypes);

            return cs.FormatDefinition(methodInfo!.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.RegularProperty), ExpectedResult = "public int RegularProperty { get; set; }")]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.ReadOnlyProperty), ExpectedResult = "public int ReadOnlyProperty { get; }")]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.WriteOnlyProperty), ExpectedResult = "public int WriteOnlyProperty { set; }")]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.InitOnlyProperty), ExpectedResult = "public int InitOnlyProperty { get; init; }")]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(int), ExpectedResult = "public int this[int i] { get; }")]
        [TestCase(typeof(Acme.SampleProperties), "Item", typeof(string), typeof(int), ExpectedResult = "public string this[string s, int i] { get; }")]
        [TestCase(typeof(Acme.SampleProperties), nameof(Acme.SampleProperties.StaticProperty), ExpectedResult = "public static string StaticProperty { get; set; }")]
        [TestCase(typeof(Acme.SampleProperties), "Acme.ISampleInterface.InterfaceProperty", ExpectedResult = "int ISampleInterface.InterfaceProperty { get; }")]
        [TestCase(typeof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct), nameof(Acme.SampleGenericStruct<>.InnerGenericStruct<,>.DeepInnerGenericStruct.Property), ExpectedResult = "public readonly T Property { get; }")]
        public string FormatDefinition_ForProperties_ReturnsExpectedString(Type type, string propertyName, params Type[] paramTypes)
        {
            var propertyInfo = type.GetProperty(propertyName, Acme.Bindings.AllDeclared, null, null, paramTypes, null);

            return cs.FormatDefinition(propertyInfo!.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_InstanceExtensionProperty), ExpectedResult = "public int extension(ISampleInterface).InstanceExtensionProperty { get; }")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_StaticExtensionProperty), ExpectedResult = "public static bool extension(ISampleInterface).StaticExtensionProperty { get; }")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_FullExtensionProperty), ExpectedResult = "public string extension(ISampleInterface).FullExtensionProperty { get; set; }")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_InstanceExtensionPropertyForClass), ExpectedResult = "public int extension<W>(SampleGenericClass<W>).InstanceExtensionPropertyForClass { get; }\n\twhere W : class")]
        [TestCase(typeof(Acme.SampleExtensions), nameof(Acme.SampleExtensions.get_StaticExtensionPropertyForClass), ExpectedResult = "public static bool extension<W>(SampleGenericClass<W>).StaticExtensionPropertyForClass { get; }\n\twhere W : class")]
        public string FormatDefinition_ForExtensionProperties_ReturnsExpectedString(Type type, string accessorName)
        {
            MemberInfo accessorInfo = type.GetMethod(accessorName, Acme.Bindings.AllDeclared)!;

            return cs.FormatDefinition(accessorInfo.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.RegularEvent), ExpectedResult = "public event EventHandler RegularEvent")]
        [TestCase(typeof(Acme.SampleEvents), nameof(Acme.SampleEvents.StaticEvent), ExpectedResult = "public static event EventHandler StaticEvent")]
        [TestCase(typeof(Acme.SampleEvents), "Acme.ISampleInterface.InterfaceEvent", ExpectedResult = "event EventHandler ISampleInterface.InterfaceEvent")]
        public string FormatDefinition_ForEvents_ReturnsExpectedString(Type type, string eventName)
        {
            var eventInfo = type.GetEvent(eventName, Acme.Bindings.AllDeclared);

            return cs.FormatDefinition(eventInfo!.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ConstField), ExpectedResult = "public const string ConstField = \"Constant\"")]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.StaticReadonlyField), ExpectedResult = "public static readonly int StaticReadonlyField")]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.FixedBuffer), ExpectedResult = "public unsafe fixed byte FixedBuffer[16]")]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.VolatileField), ExpectedResult = "public volatile int VolatileField")]
        [TestCase(typeof(Acme.SampleFields), nameof(Acme.SampleFields.ComplexField), ExpectedResult = "public unsafe double?*[][,][,,] ComplexField")]
        public string FormatDefinition_ForFields_ReturnsExpectedString(Type type, string fieldName)
        {
            var fieldInfo = type.GetField(fieldName, Acme.Bindings.AllDeclared);

            return cs.FormatDefinition(fieldInfo!.GetMetadata()).Replace("\r", string.Empty);
        }

        [TestCase("N:Acme", ExpectedResult = "Acme")]
        [TestCase("T:Acme.SampleGenericClass`1", ExpectedResult = "SampleGenericClass<T>")]
        [TestCase("T:Acme.SampleGenericClass`1.InnerGenericClass`2", ExpectedResult = "SampleGenericClass<T>.InnerGenericClass<U, V>")]
        [TestCase("T:Acme.SampleGenericClass`1.InnerGenericClass`2.DeepInnerGenericClass", ExpectedResult = "SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass")]
        [TestCase("F:Acme.SampleFields.ConstField", ExpectedResult = "SampleFields.ConstField")]
        [TestCase("F:Acme.SampleFields.StaticReadonlyField", ExpectedResult = "SampleFields.StaticReadonlyField")]
        [TestCase("P:Acme.SampleProperties.Item(System.String,System.Int32)", ExpectedResult = "SampleProperties.Item[string, int]")]
        [TestCase("P:Acme.SampleProperties.Acme#ISampleInterface#InterfaceProperty", ExpectedResult = "SampleProperties.Acme.ISampleInterface.InterfaceProperty")]
        [TestCase("M:Acme.SampleConstructors.#ctor(System.Object)", ExpectedResult = "SampleConstructors(object)")]
        [TestCase("M:Acme.SampleMethods.Acme#ISampleInterface#InterfaceMethod", ExpectedResult = "SampleMethods.Acme.ISampleInterface.InterfaceMethod()")]
        [TestCase("M:Acme.SampleMethods.RefParamsMethod(System.Int32@,System.String@,System.Double@)", ExpectedResult = "SampleMethods.RefParamsMethod(in int, ref string, out double)")]
        [TestCase("M:Acme.SampleGenericClass`1.InnerGenericClass`2.DeepInnerGenericClass.GenericMethod``1(`0,`1,`2,``0)", ExpectedResult = "SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass.GenericMethod<W>(T, U, V, W)")]
        [TestCase("M:Acme.SampleOperators.op_Addition(Acme.SampleOperators,Acme.SampleOperators)", ExpectedResult = "SampleOperators.Addition(SampleOperators, SampleOperators)")]
        [TestCase("M:Acme.SampleOperators.op_IncrementAssignment", ExpectedResult = "SampleOperators.IncrementAssignment()")]
        [TestCase("E:Acme.SampleEvents.RegularEvent", ExpectedResult = "SampleEvents.RegularEvent")]
        [TestCase("E:Acme.SampleEvents.Acme#ISampleInterface#InterfaceEvent", ExpectedResult = "SampleEvents.Acme.ISampleInterface.InterfaceEvent")]
        [TestCase("T:Unresolvable.Type", ExpectedResult = "T:Unresolvable.Type")]
        public string FormatCodeReference_WithoutQualifierSelector_ReturnsExpectedString(string cref)
        {
            return cs.FormatCodeReference(cref);
        }

        [TestCase("N:Acme", ExpectedResult = "Acme")]
        [TestCase("T:Acme.SampleGenericClass`1", ExpectedResult = "SampleGenericClass<T>")]
        [TestCase("T:Acme.SampleGenericClass`1.InnerGenericClass`2", ExpectedResult = "SampleGenericClass<T>.InnerGenericClass<U, V>")]
        [TestCase("T:Acme.SampleGenericClass`1.InnerGenericClass`2.DeepInnerGenericClass", ExpectedResult = "SampleGenericClass<T>.InnerGenericClass<U, V>.DeepInnerGenericClass")]
        [TestCase("F:Acme.SampleFields.ConstField", ExpectedResult = "ConstField")]
        [TestCase("F:Acme.SampleFields.StaticReadonlyField", ExpectedResult = "StaticReadonlyField")]
        [TestCase("P:Acme.SampleProperties.Item(System.String,System.Int32)", ExpectedResult = "Item[string, int]")]
        [TestCase("P:Acme.SampleProperties.Acme#ISampleInterface#InterfaceProperty", ExpectedResult = "ISampleInterface.InterfaceProperty")]
        [TestCase("M:Acme.SampleConstructors.#ctor(System.Object)", ExpectedResult = "SampleConstructors(object)")]
        [TestCase("M:Acme.SampleMethods.Acme#ISampleInterface#InterfaceMethod", ExpectedResult = "ISampleInterface.InterfaceMethod()")]
        [TestCase("M:Acme.SampleMethods.RefParamsMethod(System.Int32@,System.String@,System.Double@)", ExpectedResult = "RefParamsMethod(in int, ref string, out double)")]
        [TestCase("M:Acme.SampleGenericClass`1.InnerGenericClass`2.DeepInnerGenericClass.GenericMethod``1(`0,`1,`2,``0)", ExpectedResult = "GenericMethod<W>(T, U, V, W)")]
        [TestCase("M:Acme.SampleOperators.op_Addition(Acme.SampleOperators,Acme.SampleOperators)", ExpectedResult = "Addition(SampleOperators, SampleOperators)")]
        [TestCase("M:Acme.SampleOperators.op_IncrementAssignment", ExpectedResult = "IncrementAssignment()")]
        [TestCase("E:Acme.SampleEvents.RegularEvent", ExpectedResult = "RegularEvent")]
        [TestCase("E:Acme.SampleEvents.Acme#ISampleInterface#InterfaceEvent", ExpectedResult = "ISampleInterface.InterfaceEvent")]
        [TestCase("T:Unresolvable.Type", ExpectedResult = "T:Unresolvable.Type")]
        public string FormatCodeReference_WithQualifierSelector_ReturnsExpectedString(string cref)
        {
            return cs.FormatCodeReference(cref, static member => member is IType ? NameQualifier.DeclaringType : NameQualifier.None);
        }
    }
}
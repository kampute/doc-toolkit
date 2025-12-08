// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Metadata.Reflection.Internal;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides access to the reflection information of extension members defined within an extension container type.
    /// </summary>
    /// <remarks>
    /// This class analyzes a top-level, non-generic static class to collect and expose reflection information about extension methods and properties.
    /// It supports two categories of extension members:
    /// <list type="bullet">
    ///   <item>
    ///     <term>Classic extension methods</term>
    ///     <description>Static methods where the first parameter is marked with <see langword="this"/> keyword.</description>
    ///   </item>
    ///   <item>
    ///     <term>Block extension members</term>
    ///     <description>Methods and properties defined within the <see langword="extension"/> blocks.</description>
    ///   </item>
    /// </list>
    /// The class provides normalized views of extension methods, representing them as if they were members of the extended type, and allows retrieval 
    /// of the underlying extension member information.
    /// </remarks>
    public class ExtensionContainer
    {
        private readonly Type containerType;
        private readonly List<MethodInfo> extensionMethods = [];
        private readonly List<PropertyInfo> extensionProperties = [];
        private readonly Dictionary<MethodInfo, MemberInfo> memberMapping = new(ReferenceEqualityComparer<MethodInfo>.Instance);
        private readonly Dictionary<MethodInfo, MethodInfo> normalizedMethodMapping = new(ReferenceEqualityComparer<MethodInfo>.Instance);

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionContainer"/> class.
        /// </summary>
        /// <param name="containerType">The container type.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="containerType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="containerType"/> is not a top-level non-generic static class.</exception>
        public ExtensionContainer(Type containerType)
        {
            if (containerType is null)
                throw new ArgumentNullException(nameof(containerType));
            if (!containerType.IsSealed || !containerType.IsAbstract || containerType.IsNested || containerType.IsGenericType)
                throw new ArgumentException("Container type must be a top-level non-generic static class.", nameof(containerType));

            this.containerType = containerType;
            CollectExtensionMembers();
        }

        /// <summary>
        /// Gets the reflection information for all extension methods declared on the container type.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="MethodInfo"/> instances representing the extension methods declared on the container type.
        /// </value>
        public IReadOnlyList<MethodInfo> DeclaredExtensionMethods => extensionMethods;

        /// <summary>
        /// Gets the reflection information for all extension properties declared on the container type.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="PropertyInfo"/> instances representing the extension properties declared on the container type.
        /// </value>
        public IReadOnlyList<PropertyInfo> DeclaredExtensionProperties => extensionProperties;

        /// <summary>
        /// Retrieves the normalized reflection information for the specified method.
        /// </summary>
        /// <param name="methodInfo">The reflection information of the method to normalize.</param>
        /// <returns>A <see cref="MethodInfo"/> representing the normalized reflection information for the specified method.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="methodInfo"/> does not belong to the container type.</exception>
        /// <remarks>
        /// This method returns the normalized reflection information for the specified method if it is an extension method or property
        /// accessor. If the method does not represent an extension member, the original method is returned.
        /// <para>
        /// The normalized method of an extension method is a logical view of the method as it would appear when invoked on the extended
        /// type, rather than as a static method on the container type that has the extended type as its first parameter.
        /// </para>
        /// </remarks>
        public virtual MethodInfo GetNormalizedMethodInfo(MethodInfo methodInfo)
        {
            if (methodInfo is null)
                throw new ArgumentNullException(nameof(methodInfo));

            if (!ReferenceEquals(methodInfo.DeclaringType, containerType))
                throw new ArgumentException("Method does not belong to the container type.", nameof(methodInfo));

            return normalizedMethodMapping.TryGetValue(methodInfo, out var normalizedMethodInfo) ? normalizedMethodInfo : methodInfo;
        }

        /// <summary>
        /// Attempts to get extension member information for the specified method.
        /// </summary>
        /// <param name="methodInfo">The reflection information of the method to examine.</param>
        /// <returns>The extension member information if found; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="methodInfo"/> does not belong to the container type.</exception>
        /// <remarks>
        /// This method returns the <see cref="MemberInfo"/> representing the extension member associated with the
        /// specified <paramref name="methodInfo"/>.
        /// <list type="bullet">
        ///   <item>If the method is an extension method, the returned <see cref="MemberInfo"/> will be a <see cref="MethodInfo"/>.</item>
        ///   <item>If the method is an extension property accessor, the returned <see cref="MemberInfo"/> will be a <see cref="PropertyInfo"/>.</item>
        ///   <item>If the method is neither, the method returns <see langword="null"/>.</item>
        /// </list>
        /// </remarks>
        public virtual MemberInfo? GetExtensionMemberInfo(MethodInfo methodInfo)
        {
            if (methodInfo is null)
                throw new ArgumentNullException(nameof(methodInfo));

            if (!ReferenceEquals(methodInfo.DeclaringType, containerType))
                throw new ArgumentException("Method does not belong to the container type.", nameof(methodInfo));

            return memberMapping.TryGetValue(methodInfo, out var member) ? member : null;
        }

        /// <summary>
        /// Collects all extension members defined within the container type.
        /// </summary>
        private void CollectExtensionMembers()
        {
            var methods = new List<MethodGroup>(GetStaticMethodGroups());

            // Process block extension members
            foreach (var stub in GetBlockExtensionStubs())
            {
                var methodGroup = FindAndMarkProcessed(methods, stub);
                if (methodGroup is null)
                    continue;

                switch (stub.Member)
                {
                    case MethodInfo stubMethod:
                        var method = methodGroup.Method!;
                        var extensionMethod = new ExtensionMethodInfo(method, stub.Receiver, stubMethod);
                        extensionMethods.Add(extensionMethod);
                        AddMapping(method, extensionMethod, extensionMethod);
                        break;
                    case PropertyInfo stubProperty:
                        var getter = methodGroup.Getter;
                        var setter = methodGroup.Setter;
                        var extensionProperty = new ExtensionPropertyInfo(getter, setter, stub.Receiver, stubProperty);
                        extensionProperties.Add(extensionProperty);
                        if (getter is not null)
                            AddMapping(getter, extensionProperty.GetMethod!, extensionProperty);
                        if (setter is not null)
                            AddMapping(setter, extensionProperty.SetMethod!, extensionProperty);
                        break;
                }
            }

            // Process remaining classic extension methods
            foreach (var methodGroup in methods)
            {
                if (methodGroup?.Method is null || !IsClassisExtensionMethod(methodGroup.Method))
                    continue;

                var extensionMethod = new ExtensionMethodInfo(methodGroup.Method);
                extensionMethods.Add(extensionMethod);
                AddMapping(methodGroup.Method, extensionMethod, extensionMethod);
            }
        }

        /// <summary>
        /// Adds a mapping between the original method, its normalized version, and the associated member.
        /// </summary>
        /// <param name="method">The original method.</param>
        /// <param name="normalizedMethod">The normalized method.</param>
        /// <param name="member">The associated member.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddMapping(MethodInfo method, MethodInfo normalizedMethod, MemberInfo member)
        {
            memberMapping[method] = member;
            normalizedMethodMapping[method] = normalizedMethod;
        }

        /// <summary>
        /// Collects all static method groups defined within the container type.
        /// </summary>
        /// <returns>An enumerable of <see cref="MethodGroup"/> instances.</returns>
        private IEnumerable<MethodGroup> GetStaticMethodGroups()
        {
            var propertyGroups = new Dictionary<string, MethodGroup>(StringComparer.Ordinal);

            foreach (var method in containerType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (method.IsSpecialName)
                    continue;

                if (method.Name.StartsWith("get_", StringComparison.Ordinal))
                {
                    var key = method.Name[4..];
                    if (!propertyGroups.TryGetValue(method.Name[4..], out var group))
                    {
                        group = new MethodGroup();
                        propertyGroups[key] = group;
                    }
                    group.Getter = method;
                    continue;
                }

                if (method.Name.StartsWith("set_", StringComparison.Ordinal))
                {
                    var key = method.Name[4..];
                    if (!propertyGroups.TryGetValue(key, out var group))
                    {
                        group = new MethodGroup();
                        propertyGroups[key] = group;
                    }
                    group.Setter = method;
                    continue;
                }

                yield return new MethodGroup { Method = method };
            }

            foreach (var group in propertyGroups.Values)
                yield return group;
        }

        /// <summary>
        /// Finds a method group that matches the stub and marks it as processed.
        /// </summary>
        /// <param name="methods">The list of method groups to search.</param>
        /// <param name="stub">The stub to match.</param>
        /// <returns>The matching method group, or <see langword="null"/> if not found.</returns>
        private static MethodGroup? FindAndMarkProcessed(List<MethodGroup> methods, ExtensionStub stub)
        {
            for (var i = 0; i < methods.Count; i++)
            {
                var methodGroup = methods[i];
                if (methodGroup is not null && stub.Matches(methodGroup))
                {
                    methods[i] = null!; // Mark as processed
                    return methodGroup;
                }
            }
            return null;
        }

        /// <summary>
        /// Collects all the extension member stubs defined within the container type.
        /// </summary>
        /// <returns>An enumerable of <see cref="ExtensionStub"/> instances.</returns>
        private IEnumerable<ExtensionStub> GetBlockExtensionStubs()
        {
            if (!HasExtensionAttribute(containerType))
                yield break;

            foreach (var grouping in GetGroupings())
                foreach (var stub in GetGroupStubs(grouping))
                    yield return stub;
        }

        /// <summary>
        /// Collects the grouping types defined within the container type.
        /// </summary>
        /// <returns>An enumerable of grouping types.</returns>
        private IEnumerable<Type> GetGroupings()
        {
            foreach (var type in containerType.GetNestedTypes())
            {
                if (type.IsSpecialName && HasExtensionAttribute(type))
                    yield return type;
            }
        }

        /// <summary>
        /// Retrieves the marker method that defines the receiver parameters for the specified grouping type. 
        /// </summary>
        /// <param name="grouping">The grouping type.</param>
        /// <returns>The marker method info if found; otherwise, <see langword="null"/>.</returns>
        private static MethodInfo? GetGroupMarkerMethod(Type grouping)
        {
            foreach (var markerType in grouping.GetNestedTypes())
            {
                if (!markerType.IsSpecialName)
                    continue;

                var markerMethod = markerType.GetMethod("<Extension>$", BindingFlags.Public | BindingFlags.Static);
                if (markerMethod is not null)
                    return markerMethod;
            }

            return null;
        }

        /// <summary>
        /// Retrieves the extension member stubs defined within the specified grouping type.
        /// </summary>
        /// <param name="grouping">The grouping type.</param>
        /// <returns>Ab enumerable of <see cref="ExtensionStub"/> instances.</returns>
        private static IEnumerable<ExtensionStub> GetGroupStubs(Type grouping)
        {
            var markerMethod = GetGroupMarkerMethod(grouping);
            if (markerMethod is null)
                yield break;

            var receiver = markerMethod.GetParameters()[0];
            var markerName = markerMethod.DeclaringType.Name;
            foreach (var member in grouping.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (GetExtensionMarkerName(member) == markerName)
                    yield return new(member, receiver);
            }
        }

        /// <summary>
        /// Retrieves the name of the extension marker associated with the specified member.
        /// </summary>
        /// <param name="member">The member to inspect.</param>
        /// <returns>The name of the extension marker if found; otherwise, <see langword="null"/>.</returns>
        private static string? GetExtensionMarkerName(MemberInfo member)
        {
            foreach (var attribute in member.CustomAttributes)
            {
                if (attribute.AttributeType.FullName != AttributeNames.ExtensionMarker)
                    continue;

                if (attribute.ConstructorArguments.Count == 1)
                    return attribute.ConstructorArguments[0].Value as string;

                foreach (var namedArg in attribute.NamedArguments)
                {
                    if (namedArg.MemberName == "Name")
                        return namedArg.TypedValue.Value as string;
                }

                break;
            }

            return null;
        }

        /// <summary>
        /// Determines whether the specified member has the Extension attribute.
        /// </summary>
        /// <param name="member">The member to inspect.</param>
        /// <returns><see langword="true"/> if the member has the Extension attribute; otherwise, <see langword="false"/>.</returns>
        private static bool HasExtensionAttribute(MemberInfo member)
        {
            foreach (var attribute in member.CustomAttributes)
            {
                if (attribute.AttributeType.FullName == AttributeNames.Extension)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified method is a classic extension method.
        /// </summary>
        /// <param name="method">The method to inspect.</param>
        /// <returns><see langword="true"/> if the method is a classic extension method; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsClassisExtensionMethod(MethodInfo method)
        {
            return method.IsStatic
                && HasExtensionAttribute(method)
                && method.GetParameters().Length > 0;
        }

        /// <summary>
        /// Represents a group of methods related to a single member (method or property).
        /// </summary>
        private sealed class MethodGroup
        {
            /// <summary>
            /// The method info of the method member.
            /// </summary>
            public MethodInfo? Method;

            /// <summary>
            /// The getter method info of the property member.
            /// </summary>
            public MethodInfo? Getter;

            /// <summary>
            /// The setter method info of the property member.
            /// </summary>
            public MethodInfo? Setter;

            /// <summary>
            /// Returns a string that represents the current method group.
            /// </summary>
            /// <returns>A string that represents the current method group.</returns>
            public override string? ToString() => Method is not null
                ? $"Method: {Method.Name}"
                : $"Property: {(Getter ?? Setter)!.Name[4..]}";
        }

        /// <summary>
        /// Represents a stub for an extension member used in block extension definitions.
        /// </summary>
        private readonly struct ExtensionStub
        {
            /// <summary>
            /// The member info of the stub extension member.
            /// </summary>
            public readonly MemberInfo Member;

            /// <summary>
            /// The receiver parameter info associated with the stub extension member.
            /// </summary>
            public readonly ParameterInfo Receiver;

            /// <summary>
            /// Initializes a new instance of the <see cref="ExtensionStub"/> struct.
            /// </summary>
            /// <param name="member">The member info of the stub extension member.</param>
            /// <param name="receiver">The receiver parameter info associated with the stub extension member.</param>
            public ExtensionStub(MemberInfo member, ParameterInfo receiver)
            {
                Member = member;
                Receiver = receiver;
            }

            public override string? ToString()
            {
                var kind = Member is MethodInfo ? "Extension Method" : "Extension Property";
                return $"{kind} {Member.Name} for {Receiver.ParameterType}";
            }

            /// <summary>
            /// Determines whether the specified method group matches this stub.
            /// </summary>
            /// <param name="methodGroup">The method group to compare.</param>
            /// <returns><see langword="true"/> if the method group matches this stub; otherwise, <see langword="false"/>.</returns>
            public bool Matches(MethodGroup methodGroup)
            {
                switch (Member)
                {
                    case MethodInfo stubMethod when methodGroup.Method is MethodInfo method:
                        return SignatureMatches(method, stubMethod, Receiver);
                    case PropertyInfo stubProperty when stubProperty.GetMethod is MethodInfo stubGetter && methodGroup.Getter is MethodInfo getter:
                        return SignatureMatches(getter, stubGetter, Receiver);
                    case PropertyInfo stubProperty when stubProperty.SetMethod is MethodInfo stubSetter && methodGroup.Setter is MethodInfo setter:
                        return SignatureMatches(setter, stubSetter, Receiver);
                    default:
                        return false;
                }
            }

            /// <summary>
            /// Determines whether the signature of the method matches the stub method, considering the receiver parameter.
            /// </summary>
            /// <param name="method">The method to compare.</param>
            /// <param name="stub">The stub method to compare against.</param>
            /// <param name="receiverParameter">The receiver parameter info.</param>
            /// <returns><see langword="true"/> if the signatures match; otherwise, <see langword="false"/>.</returns>
            private static bool SignatureMatches(MethodInfo method, MethodInfo stub, ParameterInfo receiverParameter)
            {
                if (method.Name != stub.Name)
                    return false;

                if (method.ReturnType.FullName != stub.ReturnType.FullName)
                    return false;

                var methodParameters = method.GetParameters();
                var stubParameters = stub.GetParameters();

                var skipParam = 0;
                if (!stub.IsStatic)
                {
                    skipParam = 1;
                    if (methodParameters.Length == 0 || !ShapeEquals(methodParameters[0], receiverParameter))
                        return false;
                }

                if (methodParameters.Length != skipParam + stubParameters.Length)
                    return false;

                for (var i = 0; i < stubParameters.Length; i++)
                {
                    if (!ShapeEquals(methodParameters[skipParam + i], stubParameters[i]))
                        return false;
                }

                return true;
            }

            /// <summary>
            /// Determines whether the shapes of two parameters are equal.
            /// </summary>
            /// <param name="a">The first parameter to compare.</param>
            /// <param name="b">The second parameter to compare.</param>
            /// <returns><see langword="true"/> if the shapes are equal; otherwise, <see langword="false"/>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static bool ShapeEquals(ParameterInfo a, ParameterInfo b)
            {
                var aTypeName = a.ParameterType.FullName ?? a.ParameterType.Name;
                var bTypeName = b.ParameterType.FullName ?? b.ParameterType.Name;
                return aTypeName == bTypeName && a.IsOut == b.IsOut && a.IsIn == b.IsIn;
            }
        }
    }
}

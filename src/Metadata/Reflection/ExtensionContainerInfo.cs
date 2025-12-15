// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Reflection
{
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Reflection.Internal;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides access to the reflection information of extension blocks and their members defined within a top-level static class container.
    /// </summary>
    /// <remarks>
    /// This class analyzes a top-level, non-generic static class to collect and expose reflection information about extension methods and properties
    /// defined within its extension blocks.
    /// <para>
    /// The class provides normalized views of extension block members, representing them as if they were members of the extended type, and allows 
    /// retrieval of the underlying extension member information.
    /// </para>
    /// </remarks>
    public sealed class ExtensionContainerInfo
    {
        private readonly Dictionary<MethodInfo, MemberInfo> memberMapping = new(ReferenceEqualityComparer<MethodInfo>.Instance);
        private readonly Dictionary<MethodInfo, MethodInfo> normalizedMethodMapping = new(ReferenceEqualityComparer<MethodInfo>.Instance);

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionContainerInfo"/> class.
        /// </summary>
        /// <param name="containerType">The container type.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="containerType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="containerType"/> is not a top-level non-generic static class.</exception>
        public ExtensionContainerInfo(Type containerType)
        {
            if (containerType is null)
                throw new ArgumentNullException(nameof(containerType));
            if (!containerType.IsSealed || !containerType.IsAbstract || containerType.IsNested || containerType.IsGenericType)
                throw new ArgumentException("Container type must be a top-level non-generic static class.", nameof(containerType));

            ContainerType = containerType;
            ExtensionBlocks = [.. GetExtensionBlocks(containerType)];

            if (ExtensionBlocks.Count > 0)
                PrepareMemberMappings();
        }

        /// <summary>
        /// Gets the container type.
        /// </summary>
        /// <value>
        /// The <see cref="Type"/> representing the container type.
        /// </value>
        public Type ContainerType { get; }

        /// <summary>
        /// Gets the extension blocks defined within the container type.
        /// </summary>
        /// <value>
        /// A read-only list of <see cref="ExtensionBlockInfo"/> instances representing the extension blocks defined within the container type.
        /// </value>
        public IReadOnlyList<ExtensionBlockInfo> ExtensionBlocks { get; }

        /// <summary>
        /// Gets all block extension methods defined within the container type.
        /// </summary>
        /// <value>
        /// An enumerable of <see cref="MethodInfo"/> instances representing all block extension methods defined within the container type.
        /// </value>
        public IEnumerable<MethodInfo> ExtensionBlockMethods => memberMapping.Values
            .OfType<MethodInfo>()
            .Distinct(ReferenceEqualityComparer<MethodInfo>.Instance);

        /// <summary>
        /// Gets all block extension properties defined within the container type.
        /// </summary>
        /// <value>
        /// An enumerable of <see cref="PropertyInfo"/> instances representing all block extension properties defined within the container type.
        /// </value>
        public IEnumerable<PropertyInfo> ExtensionBlockProperties => memberMapping.Values
            .OfType<PropertyInfo>()
            .Distinct(ReferenceEqualityComparer<PropertyInfo>.Instance);

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
        public MethodInfo GetNormalizedMethodInfo(MethodInfo methodInfo)
        {
            if (methodInfo is null)
                throw new ArgumentNullException(nameof(methodInfo));

            if (!ReferenceEquals(methodInfo.DeclaringType, ContainerType))
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
        /// This method returns the <see cref="MemberInfo"/> representing the extension member associated with the specified <paramref name="methodInfo"/>.
        /// <list type="bullet">
        ///   <item>If the method is an extension method, the returned <see cref="MemberInfo"/> will be a <see cref="MethodInfo"/>.</item>
        ///   <item>If the method is an extension property accessor, the returned <see cref="MemberInfo"/> will be a <see cref="PropertyInfo"/>.</item>
        ///   <item>If the method is neither or a classic extension method, the method returns <see langword="null"/>.</item>
        /// </list>
        /// </remarks>
        public MemberInfo? GetExtensionMemberInfo(MethodInfo methodInfo)
        {
            if (methodInfo is null)
                throw new ArgumentNullException(nameof(methodInfo));

            if (!ReferenceEquals(methodInfo.DeclaringType, ContainerType))
                throw new ArgumentException("Method does not belong to the container type.", nameof(methodInfo));

            return memberMapping.TryGetValue(methodInfo, out var member) ? member : null;
        }

        /// <summary>
        /// Prepares the mappings between extension members and their normalized representations.
        /// </summary>
        private void PrepareMemberMappings()
        {
            var (methodCandidates, propertyCandidates) = CollectExtensionMemberCandidates(ContainerType);

            foreach (var block in ExtensionBlocks)
            {
                var receiver = block.ReceiverParameter;

                foreach (var method in block.ExtensionMethods)
                {
                    var i = methodCandidates.FindIndex(m => m is not null && m.Matches(receiver, method));
                    if (i == -1)
                        continue;

                    var candidate = methodCandidates[i]!;
                    methodCandidates[i] = null!; // Mark as used

                    var extensionMethod = new ExtensionBlockMethodInfo(block, method, candidate.Method);
                    RegisterMapping(candidate.Method, extensionMethod, extensionMethod);
                }

                foreach (var property in block.ExtensionProperties)
                {
                    var i = propertyCandidates.FindIndex(p => p is not null && p.Matches(receiver, property));
                    if (i == -1)
                        continue;

                    var candidate = propertyCandidates[i]!;
                    propertyCandidates[i] = null!; // Mark as used

                    var extensionProperty = new ExtensionBlockPropertyInfo(block, property, candidate.Getter, candidate.Setter);
                    if (candidate.Getter is not null)
                        RegisterMapping(candidate.Getter, extensionProperty.GetMethod!, extensionProperty);
                    if (candidate.Setter is not null)
                        RegisterMapping(candidate.Setter, extensionProperty.SetMethod!, extensionProperty);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void RegisterMapping(MethodInfo method, MethodInfo normalizedMethod, MemberInfo member)
            {
                memberMapping[method] = member;
                normalizedMethodMapping[method] = normalizedMethod;
            }
        }

        /// <summary>
        /// Collects all members of the container type that can serve as candidates for extension methods and properties.
        /// </summary>
        /// <param name="container">The container type.</param>
        /// <returns>A tuple containing lists of method and property candidates.</returns>
        private static (List<ExtensionMethodCandidate>, List<ExtensionPropertyCandidate>) CollectExtensionMemberCandidates(Type container)
        {
            var methods = new List<ExtensionMethodCandidate>();
            var properties = new Dictionary<string, ExtensionPropertyCandidate>(StringComparer.Ordinal);

            foreach (var method in container.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            {
                if (method.IsSpecialName || method.IsPrivate)
                    continue;

                // Handle property getters
                if (method.Name.StartsWith("get_", StringComparison.Ordinal))
                {
                    var key = method.Name[4..];
                    if (!properties.TryGetValue(method.Name[4..], out var property))
                    {
                        property = new ExtensionPropertyCandidate();
                        properties[key] = property;
                    }
                    property.Getter = method;
                    continue;
                }

                // Handle property setters
                if (method.Name.StartsWith("set_", StringComparison.Ordinal))
                {
                    var key = method.Name[4..];
                    if (!properties.TryGetValue(key, out var property))
                    {
                        property = new ExtensionPropertyCandidate();
                        properties[key] = property;
                    }
                    property.Setter = method;
                    continue;
                }

                // Handle regular methods
                methods.Add(new ExtensionMethodCandidate(method));
            }

            return (methods, properties.Values.ToList());
        }

        /// <summary>
        /// Collects the extension blocks defined within the container type.
        /// </summary>
        /// <param name="container">The container type.</param>
        /// <returns>An enumerable of <see cref="ExtensionBlockInfo"/> instances.</returns>
        private static IEnumerable<ExtensionBlockInfo> GetExtensionBlocks(Type container)
        {
            foreach (var type in container.GetNestedTypes())
            {
                if (type.IsSpecialName && HasExtensionAttribute(type))
                    yield return new ExtensionBlockInfo(type);
            }
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
        /// Determines whether the signature of the declared method matches that of the extension method stub.
        /// </summary>
        /// <param name="declaredMethod">The reflection information of the declared method.</param>
        /// <param name="receiverParameter">The receiver parameter of the extension method.</param>
        /// <param name="extensionMethodStub">The reflection information of the extension method stub.</param>
        /// <returns><see langword="true"/> if the signatures match; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any of the parameters is <see langword="null"/>.</exception>
        private static bool ExtensionSignatureMatches(MethodInfo declaredMethod, ParameterInfo receiverParameter, MethodInfo extensionMethodStub)
        {
            if (declaredMethod is null)
                throw new ArgumentNullException(nameof(declaredMethod));
            if (receiverParameter is null)
                throw new ArgumentNullException(nameof(receiverParameter));
            if (extensionMethodStub is null)
                throw new ArgumentNullException(nameof(extensionMethodStub));

            if (declaredMethod.Name != extensionMethodStub.Name)
                return false;

            if (declaredMethod.ReturnType.FullName != extensionMethodStub.ReturnType.FullName)
                return false;

            var methodParameters = declaredMethod.GetParameters();
            var stubParameters = extensionMethodStub.GetParameters();

            var skipParam = 0;
            if (!extensionMethodStub.IsStatic)
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

            static bool ShapeEquals(ParameterInfo x, ParameterInfo y)
            {
                if (x.Name != y.Name || x.IsOut != y.IsOut || x.IsIn != y.IsIn)
                    return false;

                var xType = x.ParameterType;
                var yType = y.ParameterType;

                return xType.Name == yType.Name
                    && xType.Namespace == yType.Namespace
                    && xType.IsGenericParameter == yType.IsGenericParameter;
            }
        }

        /// <summary>
        /// Represents a candidate method for extension matching.
        /// </summary>
        private sealed class ExtensionMethodCandidate
        {
            /// <summary>
            /// The information of the candidate extension method.
            /// </summary>
            public readonly MethodInfo Method;

            /// <summary>
            /// Initializes a new instance of the <see cref="ExtensionMethodCandidate"/> class.
            /// </summary>
            /// <param name="method">The candidate extension method.</param>
            public ExtensionMethodCandidate(MethodInfo method)
            {
                Method = method;
            }

            /// <summary>
            /// Determines whether the method matches the specified extension method.
            /// </summary>
            /// <param name="receiverParameter">The receiver parameter of the extension method.</param>
            /// <param name="extensionMethodStub">The extension method stub to compare against.</param>
            /// <returns><see langword="true"/> if the method group matches the extension method; otherwise, <see langword="false"/>.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Matches(ParameterInfo receiverParameter, MethodInfo extensionMethodStub)
                => ExtensionSignatureMatches(Method, receiverParameter, extensionMethodStub);
        }

        /// <summary>
        /// Represents a candidate property for extension matching.
        /// </summary>
        private sealed class ExtensionPropertyCandidate
        {
            /// <summary>
            /// The getter method information of the extension property candidate.
            /// </summary>
            public MethodInfo? Getter;

            /// <summary>
            /// The setter method information of the extension property candidate.
            /// </summary>
            public MethodInfo? Setter;

            /// <summary>
            /// Determines whether the property matches the specified extension property.
            /// </summary>
            /// <param name="receiverParameter">The receiver parameter of the extension property.</param>
            /// <param name="extensionPropertyStub">The extension property stub to compare against.</param>
            /// <returns><see langword="true"/> if the method group matches the extension property; otherwise, <see langword="false"/>.</returns>
            public bool Matches(ParameterInfo receiverParameter, PropertyInfo extensionPropertyStub)
            {
                var getter = extensionPropertyStub.GetMethod;
                if (getter is null != Getter is null)
                    return false;

                if (getter is not null && !ExtensionSignatureMatches(Getter!, receiverParameter, getter))
                    return false;

                var setter = extensionPropertyStub.SetMethod;
                if (setter is null != Setter is null)
                    return false;

                if (setter is not null && !ExtensionSignatureMatches(Setter!, receiverParameter, setter))
                    return false;

                return true;
            }
        }
    }
}

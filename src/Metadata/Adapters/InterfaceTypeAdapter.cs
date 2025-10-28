// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// An adapter that wraps a reflection <see cref="Type"/> representing an interface type and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="Type"/> and the metadata
    /// representation defined by the <see cref="IInterfaceType"/> interface. It provides access to interface-level
    /// information regardless of whether the assembly containing the interface's type was loaded via Common
    /// Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class InterfaceTypeAdapter : GenericCapableTypeAdapter, IInterfaceType
    {
        private readonly Lazy<IReadOnlyList<IMethod>> methods;
        private readonly Lazy<IReadOnlyList<IProperty>> properties;
        private readonly Lazy<IReadOnlyList<IEvent>> events;
        private readonly Lazy<IReadOnlyList<IInterfaceType>> interfaces;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceTypeAdapter"/> class.
        /// </summary>
        /// <param name="declaringEntity">The assembly or type that declares the interface type.</param>
        /// <param name="interfaceType">The reflection information of the interface type to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringEntity"/> or <paramref name="interfaceType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is neither an <see cref="IAssembly"/> nor an <see cref="IType"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="interfaceType"/> is not declared by the <paramref name="declaringEntity"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="interfaceType"/> is a nested type but <paramref name="declaringEntity"/> is an assembly, or when <paramref name="interfaceType"/> is a top-level type but <paramref name="declaringEntity"/> is a type.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="interfaceType"/> is not an interface.</exception>
        public InterfaceTypeAdapter(object declaringEntity, Type interfaceType)
            : base(declaringEntity, interfaceType)
        {
            if (!interfaceType.IsInterface)
                throw new ArgumentException("Type must be an interface.", nameof(interfaceType));

            methods = new(() => [.. GetMethods()]);
            properties = new(() => [.. GetProperties()]);
            events = new(() => [.. GetEvents()]);
            interfaces = new(() => [.. GetInterfaces()]);
        }

        /// <inheritdoc/>
        public virtual IReadOnlyList<IInterfaceType> Interfaces => interfaces.Value;

        /// <inheritdoc/>
        public virtual IReadOnlyList<IMethod> Methods => methods.Value;

        /// <inheritdoc/>
        public virtual IReadOnlyList<IProperty> Properties => properties.Value;

        /// <inheritdoc/>
        public virtual IReadOnlyList<IEvent> Events => events.Value;

        /// <inheritdoc/>
        public override bool IsAssignableFrom(IType source)
            => base.IsAssignableFrom(source) || (source is IInterfaceCapableType iSource && iSource.Implements(this));

        /// <summary>
        /// Gets the interfaces extended or inherited by the interface.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IInterfaceType"/> objects representing the interfaces extended or inherited by the interface.</returns>
        protected virtual IEnumerable<IInterfaceType> GetInterfaces() => Reflection
            .GetInterfaces()
            .Where(i => i.IsPublic || i.IsNestedPublic || i.IsNestedFamily || i.IsNestedFamORAssem)
            .Select(MetadataProvider.GetMetadata<IInterfaceType>)
            .OrderBy(i => i.FullName, StringComparer.Ordinal);

        /// <summary>
        /// Gets the methods declared by the interface.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IMethod"/> objects representing the methods declared by the interface.</returns>
        protected virtual IEnumerable<IMethod> GetMethods() => Reflection
            .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName && !IsCompilerGeneratedMember(m))
            .Select(Assembly.Repository.GetMethodMetadata<IMethod>)
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ThenBy(m => m.Parameters.Count);

        /// <summary>
        /// Gets the properties declared by the interface.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IProperty"/> objects representing the properties declared by the interface.</returns>
        protected virtual IEnumerable<IProperty> GetProperties() => Reflection
            .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(p => !p.IsSpecialName)
            .Select(Assembly.Repository.GetPropertyMetadata)
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ThenBy(p => p.Parameters.Count);

        /// <summary>
        /// Gets the events declared by the interface.
        /// </summary>
        /// <returns>An enumeration of <see cref="IEvent"/> objects representing the events declared by the interface.</returns>
        protected virtual IEnumerable<IEvent> GetEvents() => Reflection
            .GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
            .Where(e => !e.IsSpecialName)
            .Select(Assembly.Repository.GetEventMetadata)
            .OrderBy(e => e.Name, StringComparer.Ordinal);

        /// <summary>
        /// Determines whether the specified member is compiler-generated.
        /// </summary>
        /// <param name="member">The member to check.</param>
        /// <returns><see langword="true"/> if the member is compiler-generated; otherwise, <see langword="false"/>.</returns>
        protected virtual bool IsCompilerGeneratedMember(MemberInfo member)
            => member.CustomAttributes.Any(attr => attr.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Adapters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;

    /// <summary>
    /// Provides extension methods to retrieve metadata abstraction objects from reflection objects.
    /// </summary>
    /// <remarks>
    /// This static class offers extension methods for <see cref="Assembly"/>, <see cref="Type"/>, and various member
    /// reflection types to obtain their corresponding metadata abstractions. It manages internal caching to ensure that
    /// each reflection object maps to a single metadata object instance, optimizing memory usage and performance.
    /// <para>
    /// The class supports assemblies loaded both by the Common Language Runtime (CLR) and through Metadata Load Context (MLC).
    /// It automatically selects the appropriate factory for creating metadata objects based on the assembly's origin.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true"/>
    public static class MetadataProvider
    {
        private static readonly ConditionalWeakTable<Assembly, IAssembly> assemblyCache = [];
        private static Func<Assembly, IAssembly> assemblyFactory = asm => new AssemblyAdapter(asm);

        /// <summary>
        /// Gets the assemblies for which metadata has been created.
        /// </summary>
        /// <value>
        /// An enumerable collection of assemblies with created metadata.
        /// </value>
        /// <remarks>
        /// This property provides access to the assemblies for which metadata has been requested through  this provider.
        /// Assemblies loaded via the Common Language Runtime (CLR) persist in the collection  for the lifetime of the
        /// application domain. However, assemblies loaded through Metadata Load Context (MLC) will be removed when the
        /// MLC is disposed.
        /// <para>
        /// The property uses deferred execution, so the collection reflects the current state at the time of enumeration.
        /// </para>
        /// </remarks>
        public static IEnumerable<IAssembly> AvailableAssemblies => assemblyCache.Select(static kv => kv.Value);

        /// <summary>
        /// Uses a custom factory for creating assembly metadata objects.
        /// </summary>
        /// <param name="factory">The factory function to create assembly metadata objects.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method replaces the default factory used to create <see cref="IAssembly"/> instances for assemblies.
        /// It allows for custom implementations of assembly metadata, such as for testing or specialized scenarios.
        /// <para>
        /// The change affects only assemblies for which metadata has not yet been created. Existing cached metadata remains unchanged.
        /// If this is not the intended behavior, consider clearing the cache using <see cref="ClearCache"/> before setting a new factory.
        /// </para>
        /// </remarks>
        public static void UseAssemblyFactory(Func<Assembly, IAssembly> factory)
        {
            if (factory is null)
                throw new ArgumentNullException(nameof(factory));

            Interlocked.Exchange(ref assemblyFactory, factory);
        }

        /// <summary>
        /// Registers all assemblies currently loaded in the application domain.
        /// </summary>
        /// <remarks>
        /// This method is primarily intended for testing purposes. In normal usage, assemblies are registered automatically
        /// when their metadata or the metadata of their types or members is first requested.
        /// <para>
        /// This method only registers assemblies loaded by the Common Language Runtime (CLR) for the current application
        /// domain. It does not include assemblies loaded through MetadataLoadContext (MLC).
        /// </para>
        /// </remarks>
        public static void RegisterRuntimeAssemblies()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                assemblyCache.GetValue(assembly, CreateAssemblyMetadata);
        }

        /// <summary>
        /// Clears the internal caches of created metadata objects.
        /// </summary>
        /// <remarks>
        /// This method is primarily intended for testing purposes. In normal usage, the caches are automatically
        /// managed and do not require manual clearing.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearCache() => assemblyCache.Clear();

        /// <summary>
        /// Retrieves metadata for an assembly.
        /// </summary>
        /// <param name="assembly">The assembly to get metadata for.</param>
        /// <returns>An assembly metadata abstraction.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> is <see langword="null"/>.</exception>
        public static IAssembly GetMetadata(this Assembly assembly)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            return assemblyCache.GetValue(assembly, CreateAssemblyMetadata);
        }

        /// <summary>
        /// Retrieves metadata for a type.
        /// </summary>
        /// <param name="type">The type to get metadata for.</param>
        /// <returns>A type metadata abstraction.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        public static IType GetMetadata(this Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return GetMetadata(type.Assembly).Repository.GetTypeMetadata(type);
        }

        /// <summary>
        /// Retrieves metadata for a type, cast to a specific type.
        /// </summary>
        /// <typeparam name="T">The specific type of metadata to retrieve.</typeparam>
        /// <param name="type">The type to get metadata for.</param>
        /// <returns>A type metadata abstraction of the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidCastException">Thrown when the retrieved metadata cannot be cast to type <typeparamref name="T"/>.</exception>
        public static T GetMetadata<T>(this Type type) where T : class, IType => (T)GetMetadata(type);

        /// <summary>
        /// Retrieves metadata for a constructor.
        /// </summary>
        /// <param name="constructorInfo">The constructor to get metadata for.</param>
        /// <returns>A constructor metadata abstraction.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
        public static IConstructor GetMetadata(this ConstructorInfo constructorInfo)
        {
            if (constructorInfo is null)
                throw new ArgumentNullException(nameof(constructorInfo));

            return GetMetadata(constructorInfo.DeclaringType!.Assembly).Repository.GetConstructorMetadata(constructorInfo);
        }

        /// <summary>
        /// Retrieves metadata for a method.
        /// </summary>
        /// <param name="methodInfo">The method to get metadata for.</param>
        /// <param name="asDeclared">Indicates whether to retrieve extension methods in their declared form rather than their usage form.</param>
        /// <returns>A method or operator metadata abstraction.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// When <paramref name="asDeclared"/> is <see langword="false"/> (the default), extension methods are detected and resolved
        /// to reflect their usage form rather than their declared form. Otherwise, the metadata is retrieved as declared, without
        /// extension method detection and resolution.
        /// </remarks>
        public static IMethodBase GetMetadata(this MethodInfo methodInfo, bool asDeclared = false)
        {
            if (methodInfo is null)
                throw new ArgumentNullException(nameof(methodInfo));

            return GetMetadata(methodInfo.DeclaringType!.Assembly).Repository.GetMethodMetadata(methodInfo, asDeclared);
        }

        /// <summary>
        /// Retrieves metadata for a property.
        /// </summary>
        /// <param name="propertyInfo">The property to get metadata for.</param>
        /// <returns>A property metadata abstraction.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
        public static IProperty GetMetadata(this PropertyInfo propertyInfo)
        {
            if (propertyInfo is null)
                throw new ArgumentNullException(nameof(propertyInfo));

            return GetMetadata(propertyInfo.DeclaringType!.Assembly).Repository.GetPropertyMetadata(propertyInfo);
        }

        /// <summary>
        /// Retrieves metadata for the specified event.
        /// </summary>
        /// <param name="eventInfo">The event to get metadata for.</param>
        /// <returns>An event metadata abstraction.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
        public static IEvent GetMetadata(this EventInfo eventInfo)
        {
            if (eventInfo is null)
                throw new ArgumentNullException(nameof(eventInfo));

            return GetMetadata(eventInfo.DeclaringType!.Assembly).Repository.GetEventMetadata(eventInfo);
        }

        /// <summary>
        /// Retrieves metadata for a field.
        /// </summary>
        /// <param name="fieldInfo">The field to get metadata for.</param>
        /// <returns>A field metadata abstraction.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
        public static IField GetMetadata(this FieldInfo fieldInfo)
        {
            if (fieldInfo is null)
                throw new ArgumentNullException(nameof(fieldInfo));

            return GetMetadata(fieldInfo.DeclaringType!.Assembly).Repository.GetFieldMetadata(fieldInfo);
        }

        /// <summary>
        /// Retrieves metadata for a member.
        /// </summary>
        /// <param name="memberInfo">The member to get metadata for.</param>
        /// <returns>A member metadata abstraction.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown the member's type is not supported.</exception>
        /// <remarks>
        /// The provided <paramref name="memberInfo"/> can represent any member type, including types, constructors, methods,
        /// properties, events, and fields. If the <paramref name="memberInfo"/> corresponds to an accessor of an extension
        /// property, the method will resolve and return the appropriate property metadata.
        /// </remarks>
        public static IMember GetMetadata(this MemberInfo memberInfo)
        {
            if (memberInfo is null)
                throw new ArgumentNullException(nameof(memberInfo));

            return GetMetadata(memberInfo.Module.Assembly).Repository.GetMemberMetadata(memberInfo);
        }

        /// <summary>
        /// Searches for type metadata by its full name across all loaded assemblies.
        /// </summary>
        /// <param name="fullName">The full name of the type to search for.</param>
        /// <returns>The type metadata if found; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fullName"/> is <see langword="null"/>.</exception>
        public static IType? FindTypeByFullName(string fullName)
        {
            if (fullName is null)
                throw new ArgumentNullException(nameof(fullName));

            var noAssemblies = true;
            foreach (var assembly in AvailableAssemblies)
            {
                if (assembly.TryGetType(fullName, out var type))
                    return type;

                noAssemblies = false;
            }

            // No assemblies registered yet - fallback to searching in mscorlib / System.Private.CoreLib
            if (noAssemblies && GetMetadata(typeof(object).Assembly).TryGetType(fullName, out var coreType))
                return coreType;

            return null;
        }

        /// <summary>
        /// Creates assembly metadata using the appropriate factory.
        /// </summary>
        /// <param name="assembly">The assembly to create metadata for.</param>
        /// <returns>An assembly metadata abstraction.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IAssembly CreateAssemblyMetadata(Assembly assembly) => assemblyFactory(assembly);
    }
}
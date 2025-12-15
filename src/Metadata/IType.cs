// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Define a contract for accessing type-specific metadata.
    /// </summary>
    /// <remarks>
    /// This interface abstracts common type metadata access, allowing consistent operations regardless of whether the type
    /// was loaded via Common Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    public interface IType : IMetadataAdapter<Type>, IMember, IEquatable<IType>
    {
        /// <summary>
        /// Gets the full name of the type.
        /// </summary>
        /// <value>
        /// The type's full name including namespace and declaring type names.
        /// </value>
        string FullName { get; }

        /// <summary>
        /// Gets the base type of this type.
        /// </summary>
        /// <value>
        /// The base type that this type directly inherits from, or <see langword="null"/> if the type has no base type.
        /// </value>
        IClassType? BaseType { get; }

        /// <summary>
        /// Gets a value indicating whether the type is nested.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type is nested; otherwise, <see langword="false"/>.
        /// </value>
        bool IsNested { get; }

        /// <summary>
        /// Gets a value indicating whether the type is a primitive type.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type is a primitive type; otherwise, <see langword="false"/>.
        /// </value>
        bool IsPrimitive { get; }

        /// <summary>
        /// Gets a value indicating whether the type is an interface.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type is an interface; otherwise, <see langword="false"/>.
        /// </value>
        bool IsInterface { get; }

        /// <summary>
        /// Gets a value indicating whether the type is an enumeration.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type is an enumeration; otherwise, <see langword="false"/>.
        /// </value>
        bool IsEnum { get; }

        /// <summary>
        /// Gets a value indicating whether the type is a value type.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type is a value type; otherwise, <see langword="false"/>.
        /// </value>
        bool IsValueType { get; }

        /// <summary>
        /// Gets a value indicating whether the type is a generic type.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type is a generic type; otherwise, <see langword="false"/>.
        /// </value>
        bool IsGenericType { get; }

        /// <summary>
        /// Gets the general signature of the type.
        /// </summary>
        /// <value>
        /// A string representation of the type used for constructing type's code references.
        /// </value>
        string Signature { get; }

        /// <summary>
        /// Gets the signature of the type when used as a parameter or return type.
        /// </summary>
        /// <value>
        /// A string representation of the type used for constructing parameter and return type signatures in code references.
        /// </value>
        /// <remarks>
        /// In most cases, the <see cref="ParametricSignature"/> is identical to the <see cref="Signature"/> property. However,
        /// for certain types such as generic types, this signature may differ to accurately represent how the type is used in
        /// parameter and return type contexts.
        /// </remarks>
        string ParametricSignature { get; }

        /// <summary>
        /// Gets the hierarchy of declaring types, starting from the outermost declaring type down to the immediate declaring type.
        /// </summary>
        /// <value>
        /// An enumerable collection of <see cref="IType"/> representing the hierarchy of declaring types from outermost to immediate.
        /// </value>
        /// <remarks>
        /// This property is implemented by using deferred execution. The immediate return value is an object that stores all the information
        /// that is required to perform the action.
        /// </remarks>
        IEnumerable<IType> DeclaringTypeHierarchy
        {
            get
            {
                if (DeclaringType is not null)
                {
                    foreach (var declaringType in DeclaringType.DeclaringTypeHierarchy)
                        yield return declaringType;

                    yield return DeclaringType;
                }
            }
        }

        /// <summary>
        /// Gets the hierarchy of base types, starting from the root base type down to the immediate base type.
        /// </summary>
        /// <value>
        /// An enumerable collection of <see cref="IClassType"/> representing the hierarchy of base types from root to immediate base."/>
        /// </value>
        /// <remarks>
        /// This property is implemented by using deferred execution. The immediate return value is an object that stores all the information
        /// that is required to perform the action.
        /// </remarks>
        IEnumerable<IClassType> BaseTypeHierarchy
        {
            get
            {
                if (BaseType is not null)
                {
                    foreach (var baseType in BaseType.BaseTypeHierarchy)
                        yield return baseType;

                    yield return BaseType;
                }
            }
        }

        /// <summary>
        /// Gets extension properties of the type.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IProperty"/> representing extension properties of the type.</returns>
        /// <remarks>
        /// This property is implemented by using deferred execution. The immediate return value is an object that stores all the information
        /// that is required to perform the action.
        /// </remarks>
        IEnumerable<IProperty> ExtensionProperties => MetadataProvider
            .AvailableAssemblies
            .Where(a => a.IsReflectionOnly || ReferenceEquals(a, Assembly))
            .SelectMany(a => a.ExtensionProperties)
            .Where(p => p.Extends(this));

        /// <summary>
        /// Gets extension methods of the type.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IMethod"/> representing extension properties of the type.</returns>
        /// <remarks>
        /// This property is implemented by using deferred execution. The immediate return value is an object that stores all the information
        /// that is required to perform the action.
        /// </remarks>
        IEnumerable<IMethod> ExtensionMethods => MetadataProvider
            .AvailableAssemblies
            .Where(a => a.IsReflectionOnly || ReferenceEquals(a, Assembly))
            .SelectMany(a => a.ExtensionMethods)
            .Where(m => m.Extends(this));

        /// <summary>
        /// Determines whether the instances of the specified type can be assigned to variables of the current type.
        /// </summary>
        /// <param name="source">The source type to check against the current type.</param>
        /// <returns><see langword="true"/> if instances of the specified type can be assigned to variables of the current type; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// The following conditions make the source type assignable to the current type:
        /// <list type="bullet">
        ///  <item>The source is the same type as the current type.</item>
        ///  <item>The source is a subclass of the current type, either directly or indirectly.</item>
        ///  <item>The source implements the current interface type.</item>
        ///  <item>The source is a value type that is assignable to the underlying type of the current nullable type.</item>
        /// </list>
        /// Any other combination of types is considered not assignable.
        /// </remarks>
        bool IsAssignableFrom(IType source);

        /// <summary>
        /// Determines whether the specified type can be used in place of the current type in parameter or return type positions.
        /// </summary>
        /// <param name="other">The type to check against the current type.</param>
        /// <returns><see langword="true"/> if the specified type is parametrically compatible with the current type; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method determines whether two types are compatible when used as parameter or return types in member signatures,
        /// such as in method overrides, interface implementations, or delegate compatibility checks.
        /// <para>
        /// The specified type is parametrically compatible with the current type if any of the following conditions are met:
        /// </para>
        /// <list type="bullet">
        ///   <item>
        ///     <term>Identical types</term>
        ///     <description>Both types represent the same type with identical signatures.</description>
        ///   </item>
        ///   <item>
        ///     <term>Decorated types with matching structure</term>
        ///     <description>
        ///     Both types are decorated types (array, pointer, by-ref, or nullable) with identical decoration. For arrays, dimensions and bounds must match
        ///     (e.g., both single-dimensional, or both multi-dimensional with the same rank). The underlying element types must also be parametrically compatible.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>Equivalent generic type parameters</term>
        ///     <description>
        ///     Both types are generic type parameters that represent the same parameter in the generic hierarchy. This includes:
        ///     <list type="bullet">
        ///       <item>Same parameter in the same generic member</item>
        ///       <item>Corresponding parameters in override/interface implementation relationships</item>
        ///       <item>Type parameters accessible through nesting</item>
        ///     </list>
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>Generic parameter satisfied by concrete type</term>
        ///     <description>
        ///     The current type is a generic type parameter, and the specified type is a concrete type (or constructed generic type) that satisfies
        ///     all of the parameter's constraints. This allows an open generic type like <c>List&lt;T&gt;</c> to be satisfied by a closed type
        ///     like <c>List&lt;int&gt;</c> when <c>T</c>'s constraints are met by <c>int</c>.
        ///     </description>
        ///   </item>
        /// </list>
        /// For all other type combinations, the types are considered not parametrically compatible.
        /// <note type="important" title="Important">
        /// This method enforces strict parametric compatibility for signature matching and does not consider variance or assignability.
        /// Use <see cref="IsAssignableFrom(IType)"/> for inheritance-based compatibility checks.
        /// </note>
        /// </remarks>
        /// <seealso cref="IsAssignableFrom(IType)"/>
        bool IsSubstitutableBy(IType other);

        /// <summary>
        /// Resolves a type member based on the specified XML documentation cref string.
        /// </summary>
        /// <param name="cref">The code reference that identifies the member to resolve.</param>
        /// <returns>An <see cref="IMember"/> representing the resolved member, or <see langword="null"/> if no matching member is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cref"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method resolves members declared directly on this type, such as methods, properties, fields, and events.
        /// <note type="hint" title="Hint">
        /// For comprehensive member resolution across assemblies, use <see cref="Support.CodeReference.ResolveMember(string)"/> instead.
        /// </note>
        /// </remarks>
        /// <seealso cref="Support.CodeReference.ResolveMember(string)"/>
        IMember? ResolveMember(string cref);
    }
}

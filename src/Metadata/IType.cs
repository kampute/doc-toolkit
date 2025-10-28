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
        /// Gets the signature of the type.
        /// </summary>
        /// <value>
        /// A string representation of the type used for constructing parameter and return type signatures.
        /// </value>
        string Signature { get; }

        /// <summary>
        /// Gets the hierarchy of declaring types, starting from the outermost declaring type down to the immediate declaring type.
        /// </summary>
        /// <value>
        /// An enumerable collection of declaring types from outermost to immediate.
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
        /// An enumerable collection of base types in the hierarchy from root to immediate base.
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
        /// Gets all extension methods applicable to the type in the containing assembly.
        /// </summary>
        /// <returns>An enumerable collection of extension methods applicable to the type.</returns>
        /// <remarks>
        /// This property is implemented by using deferred execution. The immediate return value is an object that stores all the information
        /// that is required to perform the action.
        /// </remarks>
        IEnumerable<IMethod> ExtensionMethods => Assembly.ExportedTypes
            .Where(static t => t.IsStatic && !t.IsNested).OfType<IClassType>()
            .SelectMany(t => t.Methods.Where(m => m.IsExtensionMethodFor(this)));

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
        ///  <item>The source is a value type that is assignable to the underlaying type of the current nullable type.</item>
        /// </list>
        /// Any other combination of types is considered not assignable.
        /// </remarks>
        bool IsAssignableFrom(IType source);

        /// <summary>
        /// Determines whether the specified type as a parameter type can be substituted for the current type.
        /// </summary>
        /// <param name="other">The type to check against the current type.</param>
        /// <returns><see langword="true"/> if the specified type is substitutable for the current type; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method is used to determine if two members have compatible signatures, such as when checking for method overrides or interface implementations.
        /// <para>
        /// The following conditions make the other type substitutable for the current type:
        /// <list type="bullet">
        ///   <item>The current type and the other type represent the same type.</item>
        ///   <item>Both types are a decorated type (e.g., array, pointer, by-ref, or nullable) with identical modifiers and their element types are also satisfiable.</item>
        ///   <item>Both types are generic type parameters with identical constraints and declaring member ancestry.</item>
        ///   <item>The current type is a generic type parameter, and the other type is a type that satisfies all of its constraints.</item>
        /// </list>
        /// For all other cases, the current type is not considered substitutable by the other type.
        /// </para>
        /// </remarks>
        bool IsSubstitutableBy(IType other);
    }
}

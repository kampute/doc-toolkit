// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System;
    using System.Reflection;

    /// <summary>
    /// Defines a contract for accessing parameter metadata.
    /// </summary>
    public interface IParameter : IMetadataAdapter<ParameterInfo>, IWithCustomAttributes, IWithCustomModifiers
    {
        /// <summary>
        /// Gets the member that this parameter belongs to.
        /// </summary>
        /// <value>
        /// An <see cref="IMember"/> representing the member that this parameter belongs to.
        /// </value>
        IMember Member { get; }

        /// <summary>
        /// Gets the position of the parameter in the parameter list.
        /// </summary>
        /// <value>
        /// The zero-based position of the parameter in the parameter list, or -1 if the parameter is a return parameter.
        /// </value>
        int Position { get; }

        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        /// <value>
        /// The parameter's type metadata.
        /// </value>
        IType Type { get; }

        /// <summary>
        /// Gets the reference type of the parameter if it is passed by reference.
        /// </summary>
        /// <value>
        /// A <see cref="ParameterReferenceKind"/> value that indicates how the parameter is passed by reference.
        /// </value>
        ParameterReferenceKind ReferenceKind { get; }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <value>
        /// The name of the parameter or an empty string if the parameter is a return parameter.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Gets a value indicating whether the parameter is a return parameter.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the parameter is a return parameter; otherwise, <see langword="false"/>.
        /// </value>
        bool IsReturnParameter { get; }

        /// <summary>
        /// Gets a value indicating whether the parameter is passed by reference.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the parameter is passed by reference; otherwise, <see langword="false"/>.
        /// </value>
        /// <seealso cref="ReferenceKind"/>
        bool IsByRef { get; }

        /// <summary>
        /// Gets a value indicating whether the parameter is a parameter array.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the parameter is a parameter array; otherwise, <see langword="false"/>.
        /// </value>
        bool IsParameterArray { get; }

        /// <summary>
        /// Gets a value indicating whether the parameter is optional.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the parameter is optional; otherwise, <see langword="false"/>.
        /// </value>
        bool IsOptional { get; }

        /// <summary>
        /// Gets a value indicating whether the parameter has a default value.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the parameter has a default value; otherwise, <see langword="false"/>.
        /// </value>
        bool HasDefaultValue { get; }

        /// <summary>
        /// Gets the default value of the parameter.
        /// </summary>
        /// <value>
        /// The default value, or <see cref="DBNull.Value"/> if no default value is specified.
        /// </value>
        object? DefaultValue { get; }

        /// <summary>
        /// Determines whether the specified parameter can be used in place of the current parameter.
        /// </summary>
        /// <param name="other">The parameter to compare against the current parameter.</param>
        /// <returns>
        /// <see langword="true"/> if the specified parameter can be used in place of the current parameter; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method evaluates type compatibility between parameters for scenarios such as method overrides, interface implementations, 
        /// and overload resolution. The specified parameter can satisfy the current parameter if they have the same position and reference 
        /// kind, and their types meet one of the following criteria:
        /// <list type="bullet">
        ///   <item>Both types are identical.</item>
        ///   <item>Both types are decorated types (array, pointer, by-ref, or nullable) with identical modifiers and compatible element types.</item>
        ///   <item>Both types are generic type parameters with identical constraints and declaring member ancestry.</item>
        ///   <item>The current type is an open generic type parameter, and the specified type satisfies all of its constraints.</item>
        /// </list>
        /// For return parameters of non-interface members, to support covariant return types in method overrides, the specified parameter can also 
        /// satisfy the current parameter if any of the following conditions are met:
        /// <list type="bullet">
        ///   <item>The specified parameter's type is a subclass of the current parameter's type.</item>
        ///   <item>The specified parameter's type implements the current parameter's interface type.</item>
        /// </list>
        /// <note type="caution" title="Important">
        /// This method does not verify that the parameters belong to compatible members or contexts. The caller must ensure that the parameters 
        /// are from related members, such as in method inheritance hierarchies.
        /// </note>
        /// </remarks>
        bool IsSatisfiableBy(IParameter other);
    }
}

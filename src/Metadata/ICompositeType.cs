// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Defines a contract for accessing metadata for types that are composite types.
    /// </summary>
    /// <remarks>
    /// This interface provides access to metadata specific to composite types (classes and structs).
    /// </remarks>
    public interface ICompositeType : IGenericCapableType, IInterfaceCapableType,
        IWithConstructors, IWithFields, IWithProperties, IWithMethods, IWithEvents, IWithOperators, IWithNestedTypes, IWithExplicitInterfaceMembers
    {
        /// <summary>
        /// Gets a value indicating whether the type is declared as a record class or record struct.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the type is a record class or record struct; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// Metadata does not mark a type as a record. A record is recognized by its compiler-generated equality operator, which the
        /// C# compiler synthesizes for every record class and record struct and which source code cannot declare in a record.
        /// </remarks>
        bool IsRecord => Operators.Any(static op => op.IsCompilerGenerated && op.MethodName == "op_Equality");

        /// <summary>
        /// Gets the primary constructor of the type if it is a positional record.
        /// </summary>
        /// <value>
        /// The constructor whose parameters form the parameter list of the record declaration, or <see langword="null"/> if the type is
        /// not a positional record or its primary constructor cannot be identified.
        /// </value>
        /// <remarks>
        /// Metadata does not mark a constructor as primary. The primary constructor is identified as the user-declared constructor whose
        /// parameters match, by position, name, and type, the parameters of the <c>Deconstruct</c> method that the compiler synthesizes
        /// for a positional record. Therefore, this property is <see langword="null"/> for a record without a parameter list, and for a
        /// positional record whose <c>Deconstruct</c> method is declared in source code.
        /// </remarks>
        IConstructor? PrimaryConstructor
        {
            get
            {
                var deconstruct = Methods.FirstOrDefault(static m => m.IsCompilerGenerated && m.Name == "Deconstruct");
                if (deconstruct is null)
                    return null;

                return Constructors.FirstOrDefault(c => !c.IsCompilerGenerated && HaveMatchingParameters(c.Parameters, deconstruct.Parameters));

                static bool HaveMatchingParameters(IReadOnlyList<IParameter> constructorParameters, IReadOnlyList<IParameter> deconstructParameters)
                {
                    if (constructorParameters.Count != deconstructParameters.Count)
                        return false;

                    for (var i = 0; i < constructorParameters.Count; ++i)
                    {
                        var constructorParameter = constructorParameters[i];
                        var deconstructParameter = deconstructParameters[i];
                        var deconstructParameterType = deconstructParameter.Type is ITypeDecorator { Modifier: TypeModifier.ByRef } byRefType
                            ? byRefType.ElementType
                            : deconstructParameter.Type;

                        if (constructorParameter.Name != deconstructParameter.Name || !constructorParameter.Type.Equals(deconstructParameterType))
                            return false;
                    }

                    return true;
                }
            }
        }
    }
}

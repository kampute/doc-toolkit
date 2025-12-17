// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Languages
{
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public partial class CSharp
    {
        /// <summary>
        /// Writes the definition of the specified type to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="type">The type whose definition is to be written.</param>
        /// <param name="linker">A delegate for linking to the documentation of a type or type's member.</param>
        private void WriteTypeDefinition(TextWriter writer, IType type, MemberDocLinker linker)
        {
            var attributes = type.ExplicitCustomAttributes;
            if (type is IDelegateType delType)
                attributes = attributes.Concat(delType.Return.ExplicitCustomAttributes);

            WriteAttributes(writer, attributes, linker);
            WriteVisibilityModifiers(writer, type);
            switch (type)
            {
                case IClassType classType:
                    if (type.IsStatic)
                        writer.Write("static ");
                    else if (classType.IsSealed)
                        writer.Write("sealed ");
                    else if (classType.IsAbstract)
                        writer.Write("abstract ");
                    writer.Write("class ");
                    WriteTypeSignature(writer, classType, NameQualifier.DeclaringType, linker, declarative: true);
                    WriteTypeInheritance(writer, classType, linker);
                    if (classType.IsGenericTypeDefinition)
                        WriteGenericConstraints(writer, classType.TypeParameters, linker);
                    break;
                case IStructType structType:
                    if (structType.IsReadOnly)
                        writer.Write("readonly ");
                    if (structType.IsRefLike)
                        writer.Write("ref ");
                    writer.Write("struct ");
                    WriteTypeSignature(writer, structType, NameQualifier.DeclaringType, linker, declarative: true);
                    WriteTypeInheritance(writer, structType, linker);
                    if (structType.IsGenericTypeDefinition)
                        WriteGenericConstraints(writer, structType.TypeParameters, linker);
                    break;
                case IInterfaceType interfaceType:
                    writer.Write("interface ");
                    WriteTypeSignature(writer, type, NameQualifier.DeclaringType, linker, declarative: true);
                    WriteTypeInheritance(writer, type, linker);
                    if (interfaceType.IsGenericTypeDefinition)
                        WriteGenericConstraints(writer, interfaceType.TypeParameters, linker);
                    break;
                case IEnumType enumType:
                    writer.Write("enum ");
                    WriteTypeSignature(writer, enumType, NameQualifier.DeclaringType, linker, declarative: true);
                    break;
                case IDelegateType delegateType:
                    writer.Write("delegate ");
                    WriteTypeSignature(writer, delegateType.Return.Type, Options.GlobalNameQualifier, linker);
                    writer.Write(' ');
                    WriteTypeSignature(writer, delegateType, NameQualifier.DeclaringType, linker, declarative: true);
                    writer.Write('(');
                    WriteParameters(writer, delegateType.Parameters, linker, declarative: true);
                    writer.Write(')');
                    break;
                default:
                    WriteTypeSignature(writer, type, NameQualifier.DeclaringType, linker, declarative: true);
                    break;
            }
        }

        /// <summary>
        /// Writes the name of the specified type to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="type">The <see cref="Type"/> whose signature is to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the type's name.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        /// <param name="declarative">If <see langword="true"/>, include variance attributes of the type arguments.</param>
        private void WriteTypeSignature(TextWriter writer, IType type, NameQualifier qualifier, MemberDocLinker linker, bool declarative = false)
        {
            if (TryGetSimplifiedName(type, out var simplifiedName))
            {
                var name = Options.SimplifySystemTypeNames
                    ? simplifiedName
                    : qualifier == NameQualifier.Full
                        ? type.FullName
                        : type.Name;

                linker(writer, type, name);
                return;
            }

            switch (type)
            {
                case ITypeParameter typeParameter:
                    WriteTypeParameter(writer, typeParameter, linker, declarative);
                    return;
                case ITypeDecorator typeDecorator:
                    WriteTypeDecorator(writer, typeDecorator, qualifier, linker, declarative);
                    return;
                case IGenericCapableType genericType:
                    WriteGenericTypeSignature(writer, genericType, qualifier, linker, declarative);
                    return;
            }

            if (qualifier != NameQualifier.None && type.IsNested)
            {
                WriteTypeSignature(writer, type.DeclaringType!, qualifier, linker, declarative);
                writer.Write(Type.Delimiter);
            }
            else if (qualifier == NameQualifier.Full && !string.IsNullOrEmpty(type.Namespace))
            {
                writer.Write(type.Namespace);
                writer.Write(Type.Delimiter);
            }

            linker(writer, type, type.Name);
        }

        /// <summary>
        /// Writes the signature of a generic type to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="type">The <see cref="IGenericCapableType"/> whose signature is to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the type's name.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        /// <param name="declarative">If <see langword="true"/>, include variance attributes of the type arguments.</param>
        /// <param name="typeArguments">Optional list of type arguments to use instead of the type's own arguments.</param>
        private void WriteGenericTypeSignature
        (
            TextWriter writer,
            IGenericCapableType type,
            NameQualifier qualifier,
            MemberDocLinker linker,
            bool declarative,
            IReadOnlyList<IType>? typeArguments = null
        )
        {
            typeArguments ??= type.IsGenericTypeDefinition
                ? type.TypeParameters
                : type.TypeArguments;

            if (qualifier != NameQualifier.None && type.IsNested)
            {
                WriteGenericTypeSignature(writer, (IGenericCapableType)type.DeclaringType!, qualifier, linker, declarative, typeArguments);
                writer.Write(Type.Delimiter);
            }
            else if (qualifier == NameQualifier.Full && !string.IsNullOrEmpty(type.Namespace))
            {
                writer.Write(type.Namespace);
                writer.Write(Type.Delimiter);
            }

            if (type.IsGenericType)
            {
                linker(writer, type.GenericTypeDefinition ?? type, type.SimpleName);
                var (offset, count) = type.OwnGenericParameterRange;
                if (count > 0)
                {
                    writer.Write('<');
                    for (var i = 0; i < count; i++)
                    {
                        if (i > 0)
                            writer.Write(", ");

                        WriteTypeSignature(writer, typeArguments[offset + i], Options.GlobalNameQualifier, linker, declarative);
                    }
                    writer.Write('>');
                }
            }
            else
            {
                linker(writer, type, type.Name);
            }
        }

        /// <summary>
        /// Writes the signature of a type decorator to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="typeDecorator">The <see cref="ITypeDecorator"/> whose signature is to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the type's name.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        /// <param name="declarative">If <see langword="true"/>, include variance attributes of the type arguments.</param>
        private void WriteTypeDecorator(TextWriter writer, ITypeDecorator typeDecorator, NameQualifier qualifier, MemberDocLinker linker, bool declarative)
        {
            var arrayModifierIndex = 0;
            var modifiers = new List<string>();
            var type = typeDecorator.Unwrap(decorator =>
            {
                switch (decorator.Modifier)
                {
                    case TypeModifier.Array:
                        modifiers.Insert(arrayModifierIndex, decorator.ArrayRank > 1
                            ? $"[{new string(',', decorator.ArrayRank - 1)}]"
                            : "[]");
                        break;
                    case TypeModifier.Nullable:
                        modifiers.Add("?");
                        arrayModifierIndex = modifiers.Count;
                        break;
                    case TypeModifier.Pointer:
                        modifiers.Add("*");
                        arrayModifierIndex = modifiers.Count;
                        break;
                    case TypeModifier.ByRef:
                        if (declarative)
                            writer.Write("ref ");
                        break;
                }
            });

            WriteTypeSignature(writer, type, qualifier, linker, declarative);

            for (var i = modifiers.Count - 1; i >= 0; --i)
                writer.Write(modifiers[i]);
        }

        /// <summary>
        /// Writes the name of a type parameter to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="typeParameter">The <see cref="ITypeParameter"/> whose signature is to be written.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        /// <param name="declarative">If <see langword="true"/>, include variance attributes of the type parameter.</param>
        private void WriteTypeParameter(TextWriter writer, ITypeParameter typeParameter, MemberDocLinker linker, bool declarative)
        {
            if (declarative)
            {
                if (WriteAttributes(writer, typeParameter.ExplicitCustomAttributes, linker, singleLine: true))
                    writer.Write(' ');

                switch (typeParameter.Variance)
                {
                    case TypeParameterVariance.Covariant:
                        writer.Write("out ");
                        break;
                    case TypeParameterVariance.Contravariant:
                        writer.Write("in ");
                        break;
                }
            }

            writer.Write(typeParameter.Name);
        }

        /// <summary>
        /// Writes the specified type parameters to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="typeParameters">The list of <see cref="ITypeParameter"/> whose signatures are to be written.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        /// <param name="declarative">If <see langword="true"/>, include variance attributes of the type parameters.</param>
        private void WriteGenericParameters(TextWriter writer, IReadOnlyList<ITypeParameter> typeParameters, MemberDocLinker linker, bool declarative)
        {
            if (typeParameters.Count == 0)
                return;

            writer.Write('<');
            for (var i = 0; i < typeParameters.Count; i++)
            {
                if (i > 0)
                    writer.Write(", ");

                WriteTypeParameter(writer, typeParameters[i], linker, declarative);
            }
            writer.Write('>');
        }

        /// <summary>
        /// Writes the constraints of a type parameter to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="typeParameter">The <see cref="ITypeParameter"/> whose constraints are to be written.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        private void WriteTypeParameterConstraints(TextWriter writer, ITypeParameter typeParameter, MemberDocLinker linker)
        {
            if (!typeParameter.HasConstraints)
                return;

            writer.Write("where ");
            writer.Write(typeParameter.Name);
            writer.Write(" : ");

            var needsCommaSeparation = false;

            if (typeParameter.Constraints.HasFlag(TypeParameterConstraints.NotNullableValueType))
            {
                EnsureCommaSeparation();
                writer.Write("struct");
            }
            else if (typeParameter.Constraints.HasFlag(TypeParameterConstraints.ReferenceType))
            {
                EnsureCommaSeparation();
                writer.Write("class");
            }

            foreach (var typeConstraint in typeParameter.TypeConstraints)
            {
                if (typeConstraint.FullName == "System.ValueType")
                    continue; // 'struct' constraint already covers this.

                EnsureCommaSeparation();
                WriteTypeSignature(writer, typeConstraint, Options.GlobalNameQualifier, linker);
            }

            if (typeParameter.Constraints.HasFlag(TypeParameterConstraints.AllowByRefLike))
            {
                EnsureCommaSeparation();
                writer.Write("allows ref struct");
            }
            else if (typeParameter.Constraints.HasFlag(TypeParameterConstraints.DefaultConstructor)
                 && !typeParameter.Constraints.HasFlag(TypeParameterConstraints.NotNullableValueType))
            {
                EnsureCommaSeparation();
                writer.Write("new()");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void EnsureCommaSeparation()
            {
                if (needsCommaSeparation)
                    writer.Write(", ");
                else
                    needsCommaSeparation = true;
            }
        }

        /// <summary>
        /// Writes the constraints of multiple type parameters to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="typeParameters">The collection of <see cref="ITypeParameter"/> whose constraints are to be written.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        private void WriteGenericConstraints(TextWriter writer, IEnumerable<ITypeParameter> typeParameters, MemberDocLinker linker)
        {
            foreach (var typeParameter in typeParameters)
            {
                if (!typeParameter.HasConstraints)
                    continue;

                writer.WriteLine();
                writer.Write('\t');
                WriteTypeParameterConstraints(writer, typeParameter, linker);
            }
        }

        /// <summary>
        /// Writes the base type and interfaces of a type to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="type">The <see cref="IType"/> whose base type and interfaces are to be written.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        private void WriteTypeInheritance(TextWriter writer, IType type, MemberDocLinker linker)
        {
            var needsCommaSeparation = false;
            if (type.BaseType is not null && type.BaseType.FullName is not ("System.Object" or "System.ValueType"))
            {
                writer.Write(" : ");
                WriteTypeSignature(writer, type.BaseType!, Options.GlobalNameQualifier, linker);
                needsCommaSeparation = true;
            }

            if (type is not IInterfaceCapableType interfaceCapableType)
                return;

            foreach (var implementedInterface in interfaceCapableType.ImplementedInterfaces)
            {
                writer.Write(needsCommaSeparation ? ", " : " : ");
                WriteTypeSignature(writer, implementedInterface, Options.GlobalNameQualifier, linker);
                needsCommaSeparation = true;
            }
        }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Languages
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System;
    using System.IO;
    using System.Linq;

    public partial class CSharp
    {
        /// <summary>
        /// Writes the name of the specified type member to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="member">The type member whose name is to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the member's name.</param>
        /// <param name="linker">A delegate for linking to the documentation of a type or type's member.</param>
        /// <param name="indexerName">The name to use for indexers if <paramref name="member"/> is an indexer property; otherwise, <see langword="null"/>.</param>
        /// <remarks>
        /// For explicit interface implementations, when <paramref name="qualifier"/> is <see cref="NameQualifier.None"/>, the interface
        /// member name is qualified according to <see cref="CodeStyleOptions.FullyQualifyExplicitInterfaceMemberNames"/>. When
        /// <paramref name="qualifier"/> is not <see cref="NameQualifier.None"/>, the implementing type is written first according to
        /// the specified <paramref name="qualifier"/>, followed by the fully qualified interface member name.
        /// <para>
        /// Constructors have no name written, and indexer properties use the provided <paramref name="indexerName"/> if specified.
        /// </para>
        /// </remarks>
        private void WriteTypeMemberName(TextWriter writer, ITypeMember member, NameQualifier qualifier, MemberDocLinker linker, string? indexerName = null)
        {
            if (member is IVirtualTypeMember { IsExplicitInterfaceImplementation: true, ImplementedMember: ITypeMember interfaceMember })
            {
                if (qualifier != NameQualifier.None)
                {
                    WriteTypeSignature(writer, member.DeclaringType, qualifier, linker);
                    writer.Write(Type.Delimiter);
                    qualifier = NameQualifier.Full;
                }
                else
                {
                    qualifier = Options.FullyQualifyExplicitInterfaceMemberNames
                        ? NameQualifier.Full
                        : NameQualifier.DeclaringType;
                }

                member = interfaceMember;
            }

            var name = member switch
            {
                IConstructor => string.Empty,
                IProperty p when p.IsIndexer && !string.IsNullOrEmpty(indexerName) => indexerName,
                _ => member.Name
            };

            if (qualifier != NameQualifier.None)
            {
                WriteTypeSignature(writer, member.DeclaringType, qualifier, linker);
                if (string.IsNullOrEmpty(name))
                    return;

                writer.Write(Type.Delimiter);
            }

            if (member is IWithExtensionBehavior { ExtensionBlock: IExtensionBlock extensionBlock })
            {
                WriteExtensionReceiver(writer, extensionBlock, linker);
                writer.Write(Type.Delimiter);
            }

            linker(writer, member, name);
        }

        /// <summary>
        /// Writes the extension receiver parameter to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="extensionBlock">The extension block whose receiver is to be written.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        private void WriteExtensionReceiver(TextWriter writer, IExtensionBlock extensionBlock, MemberDocLinker linker)
        {
            writer.Write("extension");
            if (extensionBlock.IsGenericBlock)
                WriteGenericParameters(writer, extensionBlock.TypeParameters, linker, declarative: false);

            writer.Write('(');
            WriteParameter(writer, extensionBlock.Receiver, linker, declarative: false);
            writer.Write(')');
        }

        #region Constructors

        /// <summary>
        /// Writes the definition of the specified constructor to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="constructor">The constructor whose definition is to be written.</param>
        /// <param name="linker">A delegate for linking to the documentation of a type or type's member.</param>
        private void WriteConstructorDefinition(TextWriter writer, IConstructor constructor, MemberDocLinker linker)
        {
            WriteAttributes(writer, constructor.ExplicitCustomAttributes, linker);
            WriteConstructorModifiers(writer, constructor);
            WriteConstructorSignature(writer, constructor, NameQualifier.None, linker, declarative: true);
        }

        /// <summary>
        /// Writes the signature of the specified constructor to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="constructor">The constructor whose signature is to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the member's name.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        /// <param name="declarative"><see langword="true"/> to include the parameter names; otherwise, <see langword="false"/>.</param>
        private void WriteConstructorSignature(TextWriter writer, IConstructor constructor, NameQualifier qualifier, MemberDocLinker linker, bool declarative)
        {
            WriteTypeSignature(writer, constructor.DeclaringType, qualifier, linker);

            writer.Write('(');
            WriteParameters(writer, constructor.Parameters, linker, declarative);
            writer.Write(')');
        }

        /// <summary>
        /// Writes the modifiers of the specified constructor to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the constructor modifiers are written.</param>
        /// <param name="constructor">The constructor whose modifiers are to be written.</param>
        private static void WriteConstructorModifiers(TextWriter writer, IConstructor constructor)
        {
            if (constructor.IsStatic)
                writer.Write("static ");
            else
                WriteVisibilityModifiers(writer, constructor);
        }

        #endregion

        #region Fields

        /// <summary>
        /// Writes the definition of the specified field to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="field">The field whose definition is to be retrieved.</param>
        /// <param name="linker">A delegate for linking to the documentation of a type or type's member.</param>
        private void WriteFieldDefinition(TextWriter writer, IField field, MemberDocLinker linker)
        {
            WriteAttributes(writer, field.ExplicitCustomAttributes, linker);

            WriteFieldModifiers(writer, field);

            if (field.TryGetFixedSizeBufferInfo(out var elementType, out var length))
            {
                writer.Write("fixed ");
                WriteTypeSignature(writer, elementType, Options.GlobalNameQualifier, linker);
                writer.Write(' ');
                linker(writer, field, field.Name);
                writer.Write('[');
                writer.Write(length);
                writer.Write(']');
            }
            else
            {
                WriteTypeSignature(writer, field.Type, Options.GlobalNameQualifier, linker);
                writer.Write(' ');
                linker(writer, field, field.Name);
                if (field.IsLiteral)
                {
                    writer.Write(" = ");
                    WriteConstantValue(writer, field.LiteralValue, field.Type, linker);
                }
            }
        }

        /// <summary>
        /// Writes the signature of the specified field to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="field">The field whose signature is to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the member's name.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        private void WriteFieldSignature(TextWriter writer, IField field, NameQualifier qualifier, MemberDocLinker linker)
        {
            WriteTypeMemberName(writer, field, qualifier, linker);
        }

        /// <summary>
        /// Writes the modifiers of the specified field to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the field modifiers are written.</param>
        /// <param name="field">The field whose modifiers are to be written.</param>
        private static void WriteFieldModifiers(TextWriter writer, IField field)
        {
            if (field.DeclaringType is IEnumType)
                return;

            WriteVisibilityModifiers(writer, field);

            if (field.IsLiteral)
            {
                writer.Write("const ");
                return;
            }

            if (field.IsStatic)
                writer.Write("static ");

            if (field.IsUnsafe)
                writer.Write("unsafe ");

            if (field.IsReadOnly)
                writer.Write("readonly ");
            else if (field.IsVolatile)
                writer.Write("volatile ");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Writes the definition of the specified property to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="property">The property whose definition is to be written.</param>
        /// <param name="linker">A delegate for linking to the documentation of a type or type's member.</param>
        private void WritePropertyDefinition(TextWriter writer, IProperty property, MemberDocLinker linker)
        {
            WriteAttributes(writer, property.ExplicitCustomAttributes, linker);

            WritePropertyModifiers(writer, property);

            WriteTypeSignature(writer, property.Type, Options.GlobalNameQualifier, linker);
            writer.Write(' ');
            WritePropertySignature(writer, property, NameQualifier.None, linker, declarative: true);

            writer.Write(" { ");
            if (property.GetMethod is IMethod getter)
            {
                if (getter.Visibility != property.Visibility)
                    WriteVisibilityModifiers(writer, getter);

                writer.Write("get; ");
            }
            if (property.SetMethod is IMethod { IsVisible: true } setter)
            {
                if (setter.Visibility != property.Visibility)
                    WriteVisibilityModifiers(writer, setter);

                writer.Write(property.IsInitOnly ? "init; " : "set; ");
            }
            writer.Write('}');

            if (property.ExtensionBlock is IExtensionBlock { IsGenericBlock: true } extBlock)
                WriteGenericConstraints(writer, extBlock.TypeParameters, linker);
        }

        /// <summary>
        /// Writes the signature of the specified property to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="property">The property whose signature is to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the member's name.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        /// <param name="declarative"><see langword="true"/> to include the index parameter names; otherwise, <see langword="false"/>.</param>
        private void WritePropertySignature(TextWriter writer, IProperty property, NameQualifier qualifier, MemberDocLinker linker, bool declarative = false)
        {
            WriteTypeMemberName(writer, property, qualifier, linker, indexerName: declarative ? "this" : default);

            if (property.IsIndexer)
            {
                writer.Write('[');
                WriteParameters(writer, property.Parameters, linker, declarative);
                writer.Write(']');
            }
        }

        /// <summary>
        /// Writes the modifiers of the specified property to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the property modifiers are written.</param>
        /// <param name="property">The property whose modifiers are to be written.</param>
        private static void WritePropertyModifiers(TextWriter writer, IProperty property)
        {
            WriteVisibilityModifiers(writer, property);

            if (property.IsStatic)
                writer.Write("static ");
            if (property.IsUnsafe)
                writer.Write("unsafe ");
            if (property.IsRequired)
                writer.Write("required ");
            if (property.IsReadOnly)
                writer.Write("readonly ");

            WriteVirtualityModifiers(writer, property);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Writes the definition of the specified method to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="method">The method whose definition is to be written.</param>
        /// <param name="linker">A delegate for linking to the documentation of a type or type's member.</param>
        private void WriteMethodDefinition(TextWriter writer, IMethod method, MemberDocLinker linker)
        {
            var attributes = method.ExplicitCustomAttributes.Concat(method.Return.ExplicitCustomAttributes);
            WriteAttributes(writer, attributes, linker);

            WriteMethodModifiers(writer, method);

            WriteTypeSignature(writer, method.Return.Type, Options.GlobalNameQualifier, linker);
            writer.Write(' ');
            WriteTypeMemberName(writer, method, NameQualifier.None, linker);
            WriteGenericParameters(writer, method.TypeParameters, linker, declarative: true);

            writer.Write('(');
            if (method.IsClassicExtensionMethod)
                writer.Write("this ");
            WriteParameters(writer, method.Parameters, linker, declarative: true);
            writer.Write(')');

            if (method.ExtensionBlock is IExtensionBlock { IsGenericBlock: true } extBlock)
                WriteGenericConstraints(writer, extBlock.TypeParameters, linker);
            if (method.IsGenericMethod)
                WriteGenericConstraints(writer, method.TypeParameters, linker);
        }

        /// <summary>
        /// Writes the signature of the specified method to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="method">The method whose signature is to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the member's name.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        /// <param name="declarative"><see langword="true"/> to include the parameter names; otherwise, <see langword="false"/>.</param>
        private void WriteMethodSignature(TextWriter writer, IMethod method, NameQualifier qualifier, MemberDocLinker linker, bool declarative = false)
        {
            WriteTypeMemberName(writer, method, qualifier, linker);

            if (method.IsGenericMethod)
                WriteGenericParameters(writer, method.TypeParameters, linker, declarative);

            writer.Write('(');
            if (method.IsClassicExtensionMethod)
                writer.Write("this ");
            WriteParameters(writer, method.Parameters, linker, declarative);
            writer.Write(')');
        }

        /// <summary>
        /// Writes the modifiers of the specified method to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the method modifiers are written.</param>
        /// <param name="method">The method whose modifiers are to be written.</param>
        private static void WriteMethodModifiers(TextWriter writer, IMethod method)
        {
            WriteVisibilityModifiers(writer, method);

            if (method.IsStatic)
                writer.Write("static ");
            if (method.IsUnsafe)
                writer.Write("unsafe ");
            if (method.IsReadOnly)
                writer.Write("readonly ");

            WriteVirtualityModifiers(writer, method);
        }

        #endregion

        #region Events

        /// <summary>
        /// Writes the definition of the specified event to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="evt">The event whose definition is to be written.</param>
        /// <param name="linker">A delegate for linking to the documentation of a   type or type's member.</param>
        private void WriteEventDefinition(TextWriter writer, IEvent evt, MemberDocLinker linker)
        {
            WriteAttributes(writer, evt.ExplicitCustomAttributes, linker);
            WriteEventModifiers(writer, evt);
            writer.Write("event ");
            WriteTypeSignature(writer, evt.Type, Options.GlobalNameQualifier, linker);
            writer.Write(' ');
            WriteTypeMemberName(writer, evt, NameQualifier.None, linker);
            if (evt.AddMethod.Visibility != evt.RemoveMethod.Visibility)
            {
                writer.Write(" { ");
                if (evt.AddMethod is IMethod adder)
                {
                    if (adder.Visibility != evt.Visibility)
                        WriteVisibilityModifiers(writer, adder);

                    writer.Write("add; ");
                }
                if (evt.RemoveMethod is IMethod remover)
                {
                    if (remover.Visibility != evt.Visibility)
                        WriteVisibilityModifiers(writer, remover);

                    writer.Write("remove; ");
                }
                writer.Write('}');
            }
        }

        /// <summary>
        /// Writes the signature of the specified event to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="ev">The event whose signature is to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the member's name.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        private void WriteEventSignature(TextWriter writer, IEvent ev, NameQualifier qualifier, MemberDocLinker linker)
        {
            WriteTypeMemberName(writer, ev, qualifier, linker);
        }

        /// <summary>
        /// Writes the modifiers of the specified event to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the event modifiers are written.</param>
        /// <param name="ev">The event whose modifiers are to be written.</param>
        private static void WriteEventModifiers(TextWriter writer, IEvent ev)
        {
            WriteVisibilityModifiers(writer, ev);

            if (ev.IsStatic)
                writer.Write("static ");
            if (ev.IsUnsafe)
                writer.Write("unsafe ");

            WriteVirtualityModifiers(writer, ev);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Writes the definition of the specified operator method to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="op">The operator method whose definition is to be written.</param>
        /// <param name="linker">A delegate for linking to the documentation of a type or type's member.</param>
        private void WriteOperatorDefinition(TextWriter writer, IOperator op, MemberDocLinker linker)
        {
            var attributes = op.ExplicitCustomAttributes.Concat(op.Return.ExplicitCustomAttributes);
            WriteAttributes(writer, attributes, linker);

            WriteOperatorModifiers(writer, op);

            var interfaceOp = op.IsExplicitInterfaceImplementation ? op.ImplementedOperator : null;
            var operatorName = interfaceOp is null ? op.Name : interfaceOp.Name;

            if (!TryGetOperatorSymbol(operatorName, out var operatorSymbol))
                operatorSymbol = operatorName;

            if (op.IsConversionOperator)
            {
                writer.Write(operatorSymbol);
                WriteQualifiedOperatorKeyword();
                WriteTypeSignature(writer, op.Return.Type, Options.GlobalNameQualifier, linker);
            }
            else
            {
                WriteTypeSignature(writer, op.Return.Type, Options.GlobalNameQualifier, linker);
                WriteQualifiedOperatorKeyword();
                writer.Write(operatorSymbol);
            }

            writer.Write('(');
            WriteParameters(writer, op.Parameters, linker, declarative: true);
            writer.Write(')');

            void WriteQualifiedOperatorKeyword()
            {
                writer.Write(' ');
                if (interfaceOp is not null)
                {
                    var qualifier = Options.FullyQualifyExplicitInterfaceMemberNames ? NameQualifier.Full : NameQualifier.DeclaringType;
                    WriteTypeSignature(writer, interfaceOp.DeclaringType, qualifier, linker);
                    writer.Write('.');
                }
                writer.Write("operator ");
            }
        }

        /// <summary>
        /// Writes the signature of the specified operator method to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="op">The operator method whose signature is to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the member's name.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        /// <param name="declarative"><see langword="true"/> to include the parameter names; otherwise, <see langword="false"/>.</param>
        private void WriteOperatorSignature(TextWriter writer, IOperator op, NameQualifier qualifier, MemberDocLinker linker, bool declarative = false)
        {
            WriteTypeMemberName(writer, op, qualifier, linker);

            writer.Write('(');
            WriteParameters(writer, op.Parameters, linker, declarative);
            writer.Write(')');
        }

        /// <summary>
        /// Writes the modifiers of the specified operator to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the operator modifiers are written.</param>
        /// <param name="op">The operator whose modifiers are to be written.</param>
        private static void WriteOperatorModifiers(TextWriter writer, IOperator op)
        {
            WriteVisibilityModifiers(writer, op);

            if (op.IsStatic)
                writer.Write("static ");
            if (op.IsUnsafe)
                writer.Write("unsafe ");
            if (op.IsReadOnly)
                writer.Write("readonly ");

            WriteVirtualityModifiers(writer, op);
        }

        #endregion
    }
}

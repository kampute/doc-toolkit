// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Languages
{
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents the C# programming language model.
    /// </summary>
    /// <remarks>
    /// The <see cref="CSharp"/> class is a concrete implementation of the <see cref="IProgrammingLanguage"/> interface
    /// that provides formatting functionality specifically for the C# programming language. It handles the complexities
    /// of C# syntax including:
    /// <list type="bullet">
    ///   <item><description>Rendering type and member signatures according to C# syntax conventions</description></item>
    ///   <item><description>Formatting type definitions with proper modifiers and constraints</description></item>
    ///   <item><description>Handling special C# features like operators, extension methods, and attributes</description></item>
    ///   <item><description>Properly formatting literals according to C# syntax rules</description></item>
    ///   <item><description>Providing links to official C# documentation for language keywords</description></item>
    /// </list>
    /// This class serves as the foundation for generating properly formatted C# references in documentation, ensuring that
    /// code snippets and API references follow standard C# conventions.
    /// <para>
    /// The <see cref="CSharp"/> class supports customization through <see cref="CodeStyleOptions"/> to adjust the formatting
    /// style to match different coding standards and preferences.
    /// </para>
    /// </remarks>
    public partial class CSharp : IProgrammingLanguage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CSharp"/> class with default options.
        /// </summary>
        public CSharp()
            : this(new CodeStyleOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharp"/> class with the specified options.
        /// </summary>
        /// <param name="options">The options for the C# language formatter.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
        public CSharp(CodeStyleOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets the name of the language.
        /// </summary>
        /// <value>A <see cref="string"/> representing the name of the language.</value>
        public string Name => "C#";

        /// <summary>
        /// Gets the identifier used for signature highlighting of code written in the programming language.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the identifier of the programming language for signature highlighting.
        /// </value>
        public string Identifier => "csharp";

        /// <summary>
        /// Gets the options the define the behavior of the C# language when formatting code elements.
        /// </summary>
        /// <value>
        /// The options the define the behavior of the C# language when formatting code elements.
        /// </value>
        public CodeStyleOptions Options { get; }

        /// <summary>
        /// Attempts to retrieves the URL to the official documentation for a specified C# keyword.
        /// </summary>
        /// <param name="keyword">The C# keyword to look up.</param>
        /// <param name="url">
        /// When this method returns, contains the URL of the keyword documentation, if the keyword is found; otherwise, <see langword="null"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true"/> if the keyword is found; otherwise, <see langword="false"/>.</returns>
        public virtual bool TryGetUrl(string keyword, [NotNullWhen(true)] out Uri? url) => TryGetKeywordUrl(keyword, out url);

        /// <summary>
        /// Writes the name of a member formatted according to C# syntax rules to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the member's name is written.</param>
        /// <param name="member">The member whose name is to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the member's name.</param>
        /// <param name="linker">An optional delegate for linking to the documentation of a type or type's member.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="member"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// The formatting rules for different member types are detailed below:
        /// <list type="table">
        ///   <listheader>
        ///     <term>Member Type</term>
        ///     <description>Formatted Name and Example</description>
        ///   </listheader>
        ///   <item>
        ///     <term><see cref="IType"/></term>
        ///     <description>Type name, including its generic parameters and modifiers (e.g., "Dictionary&lt;TKey, TValue&gt;, "string[]", "int*")</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="IField"/></term>
        ///     <description>Field name (e.g., "MinValue")</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="IConstructor"/></term>
        ///     <description>Empty string</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="IMethod"/></term>
        ///     <description>Method name without generic parameters (e.g., "Calculate")</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="IProperty"/></term>
        ///     <description>Property name (e.g., "Count", "Item[]")</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="IEvent"/></term>
        ///     <description>Event name (e.g., "ValueChanged")</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="IOperator"/></term>
        ///     <description>Operator name (e.g., "Addition")</description>
        ///   </item>
        /// </list>
        /// If the member type is not recognized, the member's name is written as is.
        /// </remarks>
        public virtual void WriteName(TextWriter writer, IMember member, NameQualifier qualifier, MemberDocLinker? linker = null)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            linker ??= DefaultLinker;

            switch (member)
            {
                case ITypeMember typeMember:
                    WriteTypeMemberName(writer, typeMember, qualifier, linker, indexerName: "Item[]");
                    break;
                case IType type:
                    WriteTypeSignature(writer, type, qualifier, linker);
                    break;
                default:
                    linker(writer, member, member.Name);
                    break;
            }
        }

        /// <summary>
        /// Writes the signature of a member formatted according to C# syntax rules to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the member's signature is written.</param>
        /// <param name="member">The member whose signature is to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the member's name.</param>
        /// <param name="linker">An optional delegate for linking to the documentation of a type or type's member.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="member"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// The signature format varies by member type as detailed below:
        /// <list type="table">
        ///   <listheader>
        ///     <term>Member Type</term>
        ///     <description>Formatted Signature and Example</description>
        ///   </listheader>
        ///   <item>
        ///     <term><see cref="IType"/></term>
        ///     <description>Type name, including its generic parameters and modifiers (e.g., "Dictionary&lt;TKey, TValue&gt;, "string[]", "int*")</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="IField"/></term>
        ///     <description>Field name (e.g., "MinValue")</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="IConstructor"/></term>
        ///     <description>Declaring type name with parameters (e.g., "MyClass(string, int)")</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="IMethod"/></term>
        ///     <description>Method name with its generic and regular parameters (e.g., "Convert&lt;T&gt;(string)")</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="IProperty"/></term>
        ///     <description>Property name with any index parameters (e.g., "Count", "Items[int]")</description>
        ///   </item>
        ///   <item>
        ///     <term><see cref="IEvent"/></term>
        ///     <description>Event name (e.g., "PropertyChanged")</description>
        ///   </item>
        ///   <Item>
        ///     <term><see cref="IOperator"/></term>
        ///     <description>Operator name and its parameters (e.g., "Addition(MyType, MyType)")</description>
        ///   </Item>
        /// </list>
        /// If the member type is not recognized, the member's name is written as is.
        /// </remarks>
        public virtual void WriteSignature(TextWriter writer, IMember member, NameQualifier qualifier, MemberDocLinker? linker = null)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            linker ??= DefaultLinker;

            switch (member)
            {
                case IType t:
                    WriteTypeSignature(writer, t, qualifier, linker);
                    break;
                case IConstructor c:
                    WriteConstructorSignature(writer, c, qualifier, linker, declarative: false);
                    break;
                case IMethod m:
                    WriteMethodSignature(writer, m, qualifier, linker, declarative: false);
                    break;
                case IProperty p:
                    WritePropertySignature(writer, p, qualifier, linker, declarative: false);
                    break;
                case IEvent e:
                    WriteEventSignature(writer, e, qualifier, linker);
                    break;
                case IField f:
                    WriteFieldSignature(writer, f, qualifier, linker);
                    break;
                case IOperator o:
                    WriteOperatorSignature(writer, o, qualifier, linker);
                    break;
                default:
                    linker(writer, member, member.Name);
                    break;
            }
        }

        /// <summary>
        /// Writes the definition of a specified member as it appears in C# code to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the member definition is written.</param>
        /// <param name="member">The member whose definition is to be written.</param>
        /// <param name="linker">An optional delegate for linking to the documentation of a type or type's member.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="member"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="member"/> type is not supported.</exception>
        /// <remarks>
        /// The output includes a complete C# code definition of the member, containing:
        /// <list type="bullet">
        ///   <listheader>
        ///     <term>Component</term>
        ///     <description>Description and Example</description>
        ///   </listheader>
        ///   <item>
        ///     <term>Attributes</term>
        ///     <description>Any applied attributes</description>
        ///   </item>
        ///   <item>
        ///     <term>Access Modifiers</term>
        ///     <description>Visibility and other modifiers</description>
        ///   </item>
        ///   <item>
        ///       <term>Type Information</term>
        ///       <description>Return types, parameter types, and generic constraints where applicable</description>
        ///   </item>
        ///   <item>
        ///       <term>Signature</term>
        ///       <description>Member name and parameters</description>
        ///   </item>
        ///   <item>
        ///       <term>Property Accessors</term>
        ///       <description>Get and set accessors with their access modifiers</description>
        ///   </item>
        ///   <item>
        ///       <term>Constraints</term>
        ///       <description>Generic type constraints if present</description>
        ///   </item>
        /// </list>
        /// </remarks>
        public virtual void WriteDefinition(TextWriter writer, IMember member, MemberDocLinker? linker = null)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            linker ??= DefaultLinker;

            switch (member)
            {
                case IType t:
                    WriteTypeDefinition(writer, t, linker);
                    break;
                case IConstructor c:
                    WriteConstructorDefinition(writer, c, linker);
                    break;
                case IMethod m:
                    WriteMethodDefinition(writer, m, linker);
                    break;
                case IProperty p:
                    WritePropertyDefinition(writer, p, linker);
                    break;
                case IEvent e:
                    WriteEventDefinition(writer, e, linker);
                    break;
                case IField f:
                    WriteFieldDefinition(writer, f, linker);
                    break;
                case IOperator o:
                    WriteOperatorDefinition(writer, o, linker);
                    break;
                default:
                    throw new ArgumentException($"Unsupported member type: {member.GetType().FullName}", nameof(member));
            }
        }

        /// <summary>
        /// Writes the specified attribute formatted according to C# syntax rules to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write the attribute to.</param>
        /// <param name="attribute">The <see cref="ICustomAttribute"/> representing the attribute to be written.</param>
        /// <param name="qualifier">The level of qualification to apply to the attribute's name.</param>
        /// <param name="linker">An optional delegate for linking to the documentation of a type or type's member.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="attribute"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method writes the attribute's type name and its constructor arguments, if any, to the provided <see cref="TextWriter"/>.
        /// Named arguments are also written in the format "name = value".
        /// </remarks>
        public virtual void WriteAttribute(TextWriter writer, ICustomAttribute attribute, NameQualifier qualifier, MemberDocLinker? linker = null)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (attribute is null)
                throw new ArgumentNullException(nameof(attribute));

            linker ??= DefaultLinker;

            WriteTypeSignature(writer, attribute.Type, Options.GlobalNameQualifier, (w, member, name) =>
            {
                if (ReferenceEquals(member, attribute.Type) && name.EndsWith("Attribute", StringComparison.Ordinal))
                    name = name[..^9];

                linker(w, member, name);
            });

            var anyParameterBefore = false;

            foreach (var argument in attribute.ConstructorArguments)
            {
                EnsureCommaBeforeNext();
                WriteTypedValue(writer, argument, linker);
            }

            foreach (var (name, argument) in attribute.NamedArguments)
            {
                EnsureCommaBeforeNext();
                writer.Write(name);
                writer.Write(" = ");
                WriteTypedValue(writer, argument, linker);
            }

            if (anyParameterBefore)
                writer.Write(')');

            void EnsureCommaBeforeNext()
            {
                if (anyParameterBefore)
                {
                    writer.Write(", ");
                    return;
                }

                writer.Write('(');
                anyParameterBefore = true;
            }
        }

        /// <summary>
        /// Writes a constant value formatted according to C# syntax rules to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the constant value is written.</param>
        /// <param name="value">The value to be formatted as a constant and written.</param>
        /// <param name="valueType">The type of the value to be written. If <see langword="null"/>, the type is inferred from the value.</param>
        /// <param name="linker">An optional delegate for linking to the documentation of a type or type's member.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="value"/> is not a constant value.</exception>
        public virtual void WriteConstantValue(TextWriter writer, object? value, IType? valueType = null, MemberDocLinker? linker = null)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));

            if (value is null)
            {
                if (valueType is null || !valueType.IsValueType || valueType is ITypeDecorator { Modifier: TypeModifier.Nullable })
                    writer.Write("null");
                else
                    writer.Write("default");
                return;
            }

            valueType ??= value.GetType().GetMetadata();
            linker ??= DefaultLinker;

            if (valueType is IEnumType enumType)
            {
                WriteEnumValue(writer, enumType, value, linker);
                return;
            }

            if (valueType.FullName == "System.Type" && value is Type type)
            {
                writer.Write("typeof(");
                WriteTypeSignature(writer, type.GetMetadata(), Options.GlobalNameQualifier, linker);
                writer.Write(')');
                return;
            }

            if (valueType is ITypeDecorator { Modifier: TypeModifier.Array } arrayType && value is Array array)
            {
                writer.Write('[');
                for (var i = 0; i < array.Length; ++i)
                {
                    if (i != 0)
                        writer.Write(", ");

                    WriteConstantValue(writer, array.GetValue(i), arrayType.ElementType, linker);
                }
                writer.Write(']');
                return;
            }

            WriteLiteral(writer, value);
        }

        /// <summary>
        /// Returns a string that represents the name of the programming language.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the name of the programming language.</returns>
        public override string ToString() => Name;

        #region Helper Methods

        /// <summary>
        /// Writes the specified parameter to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="parameter">The <see cref="IParameter"/> representing the parameter.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        /// <param name="declarative"><see langword="true"/> to includes attributes, name, and default value of the parameter; otherwise, <see langword="false"/>.</param>
        private void WriteParameter(TextWriter writer, IParameter parameter, MemberDocLinker linker, bool declarative)
        {
            if (declarative && WriteAttributes(writer, parameter.ExplicitCustomAttributes, linker, singleLine: true))
                writer.Write(' ');

            var parameterType = parameter.Type;

            if (parameter.Position == 0 && parameter.Member is IMethod method && method.IsExtensionMethodFor(parameterType))
                writer.Write("this ");

            if (parameterType is ITypeDecorator { Modifier: TypeModifier.ByRef } reference)
            {
                parameterType = reference.ElementType;
                switch (parameter.ReferenceKind)
                {
                    case ParameterReferenceKind.In:
                        writer.Write("in ");
                        break;
                    case ParameterReferenceKind.Out:
                        writer.Write("out ");
                        break;
                    case ParameterReferenceKind.Ref:
                        writer.Write("ref ");
                        break;
                }
            }
            else if (parameter.IsParameterArray)
            {
                writer.Write("params ");
            }

            WriteTypeSignature(writer, parameter.Type, Options.GlobalNameQualifier, linker);

            if (declarative && !string.IsNullOrEmpty(parameter.Name))
            {
                writer.Write(' ');
                writer.Write(parameter.Name);
                if (parameter.HasDefaultValue)
                {
                    writer.Write(" = ");
                    WriteConstantValue(writer, parameter.DefaultValue, parameterType, linker);
                }
            }
        }

        /// <summary>
        /// Writes the specified parameters to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="parameters">The list of <see cref="IParameter"/> representing the parameters.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        /// <param name="declarative">If <see langword="true"/>, includes attributes, name, and default value of each parameter.</param>
        private void WriteParameters(TextWriter writer, IReadOnlyList<IParameter> parameters, MemberDocLinker linker, bool declarative)
        {
            var needsWrapping = declarative && parameters.Count > Options.MaxInlineParameters;

            for (var i = 0; i < parameters.Count; ++i)
            {
                if (i > 0)
                    writer.Write(needsWrapping ? "," : ", ");

                if (needsWrapping)
                {
                    writer.WriteLine();
                    writer.Write('\t');
                }

                WriteParameter(writer, parameters[i], linker, declarative);
            }
        }

        /// <summary>
        /// Writes the specified attributes to the <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="attributes">The collection of <see cref="ICustomAttribute"/> representing the attributes.</param>
        /// <param name="linker">The delegate for linking to the documentation of a type or type's member.</param>
        /// <param name="singleLine"><see langword="true"/> to write the attributes in a single line; otherwise, <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if any attributes were written; otherwise, <see langword="false"/>.</returns>
        private bool WriteAttributes
        (
            TextWriter writer,
            IEnumerable<ICustomAttribute> attributes,
            MemberDocLinker linker,
            bool singleLine = false
        )
        {
            var anyWritten = false;
            foreach (var attributeData in attributes)
            {
                if (Options.ShouldIgnoreAttribute(attributeData))
                    continue;

                if (attributeData.Target == AttributeTarget.ReturnParameter)
                    writer.Write("[return: ");
                else
                    writer.Write('[');

                WriteAttribute(writer, attributeData, Options.GlobalNameQualifier, linker);

                if (singleLine)
                    writer.Write(']');
                else
                    writer.WriteLine(']');

                anyWritten = true;
            }
            return anyWritten;
        }

        /// <summary>
        /// Writes the modifiers that define the visibility of a member to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the member modifiers are written.</param>
        /// <param name="visibility">The visibility level to be written.</param>
        private static void WriteVisibilityModifiers(TextWriter writer, MemberVisibility visibility)
        {
            switch (visibility)
            {
                case MemberVisibility.Public:
                    writer.Write("public ");
                    break;
                case MemberVisibility.ProtectedInternal:
                    writer.Write("protected "); // "internal" is not visible outside the assembly
                    break;
                case MemberVisibility.Protected:
                    writer.Write("protected ");
                    break;
                case MemberVisibility.Internal:
                    writer.Write("internal ");
                    break;
                case MemberVisibility.PrivateProtected:
                    writer.Write("private "); // "protected" is not visible outside the assembly
                    break;
                case MemberVisibility.Private:
                    writer.Write("private ");
                    break;
            }
        }

        /// <summary>
        /// Writes the modifiers that define the virtuality of a member to the provided <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to which the member modifiers are written.</param>
        /// <param name="virtuality">The virtuality level to be written.</param>
        private static void WriteVirtualityModifiers(TextWriter writer, MemberVirtuality virtuality)
        {
            switch (virtuality)
            {
                case MemberVirtuality.Abstract:
                    writer.Write("abstract ");
                    break;
                case MemberVirtuality.Virtual:
                    writer.Write("virtual ");
                    break;
                case MemberVirtuality.Override:
                    writer.Write("override ");
                    break;
                case MemberVirtuality.SealedOverride:
                    writer.Write("sealed override ");
                    break;
            }
        }

        /// <summary>
        /// Writes the provided name of a specified member to the <see cref="TextWriter"/> without link to its documentation.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write to.</param>
        /// <param name="member">The <see cref="IMember"/> whose name is to be written.</param>
        /// <param name="name">The name of the member to be written.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DefaultLinker(TextWriter writer, IMember member, string name) => writer.Write(name);

        #endregion
    }
}

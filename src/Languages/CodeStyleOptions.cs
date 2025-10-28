// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Languages
{
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Metadata;
    using System;

    /// <summary>
    /// Represents the options that control the behavior of the programming language models when formatting code elements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="CodeStyleOptions"/> class provides a configurable set of formatting preferences that determine how
    /// code elements are rendered in documentation. It allows documentation authors to customize the appearance of code
    /// signatures and definitions to match their preferred style or organizational standards.
    /// </para>
    /// The class offers configuration options for:
    /// <list type="bullet">
    ///   <item><description>Parameter formatting and wrapping behavior</description></item>
    ///   <item><description>Type name simplification for system types</description></item>
    ///   <item><description>Name qualification levels for different code elements</description></item>
    ///   <item><description>Attribute visibility and filtering</description></item>
    /// </list>
    /// By providing a single point of configuration, <see cref="CodeStyleOptions"/> ensures consistent formatting throughout
    /// the documentation regardless of where code elements are rendered. This consistency improves readability and maintains
    /// a professional appearance across all generated documentation.
    /// <para>
    /// Instances of this class can be passed to implementations of <see cref="IProgrammingLanguage"/> to control their formatting
    /// behavior. The default values are chosen to provide a balance between readability and completeness in most documentation
    /// scenarios.
    /// </para>
    /// </remarks>
    public class CodeStyleOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeStyleOptions"/> class.
        /// </summary>
        public CodeStyleOptions()
        {
        }

        /// <summary>
        /// Gets or sets the maximum number of parameters in a member definition before wrapping.
        /// </summary>
        /// <value>
        /// The maximum number of parameters in a member definition before wrapping. The default is 3.
        /// </value>
        /// <remarks>
        /// This property determines how many parameters can be written in a single line when writing a member definition.
        /// If the number of parameters in the definition exceeds this value, each parameter is placed on a new line.
        /// </remarks>
        public int MaxInlineParameters { get; set; } = 3;

        /// <summary>
        /// Gets or sets a value indicating whether to simplify the names of system types.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to simplify the names of system types; otherwise, <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        public bool SimplifySystemTypeNames { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to fully qualify explicit interface member names.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to fully qualify explicit interface member names with namespace and interface name; <see langword="false"/> to qualify only with the interface name.
        /// only with the interface name. The default value is <see langword="false"/>.
        /// <note type="important" title="Important">
        /// This option defines the minimum qualification level for explicit interface member names. If the caller requests a higher level of qualification, that level will be used instead.
        /// </note>
        /// </value>
        public bool FullyQualifyExplicitInterfaceMemberNames { get; set; } = false;

        /// <summary>
        /// Gets or sets the level of qualification to apply to the name of types for types of parameters, return values, extended classes, implemented
        /// interfaces, attributes, and type parameters.
        /// </summary>
        /// <value>
        /// The level of qualification to apply to the name of types for types of parameters, return values, extended classes, implemented interfaces,
        /// attributes, and type parameters. The default is <see cref="NameQualifier.DeclaringType"/>.
        /// </value>
        public NameQualifier GlobalNameQualifier { get; set; } = NameQualifier.DeclaringType;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the attributes applied to types.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to ignore the attributes applied to types; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        public bool IgnoreTypeAttributes { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the attributes applied to type members.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to ignore the attributes applied to a type member; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        public bool IgnoreTypeMemberAttributes { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the attributes applied to generic type parameters.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to ignore the attributes applied to a generic type parameter; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        public bool IgnoreTypeParameterAttributes { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the attributes applied to parameters.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to ignore the attributes applied to parameters; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        public bool IgnoreParameterAttributes { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore the attributes applied to return values.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to ignore the attributes applied to return values; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        public bool IgnoreReturnParameterAttributes { get; set; } = false;

        /// <summary>
        /// Gets the collection of patterns that determine the attributes to exclude from the definition of a type or type's member.
        /// </summary>
        /// <value>
        /// The collection of patterns that determine the attributes to exclude from the definition of a type or type's member.
        /// </value>
        public PatternCollection IgnoredAttributes { get; } = new(Type.Delimiter);

        /// <summary>
        /// Determines whether the specified attribute should be ignored based on the current options and the target where the attribute is applied.
        /// </summary>
        /// <param name="attribute">The <see cref="ICustomAttribute"/> representing the attribute to be checked.</param>
        /// <returns><see langword="true"/> if the attribute should be ignored; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="attribute"/> is <see langword="null"/>.</exception>
        public bool ShouldIgnoreAttribute(ICustomAttribute attribute)
        {
            if (attribute is null)
                throw new ArgumentNullException(nameof(attribute));

            return attribute.Target switch
            {
                AttributeTarget.Type when IgnoreTypeAttributes => true,
                AttributeTarget.TypeMember when IgnoreTypeMemberAttributes => true,
                AttributeTarget.TypeParameter when IgnoreTypeParameterAttributes => true,
                AttributeTarget.Parameter when IgnoreParameterAttributes => true,
                AttributeTarget.ReturnParameter when IgnoreReturnParameterAttributes => true,
                _ => IgnoredAttributes.Count > 0 && IgnoredAttributes.Matches(attribute.Type.FullName)
            };
        }
    }
}

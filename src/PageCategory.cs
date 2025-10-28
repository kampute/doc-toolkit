// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    /// <summary>
    /// Represents the documentation page categories.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="PageCategory"/> enum provides a standardized way to identify different types of documentation pages that can
    /// be generated. This classification is essential for documentation renderers to apply appropriate templates and formatting based
    /// on the content type being presented. It enables the documentation generation system to handle different code elements consistently
    /// while allowing for specialized presentation of each category.
    /// </para>
    /// Categories are organized into three main groups:
    /// <list type="bullet">
    ///   <item><description>Topic and namespace pages (general organization)</description></item>
    ///   <item><description>Type pages (classes, structs, interfaces, enums, and delegates)</description></item>
    ///   <item><description>Member pages (constructors, fields, properties, methods, events, and operators)</description></item>
    /// </list>
    /// Some categories indicate whether members are documented on the same page as their containing type (e.g., ClassWithMembers)
    /// or on separate pages.
    /// </remarks>
    public enum PageCategory
    {
        /// <summary>
        /// The documentation page for a topic.
        /// </summary>
        Topic,

        /// <summary>
        /// The documentation page for a <see langword="namespace"/>.
        /// </summary>
        Namespace,

        /// <summary>
        /// The documentation page for a <see langword="namespace"/>, including documentation of its contained types on the same page.
        /// </summary>
        NamespaceWithTypes,

        /// <summary>
        /// The documentation page for a <see langword="class"/>.
        /// </summary>
        Class,

        /// <summary>
        /// The documentation page for a <see langword="class"/>, including documentation of its members on the same page.
        /// </summary>
        ClassWithMembers,

        /// <summary>
        /// The documentation page for a <see langword="struct"/>.
        /// </summary>
        Struct,

        /// <summary>
        /// The documentation page for a <see langword="struct"/>, including documentation of its members on the same page.
        /// </summary>
        StructWithMembers,

        /// <summary>
        /// The documentation page for an <see langword="interface"/>.
        /// </summary>
        Interface,

        /// <summary>
        /// The documentation page for an <see langword="interface"/>, including documentation of its members on the same page.
        /// </summary>
        InterfaceWithMembers,

        /// <summary>
        /// The documentation page for an <see langword="enum"/>.
        /// </summary>
        Enum,

        /// <summary>
        /// The documentation page for a <see langword="delegate"/>.
        /// </summary>
        Delegate,

        /// <summary>
        /// The documentation page for a constructor.
        /// </summary>
        Constructor,

        /// <summary>
        /// The documentation page for a collection of overloaded constructors.
        /// </summary>
        ConstructorOverloads,

        /// <summary>
        /// The documentation page for a field.
        /// </summary>
        Field,

        /// <summary>
        /// The documentation page for an event.
        /// </summary>
        Event,

        /// <summary>
        /// The documentation page for a property.
        /// </summary>
        Property,

        /// <summary>
        /// The documentation page for a collection of overloaded properties.
        /// </summary>
        PropertyOverloads,

        /// <summary>
        /// The documentation page for a method.
        /// </summary>
        Method,

        /// <summary>
        /// The documentation page for a collection of overloaded methods.
        /// </summary>
        MethodOverloads,

        /// <summary>
        /// The documentation page for an operator.
        /// </summary>
        Operator,

        /// <summary>
        /// The documentation page for a collection of overloaded operators.
        /// </summary>
        OperatorOverloads
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Models;
    using System;
    using System.IO;

    /// <summary>
    /// Renders documentation pages for various code elements.
    /// </summary>
    /// <remarks>
    /// The <see cref="DocumentPageRenderer"/> class provides a base implementation for rendering documentation pages for various code
    /// elements. It standardizes the process of transforming code metadata and documentation content into properly formatted pages,
    /// serving as a bridge between raw documentation data and the final output format.
    /// <para>
    /// This abstract class implements the Template Method pattern, providing concrete implementations for rendering different types
    /// of documentation elements while delegating the actual formatting to derived classes through the abstract <see cref="Render"/>
    /// method. This design allows for multiple output formats (HTML, Markdown, etc.) while maintaining a consistent documentation
    /// structure.
    /// </para>
    /// The class handles rendering for all standard code elements:
    /// <list type="bullet">
    ///   <item><description>Topics and namespaces (general organization)</description></item>
    ///   <item><description>Types including classes, structs, interfaces, enums, and delegates</description></item>
    ///   <item><description>Members including constructors, fields, properties, methods, events, and operators</description></item>
    /// </list>
    /// When implementing a custom renderer, developers only need to override the abstract <see cref="Render"/> method to define
    /// how different page categories should be formatted, while inheriting the standardized rendering flow.
    /// </remarks>
    /// <seealso cref="IDocumentRenderer"/>
    public abstract class DocumentPageRenderer : IDocumentRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPageRenderer"/> class.
        /// </summary>
        protected DocumentPageRenderer()
        {
        }

        /// <summary>
        /// Renders the specified topic.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="topic">The topic to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="topic"/> is <see langword="null"/>.</exception>
        public virtual void RenderTopic(TextWriter writer, TopicModel topic)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (topic is null)
                throw new ArgumentNullException(nameof(topic));

            Render(writer, PageCategory.Topic, topic);
        }

        /// <summary>
        /// Renders the specified namespace information.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="ns">The namespace information to render.</param>
        /// <param name="includeTypes">Indicates whether to include detailed documentation of types defined in the namespace.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="ns"/> is <see langword="null"/>.</exception>
        public virtual void RenderNamespace(TextWriter writer, NamespaceModel ns, bool includeTypes)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (ns is null)
                throw new ArgumentNullException(nameof(ns));

            Render(writer, includeTypes ? PageCategory.NamespaceWithTypes : PageCategory.Namespace, ns);
        }

        /// <summary>
        /// Renders the specified class.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="classType">The class to render.</param>
        /// <param name="includeMembers">Indicates whether to include detailed documentation of class members.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="classType"/> is <see langword="null"/>.</exception>
        public virtual void RenderClass(TextWriter writer, ClassModel classType, bool includeMembers)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (classType is null)
                throw new ArgumentNullException(nameof(classType));

            Render(writer, includeMembers ? PageCategory.ClassWithMembers : PageCategory.Class, classType);
        }

        /// <summary>
        /// Renders the specified struct.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="structType">The struct to render.</param>
        /// <param name="includeMembers">Indicates whether to include detailed documentation of struct members.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="structType"/> is <see langword="null"/>.</exception>
        public virtual void RenderStruct(TextWriter writer, StructModel structType, bool includeMembers)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (structType is null)
                throw new ArgumentNullException(nameof(structType));

            Render(writer, includeMembers ? PageCategory.StructWithMembers : PageCategory.Struct, structType);
        }

        /// <summary>
        /// Renders the specified interface type
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="interfaceType">The interface to render.</param>
        /// <param name="includeMembers">Indicates whether to include detailed documentation of interface members.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="interfaceType"/> is <see langword="null"/>.</exception>
        public virtual void RenderInterface(TextWriter writer, InterfaceModel interfaceType, bool includeMembers)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (interfaceType is null)
                throw new ArgumentNullException(nameof(interfaceType));

            Render(writer, includeMembers ? PageCategory.InterfaceWithMembers : PageCategory.Interface, interfaceType);
        }

        /// <summary>
        /// Renders the specified enum.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="enumType">The enum to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="enumType"/> is <see langword="null"/>.</exception>
        public virtual void RenderEnum(TextWriter writer, EnumModel enumType)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (enumType is null)
                throw new ArgumentNullException(nameof(enumType));

            Render(writer, PageCategory.Enum, enumType);
        }

        /// <summary>
        /// Renders the specified delegate type.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="delegateType">The delegate type to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="delegateType"/> is <see langword="null"/>.</exception>
        public virtual void RenderDelegate(TextWriter writer, DelegateModel delegateType)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (delegateType is null)
                throw new ArgumentNullException(nameof(delegateType));

            Render(writer, PageCategory.Delegate, delegateType);
        }

        /// <summary>
        /// Renders the specified constructor.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="constructor">The constructor to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="constructor"/> is <see langword="null"/>.</exception>
        public virtual void RenderConstructor(TextWriter writer, ConstructorModel constructor)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (constructor is null)
                throw new ArgumentNullException(nameof(constructor));

            Render(writer, PageCategory.Constructor, constructor);
        }

        /// <summary>
        /// Renders the specified constructor overloads.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="constructors">The overloaded constructors to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="constructors"/> is <see langword="null"/>.</exception>
        public virtual void RenderConstructorOverloads(TextWriter writer, OverloadCollection<ConstructorModel> constructors)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (constructors is null)
                throw new ArgumentNullException(nameof(constructors));

            Render(writer, PageCategory.ConstructorOverloads, constructors);
        }

        /// <summary>
        /// Renders the specified field.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="field">The field to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="field"/> is <see langword="null"/>.</exception>
        public virtual void RenderField(TextWriter writer, FieldModel field)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (field is null)
                throw new ArgumentNullException(nameof(field));

            Render(writer, PageCategory.Field, field);
        }

        /// <summary>
        /// Renders the specified event.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="anEvent">The event to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="anEvent"/> is <see langword="null"/>.</exception>
        public virtual void RenderEvent(TextWriter writer, EventModel anEvent)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (anEvent is null)
                throw new ArgumentNullException(nameof(anEvent));

            Render(writer, PageCategory.Event, anEvent);
        }

        /// <summary>
        /// Renders the specified property.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="property">The property to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="property"/> is <see langword="null"/>.</exception>
        public virtual void RenderProperty(TextWriter writer, PropertyModel property)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (property is null)
                throw new ArgumentNullException(nameof(property));

            Render(writer, PageCategory.Property, property);
        }

        /// <summary>
        /// Renders the specified property overloads.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="properties">The overloaded properties to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="properties"/> is <see langword="null"/>.</exception>
        public virtual void RenderPropertyOverloads(TextWriter writer, OverloadCollection<PropertyModel> properties)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (properties is null)
                throw new ArgumentNullException(nameof(properties));

            Render(writer, PageCategory.PropertyOverloads, properties);
        }

        /// <summary>
        /// Renders the specified method.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="method">The method to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="method"/> is <see langword="null"/>.</exception>
        public virtual void RenderMethod(TextWriter writer, MethodModel method)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (method is null)
                throw new ArgumentNullException(nameof(method));

            Render(writer, PageCategory.Method, method);
        }

        /// <summary>
        /// Renders the specified method overloads.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="methods">The overloaded methods to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="methods"/> is <see langword="null"/>.</exception>
        public virtual void RenderMethodOverloads(TextWriter writer, OverloadCollection<MethodModel> methods)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (methods is null)
                throw new ArgumentNullException(nameof(methods));

            Render(writer, PageCategory.MethodOverloads, methods);
        }

        /// <summary>
        /// Renders the specified operator.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="anOperator">The operator to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="anOperator"/> is <see langword="null"/>.</exception>
        public virtual void RenderOperator(TextWriter writer, OperatorModel anOperator)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (anOperator is null)
                throw new ArgumentNullException(nameof(anOperator));

            Render(writer, PageCategory.Operator, anOperator);
        }

        /// <summary>
        /// Renders the specified operator methods.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="operators">The overloaded operators to render.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> or <paramref name="operators"/> is <see langword="null"/>.</exception>
        public virtual void RenderOperatorOverloads(TextWriter writer, OverloadCollection<OperatorModel> operators)
        {
            if (writer is null)
                throw new ArgumentNullException(nameof(writer));
            if (operators is null)
                throw new ArgumentNullException(nameof(operators));

            Render(writer, PageCategory.OperatorOverloads, operators);
        }

        /// <summary>
        /// Renders the specified documentation page.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="category">The category of the documentation page to render.</param>
        /// <param name="model">The model to render.</param>
        /// <remarks>
        /// This method renders different types of documentation pages based on the specified <paramref name="category"/>.
        /// The following table shows the mapping between page categories and model types:
        /// <list type="table">
        ///   <listheader><term>Page Category</term><description>Model Type</description></listheader>
        ///   <item><term><see cref="PageCategory.Topic"/></term><description><see cref="TopicModel"/></description></item>
        ///   <item><term><see cref="PageCategory.Namespace"/></term><description><see cref="NamespaceModel"/></description></item>
        ///   <item><term><see cref="PageCategory.Class"/></term><description><see cref="ClassModel"/></description></item>
        ///   <item><term><see cref="PageCategory.ClassWithMembers"/></term><description><see cref="ClassModel"/></description></item>
        ///   <item><term><see cref="PageCategory.Struct"/></term><description><see cref="StructModel"/></description></item>
        ///   <item><term><see cref="PageCategory.StructWithMembers"/></term><description><see cref="StructModel"/></description></item>
        ///   <item><term><see cref="PageCategory.Interface"/></term><description><see cref="InterfaceModel"/></description></item>
        ///   <item><term><see cref="PageCategory.InterfaceWithMembers"/></term><description><see cref="InterfaceModel"/></description></item>
        ///   <item><term><see cref="PageCategory.Enum"/></term><description><see cref="EnumModel"/></description></item>
        ///   <item><term><see cref="PageCategory.Delegate"/></term><description><see cref="DelegateModel"/></description></item>
        ///   <item><term><see cref="PageCategory.Constructor"/></term><description><see cref="ConstructorModel"/></description></item>
        ///   <item><term><see cref="PageCategory.ConstructorOverloads"/></term><description><see cref="OverloadCollection{T}"/> where <c>T</c> is <see cref="ConstructorModel"/></description></item>
        ///   <item><term><see cref="PageCategory.Field"/></term><description><see cref="FieldModel"/></description></item>
        ///   <item><term><see cref="PageCategory.Event"/></term><description><see cref="EventModel"/></description></item>
        ///   <item><term><see cref="PageCategory.Property"/></term><description><see cref="PropertyModel"/></description></item>
        ///   <item><term><see cref="PageCategory.PropertyOverloads"/></term><description><see cref="OverloadCollection{T}"/> where <c>T</c> is <see cref="PropertyModel"/></description></item>
        ///   <item><term><see cref="PageCategory.Method"/></term><description><see cref="MethodModel"/></description></item>
        ///   <item><term><see cref="PageCategory.MethodOverloads"/></term><description><see cref="OverloadCollection{T}"/> where <c>T</c> is <see cref="MethodModel"/></description></item>
        ///   <item><term><see cref="PageCategory.Operator"/></term><description><see cref="OperatorModel"/></description></item>
        ///   <item><term><see cref="PageCategory.OperatorOverloads"/></term><description><see cref="OverloadCollection{T}"/> where <c>T</c> is <see cref="OperatorModel"/></description></item>
        /// </list>
        /// </remarks>
        protected abstract void Render(TextWriter writer, PageCategory category, IDocumentModel model);
    }
}

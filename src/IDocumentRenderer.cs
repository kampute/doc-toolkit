// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Models;
    using System.IO;

    /// <summary>
    /// Defines a contract for rendering documentation pages.
    /// </summary>
    /// <remarks>
    /// The <see cref="IDocumentRenderer"/> interface serves as a key component in the documentation generation pipeline, providing
    /// the contract for transforming code metadata and content into formatted documentation pages. Implementations of this interface
    /// are responsible for applying specific formatting, styling, and layout to documentation content.
    /// <para>
    /// This interface follows the Visitor pattern, with methods for rendering different types of documentation elements (namespaces,
    /// classes, methods, etc.). Each method receives a <see cref="TextWriter"/> and the specific element to render, allowing the
    /// renderer to output the formatted documentation to the destination specified in the output.
    /// </para>
    /// Implementers should ensure consistent styling and formatting across all documentation elements to create a cohesive documentation
    /// experience.
    /// </remarks>
    public interface IDocumentRenderer
    {
        /// <summary>
        /// Renders the specified topic.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="topic">The topic to render.</param>
        void RenderTopic(TextWriter writer, TopicModel topic);

        /// <summary>
        /// Renders the specified namespace information.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="ns">The namespace information to render.</param>
        /// <param name="includeTypes">Indicates whether to include detailed documentation of types defined in the namespace.</param>
        void RenderNamespace(TextWriter writer, NamespaceModel ns, bool includeTypes);

        /// <summary>
        /// Renders the specified class.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="classType">The class to render.</param>
        /// <param name="includeMembers">Indicates whether to include detailed documentation of class members.</param>
        void RenderClass(TextWriter writer, ClassModel classType, bool includeMembers);

        /// <summary>
        /// Renders the specified struct.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="structType">The struct to render.</param>
        /// <param name="includeMembers">Indicates whether to include detailed documentation of struct members.</param>
        void RenderStruct(TextWriter writer, StructModel structType, bool includeMembers);

        /// <summary>
        /// Renders the specified interface type
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="interfaceType">The interface to render.</param>
        /// <param name="includeMembers">Indicates whether to include detailed documentation of interface members.</param>
        void RenderInterface(TextWriter writer, InterfaceModel interfaceType, bool includeMembers);

        /// <summary>
        /// Renders the specified enum.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="enumType">The enum to render.</param>
        void RenderEnum(TextWriter writer, EnumModel enumType);

        /// <summary>
        /// Renders the specified delegate type.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="delegateType">The delegate type to render.</param>
        void RenderDelegate(TextWriter writer, DelegateModel delegateType);

        /// <summary>
        /// Renders the specified constructor.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="constructor">The constructor to render.</param>
        void RenderConstructor(TextWriter writer, ConstructorModel constructor);

        /// <summary>
        /// Renders the specified constructor overloads.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="constructors">The overloaded constructors to render.</param>
        void RenderConstructorOverloads(TextWriter writer, OverloadCollection<ConstructorModel> constructors);

        /// <summary>
        /// Renders the specified field.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="field">The field to render.</param>
        void RenderField(TextWriter writer, FieldModel field);

        /// <summary>
        /// Renders the specified event.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="theEvent">The event to render.</param>
        void RenderEvent(TextWriter writer, EventModel theEvent);

        /// <summary>
        /// Renders the specified property.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="property">The property to render.</param>
        void RenderProperty(TextWriter writer, PropertyModel property);

        /// <summary>
        /// Renders the specified property overloads.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="properties">The overloaded properties to render.</param>
        void RenderPropertyOverloads(TextWriter writer, OverloadCollection<PropertyModel> properties);

        /// <summary>
        /// Renders the specified method.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="method">The method to render.</param>
        void RenderMethod(TextWriter writer, MethodModel method);

        /// <summary>
        /// Renders the specified method overloads.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="methods">The overloaded methods to render.</param>
        void RenderMethodOverloads(TextWriter writer, OverloadCollection<MethodModel> methods);

        /// <summary>
        /// Renders the specified operator.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="theOperator">The operator to render.</param>
        void RenderOperator(TextWriter writer, OperatorModel theOperator);

        /// <summary>
        /// Renders the specified operator methods.
        /// </summary>
        /// <param name="writer">The text writer to render the documentation to.</param>
        /// <param name="operators">The overloaded operator methods to render.</param>
        void RenderOperatorOverloads(TextWriter writer, OverloadCollection<OperatorModel> operators);
    }
}

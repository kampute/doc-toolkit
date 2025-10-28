// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a base class for generating documentation pages.
    /// </summary>
    /// <remarks>
    /// The <see cref="DocumentationComposer"/> class orchestrates the generation of documentation pages, managing both the traversal
    /// of the code structure and the production of individual documentation pages. It functions as the controller in the documentation
    /// generation process, working with a renderer to transform code metadata into formatted documentation.
    /// <para>
    /// The class uses the Template Method pattern, defining the generation algorithm while delegating the actual rendering to an
    /// <see cref="IDocumentRenderer"/> implementation. This separation of concerns allows the composer to focus on the structure
    /// and organization of the documentation while the renderer handles the formatting details.
    /// </para>
    /// <note type="hint" Title="Hint">
    /// The <see cref="FileSystemDocumentationComposer"/> class provides a ready-to-use implementation of this class that writes
    /// documentation to the file system.
    /// </note>
    /// </remarks>
    /// <seealso cref="FileSystemDocumentationComposer"/>
    /// <seealso cref="IDocumentRenderer"/>
    /// <seealso cref="IDocumentationContext"/>
    public abstract class DocumentationComposer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentationComposer"/> class.
        /// </summary>
        protected DocumentationComposer()
        {
        }

        /// <summary>
        /// Gets the documentation renderer.
        /// </summary>
        /// <value>
        /// The documentation renderer used to generate documentation content.
        /// </value>
        /// <remarks>
        /// The renderer is responsible for the actual content generation, while the <see cref="DocumentationComposer"/> handles the traversal of
        /// code elements and the structure of the documentation.
        /// <para>
        /// Derived classes must override this property to provide a concrete <see cref="IDocumentRenderer"/> implementation that defines how
        /// different documentation elements are rendered.
        /// </para>
        /// </remarks>
        protected abstract IDocumentRenderer Renderer { get; }

        /// <summary>
        /// Generates documentation pages for the specified documentation context.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="documentationContext">The documentation context.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="documentationContext"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">Throw when the format of a documentation topic is not supported by the renderer.</exception>
        /// <remarks>
        /// This method initiates the documentation generation process by iterating through all namespaces, types, type members, and topics in
        /// the provided context. For each element, it delegates to specialized methods for generating appropriate documentation pages.
        /// <para>
        /// The page granularity of the documentation context defines which code elements receive their own dedicated files.
        /// </para>
        /// </remarks>
        public virtual void GenerateDocumentation(IDocumentWriterFactory writerFactory, IDocumentationContext documentationContext)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (documentationContext is null)
                throw new ArgumentNullException(nameof(documentationContext));

            foreach (var topic in documentationContext.Topics.Flatten)
                GenerateDocumentationForTopic(writerFactory, topic);

            var granularity = documentationContext.AddressProvider.Granularity;
            foreach (var ns in documentationContext.Namespaces)
                GenerateDocumentationForNamespace(writerFactory, ns, granularity);
        }

        /// <summary>
        /// Generates documentation page for the specified topic.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="topic">The topic to document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="topic"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method creates a documentation page for a topic. Subtopics are not processed; they must be included in the topics
        /// collection of the documentation context to be documented.
        /// </remarks>
        protected virtual void GenerateDocumentationForTopic(IDocumentWriterFactory writerFactory, TopicModel topic)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (topic is null)
                throw new ArgumentNullException(nameof(topic));

            using var writer = writerFactory.CreateWriter(topic);
            Renderer.RenderTopic(writer, topic);
        }

        /// <summary>
        /// Generates documentation pages for the specified namespace and the types it contains.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="ns">The namespace to document.</param>
        /// <param name="granularity">The page granularity that defines how the namespace and its types are organized in the documentation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="ns"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes a namespace and its types based on the specified granularity. The granularity parameter determines
        /// whether the namespace and its types receive their own dedicated documentation pages.
        /// </remarks>
        protected virtual void GenerateDocumentationForNamespace(IDocumentWriterFactory writerFactory, NamespaceModel ns, PageGranularity granularity)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (ns is null)
                throw new ArgumentNullException(nameof(ns));

            if (granularity.HasFlag(PageGranularity.Namespace))
            {
                using var writer = writerFactory.CreateWriter(ns);
                Renderer.RenderNamespace(writer, ns, includeTypes: !granularity.HasFlag(PageGranularity.Type));
            }

            GenerateDocumentationForTypes(writerFactory, ns.Types, granularity);
        }

        /// <summary>
        /// Generates documentation pages for the types in the specified collection.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="types">The collection of types to document.</param>
        /// <param name="granularity">The page granularity that defines how the type and its members are organized in the documentation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="types"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes different type categories (classes, structs, interfaces, enums, delegates) and delegates each type to the
        /// appropriate specialized methods for generating their documentation.
        /// <para>
        /// The <paramref name="granularity"/> parameter controls how documentation is organized, determining whether types and their members
        /// receive their own documentation pages.
        /// </para>
        /// </remarks>
        protected virtual void GenerateDocumentationForTypes(IDocumentWriterFactory writerFactory, IReadOnlyTypeCollection types, PageGranularity granularity)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (types is null)
                throw new ArgumentNullException(nameof(types));

            foreach (var classType in types.Classes)
                GenerateDocumentationForClass(writerFactory, classType, granularity);

            foreach (var structType in types.Structs)
                GenerateDocumentationForStruct(writerFactory, structType, granularity);

            foreach (var interfaceType in types.Interfaces)
                GenerateDocumentationForInterface(writerFactory, interfaceType, granularity);

            if (granularity.HasFlag(PageGranularity.Type))
            {
                foreach (var enumType in types.Enums)
                    GenerateDocumentationForEnum(writerFactory, enumType);

                foreach (var delegateType in types.Delegates)
                    GenerateDocumentationForDelegate(writerFactory, delegateType);
            }
        }

        /// <summary>
        /// Generates documentation pages for the specified class type and its members.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="classType">The class type to document.</param>
        /// <param name="granularity">The page granularity that defines how the class and its members are organized in the documentation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="classType"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes a class type and its members based on the specified granularity.
        /// <list type="bullet">
        ///   <item>If the <paramref name="granularity"/> includes <see cref="PageGranularity.Type"/>, a dedicated page is created for the class.</item>
        ///   <item>If the <paramref name="granularity"/> includes <see cref="PageGranularity.Member"/>, separate documentation pages are generated
        ///         for each member of the class. Otherwise, all member documentation is included on the class page.</item>
        /// </list>
        /// The method also recursively processes any nested types declared within the class.
        /// </remarks>
        protected virtual void GenerateDocumentationForClass(IDocumentWriterFactory writerFactory, ClassModel classType, PageGranularity granularity)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (classType is null)
                throw new ArgumentNullException(nameof(classType));

            if (granularity.HasFlag(PageGranularity.Type))
            {
                using var writer = writerFactory.CreateWriter(classType);
                Renderer.RenderClass(writer, classType, includeMembers: !granularity.HasFlag(PageGranularity.Member));
            }

            if (granularity.HasFlag(PageGranularity.Member))
                GenerateDocumentationForCompositeTypeMembers(writerFactory, classType);
        }

        /// <summary>
        /// Generates documentation pages for the specified struct type and its members.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="structType">The struct type to document.</param>
        /// <param name="granularity">The page granularity that defines how the struct and its members are organized in the documentation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="structType"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes a struct type and its members based on the specified granularity.
        /// <list type="bullet">
        ///   <item>If the <paramref name="granularity"/> includes <see cref="PageGranularity.Type"/>, a dedicated page is created for the struct.</item>
        ///   <item>If the <paramref name="granularity"/> includes <see cref="PageGranularity.Member"/>, separate documentation pages are generated
        ///         for each member of the struct. Otherwise, all member documentation is included on the struct page.</item>
        /// </list>
        /// The method also recursively processes any nested types declared within the struct.
        /// </remarks>
        protected virtual void GenerateDocumentationForStruct(IDocumentWriterFactory writerFactory, StructModel structType, PageGranularity granularity)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (structType is null)
                throw new ArgumentNullException(nameof(structType));

            if (granularity.HasFlag(PageGranularity.Type))
            {
                using var writer = writerFactory.CreateWriter(structType);
                Renderer.RenderStruct(writer, structType, includeMembers: !granularity.HasFlag(PageGranularity.Member));
            }

            if (granularity.HasFlag(PageGranularity.Member))
                GenerateDocumentationForCompositeTypeMembers(writerFactory, structType);
        }

        /// <summary>
        /// Generates documentation pages for the specified interface type and its members.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="interfaceType">The interface type to document.</param>
        /// <param name="granularity">The page granularity that defines how the interface and its members are organized in the documentation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="interfaceType"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes an interface type and its members based on the specified granularity.
        /// <list type="bullet">
        ///   <item>If the <paramref name="granularity"/> includes <see cref="PageGranularity.Type"/>, a dedicated page is created for the interface.</item>
        ///   <item>If the <paramref name="granularity"/> includes <see cref="PageGranularity.Member"/>, separate documentation pages are generated
        ///         for each member of the interface. Otherwise, all member documentation is included on the interface page.</item>
        /// </list>
        /// </remarks>
        protected virtual void GenerateDocumentationForInterface(IDocumentWriterFactory writerFactory, InterfaceModel interfaceType, PageGranularity granularity)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (interfaceType is null)
                throw new ArgumentNullException(nameof(interfaceType));

            if (granularity.HasFlag(PageGranularity.Type))
            {
                using var writer = writerFactory.CreateWriter(interfaceType);
                Renderer.RenderInterface(writer, interfaceType, includeMembers: !granularity.HasFlag(PageGranularity.Member));
            }

            if (granularity.HasFlag(PageGranularity.Member))
            {
                GenerateDocumentationForProperties(writerFactory, interfaceType.Properties);
                GenerateDocumentationForMethods(writerFactory, interfaceType.Methods);
                GenerateDocumentationForEvents(writerFactory, interfaceType.Events);
            }
        }

        /// <summary>
        /// Generates documentation page for the specified enum type.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="enumType">The enum type to document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="enumType"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes an enumeration type to generate its documentation.
        /// </remarks>
        protected virtual void GenerateDocumentationForEnum(IDocumentWriterFactory writerFactory, EnumModel enumType)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (enumType is null)
                throw new ArgumentNullException(nameof(enumType));

            using var writer = writerFactory.CreateWriter(enumType);
            Renderer.RenderEnum(writer, enumType);
        }

        /// <summary>
        /// Generates documentation page for the specified delegate type.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="delegateType">The delegate type to document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="delegateType"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes a delegate type to generate its documentation.
        /// </remarks>
        protected virtual void GenerateDocumentationForDelegate(IDocumentWriterFactory writerFactory, DelegateModel delegateType)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (delegateType is null)
                throw new ArgumentNullException(nameof(delegateType));

            using var writer = writerFactory.CreateWriter(delegateType);
            Renderer.RenderDelegate(writer, delegateType);
        }

        /// <summary>
        /// Generates documentation page for the specified constructors.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="constructors">The constructors to document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="constructors"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes constructors of a type. Regardless of the number of constructors, it creates a single documentation page.
        /// </remarks>
        protected virtual void GenerateDocumentationForConstructors(IDocumentWriterFactory writerFactory, IReadOnlyList<ConstructorModel> constructors)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (constructors is null)
                throw new ArgumentNullException(nameof(constructors));

            if (constructors.Count == 0)
                return;

            using var writer = writerFactory.CreateWriter(constructors[0]);
            if (constructors.Count == 1)
                Renderer.RenderConstructor(writer, constructors[0]);
            else
                Renderer.RenderConstructorOverloads(writer, new OverloadCollection<ConstructorModel>(constructors));
        }

        /// <summary>
        /// Generates documentation pages for the specified fields.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="fields">The fields to document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="fields"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes fields, generating documentation pages for each field in the collection.
        /// </remarks>
        protected virtual void GenerateDocumentationForFields(IDocumentWriterFactory writerFactory, IEnumerable<FieldModel> fields)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (fields is null)
                throw new ArgumentNullException(nameof(fields));

            foreach (var field in fields)
            {
                using var writer = writerFactory.CreateWriter(field);
                Renderer.RenderField(writer, field);
            }
        }

        /// <summary>
        /// Generates documentation pages for the specified events.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="events">The events to document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="events"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes events, generating documentation pages for each event in the collection.
        /// </remarks>
        protected virtual void GenerateDocumentationForEvents(IDocumentWriterFactory writerFactory, IEnumerable<EventModel> events)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (events is null)
                throw new ArgumentNullException(nameof(events));

            foreach (var theEvent in events)
            {
                using var writer = writerFactory.CreateWriter(theEvent);
                Renderer.RenderEvent(writer, theEvent);
            }
        }

        /// <summary>
        /// Generates documentation pages for the specified properties.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="properties">The properties to document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="properties"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes properties and documentation pages for each property or set of property overloads in the collection.
        /// </remarks>
        protected virtual void GenerateDocumentationForProperties(IDocumentWriterFactory writerFactory, IEnumerable<PropertyModel> properties)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (properties is null)
                throw new ArgumentNullException(nameof(properties));

            foreach (var propertyGroup in properties.GroupBy(static p => p.Metadata.Name))
            {
                var property = propertyGroup.First();
                using var writer = writerFactory.CreateWriter(property);

                if (propertyGroup.Skip(1).Any())
                    Renderer.RenderPropertyOverloads(writer, new OverloadCollection<PropertyModel>(propertyGroup));
                else
                    Renderer.RenderProperty(writer, property);
            }
        }

        /// <summary>
        /// Generates documentation pages for the specified methods.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="methods">The methods to document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="methods"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes methods and generates documentation pages for each method or set of method overloads in the collection.
        /// </remarks>
        protected virtual void GenerateDocumentationForMethods(IDocumentWriterFactory writerFactory, IEnumerable<MethodModel> methods)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (methods is null)
                throw new ArgumentNullException(nameof(methods));

            foreach (var methodGroup in methods.GroupBy(static m => m.Metadata.Name))
            {
                var method = methodGroup.First();
                using var writer = writerFactory.CreateWriter(method);

                if (methodGroup.Skip(1).Any())
                    Renderer.RenderMethodOverloads(writer, new OverloadCollection<MethodModel>(methodGroup));
                else
                    Renderer.RenderMethod(writer, method);
            }
        }

        /// <summary>
        /// Generates documentation pages for the specified operators.
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="operators">The operators to document.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="operators"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method processes operators and generates documentation pages for each operator or set of operator methods in the collection.
        /// </remarks>
        protected virtual void GenerateDocumentationForOperators(IDocumentWriterFactory writerFactory, IEnumerable<OperatorModel> operators)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (operators is null)
                throw new ArgumentNullException(nameof(operators));

            foreach (var operatorGroup in operators.GroupBy(static m => m.Metadata.Name))
            {
                var operatorMethod = operatorGroup.First();
                using var writer = writerFactory.CreateWriter(operatorMethod);

                if (operatorGroup.Skip(1).Any())
                    Renderer.RenderOperatorOverloads(writer, new OverloadCollection<OperatorModel>(operatorGroup));
                else
                    Renderer.RenderOperator(writer, operatorMethod);
            }
        }

        /// <summary>
        /// Generates documentation pages for the members of the specified composite type (class or struct).
        /// </summary>
        /// <param name="writerFactory">The factory for creating rendering contexts.</param>
        /// <param name="compositeType">The composite type whose members are to be documented.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writerFactory"/> or <paramref name="compositeType"/> is <see langword="null"/>.</exception>
        protected virtual void GenerateDocumentationForCompositeTypeMembers(IDocumentWriterFactory writerFactory, CompositeTypeModel compositeType)
        {
            if (writerFactory is null)
                throw new ArgumentNullException(nameof(writerFactory));
            if (compositeType is null)
                throw new ArgumentNullException(nameof(compositeType));

            GenerateDocumentationForConstructors(writerFactory, compositeType.Constructors);
            GenerateDocumentationForFields(writerFactory, compositeType.Fields);
            GenerateDocumentationForProperties(writerFactory, compositeType.Properties.Concat(compositeType.ExplicitInterfaceMembers.OfType<PropertyModel>()));
            GenerateDocumentationForMethods(writerFactory, compositeType.Methods.Concat(compositeType.ExplicitInterfaceMembers.OfType<MethodModel>()));
            GenerateDocumentationForEvents(writerFactory, compositeType.Events.Concat(compositeType.ExplicitInterfaceMembers.OfType<EventModel>()));
            GenerateDocumentationForOperators(writerFactory, compositeType.Operators);
        }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Models
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Collections;
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a documentation model for a composite .NET type that can have fields and nested types.
    /// </summary>
    /// <remarks>
    /// The <see cref="CompositeTypeModel"/> class is an abstract base class for class and struct types that can contain
    /// fields, constructors, properties, methods, events, operators, and nested types.
    /// </remarks>
    public abstract class CompositeTypeModel : TypeModel<ICompositeType>
    {
        private readonly Lazy<List<FieldModel>> fields;
        private readonly Lazy<List<ConstructorModel>> constructors;
        private readonly Lazy<List<PropertyModel>> properties;
        private readonly Lazy<List<MethodModel>> methods;
        private readonly Lazy<List<EventModel>> events;
        private readonly Lazy<List<OperatorModel>> operators;
        private readonly Lazy<List<TypeMemberModel>> explicitInterfaceMembers;
        private readonly Lazy<TypeCollection> nestedTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeTypeModel"/> class.
        /// </summary>
        /// <param name="declaringEntity">The object that declares the type, which is either an <see cref="AssemblyModel"/> for top-level types or a <see cref="TypeModel"/> for nested types.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="declaringEntity"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is not a relevant instance of <see cref="AssemblyModel"/> or <see cref="TypeModel"/>.</exception>
        protected CompositeTypeModel(object declaringEntity, ICompositeType type)
            : base(declaringEntity, type)
        {
            fields = new(() => [.. Metadata.Fields.Select(f => new FieldModel(this, f))]);
            constructors = new(() => [.. Metadata.Constructors.Select(c => new ConstructorModel(this, c))]);
            properties = new(() => [.. Metadata.Properties.Select(p => new PropertyModel(this, p))]);
            methods = new(() => [.. Metadata.Methods.Select(m => new MethodModel(this, m))]);
            events = new(() => [.. Metadata.Events.Select(e => new EventModel(this, e))]);
            operators = new(() => [.. Metadata.Operators.Select(o => new OperatorModel(this, o))]);

            explicitInterfaceMembers = new(() => [
                .. Metadata.ExplicitInterfaceProperties.Select(p => new PropertyModel(this, p)),
                .. Metadata.ExplicitInterfaceMethods.Select(m => new MethodModel(this, m)),
                .. Metadata.ExplicitInterfaceEvents.Select(e => new EventModel(this, e))
            ]);

            nestedTypes = new(() => type.HasNestedTypes
                ? new(Metadata.NestedTypes.Select(Assembly.FindMember).OfType<TypeModel>())
                : TypeCollection.Empty);
        }

        /// <summary>
        /// Gets the types declared directly by this type and visible within its scope.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="TypeModel"/> representing the types declared directly by this type.
        /// The types in the collection are ordered by their names and categorized by their kinds.
        /// </value>
        public IReadOnlyTypeCollection NestedTypes => nestedTypes.Value;

        /// <summary>
        /// Gets the fields declared by the type and visible within its scope.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="FieldModel"/> representing the fields declared by the type.
        /// The fields in the collection are ordered by their names.
        /// </value>
        public IReadOnlyList<FieldModel> Fields => fields.Value;

        /// <summary>
        /// Gets the constructors declared by the type and visible within its scope.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="ConstructorModel"/> representing the constructors declared by the type.
        /// The constructors in the collection are ordered by their number of parameters.
        /// </value>
        /// <remarks>
        /// If the type only has a default public constructor without any documentation, the constructor is considered implicit
        /// and the collection will be empty.
        /// </remarks>
        public IReadOnlyList<ConstructorModel> Constructors => constructors.Value;

        /// <summary>
        /// Gets the properties declared by the type and visible within its scope.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="PropertyModel"/> representing the properties declared by the type.
        /// The properties in the collection  are ordered by their names and number of index parameters.
        /// </value>
        public IReadOnlyList<PropertyModel> Properties => properties.Value;

        /// <summary>
        /// Gets the methods declared by the type and visible within its scope.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="MethodModel"/> representing the methods declared by the type.
        /// The methods in the collection are ordered by their names and number of parameters.
        /// </value>
        public IReadOnlyList<MethodModel> Methods => methods.Value;

        /// <summary>
        /// Gets the events declared by the type and visible within its scope.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="EventModel"/> representing the events declared by the type.
        /// The events in the collection are ordered by their names.
        /// </value>
        public IReadOnlyList<EventModel> Events => events.Value;

        /// <summary>
        /// Gets the operators declared by the type and visible within its scope.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="OperatorModel"/> representing the operators declared by the type.
        /// The operators in the collection are ordered by their names and number of parameters.
        /// </value>
        public IReadOnlyList<OperatorModel> Operators => operators.Value;

        /// <summary>
        /// Gets the explicit interface members implemented by the type.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="TypeMemberModel"/> representing the explicit interface members implemented by the type.
        /// </value>
        public IReadOnlyList<TypeMemberModel> ExplicitInterfaceMembers => explicitInterfaceMembers.Value;

        /// <inheritdoc/>
        public override IEnumerable<TypeMemberModel> Members => base.Members
            .Concat(Constructors)
            .Concat(Methods)
            .Concat(Properties)
            .Concat(Events)
            .Concat(Fields)
            .Concat(Operators)
            .Concat(ExplicitInterfaceMembers);

        /// <inheritdoc/>
        public override MemberModel? FindMember(IMember member)
        {
            if (member is null || !ReferenceEquals(Metadata, member.DeclaringType))
                return null;

            return member switch
            {
                IVirtualTypeMember { IsExplicitInterfaceImplementation: true } => ExplicitInterfaceMembers.FirstOrDefault(m => ReferenceEquals(m.Metadata, member)),
                IType => NestedTypes.FirstOrDefault(t => ReferenceEquals(t.Metadata, member)),
                IField => Fields.FirstOrDefault(f => ReferenceEquals(f.Metadata, member)),
                IConstructor => Constructors.FirstOrDefault(c => ReferenceEquals(c.Metadata, member)),
                IProperty => Properties.FirstOrDefault(p => ReferenceEquals(p.Metadata, member)),
                IMethod => Methods.FirstOrDefault(m => ReferenceEquals(m.Metadata, member)),
                IEvent => Events.FirstOrDefault(e => ReferenceEquals(e.Metadata, member)),
                IOperator => Operators.FirstOrDefault(o => ReferenceEquals(o.Metadata, member)),
                _ => null
            };
        }
    }

    /// <summary>
    /// Represents a documentation model for a composite .NET type of a specific kind that can have fields and nested types.
    /// </summary>
    /// <typeparam name="T">The type of the reflection metadata, which must implement <see cref="ICompositeType"/>.</typeparam>
    public abstract class CompositeTypeModel<T> : CompositeTypeModel
        where T : class, ICompositeType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeTypeModel{T}"/> class.
        /// </summary>
        /// <param name="declaringEntity">The object that declares the type, which is either an <see cref="AssemblyModel"/> for top-level types or a <see cref="TypeModel"/> for nested types.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="declaringEntity"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringEntity"/> is not a relevant instance of <see cref="AssemblyModel"/> or <see cref="TypeModel"/>.</exception>
        protected CompositeTypeModel(object declaringEntity, T type)
            : base(declaringEntity, type)
        {
        }

        /// <summary>
        /// Gets the reflection metadata for the type.
        /// </summary>
        /// <value>
        /// The reflection metadata object that provides detailed information about the type.
        /// </value>
        public new T Metadata => (T)base.Metadata;
    }
}

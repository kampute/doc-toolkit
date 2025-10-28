// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Models
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a documentation model for a .NET interface.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    public class InterfaceModel : TypeModel<IInterfaceType>
    {
        private readonly Lazy<List<PropertyModel>> properties;
        private readonly Lazy<List<MethodModel>> methods;
        private readonly Lazy<List<EventModel>> events;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassModel"/> class.
        /// </summary>
        /// <param name="declaryingEntity">The object that declares the type, which is either an <see cref="AssemblyModel"/> for top-level types or a <see cref="TypeModel"/> for nested types.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="declaryingEntity"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaryingEntity"/> is not an instance of <see cref="AssemblyModel"/> or <see cref="TypeModel"/>.</exception>
        protected InterfaceModel(object declaryingEntity, IInterfaceType type)
            : base(declaryingEntity, type)
        {
            properties = new(() => [.. type.Properties.Select(p => new PropertyModel(this, p))]);
            methods = new(() => [.. type.Methods.Select(m => new MethodModel(this, m))]);
            events = new(() => [.. type.Events.Select(e => new EventModel(this, e))]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceModel"/> class as a top-level type.
        /// </summary>
        /// <param name="assembly">The assembly that contains the type.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="assembly"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not a top-level type of <paramref name="assembly"/>.</exception>
        public InterfaceModel(AssemblyModel assembly, IInterfaceType type)
            : this((object)assembly ?? throw new ArgumentNullException(nameof(assembly)), type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceModel"/> class as a nested type.
        /// </summary>
        /// <param name="declaringType">The type that declares the type.</param>
        /// <param name="type">The metadata of the type represented by this instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is not directedly nested within <paramref name="declaringType"/>.</exception>
        public InterfaceModel(TypeModel declaringType, IInterfaceType type)
            : this((object)declaringType ?? throw new ArgumentNullException(nameof(declaringType)), type)
        {
        }

        /// <summary>
        /// Gets the type of the documentation model.
        /// </summary>
        /// <value>
        /// The type of the documentation model, which is always <see cref="DocumentationModelType.Interface"/> for this model.
        /// </value>
        public override DocumentationModelType ModelType => DocumentationModelType.Interface;

        /// <summary>
        /// Gets the properties declared by the interface.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="PropertyModel"/> representing the properties declared by the interface.
        /// The properties in the collection are ordered by their names and number of index parameters.
        /// </value>
        public IReadOnlyList<PropertyModel> Properties => properties.Value;

        /// <summary>
        /// Gets the methods declared by the interface.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="MethodModel"/> representing the methods declared by the interface.
        /// The methods in the collection are ordered by their names and number of parameters.
        /// </value>
        public IReadOnlyList<MethodModel> Methods => methods.Value;

        /// <summary>
        /// Gets the events declared by the interface.
        /// </summary>
        /// <value>
        /// A read-only collection of <see cref="EventModel"/> representing the events declared by the interface.
        /// The events in the collection are ordered by their names.
        /// </value>
        public IReadOnlyList<EventModel> Events => events.Value;

        /// <inheritdoc/>
        public override IEnumerable<TypeMemberModel> Members => base.Members.Concat(Methods).Concat(Properties).Concat(Events);

        /// <inheritdoc/>
        public override MemberModel? FindMember(IMember member)
        {
            if (member is null || !ReferenceEquals(Metadata, member.DeclaringType))
                return null;

            return member switch
            {
                IProperty => Properties.FirstOrDefault(p => ReferenceEquals(p.Metadata, member)),
                IMethod => Methods.FirstOrDefault(m => ReferenceEquals(m.Metadata, member)),
                IEvent => Events.FirstOrDefault(e => ReferenceEquals(e.Metadata, member)),
                _ => null
            };
        }
    }
}

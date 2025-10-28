// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// An adapter that wraps a <see cref="EventInfo"/> and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="EventInfo"/> and the metadata representation
    /// defined by the <see cref="IEvent"/> interface. It provides access to event-level information regardless of whether
    /// the assembly containing the event's type was loaded via Common Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class EventAdapter : VirtualTypeMemberAdapter<EventInfo>, IEvent
    {
        private readonly Lazy<IType> eventHandlerType;
        private readonly Lazy<IMethod> addMethod;
        private readonly Lazy<IMethod> removeMethod;
        private readonly Lazy<IMethod?> raiseMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventAdapter"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the event.</param>
        /// <param name="eventInfo">The event to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="eventInfo"/> is not an event of <paramref name="declaringType"/>.</exception>
        public EventAdapter(IType declaringType, EventInfo eventInfo)
            : base(declaringType, eventInfo)
        {
            eventHandlerType = new(GetEventHandlerType);
            addMethod = new(GetAdderMethod);
            removeMethod = new(GetRemoverMethod);
            raiseMethod = new(GetRaiserMethod);
        }

        /// <inheritdoc/>
        public IType Type => eventHandlerType.Value;

        /// <inheritdoc/>
        public IMethod AddMethod => addMethod.Value;

        /// <inheritdoc/>
        public IMethod RemoveMethod => removeMethod.Value;

        /// <inheritdoc/>
        public IMethod? RaiseMethod => raiseMethod.Value;

        /// <inheritdoc/>
        public override bool IsSpecialName => Reflection.IsSpecialName;

        /// <inheritdoc/>
        public override bool IsStatic => AddMethod.IsStatic;

        /// <inheritdoc/>
        public override bool IsUnsafe => AddMethod.IsUnsafe;

        /// <inheritdoc/>
        public IEvent? OverriddenEvent => (IEvent?)OverriddenMember;

        /// <inheritdoc/>
        public IEvent? ImplementedEvent => (IEvent?)ImplementedMember;

        /// <inheritdoc/>
        protected override MemberVisibility GetMemberVisibility() => AddMethod.Visibility;

        /// <inheritdoc/>
        protected override MemberVirtuality GetMemberVirtuality() => AddMethod.Virtuality;

        /// <inheritdoc/>
        protected sealed override (char, string) GetCodeReferenceParts()
        {
            var signature = Name;
            if (signature.Contains('.'))
                signature = signature.Replace('.', '#').Replace('<', '{').Replace('>', '}');

            return ('E', signature);
        }

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindOverriddenMember()
        {
            if (IsStatic || IsExplicitInterfaceImplementation || Virtuality == MemberVirtuality.None)
                return null;

            for (var baseType = DeclaringType.BaseType; baseType is not null; baseType = baseType.BaseType)
            {
                if (baseType.IsConstructedGenericType)
                    baseType = (IClassType)baseType.GenericTypeDefinition!;

                var candidate = baseType.Events.FindByName(Name);
                if (candidate is not null && !candidate.IsStatic && candidate.IsOverridable && HasMatchingSignature(candidate))
                    return candidate;
            }

            return null;
        }

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindImplementedMember()
        {
            if (IsPublic && !IsStatic)
            {
                return ((IInterfaceCapableType)DeclaringType).Interfaces
                    .Select(i => i.Events.FindByName(Name))
                    .FirstOrDefault(HasMatchingSignature);
            }

            if (IsExplicitInterfaceImplementation)
            {
                var (interfaceFullName, memberName) = AdapterHelper.DecodeExplicitName(Name);

                return ((IInterfaceCapableType)DeclaringType)
                    .Interfaces.FindByFullName(interfaceFullName)?
                    .Events.FindByName(memberName);
            }

            return null;
        }

        /// <inheritdoc/>
        protected override IVirtualTypeMember? FindGenericDefinition()
        {
            return DeclaringType is IGenericCapableType { IsConstructedGenericType: true, GenericTypeDefinition: IWithEvents typeDef }
                ? typeDef.Events.FindByName(Name)
                : (IVirtualTypeMember?)null;
        }

        /// <summary>
        /// Determines whether the given eent can be considered a base declaration of this event.
        /// </summary>
        /// <param name="baseCandidate">The other event to compare against.</param>
        /// <returns><see langword="true"/> if the given event can be considered a base declaration; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method compares only the event handler types of the events. It assumes that the caller has already verified other necessary conditions,
        /// such as matching names.
        /// </remarks>
        protected virtual bool HasMatchingSignature(IEvent? baseCandidate) => baseCandidate is not null && baseCandidate.Type.IsSubstitutableBy(Type);

        /// <summary>
        /// Retrieves the event handler type for the event.
        /// </summary>
        /// <returns>An <see cref="IType"/> representing the event handler type.</returns>
        protected virtual IType GetEventHandlerType() => MetadataProvider.GetMetadata(Reflection.EventHandlerType!);

        /// <summary>
        /// Retrieves the add method for the event.
        /// </summary>
        /// <returns>An <see cref="IMethod"/> representing the add method.</returns>
        protected virtual IMethod GetAdderMethod() => (IMethod)Assembly.Repository.GetMethodMetadata(Reflection.AddMethod!);

        /// <summary>
        /// Retrieves the remove method for the event.
        /// </summary>
        /// <returns>An <see cref="IMethod"/> representing the remove method.</returns>
        protected virtual IMethod GetRemoverMethod() => (IMethod)Assembly.Repository.GetMethodMetadata(Reflection.RemoveMethod!);

        /// <summary>
        /// Retrieves the raise method for the event, if it exists.
        /// </summary>
        /// <returns>An <see cref="IMethod"/> representing the raise method, or <see langword="null"/> if none exists.</returns>
        protected virtual IMethod? GetRaiserMethod() => Reflection.RaiseMethod is MethodInfo raiser ? (IMethod)Assembly.Repository.GetMethodMetadata(raiser) : null;
    }
}

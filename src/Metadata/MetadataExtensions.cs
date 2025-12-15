// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides extension methods for working with metadata elements.
    /// </summary>
    public static class MetadataExtensions
    {
        /// <summary>
        /// Determines whether the type's requirements can be satisfied by the specified candidate type.
        /// </summary>
        /// <param name="type">The type whose requirements must be satisfied.</param>
        /// <param name="candidate">The candidate type to check for satisfaction.</param>
        /// <returns><see langword="true"/> if the candidate type satisfies the requirements of the target type; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method evaluates whether a candidate type can be used in place of another type in contexts such as method overrides
        /// and interface implementations.
        /// <note type="caution" title="Important">
        /// This method is designed specifically for signature matching in override and implementation scenarios. It does not perform
        /// general type compatibility checks such as inheritance or interface implementation. Use <see cref="IType.IsAssignableFrom(IType)"/>
        /// for general type compatibility checks.
        /// </note>
        /// </remarks>
        /// <seealso cref="IType.IsAssignableFrom(IType)"/>
        /// <seealso cref="ITypeParameter.IsSubstitutableBy(IType)"/>
        /// <seealso cref="IParameter.IsSatisfiableBy(IParameter)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSatisfiableBy(this IType type, IType candidate)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type is ITypeParameter typeParameter
                ? typeParameter.IsSubstitutableBy(candidate)
                : type.Equals(candidate);
        }

        /// <summary>
        /// Retrieves the member that this member directly inherits from.
        /// </summary>
        /// <param name="member">The member whose inherited member is to be retrieved.</param>
        /// <returns>The inherited member, or <see langword="null"/> if there is no inherited member.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method is particularly useful for resolving inherited documentation or attributes of members.
        /// <para>
        /// Based on the type of member, the following rules apply:
        /// <list type="bullet">
        ///   <item>constructed generic types return their generic type definition</item>
        ///   <item>generic type definitions and non-generic types return their base type</item>
        ///   <item>constructors return the base type constructor with matching signature</item>
        ///   <item>methods, properties, and events return the overridden base member or implemented interface member</item>
        ///   <item>other member types return <see langword="null"/></item>
        /// </list>
        /// </para>
        /// <note type="hint" title="Hint">
        /// If having a member definition is important for your use case, you may want to consider using <see cref="GetMemberDefinition(IMember)"/>
        /// after retrieving the inherited member to ensure you have the canonical form of the member.
        /// </note>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMember? GetInheritedMember(this IMember member) => member switch
        {
            null => throw new ArgumentNullException(nameof(member)),
            IGenericCapableType { IsConstructedGenericType: true } genericType => genericType.GenericTypeDefinition,
            IType type => type.BaseType,
            IConstructor constructor => constructor.BaseConstructor,
            IVirtualTypeMember typeMember => typeMember.OverriddenMember ?? typeMember.ImplementedMember,
            _ => null
        };

        /// <summary>
        /// Retrieves the definition of the member, resolving any constructed or decorated forms to their underlying definitions.
        /// </summary>
        /// <param name="member">The member whose definition is to be retrieved.</param>
        /// <returns>The member definition or the member itself if it is already a definition.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method is particularly useful for resolving the canonical form of members for comparison or documentation purposes.
        /// <para>
        /// Based on the type of member, the following rules apply:
        /// <list type="bullet">
        ///   <item>constructed generic types return their generic type definition</item>
        ///   <item>virtual type members declared in constructed generic types return their generic member definition</item>
        ///   <item>array, pointer, by-ref, and nullable types return their unwrapped element type</item>
        ///   <item>other member types return themselves</item>
        /// </list>
        /// </para>
        /// This method is typically used in combination with <see cref="GetInheritedMember(IMember)"/> to navigate inheritance hierarchies
        /// while ensuring that the members being compared or documented are in their canonical forms.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IMember GetMemberDefinition(this IMember member) => member switch
        {
            null => throw new ArgumentNullException(nameof(member)),
            IVirtualTypeMember typeMember when typeMember.DeclaringType is IGenericCapableType { IsConstructedGenericType: true } => typeMember.GenericMemberDefinition!,
            IGenericCapableType { IsConstructedGenericType: true } genericType => genericType.GenericTypeDefinition!,
            ITypeDecorator typeDecorator => typeDecorator.Unwrap(),
            _ => member,
        };

        /// <summary>
        /// Gets all members directly declared by the type, excluding nested types.
        /// </summary>
        /// <param name="type">The type whose members are to be retrieved.</param>
        /// <returns>An enumerable collection of members declared by the type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method retrieves fields, properties, methods, events, constructors, operators, and explicit interface members
        /// directly declared by the type. Private and inherited members and nested types are excluded.
        /// <para>
        /// This method is implemented using deferred execution. The immediate return value is an object that stores all the
        /// information required to perform the action.
        /// </para>
        /// </remarks>
        public static IEnumerable<ITypeMember> GetMembers(this IType type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return EnumerateMembers(type);

            static IEnumerable<ITypeMember> EnumerateMembers(IType type)
            {
                if (type is IWithConstructors { HasConstructors: true } typeWithConstructors)
                {
                    foreach (var ctor in typeWithConstructors.Constructors)
                        yield return ctor;
                }

                if (type is IWithMethods { HasMethods: true } typeWithMethods)
                {
                    foreach (var method in typeWithMethods.Methods)
                        yield return method;
                }

                if (type is IWithProperties { HasProperties: true } typeWithProperties)
                {
                    foreach (var prop in typeWithProperties.Properties)
                        yield return prop;
                }

                if (type is IWithEvents { HasEvents: true } typeWithEvents)
                {
                    foreach (var evt in typeWithEvents.Events)
                        yield return evt;
                }

                if (type is IWithFields { HasFields: true } typeWithFields)
                {
                    foreach (var field in typeWithFields.Fields)
                        yield return field;
                }

                if (type is IWithOperators { HasOperators: true } typeWithOperators)
                {
                    foreach (var op in typeWithOperators.Operators)
                        yield return op;
                }

                if (type is IWithExplicitInterfaceMembers { HasExplicitInterfaceMembers: true } typeWithExplicitInterfaceMembers)
                {
                    foreach (var explicitMember in typeWithExplicitInterfaceMembers.ExplicitInterfaceMembers)
                        yield return explicitMember;
                }
            }
        }

        /// <summary>
        /// Gets all members of the type, including its nested types and their members at all nesting levels.
        /// </summary>
        /// <param name="type">The type whose members are to be retrieved.</param>
        /// <returns>An enumerable collection of members from the type and its nested types.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method retrieves all members directly declared by the type and includes nested types and their members at all nesting levels.
        /// Private and inherited members are excluded.
        /// <para>
        /// This method is implemented using deferred execution. The immediate return value is an object that stores all the
        /// information required to perform the action.
        /// </para>
        /// </remarks>
        public static IEnumerable<IMember> GetMembersIncludingNested(this IType type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return EnumerateMembers(type);

            static IEnumerable<IMember> EnumerateMembers(IType type)
            {
                foreach (var member in type.GetMembers())
                    yield return member;

                if (type is IWithNestedTypes typeWithNestedTypes)
                {
                    foreach (var nestedType in typeWithNestedTypes.NestedTypes)
                    {
                        yield return nestedType;
                        foreach (var nestedMember in EnumerateMembers(nestedType))
                            yield return nestedMember;
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to get the type parameters declared directly by the specified member if it is generic.
        /// </summary>
        /// <param name="member">The member to check for type parameters.</param>
        /// <param name="typeParameters">When this method returns, contains the type parameters declared by the member if it is generic; otherwise, an empty collection.</param>
        /// <returns><see langword="true"/> if the member is generic; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> is <see langword="null"/>.</exception>
        public static bool TryGetOwnTypeParameters(this IMember member, out IEnumerable<ITypeParameter> typeParameters)
        {
            switch (member)
            {
                case null:
                    throw new ArgumentNullException(nameof(member));
                case IGenericCapableType { IsGenericType: true } genericType:
                    var (start, count) = genericType.OwnGenericParameterRange;
                    typeParameters = genericType.TypeParameters.Skip(start).Take(count);
                    return true;
                case IMethod { IsGenericMethod: true } genericMethod:
                    typeParameters = genericMethod.TypeParameters;
                    return true;
                default:
                    typeParameters = [];
                    return false;

            }
        }
    }
}
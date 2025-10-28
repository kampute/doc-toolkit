// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Capabilities;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides methods for validating code reference (cref) strings used in XML documentation comments.
    /// </summary>
    public static class CodeReference
    {
        /// <summary>
        /// Determines whether the specified code reference string is valid.
        /// </summary>
        /// <param name="cref">The code reference string to validate.</param>
        /// <returns><see langword="true"/> if the code reference string is valid; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// A valid code reference string must:
        /// <list type="bullet">
        ///   <item>Be at least 3 characters long.</item>
        ///   <item>Start with a valid code reference type character ('T', 'F', 'P', 'M', 'E', or 'N').</item>
        ///   <item>Contain a colon (':') at index 1.</item>
        /// </list>
        /// <note type="caution" title="Caution">
        /// This method performs only basic validation of the code reference format and does not verify the existence
        /// of the referenced code element.
        /// </note>
        /// </remarks>
        public static bool IsValid(string cref)
        {
            return cref is not null
                && cref.Length > 2
                && cref[1] is ':'
                && cref[0] is 'T' or 'F' or 'P' or 'M' or 'E' or 'N';
        }

        /// <summary>
        /// Determines whether the specified code reference string refers to a namespace.
        /// </summary>
        /// <param name="cref">The code reference string to check.</param>
        /// <returns><see langword="true"/> if the code reference string refers to a namespace; otherwise, <see langword="false"/>.</returns>
        public static bool IsNamespace(string cref)
        {
            return cref is not null
                && cref.Length > 2
                && cref[0] is 'N'
                && cref[1] is ':';
        }

        /// <summary>
        /// Determines whether the specified code reference string refers to a member.
        /// </summary>
        /// <param name="cref">The code reference string to check.</param>
        /// <returns><see langword="true"/> if the code reference string refers to a type or type member; otherwise, <see langword="false"/>.</returns>
        public static bool IsMember(string cref)
        {
            return cref is not null
                && cref.Length > 2
                && cref[0] is 'T' or 'M' or 'P' or 'F' or 'E'
                && cref[1] is ':';
        }

        /// <summary>
        /// Resolves a code reference string to its corresponding member metadata.
        /// </summary>
        /// <param name="cref">The code reference string to resolve.</param>
        /// <returns>The member metadata if the code reference is valid and member is found; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cref"/> is <see langword="null"/>.</exception>
        public static IMember? ResolveMember(string cref)
        {
            if (cref is null)
                throw new ArgumentNullException(nameof(cref));

            if (cref.Length < 3 || cref[1] is not ':')
                return null;

            if (cref[0] is 'T')
                return MetadataProvider.FindTypeByFullName(cref[2..]);

            if (cref[0] is 'N')
                return null;

            if (!TryExtractTypeNameAndMemberCategory(cref, out var memberCategory, out var typeName))
                return null;

            var type = MetadataProvider.FindTypeByFullName(typeName);
            if (type is null)
                return null;

            IEnumerable<IMember> members = memberCategory switch
            {
                'M' => GetAllMethodLikeMembers(type),
                'P' => GetAllProperties(type),
                'E' => GetAllEvents(type),
                'F' when type is IWithFields t => t.Fields,
                _ => []
            };

            return members.FirstOrDefault(m => m.CodeReference == cref);
        }

        /// <summary>
        /// Tries to extract the type name and member category from a code reference string.
        /// </summary>
        /// <param name="cref">The code reference string.</param>
        /// <param name="memberCategory">When this method returns, contains the extracted member category if successful; otherwise, '!' character.</param>
        /// <param name="typeName">When this method returns, contains the extracted type name if successful; otherwise, an empty string.</param>
        /// <returns><see langword="true"/> if the extraction was successful; otherwise, <see langword="false"/>.</returns>
        private static bool TryExtractTypeNameAndMemberCategory(string cref, out char memberCategory, out string typeName)
        {
            var firstParenthesisIndex = cref.IndexOf('(', StringComparison.Ordinal);
            var nameEndIndex = firstParenthesisIndex > 0 ? firstParenthesisIndex : cref.Length;
            var lastDotIndex = cref.LastIndexOf('.', nameEndIndex - 1, nameEndIndex);
            if (lastDotIndex > 3)
            {
                memberCategory = cref[0];
                typeName = cref[2..lastDotIndex];
                return true;
            }

            memberCategory = '!';
            typeName = string.Empty;
            return false;
        }

        /// <summary>
        /// Gets all method-like members (constructors, methods, and operators) of the specified type.
        /// </summary>
        /// <param name="type">The type whose method-like members are to be retrieved.</param>
        /// <returns>An enumerable collection of method-like members.</returns>
        private static IEnumerable<ITypeMember> GetAllMethodLikeMembers(IType type)
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

            if (type is IWithOperators { HasOperators: true } typeWithOperators)
            {
                foreach (var op in typeWithOperators.Operators)
                    yield return op;
            }

            if (type is IWithExplicitInterfaceMembers { HasExplicitInterfaceMethods: true } typeWithExplicitInterfaceMethods)
            {
                foreach (var explicitMethod in typeWithExplicitInterfaceMethods.ExplicitInterfaceMethods)
                    yield return explicitMethod;
            }
        }

        /// <summary>
        /// Gets all properties of the specified type, including explicit interface implementations.
        /// </summary>
        /// <param name="type">The type whose properties are to be retrieved.</param>
        /// <returns>An enumerable collection of properties.</returns>
        private static IEnumerable<IProperty> GetAllProperties(IType type)
        {
            if (type is IWithProperties { HasProperties: true } typeWithProperties)
            {
                foreach (var property in typeWithProperties.Properties)
                    yield return property;
            }

            if (type is IWithExplicitInterfaceMembers { HasExplicitInterfaceProperties: true } typeWithExplicitInterfaceProperties)
            {
                foreach (var explicitProperty in typeWithExplicitInterfaceProperties.ExplicitInterfaceProperties)
                    yield return explicitProperty;
            }
        }

        /// <summary>
        /// Gets all events of the specified type, including explicit interface implementations.
        /// </summary>
        /// <param name="type">The type whose events are to be retrieved.</param>
        /// <returns>An enumerable collection of events.</returns>
        private static IEnumerable<IEvent> GetAllEvents(IType type)
        {
            if (type is IWithEvents { HasEvents: true } typeWithEvents)
            {
                foreach (var evt in typeWithEvents.Events)
                    yield return evt;
            }

            if (type is IWithExplicitInterfaceMembers { HasExplicitInterfaceEvents: true } typeWithExplicitInterfaceEvents)
            {
                foreach (var explicitEvent in typeWithExplicitInterfaceEvents.ExplicitInterfaceEvents)
                    yield return explicitEvent;
            }
        }
    }
}

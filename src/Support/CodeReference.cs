// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using Kampute.DocToolkit.Metadata;
    using System;

    /// <summary>
    /// Provides methods for validating code reference (cref) strings used in XML documentation comments.
    /// </summary>
    public static class CodeReference
    {
        /// <summary>
        /// Determines whether the specified XML documentation cref string is valid.
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
        /// Determines whether the specified XML documentation cref string refers to a namespace.
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
        /// Determines whether the specified XML documentation cref string refers to a member.
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
        /// Resolves a type or type member based on the specified XML documentation cref string.
        /// </summary>
        /// <param name="cref">The code reference that identifies the member to resolve.</param>
        /// <returns>The member metadata if the code reference is valid and member is found; otherwise, <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="cref"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method only resolves types and members of the types that are exported by the assemblies registered with the
        /// <see cref="MetadataProvider"/>.
        /// </remarks>
        public static IMember? ResolveMember(string cref)
        {
            if (cref is null)
                throw new ArgumentNullException(nameof(cref));

            if (cref.Length < 3 || cref[1] is not ':' || cref[0] is 'N')
                return null;

            if (cref[0] is 'T')
                return MetadataProvider.FindTypeByFullName(cref[2..]);

            if (cref[0] is ('M' or 'P' or 'F' or 'E') && TryExtractTypeName(cref, out var typeName))
                return MetadataProvider.FindTypeByFullName(typeName)?.ResolveMember(cref);

            return null;

            static bool TryExtractTypeName(string cref, out string typeFullName)
            {
                var firstParenthesisIndex = cref.IndexOf('(', StringComparison.Ordinal);
                var nameEndIndex = firstParenthesisIndex != -1 ? firstParenthesisIndex : cref.Length;
                var lastDotIndex = cref.LastIndexOf('.', nameEndIndex - 1, nameEndIndex);
                if (lastDotIndex > 3)
                {
                    typeFullName = cref[2..lastDotIndex];
                    return true;
                }

                typeFullName = string.Empty;
                return false;
            }
        }
    }
}

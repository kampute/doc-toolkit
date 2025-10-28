// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides extension methods to various types related to documentation URL resolution and addressing strategies.
    /// </summary>
    public static class RoutingExtensions
    {
        /// <summary>
        /// Attempts to retrieves the documentation URL for the specified code reference.
        /// </summary>
        /// <param name="urlProvider">The URL provider to use for resolving the URL.</param>
        /// <param name="cref">The code reference to resolve.</param>
        /// <param name="url">When this method returns, contains the URL of the code reference if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the URL of the code reference is found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="urlProvider"/> or <paramref name="cref"/> is <see langword="null"/>.</exception>
        public static bool TryGetUrlByCodeReference(this IApiDocUrlProvider urlProvider, string cref, [NotNullWhen(true)] out Uri? url)
        {
            if (urlProvider is null)
                throw new ArgumentNullException(nameof(urlProvider));
            if (cref is null)
                throw new ArgumentNullException(nameof(cref));

            if (CodeReference.IsNamespace(cref))
                return urlProvider.TryGetNamespaceUrl(cref[2..], out url);

            if (CodeReference.ResolveMember(cref) is IMember member)
                return urlProvider.TryGetMemberUrl(GetEffectiveMember(member), out url);

            url = null;
            return false;
        }

        /// <summary>
        /// Attempts to resolve the address of the documentation content for the specified code reference.
        /// </summary>
        /// <param name="strategy">The addressing strategy to use for resolving the address.</param>
        /// <param name="cref">The code reference to resolve.</param>
        /// <param name="address">When this method returns, contains the address of the documentation content for the specified code reference, or <see langword="null"/> if the address could not be resolved. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the address was successfully resolved; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> or <paramref name="cref"/> is <see langword="null"/>.</exception>
        public static bool TryResolveAddressByCodeReference(this IDocumentAddressingStrategy strategy, string cref, [NotNullWhen(true)] out IResourceAddress? address)
        {
            if (strategy is null)
                throw new ArgumentNullException(nameof(strategy));
            if (cref is null)
                throw new ArgumentNullException(nameof(cref));

            if (CodeReference.IsNamespace(cref))
                return strategy.TryResolveNamespaceAddress(cref[2..], out address);

            if (CodeReference.ResolveMember(cref) is IMember member)
                return strategy.TryResolveMemberAddress(member, out address);

            address = null;
            return false;
        }

        /// <summary>
        /// Gets the effective member for URL resolution.
        /// </summary>
        /// <param name="member">The member to evaluate.</param>
        /// <returns>The effective member for URL resolution.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IMember GetEffectiveMember(IMember member) => member is IField { IsEnumValue: true } ? member.DeclaringType! : member;
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using System;
    using System.Reflection;

    /// <summary>
    /// An abstract base class for adapters that wrap reflected type members and provide metadata access.
    /// </summary>
    /// <typeparam name="T">The type of the underlying member info.</typeparam>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="MemberInfo"/> and the metadata
    /// representation defined by the <see cref="ITypeMember"/> interface. It provides access to type member-level
    /// information regardless of whether the assembly containing the member's type was loaded via Common
    /// Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public abstract class TypeMemberAdapter<T> : MemberAdapter<T>, ITypeMember
        where T : MemberInfo
    {
        private string? codeReference;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMemberAdapter{T}"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type metadata of the member.</param>
        /// <param name="underlying">The underlying member info.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="underlying"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="declaringType"/> does not match the member's declaring type.</exception>
        protected TypeMemberAdapter(IType declaringType, T underlying)
            : base(underlying)
        {
            if (declaringType is null)
                throw new ArgumentNullException(nameof(declaringType));
            if (!declaringType.Represents(underlying.DeclaringType!))
                throw new ArgumentException("Declaring type does not match the member's declaring type.", nameof(declaringType));

            DeclaringType = declaringType;
        }

        /// <inheritdoc/>
        public override IAssembly Assembly => DeclaringType.Assembly;

        /// <inheritdoc/>
        public override string Namespace => DeclaringType.Namespace;

        /// <inheritdoc/>
        public override IType DeclaringType { get; }

        /// <inheritdoc/>
        public sealed override bool IsDirectDeclaration => DeclaringType.IsDirectDeclaration;

        /// <inheritdoc/>
        public sealed override string CodeReference => codeReference ??= ConstructCodeReference();

        /// <inheritdoc/>
        public override string ToString() => $"{DeclaringType.FullName}{Type.Delimiter}{Name}";

        /// <summary>
        /// Retrieves the code reference parts for this member.
        /// </summary>
        /// <returns>A tuple containing the member prefix character and the unqualified code reference string.</returns>
        protected abstract (char, string) GetCodeReferenceParts();

        /// <summary>
        /// Constructs the code reference string for this member.
        /// </summary>
        /// <returns>The constructed code reference string.</returns>
        private string ConstructCodeReference()
        {
            var (prefix, unqualifiedSignature) = GetCodeReferenceParts();
            return $"{prefix}:{DeclaringType.Signature}.{unqualifiedSignature}";
        }

        /// <inheritdoc/>
        protected override ICustomAttribute CreateAttributeMetadata(CustomAttributeData attribute)
            => Assembly.Repository.GetCustomAttributeMetadata(attribute, AttributeTarget.TypeMember);

        /// <inheritdoc/>
        IType? IMember.DeclaringType => DeclaringType;
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Capabilities;
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// An adapter that wraps a <see cref="ConstructorInfo"/> and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="ConstructorInfo"/> and the metadata
    /// representation defined by the <see cref="IConstructor"/> interface. It provides access to constructor-level
    /// information regardless of whether the assembly containing the constructor's type was loaded via Common Language
    /// Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class ConstructorAdapter : TypeMemberAdapter<ConstructorInfo>, IConstructor
    {
        private readonly Lazy<IReadOnlyList<IParameter>> parameters;
        private readonly Lazy<IConstructor?> baseConstructor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorAdapter"/> class.
        /// </summary>
        /// <param name="declaringType">The declaring type of the constructor.</param>
        /// <param name="constructor">The constructor to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringType"/> or <paramref name="constructor"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="constructor"/> is not a constructor of <paramref name="declaringType"/>.</exception>
        public ConstructorAdapter(IType declaringType, ConstructorInfo constructor)
            : base(declaringType, constructor)
        {
            parameters = new(() => [.. GetParameters()]);
            baseConstructor = new(FindBaseConstructor);
        }

        /// <inheritdoc/>
        public IReadOnlyList<IParameter> Parameters => parameters.Value;

        /// <inheritdoc/>
        public override bool IsStatic => Reflection.IsStatic;

        /// <inheritdoc/>
        public override bool IsSpecialName => Reflection.IsSpecialName;

        /// <inheritdoc/>
        public override bool IsUnsafe => Parameters.Any(static p => p.Type.IsUnsafe);

        /// <inheritdoc/>
        public IConstructor? BaseConstructor => baseConstructor.Value;

        /// <inheritdoc/>
        public virtual IEnumerable<IMember> Overloads => !IsStatic && DeclaringType is IWithConstructors withConstructors
             ? withConstructors.Constructors.Where(c => !ReferenceEquals(c, this) && !c.IsStatic) : [];

        /// <inheritdoc/>
        protected override MemberVisibility GetMemberVisibility()
        {
            if (Reflection.IsPublic)
                return MemberVisibility.Public;
            else if (Reflection.IsFamily)
                return MemberVisibility.Protected;
            else if (Reflection.IsAssembly)
                return MemberVisibility.Internal;
            else if (Reflection.IsFamilyAndAssembly)
                return MemberVisibility.PrivateProtected;
            else if (Reflection.IsFamilyOrAssembly)
                return MemberVisibility.ProtectedInternal;
            else
                return MemberVisibility.Private;
        }

        /// <summary>
        /// Finds the constructor in the base class that has the same signature as this constructor, if any.
        /// </summary>
        /// <returns>The matching base constructor, or <see langword="null"/> if none is found.</returns>
        protected virtual IConstructor? FindBaseConstructor()
        {
            return !IsStatic ? DeclaringType.BaseType?.Constructors.FirstOrDefault(HasMatchingSignature) : null;
        }

        /// <summary>
        /// Determines whether the given constructor can be considered a base declaration of this constructor.
        /// </summary>
        /// <param name="baseCandidate">The other constructor to compare against.</param>
        /// <returns><see langword="true"/> if the given constructor can be considered a base declaration; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method checks whether the candidate constructor has the same parameter signature as this constructor. It does not consider other factors
        /// such as accessibility or being declared as static or instance.
        /// </remarks>
        protected virtual bool HasMatchingSignature(IConstructor baseCandidate)
        {
            return baseCandidate is not null
                && baseCandidate.Parameters.Count == Parameters.Count
                && AdapterHelper.EquivalentParameters(baseCandidate.Parameters, Parameters);
        }

        /// <summary>
        /// Retrieves the parameters of the constructor.
        /// </summary>
        /// <returns>An enumerable collection of <see cref="IParameter"/> objects representing the parameters of the constructor.</returns>
        protected virtual IEnumerable<IParameter> GetParameters()
        {
            return Reflection.GetParameters().Select(Assembly.Repository.GetParameterMetadata);
        }

        /// <inheritdoc/>
        protected sealed override (char, string) GetCodeReferenceParts()
        {
            using var reusable = StringBuilderPool.Shared.GetBuilder();
            var sb = reusable.Builder;

            sb.Append(Name).Replace('.', '#');
            if (Parameters.Count > 0)
            {
                sb.Append('(');
                sb.AppendJoin(',', Parameters.Select(static p => p.Type.ParametricSignature));
                sb.Append(')');
            }

            return ('M', sb.ToString());
        }
    }
}
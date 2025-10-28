// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// An adapter that wraps a <see cref="ParameterInfo"/> and provides metadata access.
    /// </summary>
    /// <remarks>
    /// This class serves as a bridge between the reflection-based <see cref="ParameterInfo"/> and the metadata
    /// representation defined by the <see cref="IParameter"/> interface. It provides access to parameter-level
    /// information regardless of whether the assembly containing the parameter's member was loaded via Common
    /// Language Runtime (CLR) or Metadata Load Context (MLC).
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class ParameterAdapter : AttributeAwareMetadataAdapter<ParameterInfo>, IParameter
    {
        private readonly Lazy<IType> parameterType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterAdapter"/> class.
        /// </summary>
        /// <param name="member">The member that the parameter belongs to.</param>
        /// <param name="parameter">The runtime parameter to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="member"/> or <paramref name="parameter"/> is <see langword="null"/>.</exception>
        public ParameterAdapter(IMember member, ParameterInfo parameter)
            : base(parameter)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));

            parameterType = new(Reflection.ParameterType.GetMetadata);
        }

        /// <inheritdoc/>
        public IMember Member { get; }

        /// <inheritdoc/>
        public override string Name => Reflection.Name ?? string.Empty;

        /// <inheritdoc/>
        public virtual int Position => Reflection.Position;

        /// <inheritdoc/>
        public virtual IType Type => parameterType.Value;

        /// <inheritdoc/>
        public virtual ParameterReferenceKind ReferenceKind
        {
            get
            {
                if (!IsByRef)
                    return ParameterReferenceKind.None;

                if (Reflection.IsIn)
                    return ParameterReferenceKind.In;
                else if (Reflection.IsOut)
                    return ParameterReferenceKind.Out;
                else
                    return ParameterReferenceKind.Ref;
            }
        }

        /// <inheritdoc/>
        public virtual bool IsByRef => Reflection.ParameterType.IsByRef;

        /// <inheritdoc/>
        public virtual bool IsOptional => Reflection.IsOptional;

        /// <inheritdoc/>
        public virtual bool IsParameterArray => HasCustomAttribute("System.ParamArrayAttribute");

        /// <inheritdoc/>
        public virtual bool IsReturnParameter => Reflection.Position == -1;

        /// <inheritdoc/>
        public virtual bool HasDefaultValue => Reflection.HasDefaultValue;

        /// <inheritdoc/>
        public virtual object? DefaultValue => Reflection.RawDefaultValue;

        /// <inheritdoc/>
        public virtual bool IsSatisfiableBy(IParameter other)
        {
            return other is not null
                && Position == other.Position
                && ReferenceKind == other.ReferenceKind
                && Type.IsSubstitutableBy(other.Type);
        }

        /// <inheritdoc/>
        public virtual bool HasRequiredCustomModifier(string modifierFullName)
        {
            foreach (var modifier in Reflection.GetRequiredCustomModifiers())
            {
                if (modifier.FullName == modifierFullName)
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public virtual bool HasOptionalCustomModifier(string modifierFullName)
        {
            foreach (var modifier in Reflection.GetOptionalCustomModifiers())
            {
                if (modifier.FullName == modifierFullName)
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        protected override IEnumerable<CustomAttributeData> GetCustomAttributes() => Reflection.CustomAttributes;

        /// <inheritdoc/>
        protected override ICustomAttribute CreateAttributeMetadata(CustomAttributeData attribute)
            => Member.Assembly.Repository.GetCustomAttributeMetadata(attribute, IsReturnParameter ? AttributeTarget.ReturnParameter : AttributeTarget.Parameter);
    }
}

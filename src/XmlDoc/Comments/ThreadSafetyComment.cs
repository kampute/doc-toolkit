// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.XmlDoc.Comments
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    /// <summary>
    /// Represents the documentation content describing the thread safety of a class or struct.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    /// <remarks>
    /// The <see cref="ThreadSafetyComment"/> class represents the <c>&lt;threadSafety&gt;</c> XML documentation tag, which is
    /// used to provide information about the thread safety of a class or struct.
    /// <para>
    /// Thread safety information is separated into two distinct categories: static member thread safety and instance member
    /// thread safety. This distinction is important because a type might have thread-safe static members but non-thread-safe
    /// instance members, or vice versa.
    /// </para>
    /// </remarks>
    public class ThreadSafetyComment : Comment
    {
        /// <summary>
        /// Represents an empty thread safety comment.
        /// </summary>
        public static new readonly ThreadSafetyComment Empty = new(new("none"));

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafetyComment"/> struct.
        /// </summary>
        /// <param name="element">The XML element containing the thread safety description.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="element"/> is <see langword="null"/>.</exception>
        public ThreadSafetyComment(XElement element)
            : base(element)
        {
            IsInstanceSafe = bool.TryParse(Content.Attribute("instance")?.Value, out var instanceSafety) ? instanceSafety : null;
            IsStaticSafe = bool.TryParse(Content.Attribute("static")?.Value, out var staticSafety) ? staticSafety : null;
        }

        /// <summary>
        /// Gets a value indicating whether the comment is without thread-safety information.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the comment is without thread-safety information; otherwise, <see langword="false"/>.
        /// </value>
        public override bool IsEmpty => !IsInstanceSafe.HasValue && !IsStaticSafe.HasValue && base.IsEmpty;

        /// <summary>
        /// Gets a value indicating whether the static members of the class/struct are thread-safe.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if static members are thread-safe; <see langword="false"/> if not; <see langword="null"/> if unspecified.</value>
        public bool? IsStaticSafe { get; }

        /// <summary>
        /// Gets a value indicating whether the instance members of the class/struct are thread-safe.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if instance members are thread-safe; <see langword="false"/> if not; <see langword="null"/> if unspecified.
        /// </value>
        public bool? IsInstanceSafe { get; }

        /// <summary>
        /// Gets a value indicating whether both static and instance members of the class/struct are thread-safe.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if both static and instance members are thread-safe; <see langword="false"/> if not; <see langword="null"/> if any is unspecified.
        /// </value>
        public bool? IsSafe => IsStaticSafe.HasValue && IsInstanceSafe.HasValue ? IsStaticSafe.Value & IsInstanceSafe.Value : null;

        /// <summary>
        /// Creates a thread safety comment from the specified XML element.
        /// </summary>
        /// <param name="element">The XML element containing the thread safety description.</param>
        /// <returns>A thread safety comment representing the specified XML element.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static new ThreadSafetyComment Create(XElement? element) => element is null ? Empty : new(element);
    }
}

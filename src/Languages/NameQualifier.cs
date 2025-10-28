// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Languages
{
    /// <summary>
    /// Defines the level of qualification for name of a member.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="NameQualifier"/> enum provides control over how member names are displayed in documentation.
    /// This affects code references, signatures, and links throughout the documentation system.
    /// </para>
    /// Different qualification levels are useful in different contexts:
    /// <list type="bullet">
    ///     <item><term><see cref="None"/></term><description>Used when the member's context is already clear, such as in class documentation</description></item>
    ///     <item><term><see cref="DeclaringType"/></term><description>Default for most cases, provides context without excessive verbosity</description></item>
    ///     <item><term><see cref="Full"/></term><description>Used for API references or when members from different namespaces need to be distinguished</description></item>
    /// </list>
    /// By controlling name qualification, documentation generators can balance between readability and precision in member
    /// references based on the context in which they appear.
    /// </remarks>
    public enum NameQualifier
    {
        /// <summary>
        /// No qualification is applied to the member's name.
        /// </summary>
        None,

        /// <summary>
        /// Qualifies the member's name with the type that declares it.
        /// </summary>
        DeclaringType,

        /// <summary>
        /// Qualifies the member's name with the namespace that contains it and the type that declares it.
        /// </summary>
        Full
    }
}

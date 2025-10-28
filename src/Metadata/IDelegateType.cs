// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;

    /// <summary>
    /// Defines a contract for accessing delegate-specific metadata.
    /// </summary>
    public interface IDelegateType : IGenericCapableType, IWithParameters, IWithReturnParameter
    {
        /// <summary>
        /// Gets the Invoke method of the delegate.
        /// </summary>
        /// <value>
        /// The method metadata for the delegate's Invoke method.
        /// </value>
        IMethod InvokeMethod { get; }
    }
}

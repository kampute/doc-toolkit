// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Metadata
{
    using Kampute.DocToolkit.Metadata.Capabilities;

    /// <summary>
    /// Defines a contract for accessing metadata common to methods and method operators.
    /// </summary>
    /// <remarks>
    /// This interface is implemented by both regular methods and operator methods, but unlike the .NET reflection API,
    /// it does not include constructors.
    /// </remarks>
    public interface IMethodBase : ITypeMember, IWithParameters, IWithReturnParameter, IWithOverloads
    {
    }
}

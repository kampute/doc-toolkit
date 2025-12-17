// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    /// <summary>
    /// Represents the type of a documentation model.
    /// </summary>
    public enum DocumentationModelType
    {
        /// <summary>
        /// The model represents a documentation topic.
        /// </summary>
        Topic,

        /// <summary>
        /// The model represents a namespace.
        /// </summary>
        Namespace,

        /// <summary>
        /// The model represents a class type.
        /// </summary>
        Class,

        /// <summary>
        /// The model represents a struct type.
        /// </summary>
        Struct,

        /// <summary>
        /// The model represents an interface type.
        /// </summary>
        Interface,

        /// <summary>
        /// The model represents an enum type.
        /// </summary>
        Enum,

        /// <summary>
        /// The model represents a delegate type.
        /// </summary>
        Delegate,

        /// <summary>
        /// The model represents a field member.
        /// </summary>
        Field,

        /// <summary>
        /// The model represents a constructor member or overloads.
        /// </summary>
        Constructor,

        /// <summary>
        /// The model represents a property member or overloads.
        /// </summary>
        Property,

        /// <summary>
        /// The model represents a method member or overloads.
        /// </summary>
        Method,

        /// <summary>
        /// The model represents an event member.
        /// </summary>
        Event,

        /// <summary>
        /// The model represents an operator member or overloads.
        /// </summary>
        Operator,

        /// <summary>
        /// The model represents an extension block.
        /// </summary>
        ExtensionBlock,
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Topics
{
    /// <summary>
    /// Defines a contract for a documentation topic that is based on a file system resource.
    /// </summary>
    /// <remarks>
    /// The <see cref="IFileBasedTopic"/> interface represents a topic that is stored in a file, such as
    /// a Markdown or HTML file.
    /// </remarks>
    public interface IFileBasedTopic
    {
        /// <summary>
        /// Gets the file path to the file that contains the topic content.
        /// </summary>
        /// <value>
        /// The file path to the file that contains the topic content.
        /// </value>
        string FilePath { get; }
    }
}

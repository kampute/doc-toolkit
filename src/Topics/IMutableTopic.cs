// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Topics
{
    /// <summary>
    /// Defines a contract for a mutable documentation topic that allows modification of the topic title and hierarchical
    /// relationships.
    /// </summary>
    /// <remarks>
    /// The <see cref="IMutableTopic"/> interface extends the <see cref="ITopic"/> interface by providing properties and
    /// methods to allow modification of the topic's title and its parent-child relationships.
    /// </remarks>
    public interface IMutableTopic : ITopic
    {
        /// <summary>
        /// Gets or sets the title of the topic.
        /// </summary>
        /// <value>
        /// The title of the topic, which is a human-readable string that describes the topic.
        /// </value>
        new string Title { get; set; }

        /// <summary>
        /// Gets or sets the parent topic for the current topic.
        /// </summary>
        /// <value>
        /// The parent topic to associate with the current topic, or <see langword="null"/> if the topic is not
        /// subordinate to any other topic.
        /// </value>
        new IMutableTopic? ParentTopic { get; set; }

        /// <summary>
        /// Adds a subtopic to the current topic.
        /// </summary>
        /// <param name="subtopic">The subtopic to add.</param>
        void AddSubtopic(IMutableTopic subtopic);

        /// <summary>
        /// Removes a subtopic from the current topic.
        /// </summary>
        /// <param name="subtopic">The subtopic to remove.</param>
        /// <returns>
        /// <see langword="true"/> if the subtopic was successfully removed; otherwise, <see langword="false"/>
        /// if the subtopic was not found.
        /// </returns>
        bool RemoveSubtopic(IMutableTopic subtopic);
    }
}

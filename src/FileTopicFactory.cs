// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Provides methods for creating appropriate <see cref="FileTopic"/> instances based on file extensions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="FileTopicFactory"/> class serves as a central registry for topic type selection,
    /// allowing the system to dynamically create appropriate <see cref="FileTopic"/> instances based on file extensions.
    /// </para>
    /// This class:
    /// <list type="bullet">
    ///   <item>Manages associations between file extensions and topic type implementations</item>
    ///   <item>Provides factory methods to create topic instances on demand</item>
    ///   <item>Supports registration of custom topic types for specific file formats</item>
    ///   <item>Allows for overriding default topic types with custom implementations</item>    /// </list>
    /// The system comes pre-configured with topic types for common documentation formats (HTML and Markdown),
    /// but applications can register additional topic types or replace the built-in ones as needed.
    /// </remarks>
    /// <seealso cref="FileTopic"/>
    public static class FileTopicFactory
    {
        static FileTopicFactory()
        {
            Register(static (id, filePath) => new HtmlFileTopic(id, filePath), FileExtensions.HtmlExtensions);
            Register(static (id, filePath) => new MarkdownFileTopic(id, filePath), FileExtensions.MarkdownExtensions);
        }

        /// <summary>
        /// Registers a topic type for the specified file extensions with the specified factory delegate.
        /// </summary>
        /// <typeparam name="TTopic">The type of the topic to register.</typeparam>
        /// <param name="factory">A factory delegate that creates a new instance of the topic. The first parameter is the topic identifier, and the second is the file path.</param>
        /// <param name="fileExtensions">The file extensions (including the leading period) to register the topic type for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> or <paramref name="fileExtensions"/> is <see langword="null"/>.</exception>
        public static void Register<TTopic>(Func<string, string, TTopic> factory, params IEnumerable<string> fileExtensions)
            where TTopic : FileTopic
        {
            if (factory is null)
                throw new ArgumentNullException(nameof(factory));
            if (fileExtensions is null)
                throw new ArgumentNullException(nameof(fileExtensions));

            var registration = new Registration(typeof(TTopic), factory);

            foreach (var fileExtension in fileExtensions)
            {
                if (fileExtension is not null)
                    registry[fileExtension] = registration;
            }
        }

        /// <summary>
        /// Unregisters a topic type for all associated file extensions.
        /// </summary>
        /// <typeparam name="TTopic">The type of the topic to unregister.</typeparam>
        public static void Unregister<TTopic>()
            where TTopic : FileTopic
        {
            var keys = registry
                .Where(static pair => pair.Value.TopicType == typeof(TTopic))
                .Select(static pair => pair.Key)
                .ToList();

            foreach (var key in keys)
                registry.TryRemove(key, out _);
        }

        /// <summary>
        /// Unregisters a topic type for the specified file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension (including the leading period) to unregister the topic type for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileExtension"/> is <see langword="null"/>.</exception>
        public static void Unregister(string fileExtension)
        {
            if (fileExtension is null)
                throw new ArgumentNullException(nameof(fileExtension));

            registry.TryRemove(fileExtension, out _);
        }

        /// <summary>
        /// Determines whether a specialized topic type is registered for the specified file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension to check.</param>
        /// <returns><see langword="true"/> if a specialized topic type is registered for the specified file extension; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileExtension"/> is <see langword="null"/>.</exception>
        public static bool IsRegistered(string fileExtension)
        {
            if (fileExtension is null)
                throw new ArgumentNullException(nameof(fileExtension));

            return registry.ContainsKey(fileExtension);
        }

        /// <summary>
        /// Creates a topic instance based on the specified file path, using the file name (without extension) as the topic name.
        /// </summary>
        /// <param name="filePath">The path to the file that contains the topic content.</param>
        /// <returns>An instance of a <see cref="FileTopic"/> or its derived class appropriate for the file extension.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is <see langword="null"/> or whitespace, or does not have a valid file name.</exception>
        public static FileTopic Create(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException($"'{nameof(filePath)}' cannot be null or whitespace.", nameof(filePath));

            var topicId = Path.GetFileNameWithoutExtension(filePath);

            if (string.IsNullOrWhiteSpace(topicId))
                throw new ArgumentException($"'{nameof(filePath)}' must have a valid file name.", nameof(filePath));

            return Create(topicId, filePath);
        }

        /// <summary>
        /// Creates a topic instance with the specified name based on the specified file path.
        /// </summary>
        /// <param name="id">The unique identifier of the topic among its siblings, typically used for URLs or filenames.</param>
        /// <param name="filePath">The path to the file that contains the topic content.</param>
        /// <returns>An instance of a <see cref="FileTopic"/> or its derived class appropriate for the file extension.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> or <paramref name="filePath"/> is <see langword="null"/>, whitespace, or <paramref name="id"/> contains invalid characters.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file specified by <paramref name="filePath"/> does not exist.</exception>
        public static FileTopic Create(string id, string filePath)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException($"'{nameof(id)}' cannot be null or whitespace.", nameof(id));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException($"'{nameof(filePath)}' cannot null or be whitespace.", nameof(filePath));
            if (Path.GetInvalidPathChars().Any(id.Contains))
                throw new ArgumentException($"'{nameof(id)}' contains invalid characters.", nameof(id));

            return registry.TryGetValue(Path.GetExtension(filePath), out var registration)
                ? registration.Factory(id, filePath)
                : new FileTopic(filePath, id);
        }

        #region Private Members

        private sealed class Registration
        {
            public Registration(Type topicType, Func<string, string, FileTopic> factory)
            {
                Factory = factory;
                TopicType = topicType;
            }

            public Type TopicType { get; }
            public Func<string, string, FileTopic> Factory { get; }
        }

        private static readonly ConcurrentDictionary<string, Registration> registry = new(StringComparer.OrdinalIgnoreCase);

        #endregion
    }
}
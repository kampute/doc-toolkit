// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Formatters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Provides a registry for text transformers that convert content from different formats to a target format.
    /// </summary>
    /// <remarks>
    /// The <see cref="TextTransformerRegistry"/> class manages a collection of text transformers for converting content
    /// from various source formats to a target format. Each transformer is registered with one or more file extensions
    /// that it can handle as input.
    /// <note type="info" title="Important">
    /// By default, the target file extension is registered with the <see cref="IdentityTransformer"/>. This means that
    /// if no other transformer is registered for the target file extension, the content will be passed through unchanged.
    /// It is possible to override this default behavior by registering a different transformer for the target file
    /// extension. However, removing the target file extension from the registry is not allowed.
    /// </note>
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    /// <seealso cref="ITextTransformer"/>
    public class TextTransformerRegistry
    {
        private readonly ConcurrentDictionary<string, ITextTransformer> textTransformers = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="TextTransformerRegistry"/> class.
        /// </summary>
        /// <param name="targetFileExtension">The file extension (including the leading period) of the target format.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="targetFileExtension"/> is <see langword="null"/>.</exception>
        public TextTransformerRegistry(string targetFileExtension)
        {
            TargetFileExtension = targetFileExtension ?? throw new ArgumentNullException(nameof(targetFileExtension));
            textTransformers[targetFileExtension] = IdentityTransformer.Instance;
        }

        /// <summary>
        /// Gets the file extension of the target format.
        /// </summary>
        /// <value>
        /// The file extension (including the leading period) of the target format.
        /// </value>
        public string TargetFileExtension { get; }

        /// <summary>
        /// Gets the file extensions that can be transformed to the target format.
        /// </summary>
        /// <value>
        /// An enumerable of file extensions (including the leading period) that can be transformed to the target format.
        /// </value>
        public IEnumerable<string> SupportedFileExtensions => textTransformers.Keys;

        /// <summary>
        /// Determines if a text transformer is available for the specified file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension (including the leading period) of the source format.</param>
        /// <returns><see langword="true"/> if a text transformer is available for the specified file extension; otherwise, <see langword="false"/>.</returns>
        public bool CanTransform(string fileExtension) => fileExtension is not null && textTransformers.ContainsKey(fileExtension);

        /// <summary>
        /// Attempts to get a text transformer for the specified file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension (including the leading period) of the source format.</param>
        /// <param name="transformer">
        /// When this method returns, contains the text transformer for the specified file extension, if found; otherwise, <see langword="null"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true"/> if a format transformer was found for the specified file extension; otherwise, <see langword="false"/>.</returns>
        public bool TryGet(string fileExtension, [NotNullWhen(true)] out ITextTransformer? transformer)
        {
            if (fileExtension is not null)
                return textTransformers.TryGetValue(fileExtension, out transformer);

            transformer = null;
            return false;
        }

        /// <summary>
        /// Registers a text transformer for for one or more file extensions.
        /// </summary>
        /// <typeparam name="TTransformer">The type of the text transformer to register.</typeparam>
        /// <param name="fileExtensions">The file extensions (including the leading period) of the source formats.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileExtensions"/> is <see langword="null"/>.</exception>
        public void Register<TTransformer>(params IEnumerable<string> fileExtensions)
            where TTransformer : ITextTransformer, new()
        {
            if (fileExtensions is null)
                throw new ArgumentNullException(nameof(fileExtensions));

            Register(new TTransformer(), fileExtensions);
        }

        /// <summary>
        /// Registers a text transformer for one or more file extensions.
        /// </summary>
        /// <param name="transformer">The text transformer to register.</param>
        /// <param name="fileExtensions">The file extensions (including the leading period) of the source formats.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="transformer"/> or <paramref name="fileExtensions"/> is <see langword="null"/>.</exception>
        public void Register(ITextTransformer transformer, params IEnumerable<string> fileExtensions)
        {
            if (transformer is null)
                throw new ArgumentNullException(nameof(transformer));
            if (fileExtensions is null)
                throw new ArgumentNullException(nameof(fileExtensions));

            foreach (var fileExtension in fileExtensions)
            {
                if (fileExtension is not null)
                    textTransformers[fileExtension] = transformer;
            }
        }

        /// <summary>
        /// Removes all registered transformers of the specified type.
        /// </summary>
        /// <typeparam name="TTransformer">The type of the text transformer to remove.</typeparam>
        public void Remove<TTransformer>()
            where TTransformer : ITextTransformer
        {
            var fileExtensions = textTransformers
                .Where(pair => pair.Value.GetType() == typeof(TTransformer) && !IsTargetFileExtension(pair.Key))
                .Select(static pair => pair.Key)
                .ToList();

            foreach (var fileExtension in fileExtensions)
                textTransformers.TryRemove(fileExtension, out _);
        }

        /// <summary>
        /// Removes a registered transformer for the specified file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension (including the leading period) of the source format.</param>
        /// <returns><see langword="true"/> if the transformer was found and removed; otherwise, <see langword="false"/>.</returns>
        public bool Remove(string fileExtension) => fileExtension is not null && !IsTargetFileExtension(fileExtension) && textTransformers.TryRemove(fileExtension, out _);

        /// <summary>
        /// Determines if the specified file extension is the target file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension (including the leading period) to check.</param>
        /// <returns><see langword="true"/> if the specified file extension is the target file extension; otherwise, <see langword="false"/>.</returns>
        public bool IsTargetFileExtension(string fileExtension) => StringComparer.OrdinalIgnoreCase.Equals(fileExtension, TargetFileExtension);
    }
}
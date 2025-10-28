// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Formatters
{
    using Kampute.DocToolkit.Support;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provides methods for registering and retrieving documentation formatters based on file extensions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DocFormatProvider"/> class serves as a central registry for documentation formatters,
    /// allowing the system to dynamically select appropriate formatters based on file extensions.
    /// </para>
    /// This class:
    /// <list type="bullet">
    ///   <item>Manages associations between file extensions and formatter implementations</item>
    ///   <item>Provides factory methods to create formatters on demand</item>
    ///   <item>Supports registration of custom formatters for specific file types</item>
    ///   <item>Allows for overriding default formatters with custom implementations</item>
    /// </list>
    /// The system comes pre-configured with formatters for common documentation formats (HTML and Markdown),
    /// but applications can register additional formatters or replace the built-in ones as needed.
    /// </remarks>
    /// <threadsafety static="true"/>
    /// <seealso cref="IDocumentFormatter"/>
    public static class DocFormatProvider
    {
        static DocFormatProvider()
        {
            Register(static targetExtension => new MarkdownFormat(targetExtension), FileExtensions.MarkdownExtensions);
            Register(static targetExtension => new HtmlFormat(targetExtension), FileExtensions.HtmlExtensions);
        }

        /// <summary>
        /// Registers a documentation formatter for the specified file extensions with the specified factory delegate.
        /// </summary>
        /// <typeparam name="TFormatter">The type of the documentation formatter to register.</typeparam>
        /// <param name="factory">A factory delegate that creates a new instance of the formatter.</param>
        /// <param name="fileExtensions">The file extensions (including the leading period) to register the formatter for.</param>
        public static void Register<TFormatter>(Func<string, TFormatter> factory, params IEnumerable<string> fileExtensions)
            where TFormatter : IDocumentFormatter
        {
            if (factory is null)
                throw new ArgumentNullException(nameof(factory));
            if (fileExtensions is null)
                throw new ArgumentNullException(nameof(fileExtensions));

            var registration = new Registration(typeof(TFormatter), targetExtension => factory(targetExtension));

            foreach (var fileExtension in fileExtensions)
            {
                if (fileExtension is not null)
                    registry[fileExtension] = registration;
            }
        }

        /// <summary>
        /// Unregisters a documentation formatter for the associated file extensions.
        /// </summary>
        /// <typeparam name="TFormatter">The type of the documentation formatter to unregister.</typeparam>
        public static void Unregister<TFormatter>()
            where TFormatter : IDocumentFormatter
        {
            var keys = registry
                .Where(static pair => pair.Value.FormatterType == typeof(TFormatter))
                .Select(static pair => pair.Key)
                .ToList();

            foreach (var key in keys)
                registry.TryRemove(key, out _);
        }

        /// <summary>
        /// Unregisters a documentation formatter for the specified file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension (including the leading period) to unregister the formatter for.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileExtension"/> is <see langword="null"/>.</exception>
        public static void Unregister(string fileExtension)
        {
            if (fileExtension is null)
                throw new ArgumentNullException(nameof(fileExtension));

            registry.TryRemove(fileExtension, out _);
        }

        /// <summary>
        /// Determines whether a documentation formatter is registered for the specified file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension (including the leading period) to check for a registered formatter.</param>
        /// <returns><see langword="true"/> if a formatter is registered for the specified file extension; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileExtension"/> is <see langword="null"/>.</exception>
        public static bool IsRegistered(string fileExtension)
        {
            if (fileExtension is null)
                throw new ArgumentNullException(nameof(fileExtension));

            return registry.ContainsKey(fileExtension);
        }

        /// <summary>
        /// Gets a documentation formatter based on the specified file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension (including the leading period) to get a documentation formatter for.</param>
        /// <returns>An instance of a documentation formatter for the specified file extension, or <see langword="null"/> if no formatter is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileExtension"/> is <see langword="null"/>.</exception>
        public static IDocumentFormatter? GetFormatterByExtension(string fileExtension)
        {
            if (fileExtension is null)
                throw new ArgumentNullException(nameof(fileExtension));

            return registry.TryGetValue(fileExtension, out var registration) ? registration.Factory(fileExtension) : null;
        }

        /// <summary>
        /// Gets a documentation formatter based on the specified file extension or throws an exception if no formatter is found.
        /// </summary>
        /// <param name="fileExtension">The file extension (including the leading period) to get a documentation formatter for.</param>
        /// <returns>An instance of a documentation formatter for the specified file extension.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fileExtension"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">Thrown when no formatter is found for the specified file extension.</exception>
        public static IDocumentFormatter GetRequiredFormatterByExtension(string fileExtension)
        {
            return GetFormatterByExtension(fileExtension) ?? throw new NotSupportedException
            (
                $"No formatter is registered for the file extension '{fileExtension}'." +
                $"Please use the '{nameof(DocFormatProvider)}.{nameof(Register)}' method to register a formatter for the specified file extension."
            );
        }

        #region Private Members

        private sealed class Registration
        {
            public Registration(Type formatterType, Func<string, IDocumentFormatter> factory)
            {
                Factory = factory;
                FormatterType = formatterType;
            }

            public Type FormatterType { get; }
            public Func<string, IDocumentFormatter> Factory { get; }
        }

        private static readonly ConcurrentDictionary<string, Registration> registry = new(StringComparer.OrdinalIgnoreCase);

        #endregion
    }
}

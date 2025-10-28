// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Xslt
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Xsl;

    /// <summary>
    /// Provides functionality for compiling XSLT transformations.
    /// </summary>
    /// <remarks>
    /// The <see cref="XsltCompiler"/> class offers a set of utility methods to compile XSLT transformations from various
    /// sources, including text readers, streams, strings, and embedded resources.
    /// </remarks>
    /// <threadsafety static="true"/>
    public static class XsltCompiler
    {
        /// <summary>
        /// Compiles the XSLT transformation from the provided text reader.
        /// </summary>
        /// <param name="textReader">The text reader containing the XSLT transform to compile.</param>
        /// <returns>The compiled XSLT transform.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="textReader"/> is <see langword="null"/>.</exception>
        public static XslCompiledTransform Compile(TextReader textReader)
        {
            if (textReader is null)
                throw new ArgumentNullException(nameof(textReader));

            using var xmlReader = XmlReader.Create(textReader, new XmlReaderSettings
            {
                CloseInput = true,
                IgnoreComments = true,
                IgnoreWhitespace = true,
            });

            var xslt = new XslCompiledTransform();
            xslt.Load(xmlReader);
            return xslt;
        }

        /// <summary>
        /// Compiles the XSLT transformation from the provided stream.
        /// </summary>
        /// <param name="stream">The stream containing the XSLT transform to compile.</param>
        /// <returns>The compiled XSLT transform.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is <see langword="null"/>.</exception>
        public static XslCompiledTransform Compile(Stream stream)
        {
            if (stream is null)
                throw new ArgumentNullException(nameof(stream));

            using var textReader = new StreamReader(stream);
            return Compile(textReader);
        }

        /// <summary>
        /// Compiles the XSLT transformation from the provided string.
        /// </summary>
        /// <param name="styleSheet">The XSLT transform to compile.</param>
        /// <returns>The compiled XSLT transform.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="styleSheet"/> is <see langword="null"/>.</exception>
        public static XslCompiledTransform Compile(string styleSheet)
        {
            if (styleSheet is null)
                throw new ArgumentNullException(nameof(styleSheet));

            using var textReader = new StringReader(styleSheet);
            return Compile(textReader);
        }

        /// <summary>
        /// Compiles the XSLT transformation from an embedded resource specified by its name.
        /// </summary>
        /// <param name="name">The name of the XSLT transform resource to compile.</param>
        /// <returns>The compiled XSLT transform.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the specified XSLT resource cannot be found within the assembly's resources.</exception>
        internal static XslCompiledTransform CompileEmbeddedResource(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            var fullPath = $"{typeof(XsltCompiler).Namespace}.Resources.{name}.xslt";
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullPath)
                ?? throw new ArgumentException($"The XSLT resource could not be found: {fullPath}", nameof(name));

            return Compile(stream);
        }
    }
}

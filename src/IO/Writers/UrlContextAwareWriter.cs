// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.IO.Writers
{
    using Kampute.DocToolkit.Routing;
    using System;
    using System.IO;

    /// <summary>
    /// Represents a text writer for rendering documentation content.
    /// </summary>
    /// <remarks>
    /// The <see cref="UrlContextAwareWriter"/> class serves as a text writer for rendering documentation content, which includes
    /// handling URL adjustments for cross-document links and managing the output stream.
    /// <para>
    /// When working with <see cref="UrlContextAwareWriter"/> instances, note that they must be properly disposed after use to ensure
    /// all resources are released correctly and state is reset for subsequent rendering tasks.
    /// </para>
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public class UrlContextAwareWriter : WrappedTextWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlContextAwareWriter"/> class.
        /// </summary>
        /// <param name="textWriter">The <see cref="TextWriter"/> used for writing the content of the document being rendered.</param>
        /// <param name="urlContext">The <see cref="DocumentUrlContext"/> that provides URL adjustment for the document being rendered.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="textWriter"/> or <paramref name="urlContext"/> is <see langword="null"/>.</exception>
        public UrlContextAwareWriter(TextWriter textWriter, DocumentUrlContext urlContext)
            : base(textWriter, leaveOpen: false)
        {
            UrlContext = urlContext ?? throw new ArgumentNullException(nameof(urlContext));
        }

        /// <summary>
        /// Gets the <see cref="DocumentUrlContext"/> that provides URL adjustment for the document being rendered.
        /// </summary>
        /// <value>
        /// The <see cref="DocumentUrlContext"/> instance that specifies the context for cross-document links in the document being rendered.
        /// </value>
        protected DocumentUrlContext UrlContext { get; }

        /// <summary>
        /// Disposes the current instance of <see cref="UrlContextAwareWriter"/> and releases any resources it holds.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is being called from the <see cref="TextWriter.Dispose()"/> method or the finalizer.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                UrlContext.Dispose();

            base.Dispose(disposing);
        }
    }
}

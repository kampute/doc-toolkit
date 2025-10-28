// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.IO.Writers
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a text writer that wraps another <see cref="TextWriter"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="WrappedTextWriter"/> class provides a base implementation for text writers that need to delegate
    /// or transform content written to an underlying <see cref="TextWriter"/>. It implements the core functionality of
    /// the <see cref="TextWriter"/> class while forwarding write operations to the wrapped instance.
    /// </para>
    /// This pattern is useful for several scenarios in documentation generation:
    /// <list type="bullet">
    ///   <item><description>Adding content transformations (like encoding, minification, or formatting)</description></item>
    ///   <item><description>Tracking or logging write operations</description></item>
    ///   <item><description>Adding buffering capabilities</description></item>
    ///   <item><description>Supporting special document-format specific behaviors</description></item>
    /// </list>
    /// By using composition rather than inheritance from specific <see cref="TextWriter"/> implementations, the
    /// <see cref="WrappedTextWriter"/> can work with any <see cref="TextWriter"/> instance, making it adaptable
    /// to various output destinations (files, memory, console, etc.).
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    public abstract class WrappedTextWriter : TextWriter
    {
        private readonly bool leaveOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="WrappedTextWriter"/> class.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to write encoded text to.</param>
        /// <param name="leaveOpen">A value indicating whether the underlying writer should be left open when the <see cref="WrappedTextWriter"/> is disposed.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="writer"/> is <see langword="null"/>.</exception>
        protected WrappedTextWriter(TextWriter writer, bool leaveOpen)
        {
            UnderlyingWriter = writer ?? throw new ArgumentNullException(nameof(writer));
            CoreNewLine = writer.NewLine.ToCharArray();
            this.leaveOpen = leaveOpen;
        }

        /// <summary>
        /// Gets the underlying <see cref="TextWriter"/> that this <see cref="WrappedTextWriter"/> writes to.
        /// </summary>
        /// <value>
        /// The underlying <see cref="TextWriter"/> that this <see cref="WrappedTextWriter"/> writes to.
        /// </value>
        protected TextWriter UnderlyingWriter { get; }

        /// <summary>
        /// Gets the <see cref="Encoding"/> in use by the underlying <see cref="TextWriter"/>.
        /// </summary>
        /// <value>
        /// The <see cref="Encoding"/> in use by the underlying <see cref="TextWriter"/>.
        /// </value>
        public override Encoding Encoding => UnderlyingWriter.Encoding;

        /// <summary>
        /// Gets or sets the line terminator string used by the underlying <see cref="TextWriter"/>.
        /// </summary>
        /// <value>
        /// The line terminator string for the underlying <see cref="TextWriter"/>.
        /// </value>
        public override string NewLine
        {
            get => UnderlyingWriter.NewLine;
#pragma warning disable CS8765 // Nullability of parameter doesn't match overridden member
            set
            {
                UnderlyingWriter.NewLine = value;
                CoreNewLine = UnderlyingWriter.NewLine.ToCharArray();
            }
#pragma warning restore CS8765 // Nullability of parameter doesn't match overridden member
        }

        /// <summary>
        /// Writes a character to the underlying <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="value">The character to write.</param>
        public override void Write(char value) => UnderlyingWriter.Write(value);

        /// <summary>
        /// Clears all buffers for the underlying <see cref="TextWriter"/> and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush() => UnderlyingWriter.Flush();

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => UnderlyingWriter.ToString()!;

        /// <summary>
        /// Releases the unmanaged resources used by the current writer and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (leaveOpen)
                    UnderlyingWriter.Flush();
                else
                    UnderlyingWriter.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

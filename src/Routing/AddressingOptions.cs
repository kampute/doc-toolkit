// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Routing
{
    using System;

    /// <summary>
    /// Provides options for addressing and organizing documentation files.
    /// </summary>
    /// <remarks>
    /// This abstract class defines common configuration options used by addressing strategies in the documentation system.
    /// Derived classes should extend these options with additional configuration properties specific to particular addressing
    /// schemes.
    /// </remarks>
    /// <seealso cref="AddressingStrategy"/>
    public abstract class AddressingOptions
    {
        private string fileExtension;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressingStrategy"/> class.
        /// </summary>
        /// <param name="fileExtension">The file extension for the documentation files.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fileExtension"/> is <see langword="null"/> or empty.</exception>
        protected AddressingOptions(string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
                throw new ArgumentException($"'{nameof(fileExtension)}' cannot be null or empty.", nameof(fileExtension));

            this.fileExtension = fileExtension.StartsWith('.') ? fileExtension : '.' + fileExtension;
        }

        /// <summary>
        /// Gets or sets the file extension to be used for documentation files.
        /// </summary>
        /// <value>
        /// The file extension (including the leading period) to be used for documentation files.
        /// </value>
        /// <exception cref="ArgumentException">Thrown when the value is <see langword="null"/> or empty.</exception>
        public string FileExtension
        {
            get => fileExtension;
            set => fileExtension = !string.IsNullOrEmpty(value)
                ? value.StartsWith('.') ? value : '.' + value
                : throw new ArgumentException($"'{nameof(value)}' cannot be null or empty.", nameof(value));
        }

        /// <summary>
        /// Gets or sets a value indicating whether the file extension should be excluded from URLs.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the file extension should be omitted in URLs; otherwise, <see langword="false"/>.
        /// </value>
        public bool OmitExtensionInUrls { get; set; }
    }
}

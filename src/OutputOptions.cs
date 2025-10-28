// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    /// <summary>
    /// Provides options for output generation in the documentation process.
    /// </summary>
    /// <remarks>
    /// This class controls various aspects of the generated documentation output,
    /// such as minification settings.
    /// </remarks>
    public class OutputOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputOptions"/> class.
        /// </summary>
        public OutputOptions()
        {
        }

        /// <summary>
        /// Gets a value indicating whether to disable minification of the output.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if minification is disabled; otherwise, <see langword="false"/>. Default is <see langword="false"/>.
        /// </value>
        public bool DisableMinification { get; set; }
    }
}

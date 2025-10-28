namespace Kampute.DocToolkit.Support
{
    using System;

    /// <summary>
    /// Represents a URI that preserves the original string representation including all escape sequences.
    /// </summary>
    /// <remarks>
    /// The <see cref="RawUri"/> class is a subclass of <see cref="Uri"/> that preserves the original URI string exactly
    /// as provided, including all escape sequences. This is particularly useful when standard <see cref="Uri"/> behavior
    /// would decode escaped characters like <c>%2D</c> (dash) back to their unescaped forms.
    /// </remarks>
    /// <threadsafety static="true" instance="true"/>
    public class RawUri : Uri
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawUri"/> class using the specified URI string.
        /// </summary>
        /// <param name="uriString">The URI string to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="uriString"/> is <see langword="null"/>.</exception>
        /// <exception cref="UriFormatException">Thrown when <paramref name="uriString"/> is not a valid URI.</exception>
        /// <remarks>
        /// The characters in the URI string that have a reserved meaning in the requested URI components must be properly escaped.
        /// </remarks>
        public RawUri(string uriString)
            : base(uriString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawUri"/> class with the specified base URI and relative URI string.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="relativeUri">The relative URI string to add to the base URI.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseUri"/> or <paramref name="relativeUri"/> is <see langword="null"/>.</exception>
        /// <exception cref="UriFormatException">Thrown when the combination of <paramref name="baseUri"/> and <paramref name="relativeUri"/> is not a valid URI.</exception>
        /// <remarks>
        /// The characters in the relative URI string that have a reserved meaning in the requested URI components must be properly escaped.
        /// </remarks>
        public RawUri(Uri baseUri, string relativeUri)
            : base(baseUri, relativeUri)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawUri"/> class with the specified base URI and relative URI.
        /// </summary>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="relativeUri">The relative URI to add to the base URI.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseUri"/> or <paramref name="relativeUri"/> is <see langword="null"/>.</exception>
        /// <exception cref="UriFormatException">Thrown when the combination of <paramref name="baseUri"/> and <paramref name="relativeUri"/> is not a valid URI.</exception>
        public RawUri(Uri baseUri, Uri relativeUri)
            : base(baseUri, relativeUri)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RawUri"/> class using the specified URI string and URI kind.
        /// </summary>
        /// <param name="uriString">The URI string to use.</param>
        /// <param name="uriKind">The type of the URI in <paramref name="uriString"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="uriString"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="uriKind"/> is invalid.</exception>
        /// <exception cref="UriFormatException">Thrown when <paramref name="uriString"/> is not a valid URI of the specified kind.</exception>
        /// <remarks>
        /// The characters in the URI string that have a reserved meaning in the requested URI components must be properly escaped.
        /// </remarks>
        public RawUri(string uriString, UriKind uriKind)
            : base(uriString, uriKind)
        {
        }

        /// <summary>
        /// Returns the original string representation of the URI without any normalization or decoding of escape sequences.
        /// </summary>
        /// <returns>The original string representation of the URI.</returns>
        /// <remarks>
        /// This override ensures that all escape sequences in the original URI string are preserved exactly as provided, rather than
        /// being normalized or decoded as in the base <see cref="Uri"/> implementation.
        /// <para>
        /// This is particularly useful for preserving escaped sequences like <c>%2D</c> (dash) that would otherwise be automatically
        /// converted back to their character representations by the standard <see cref="Uri"/> class.
        /// </para>
        /// </remarks>
        public override string ToString() => OriginalString;
    }
}

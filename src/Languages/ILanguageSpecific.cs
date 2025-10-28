namespace Kampute.DocToolkit.Languages
{
    /// <summary>
    /// Defines a contract for language-specific features.
    /// </summary>
    /// <remarks>
    /// This interface is used to identify components that have language-specific behavior
    /// or formatting. Implementations can adapt their functionality based on the programming
    /// language they are working with.
    /// </remarks>
    public interface ILanguageSpecific
    {
        /// <summary>
        /// Gets the programming language.
        /// </summary>
        /// <value>
        /// The programming language.
        /// </value>
        IProgrammingLanguage Language { get; }
    }
}

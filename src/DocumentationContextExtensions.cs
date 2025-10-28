// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Models;
    using Kampute.DocToolkit.XmlDoc;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides extension methods for <see cref="IDocumentationContext"/> instances to simplify common tasks.
    /// </summary>
    public static class DocumentationContextExtensions
    {
        /// <summary>
        /// Finds the model corresponding to the specified reflection metadata within the documentation context.
        /// </summary>
        /// <param name="context">The documentation context to search for the member.</param>
        /// <param name="member">The reflection metadata of the member to find.</param>
        /// <returns>The documentation model representing the member, or <see langword="null"/> if the member is not found in the context.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="member"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method provides a convenient way to locate the documentation model that corresponds to a given reflection member. It searches
        /// the assemblies in the context for the specified member and returns the corresponding documentation model if found. The method handles
        /// both top-level types and nested members, making it suitable for resolving any member in the context.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberModel? FindMember(this IDocumentationContext context, IMember member)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            return context.Assemblies.FirstOrDefault(assembly => ReferenceEquals(assembly.Metadata, member.Assembly))?.FindMember(member);
        }

        /// <summary>
        /// Inspects the documentation of members in the context for missing or incomplete elements.
        /// </summary>
        /// <param name="context">The documentation context to inspect.</param>
        /// <param name="options">The options to determine which tags to inspect.</param>
        /// <returns>An enumerable of <see cref="XmlDocInspectionIssue"/> instances representing the issues found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method inspects the XML documentation comments of all members in the context and yields issues for missing
        /// or incomplete documentation elements. It leverages cached documentation from model instances to avoid redundant lookups.
        /// </remarks>
        public static IEnumerable<XmlDocInspectionIssue> InspectDocumentations(this IDocumentationContext context, XmlDocInspectionOptions options = XmlDocInspectionOptions.Required)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            return options != XmlDocInspectionOptions.None ? EnumerateDocumentationIssues() : [];

            IEnumerable<XmlDocInspectionIssue> EnumerateDocumentationIssues()
            {
                foreach (var type in context.Types)
                {
                    foreach (var issue in type.Doc.Inspect(type.Metadata, options, GetOverloadDoc))
                        yield return issue;

                    foreach (var member in type.Members)
                    {
                        foreach (var issue in member.Doc.Inspect(member.Metadata, options, GetOverloadDoc))
                            yield return issue;
                    }
                }
            }

            XmlDocEntry? GetOverloadDoc(IMember member) => context.FindMember(member)?.Doc;
        }

        /// <summary>
        /// Determines whether the specified file format (based on the file extension) is supported by the content formatter of the documentation context.
        /// </summary>
        /// <param name="context">The documentation context to check for format support.</param>
        /// <param name="filePath">The file path to check for format support.</param>
        /// <returns><see langword="true"/> if the format is supported; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="filePath"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is <see langword="null"/> or empty.</exception>
        /// <remarks>
        /// This method checks if the file format specified by the file extension of the given file path is supported by the content formatter
        /// associated with the documentation context.
        /// <note type="hint" title="Hint">
        /// The <paramref name="filePath"/> can be either the full path of a file or a file extension (including the leading period).
        /// </note>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFormatSupported(this IDocumentationContext context, string filePath)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException($"'{nameof(filePath)}' cannot be null or empty.", nameof(filePath));

            return context.ContentFormatter.TextTransformers.CanTransform(Path.GetExtension(filePath));
        }

        /// <summary>
        /// Attempts to transform the specified text using the appropriate text transformer based on the file extension.
        /// </summary>
        /// <param name="context">The documentation context that provides the text transformer.</param>
        /// <param name="fileExtension">The file extension to identify the appropriate text transformer.</param>
        /// <param name="text">The text to be transformed.</param>
        /// <param name="transformedText">
        /// When this method returns, contains the transformed text if the transformation was successful; otherwise, <see langword="null"/>.
        /// This parameter is passed uninitialized.
        /// </param>
        /// <returns><see langword="true"/> if the transformation was successful; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/>, <paramref name="fileExtension"/>, or <paramref name="text"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method attempts to transform the provided text using the appropriate text transformer based on the specified file
        /// extension as the source format. If the <paramref name="text"/> is empty, the method returns an empty string as the transformed
        /// text even if the format is not supported.
        /// </remarks>
        public static bool TryTransformText(this IDocumentationContext context, string fileExtension, string text, [NotNullWhen(true)] out string? transformedText)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (fileExtension is null)
                throw new ArgumentNullException(nameof(fileExtension));

            if (string.IsNullOrEmpty(text))
            {
                transformedText = string.Empty;
                return true;
            }

            if (!context.ContentFormatter.TextTransformers.TryGet(fileExtension, out var transformer))
            {
                transformedText = null;
                return false;
            }

            if (transformer is IdentityTransformer)
            {
                transformedText = text;
                return true;
            }

            using var reader = new StringReader(text);
            using var writer = new StringWriter();
            transformer.Transform(reader, writer, context.UrlTransformer);
            transformedText = writer.ToString();
            return true;
        }

        /// <summary>
        /// Determines the most suitable name qualifier for displaying the specified member in the context of the current documentation scope.
        /// </summary>
        /// <param name="context">The documentation context to use for determining the preferred name qualifier.</param>
        /// <param name="member">The member for which to determine the preferred name qualifier.</param>
        /// <returns>The preferred <see cref="NameQualifier"/> for displaying the specified member.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="member"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method helps in deciding how to qualify the name of a member when generating documentation based on the current
        /// scope of the documentation context.
        /// <list type="bullet">
        ///   <item>Types always use <see cref="NameQualifier.DeclaringType"/>.</item>
        ///   <item>Extension methods and operators use <see cref="NameQualifier.None"/>.</item>
        ///   <item>Other members use <see cref="NameQualifier.None"/> if they belong to the type currently being documented; otherwise, they use <see cref="NameQualifier.DeclaringType"/>.</item>
        /// </list>
        /// </remarks>
        public static NameQualifier DetermineNameQualifier(this IDocumentationContext context, IMember member)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            if (member is IType)
                return NameQualifier.DeclaringType;

            if (member is IMethod { IsExtension: true } or IOperator)
                return NameQualifier.None;

            return context.AddressProvider.ActiveScope.Model is TypeModel typeModel
                && ReferenceEquals(typeModel.Metadata, member.DeclaringType)
                    ? NameQualifier.None
                    : NameQualifier.DeclaringType;
        }
    }
}

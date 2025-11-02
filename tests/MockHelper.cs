// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using Kampute.DocToolkit.Topics;
    using Kampute.DocToolkit.XmlDoc;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Linq;

    /// <summary>
    /// Provides helper methods for creating mock objects.
    /// </summary>
    internal static class MockHelper
    {
        /// <summary>
        /// Creates a documentation context for the specified assemblies.
        /// </summary>
        /// <typeparam name="TFormat">The type of the document formatter.</typeparam>
        /// <param name="assemblies">The assemblies to include in the documentation context.</param>
        /// <returns>A mocked documentation context.</returns>
        public static IDocumentationContext CreateDocumentationContext<TFormat>(params IEnumerable<IAssembly> assemblies)
            where TFormat : IDocumentFormatter, new()
        {
            return CreateDocumentationContext<TFormat>(assemblies, []);
        }

        /// <summary>
        /// Creates a documentation context for the specified topics.
        /// </summary>
        /// <typeparam name="TFormat">The type of the document formatter.</typeparam>
        /// <param name="topics">The topics to include in the documentation context.</param>
        /// <returns>A mocked documentation context.</returns>
        public static IDocumentationContext CreateDocumentationContext<TFormat>(IEnumerable<ITopic> topics)
            where TFormat : IDocumentFormatter, new()
        {
            return CreateDocumentationContext<TFormat>([], topics);
        }

        /// <summary>
        /// Creates a documentation context for the specified assemblies and topics.
        /// </summary>
        /// <typeparam name="TFormat">The type of the document formatter.</typeparam>
        /// <param name="assemblies">The assemblies to include in the documentation context.</param>
        /// <param name="topics">The topics to include in the documentation context.</param>
        /// <returns>A mocked documentation context.</returns>
        public static IDocumentationContext CreateDocumentationContext<TFormat>(IEnumerable<IAssembly> assemblies, IEnumerable<ITopic> topics)
            where TFormat : IDocumentFormatter, new()
        {
            return new DocumentationContext(new CSharp(), CreateAddressProvider(), CreateXmlDocProvider(), new TFormat(), assemblies, topics);
        }

        /// <summary>
        /// Creates a mocked address provider.
        /// </summary>
        /// <returns>A mocked address provider.</returns>
        public static IDocumentAddressProvider CreateAddressProvider()
        {
            var urlContext = new ContextAwareUrlNormalizer();
            var addressProviderMock = new Mock<IDocumentAddressProvider>();
            addressProviderMock.SetupGet(x => x.Granularity).Returns(PageGranularity.NamespaceTypeMember);
            addressProviderMock.SetupGet(x => x.ActiveScope).Returns(() => urlContext.ActiveScope);
            addressProviderMock.Setup(x => x.BeginScope(It.IsAny<string>(), It.IsAny<IDocumentModel?>()))
                .Returns((string directory, IDocumentModel? model) => urlContext.BeginScope(directory, model));
            addressProviderMock.Setup(x => x.TryGetNamespaceUrl(It.IsAny<string>(), out It.Ref<Uri?>.IsAny))
                .Returns((string ns, out Uri? url) =>
                {
                    url = new RawUri($"https://example.com/{ns.ToLowerInvariant()}", UriKind.Absolute);
                    return true;
                });
            addressProviderMock.Setup(x => x.TryGetMemberUrl(It.IsAny<IMember>(), out It.Ref<Uri?>.IsAny))
                .Returns((IMember member, out Uri? url) =>
                {
                    if (member.IsDirectDeclaration)
                    {
                        var resourceName = member.CodeReference[2..].ReplaceMany(['`', '#'], '-').ToLowerInvariant();
                        url = new RawUri($"https://example.com/{resourceName}", UriKind.Absolute);
                        return true;
                    }

                    url = null;
                    return false;
                });
            addressProviderMock.Setup(x => x.TryGetTopicUrl(It.IsAny<ITopic>(), out It.Ref<Uri?>.IsAny))
                .Returns((ITopic topic, out Uri? url) =>
                {
                    url = new RawUri($"~/{topic.Id.ToLowerInvariant()}", UriKind.Relative);
                    return true;
                });
            addressProviderMock.Setup(x => x.TryGetNamespaceFile(It.IsAny<string>(), out It.Ref<string?>.IsAny))
                .Returns((string ns, out string? path) =>
                {
                    path = ns.ToLowerInvariant();
                    return true;
                });
            addressProviderMock.Setup(x => x.TryGetMemberFile(It.IsAny<IMember>(), out It.Ref<string?>.IsAny))
                .Returns((IMember member, out string? path) =>
                {
                    if (member.IsDirectDeclaration)
                    {
                        path = member.CodeReference[2..].ReplaceMany(['`', '#'], '-').ToLowerInvariant();
                        return true;
                    }

                    path = null;
                    return false;
                });
            addressProviderMock.Setup(x => x.TryGetTopicFile(It.IsAny<ITopic>(), out It.Ref<string?>.IsAny))
                .Returns((ITopic topic, out string? path) =>
                {
                    path = topic.Id.ToLowerInvariant();
                    return false;
                });
            return addressProviderMock.Object;
        }

        /// <summary>
        /// Creates a mocked XML documentation provider.
        /// </summary>
        /// <returns>A mocked XML documentation provider.</returns>
        public static IXmlDocProvider CreateXmlDocProvider()
        {
            var contentProviderMock = new Mock<IXmlDocProvider>();

            contentProviderMock.SetupGet(x => x.HasDocumentation).Returns(true);
            contentProviderMock.Setup(x => x.TryGetDoc(It.IsAny<string>(), out It.Ref<XmlDocEntry?>.IsAny))
                .Returns((string cref, out XmlDocEntry? doc) =>
                {
                    if (CodeReference.IsValid(cref))
                    {
                        var xmlComment = XElement.Parse($"<member name=\"{cref}\"><summary>Description of <c>{cref[2..]}</c>.</summary></member>");
                        doc = new XmlDocEntry(xmlComment);
                        return true;
                    }

                    doc = null;
                    return false;
                });

            return contentProviderMock.Object;
        }

        /// <summary>
        /// Creates a mocked XML documentation provider.
        /// </summary>
        /// <returns>A mocked XML documentation provider.</returns>
        public static IXmlDocReferenceResolver CreateXmlDocReferenceResolver()
        {
            var referenceResolverMock = new Mock<IXmlDocReferenceResolver>();

            referenceResolverMock.Setup(static r => r.FormatCode(It.IsAny<string>())).Returns<string>(static code => code);
            referenceResolverMock.Setup(static r => r.GetLanguageId(It.IsAny<string>())).Returns("csharp");
            referenceResolverMock.Setup(static r => r.GetKeywordUrl(It.IsAny<string>())).Returns<string>(static kw => $"https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/{kw}");
            referenceResolverMock.Setup(static r => r.GetCodeReferenceUrl(It.IsAny<string>())).Returns<string>(static cref => $"https://learn.microsoft.com/dotnet/api/{cref[2..].ToLowerInvariant()}");
            referenceResolverMock.Setup(static r => r.GetCodeReferenceTitle(It.IsAny<string>())).Returns<string>(static cref => cref[2..]);
            referenceResolverMock.Setup(static r => r.GetTopicUrl(It.IsAny<string>())).Returns<string>(static href => href);
            referenceResolverMock.Setup(static r => r.GetTopicTitle(It.IsAny<string>())).Returns<string>(static href => $"Title of {href}");

            return referenceResolverMock.Object;
        }

        /// <summary>
        /// Creates a mocked assembly.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <param name="namespaces">The namespaces to include in the assembly.</param>
        /// <returns>A mocked assembly.</returns>
        public static IAssembly CreateAssembly(string name, IEnumerable<string> namespaces)
        {
            var assemblyMock = new Mock<IAssembly>();

            List<IType> exportedTypes = [
                .. namespaces.Select(ns => CreateType(assemblyMock.Object, ns, "ProtectedDummy", false)),
                .. namespaces.Select(ns => CreateType(assemblyMock.Object, ns, "PublicDummy", true))
            ];

            SortedDictionary<string, IReadOnlyList<IType>> exportedNamespaces = [];
            foreach (var group in exportedTypes.GroupBy(static t => t.Namespace))
                exportedNamespaces[group.Key] = [.. group.OrderBy(static t => t.Name)];

            assemblyMock.SetupGet(static a => a.Name).Returns(name);
            assemblyMock.SetupGet(static a => a.Identity).Returns(new AssemblyName(name));
            assemblyMock.SetupGet(static a => a.Modules).Returns([]);
            assemblyMock.SetupGet(static a => a.Namespaces).Returns(exportedNamespaces);
            assemblyMock.SetupGet(static a => a.ExportedTypes).Returns(exportedTypes);

            return assemblyMock.Object;
        }

        /// <summary>
        /// Creates a mocked type.
        /// </summary>
        /// <param name="assembly">The assembly that contains the type.</param>
        /// <param name="ns">The namespace of the type.</param>
        /// <param name="name">The name of the type.</param>
        /// <param name="isPublic">Whether the type is public.</param>
        /// <returns>A mocked type.</returns>
        public static IType CreateType(IAssembly assembly, string ns, string name, bool isPublic)
        {
            var typeMock = new Mock<IClassType>();

            typeMock.SetupGet(static t => t.Assembly).Returns(assembly);
            typeMock.SetupGet(static t => t.Namespace).Returns(ns);
            typeMock.SetupGet(static t => t.Name).Returns(name);
            typeMock.SetupGet(static t => t.FullName).Returns($"{ns}.{name}");
            typeMock.SetupGet(static t => t.Visibility).Returns(isPublic ? MemberVisibility.Public : MemberVisibility.Protected);
            typeMock.SetupGet(static t => t.IsVisible).Returns(isPublic);
            typeMock.SetupGet(static t => t.Fields).Returns([]);
            typeMock.SetupGet(static t => t.Constructors).Returns([]);
            typeMock.SetupGet(static t => t.Methods).Returns([]);
            typeMock.SetupGet(static t => t.Properties).Returns([]);
            typeMock.SetupGet(static t => t.Events).Returns([]);
            typeMock.SetupGet(static t => t.Operators).Returns([]);
            typeMock.SetupGet(static t => t.ExplicitInterfaceMethods).Returns([]);
            typeMock.SetupGet(static t => t.ExplicitInterfaceProperties).Returns([]);
            typeMock.SetupGet(static t => t.ExplicitInterfaceEvents).Returns([]);
            typeMock.SetupGet(static t => t.ExplicitInterfaceMembers).Returns([]);
            typeMock.SetupGet(static t => t.NestedTypes).Returns([]);
            typeMock.SetupGet(static t => t.BaseType).Returns(typeof(object).GetMetadata<IClassType>());
            typeMock.SetupGet(static t => t.Interfaces).Returns([]);
            typeMock.SetupGet(static t => t.TypeParameters).Returns([]);
            typeMock.SetupGet(static t => t.TypeArguments).Returns([]);
            typeMock.SetupGet(static t => t.IsGenericType).Returns(false);
            typeMock.SetupGet(static t => t.IsGenericTypeDefinition).Returns(false);
            typeMock.SetupGet(static t => t.IsConstructedGenericType).Returns(false);
            typeMock.SetupGet(static t => t.OwnGenericParameterRange).Returns((0, 0));
            typeMock.SetupGet(static t => t.IsAbstract).Returns(false);
            typeMock.SetupGet(static t => t.IsSealed).Returns(false);
            typeMock.SetupGet(static t => t.IsStatic).Returns(false);
            typeMock.SetupGet(static t => t.IsValueType).Returns(false);
            typeMock.SetupGet(static t => t.IsNested).Returns(false);
            typeMock.SetupGet(static t => t.Signature).Returns($"{ns}.{name}");
            typeMock.SetupGet(static t => t.CodeReference).Returns($"T:{ns}.{name}");
            typeMock.SetupGet(static t => t.IsDirectDeclaration).Returns(true);
            typeMock.SetupGet(static t => t.CustomAttributes).Returns([]);

            return typeMock.Object;
        }
    }
}

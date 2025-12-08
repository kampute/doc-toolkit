// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.XmlDoc;
    using Moq;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class XmlDocExtensionsTests
    {
        private string xmlFilePath = null!;
        private IXmlDocProvider xmlDocProvider = null!;

        [SetUp]
        public void Setup()
        {
            MetadataProvider.RegisterRuntimeAssemblies();

            const string xmlDoc =
                @"<?xml version='1.0' encoding='utf-8'?>
                <doc>
                    <members>
                        <member name='T:Acme.NamespaceDoc'>
                            <summary>Documentation for the Acme namespace.</summary>
                        </member>
                        <member name='T:Acme.ISampleInterface'>
                            <summary>A documented type but without member documentation.</summary>
                        </member>
                        <member name='M:Acme.SampleDerivedGenericClass`3.GenericMethod``1(`0,`1,`2,``0)'>
                            <seealso href='https://example.com/'></seealso>
                            <exception cref='System.ArgumentNullException'></exception>
                            <permission cref='System.Security.Permissions.SecurityPermission'></permission>
                            <event cref='System.EventHandler'></event>
                        </member>
                    </members>
                </doc>";

            xmlFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xml");
            File.WriteAllText(xmlFilePath, xmlDoc);

            var provider = new XmlDocProvider();
            provider.ImportFile(xmlFilePath);
            xmlDocProvider = provider;
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(xmlFilePath))
                File.Delete(xmlFilePath);
        }

        #region WithCaching Tests

        [Test]
        public void WithCaching_WithNullProvider_ThrowsArgumentNullException()
        {
            Assert.That(() => ((IXmlDocProvider)null!).WithCaching(),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("xmlDocProvider"));
        }

        [Test]
        public void WithCaching_WithNonCachedProvider_ReturnsCacheWrapper()
        {
            var mockProvider = new Mock<IXmlDocProvider>();
            var result = mockProvider.Object.WithCaching();

            Assert.That(result, Is.InstanceOf<XmlDocProviderCache>());
            Assert.That(result, Is.Not.SameAs(mockProvider.Object));
        }

        [Test]
        public void WithCaching_WithAlreadyCachedProvider_ReturnsSameInstance()
        {
            var mockProvider = new Mock<IXmlDocProvider>();
            var cache = new XmlDocProviderCache(mockProvider.Object);
            var result = cache.WithCaching();

            Assert.That(result, Is.SameAs(cache));
        }

        #endregion

        #region TryGetNamespaceDoc Tests

        [Test]
        public void TryGetNamespaceDoc_WithNullProvider_ThrowsArgumentNullException()
        {
            Assert.That(() => ((IXmlDocProvider)null!).TryGetNamespaceDoc("Acme", out _),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("xmlDocProvider"));
        }

        [Test]
        public void TryGetNamespaceDoc_WithNullNamespace_ThrowsArgumentException()
        {
            Assert.That(() => xmlDocProvider.TryGetNamespaceDoc(null!, out _),
                Throws.ArgumentException.With.Property("ParamName").EqualTo("ns"));
        }

        [Test]
        public void TryGetNamespaceDoc_WithEmptyNamespace_ThrowsArgumentException()
        {
            Assert.That(() => xmlDocProvider.TryGetNamespaceDoc(string.Empty, out _),
                Throws.ArgumentException.With.Property("ParamName").EqualTo("ns"));
        }

        [Test]
        public void TryGetNamespaceDoc_WithWhitespaceNamespace_ThrowsArgumentException()
        {
            Assert.That(() => xmlDocProvider.TryGetNamespaceDoc("   ", out _),
                Throws.ArgumentException.With.Property("ParamName").EqualTo("ns"));
        }

        [Test]
        public void TryGetNamespaceDoc_WithExistingNamespace_ReturnsCorrectDocumentation()
        {
            var result = xmlDocProvider.TryGetNamespaceDoc("Acme", out var doc);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(doc!.Summary.ToString(), Is.EqualTo("Documentation for the Acme namespace."));
            }
        }

        [Test]
        public void TryGetNamespaceDoc_WithNonExistingNamespace_ReturnsFalse()
        {
            var result = xmlDocProvider.TryGetNamespaceDoc("NonExistent.Namespace", out var doc);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(doc, Is.Null);
            }
        }

        #endregion

        #region TryGetMemberDoc Tests

        [Test]
        public void TryGetMemberDoc_WithNullProvider_ThrowsArgumentNullException()
        {
            var member = typeof(Acme.ISampleInterface).GetMetadata();

            Assert.That(() => ((IXmlDocProvider)null!).TryGetMemberDoc(member, out _),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("xmlDocProvider"));
        }

        [Test]
        public void TryGetMemberDoc_WithNullMember_ThrowsArgumentNullException()
        {
            Assert.That(() => xmlDocProvider.TryGetMemberDoc(null!, out _),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("member"));
        }

        [Test]
        public void TryGetMemberDoc_WithDocumentedMember_ReturnsCorrectDocumentation()
        {
            var member = typeof(Acme.ISampleInterface).GetMetadata();

            var result = xmlDocProvider.TryGetMemberDoc(member, out var doc);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(doc!.Summary.ToString(), Is.EqualTo("A documented type but without member documentation."));
            }
        }

        [Test]
        public void TryGetMemberDoc_WithUndocumentedMember_ReturnsFalse()
        {
            var member = typeof(Acme.SampleAttribute).GetMetadata();

            var result = xmlDocProvider.TryGetMemberDoc(member, out var doc);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(doc, Is.Null);
            }
        }

        #endregion

        #region InspectDocumentation Tests

        [Test]
        public void InspectDocumentation_WithNullProvider_ThrowsArgumentNullException()
        {
            var member = typeof(Acme.ISampleInterface).GetMetadata();

            Assert.That(() => ((IXmlDocProvider)null!).InspectDocumentation(member),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("xmlDocProvider"));
        }

        [Test]
        public void InspectDocumentation_WithNullMember_ThrowsArgumentNullException()
        {
            Assert.That(() => xmlDocProvider.InspectDocumentation(null!),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("member"));
        }

        [Test]
        public void InspectDocumentation_WithNoneOptions_ReturnsEmpty()
        {
            var member = typeof(Acme.ISampleInterface).GetMetadata();

            var issues = xmlDocProvider.InspectDocumentation(member, XmlDocInspectionOptions.None).ToList();

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void InspectDocumentation_Documented_WithRequiredOnly_ReturnsNoIssues()
        {
            var member = typeof(Acme.ISampleInterface).GetMetadata();

            var issues = xmlDocProvider.InspectDocumentation(member, XmlDocInspectionOptions.Required).ToList();

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void InspectDocumentation_MissingSummary_WithRequiredOption_ReportsIssue()
        {
            var type = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod));

            var summaryIssues = xmlDocProvider
                .InspectDocumentation(member, XmlDocInspectionOptions.Required)
                .Where(i => i.XmlTag == XmlDocTag.Summary)
                .ToList();

            Assert.That(summaryIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(summaryIssues[0].IssueType, Is.EqualTo(XmlDocInspectionIssueType.MissingRequiredTag));
                Assert.That(summaryIssues[0].Member, Is.EqualTo(member));
            }
        }

        [Test]
        public void InspectDocumentation_MissingTypeParam_WithRequiredOption_ReportsIssue()
        {
            var type = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod));

            var typeParamIssues = xmlDocProvider
                .InspectDocumentation(member, XmlDocInspectionOptions.Required)
                .Where(i => i.XmlTag == XmlDocTag.TypeParam)
                .ToList();

            Assert.That(typeParamIssues, Is.Not.Empty);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(typeParamIssues.All(i => i.IssueType == XmlDocInspectionIssueType.MissingRequiredTag));
                Assert.That(typeParamIssues.All(i => i.Member == member));
                Assert.That(typeParamIssues.All(i => i.TypeParameter is not null));
            }
        }

        [Test]
        public void InspectDocumentation_MissingParam_WithRequiredOption_ReportsIssue()
        {
            var type = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod));

            var paramIssues = xmlDocProvider
                .InspectDocumentation(member, XmlDocInspectionOptions.Required)
                .Where(i => i.XmlTag == XmlDocTag.Param)
                .ToList();

            Assert.That(paramIssues, Is.Not.Empty);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(paramIssues.All(i => i.IssueType == XmlDocInspectionIssueType.MissingRequiredTag));
                Assert.That(paramIssues.All(i => i.Member == member));
                Assert.That(paramIssues.All(i => i.Parameter is not null));
            }
        }

        [Test]
        public void InspectDocumentation_MissingReturns_WithRequiredOption_ReportsIssue()
        {
            var type = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod));

            var returnsIssues = xmlDocProvider
                .InspectDocumentation(member, XmlDocInspectionOptions.Required)
                .Where(i => i.XmlTag == XmlDocTag.Returns)
                .ToList();

            Assert.That(returnsIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(returnsIssues.All(i => i.IssueType == XmlDocInspectionIssueType.MissingRequiredTag));
                Assert.That(returnsIssues.All(i => i.Member == member));
                Assert.That(returnsIssues.All(i => i.Parameter is not null));
            }

        }

        [Test]
        public void InspectDocumentation_MissingRemarks_WithRemarksOption_ReportsIssue()
        {
            var type = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod));

            var remarksIssues = xmlDocProvider
                .InspectDocumentation(member, XmlDocInspectionOptions.Remarks)
                .Where(i => i.XmlTag == XmlDocTag.Remarks)
                .ToList();

            Assert.That(remarksIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(remarksIssues.All(i => i.IssueType == XmlDocInspectionIssueType.MissingOptionalTag));
                Assert.That(remarksIssues.All(i => i.Member == member));
            }
        }

        [Test]
        public void InspectDocumentation_MissingExample_WithExampleOption_ReportsIssue()
        {
            var type = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod));

            var exampleIssues = xmlDocProvider
                .InspectDocumentation(member, XmlDocInspectionOptions.Example)
                .Where(i => i.XmlTag == XmlDocTag.Example)
                .ToList();

            Assert.That(exampleIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exampleIssues.All(i => i.IssueType == XmlDocInspectionIssueType.MissingOptionalTag));
                Assert.That(exampleIssues.All(i => i.Member == member));
            }
        }

        [Test]
        public void InspectDocumentation_MissingException_WithExceptionOption_ReportsIssue()
        {
            var type = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod));

            var exceptionIssues = xmlDocProvider
                .InspectDocumentation(member, XmlDocInspectionOptions.Exception)
                .Where(i => i.XmlTag == XmlDocTag.Exception)
                .ToList();

            Assert.That(exceptionIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exceptionIssues.All(i => i.IssueType == XmlDocInspectionIssueType.UndocumentedReference));
                Assert.That(exceptionIssues.All(i => i.Member == member));
                Assert.That(exceptionIssues.All(i => i.CodeReference is not null));
            }
        }

        [Test]
        public void InspectDocumentation_MissingPermission_WithPermissionOption_ReportsIssue()
        {
            var type = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod));

            var permissionIssues = xmlDocProvider
                .InspectDocumentation(member, XmlDocInspectionOptions.Permission)
                .Where(i => i.XmlTag == XmlDocTag.Permission)
                .ToList();

            Assert.That(permissionIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(permissionIssues.All(i => i.IssueType == XmlDocInspectionIssueType.UndocumentedReference));
                Assert.That(permissionIssues.All(i => i.Member == member));
                Assert.That(permissionIssues.All(i => i.CodeReference is not null));
            }
        }

        [Test]
        public void InspectDocumentation_MissingEvent_WithEventOption_ReportsIssue()
        {
            var type = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod));

            var eventIssues = xmlDocProvider
                .InspectDocumentation(member, XmlDocInspectionOptions.Event)
                .Where(i => i.XmlTag == XmlDocTag.Event);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(eventIssues.All(i => i.IssueType == XmlDocInspectionIssueType.UndocumentedReference));
                Assert.That(eventIssues.All(i => i.Member == member));
                Assert.That(eventIssues.All(i => i.CodeReference is not null));
            }
        }

        [Test]
        public void InspectDocumentation_MissingSeeAlso_WithSeeAlsoOption_ReportsIssue()
        {
            var type = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod));

            var seeAlsoIssues = xmlDocProvider
                .InspectDocumentation(member, XmlDocInspectionOptions.SeeAlso)
                .Where(i => i.XmlTag == XmlDocTag.SeeAlso)
                .ToList();

            Assert.That(seeAlsoIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(seeAlsoIssues[0].IssueType, Is.EqualTo(XmlDocInspectionIssueType.UntitledSeeAlso));
                Assert.That(seeAlsoIssues[0].Member, Is.EqualTo(member));
                Assert.That(seeAlsoIssues[0].Hyperlink, Is.Not.Null);
            }
        }

        [Test]
        public void InspectDocumentation_MissingThreadSafety_WithThreadSafetyOption_ReportsIssue()
        {
            var member = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();

            var threadSafetyIssues = xmlDocProvider
                .InspectDocumentation(member, XmlDocInspectionOptions.ThreadSafety)
                .Where(i => i.XmlTag == XmlDocTag.ThreadSafety)
                .ToList();

            Assert.That(threadSafetyIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(threadSafetyIssues.All(i => i.IssueType == XmlDocInspectionIssueType.MissingOptionalTag));
                Assert.That(threadSafetyIssues.All(i => i.Member == member));
            }
        }

        [Test]
        public void InspectDocumentation_MissingOverloads_WithOverloadsOption_ReportsIssue()
        {
            var type = typeof(Acme.SampleMethods).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(Acme.SampleMethods.OverloadedMethod));

            var overloadsIssues = xmlDocProvider
                .InspectDocumentation(member, XmlDocInspectionOptions.Overloads)
                .Where(i => i.XmlTag == XmlDocTag.Overloads)
                .ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(overloadsIssues.All(i => i.IssueType == XmlDocInspectionIssueType.MissingOptionalTag));
                Assert.That(overloadsIssues.All(i => i.Member == member));
            }
        }

        [Test]
        public void InspectDocumentation_MultipleOptions_ReportsAllIssues()
        {
            var type = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod));

            var issues = xmlDocProvider.InspectDocumentation(member, XmlDocInspectionOptions.Remarks | XmlDocInspectionOptions.Example).ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(issues.Any(i => i.XmlTag == XmlDocTag.Remarks), "Should report missing remarks");
                Assert.That(issues.Any(i => i.XmlTag == XmlDocTag.Example), "Should report missing example");
            }
        }

        [Test]
        public void InspectDocumentation_WithAllOptions_ReportsAllRelevantMissingTags()
        {
            var type = typeof(Acme.SampleDerivedGenericClass<,,>).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(Acme.SampleDerivedGenericClass<,,>.GenericMethod));

            var issues = xmlDocProvider.InspectDocumentation(member, XmlDocInspectionOptions.All).ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(issues.Any(i => i.XmlTag == XmlDocTag.Summary), "Should report missing summary");
                Assert.That(issues.Any(i => i.XmlTag == XmlDocTag.TypeParam), "Should report undocumented typeparam");
                Assert.That(issues.Any(i => i.XmlTag == XmlDocTag.Param), "Should report undocumented param");
                Assert.That(issues.Any(i => i.XmlTag == XmlDocTag.Returns), "Should report undocumented returns");
                Assert.That(issues.Any(i => i.XmlTag == XmlDocTag.Remarks), "Should report missing remarks");
                Assert.That(issues.Any(i => i.XmlTag == XmlDocTag.Example), "Should report missing example");
                Assert.That(issues.Any(i => i.XmlTag == XmlDocTag.SeeAlso), "Should report untitled seealso");
                Assert.That(issues.Any(i => i.XmlTag == XmlDocTag.Exception), "Should report undocumented exceptions");
                Assert.That(issues.Any(i => i.XmlTag == XmlDocTag.Permission), "Should report undocumented permissions");
                Assert.That(issues.Any(i => i.XmlTag == XmlDocTag.Event), "Should report undocumented events");
            }
        }

        [Test]
        public void InspectDocumentation_EnumValue_DoesNotReportOptionalTags()
        {
            var enumType = typeof(DayOfWeek).GetMetadata<IEnumType>();
            var enumValue = enumType.Fields[0];

            var issues = xmlDocProvider.InspectDocumentation(enumValue, XmlDocInspectionOptions.All).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(issues[0].IssueType, Is.EqualTo(XmlDocInspectionIssueType.MissingRequiredTag));
                Assert.That(issues[0].XmlTag, Is.EqualTo(XmlDocTag.Summary));
                Assert.That(issues[0].Member, Is.EqualTo(enumValue));
            }
        }

        [Test]
        public void InspectDocumentation_ImplicitConstructor_WithoutOmitFlag_ReportsMissingSummary()
        {
            var type = typeof(Acme.SampleGenericClass<>).GetMetadata<IClassType>();
            var implicitConstructor = type.Constructors.First(c => c.IsDefaultConstructor);

            var issues = xmlDocProvider.InspectDocumentation(implicitConstructor, XmlDocInspectionOptions.Summary).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(issues[0].IssueType, Is.EqualTo(XmlDocInspectionIssueType.MissingRequiredTag));
                Assert.That(issues[0].XmlTag, Is.EqualTo(XmlDocTag.Summary));
                Assert.That(issues[0].Member, Is.EqualTo(implicitConstructor));
            }
        }

        [Test]
        public void InspectDocumentation_ImplicitConstructor_WithOmitFlag_ReportsNoIssues()
        {
            var type = typeof(Acme.SampleDirectDerivedConstructedGenericClass).GetMetadata<IClassType>();
            var implicitConstructor = type.Constructors.First(c => c.IsDefaultConstructor);

            var issues = xmlDocProvider.InspectDocumentation(implicitConstructor, XmlDocInspectionOptions.Summary | XmlDocInspectionOptions.OmitImplicitlyCreatedConstructors).ToList();

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void InspectDocumentation_ExplicitConstructor_WithOmitFlag_ReportsMissingSummary()
        {
            var type = typeof(Acme.SampleDerivedConstructedGenericClass).GetMetadata<IClassType>();
            var explicitConstructor = type.Constructors.First(c => c.IsDefaultConstructor);

            var issues = xmlDocProvider.InspectDocumentation(explicitConstructor, XmlDocInspectionOptions.Summary | XmlDocInspectionOptions.OmitImplicitlyCreatedConstructors).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(issues[0].IssueType, Is.EqualTo(XmlDocInspectionIssueType.MissingRequiredTag));
                Assert.That(issues[0].XmlTag, Is.EqualTo(XmlDocTag.Summary));
                Assert.That(issues[0].Member, Is.EqualTo(explicitConstructor));
            }
        }

        #endregion
    }
}

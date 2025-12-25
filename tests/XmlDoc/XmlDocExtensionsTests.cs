// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.XmlDoc;
    using NUnit.Framework;
    using System;
    using System.IO;
    using System.Linq;

    [TestFixture]
    public class XmlDocExtensionsTests
    {
        private string xmlFilePath = null!;
        private IXmlDocProvider xmlDocProvider = null!;
        private ITypeMember testMember = null!;

        [SetUp]
        public void Setup()
        {
            MetadataProvider.RegisterRuntimeAssemblies();

            const string xmlDoc =
                @"<?xml version='1.0' encoding='utf-8'?>
                <doc>
                    <members>
                        <member name='T:Kampute.DocToolkit.Test.XmlDoc.XmlDocExtensionsTests.TestSample'>
                            <summary>A documented type but without member documentation.</summary>
                        </member>
                        <member name='M:Kampute.DocToolkit.Test.XmlDoc.XmlDocExtensionsTests.TestSample.Member``1(``0)'>
                            <seealso href='https://example.com/'></seealso>
                            <exception cref='System.ArgumentNullException'></exception>
                            <permission cref='System.Security.Permissions.SecurityPermission'></permission>
                            <event cref='System.EventHandler'></event>
                        </member>
                        <member name='M:Kampute.DocToolkit.Test.XmlDoc.XmlDocExtensionsTests.TestSample.OverloadPresent'>
                            <summary>Overload with documentation.</summary> 
                            <overloads>
                                <summary>Overloaded method.</summary>
                            </overloads>
                        </member>
                    </members>
                </doc>";

            xmlFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.xml");
            File.WriteAllText(xmlFilePath, xmlDoc);

            var repository = new XmlDocRepository();
            repository.ImportFile(xmlFilePath);
            xmlDocProvider = new XmlDocProvider(repository);

            testMember = typeof(TestSample).GetMethod(nameof(TestSample.Member))!.GetMetadata();
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(xmlFilePath))
                File.Delete(xmlFilePath);
        }

        [Test]
        public void InspectDocumentation_WithNullProvider_ThrowsArgumentNullException()
        {
            Assert.That(() => ((IXmlDocProvider)null!).InspectDocumentation(testMember),
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
            var issues = xmlDocProvider.InspectDocumentation(testMember, XmlDocInspectionOptions.None).ToList();

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void InspectDocumentation_Documented_WithRequiredOnly_ReturnsNoIssues()
        {
            var issues = xmlDocProvider.InspectDocumentation(typeof(TestSample).GetMetadata(), XmlDocInspectionOptions.Required).ToList();

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void InspectDocumentation_MissingSummaryComment_WithRequiredOption_ReportsIssue()
        {
            var summaryIssues = xmlDocProvider
                .InspectDocumentation(testMember, XmlDocInspectionOptions.Required)
                .Where(static i => i.XmlTag == XmlDocTag.Summary)
                .ToList();

            Assert.That(summaryIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(summaryIssues[0].IssueType, Is.EqualTo(XmlDocInspectionIssueType.MissingRequiredTag));
                Assert.That(summaryIssues[0].Member, Is.EqualTo(testMember));
            }
        }

        [Test]
        public void InspectDocumentation_MissingTypeParamComment_WithRequiredOption_ReportsIssue()
        {
            var typeParamIssues = xmlDocProvider
                .InspectDocumentation(testMember, XmlDocInspectionOptions.Required)
                .Where(i => i.XmlTag == XmlDocTag.TypeParam)
                .ToList();

            Assert.That(typeParamIssues, Is.Not.Empty);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(typeParamIssues.All(i => i.IssueType == XmlDocInspectionIssueType.MissingRequiredTag));
                Assert.That(typeParamIssues.All(i => i.Member == testMember));
                Assert.That(typeParamIssues.All(i => i.TypeParameter is not null));
            }
        }

        [Test]
        public void InspectDocumentation_MissingParamComment_WithRequiredOption_ReportsIssue()
        {
            var paramIssues = xmlDocProvider
                .InspectDocumentation(testMember, XmlDocInspectionOptions.Required)
                .Where(i => i.XmlTag == XmlDocTag.Param)
                .ToList();

            Assert.That(paramIssues, Is.Not.Empty);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(paramIssues.All(i => i.IssueType == XmlDocInspectionIssueType.MissingRequiredTag));
                Assert.That(paramIssues.All(i => i.Member == testMember));
                Assert.That(paramIssues.All(i => i.Parameter is not null));
            }
        }

        [Test]
        public void InspectDocumentation_MissingReturnsComment_WithRequiredOption_ReportsIssue()
        {
            var returnsIssues = xmlDocProvider
                .InspectDocumentation(testMember, XmlDocInspectionOptions.Required)
                .Where(i => i.XmlTag == XmlDocTag.Returns)
                .ToList();

            Assert.That(returnsIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(returnsIssues.All(i => i.IssueType == XmlDocInspectionIssueType.MissingRequiredTag));
                Assert.That(returnsIssues.All(i => i.Member == testMember));
                Assert.That(returnsIssues.All(i => i.Parameter is not null));
            }

        }

        [Test]
        public void InspectDocumentation_MissingRemarksComment_WithRemarksOption_ReportsIssue()
        {
            var remarksIssues = xmlDocProvider
                .InspectDocumentation(testMember, XmlDocInspectionOptions.Remarks)
                .Where(i => i.XmlTag == XmlDocTag.Remarks)
                .ToList();

            Assert.That(remarksIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(remarksIssues.All(i => i.IssueType == XmlDocInspectionIssueType.MissingOptionalTag));
                Assert.That(remarksIssues.All(i => i.Member == testMember));
            }
        }

        [Test]
        public void InspectDocumentation_MissingExampleComment_WithExampleOption_ReportsIssue()
        {
            var exampleIssues = xmlDocProvider
                .InspectDocumentation(testMember, XmlDocInspectionOptions.Example)
                .Where(i => i.XmlTag == XmlDocTag.Example)
                .ToList();

            Assert.That(exampleIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exampleIssues.All(i => i.IssueType == XmlDocInspectionIssueType.MissingOptionalTag));
                Assert.That(exampleIssues.All(i => i.Member == testMember));
            }
        }

        [Test]
        public void InspectDocumentation_MissingExceptionDescription_WithExceptionOption_ReportsIssue()
        {
            var exceptionIssues = xmlDocProvider
                .InspectDocumentation(testMember, XmlDocInspectionOptions.Exception)
                .Where(i => i.XmlTag == XmlDocTag.Exception)
                .ToList();

            Assert.That(exceptionIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(exceptionIssues.All(i => i.IssueType == XmlDocInspectionIssueType.UndocumentedReference));
                Assert.That(exceptionIssues.All(i => i.Member == testMember));
                Assert.That(exceptionIssues.All(i => i.CodeReference is not null));
            }
        }

        [Test]
        public void InspectDocumentation_MissingPermissionDescription_WithPermissionOption_ReportsIssue()
        {
            var permissionIssues = xmlDocProvider
                .InspectDocumentation(testMember, XmlDocInspectionOptions.Permission)
                .Where(i => i.XmlTag == XmlDocTag.Permission)
                .ToList();

            Assert.That(permissionIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(permissionIssues.All(i => i.IssueType == XmlDocInspectionIssueType.UndocumentedReference));
                Assert.That(permissionIssues.All(i => i.Member == testMember));
                Assert.That(permissionIssues.All(i => i.CodeReference is not null));
            }
        }

        [Test]
        public void InspectDocumentation_MissingEventDescription_WithEventOption_ReportsIssue()
        {
            var eventIssues = xmlDocProvider
                .InspectDocumentation(testMember, XmlDocInspectionOptions.Event)
                .Where(i => i.XmlTag == XmlDocTag.Event);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(eventIssues.All(i => i.IssueType == XmlDocInspectionIssueType.UndocumentedReference));
                Assert.That(eventIssues.All(i => i.Member == testMember));
                Assert.That(eventIssues.All(i => i.CodeReference is not null));
            }
        }

        [Test]
        public void InspectDocumentation_MissingHyperlinkSeeAlsoDescription_WithSeeAlsoOption_ReportsIssue()
        {
            var seeAlsoIssues = xmlDocProvider
                .InspectDocumentation(testMember, XmlDocInspectionOptions.SeeAlso)
                .Where(static i => i.XmlTag == XmlDocTag.SeeAlso)
                .ToList();

            Assert.That(seeAlsoIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(seeAlsoIssues[0].IssueType, Is.EqualTo(XmlDocInspectionIssueType.UntitledSeeAlso));
                Assert.That(seeAlsoIssues[0].Member, Is.EqualTo(testMember));
                Assert.That(seeAlsoIssues[0].Hyperlink, Is.Not.Null);
            }
        }

        [Test]
        public void InspectDocumentation_MissingThreadSafetyComment_WithThreadSafetyOption_ReportsIssue()
        {
            var type = typeof(TestSample).GetMetadata<IClassType>();

            var threadSafetyIssues = xmlDocProvider
                .InspectDocumentation(type, XmlDocInspectionOptions.ThreadSafety)
                .Where(i => i.XmlTag == XmlDocTag.ThreadSafety)
                .ToList();

            Assert.That(threadSafetyIssues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(threadSafetyIssues.All(i => i.IssueType == XmlDocInspectionIssueType.MissingOptionalTag));
                Assert.That(threadSafetyIssues.All(i => i.Member == type));
            }
        }

        [Test]
        public void InspectDocumentation_MissingOverloads_WithOverloadsOption_ReportsIssue()
        {
            var type = typeof(TestSample).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(TestSample.OverloadAbsent));

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
        public void InspectDocumentation_HavingOverloads_WithOverloadsOption_ReportsNoIssue()
        {
            var type = typeof(TestSample).GetMetadata<IClassType>();
            var member = type.Methods.First(m => m.Name == nameof(TestSample.OverloadPresent));

            var overloadsIssues = xmlDocProvider
                .InspectDocumentation(member, XmlDocInspectionOptions.Overloads)
                .Where(i => i.XmlTag == XmlDocTag.Overloads)
                .ToList();

            Assert.That(overloadsIssues, Is.Empty);
        }

        [Test]
        public void InspectDocumentation_MultipleOptions_ReportsAllIssues()
        {
            var issues = xmlDocProvider.InspectDocumentation(testMember, XmlDocInspectionOptions.Remarks | XmlDocInspectionOptions.Example).ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(issues.Any(static i => i.XmlTag == XmlDocTag.Remarks), "Should report missing remarks");
                Assert.That(issues.Any(static i => i.XmlTag == XmlDocTag.Example), "Should report missing example");
            }
        }

        [Test]
        public void InspectDocumentation_WithAllOptions_ReportsAllRelevantMissingTags()
        {
            var issues = xmlDocProvider.InspectDocumentation(testMember, XmlDocInspectionOptions.All).ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(issues.Any(static i => i.XmlTag == XmlDocTag.Summary), "Should report missing summary");
                Assert.That(issues.Any(static i => i.XmlTag == XmlDocTag.TypeParam), "Should report undocumented typeparam");
                Assert.That(issues.Any(static i => i.XmlTag == XmlDocTag.Param), "Should report undocumented param");
                Assert.That(issues.Any(static i => i.XmlTag == XmlDocTag.Returns), "Should report undocumented returns");
                Assert.That(issues.Any(static i => i.XmlTag == XmlDocTag.Remarks), "Should report missing remarks");
                Assert.That(issues.Any(static i => i.XmlTag == XmlDocTag.Example), "Should report missing example");
                Assert.That(issues.Any(static i => i.XmlTag == XmlDocTag.SeeAlso), "Should report untitled seealso");
                Assert.That(issues.Any(static i => i.XmlTag == XmlDocTag.Exception), "Should report undocumented exceptions");
                Assert.That(issues.Any(static i => i.XmlTag == XmlDocTag.Permission), "Should report undocumented permissions");
                Assert.That(issues.Any(static i => i.XmlTag == XmlDocTag.Event), "Should report undocumented events");
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
            var implicitConstructor = type.Constructors.First(static c => c.IsDefaultConstructor);

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
            var implicitConstructor = type.Constructors.First(static c => c.IsDefaultConstructor);

            var issues = xmlDocProvider.InspectDocumentation(implicitConstructor, XmlDocInspectionOptions.Summary | XmlDocInspectionOptions.OmitImplicitlyCreatedConstructors).ToList();

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void InspectDocumentation_ExplicitConstructor_WithOmitFlag_ReportsMissingSummary()
        {
            var type = typeof(Acme.SampleDerivedConstructedGenericClass).GetMetadata<IClassType>();
            var explicitConstructor = type.Constructors.First(static c => c.IsDefaultConstructor);

            var issues = xmlDocProvider.InspectDocumentation(explicitConstructor, XmlDocInspectionOptions.Summary | XmlDocInspectionOptions.OmitImplicitlyCreatedConstructors).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(issues[0].IssueType, Is.EqualTo(XmlDocInspectionIssueType.MissingRequiredTag));
                Assert.That(issues[0].XmlTag, Is.EqualTo(XmlDocTag.Summary));
                Assert.That(issues[0].Member, Is.EqualTo(explicitConstructor));
            }
        }

        private static class TestSample
        {
            public static T Member<T>(T x) => throw new NotImplementedException();

            public static void OverloadAbsent() => throw new NotImplementedException();
            public static void OverloadAbsent(int x) => throw new NotImplementedException();

            public static void OverloadPresent() => throw new NotImplementedException();
            public static void OverloadPresent(int x) => throw new NotImplementedException();
        }
    }
}

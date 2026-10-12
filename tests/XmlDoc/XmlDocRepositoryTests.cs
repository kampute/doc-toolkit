// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.XmlDoc
{
    using Kampute.DocToolkit.XmlDoc;
    using Moq;
    using NUnit.Framework;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    [TestFixture]
    public class XmlDocRepositoryTests
    {
        private const string XmlDoc =
          @"<?xml version='1.0' encoding='utf-8'?>
            <doc>
                <members>
                    <member name='M:System.Object.ToString'>
                        <summary>Returns a string that represents the current object.</summary>
                    </member>
                    <member name='M:System.String.ToString'>
                        <inheritdoc/>
                    </member>
                    <member name='M:System.Int32.ToString'>
                        <inheritdoc cref='M:System.Decimal.ToString'/>
                    </member>
                    <member name='M:System.Double.ToString'>
                        <inheritdoc cref='M:Acme.Base.Method(System.Object)' path='/summary'/>
                    </member>
                    <member name='M:System.Single.ToString'>
                        <inheritdoc cref='M:Acme.Base.Method(System.Object)' path='exception[@cref=""T:System.InvalidOperationException""]'/>
                    </member>
                    <member name='M:System.Byte.ToString'>
                        <inheritdoc cref='M:Acme.Base.Method(System.Object)' path='[['/>
                    </member>
                    <member name='M:System.SByte.ToString'>
                        <inheritdoc cref='M:Acme.Base.Method(System.Object)' path='//@cref'/>
                    </member>
                    <member name='M:System.Int16.ToString'>
                        <inheritdoc cref='M:Acme.Base.Method(System.Object)' path='count(*)'/>
                    </member>
                    <member name='M:System.Object.GetHashCode'>
                        <summary>Serves as the default hash function.</summary>
                        <returns>A hash code for the current object.</returns>
                    </member>
                    <member name='M:System.String.GetHashCode'>
                        <inheritdoc path='/returns'/>
                    </member>
                    <member name='M:System.Decimal.ToString'>
                        <include file='included.xml' path='/doc/members/member[@name=""M:System.Decimal.ToString""]/*'/>
                    </member>
                    <member name='M:System.Decimal.Parse(System.String)'>
                        <include file='included.xml' path='/doc/members/member[@name=""M:System.Decimal.Parse(System.String)""]/*'/>
                        <returns>Own returns.</returns>
                    </member>
                    <member name='M:Acme.Base.Method(System.Object)'>
                        <summary>Base summary.</summary>
                        <param name='value'>Base value.</param>
                        <exception cref='T:System.ArgumentException'>Base argument.</exception>
                        <exception cref='T:System.InvalidOperationException'>Base operation.</exception>
                        <seealso href='https://example.com/shared'>Base shared link.</seealso>
                        <seealso href='https://example.com/base'>Base link.</seealso>
                    </member>
                    <member name='M:Acme.Derived.Method(System.Object)'>
                        <inheritdoc cref='M:Acme.Base.Method(System.Object)'/>
                        <param name='value'>Own value.</param>
                        <exception cref='T:System.ArgumentException'>Own argument.</exception>
                    </member>
                    <member name='M:Acme.Derived.Other(System.Object)'>
                        <inheritdoc cref='M:Acme.Base.Method(System.Object)'/>
                        <summary>Own summary.</summary>
                        <seealso href='https://example.com/shared'>Own shared link.</seealso>
                    </member>
                    <member name='M:Acme.Derived.Filtered(System.Object)'>
                        <inheritdoc cref='M:Acme.Base.Method(System.Object)' path='/summary'/>
                        <inheritdoc cref='M:Acme.Base.Method(System.Object)' path='/exception'/>
                    </member>
                    <member name='M:Acme.Exceptions.Base'>
                        <exception cref='T:System.ArgumentException'>The type belongs to another assembly.</exception>
                        <exception cref='T:System.ArgumentException'>The type is nested.</exception>
                    </member>
                    <member name='M:Acme.Exceptions.Derived'>
                        <inheritdoc cref='M:Acme.Exceptions.Base'/>
                    </member>
                    <member name='M:Acme.Exceptions.Filtered'>
                        <inheritdoc cref='M:Acme.Exceptions.Base' path='/exception'/>
                    </member>
                    <member name='M:Acme.Exceptions.Split'>
                        <inheritdoc cref='M:Acme.Exceptions.Base' path='/exception[1]'/>
                        <inheritdoc cref='M:Acme.Exceptions.Base' path='/exception[2]'/>
                    </member>
                    <member name='M:Acme.Exceptions.Overlapping'>
                        <inheritdoc cref='M:Acme.Exceptions.Base' path='/exception[1]'/>
                        <inheritdoc cref='M:Acme.Exceptions.Base' path='/exception'/>
                    </member>
                    <member name='M:Acme.Exceptions.Chained'>
                        <inheritdoc cref='M:Acme.Exceptions.Derived'/>
                    </member>
                    <member name='M:Acme.Exceptions.Own'>
                        <inheritdoc cref='M:Acme.Exceptions.Base'/>
                        <exception cref='T:System.ArgumentException'>Own first condition.</exception>
                        <exception cref='T:System.ArgumentException'>Own second condition.</exception>
                    </member>
                    <member name='M:Acme.Cycle.First'>
                        <summary>First summary.</summary>
                        <inheritdoc cref='M:Acme.Cycle.Second'/>
                    </member>
                    <member name='M:Acme.Cycle.Second'>
                        <returns>Second returns.</returns>
                        <inheritdoc cref='M:Acme.Cycle.First'/>
                    </member>
                </members>
            </doc>";

        private const string IncludedXmlDoc =
          @"<?xml version='1.0' encoding='utf-8'?>
            <doc>
                <members>
                    <member name='M:System.Decimal.ToString'>
                        <summary>Converts the numeric value of this instance to its equivalent string representation.</summary>
                    </member>
                    <member name='M:System.Decimal.Parse(System.String)'>
                        <summary>Included summary.</summary>
                        <param name='s'>Included param.</param>
                    </member>
                </members>
            </doc>";

        private XmlDocRepository repository = null!;

        private string dir = null!;
        private string MainXmlFilePath => Path.Combine(dir, "doc.xml");
        private string IncludedXmlFilePath => Path.Combine(dir, "included.xml");

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(dir);
            File.WriteAllText(MainXmlFilePath, XmlDoc);
            File.WriteAllText(IncludedXmlFilePath, IncludedXmlDoc);
            repository = new XmlDocRepository();
            repository.ImportFile(MainXmlFilePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }

        private string WriteDoc(string members)
        {
            var path = Path.Combine(dir, Path.GetRandomFileName() + ".xml");
            File.WriteAllText(path, $"<?xml version='1.0' encoding='utf-8'?><doc><members>{members}</members></doc>");
            return path;
        }

        [Test]
        public void HasDocumentation_ReturnsTrue()
        {
            Assert.That(repository.HasDocumentation, Is.True);
        }

        [TestCase("M:System.Object.ToString", ExpectedResult = "Returns a string that represents the current object.")] // Direct documentation
        [TestCase("M:System.String.ToString", ExpectedResult = "Returns a string that represents the current object.")] // Inherited documentation by default
        [TestCase("M:System.Int32.ToString", ExpectedResult = "Converts the numeric value of this instance to its equivalent string representation.")] // Inherited documentation by cref
        [TestCase("M:System.Double.ToString", ExpectedResult = "Base summary.")] // Inherited documentation by cref, filtered by rooted path
        [TestCase("M:System.Single.ToString", ExpectedResult = "Base operation.")] // Inherited documentation by cref, filtered by relative path
        [TestCase("M:System.Byte.ToString", ExpectedResult = "")] // Invalid path inherits nothing
        [TestCase("M:System.SByte.ToString", ExpectedResult = "")] // Path selecting attributes inherits nothing
        [TestCase("M:System.Int16.ToString", ExpectedResult = "")] // Path evaluating to a number inherits nothing
        [TestCase("M:System.String.GetHashCode", ExpectedResult = "A hash code for the current object.")] // Inherited documentation by default, filtered by path
        [TestCase("M:System.Decimal.ToString", ExpectedResult = "Converts the numeric value of this instance to its equivalent string representation.")] // Included documentation
        [TestCase("M:System.NonExistent.ToString", ExpectedResult = null)] // Non-existent member
        public string? TryGetXmlDoc_ResolveDocumentation(string cref)
        {
            return repository.TryGetXmlDoc(cref, out var xmlDoc) ? xmlDoc.Value.Trim() : null;
        }

        [Test]
        public void TryGetXmlDoc_WithIncludePathSelectingSeveralElements_IncludesAllOfThemInPlace()
        {
            Assert.That(repository.TryGetXmlDoc("M:System.Decimal.Parse(System.String)", out var xmlDoc), Is.True);

            Assert.That(xmlDoc!.Elements().Select(static e => $"{e.Name}:{e.Value}"), Is.EqualTo(new[]
            {
                "summary:Included summary.",
                "param:Included param.",
                "returns:Own returns.",
            }));
        }

        [TestCase("M:System.Byte.ToString")] // Invalid path
        [TestCase("M:System.SByte.ToString")] // Path selecting attributes only
        [TestCase("M:System.Int16.ToString")] // Path evaluating to a number
        public void TryGetXmlDoc_WithInheritDocPathSelectingNoElements_ReportsInheritDocNotFound(string cref)
        {
            var errorHandler = new Mock<IXmlDocErrorHandler>();
            repository.ErrorHandler = errorHandler.Object;

            Assert.That(repository.TryGetXmlDoc(cref, out var xmlDoc), Is.True);

            errorHandler.Verify(h => h.InheritDocNotFound(xmlDoc!), Times.Once);
        }

        [TestCase(null)]
        [TestCase("")]
        public void ImportFile_WithNullOrEmptyPath_ThrowsArgumentException(string? path)
        {
            Assert.That(() => new XmlDocRepository().ImportFile(path!), Throws.ArgumentException.With.Property("ParamName").EqualTo("xmlDocPath"));
        }

        [Test]
        public void ImportFile_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            Assert.That(() => new XmlDocRepository().ImportFile(Path.Combine(dir, "missing.xml")), Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void ImportFile_WithIncludeOfMissingFileAndNoErrorHandler_ThrowsFileNotFoundException()
        {
            var path = WriteDoc("<member name='M:A.B'><include file='missing.xml' path='/*'/></member>");

            Assert.That(() => new XmlDocRepository().ImportFile(path), Throws.TypeOf<FileNotFoundException>());
        }

        [Test]
        public void ImportFile_WithIncludeOfMissingFileAndErrorHandler_ReportsIncludeFileNotFound()
        {
            var path = WriteDoc("<member name='M:A.B'><include file='missing.xml' path='/*'/></member>");
            var errorHandler = new Mock<IXmlDocErrorHandler>();

            new XmlDocRepository { ErrorHandler = errorHandler.Object }.ImportFile(path);

            errorHandler.Verify(h => h.IncludeFileNotFound(It.Is<XElement>(static e => e.Attribute("name")!.Value == "M:A.B"), "missing.xml"), Times.Once);
        }

        [Test]
        public void ImportFile_WithIncludePathSelectingNothing_ReportsIncludeMemberNotFound()
        {
            var path = WriteDoc("<member name='M:A.B'><include file='included.xml' path='/doc/members/member[@name=\"M:None\"]/*'/></member>");
            var errorHandler = new Mock<IXmlDocErrorHandler>();

            new XmlDocRepository { ErrorHandler = errorHandler.Object }.ImportFile(path);

            errorHandler.Verify(h => h.IncludeMemberNotFound(It.Is<XElement>(static e => e.Attribute("name")!.Value == "M:A.B")), Times.Once);
        }

        [Test]
        public void Import_WithAlreadyImportedMember_ReplacesItsDocumentation()
        {
            repository.Import(XDocument.Parse("<doc><members><member name='M:System.Object.ToString'><summary>Replaced.</summary></member></members></doc>"));

            Assert.That(repository.TryGetXmlDoc("M:System.Object.ToString", out var xmlDoc) ? xmlDoc.Value : null, Is.EqualTo("Replaced."));
        }

        [Test]
        public void TryGetXmlDoc_WithInheritDocAndOwnTags_KeepsOwnTagsAndInheritsMissingOnes()
        {
            Assert.That(repository.TryGetXmlDoc("M:Acme.Derived.Method(System.Object)", out var xmlDoc), Is.True);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(xmlDoc!.Elements("summary").Select(static e => e.Value), Is.EqualTo(new[] { "Base summary." }));
                Assert.That(xmlDoc.Elements("param").Select(static e => e.Value), Is.EqualTo(new[] { "Own value." }));
                Assert.That(xmlDoc.Elements("exception").Select(static e => e.Value), Is.EquivalentTo(new[] { "Own argument.", "Base operation." }));
            }
        }

        [Test]
        public void TryGetXmlDoc_WithInheritDocAndOwnSummaryAndLink_KeepsOwnSummaryAndLink()
        {
            Assert.That(repository.TryGetXmlDoc("M:Acme.Derived.Other(System.Object)", out var xmlDoc), Is.True);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(xmlDoc!.Elements("summary").Select(static e => e.Value), Is.EqualTo(new[] { "Own summary." }));
                Assert.That(xmlDoc.Elements("seealso").Select(static e => e.Value), Is.EquivalentTo(new[] { "Own shared link.", "Base link." }));
            }
        }

        [Test]
        public void TryGetXmlDoc_WithInheritDocsFromSameMemberWithDifferentPaths_InheritsEachSelection()
        {
            Assert.That(repository.TryGetXmlDoc("M:Acme.Derived.Filtered(System.Object)", out var xmlDoc), Is.True);

            Assert.That(xmlDoc!.Elements().Select(static e => e.Value), Is.EquivalentTo(new[] { "Base summary.", "Base argument.", "Base operation." }));
        }

        [TestCase("M:Acme.Exceptions.Derived", ExpectedResult = new[] { "The type belongs to another assembly.", "The type is nested." })]
        [TestCase("M:Acme.Exceptions.Filtered", ExpectedResult = new[] { "The type belongs to another assembly.", "The type is nested." })]
        [TestCase("M:Acme.Exceptions.Split", ExpectedResult = new[] { "The type belongs to another assembly.", "The type is nested." })]
        [TestCase("M:Acme.Exceptions.Overlapping", ExpectedResult = new[] { "The type belongs to another assembly.", "The type is nested." })]
        [TestCase("M:Acme.Exceptions.Chained", ExpectedResult = new[] { "The type belongs to another assembly.", "The type is nested." })]
        [TestCase("M:Acme.Exceptions.Own", ExpectedResult = new[] { "Own first condition.", "Own second condition." })]
        public string[]? TryGetXmlDoc_WithMultipleConditionsForSameException_PreservesConditionsAndLocalOverrides(string cref)
        {
            return repository.TryGetXmlDoc(cref, out var xmlDoc) ? xmlDoc.Elements("exception").Select(static e => e.Value).ToArray() : null;
        }

        [Test]
        public void TryGetXmlDoc_WithCyclicInheritDoc_TerminatesAndMergesBothMembers()
        {
            Assert.That(repository.TryGetXmlDoc("M:Acme.Cycle.First", out var xmlDoc), Is.True);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(xmlDoc!.Elements("summary").Select(static e => e.Value), Is.EqualTo(new[] { "First summary." }));
                Assert.That(xmlDoc.Elements("returns").Select(static e => e.Value), Is.EqualTo(new[] { "Second returns." }));
                Assert.That(xmlDoc.Elements("inheritdoc"), Is.Empty);
            }
        }
    }
}

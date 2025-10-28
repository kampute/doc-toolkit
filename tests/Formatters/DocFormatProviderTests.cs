// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Formatters
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.IO.Writers;
    using Kampute.DocToolkit.XmlDoc;
    using NUnit.Framework;
    using System;
    using System.IO;

    [TestFixture]
    public class DocFormatProviderTests
    {
        [SetUp]
        public void Setup()
        {
            DocFormatProvider.Unregister<TestDocFormatter>();
            DocFormatProvider.Unregister<AnotherTestDocFormatter>();
        }

        [Test]
        public void Register_RegistersFormatterForMultipleExtensions()
        {
            DocFormatProvider.Register(static ext => new TestDocFormatter(ext), ".test", ".tst");

            var formatter1 = DocFormatProvider.GetFormatterByExtension(".test");
            var formatter2 = DocFormatProvider.GetFormatterByExtension(".tst");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(formatter1, Is.TypeOf<TestDocFormatter>());
                Assert.That(formatter2, Is.TypeOf<TestDocFormatter>());
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(formatter1.FileExtension, Is.EqualTo(".test"));
                Assert.That(formatter2.FileExtension, Is.EqualTo(".tst"));
            }
        }

        [Test]
        public void Register_OverwritesExistingRegistration()
        {
            DocFormatProvider.Register(static ext => new TestDocFormatter(ext), ".test");
            DocFormatProvider.Register(static ext => new AnotherTestDocFormatter(ext), ".test");

            var formatter = DocFormatProvider.GetFormatterByExtension(".test");

            Assert.That(formatter, Is.TypeOf<AnotherTestDocFormatter>());
            Assert.That(formatter.FileExtension, Is.EqualTo(".test"));
        }

        [Test]
        public void Unregister_ByFormatterType_RemovesAllAssociatedExtensions()
        {
            DocFormatProvider.Register(static ext => new TestDocFormatter(ext), ".test", ".tst");

            DocFormatProvider.Unregister<TestDocFormatter>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(DocFormatProvider.GetFormatterByExtension(".test"), Is.Null);
                Assert.That(DocFormatProvider.GetFormatterByExtension(".tst"), Is.Null);
            }
        }

        [Test]
        public void Unregister_ByFormatterType_RemovesOnlySpecifiedFormatterType()
        {
            DocFormatProvider.Register(static ext => new TestDocFormatter(ext), ".test");
            DocFormatProvider.Register(static ext => new AnotherTestDocFormatter(ext), ".another");

            DocFormatProvider.Unregister<TestDocFormatter>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(DocFormatProvider.GetFormatterByExtension(".test"), Is.Null);
                Assert.That(DocFormatProvider.GetFormatterByExtension(".another"), Is.TypeOf<AnotherTestDocFormatter>());
            }
        }

        [Test]
        public void Unregister_ByExtension_RemovesFormatterForThatExtension()
        {
            DocFormatProvider.Register(static ext => new TestDocFormatter(ext), ".test", ".tst");

            DocFormatProvider.Unregister(".test");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(DocFormatProvider.GetFormatterByExtension(".test"), Is.Null);
                Assert.That(DocFormatProvider.GetFormatterByExtension(".tst"), Is.TypeOf<TestDocFormatter>());
            }
        }

        [Test]
        public void IsRegistered_ForRegisteredExtensions_ReturnsTrue()
        {
            DocFormatProvider.Register(static ext => new TestDocFormatter(ext), ".test");

            Assert.That(DocFormatProvider.IsRegistered(".test"), Is.True);
        }

        [Test]
        public void IsRegistered_ForUnregisteredExtensions__ReturnsFalse()
        {
            Assert.That(DocFormatProvider.IsRegistered(".unknown"), Is.False);
        }

        [Test]
        public void IsRegistered_IsCaseInsensitive()
        {
            DocFormatProvider.Register(static ext => new TestDocFormatter(ext), ".TEST");

            Assert.That(DocFormatProvider.IsRegistered(".test"), Is.True);
        }

        [Test]
        public void GetFormatterByExtension_ReturnsCorrectFormatter()
        {
            DocFormatProvider.Register(static ext => new TestDocFormatter(ext), ".test");

            var formatter = DocFormatProvider.GetFormatterByExtension(".test");

            Assert.That(formatter, Is.TypeOf<TestDocFormatter>());
            Assert.That(formatter.FileExtension, Is.EqualTo(".test"));
        }

        [Test]
        public void GetFormatterByExtension_ReturnsNullForUnregisteredExtension()
        {
            var formatter = DocFormatProvider.GetFormatterByExtension(".unknown");

            Assert.That(formatter, Is.Null);
        }

        [Test]
        public void GetFormatterByExtension_IsCaseInsensitive()
        {
            DocFormatProvider.Register(static ext => new TestDocFormatter(ext), ".TEST");

            var formatter = DocFormatProvider.GetFormatterByExtension(".test");

            Assert.That(formatter, Is.TypeOf<TestDocFormatter>());
            Assert.That(formatter.FileExtension.ToLower(), Is.EqualTo(".test"));
        }

        [TestCase(".md", ExpectedResult = typeof(MarkdownFormat))]
        [TestCase(".markdown", ExpectedResult = typeof(MarkdownFormat))]
        [TestCase(".html", ExpectedResult = typeof(HtmlFormat))]
        [TestCase(".htm", ExpectedResult = typeof(HtmlFormat))]
        [TestCase(".xhtml", ExpectedResult = typeof(HtmlFormat))]
        public Type? BuiltInFormatters_AreRegisteredByDefault(string extension)
        {
            return DocFormatProvider.GetFormatterByExtension(extension)?.GetType();
        }

        private class TestDocFormatter : DocFormatter
        {
            public TestDocFormatter(string extension) : base(extension) { }

            protected override IXmlDocTransformer XmlDocTransformer => throw new NotImplementedException();
            public override void Encode(ReadOnlySpan<char> text, TextWriter writer) => throw new NotImplementedException();
            public override MarkupWriter CreateMarkupWriter(TextWriter writer, bool disposeWriter = false) => throw new NotImplementedException();
        }

        private class AnotherTestDocFormatter : TestDocFormatter
        {
            public AnotherTestDocFormatter(string extension) : base(extension) { }
        }
    }
}

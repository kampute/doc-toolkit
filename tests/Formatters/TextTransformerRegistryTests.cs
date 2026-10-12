// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Formatters
{
    using Kampute.DocToolkit.Formatters;
    using Kampute.DocToolkit.Routing;
    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class TextTransformerRegistryTests
    {
        [Test]
        public void Constructor_WithValidTargetFileExtension_InitializesProperties()
        {
            const string targetExtension = ".html";

            var registry = new TextTransformerRegistry(targetExtension);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(registry.TargetFileExtension, Is.EqualTo(targetExtension));
                Assert.That(registry.SupportedFileExtensions, Is.EqualTo([targetExtension]));
            }
        }

        [Test]
        public void Constructor_WithNullTargetFileExtension_ThrowsArgumentNullException()
        {
            Assert.That(() => new TextTransformerRegistry(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("targetFileExtension"));
        }

        [Test]
        public void Register_WithNullArgument_ThrowsArgumentNullException()
        {
            var registry = new TextTransformerRegistry(".html");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(() => registry.Register(null!, ".md"), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("transformer"));
                Assert.That(() => registry.Register(new TestTransformer(), (string[])null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("fileExtensions"));
            }
        }

        [Test]
        public void CanTransform_WithUnsupportedFileExtension_ReturnsFalse()
        {
            var registry = new TextTransformerRegistry(".html");

            var result = registry.CanTransform(".unknown");

            Assert.That(result, Is.False);
        }

        [Test]
        public void CanTransform_WithNullFileExtension_ReturnsFalse()
        {
            var registry = new TextTransformerRegistry(".html");

            var result = registry.CanTransform(null!);

            Assert.That(result, Is.False);
        }

        [Test]
        public void TryGet_WithSupportedFileExtension_ReturnsTrueAndTransformer()
        {
            var registry = new TextTransformerRegistry(".html");
            var testTransformer = new TestTransformer();
            registry.Register(testTransformer, ".md");

            var result = registry.TryGet(".md", out var transformer);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformer, Is.SameAs(testTransformer));
            }
        }

        [Test]
        public void TryGet_WithTargetFileExtension_ReturnsTrueAndIdentityTransformer()
        {
            var registry = new TextTransformerRegistry(".html");

            var result = registry.TryGet(".html", out var transformer);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(transformer, Is.SameAs(IdentityTransformer.Instance));
            }
        }

        [Test]
        public void TryGet_WithUnsupportedFileExtension_ReturnsFalseAndNullTransformer()
        {
            var registry = new TextTransformerRegistry(".html");

            var result = registry.TryGet(".unknown", out var transformer);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(transformer, Is.Null);
            }
        }

        [Test]
        public void Register_WithExistingExtension_ReplacesTransformer()
        {
            var registry = new TextTransformerRegistry(".html");
            var original = new TestTransformer();
            var replacement = new TestTransformer();
            registry.Register(original, ".md");

            registry.Register(replacement, ".md");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(registry.TryGet(".md", out var transformer), Is.True);
                Assert.That(transformer, Is.SameAs(replacement));
                Assert.That(registry.SupportedFileExtensions, Is.EquivalentTo([".html", ".md"]));
            }
        }

        [Test]
        public void Register_WithTargetExtension_OverridesIdentityAndCannotBeRemoved()
        {
            var registry = new TextTransformerRegistry(".html");
            var custom = new TestTransformer();
            registry.Register(custom, ".HTML", ".md");

            Assert.That(registry.Remove(".HTML"), Is.False);
            registry.Remove<TestTransformer>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(registry.TryGet(".html", out var transformer), Is.True);
                Assert.That(transformer, Is.SameAs(custom));
                Assert.That(registry.CanTransform(".md"), Is.False);
                Assert.That(registry.SupportedFileExtensions, Is.EqualTo([".html"]));
            }
        }

        [Test]
        public void RegisteredExtensions_AreCaseInsensitiveForLookupAndRemoval()
        {
            var registry = new TextTransformerRegistry(".html");
            var custom = new TestTransformer();
            registry.Register(custom, ".MD");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(registry.CanTransform(".md"), Is.True);
                Assert.That(registry.TryGet(".md", out var transformer), Is.True);
                Assert.That(transformer, Is.SameAs(custom));
                Assert.That(registry.IsTargetFileExtension(".HTML"), Is.True);
            }

            using (Assert.EnterMultipleScope())
            {
                Assert.That(registry.Remove(".md"), Is.True);
                Assert.That(registry.CanTransform(".MD"), Is.False);
            }
        }

        [Test]
        public void Register_NonGeneric_WithValidParameters_AddsTransformersToRegistry()
        {
            var registry = new TextTransformerRegistry(".html");
            var extensions = new[] { ".md", ".txt" };

            registry.Register(new TestTransformer(), extensions);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(registry.CanTransform(".md"), Is.True);
                Assert.That(registry.CanTransform(".txt"), Is.True);
                Assert.That(registry.SupportedFileExtensions, Is.EquivalentTo([".html", ".md", ".txt"]));
            }
        }

        [Test]
        public void Register_NonGeneric_WithMixedValidAndNullExtensions_RegistersOnlyValidExtensions()
        {
            var registry = new TextTransformerRegistry(".html");
            var extensions = new[] { ".md", null!, ".txt" };

            registry.Register(new TestTransformer(), extensions);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(registry.CanTransform(".md"), Is.True);
                Assert.That(registry.CanTransform(".txt"), Is.True);
                Assert.That(registry.CanTransform(null!), Is.False);
                Assert.That(registry.SupportedFileExtensions, Is.EquivalentTo([".html", ".md", ".txt"]));
            }
        }

        [Test]
        public void Register_Generic_WithValidParameters_AddsTransformersToRegistry()
        {
            var registry = new TextTransformerRegistry(".html");
            var extensions = new[] { ".md", ".txt" };

            registry.Register<TestTransformer>(extensions);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(registry.CanTransform(".md"), Is.True);
                Assert.That(registry.CanTransform(".txt"), Is.True);
                Assert.That(registry.TryGet(".md", out var mdTransformer), Is.True);
                Assert.That(mdTransformer, Is.TypeOf<TestTransformer>());
                Assert.That(registry.TryGet(".txt", out var txtTransformer), Is.True);
                Assert.That(txtTransformer, Is.TypeOf<TestTransformer>());
            }
        }

        [Test]
        public void Register_Generic_WithMixedValidAndNullExtensions_RegistersOnlyValidExtensions()
        {
            var registry = new TextTransformerRegistry(".html");
            var extensions = new[] { ".md", null!, ".txt" };

            registry.Register<TestTransformer>(extensions);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(registry.CanTransform(".md"), Is.True);
                Assert.That(registry.CanTransform(".txt"), Is.True);
                Assert.That(registry.CanTransform(null!), Is.False);
                Assert.That(registry.SupportedFileExtensions, Is.EquivalentTo([".html", ".md", ".txt"]));
            }
        }

        [Test]
        public void Remove_NonGeneric_WithRegisteredFileExtension_RemovesAndReturnsTrue()
        {
            var registry = new TextTransformerRegistry(".html");
            registry.Register(new TestTransformer(), ".md");

            var result = registry.Remove(".md");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(registry.CanTransform(".md"), Is.False);
                Assert.That(registry.SupportedFileExtensions, Is.EqualTo([".html"]));
            }
        }

        [Test]
        public void Remove_NonGeneric_WithUnregisteredFileExtension_ReturnsFalse()
        {
            var registry = new TextTransformerRegistry(".html");

            var result = registry.Remove(".unknown");

            Assert.That(result, Is.False);
        }

        [Test]
        public void Remove_Generic_RemovesAllTransformersOfSpecifiedType()
        {
            var registry = new TextTransformerRegistry(".html");
            var transformer1 = new TestTransformer();
            var transformer2 = IdentityTransformer.Instance;

            registry.Register(transformer1, ".md", ".txt");
            registry.Register(transformer2, ".rst");

            registry.Remove<TestTransformer>();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(registry.CanTransform(".md"), Is.False);
                Assert.That(registry.CanTransform(".txt"), Is.False);
                Assert.That(registry.CanTransform(".rst"), Is.True);
                Assert.That(registry.SupportedFileExtensions, Is.EquivalentTo([".html", ".rst"]));
            }
        }

        [Test]
        public void IsTargetFileExtension_WithNonTargetFileExtension_ReturnsFalse()
        {
            var registry = new TextTransformerRegistry(".html");

            var result = registry.IsTargetFileExtension(".xhtml");

            Assert.That(result, Is.False);
        }

        private class TestTransformer : ITextTransformer
        {
            public void Transform(TextReader reader, TextWriter writer, IUrlTransformer? urlTransformer = null) => throw new System.NotImplementedException();
        }
    }
}

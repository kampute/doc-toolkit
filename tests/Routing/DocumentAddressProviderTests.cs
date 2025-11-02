// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Routing
{
    using Kampute.DocToolkit;
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Routing;
    using NUnit.Framework;
    using System;

    [TestFixture]
    public class DocumentAddressProviderTests
    {
        [Test]
        public void TryGetMemberUrl_ForNonExternalReference_ReturnsUrl()
        {
            var member = typeof(Console).GetMethod(nameof(Console.WriteLine), [typeof(string)])!.GetMetadata();
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([member.Assembly]);

            var expectedUrl = new Uri("api/system.console.writeline.html#system-console-writeline(system-string)", UriKind.Relative);

            var result = addressProvider.TryGetMemberUrl(member, out var url);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(url, Is.EqualTo(expectedUrl));
            }
        }

        [Test]
        public void TryGetMemberUrl_ForExternalReferences_ReturnsUrl()
        {
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([]);
            addressProvider.ExternalReferences.Add(new MicrosoftDocs());

            var member = typeof(Console).GetMethod(nameof(Console.WriteLine), [typeof(string)])!.GetMetadata();
            var expectedUrl = new Uri("https://learn.microsoft.com/dotnet/api/system.console.writeline#system-console-writeline(system-string)");

            var result = addressProvider.TryGetMemberUrl(member, out var url);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(url, Is.EqualTo(expectedUrl));
            }
        }

        [Test]
        public void TryGetMemberUrl_WithBaseUrl_ReturnsAbsoluteUrl()
        {
            var baseUrl = new Uri("https://docs.example.com/");
            var member = typeof(Console).GetMethod(nameof(Console.WriteLine), [typeof(string)])!.GetMetadata();
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([member.Assembly], baseUrl);

            var expectedUrl = new Uri(baseUrl, "api/system.console.writeline.html#system-console-writeline(system-string)");

            var result = addressProvider.TryGetMemberUrl(member, out var url);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(url, Is.EqualTo(expectedUrl));
            }
        }

        [Test]
        public void TryGetNamespaceUrl_ForNonExternalReference_ReturnsUrl()
        {
            var assembly = typeof(System.IO.Stream).Assembly.GetMetadata();
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([assembly]);

            var namespaceName = "System.IO";
            var expectedUrl = new Uri("api/system.io.html", UriKind.Relative);

            var result = addressProvider.TryGetNamespaceUrl(namespaceName, out var url);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(url, Is.EqualTo(expectedUrl));
            }
        }

        [Test]
        public void TryGetNamespaceUrl_ForExternalReferences_ReturnsUrl()
        {
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([]);
            addressProvider.ExternalReferences.Add(new MicrosoftDocs());

            var namespaceName = "System.IO";
            var expectedUrl = new Uri("https://learn.microsoft.com/dotnet/api/system.io");

            var result = addressProvider.TryGetNamespaceUrl(namespaceName, out var url);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(url, Is.EqualTo(expectedUrl));
            }
        }

        [Test]
        public void TryGetNamespaceUrl_WithBaseUrl_ReturnsAbsoluteUrl()
        {
            var baseUrl = new Uri("https://docs.example.com/");
            var assembly = typeof(System.IO.Stream).Assembly.GetMetadata();
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([assembly], baseUrl);

            var namespaceName = "System.IO";
            var expectedUrl = new Uri(baseUrl, "api/system.io.html");

            var result = addressProvider.TryGetNamespaceUrl(namespaceName, out var url);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(url, Is.EqualTo(expectedUrl));
            }
        }

        [Test]
        public void TryGetUrl_Topic_ReturnsUrl()
        {
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([]);

            var topic = MockTopicBuilder.Topic("sample-topic", "Sample Topic").Build();
            var expectedUrl = new Uri("sample-topic.html", UriKind.Relative);

            var result = addressProvider.TryGetTopicUrl(topic, out var url);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(url, Is.EqualTo(expectedUrl));
            }
        }

        [Test]
        public void TryGetUrl_Topic_WithBaseUrl_ReturnsAbsoluteUrl()
        {
            var baseUrl = new Uri("https://docs.example.com/");
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([], baseUrl);

            var topic = MockTopicBuilder.Topic("sample-topic", "Sample Topic").Build();
            var expectedUrl = new Uri(baseUrl, "sample-topic.html");

            var result = addressProvider.TryGetTopicUrl(topic, out var url);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(url, Is.EqualTo(expectedUrl));
            }
        }

        [Test]
        public void TryGetMemberFile_ForNonExternalReference_ReturnsFilePath()
        {
            var member = typeof(Console).GetMethod(nameof(Console.WriteLine), [typeof(string)])!.GetMetadata();
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([member.Assembly]);

            var expectedPath = "api/system.console.writeline.html";

            var result = addressProvider.TryGetMemberFile(member, out var path);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(path, Is.SamePath(expectedPath));
            }
        }

        [Test]
        public void TryGetMemberFile_ForExternalReferences_DoesNotReturnFilePath()
        {
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([]);
            addressProvider.ExternalReferences.Add(new MicrosoftDocs());

            var member = typeof(Console).GetMethod(nameof(Console.WriteLine), [typeof(string)])!.GetMetadata();

            var result = addressProvider.TryGetMemberFile(member, out var path);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(path, Is.Null);
            }
        }

        [Test]
        public void TryGetNamespaceFile_ForNonExternalReference_ReturnsFilePath()
        {
            var assembly = typeof(System.IO.Stream).Assembly.GetMetadata();
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([assembly]);

            var namespaceName = "System.IO";
            var expectedPath = "api/system.io.html";

            var result = addressProvider.TryGetNamespaceFile(namespaceName, out var path);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(path, Is.SamePath(expectedPath));
            }
        }

        [Test]
        public void TryGetNamespaceFile_ForExternalReferences_DoesNotReturnFilePath()
        {
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([]);
            addressProvider.ExternalReferences.Add(new MicrosoftDocs());

            var namespaceName = "System.IO";
            var result = addressProvider.TryGetNamespaceFile(namespaceName, out var path);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(path, Is.Null);
            }
        }

        [Test]
        public void TryGetFilePath_Topic_ReturnsFilePath()
        {
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([]);

            var topic = MockTopicBuilder.Topic("sample-topic", "Sample Topic").Build();
            var expectedPath = "sample-topic.html";

            var result = addressProvider.TryGetTopicFile(topic, out var path);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(path, Is.EqualTo(expectedPath));
            }
        }

        [Test]
        public void BeginScope_SetsActiveScope()
        {
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([]);
            using var scope = addressProvider.BeginScope("system", null);

            Assert.That(addressProvider.ActiveScope, Is.EqualTo(scope));
        }

        [Test]
        public void BeginScope_AffectsRelativeUrls()
        {
            var assembly = typeof(object).Assembly.GetMetadata();
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([assembly]);
            var cref = "T:System.Object";

            using (addressProvider.BeginScope("api/system", null))
            {
                addressProvider.TryGetUrlByCodeReference(cref, out var urlInScope);
                Assert.That(urlInScope!.ToString(), Is.EqualTo("../system.object.html"));
            }

            addressProvider.TryGetUrlByCodeReference(cref, out var urlAfterScope);
            Assert.That(urlAfterScope!.ToString(), Is.EqualTo("api/system.object.html"));
        }

        [Test]
        public void ActiveScope_Default_IsRoot()
        {
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([]);

            Assert.That(addressProvider.ActiveScope, Is.Not.Null);
            Assert.That(addressProvider.ActiveScope.IsRoot, Is.True);
        }

        [Test]
        public void ActiveScope_DisposingScope_ReturnsToPrevious()
        {
            var addressProvider = DocumentAddressProvider.Create<DotNetApiStrategy>([]);
            var initialScope = addressProvider.ActiveScope;

            using (var scope = addressProvider.BeginScope("subfolder", null))
            {
                Assert.That(addressProvider.ActiveScope, Is.EqualTo(scope));
            }

            Assert.That(addressProvider.ActiveScope, Is.SameAs(initialScope));
        }
    }
}

// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Routing
{
    using Kampute.DocToolkit.Routing;
    using NUnit.Framework;
    using System;
    using System.Threading.Tasks;

    [TestFixture]
    public class AbsoluteUrlContextManagerTests
    {
        [Test]
        public void Constructor_WithRelativeBaseUrl_ThrowsArgumentException()
        {
            var relativeUrl = new Uri("/docs/", UriKind.Relative);

            Assert.That(() => new AbsoluteUrlContextManager(relativeUrl), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithAbsoluteBaseUrl_InitializesCorrectly()
        {
            var baseUrl = new Uri("https://example.com/docs/");

            var manager = new AbsoluteUrlContextManager(baseUrl);

            Assert.That(manager.BaseUrl, Is.EqualTo(baseUrl));
        }

        [Test]
        public void Constructor_WithRelativeBaseUrlString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(static () => new AbsoluteUrlContextManager("docs/"));
        }

        [Test]
        public void Constructor_WithAbsoluteBaseUrlStringWithoutTrailingSlash_InitializesCorrectly()
        {
            var baseUrlString = "https://example.com/docs";
            var manager = new AbsoluteUrlContextManager(baseUrlString);

            Assert.That(manager.BaseUrl, Is.EqualTo(new Uri("https://example.com/docs/")));
        }

        [Test]
        public void Constructor_WithAbsoluteBaseUrlStringWithTrailingSlash_InitializesCorrectly()
        {
            var baseUrlString = "https://example.com/docs/";
            var manager = new AbsoluteUrlContextManager(baseUrlString);

            Assert.That(manager.BaseUrl, Is.EqualTo(new Uri("https://example.com/docs/")));
        }

        [Test]
        public void ActiveScope_WhenNoScopeActive_ReturnsDefaultScope()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var manager = new AbsoluteUrlContextManager(baseUrl);

            var scope = manager.ActiveScope;

            Assert.That(scope, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(scope.DocumentationRootUrl, Is.EqualTo(baseUrl));
                Assert.That(scope.Directory, Is.Empty);
            }
        }

        [TestCase("", ExpectedResult = null)]
        [TestCase("#section", ExpectedResult = null)]
        [TestCase("?query=param", ExpectedResult = null)]
        [TestCase("/other/api/index.html", ExpectedResult = null)]
        [TestCase("api/index.html", ExpectedResult = null)]
        [TestCase("~/api/index.html", ExpectedResult = "https://example.com/docs/api/index.html")]
        [TestCase("~/api/namespace/class.html", ExpectedResult = "https://example.com/docs/api/namespace/class.html")]
        [TestCase("~/api/../index.html", ExpectedResult = "https://example.com/docs/index.html")]
        [TestCase("~/../index.html", ExpectedResult = null)]
        [TestCase("https://other.com/api/index.html", ExpectedResult = null)]
        public string? ActiveScope_TryResolveUrl_ReturnsExpectedUrl(string uriString)
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var manager = new AbsoluteUrlContextManager(baseUrl);

            return manager.ActiveScope.TryResolveUrl(uriString, out var transformedUrl) ? transformedUrl : null;
        }

        [Test]
        public void BeginScope_ReturnsValidScope()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var manager = new AbsoluteUrlContextManager(baseUrl);
            var directory = "some/dir";

            var scope = manager.BeginScope(directory, null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(scope, Is.Not.Null);
                Assert.That(scope, Is.SameAs(manager.ActiveScope));
                Assert.That(scope.Directory, Is.EqualTo(directory));
            }
        }

        [Test]
        public void Scope_TryResolveUrl_UsesBaseUrl()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var relativeUrlString = "~/api/index.html";
            var manager = new AbsoluteUrlContextManager(baseUrl);
            var expected = new Uri("https://example.com/docs/api/index.html");

            using var scope = manager.BeginScope("some/dir", null);
            var success = scope.TryResolveUrl(relativeUrlString, out var transformedUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(success, Is.True);
                Assert.That(new Uri(transformedUrl!), Is.EqualTo(expected));
            }
        }

        [Test]
        public void Scope_Dispose_RestoresPreviousScope()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var manager = new AbsoluteUrlContextManager(baseUrl);
            using var scope = manager.BeginScope("api/namespace", null);

            Assert.That(manager.ActiveScope, Is.SameAs(scope));
            using (var nestedScope = manager.BeginScope("api/namespace/class", null))
            {
                Assert.That(manager.ActiveScope, Is.SameAs(nestedScope));
            }
            Assert.That(manager.ActiveScope, Is.SameAs(scope));
        }

        [Test]
        public void Scope_Dispose_CanBeCalledMultipleTimes()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var manager = new AbsoluteUrlContextManager(baseUrl);
            using var scope1 = manager.BeginScope("api", null);
            var scope2 = manager.BeginScope("api/namespace", null);

            scope2.Dispose();
            scope2.Dispose();

            Assert.That(manager.ActiveScope, Is.SameAs(scope1));
        }

        [TestCase("api/index.html", ExpectedResult = "https://example.com/docs/api/index.html")]
        [TestCase("api/namespace/class.html", ExpectedResult = "https://example.com/docs/api/namespace/class.html")]
        [TestCase("index.html", ExpectedResult = "https://example.com/docs/index.html")]
        [TestCase("api/page.html?query=param", ExpectedResult = "https://example.com/docs/api/page.html?query=param")]
        [TestCase("api/page.html#fragment", ExpectedResult = "https://example.com/docs/api/page.html#fragment")]
        [TestCase("api/page.html?query=param#fragment", ExpectedResult = "https://example.com/docs/api/page.html?query=param#fragment")]
        [TestCase("", ExpectedResult = "https://example.com/docs/")]
        public string ActiveScope_ResolveFromDocumentationRoot_ReturnsAbsoluteUrl(string urlString)
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var manager = new AbsoluteUrlContextManager(baseUrl);

            return manager.ActiveScope.ResolveFromDocumentationRoot(urlString);
        }

        [Test]
        public void ActiveScope_ResolveFromDocumentationRoot_WithNullUrlString_ThrowsArgumentNullException()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var manager = new AbsoluteUrlContextManager(baseUrl);

            Assert.Throws<ArgumentNullException>(() => manager.ActiveScope.ResolveFromDocumentationRoot(null!));
        }

        [TestCase("api", "api/index.html", ExpectedResult = "https://example.com/docs/api/index.html")]
        [TestCase("api/namespace", "api/namespace/class.html", ExpectedResult = "https://example.com/docs/api/namespace/class.html")]
        [TestCase("api/namespace", "api/other-namespace/interface.html", ExpectedResult = "https://example.com/docs/api/other-namespace/interface.html")]
        [TestCase("api/namespace", "index.html", ExpectedResult = "https://example.com/docs/index.html")]
        [TestCase("api", "api/page.html?query=param#fragment", ExpectedResult = "https://example.com/docs/api/page.html?query=param#fragment")]
        public string Scope_ResolveFromDocumentationRoot_ReturnsAbsoluteUrlIndependentOfCurrentDirectory(string currentDir, string urlString)
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var manager = new AbsoluteUrlContextManager(baseUrl);

            using var scope = manager.BeginScope(currentDir, null);
            return scope.ResolveFromDocumentationRoot(urlString);
        }

        [Test]
        public void Scope_ResolveFromDocumentationRoot_WithNullUrlString_ThrowsArgumentNullException()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var manager = new AbsoluteUrlContextManager(baseUrl);

            using var scope = manager.BeginScope("api/namespace", null);
            Assert.Throws<ArgumentNullException>(() => scope.ResolveFromDocumentationRoot(null!));
        }

        [Test]
        public void NestedScopes_WhenDisposed_CorrectlyRestorePreviousScopes()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var manager = new AbsoluteUrlContextManager(baseUrl);
            var originalScope = manager.ActiveScope;

            using (var outerScope = manager.BeginScope("api/namespace", null))
            {
                Assert.That(manager.ActiveScope, Is.SameAs(outerScope));
                Assert.That(manager.ActiveScope.Directory, Is.EqualTo("api/namespace"));

                using (var innerScope = manager.BeginScope("api/namespace/class", null))
                {
                    Assert.That(manager.ActiveScope, Is.SameAs(innerScope));
                    Assert.That(manager.ActiveScope.Directory, Is.EqualTo("api/namespace/class"));
                }

                Assert.That(manager.ActiveScope, Is.SameAs(outerScope));
                Assert.That(manager.ActiveScope.Directory, Is.EqualTo("api/namespace"));
            }

            Assert.That(manager.ActiveScope, Is.SameAs(originalScope));
            Assert.That(manager.ActiveScope.Directory, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task AsyncOperations_MaintainCorrectContext()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var manager = new AbsoluteUrlContextManager(baseUrl);
            var targetUrlString = "~/page.html";

            using var scope = manager.BeginScope("api/namespace", null);
            var success = scope.TryResolveUrl(targetUrlString, out var initialResult);
            Assert.That(success, Is.True);

            var asyncResult = await Task.Run(() =>
            {
                return manager.ActiveScope.TryResolveUrl(targetUrlString, out var result) ? result : null;
            });

            Assert.That(asyncResult, Is.EqualTo("https://example.com/docs/page.html"));
            Assert.That(asyncResult, Is.EqualTo(initialResult));
        }

        [Test]
        public async Task AsyncOperations_Parallel_MaintainIsolatedContexts()
        {
            var path1 = "api/namespace";
            var path2 = "api/namespace/class";
            var baseUrl = new Uri("https://example.com/docs/");
            var manager = new AbsoluteUrlContextManager(baseUrl);

            var task1 = Task.Run(async () =>
            {
                using var scope = manager.BeginScope(path1, null);
                await Task.Delay(50);
                return scope.Directory;
            });

            var task2 = Task.Run(async () =>
            {
                using var scope = manager.BeginScope(path2, null);
                await Task.Delay(50);
                return scope.Directory;
            });

            var results = await Task.WhenAll(task1, task2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(results[0], Is.EqualTo(path1));
                Assert.That(results[1], Is.EqualTo(path2));
            }
        }
    }
}

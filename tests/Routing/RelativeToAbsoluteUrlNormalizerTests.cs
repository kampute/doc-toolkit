// Copyright (C) 2025 Kampute
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
    public class RelativeToAbsoluteUrlNormalizerTests
    {
        [Test]
        public void Constructor_WithRelativeBaseUrl_ThrowsArgumentException()
        {
            var relativeUrl = new Uri("/docs/", UriKind.Relative);

            Assert.That(() => new RelativeToAbsoluteUrlNormalizer(relativeUrl), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_WithAbsoluteBaseUrl_InitializesCorrectly()
        {
            var baseUrl = new Uri("https://example.com/docs/");

            var normalizer = new RelativeToAbsoluteUrlNormalizer(baseUrl);

            Assert.That(normalizer.BaseUrl, Is.EqualTo(baseUrl));
        }

        [Test]
        public void Constructor_WithRelativeBaseUrlString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(static () => new RelativeToAbsoluteUrlNormalizer("docs/"));
        }

        [Test]
        public void Constructor_WithAbsoluteBaseUrlStringWithoutTrailingSlash_InitializesCorrectly()
        {
            var baseUrlString = "https://example.com/docs";
            var normalizer = new RelativeToAbsoluteUrlNormalizer(baseUrlString);

            Assert.That(normalizer.BaseUrl, Is.EqualTo(new Uri("https://example.com/docs/")));
        }

        [Test]
        public void Constructor_WithAbsoluteBaseUrlStringWithTrailingSlash_InitializesCorrectly()
        {
            var baseUrlString = "https://example.com/docs/";
            var normalizer = new RelativeToAbsoluteUrlNormalizer(baseUrlString);

            Assert.That(normalizer.BaseUrl, Is.EqualTo(new Uri("https://example.com/docs/")));
        }

        [Test]
        public void ActiveScope_WhenNoScopeActive_ReturnsDefaultScope()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var normalizer = new RelativeToAbsoluteUrlNormalizer(baseUrl);

            var scope = normalizer.ActiveScope;

            Assert.That(scope, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(scope.RootUrl, Is.EqualTo(baseUrl));
                Assert.That(scope.Directory, Is.Empty);
            }
        }

        [TestCase("", ExpectedResult = null)]
        [TestCase("#section", ExpectedResult = null)]
        [TestCase("?query=param", ExpectedResult = null)]
        [TestCase("/other/api/index.html", ExpectedResult = null)]
        [TestCase("api/index.html", ExpectedResult = "https://example.com/docs/api/index.html")]
        [TestCase("api/namespace/class.html", ExpectedResult = "https://example.com/docs/api/namespace/class.html")]
        [TestCase("https://other.com/api/index.html", ExpectedResult = null)]
        public string? ActiveScope_TryTransformSiteRelativeUrl_ReturnsExpectedUrl(string uriString)
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var normalizer = new RelativeToAbsoluteUrlNormalizer(baseUrl);

            return normalizer.ActiveScope.TryTransformSiteRelativeUrl(uriString, out var transformedUrl) ? transformedUrl : null;
        }

        [Test]
        public void BeginScope_ReturnsValidScope()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var normalizer = new RelativeToAbsoluteUrlNormalizer(baseUrl);
            var directory = "some/dir";

            var scope = normalizer.BeginScope(directory, null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(scope, Is.Not.Null);
                Assert.That(scope, Is.SameAs(normalizer.ActiveScope));
                Assert.That(scope.Directory, Is.EqualTo(directory));
            }
        }

        [Test]
        public void Scope_TryTransformSiteRelativeUrl_UsesBaseUrl()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var relativeUrlString = "api/index.html";
            var normalizer = new RelativeToAbsoluteUrlNormalizer(baseUrl);
            var expected = new Uri("https://example.com/docs/api/index.html");

            using var scope = normalizer.BeginScope("some/dir", null);
            var success = scope.TryTransformSiteRelativeUrl(relativeUrlString, out var transformedUrl);

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
            var normalizer = new RelativeToAbsoluteUrlNormalizer(baseUrl);
            using var scope = normalizer.BeginScope("api/namespace", null);

            Assert.That(normalizer.ActiveScope, Is.SameAs(scope));
            using (var nestedScope = normalizer.BeginScope("api/namespace/class", null))
            {
                Assert.That(normalizer.ActiveScope, Is.SameAs(nestedScope));
            }
            Assert.That(normalizer.ActiveScope, Is.SameAs(scope));
        }

        [Test]
        public void Scope_Dispose_CanBeCalledMultipleTimes()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var normalizer = new RelativeToAbsoluteUrlNormalizer(baseUrl);
            using var scope1 = normalizer.BeginScope("api", null);
            var scope2 = normalizer.BeginScope("api/namespace", null);

            scope2.Dispose();
            scope2.Dispose();

            Assert.That(normalizer.ActiveScope, Is.SameAs(scope1));
        }

        [Test]
        public void NestedScopes_WhenDisposed_CorrectlyRestorePreviousScopes()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var normalizer = new RelativeToAbsoluteUrlNormalizer(baseUrl);
            var originalScope = normalizer.ActiveScope;

            using (var outerScope = normalizer.BeginScope("api/namespace", null))
            {
                Assert.That(normalizer.ActiveScope, Is.SameAs(outerScope));
                Assert.That(normalizer.ActiveScope.Directory, Is.EqualTo("api/namespace"));

                using (var innerScope = normalizer.BeginScope("api/namespace/class", null))
                {
                    Assert.That(normalizer.ActiveScope, Is.SameAs(innerScope));
                    Assert.That(normalizer.ActiveScope.Directory, Is.EqualTo("api/namespace/class"));
                }

                Assert.That(normalizer.ActiveScope, Is.SameAs(outerScope));
                Assert.That(normalizer.ActiveScope.Directory, Is.EqualTo("api/namespace"));
            }

            Assert.That(normalizer.ActiveScope, Is.SameAs(originalScope));
            Assert.That(normalizer.ActiveScope.Directory, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task AsyncOperations_MaintainCorrectContext()
        {
            var baseUrl = new Uri("https://example.com/docs/");
            var normalizer = new RelativeToAbsoluteUrlNormalizer(baseUrl);
            var targetUrlString = "page.html";

            using var scope = normalizer.BeginScope("api/namespace", null);
            var success = scope.TryTransformSiteRelativeUrl(targetUrlString, out var initialResult);
            Assert.That(success, Is.True);

            var asyncResult = await Task.Run(() =>
            {
                return normalizer.ActiveScope.TryTransformSiteRelativeUrl(targetUrlString, out var result) ? result : null;
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
            var normalizer = new RelativeToAbsoluteUrlNormalizer(baseUrl);

            var task1 = Task.Run(async () =>
            {
                using var scope = normalizer.BeginScope(path1, null);
                await Task.Delay(50);
                return scope.Directory;
            });

            var task2 = Task.Run(async () =>
            {
                using var scope = normalizer.BeginScope(path2, null);
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
// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Routing
{
    using Kampute.DocToolkit.Routing;
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System.Threading.Tasks;

    [TestFixture]
    public class ContextAwareUrlNormalizerTests
    {
        [Test]
        public void ActiveScope_WhenNoScopeActive_ReturnsRootScope()
        {
            var normalizer = new ContextAwareUrlNormalizer();

            Assert.That(normalizer.ActiveScope, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(normalizer.ActiveScope.Directory, Is.Empty);
                Assert.That(normalizer.ActiveScope.RootUrl, Is.EqualTo(UriHelper.EmptyUri));
            }
        }

        [Test]
        public void BeginScope_ReturnsValidScope()
        {
            var normalizer = new ContextAwareUrlNormalizer();
            using var scope = normalizer.BeginScope("api/namespace", null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(scope, Is.Not.Null);
                Assert.That(normalizer.ActiveScope, Is.SameAs(scope));
                Assert.That(scope.Directory, Is.EqualTo("api/namespace"));
            }
        }

        [TestCase("", ExpectedResult = "")]
        [TestCase("api", ExpectedResult = "../")]
        [TestCase("api/namespace", ExpectedResult = "../../")]
        [TestCase("api/namespace/class", ExpectedResult = "../../../")]
        public string Scope_RootUrl_ReturnsCorrectRelativePathToRoot(string currentDir)
        {
            var normalizer = new ContextAwareUrlNormalizer();
            using var scope = normalizer.BeginScope(currentDir, null);
            return scope.RootUrl.ToString();
        }

        [TestCase("api/namespace", "", ExpectedResult = null)]
        [TestCase("api", "api", ExpectedResult = "../api")]
        [TestCase("api", "page.html", ExpectedResult = "../page.html")]
        [TestCase("api", "/page.html", ExpectedResult = null)]
        [TestCase("api", "api/page.html", ExpectedResult = "page.html")]
        [TestCase("api/namespace", "page.html", ExpectedResult = "../../page.html")]
        [TestCase("api/namespace", "api/page.html", ExpectedResult = "../page.html")]
        [TestCase("api", "api/namespace/page.html", ExpectedResult = "namespace/page.html")]
        [TestCase("api/namespace", "api/namespace/page.html", ExpectedResult = "page.html")]
        [TestCase("api/namespace", "api/other-namespace/interface.html", ExpectedResult = "../other-namespace/interface.html")]
        [TestCase("api/namespace", "api/other-namespace/interface.html?query=param", ExpectedResult = "../other-namespace/interface.html?query=param")]
        [TestCase("api/namespace/classes", "api/page.html#fragment", ExpectedResult = "../../page.html#fragment")]
        [TestCase("api/namespace/classes", "api/other-namespace/page.html?query=param#fragment", ExpectedResult = "../../other-namespace/page.html?query=param#fragment")]
        [TestCase("api/namespace", "api/namespace/class", ExpectedResult = "class")]
        [TestCase("api/namespace", "api/other-namespace/class", ExpectedResult = "../other-namespace/class")]
        [TestCase("api", "https://example.com/page?query=param#fragment", ExpectedResult = null)]
        public string? Scope_TryTransformSiteRelativeUrl_ReturnsExpectedUrl(string currentDir, string urlString)
        {
            var normalizer = new ContextAwareUrlNormalizer();
            using var scope = normalizer.BeginScope(currentDir, null);
            return scope.TryTransformSiteRelativeUrl(urlString, out var transformedUrl) ? transformedUrl : null;
        }

        [Test]
        public void Scope_Dispose_RestoresPreviousContext()
        {
            var normalizer = new ContextAwareUrlNormalizer();
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
            var normalizer = new ContextAwareUrlNormalizer();
            using var scope1 = normalizer.BeginScope("api", null);
            var scope2 = normalizer.BeginScope("api/namespace", null);

            scope2.Dispose();
            scope2.Dispose();

            Assert.That(normalizer.ActiveScope, Is.SameAs(scope1));
        }

        [Test]
        public void NestedScopes_WhenDisposed_CorrectlyRestorePreviousContext()
        {
            var outerPath = "api/namespace";
            var innerPath = "api/namespace/class";
            var targetUrlString = "page.html";

            var normalizer = new ContextAwareUrlNormalizer();
            using (var outerScope = normalizer.BeginScope(outerPath, null))
            {
                var success1 = outerScope.TryTransformSiteRelativeUrl(targetUrlString, out var result1);
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(success1, Is.True);
                    Assert.That(result1, Is.EqualTo("../../page.html"));
                    Assert.That(outerScope.RootUrl.ToString(), Is.EqualTo("../../"));
                }

                using (var innerScope = normalizer.BeginScope(innerPath, null))
                {
                    var success2 = innerScope.TryTransformSiteRelativeUrl(targetUrlString, out var result2);
                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(success2, Is.True);
                        Assert.That(result2, Is.EqualTo("../../../page.html"));
                        Assert.That(innerScope.RootUrl.ToString(), Is.EqualTo("../../../"));
                    }
                }

                var success3 = outerScope.TryTransformSiteRelativeUrl(targetUrlString, out var result3);
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(success3, Is.True);
                    Assert.That(result3, Is.EqualTo("../../page.html"));
                    Assert.That(outerScope.RootUrl.ToString(), Is.EqualTo("../../"));
                }
            }

            using (Assert.EnterMultipleScope())
            {
                var success4 = normalizer.ActiveScope.TryTransformSiteRelativeUrl(targetUrlString, out var result4);
                Assert.That(success4, Is.True);
                Assert.That(result4, Is.EqualTo("page.html"));
                Assert.That(normalizer.ActiveScope.RootUrl.ToString(), Is.Empty);
            }
        }

        [Test]
        public async Task AsyncOperations_MaintainCorrectContext()
        {
            var path = "api/namespace";
            var targetUrlString = "page.html";
            var normalizer = new ContextAwareUrlNormalizer();

            using var scope = normalizer.BeginScope(path, null);
            var success = scope.TryTransformSiteRelativeUrl(targetUrlString, out var initialResult);
            Assert.That(success, Is.True);

            var asyncResult = await Task.Run(() =>
            {
                var success2 = normalizer.ActiveScope.TryTransformSiteRelativeUrl(targetUrlString, out var res);
                return success2 ? res : null;
            });

            Assert.That(asyncResult, Is.EqualTo("../../page.html"));
            Assert.That(asyncResult, Is.EqualTo(initialResult));
        }

        [Test]
        public async Task AsyncOperations_Parallel_MaintainIsolatedContexts()
        {
            var path1 = "api/namespace";
            var path2 = "api/namespace/class";
            var targetUrlString = "page.html";
            var normalizer = new ContextAwareUrlNormalizer();

            var task1 = Task.Run(async () =>
            {
                using var scope = normalizer.BeginScope(path1, null);
                await Task.Delay(50);
                return scope.TryTransformSiteRelativeUrl(targetUrlString, out var result) ? result : null;
            });

            var task2 = Task.Run(async () =>
            {
                using var scope = normalizer.BeginScope(path2, null);
                await Task.Delay(50);
                return scope.TryTransformSiteRelativeUrl(targetUrlString, out var result) ? result : null;
            });

            var results = await Task.WhenAll(task1, task2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(results[0], Is.EqualTo("../../page.html"));
                Assert.That(results[1], Is.EqualTo("../../../page.html"));
            }
        }
    }
}
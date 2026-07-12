// Copyright (C) Kampute
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
    public class RelativeUrlContextManagerTests
    {
        [Test]
        public void ActiveScope_WhenNoScopeActive_ReturnsRootScope()
        {
            var manager = new RelativeUrlContextManager();

            Assert.That(manager.ActiveScope, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(manager.ActiveScope.Directory, Is.Empty);
                Assert.That(manager.ActiveScope.DocumentationRootUrl, Is.EqualTo(UriHelper.EmptyUri));
            }
        }

        [Test]
        public void BeginScope_ReturnsValidScope()
        {
            var manager = new RelativeUrlContextManager();
            using var scope = manager.BeginScope("api/namespace", null);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(scope, Is.Not.Null);
                Assert.That(manager.ActiveScope, Is.SameAs(scope));
                Assert.That(scope.Directory, Is.EqualTo("api/namespace"));
            }
        }

        [TestCase("", ExpectedResult = "")]
        [TestCase("api", ExpectedResult = "../")]
        [TestCase("api/namespace", ExpectedResult = "../../")]
        [TestCase("api/namespace/class", ExpectedResult = "../../../")]
        public string Scope_DocumentationRootUrl_ReturnsCorrectRelativePathToRoot(string currentDir)
        {
            var manager = new RelativeUrlContextManager();
            using var scope = manager.BeginScope(currentDir, null);
            return scope.DocumentationRootUrl.ToString();
        }

        [TestCase("api/namespace", "", ExpectedResult = null)]
        [TestCase("api", "api", ExpectedResult = null)]
        [TestCase("api", "~/page.html", ExpectedResult = "../page.html")]
        [TestCase("api", "/page.html", ExpectedResult = null)]
        [TestCase("api", "~/api/page.html", ExpectedResult = "page.html")]
        [TestCase("api/namespace", "~/api/page.html", ExpectedResult = "../page.html")]
        [TestCase("api", "~/api/namespace/page.html", ExpectedResult = "namespace/page.html")]
        [TestCase("api/namespace", "~/api/namespace/page.html", ExpectedResult = "page.html")]
        [TestCase("api/namespace", "~/api/other-namespace/interface.html", ExpectedResult = "../other-namespace/interface.html")]
        [TestCase("api/namespace", "~/api/other-namespace/interface.html?query=param", ExpectedResult = "../other-namespace/interface.html?query=param")]
        [TestCase("api/namespace/classes", "~/api/page.html#fragment", ExpectedResult = "../../page.html#fragment")]
        [TestCase("api/namespace/classes", "~/api/other-namespace/page.html?query=param#fragment", ExpectedResult = "../../other-namespace/page.html?query=param#fragment")]
        [TestCase("api/namespace", "~/api/namespace/class", ExpectedResult = "class")]
        [TestCase("api/namespace", "~/api/other-namespace/class", ExpectedResult = "../other-namespace/class")]
        [TestCase("api/namespace", "~/api/../docs", ExpectedResult = "../../docs")]
        [TestCase("api/namespace", "~/../docs", ExpectedResult = null)]
        [TestCase("api", "https://example.com/page?query=param#fragment", ExpectedResult = null)]
        public string? Scope_TryResolveUrl_ReturnsExpectedUrl(string currentDir, string urlString)
        {
            var manager = new RelativeUrlContextManager();
            using var scope = manager.BeginScope(currentDir, null);
            return scope.TryResolveUrl(urlString, out var transformedUrl) ? transformedUrl : null;
        }

        [Test]
        public void Scope_Dispose_RestoresPreviousContext()
        {
            var manager = new RelativeUrlContextManager();
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
            var manager = new RelativeUrlContextManager();
            using var scope1 = manager.BeginScope("api", null);
            var scope2 = manager.BeginScope("api/namespace", null);

            scope2.Dispose();
            scope2.Dispose();

            Assert.That(manager.ActiveScope, Is.SameAs(scope1));
        }

        [Test]
        public void NestedScopes_WhenDisposed_CorrectlyRestorePreviousContext()
        {
            var outerPath = "api/namespace";
            var innerPath = "api/namespace/class";
            var targetUrlString = "~/page.html";

            var manager = new RelativeUrlContextManager();
            using (var outerScope = manager.BeginScope(outerPath, null))
            {
                var success1 = outerScope.TryResolveUrl(targetUrlString, out var result1);
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(success1, Is.True);
                    Assert.That(result1, Is.EqualTo("../../page.html"));
                    Assert.That(outerScope.DocumentationRootUrl.ToString(), Is.EqualTo("../../"));
                }

                using (var innerScope = manager.BeginScope(innerPath, null))
                {
                    var success2 = innerScope.TryResolveUrl(targetUrlString, out var result2);
                    using (Assert.EnterMultipleScope())
                    {
                        Assert.That(success2, Is.True);
                        Assert.That(result2, Is.EqualTo("../../../page.html"));
                        Assert.That(innerScope.DocumentationRootUrl.ToString(), Is.EqualTo("../../../"));
                    }
                }

                var success3 = outerScope.TryResolveUrl(targetUrlString, out var result3);
                using (Assert.EnterMultipleScope())
                {
                    Assert.That(success3, Is.True);
                    Assert.That(result3, Is.EqualTo("../../page.html"));
                    Assert.That(outerScope.DocumentationRootUrl.ToString(), Is.EqualTo("../../"));
                }
            }

            using (Assert.EnterMultipleScope())
            {
                var success4 = manager.ActiveScope.TryResolveUrl(targetUrlString, out var result4);
                Assert.That(success4, Is.True);
                Assert.That(result4, Is.EqualTo("page.html"));
                Assert.That(manager.ActiveScope.DocumentationRootUrl.ToString(), Is.Empty);
            }
        }

        [Test]
        public async Task AsyncOperations_MaintainCorrectContext()
        {
            var path = "api/namespace";
            var targetUrlString = "~/page.html";
            var manager = new RelativeUrlContextManager();

            using var scope = manager.BeginScope(path, null);
            var success = scope.TryResolveUrl(targetUrlString, out var initialResult);
            Assert.That(success, Is.True);

            var asyncResult = await Task.Run(() =>
            {
                var success2 = manager.ActiveScope.TryResolveUrl(targetUrlString, out var res);
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
            var targetUrlString = "~/page.html";
            var manager = new RelativeUrlContextManager();

            var task1 = Task.Run(async () =>
            {
                using var scope = manager.BeginScope(path1, null);
                await Task.Delay(50);
                return scope.TryResolveUrl(targetUrlString, out var result) ? result : null;
            });

            var task2 = Task.Run(async () =>
            {
                using var scope = manager.BeginScope(path2, null);
                await Task.Delay(50);
                return scope.TryResolveUrl(targetUrlString, out var result) ? result : null;
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

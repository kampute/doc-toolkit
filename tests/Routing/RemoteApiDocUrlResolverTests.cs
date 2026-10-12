// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Routing
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Routing;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

    [TestFixture]
    public class RemoteApiDocUrlResolverTests
    {
        /// <summary>
        /// A resolver that returns preset URLs and records the requests that reach its resolution methods.
        /// </summary>
        private sealed class TestResolver(Uri siteUrl, Uri? resolvedUrl) : RemoteApiDocUrlResolver(siteUrl)
        {
            public List<string> Requests { get; } = [];

            protected override Uri? ResolveMemberUrl(IMember member)
            {
                Requests.Add(member.CodeReference);
                return resolvedUrl;
            }

            protected override Uri? ResolveNamespaceUrl(string ns)
            {
                Requests.Add(ns);
                return resolvedUrl;
            }
        }

        private static readonly Uri SiteUrl = new("https://docs.example.com/api/");

        [Test]
        public void Constructor_WithNullUrl_ThrowsArgumentNullException()
        {
            Assert.That(static () => new TestResolver(null!, null), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("url"));
        }

        [Test]
        public void Constructor_WithRelativeUrl_ThrowsArgumentException()
        {
            Assert.That(static () => new TestResolver(new Uri("api/", UriKind.Relative), null), Throws.ArgumentException.With.Property("ParamName").EqualTo("url"));
        }

        [TestCase("System", ExpectedResult = true)]
        [TestCase("System.Collections", ExpectedResult = true)]
        [TestCase("SystemX", ExpectedResult = false)]
        [TestCase(" ", ExpectedResult = false)]
        public bool SupportsNamespace_FollowsNamespacePatterns(string ns)
        {
            var resolver = new TestResolver(SiteUrl, null);
            resolver.NamespacePatterns.Add("System.*");

            return resolver.SupportsNamespace(ns);
        }

        [Test]
        public void TryGetNamespaceUrl_WithUnsupportedNamespace_ReturnsFalseWithoutResolving()
        {
            var resolver = new TestResolver(SiteUrl, new Uri("https://other.example.com/"));
            resolver.NamespacePatterns.Add("System.*");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(resolver.TryGetNamespaceUrl("Acme", out var url), Is.False);
                Assert.That(url, Is.Null);
                Assert.That(resolver.Requests, Is.Empty);
            }
        }

        [Test]
        public void TryGetNamespaceUrl_WithRelativeResolvedUrl_ReturnsUrlRelativeToSite()
        {
            var resolver = new TestResolver(SiteUrl, new Uri("system", UriKind.Relative));
            resolver.NamespacePatterns.Add("System.*");

            Assert.That(resolver.TryGetNamespaceUrl("System", out var url) ? url : null, Is.EqualTo(new Uri("https://docs.example.com/api/system")));
        }

        [Test]
        public void TryGetNamespaceUrl_WithAbsoluteResolvedUrl_ReturnsItUnchanged()
        {
            var resolver = new TestResolver(SiteUrl, new Uri("https://mirror.example.com/system"));
            resolver.NamespacePatterns.Add("System.*");

            Assert.That(resolver.TryGetNamespaceUrl("System", out var url) ? url : null, Is.EqualTo(new Uri("https://mirror.example.com/system")));
        }

        [Test]
        public void TryGetNamespaceUrl_WhenResolutionFails_ReturnsFalse()
        {
            var resolver = new TestResolver(SiteUrl, null);
            resolver.NamespacePatterns.Add("System.*");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(resolver.TryGetNamespaceUrl("System", out var url), Is.False);
                Assert.That(url, Is.Null);
            }
        }

        [Test]
        public void TryGetMemberUrl_UsesNamespaceOfMember()
        {
            var resolver = new TestResolver(SiteUrl, new Uri("member", UriKind.Relative));
            resolver.NamespacePatterns.Add("System.*");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(resolver.TryGetMemberUrl(typeof(DateTime).GetMetadata(), out var supported) ? supported : null, Is.EqualTo(new Uri("https://docs.example.com/api/member")));
                Assert.That(resolver.TryGetMemberUrl(typeof(Acme.SampleMethods).GetMetadata(), out var unsupported), Is.False);
                Assert.That(unsupported, Is.Null);
                Assert.That(resolver.TryGetMemberUrl(null!, out _), Is.False);
            }
        }
    }
}

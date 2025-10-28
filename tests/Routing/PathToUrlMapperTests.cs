// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Routing
{
    using Kampute.DocToolkit.Routing;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class PathToUrlMapperTests
    {
        [Test]
        public void Constructor_CreatesEmptyMapper()
        {
            var mapper = new PathToUrlMapper();

            Assert.That(mapper, Is.Empty);
        }

        [Test]
        public void Add_WithValidParameters_AddsMapping()
        {
            var mapper = new PathToUrlMapper();

            var result = mapper.Add("doc/page.html", new Uri("https://example.com/docs/page"));

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(mapper, Has.Count.EqualTo(1));
                Assert.That(mapper.Contains("doc/page.html"), Is.True);
            }
        }

        [Test]
        public void Add_WithDuplicatePath_ReturnsFalse()
        {
            var mapper = new PathToUrlMapper();
            var path = "doc/page.html";
            var url1 = new Uri("https://example.com/docs/page1");
            var url2 = new Uri("https://example.com/docs/page2");

            mapper.Add(path, url1);
            var result = mapper.Add(path, url2);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(mapper, Has.Count.EqualTo(1));
            }
        }

        [Test]
        public void Add_WithNullPath_ThrowsArgumentException()
        {
            var mapper = new PathToUrlMapper();

            Assert.Throws<ArgumentException>(() => mapper.Add(default!, new Uri("https://example.com/docs/page")));
        }

        [Test]
        public void Add_WithEmptyPath_ThrowsArgumentException()
        {
            var mapper = new PathToUrlMapper();

            Assert.Throws<ArgumentException>(() => mapper.Add(string.Empty, new Uri("https://example.com/docs/page")));
        }

        [Test]
        public void Add_WithNullUrl_ThrowsArgumentNullException()
        {
            var mapper = new PathToUrlMapper();

            Assert.Throws<ArgumentNullException>(() => mapper.Add("doc/page.html", default!));
        }

        [Test]
        public void Remove_ExistingPath_RemovesMapping()
        {
            var mapper = new PathToUrlMapper
            {
                { "doc/page.html", new Uri("https://example.com/docs/page") }
            };

            var result = mapper.Remove("doc/page.html");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(mapper, Is.Empty);
                Assert.That(mapper.Contains("doc/page.html"), Is.False);
            }
        }

        [Test]
        public void Remove_NonExistingPath_ReturnsFalse()
        {
            var mapper = new PathToUrlMapper();

            Assert.That(mapper.Remove("doc/page.html"), Is.False);
        }

        [Test]
        public void Remove_WithNullPath_ThrowsArgumentException()
        {
            var mapper = new PathToUrlMapper();
            var path = default(string)!;

            Assert.Throws<ArgumentException>(() => mapper.Remove(path));
        }

        [Test]
        public void Remove_WithEmptyPath_ThrowsArgumentException()
        {
            var mapper = new PathToUrlMapper();

            Assert.Throws<ArgumentException>(() => mapper.Remove(string.Empty));
        }

        [Test]
        public void Contains_ExistingPath_ReturnsTrue()
        {
            var mapper = new PathToUrlMapper
            {
                { "doc/page.html", new Uri("https://example.com/docs/page") }
            };

            Assert.That(mapper.Contains("doc/page.html"), Is.True);
        }

        [Test]
        public void Contains_NonExistingPath_ReturnsFalse()
        {
            var mapper = new PathToUrlMapper();

            Assert.That(mapper.Contains("doc/page.html"), Is.False);
        }

        [Test]
        public void Contains_WithNullPath_ThrowsArgumentException()
        {
            var mapper = new PathToUrlMapper();

            Assert.Throws<ArgumentException>(() => mapper.Contains(default!));
        }

        [Test]
        public void Contains_WithEmptyPath_ThrowsArgumentException()
        {
            var mapper = new PathToUrlMapper();

            Assert.Throws<ArgumentException>(() => mapper.Contains(string.Empty));
        }

        [Test]
        public void TryTransformUrl_ExactMatch_ReturnsTrue()
        {
            var mapper = new PathToUrlMapper
            {
                { "doc/page.html", new Uri("https://example.com/docs/page") }
            };

            var result = mapper.TryTransformUrl("doc/page.html", out var replacementUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(replacementUrl!.ToString(), Is.EqualTo("https://example.com/docs/page"));
            }
        }

        [Test]
        public void TryTransformUrl_PathWithQueryString_ReturnsComposedUrl()
        {
            var mapper = new PathToUrlMapper
            {
                { "doc/page.html", new Uri("https://example.com/docs/page") }
            };

            var result = mapper.TryTransformUrl("doc/page.html?query=value", out var replacementUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(replacementUrl!.ToString(), Is.EqualTo("https://example.com/docs/page?query=value"));
            }
        }

        [Test]
        public void TryTransformUrl_PathWithFragment_ReturnsComposedUrl()
        {
            var mapper = new PathToUrlMapper
            {
                { "doc/page.html", new Uri("https://example.com/docs/page") }
            };

            var result = mapper.TryTransformUrl("doc/page.html#section", out var replacementUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(replacementUrl!.ToString(), Is.EqualTo("https://example.com/docs/page#section"));
            }
        }

        [Test]
        public void TryTransformUrl_PathWithQueryAndFragment_ReturnsComposedUrl()
        {
            var mapper = new PathToUrlMapper
            {
                { "doc/page.html", new Uri("https://example.com/docs/page") }
            };

            var result = mapper.TryTransformUrl("doc/page.html?query=value#section", out var replacementUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(replacementUrl!.ToString(), Is.EqualTo("https://example.com/docs/page?query=value#section"));
            }
        }

        [Test]
        public void TryTransformUrl_PathWithQueryAndFragment_NoExtensionInMappings_ReturnsComposedUrl()
        {
            var mapper = new PathToUrlMapper
            {
                { "example.doc/page", new Uri("https://example.com/docs/page") }
            };

            var result = mapper.TryTransformUrl("example.doc/page.html?query=value#section", out var replacementUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(replacementUrl!.ToString(), Is.EqualTo("https://example.com/docs/page?query=value#section"));
            }
        }

        [Test]
        public void TryTransformUrl_PathWithoutQueryAndFragment_NoExtensionInMappings_ReturnsComposedUrl()
        {
            var mapper = new PathToUrlMapper
            {
                { "example.doc/page", new Uri("https://example.com/docs/page") }
            };

            var result = mapper.TryTransformUrl("example.doc/page.html", out var replacementUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(replacementUrl!.ToString(), Is.EqualTo("https://example.com/docs/page"));
            }
        }

        [Test]
        public void TryTransformUrl_PathWithMultipleExtensions_NoExtensionInMappings_MatchesLastExtensionOnly()
        {
            var mapper = new PathToUrlMapper
            {
                { "doc/page.min", new Uri("https://example.com/docs/page-min") }
            };

            var result = mapper.TryTransformUrl("doc/page.min.js?v=1.0", out var replacementUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(replacementUrl!.ToString(), Is.EqualTo("https://example.com/docs/page-min?v=1.0"));
            }
        }

        [Test]
        public void TryTransformUrl_PathWithUrlEncodedCharacters_AreDecodedCorrectly()
        {
            var mapper = new PathToUrlMapper
            {
                { "doc/special page", new Uri("https://example.com/docs/special-page") }
            };

            var result = mapper.TryTransformUrl("doc/special%20page.html", out var replacementUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(replacementUrl!.ToString(), Is.EqualTo("https://example.com/docs/special-page"));
            }
        }

        [Test]
        public void TryTransformUrl_NoMatch_ReturnsFalse()
        {
            var mapper = new PathToUrlMapper
            {
                { "doc/page.html", new Uri("https://example.com/docs/page") }
            };

            var result = mapper.TryTransformUrl("doc/other-page.html", out var replacementUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(replacementUrl, Is.Null);
            }
        }

        [Test]
        public void TryTransformUrl_NullUrlString_ReturnsFalse()
        {
            var mapper = new PathToUrlMapper();

            var result = mapper.TryTransformUrl(default!, out var replacementUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(replacementUrl, Is.Null);
            }
        }

        [Test]
        public void TryTransformUrl_EmptyUrlString_ReturnsFalse()
        {
            var mapper = new PathToUrlMapper();

            var result = mapper.TryTransformUrl(string.Empty, out var replacementUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(replacementUrl, Is.Null);
            }
        }

        [Test]
        public void TryTransformUrl_EmptyMapper_ReturnsFalse()
        {
            var mapper = new PathToUrlMapper();

            var result = mapper.TryTransformUrl("doc/page", out var replacementUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.False);
                Assert.That(replacementUrl, Is.Null);
            }
        }

        [Test]
        public void Clear_RemovesAllMappings()
        {
            var mapper = new PathToUrlMapper
            {
                { "doc/page1.html", new Uri("https://example.com/docs/page1") },
                { "doc/page2.html", new Uri("https://example.com/docs/page2") }
            };

            mapper.Clear();

            Assert.That(mapper, Is.Empty);
        }

        [Test]
        public void Count_ReflectsNumberOfMappings()
        {
            var mapper = new PathToUrlMapper();

            Assert.That(mapper, Is.Empty);

            mapper.Add("doc/page1.html", new Uri("https://example.com/docs/page1"));
            Assert.That(mapper, Has.Count.EqualTo(1));

            mapper.Add("doc/page2.html", new Uri("https://example.com/docs/page2"));
            Assert.That(mapper, Has.Count.EqualTo(2));

            mapper.Remove("doc/page1.html");
            Assert.That(mapper, Has.Count.EqualTo(1));

            mapper.Clear();
            Assert.That(mapper, Is.Empty);
        }

        [Test]
        public void GetEnumerator_ReturnsAllMappings()
        {
            var mapper = new PathToUrlMapper();
            var path1 = "doc/page1.html";
            var url1 = new Uri("https://example.com/docs/page1");
            var path2 = "doc/page2.html";
            var url2 = new Uri("https://example.com/docs/page2");
            mapper.Add(path1, url1);
            mapper.Add(path2, url2);

            var mappings = mapper.ToList();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(mappings, Has.Count.EqualTo(2));
                Assert.That(mappings, Has.Exactly(1).Matches<KeyValuePair<string, Uri>>(m => m.Key == path1 && m.Value == url1));
                Assert.That(mappings, Has.Exactly(1).Matches<KeyValuePair<string, Uri>>(m => m.Key == path2 && m.Value == url2));
            }
        }

        [Test]
        public void CaseInsensitiveMatching_WorksAsExpected()
        {
            var mapper = new PathToUrlMapper();
            var path = "DOC/PAGE.HTML";
            var url = new Uri("https://example.com/docs/page");
            mapper.Add(path, url);

            var result = mapper.TryTransformUrl("doc/page.html", out var replacementUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.True);
                Assert.That(replacementUrl, Is.EqualTo(url));
            }
        }
    }
}
// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Support
{
    using Kampute.DocToolkit.Support;
    using NUnit.Framework;
    using System;
    using System.Globalization;
    using System.Text;

    [TestFixture]
    public class StringBuilderPoolTests
    {
        [Test]
        public void GetBuilder_ReturnsReusableStringBuilder_AndRecycles()
        {
            var pool = new StringBuilderPool(2);

            var rsb = pool.GetBuilder();
            Assert.That(rsb.Builder, Is.Not.Null);

            rsb.Builder.Append("test");
            Assert.That(rsb.ToString(), Is.EqualTo("test"));

            rsb.Dispose();
            Assert.That(pool.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetWriter_ReturnsStringWriter_AndRecycles()
        {
            var pool = new StringBuilderPool(2);

            var writer = pool.GetWriter();
            Assert.That(writer, Is.Not.Null);

            writer.Write("test");
            Assert.That(writer.ToString(), Is.EqualTo("test"));

            writer.Dispose();
            Assert.That(pool.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetWriter_WithFormatProvider_ReturnsStringWriter_AndRecycles()
        {
            var pool = new StringBuilderPool(2);

            var formatProvider = CultureInfo.InvariantCulture;
            var writer = pool.GetWriter(formatProvider);
            Assert.That(writer, Is.Not.Null);

            writer.Write("test");
            Assert.That(writer.ToString(), Is.EqualTo("test"));

            writer.Dispose();
            Assert.That(pool.Count, Is.EqualTo(1));
        }

        [Test]
        public void Pool_RespectsCapacity()
        {
            var pool = new StringBuilderPool(1);

            var rsb1 = pool.GetBuilder();
            rsb1.Dispose();

            var rsb2 = pool.GetBuilder();
            rsb2.Dispose();

            Assert.That(pool.Count, Is.EqualTo(1));
        }

        [Test]
        public void Clear_EmptiesPool()
        {
            var pool = new StringBuilderPool(2);

            var rsb1 = pool.GetBuilder();
            var rsb2 = pool.GetBuilder();
            rsb1.Dispose();
            rsb2.Dispose();
            Assert.That(pool.Count, Is.EqualTo(2));

            pool.Clear();
            Assert.That(pool.Count, Is.Zero);
        }

        [Test]
        public void ReusableStringBuilder_ThrowsAfterDispose()
        {
            var pool = new StringBuilderPool();
            var rsb = pool.GetBuilder();
            rsb.Dispose();

            Assert.That(() => { var _ = rsb.Builder; }, Throws.TypeOf<ObjectDisposedException>());
            Assert.That(rsb.ToString, Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void ImplicitConversion_ReturnsBuilder()
        {
            var pool = new StringBuilderPool();
            using var rsb = pool.GetBuilder();

            StringBuilder sb = rsb;
            Assert.That(sb, Is.Not.Null);

            sb.Append("test");

            Assert.That(rsb.ToString(), Is.EqualTo("test"));
        }
    }
}


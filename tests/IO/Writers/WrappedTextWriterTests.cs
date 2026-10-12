// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.IO.Writers
{
    using Kampute.DocToolkit.IO.Writers;
    using NUnit.Framework;
    using System.IO;

    [TestFixture]
    public class WrappedTextWriterTests
    {
        private sealed class PassThroughWriter(TextWriter writer, bool leaveOpen = false) : WrappedTextWriter(writer, leaveOpen)
        {
        }

        [Test]
        public void Constructor_WithNullWriter_ThrowsArgumentNullException()
        {
            Assert.That(() => new PassThroughWriter(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("writer"));
        }

        [Test]
        public void WriteLine_UsesLineTerminatorOfUnderlyingWriter()
        {
            using var stringWriter = new StringWriter { NewLine = "#" };
            using var writer = new PassThroughWriter(stringWriter, leaveOpen: true);

            writer.WriteLine();
            writer.WriteLine('c');
            writer.WriteLine("line");

            Assert.That(stringWriter.ToString(), Is.EqualTo("#c#line#"));
        }

        [Test]
        public void NewLine_WhenSet_ChangesLineTerminatorOfUnderlyingWriterAndWrapper()
        {
            using var stringWriter = new StringWriter();
            using var writer = new PassThroughWriter(stringWriter, leaveOpen: true);

            writer.NewLine = "#";
            writer.WriteLine("line");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(stringWriter.NewLine, Is.EqualTo("#"));
                Assert.That(stringWriter.ToString(), Is.EqualTo("line#"));
            }
        }
    }
}

// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Support
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Provides a pool of <see cref="StringBuilder"/> instances to minimize memory allocations.
    /// </summary>
    public class StringBuilderPool
    {
        /// <summary>
        /// The shared instance of the <see cref="StringBuilderPool"/> for global use.
        /// </summary>
        public static readonly StringBuilderPool Shared = new();

        private readonly ConcurrentBag<StringBuilder> pool = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="StringBuilderPool"/> class with a default capacity.
        /// </summary>
        public StringBuilderPool()
            : this(Environment.ProcessorCount * 2)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringBuilderPool"/> class with a specified capacity.
        /// </summary>
        /// <param name="capacity">The maximum number of <see cref="StringBuilder"/> instances in the pool.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is less than one.</exception>
        public StringBuilderPool(int capacity)
        {
            if (capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(capacity), "The size of the pool must be greater than zero.");

            Capacity = capacity;
        }

        /// <summary>
        /// Gets the maximum number of <see cref="StringBuilder"/> instances in the pool.
        /// </summary>
        /// <value>
        /// The maximum number of <see cref="StringBuilder"/> instances in the pool.
        /// </value>
        public int Capacity { get; }

        /// <summary>
        /// Gets the current number of <see cref="StringBuilder"/> instances in the pool.
        /// </summary>
        /// <value>
        /// The current number of <see cref="StringBuilder"/> instances in the pool.
        /// </value>
        public int Count => pool.Count;

        /// <summary>
        /// Acquires a <see cref="ReusableStringBuilder"/> from the pool.
        /// </summary>
        /// <returns>A <see cref="ReusableStringBuilder"/> instance wrapping the <see cref="StringBuilder"/>.</returns>
        public ReusableStringBuilder GetBuilder()
        {
            if (!pool.TryTake(out var stringBuilder))
                stringBuilder = new StringBuilder();

            return new ReusableStringBuilder(this, stringBuilder);
        }

        /// <summary>
        /// Acquires a <see cref="StringWriter"/> that uses a pooled <see cref="StringBuilder"/>.
        /// </summary>
        /// <returns>A <see cref="StringWriter"/> instance.</returns>
        public StringWriter GetWriter()
        {
            if (!pool.TryTake(out var stringBuilder))
                stringBuilder = new StringBuilder();

            return new ReusableStringWriter(this, stringBuilder);
        }

        /// <summary>
        /// Acquires a <see cref="StringWriter"/> that uses a pooled <see cref="StringBuilder"/> with a specified format provider.
        /// </summary>
        /// <param name="formatProvider">The format provider to use.</param>
        /// <returns>A <see cref="StringWriter"/> instance.</returns>
        public StringWriter GetWriter(IFormatProvider formatProvider)
        {
            if (!pool.TryTake(out var stringBuilder))
                stringBuilder = new StringBuilder();

            return new ReusableStringWriter(this, stringBuilder, formatProvider);
        }

        /// <summary>
        /// Returns a <see cref="StringBuilder"/> to the pool.
        /// </summary>
        /// <param name="stringBuilder">The <see cref="StringBuilder"/> to return.</param>
        protected void Recycle(StringBuilder stringBuilder)
        {
            if (Count < Capacity)
            {
                stringBuilder.Clear();
                pool.Add(stringBuilder);
            }
        }

        /// <summary>
        /// Clears all <see cref="StringBuilder"/> instances from the pool.
        /// </summary>
        public void Clear()
        {
            while (pool.TryTake(out var _)) { }
        }

        /// <summary>
        /// Represents a <see cref="StringBuilder"/> instance that can be returned to the pool.
        /// </summary>
        public sealed class ReusableStringBuilder : IDisposable
        {
            private readonly StringBuilderPool pool;
            private StringBuilder? builder;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReusableStringBuilder"/> class.
            /// </summary>
            /// <param name="pool">The <see cref="StringBuilderPool"/> instance to which this builder belongs.</param>
            /// <param name="builder">The <see cref="StringBuilder"/> instance to wrap.</param>
            internal ReusableStringBuilder(StringBuilderPool pool, StringBuilder builder)
            {
                this.pool = pool;
                this.builder = builder;
            }

            /// <summary>
            /// Gets the <see cref="StringBuilder"/> instance.
            /// </summary>
            /// <value>
            /// The <see cref="StringBuilder"/> instance.
            /// </value>
            /// <exception cref="ObjectDisposedException">Thrown when the instance has been disposed.</exception>
            public StringBuilder Builder => builder ?? throw new ObjectDisposedException(nameof(ReusableStringBuilder));

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>A string that represents the current object.</returns>
            /// <exception cref="ObjectDisposedException">Thrown when the instance has been disposed.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString() => Builder.ToString();

            /// <summary>
            /// Releases the <see cref="StringBuilder"/> back to the pool.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                var pooledBuilder = Interlocked.Exchange(ref builder, null);
                if (pooledBuilder is not null)
                    pool.Recycle(pooledBuilder);

                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Implicitly converts the <see cref="ReusableStringBuilder"/> to a <see cref="StringBuilder"/>.
            /// </summary>
            /// <param name="rsb">The <see cref="ReusableStringBuilder"/> instance.</param>
            /// <returns>The <see cref="StringBuilder"/> instance.</returns>
            /// <exception cref="ObjectDisposedException">Thrown when the instance has been disposed.</exception>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            [return: NotNullIfNotNull(nameof(rsb))]
            public static implicit operator StringBuilder?(ReusableStringBuilder? rsb) => rsb?.Builder;
        }

        /// <summary>
        /// Represents a <see cref="StringWriter"/> that returns its underlying <see cref="StringBuilder"/> to the pool upon disposal.
        /// </summary>
        private sealed class ReusableStringWriter : StringWriter
        {
            private readonly StringBuilderPool pool;

            /// <summary>
            /// Initializes a new instance of the <see cref="ReusableStringWriter"/> class.
            /// </summary>
            /// <param name="pool">The <see cref="StringBuilderPool"/> instance to which this writer belongs.</param>
            /// <param name="builder">The <see cref="StringBuilder"/> instance to wrap.</param>
            public ReusableStringWriter(StringBuilderPool pool, StringBuilder builder)
                : base(builder)
            {
                this.pool = pool;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ReusableStringWriter"/> class with a specified format provider.
            /// </summary>
            /// <param name="pool">The <see cref="StringBuilderPool"/> instance to which this writer belongs.</param>
            /// <param name="builder">The <see cref="StringBuilder"/> instance to wrap.</param>
            /// <param name="formatProvider">The format provider to use.</param>
            public ReusableStringWriter(StringBuilderPool pool, StringBuilder builder, IFormatProvider formatProvider)
                : base(builder, formatProvider)
            {
                this.pool = pool;
            }

            /// <summary>
            /// Releases the <see cref="StringBuilder"/> back to the pool.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> if called from <see cref="IDisposable.Dispose"/>; <see langword="false"/> if called from a finalizer.</param>
            protected override void Dispose(bool disposing)
            {
                var pooledBuilder = GetStringBuilder();

                base.Dispose(disposing);

                if (disposing)
                    pool.Recycle(pooledBuilder);
            }
        }
    }
}

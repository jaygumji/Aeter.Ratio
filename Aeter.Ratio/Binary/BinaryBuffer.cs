/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.IO;
using System;

namespace Aeter.Ratio.Binary
{
    /// <summary>
    /// Base implementation shared by read and write buffers that exposes pooled memory and helpers
    /// for streaming data in and out of <see cref="IBinaryStream"/> instances.
    /// </summary>
    public class BinaryBuffer : IDisposable
    {
        private readonly BinaryBufferPool _pool;
        private BinaryBufferPool.BinaryMemoryHandle _handle;
        private bool _isDisposed;

        /// <summary>
        /// Gets the rented memory backing the buffer. Use this when low-level APIs need access to the full block.
        /// Prefer <see cref="Span"/> for stack-only operations when performance matters.
        /// </summary>
        private IBinaryStream Stream { get; }

        private readonly Memory<byte> fixedBuffer;
        private long streamOffset;
        private int streamLength;
        private long streamPosition;

        /// <summary>
        /// Gets a span view of <see cref="Buffer"/>. Prefer this over <see cref="Buffer"/> when you do not need
        /// to box the memory as it avoids heap allocations.
        /// </summary>
        public Memory<byte> Memory => fixedBuffer.IsEmpty ? _handle.Memory : fixedBuffer;
        /// <summary>
        /// Gets the span that points at the current memory region.
        /// </summary>
        public Span<byte> Span => Memory.Span;
        /// <summary>
        /// Gets or sets the in-buffer position. Derived types advance this as they consume bytes.
        /// </summary>
        public int Position { get; protected set; }
        /// <summary>
        /// Gets the size of the current memory block.
        /// </summary>
        protected int Size => Memory.Length;

        /// <summary>
        /// Initializes a buffer backed by a user supplied memory block.
        /// Use this when the caller wants full control over the lifecycle of the memory.
        /// </summary>
        public BinaryBuffer(Memory<byte> buffer, IBinaryStream stream, long streamOffset, int streamLength)
        {
            fixedBuffer = buffer;
            Stream = stream;
            this.streamOffset = streamOffset;
            this.streamLength = streamLength;
            streamPosition = streamOffset;
            Position = 0;
            _pool = BinaryMemoryProvider.Single(buffer, out _handle);
        }
        /// <summary>
        /// Initializes a buffer that rents its backing memory from a <see cref="BinaryBufferPool"/>.
        /// Prefer this overload when buffers are frequently created or large to reduce GC pressure.
        /// </summary>
        public BinaryBuffer(BinaryBufferPool pool, BinaryBufferPool.BinaryMemoryHandle handle, IBinaryStream stream, long streamOffset, int streamLength)
        {
            fixedBuffer = Memory<byte>.Empty;
            Stream = stream;
            this.streamOffset = streamOffset;
            this.streamLength = streamLength;
            streamPosition = streamOffset;
            _pool = pool;
            Position = 0;
            _handle = handle;
        }

        /// <summary>
        /// Returns the next slice available in the stream and advances the internal pointer.
        /// </summary>
        protected (long Offset, int Length) GetAndAdvanceStreamPosition(int length)
        {
            var position = streamPosition;
            var streamLengthLeft = streamLength - (streamPosition - streamOffset);
            var actLength = length > streamLengthLeft ? streamLengthLeft : length;

            streamPosition += actLength;
            return (position, (int)actLength);
        }

        /// <summary>
        /// Expands the buffer to accommodate <paramref name="length"/> bytes while preserving data.
        /// </summary>
        protected void Expand(int length, int keepPosition, int keepLength)
        {
            Verify();

            var newSize = Math.Max(length, Size * 2);

            if (_pool is null) throw new NotSupportedException("No pool has been specified, can not expand");

            var newHandle = _pool.Acquire(newSize);
            var target = newHandle.Memory.Span;
            Span.Slice(keepPosition, keepLength).CopyTo(target);

            _handle.Dispose();
            _handle = newHandle;
        }

        /// <summary>
        /// Throws if the buffer has already been disposed.
        /// </summary>
        protected void Verify()
        {
            if (_isDisposed) {
                throw new ObjectDisposedException("BufferPool");
            }
        }

        /// <summary>
        /// Returns the rented memory back to the pool.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            OnDispose();

            _handle.Dispose();
            _handle = BinaryBufferPool.InvalidHandle;
            Position = -1;
        }

        /// <summary>
        /// Allows derived classes to run custom disposal logic.
        /// </summary>
        protected virtual void OnDispose()
        {
        }
    }
}

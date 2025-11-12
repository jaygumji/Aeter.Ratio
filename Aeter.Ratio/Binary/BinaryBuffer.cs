/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.IO;
using System;

namespace Aeter.Ratio.Binary
{
    public class BinaryBuffer : IDisposable
    {
        private readonly BinaryBufferPool _pool;
        private BinaryBufferPool.BinaryMemoryHandle _handle;
        private bool _isDisposed;

        public Memory<byte> Buffer { get; }
        private IBinaryStream Stream { get; }
        private long streamOffset;
        private int streamLength;
        private long streamPosition;

        protected Memory<byte> Memory => _handle.Memory;
        public Span<byte> Span => Memory.Span;
        public int Position { get; protected set; }
        protected int Size => Memory.Length;

        public BinaryBuffer(Memory<byte> buffer, IBinaryStream stream, long streamOffset, int streamLength)
        {
            Buffer = buffer;
            Stream = stream;
            this.streamOffset = streamOffset;
            this.streamLength = streamLength;
            streamPosition = streamOffset;
            Position = 0;
            _pool = BinaryMemoryProvider.Single(buffer, out _handle);
        }
        public BinaryBuffer(BinaryBufferPool pool, BinaryBufferPool.BinaryMemoryHandle handle, IBinaryStream stream, long streamOffset, int streamLength)
        {
            Stream = stream;
            this.streamOffset = streamOffset;
            this.streamLength = streamLength;
            streamPosition = streamOffset;
            _pool = pool;
            Position = 0;
            _handle = handle;
        }

        protected (long Offset, int Length) GetAndAdvanceStreamPosition(int length)
        {
            var position = streamPosition;
            var streamLengthLeft = streamLength - (streamPosition - streamOffset);
            var actLength = length > streamLengthLeft ? streamLengthLeft : length;

            streamPosition += actLength;
            return (position, (int)actLength);
        }

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

        protected void Verify()
        {
            if (_isDisposed) {
                throw new ObjectDisposedException("BufferPool");
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            OnDispose();

            _handle.Dispose();
            _handle = BinaryBufferPool.InvalidHandle;
            Position = -1;
        }

        protected virtual void OnDispose()
        {
        }
    }
}

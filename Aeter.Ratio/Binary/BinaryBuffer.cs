/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary
{
    public class BinaryBuffer : IDisposable
    {
        private readonly BinaryBufferPool _pool;
        private BinaryBufferPool.BinaryMemoryHandle _handle;
        private bool _isDisposed;

        protected Stream Stream { get; }
        protected Memory<byte> Memory => _handle.Memory;
        public Span<byte> Span => Memory.Span;
        public int Position { get; protected set; }
        protected int Size => Memory.Length;

        protected ValueTask<int> ReadAsync(Memory<byte> destination, CancellationToken cancellationToken = default)
            => Stream.ReadAsync(destination, cancellationToken);

        protected int Read(Memory<byte> destination)
            => Stream.Read(destination.Span);

        protected ValueTask WriteAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
            => Stream.WriteAsync(source, cancellationToken);

        protected void Write(ReadOnlyMemory<byte> source)
            => Stream.Write(source.Span);

        public BinaryBuffer(Memory<byte> buffer, Stream stream)
        {
            Stream = stream;
            Position = 0;
            _pool = BinaryMemoryProvider.Single(buffer, out _handle);
        }
        public BinaryBuffer(BinaryBufferPool pool, BinaryBufferPool.BinaryMemoryHandle handle, Stream stream)
        {
            Stream = stream;
            _pool = pool;
            Position = 0;
            _handle = handle;
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

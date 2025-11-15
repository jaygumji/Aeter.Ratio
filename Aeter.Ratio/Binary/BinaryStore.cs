/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.IO;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary
{
    /// <summary>
    /// A synchronized binary store of data
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class BinaryStore :  IDisposable
    {
        private readonly IBinaryWriteStream stream;
        private readonly BinaryBufferPool bufferPool;
        private long _flushOffset;
        private readonly SemaphoreSlim _flushSemaphore = new SemaphoreSlim(1);
        private const int HeaderLength = 5;

        public BinaryStore(string path, BinaryBufferPool bufferPool)
            : this(BinaryStream.ParallellFileStream(path), bufferPool)
        {
        }

        public BinaryStore(IBinaryWriteStream stream, BinaryBufferPool bufferPool)
        {
            this.stream = stream;
            this.bufferPool = bufferPool;

            _flushOffset = stream.Length;
        }

        public long Size => stream.Length;

        public async Task<BinaryWriteBuffer> WriteAsync(long offset, int length, CancellationToken cancellationToken = default)
        {
            using var header = bufferPool.Acquire(HeaderLength);
            header.Memory.Span[0] = 1;
            if (!BitConverter.TryWriteBytes(header.Memory.Span[1..], length + HeaderLength)) {
                throw new ArgumentException("Unexpected error when creating header");
            }

            await stream.WriteAsync(offset, header.Memory, cancellationToken);
            return bufferPool.AcquireWriteBuffer(stream, offset + HeaderLength, length);
        }

        public async Task MarkAsNotUsedAsync(long offset, CancellationToken cancellationToken = default)
        {
            using var header = bufferPool.Acquire(HeaderLength);
            await stream.ReadAsync(offset, header.Memory[..5], cancellationToken);

            var length = BitConverter.ToInt32(header.Memory.Span[1..5]);
            if (!(length > 0)) {
                throw new ArgumentException("Written length must be a positive number");
            }

            header.Memory.Span[0] = 255;
            await stream.WriteAsync(offset, header.Memory[0..1], cancellationToken);
        }

        public async Task MarkAsNotUsedAsync(long offset, int length, CancellationToken cancellationToken = default)
        {
            using var header = bufferPool.Acquire(HeaderLength);
            header.Memory.Span[0] = 255;
            if (!BitConverter.TryWriteBytes(header.Memory.Span[1..], length + HeaderLength)) {
                throw new ArgumentException("Unexpected error when creating header");
            }
            await stream.WriteAsync(offset, header.Memory, cancellationToken);
        }

        private async Task EnsureFlushedAsync(long offset, CancellationToken cancellationToken = default)
        {
            if (offset < _flushOffset) return;

            await _flushSemaphore.WaitAsync(cancellationToken: cancellationToken);
            try {
                if (offset < _flushOffset) return;

                stream.Flush();
                _flushOffset = stream.Length;
            }
            finally {
                _flushSemaphore.Release();
            }
        }

        public async Task ReadAllAsync(Func<BinaryStoreReadAllArgs, Task> callback, object? state = null, CancellationToken cancellationToken = default)
        {
            var buffer = bufferPool.AcquireReadBuffer(stream);

            var offset = 0L;
            while (offset < Size) {
                await EnsureFlushedAsync(offset, cancellationToken);

                var space = await buffer.ReadAsync(5);
                var type = space.Span[0];
                var size = BitConverter.ToInt32(space.Span[1..]);

                var args = new BinaryStoreReadAllArgs(this, offset, type, size, state);
                await callback.Invoke(args);
                offset += size;
            }
        }

        public async Task<BinaryReadBuffer> ReadAsync(long offset, CancellationToken cancellationToken = default)
        {
            await EnsureFlushedAsync(offset, cancellationToken);

            using var header = bufferPool.Acquire(HeaderLength);
            await stream.ReadAsync(offset, header.Memory, cancellationToken);

            var type = header.Memory.Span[0];
            var size = BitConverter.ToInt32(header.Memory.Span[1..]);

            return bufferPool.AcquireReadBuffer(stream, offset + HeaderLength, size - HeaderLength);
        }

        public void Dispose()
        {
            stream.Dispose();
            _flushSemaphore.Dispose();
        }

    }
}

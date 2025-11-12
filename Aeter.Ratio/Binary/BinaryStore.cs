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

        public async Task<BinaryWriteBuffer> GetWriteSpaceAsync(long offset, int length, CancellationToken cancellationToken = default)
        {
            using var header = bufferPool.Acquire(HeaderLength);
            header.Memory.Span[0] = 1;
            if (!BitConverter.TryWriteBytes(header.Memory.Span[1..], length + HeaderLength)) {
                throw new ArgumentException("Unexpected error when creating header");
            }

            await stream.WriteAsync(offset, header.Memory, cancellationToken);
            return bufferPool.AcquireWriteBuffer(stream, offset + HeaderLength, length);
        }

        public async ValueTask<long> WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            var offset = stream.Length;
            await WriteAsync(offset, data, cancellationToken);
            return offset;
        }

        public async ValueTask WriteAsync(long offset, ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            using var header = bufferPool.Acquire(HeaderLength);
            header.Memory.Span[0] = 1;
            if (!BitConverter.TryWriteBytes(header.Memory.Span[1..], data.Length + HeaderLength)) {
                throw new ArgumentException("Unexpected error when creating header");
            }
            await stream.WriteAsync(offset, header.Memory, cancellationToken);
            await stream.WriteAsync(offset + HeaderLength, data, cancellationToken);
            if (_flushOffset > offset)
                _flushOffset = offset - 1;
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

        public async Task<BinaryReadBuffer> GetReadSpaceAsync(long offset, CancellationToken cancellationToken = default)
        {
            await EnsureFlushedAsync(offset, cancellationToken);

            using var header = bufferPool.Acquire(HeaderLength);
            await stream.ReadAsync(offset, header.Memory, cancellationToken);

            var type = header.Memory.Span[0];
            var size = BitConverter.ToInt32(header.Memory.Span[1..]);

            return bufferPool.AcquireReadBuffer(stream, offset + HeaderLength, size - HeaderLength);
        }

        public async Task<Memory<byte>> ReadAsync(long offset, CancellationToken cancellationToken = default)
        {
            await EnsureFlushedAsync(offset, cancellationToken);

            using var header = bufferPool.Acquire(HeaderLength);
            await stream.ReadAsync(offset, header.Memory, cancellationToken);

            var type = header.Memory.Span[0];
            var size = BitConverter.ToInt32(header.Memory.Span[1..]);

            var buffer = new byte[size - HeaderLength];
            await stream.ReadAsync(offset + HeaderLength, buffer, cancellationToken);
            return buffer;
        }

        public void Dispose()
        {
            stream.Dispose();
            _flushSemaphore.Dispose();
        }

    }
}

/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.IO;
using System;
using System.Buffers.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary.EntityStore
{
    /// <summary>
    /// Binary store that stores per-entity metadata in the header of every block.
    /// Use it when you need to persist raw entity payloads together with versioned header information.
    /// </summary>
    public sealed class BinaryEntityStore : IDisposable
    {
        private const byte InUseMarker = 1;
        private const byte NotUsedMarker = 255;
        private const int HeaderPrefixLength = 8;

        private readonly IBinaryWriteStream stream;
        private readonly BinaryBufferPool bufferPool;
        private long _flushOffset;
        private readonly SemaphoreSlim _flushSemaphore = new(1);

        /// <summary>
        /// Creates a store backed by the file at <paramref name="path"/> while reusing buffers from <paramref name="bufferPool"/>.
        /// </summary>
        public BinaryEntityStore(string path, BinaryBufferPool bufferPool)
            : this(BinaryStream.ParallellFileStream(path), bufferPool)
        {
        }

        /// <summary>
        /// Creates a store that writes to the supplied <paramref name="stream"/>.
        /// </summary>
        public BinaryEntityStore(IBinaryWriteStream stream, BinaryBufferPool bufferPool)
        {
            this.stream = stream;
            this.bufferPool = bufferPool;
            _flushOffset = stream.Length;
        }

        /// <summary>
        /// Gets the current size of the store in bytes.
        /// </summary>
        public long Size => stream.Length;

        /// <summary>
        /// Allocates space at <paramref name="offset"/> for an entity that uses <paramref name="length"/> bytes
        /// and sets the metadata using <paramref name="key"/> and <paramref name="version"/>.
        /// Use this overload when you only need to store the default metadata layout.
        /// </summary>
        public Task<BinaryWriteBuffer> WriteAsync(long offset, int length, Guid key, uint version, CancellationToken cancellationToken = default)
            => WriteAsync(offset, length, new BinaryEntityStoreRecordMetadata(key, version), cancellationToken);

        /// <summary>
        /// Allocates space for an entity using a prebuilt metadata description. Use when you need to customize the metadata block.
        /// </summary>
        public async Task<BinaryWriteBuffer> WriteAsync(long offset, int length, BinaryEntityStoreRecordMetadata metadata, CancellationToken cancellationToken = default)
        {
            var metadataLength = metadata.GetSerializedLength();
            if (metadataLength > ushort.MaxValue) {
                throw new ArgumentOutOfRangeException(nameof(metadata), "Metadata is larger than allowed header space.");
            }

            var headerLength = HeaderPrefixLength + metadataLength;
            using var header = bufferPool.Acquire(headerLength);

            header.Memory.Span[0] = InUseMarker;
            BinaryPrimitives.WriteInt32LittleEndian(header.Memory.Span.Slice(1, 4), length + headerLength);
            header.Memory.Span[5] = metadata.MetadataVersion;
            BinaryPrimitives.WriteUInt16LittleEndian(header.Memory.Span.Slice(6, 2), (ushort)metadataLength);
            metadata.WriteTo(header.Memory.Span.Slice(HeaderPrefixLength, metadataLength));

            await stream.WriteAsync(offset, header.Memory[..headerLength], cancellationToken);
            return bufferPool.AcquireWriteBuffer(stream, offset + headerLength, length);
        }

        /// <summary>
        /// Marks the record at <paramref name="offset"/> as unused by reading its header and updating the marker.
        /// Call this when you do not know the length beforehand.
        /// </summary>
        public async Task MarkAsNotUsedAsync(long offset, CancellationToken cancellationToken = default)
        {
            using var header = bufferPool.Acquire(HeaderPrefixLength);
            await stream.ReadAsync(offset, header.Memory[..HeaderPrefixLength], cancellationToken);
            var length = BinaryPrimitives.ReadInt32LittleEndian(header.Memory.Span.Slice(1, 4));
            if (length <= 0) {
                throw new ArgumentException("Written length must be a positive number");
            }

            header.Memory.Span[0] = NotUsedMarker;
            await stream.WriteAsync(offset, header.Memory[..1], cancellationToken);
        }

        /// <summary>
        /// Marks the record at <paramref name="offset"/> with a known <paramref name="length"/> as unused,
        /// using the supplied key/version metadata.
        /// Prefer this overload when you already tracked the record length to avoid a read.
        /// </summary>
        public Task MarkAsNotUsedAsync(long offset, int length, Guid key, uint version, CancellationToken cancellationToken = default)
            => MarkAsNotUsedAsync(offset, length, new BinaryEntityStoreRecordMetadata(key, version), cancellationToken);

        /// <summary>
        /// Marks the record as unused while writing a preconstructed metadata block.
        /// </summary>
        public async Task MarkAsNotUsedAsync(long offset, int length, BinaryEntityStoreRecordMetadata metadata, CancellationToken cancellationToken = default)
        {
            var metadataLength = metadata.GetSerializedLength();
            if (metadataLength > ushort.MaxValue) {
                throw new ArgumentOutOfRangeException(nameof(metadata), "Metadata is larger than allowed header space.");
            }

            var headerLength = HeaderPrefixLength + metadataLength;
            using var header = bufferPool.Acquire(headerLength);

            header.Memory.Span[0] = NotUsedMarker;
            BinaryPrimitives.WriteInt32LittleEndian(header.Memory.Span.Slice(1, 4), length + headerLength);
            header.Memory.Span[5] = metadata.MetadataVersion;
            BinaryPrimitives.WriteUInt16LittleEndian(header.Memory.Span.Slice(6, 2), (ushort)metadataLength);
            metadata.WriteTo(header.Memory.Span.Slice(HeaderPrefixLength, metadataLength));

            await stream.WriteAsync(offset, header.Memory[..headerLength], cancellationToken);
        }

        /// <summary>
        /// Iterates every record sequentially and executes <paramref name="callback"/> for each.
        /// Use this to scan the store without materializing payloads.
        /// </summary>
        public async Task ReadAllAsync(Func<BinaryEntityStoreReadAllArgs, Task> callback, object? state = null, CancellationToken cancellationToken = default)
        {
            using var buffer = bufferPool.AcquireReadBuffer(stream);
            var offset = 0L;
            while (offset < Size) {
                await EnsureFlushedAsync(offset, cancellationToken);

                var headerPrefix = await buffer.ReadAsync(HeaderPrefixLength, cancellationToken);
                var marker = headerPrefix.Span[0];
                var totalLength = BinaryPrimitives.ReadInt32LittleEndian(headerPrefix.Span.Slice(1, 4));
                var metadataVersion = headerPrefix.Span[5];
                var metadataLength = BinaryPrimitives.ReadUInt16LittleEndian(headerPrefix.Span.Slice(6, 2));
                var metadataMemory = metadataLength > 0 ? await buffer.ReadAsync(metadataLength, cancellationToken) : ReadOnlyMemory<byte>.Empty;
                var entryHeader = CreateEntryHeader(marker, totalLength, metadataVersion, metadataMemory.Span);
                var args = new BinaryEntityStoreReadAllArgs(this, offset, entryHeader, state);
                await callback.Invoke(args);

                if (entryHeader.PayloadLength > 0) {
                    await buffer.SkipAsync(entryHeader.PayloadLength, cancellationToken);
                }
                offset += entryHeader.Size;
            }
        }

        /// <summary>
        /// Reads a single record at <paramref name="offset"/> and returns both the metadata and a read buffer for the payload.
        /// </summary>
        public async Task<BinaryEntityStoreReadResult> ReadAsync(long offset, CancellationToken cancellationToken = default)
        {
            await EnsureFlushedAsync(offset, cancellationToken);

            using var headerPrefix = bufferPool.Acquire(HeaderPrefixLength);
            await stream.ReadAsync(offset, headerPrefix.Memory[..HeaderPrefixLength], cancellationToken);
            var marker = headerPrefix.Memory.Span[0];
            var totalLength = BinaryPrimitives.ReadInt32LittleEndian(headerPrefix.Memory.Span.Slice(1, 4));
            var metadataVersion = headerPrefix.Memory.Span[5];
            var metadataLength = BinaryPrimitives.ReadUInt16LittleEndian(headerPrefix.Memory.Span.Slice(6, 2));

            using var metadataHandle = metadataLength > 0 ? bufferPool.Acquire(metadataLength) : null;

            BinaryEntityStoreEntryHeader entryHeader;
            if (metadataLength > 0) {
                var metadataMemory = metadataHandle!.Memory[..metadataLength];
                await stream.ReadAsync(offset + HeaderPrefixLength, metadataMemory, cancellationToken);
                entryHeader = CreateEntryHeader(marker, totalLength, metadataVersion, metadataMemory.Span);
            }
            else {
                entryHeader = CreateEntryHeader(marker, totalLength, metadataVersion, Span<byte>.Empty);
            }

            var payloadLength = entryHeader.PayloadLength;
            if (payloadLength < 0) {
                throw new InvalidOperationException("Corrupted entity store entry encountered.");
            }

            var buffer = bufferPool.AcquireReadBuffer(stream, offset + entryHeader.HeaderLength, payloadLength);
            return new BinaryEntityStoreReadResult(buffer, entryHeader);
        }

        private static BinaryEntityStoreEntryHeader CreateEntryHeader(byte marker, int totalLength, byte metadataVersion, ReadOnlySpan<byte> metadataSpan)
        {
            var metadata = BinaryEntityStoreRecordMetadata.ReadFrom(metadataVersion, metadataSpan);
            var metadataLength = metadataSpan.Length;
            var headerLength = HeaderPrefixLength + metadataLength;
            if (totalLength < headerLength) {
                throw new InvalidOperationException("Corrupted entity store entry encountered.");
            }

            return new BinaryEntityStoreEntryHeader(marker, totalLength, headerLength, metadata);
        }

        /// <summary>
        /// Ensures that all data up to <paramref name="offset"/> has been flushed to the underlying stream.
        /// </summary>
        private async Task EnsureFlushedAsync(long offset, CancellationToken cancellationToken = default)
        {
            if (offset < _flushOffset) return;

            await _flushSemaphore.WaitAsync(cancellationToken);
            try {
                if (offset < _flushOffset) return;

                stream.Flush();
                _flushOffset = stream.Length;
            }
            finally {
                _flushSemaphore.Release();
            }
        }

        /// <summary>
        /// Releases the underlying stream and synchronization primitives.
        /// </summary>
        public void Dispose()
        {
            stream.Dispose();
            _flushSemaphore.Dispose();
        }
    }
}

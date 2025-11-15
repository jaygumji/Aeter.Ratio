/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.IO;
using System;
using System.Buffers.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary
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

    /// <summary>
    /// Represents the result of <see cref="BinaryEntityStore.ReadAsync"/> which couples metadata with a payload buffer.
    /// </summary>
    public sealed class BinaryEntityStoreReadResult : IDisposable
    {
        public BinaryEntityStoreReadResult(BinaryReadBuffer buffer, BinaryEntityStoreEntryHeader header)
        {
            Buffer = buffer;
            Header = header;
        }

        /// <summary>
        /// Gets the buffer positioned at the payload.
        /// </summary>
        public BinaryReadBuffer Buffer { get; }
        /// <summary>
        /// Gets the parsed entry header.
        /// </summary>
        public BinaryEntityStoreEntryHeader Header { get; }

        /// <summary>
        /// Returns the buffer to its pool.
        /// </summary>
        public void Dispose()
        {
            Buffer.Dispose();
        }
    }

    /// <summary>
    /// Metadata describing where a record is stored within the stream and how to interpret it.
    /// </summary>
    public readonly struct BinaryEntityStoreEntryHeader
    {
        internal BinaryEntityStoreEntryHeader(byte marker, int size, int headerLength, BinaryEntityStoreRecordMetadata metadata)
        {
            Marker = marker;
            Size = size;
            HeaderLength = headerLength;
            Metadata = metadata;
        }

        /// <summary>
        /// Gets the marker byte. Values other than 255 mean the record is active.
        /// </summary>
        public byte Marker { get; }
        /// <summary>
        /// Gets the total size of the record including header and payload.
        /// </summary>
        public int Size { get; }
        /// <summary>
        /// Gets the length of the header portion.
        /// </summary>
        public int HeaderLength { get; }
        /// <summary>
        /// Gets the strongly typed metadata describing the entity.
        /// </summary>
        public BinaryEntityStoreRecordMetadata Metadata { get; }
        /// <summary>
        /// Gets the size of the payload portion.
        /// </summary>
        public int PayloadLength => Size - HeaderLength;
        /// <summary>
        /// Gets a value indicating whether the record is active.
        /// </summary>
        public bool IsInUse => Marker != 255;
    }

    /// <summary>
    /// Versioned metadata stored in each record header.
    /// </summary>
    public readonly struct BinaryEntityStoreRecordMetadata
    {
        private const int V1Length = 20;
        public const byte CurrentMetadataVersion = 1;

        public BinaryEntityStoreRecordMetadata(Guid key, uint version, byte metadataVersion = CurrentMetadataVersion)
        {
            Key = key;
            Version = version;
            MetadataVersion = metadataVersion;
        }

        /// <summary>
        /// Gets the entity key.
        /// </summary>
        public Guid Key { get; }
        /// <summary>
        /// Gets the entity version number.
        /// </summary>
        public uint Version { get; }
        /// <summary>
        /// Gets the metadata format version.
        /// </summary>
        public byte MetadataVersion { get; }

        internal int GetSerializedLength()
            => MetadataVersion switch {
                CurrentMetadataVersion => V1Length,
                _ => throw new NotSupportedException($"Metadata version {MetadataVersion} is not supported.")
            };

        internal void WriteTo(Span<byte> destination)
        {
            var length = GetSerializedLength();
            if (destination.Length < length) {
                throw new ArgumentException("Insufficient destination span for metadata.", nameof(destination));
            }

            switch (MetadataVersion) {
                case CurrentMetadataVersion:
                    BinaryPrimitives.WriteUInt32LittleEndian(destination[..4], Version);
                    if (!Key.TryWriteBytes(destination.Slice(4, 16))) {
                        throw new InvalidOperationException("Unable to write entity key to metadata block.");
                    }
                    break;
                default:
                    throw new NotSupportedException($"Metadata version {MetadataVersion} is not supported.");
            }
        }

        internal static BinaryEntityStoreRecordMetadata ReadFrom(byte metadataVersion, ReadOnlySpan<byte> source)
        {
            return metadataVersion switch {
                CurrentMetadataVersion => ReadV1(source),
                _ => throw new NotSupportedException($"Metadata version {metadataVersion} is not supported.")
            };
        }

        private static BinaryEntityStoreRecordMetadata ReadV1(ReadOnlySpan<byte> source)
        {
            if (source.Length < V1Length) {
                throw new ArgumentException("Metadata span too small for v1 layout.", nameof(source));
            }

            var version = BinaryPrimitives.ReadUInt32LittleEndian(source[..4]);
            var key = new Guid(source.Slice(4, 16));
            return new BinaryEntityStoreRecordMetadata(key, version);
        }
    }

    /// <summary>
    /// Arguments passed to <see cref="BinaryEntityStore.ReadAllAsync(Func{BinaryEntityStoreReadAllArgs, Task}, object?, CancellationToken)"/>.
    /// </summary>
    public class BinaryEntityStoreReadAllArgs(BinaryEntityStore store, long offset, BinaryEntityStoreEntryHeader header, object? state)
    {
        /// <summary>
        /// Gets the store that produced the entry.
        /// </summary>
        public BinaryEntityStore Store { get; } = store;
        /// <summary>
        /// Gets the absolute offset of the entry.
        /// </summary>
        public long Offset { get; } = offset;
        /// <summary>
        /// Gets the parsed header.
        /// </summary>
        public BinaryEntityStoreEntryHeader Header { get; } = header;
        /// <summary>
        /// Gets the optional state object passed to <see cref="BinaryEntityStore.ReadAllAsync"/>.
        /// </summary>
        public object? State { get; } = state;
    }
}

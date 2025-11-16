/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.IO;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary
{
    /// <summary>
    /// Stream-oriented buffer that provides efficient random-access reads over <see cref="IBinaryReadStream"/>.
    /// </summary>
    public class BinaryReadBuffer : BinaryBuffer
    {
        private readonly IBinaryReadStream stream;
        private readonly long originOffset;
        private long consumed;

        /// <summary>
        /// Gets the number of valid bytes currently cached in the buffer. A value of -1 means it has not been filled yet.
        /// </summary>
        public int Length { get; private set; } = -1;
        /// <summary>
        /// Gets the total number of bytes consumed from the start offset supplied when the buffer was created.
        /// </summary>
        public long TotalConsumed => consumed;

        /// <summary>
        /// Creates a reader that rents its backing memory from a pool.
        /// Use this overload when buffers are short-lived or large to reduce allocations.
        /// </summary>
        public BinaryReadBuffer(BinaryBufferPool poolHandle, BinaryBufferPool.BinaryMemoryHandle handle, IBinaryReadStream stream, long streamOffset = 0, int streamLength = int.MaxValue)
            : base(poolHandle, handle, stream, streamOffset, streamLength)
        {
            this.stream = stream;
            originOffset = streamOffset;
        }

        /// <summary>
        /// Creates a reader backed by a fixed buffer. Useful for tests or single-use reads.
        /// </summary>
        public BinaryReadBuffer(int size, IBinaryReadStream stream, long streamOffset = 0, int streamLength = int.MaxValue)
            : base(new byte[size], stream, streamOffset, streamLength)
        {
            this.stream = stream;
            originOffset = streamOffset;
        }

        /// <summary>
        /// Gets a value indicating whether the buffer has reached the end of the permitted stream range.
        /// </summary>
        public bool IsEndOfStream => Position == Length && Length < Size;

        private void EnsureInitialized()
        {
            if (Length < 0) RefillBuffer();
        }
        private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
        {
            if (Length < 0) await RefillBufferAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Reads the next byte from the buffer, refilling it synchronously if needed. Prefer this in tight loops that already operate synchronously.
        /// </summary>
        public byte ReadByte()
        {
            RefillBuffer();
            consumed++;
            return Span[Position++];
        }

        /// <summary>
        /// Reads the next byte asynchronously. Use this inside async flows to avoid blocking threads.
        /// </summary>
        public async Task<byte> ReadByteAsync(CancellationToken cancellationToken = default)
        {
            await RefillBufferAsync(cancellationToken: cancellationToken);
            consumed++;
            return Span[Position++];
        }

        private void RefillBuffer(bool require = true)
        {
            var sizeLeft = 0;
            var copyOffset = 0;
            if (Length >= 0) {
                if (Position != Length) {
                    return;
                }
                if (require && Length < Size) {
                    throw new EndOfStreamException();
                }

                sizeLeft = Size - Position;
                if (Position != Length) {
                    Span.Slice(Position, sizeLeft).CopyTo(Span);
                    copyOffset = sizeLeft;
                }
            }

            Position = 0;
            var (streamOffset, streamLength) = GetAndAdvanceStreamPosition(Size - copyOffset);
            Length = sizeLeft + streamLength;
            stream.Read(streamOffset, Memory.Span[copyOffset..Length]);

            if (require && Length == 0) {
                throw new EndOfStreamException();
            }
        }

        private async Task RefillBufferAsync(bool require = true, CancellationToken cancellationToken = default)
        {
            var sizeLeft = 0;
            var copyOffset = 0;
            if (Length >= 0) {
                if (Position != Length) {
                    return;
                }
                if (require && Length < Size) {
                    throw new EndOfStreamException();
                }

                sizeLeft = Size - Position;
                if (Position != Length) {
                    Span.Slice(Position, sizeLeft).CopyTo(Span);
                    copyOffset = sizeLeft;
                }
            }

            Position = 0;
            var (streamOffset, streamLength) = GetAndAdvanceStreamPosition(Size - copyOffset);
            Length = sizeLeft + streamLength;
            await stream.ReadAsync(streamOffset, Memory[copyOffset..Length], cancellationToken);

            if (require && Length == 0) {
                throw new EndOfStreamException();
            }
        }

        /// <summary>
        /// Ensures at least <paramref name="length"/> bytes are buffered synchronously. Use when subsequent reads happen synchronously.
        /// </summary>
        public void RequestSpace(int length)
        {
            if (Length >= 0 && Length - Position >= length) {
                return;
            }

            if (length > Size) {
                EnsureInitialized();
                var sizeLeft = Length - Position;
                var copyOffset = sizeLeft;
                Expand(length, Position, sizeLeft);
                Position = 0;

                var (streamOffset, streamLength) = GetAndAdvanceStreamPosition(Size - copyOffset);
                Length = sizeLeft + streamLength;
                stream.Read(streamOffset, Memory.Span[copyOffset..Length]);
            }
            else {
                RefillBuffer(require: false);
            }
        }

        /// <summary>
        /// Ensures at least <paramref name="length"/> bytes are buffered asynchronously. Use when the caller is already in an async method.
        /// </summary>
        public async Task RequestSpaceAsync(int length, CancellationToken cancellationToken = default)
        {
            if (Length >= 0 && Length - Position >= length) {
                return;
            }

            if (length > Size) {
                await EnsureInitializedAsync(cancellationToken);
                var sizeLeft = Length - Position;
                var copyOffset = sizeLeft;
                Expand(length, Position, sizeLeft);
                Position = 0;

                var (streamOffset, streamLength) = GetAndAdvanceStreamPosition(Size - copyOffset);
                Length = sizeLeft + streamLength;
                await stream.ReadAsync(streamOffset, Memory[copyOffset..Length], cancellationToken);
            }
            else {
                await RefillBufferAsync(require: false, cancellationToken);
            }
        }

        /// <summary>
        /// Reads <paramref name="length"/> bytes synchronously and advances the position. Use this in synchronous code paths for best throughput.
        /// </summary>
        public ReadOnlySpan<byte> Read(int length)
        {
            RequestSpace(length);
            var span = Span.Slice(Position, length);
            Advance(length);
            return span;
        }

        /// <summary>
        /// Reads <paramref name="length"/> bytes asynchronously and advances the position. Prefer this overload when awaiting I/O already.
        /// </summary>
        public async ValueTask<ReadOnlyMemory<byte>> ReadAsync(int length, CancellationToken cancellationToken = default)
        {
            await RequestSpaceAsync(length, cancellationToken);
            var span = Memory.Slice(Position, length);
            await AdvanceAsync(length, cancellationToken);
            return span;
        }

        /// <summary>
        /// Peeks a byte at the current position.
        /// </summary>
        public byte PeekByte() => PeekByte(0);

        /// <summary>
        /// Asynchronously peeks a byte at the current position without advancing.
        /// </summary>
        public ValueTask<byte> PeekByteAsync(CancellationToken cancellationToken = default) => PeekByteAsync(0, cancellationToken);

        /// <summary>
        /// Peeks a byte at the specified <paramref name="offset"/> from the current position.
        /// </summary>
        public byte PeekByte(int offset)
        {
            EnsureInitialized();
            var offsetPosition = Position + offset;
            if (offsetPosition >= Length) {
                if (Length < Size) {
                    throw new EndOfStreamException();
                }
                RefillBuffer();
                if (offsetPosition >= Length) {
                    throw new EndOfStreamException();
                }
            }
            return Span[offsetPosition];
        }

        /// <summary>
        /// Asynchronously peeks a byte at the specified <paramref name="offset"/> from the current position.
        /// </summary>
        public async ValueTask<byte> PeekByteAsync(int offset, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            var offsetPosition = Position + offset;
            if (offsetPosition >= Length) {
                if (Length < Size) {
                    throw new EndOfStreamException();
                }
                await RefillBufferAsync(cancellationToken: cancellationToken);
                if (offsetPosition >= Length) {
                    throw new EndOfStreamException();
                }
            }
            return Span[offsetPosition];
        }

        /// <summary>
        /// Copies the remaining data to <paramref name="destArr"/> synchronously. Prefer this for small payloads.
        /// </summary>
        public void CopyTo(byte[] destArr)
        {
            CopyTo(destArr, 0, destArr.Length);
        }

        /// <summary>
        /// Copies the remaining data to <paramref name="destArr"/> asynchronously. Use this when the copy might block on I/O.
        /// </summary>
        public Task CopyToAsync(byte[] destArr, CancellationToken cancellationToken = default)
        {
            return CopyToAsync(destArr, 0, destArr.Length, cancellationToken);
        }

        /// <summary>
        /// Copies <paramref name="length"/> bytes into <paramref name="destArr"/> at <paramref name="destOffset"/> synchronously.
        /// </summary>
        public void CopyTo(byte[] destArr, int destOffset, int length)
        {
            if (destArr.Length < length) throw new ArgumentException("Insufficient length of array", nameof(destArr));

            var remaining = length;
            var writeOffset = destOffset;
            while (remaining > 0) {
                if (Length < 0 || Position == Length) {
                    RefillBuffer(require: false);
                    if (Length == Position) {
                        throw new EndOfStreamException();
                    }
                }

                var available = Math.Min(Length - Position, remaining);
                Span.Slice(Position, available).CopyTo(destArr.AsSpan(writeOffset, available));
                Position += available;
                consumed += available;
                remaining -= available;
                writeOffset += available;
            }
        }

        /// <summary>
        /// Copies <paramref name="length"/> bytes into <paramref name="destArr"/> asynchronously.
        /// </summary>
        public async Task CopyToAsync(byte[] destArr, int destOffset, int length, CancellationToken cancellationToken = default)
        {
            if (destArr.Length < length) throw new ArgumentException("Insufficient length of array", nameof(destArr));

            var remaining = length;
            var writeOffset = destOffset;
            while (remaining > 0) {
                if (Length < 0 || Position == Length) {
                    await RefillBufferAsync(require: false, cancellationToken: cancellationToken);
                    if (Length == Position) {
                        throw new EndOfStreamException();
                    }
                }

                var available = Math.Min(Length - Position, remaining);
                Span.Slice(Position, available).CopyTo(destArr.AsSpan(writeOffset, available));
                Position += available;
                consumed += available;
                remaining -= available;
                writeOffset += available;
            }
        }

        /// <summary>
        /// Copies <paramref name="length"/> bytes into <paramref name="writeBuffer"/> asynchronously.
        /// </summary>
        public async Task CopyToAsync(BinaryWriteBuffer writeBuffer, int length, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(writeBuffer);
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            var remaining = length;
            while (remaining > 0) {
                if (Length < 0 || Position == Length) {
                    await RefillBufferAsync(require: false, cancellationToken: cancellationToken);
                    if (Length == Position) {
                        throw new EndOfStreamException();
                    }
                }

                var available = Math.Min(Length - Position, remaining);
                await writeBuffer.WriteAsync(Memory.Slice(Position, available), cancellationToken);
                Position += available;
                consumed += available;
                remaining -= available;
            }
        }

        /// <summary>
        /// Advances the in-buffer position synchronously. Only use this directly if <see cref="RequestSpace"/> has been called for the same length.
        /// </summary>
        public void Advance(int length)
        {
            RefillBuffer();
            Position += length;
            consumed += length;
        }

        /// <summary>
        /// Advances the position asynchronously. Use this alongside <see cref="RequestSpaceAsync"/> to remain on the async path.
        /// </summary>
        public async Task AdvanceAsync(int length, CancellationToken cancellationToken = default)
        {
            await RefillBufferAsync(cancellationToken: cancellationToken);
            Position += length;
            consumed += length;
        }

        /// <summary>
        /// Skips <paramref name="length"/> bytes, requesting additional space as needed.
        /// Prefer this helper for large skips instead of manual Request/Advance loops.
        /// </summary>
        public async Task SkipAsync(int length, CancellationToken cancellationToken = default)
        {
            if (length < 0) throw new ArgumentOutOfRangeException(nameof(length));

            var remaining = length;
            while (remaining > 0) {
                if (Length < 0 || Position == Length) {
                    await RefillBufferAsync(require: false, cancellationToken: cancellationToken);
                    if (Length == Position) {
                        throw new EndOfStreamException();
                    }
                }

                var available = Length - Position;
                var consume = Math.Min(available, remaining);
                Position += consume;
                consumed += consume;
                remaining -= consume;
            }
        }

        /// <summary>
        /// Skips forward to the absolute <paramref name="offset"/> measured from the origin supplied when the buffer was created.
        /// </summary>
        public async Task SkipToAsync(long offset, CancellationToken cancellationToken = default)
        {
            var current = originOffset + consumed;
            if (offset < current) {
                throw new ArgumentOutOfRangeException(nameof(offset), "Cannot skip backwards in the stream.");
            }

            var delta = offset - current;
            while (delta > 0) {
                var chunk = delta > int.MaxValue ? int.MaxValue : (int)delta;
                await SkipAsync(chunk, cancellationToken);
                delta -= chunk;
            }
        }
    }
}

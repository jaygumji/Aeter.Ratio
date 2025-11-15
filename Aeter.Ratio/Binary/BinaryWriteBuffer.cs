/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary
{
    /// <summary>
    /// Buffered writer that batches data before sending it to an <see cref="IBinaryWriteStream"/>.
    /// </summary>
    public class BinaryWriteBuffer : BinaryBuffer
    {
        private readonly List<BinaryBufferReservation> _reservations = new List<BinaryBufferReservation>();
        private readonly IBinaryWriteStream stream;
        private BinaryBufferReservation? _firstReservation;

        /// <summary>
        /// Creates a writer that obtains its memory from a pool. Use this in production paths to minimize allocations.
        /// </summary>
        public BinaryWriteBuffer(BinaryBufferPool pool, BinaryBufferPool.BinaryMemoryHandle handle, IBinaryWriteStream stream, long streamOffset = 0, int streamLength = int.MaxValue)
            : base(pool, handle, stream, streamOffset, streamLength)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Creates a writer backed by a fixed-size array. Handy for tests or when pooling is not required.
        /// </summary>
        public BinaryWriteBuffer(int size, IBinaryWriteStream stream, long streamOffset = 0, int streamLength = int.MaxValue) : base(new byte[size], stream, streamOffset, streamLength)
        {
            this.stream = stream;
        }

        private bool HasReservations => _firstReservation != null;

        /// <summary>
        /// Ensures there is enough space to write <paramref name="length"/> bytes synchronously.
        /// Prefer this when writing from synchronous code paths.
        /// </summary>
        public void RequestSpace(int length)
        {
            Verify();

            if (Size - Position > length)
                return;

            if (_firstReservation == null && length <= Size) {
                Flush();
                return;
            }

            Expand(length, 0, Position);
        }

        /// <summary>
        /// Ensures there is enough space to write <paramref name="length"/> bytes asynchronously.
        /// Use this when the caller is already using async I/O.
        /// </summary>
        public async Task RequestSpaceAsync(int length, CancellationToken cancellationToken = default)
        {
            Verify();

            if (Size - Position > length)
                return;

            if (_firstReservation == null && length <= Size) {
                await FlushAsync(cancellationToken);
                return;
            }

            Expand(length, 0, Position);
        }

        /// <summary>
        /// Flushes buffered data to the underlying stream synchronously. Use this to keep latency low when you know the thread can block.
        /// </summary>
        public void Flush()
        {
            Verify();

            if (Position <= 0) return;

            var (streamOffset, streamLength) = GetAndAdvanceStreamPosition(Position);
            if (streamLength < Position) {
                throw new EndOfStreamException();
            }
            stream.Write(streamOffset, Memory.Span[..Position]);
            Position = 0;
        }

        /// <summary>
        /// Flushes buffered data asynchronously. Prefer this in asynchronous workflows so flushing does not block the caller.
        /// </summary>
        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            Verify();

            if (Position <= 0) return;

            var (streamOffset, streamLength) = GetAndAdvanceStreamPosition(Position);
            if (streamLength < Position) {
                throw new EndOfStreamException();
            }
            await stream.WriteAsync(streamOffset, Memory[..Position], cancellationToken);
            Position = 0;
        }

        /// <summary>
        /// Writes the difference between the current position and the reservation as an unsigned 32-bit value.
        /// </summary>
        public void ApplyUInt32Size(BinaryBufferReservation reservation)
        {
            Verify();

            var value = (uint)(Position - reservation.Position);
            var buffer = BinaryInformation.UInt32.Converter.Convert(value);
            Use(reservation, buffer);
        }

        /// <summary>
        /// Writes the difference between the current position and the reservation as a signed 32-bit value.
        /// </summary>
        public void ApplyInt32Size(BinaryBufferReservation reservation)
        {
            Verify();

            var value = Position - reservation.Position;
            var buffer = BinaryInformation.Int32.Converter.Convert(value);
            Use(reservation, buffer);
        }

        /// <summary>
        /// Fills the reserved area with <paramref name="value"/>. Use when you computed a custom payload.
        /// </summary>
        public void Use(BinaryBufferReservation reservation, Span<byte> value)
        {
            Verify();

            if (value.Length > reservation.Size)
                throw new ArgumentException("The supplied buffer can not exceed the reservation size");

            value.CopyTo(Span.Slice(reservation.Position, value.Length));

            if (ReferenceEquals(_firstReservation, reservation)) {
                _reservations.RemoveAt(0);
                _firstReservation = _reservations.FirstOrDefault();
                return;
            }

            _reservations.Remove(reservation);
        }

        /// <summary>
        /// Reserves <paramref name="size"/> bytes synchronously. Ideal when writing headers that need back-patching later.
        /// </summary>
        public BinaryBufferReservation Reserve(int size)
        {
            Verify();

            RequestSpace(size);
            if (!HasReservations) {
                if (Size - Position < size / 2) {
                    Flush();
                }
            }

            var reservation = new BinaryBufferReservation(Position, size);

            Span.Slice(Position, size).Clear();
            Position += size;

            _reservations.Add(reservation);
            if (_firstReservation == null)
                _firstReservation = reservation;

            return reservation;
        }

        /// <summary>
        /// Reserves <paramref name="size"/> bytes asynchronously.
        /// </summary>
        public async Task<BinaryBufferReservation> ReserveAsync(int size, CancellationToken cancellationToken = default)
        {
            Verify();

            await RequestSpaceAsync(size, cancellationToken);
            if (!HasReservations) {
                // If less than half of the buffer size is left
                if (Size - Position < size/2) {
                    // Preemptive flush to ensure we have space to write
                    // the data the reservation is about
                    await FlushAsync(cancellationToken);
                }
            }

            var reservation = new BinaryBufferReservation(Position, size);

            Span.Slice(Position, size).Clear();
            Position += size;

            _reservations.Add(reservation);
            if (_firstReservation == null)
                _firstReservation = reservation;

            return reservation;
        }

        /// <summary>
        /// Returns a writable span of <paramref name="length"/> bytes. Use for synchronous writes.
        /// </summary>
        public Span<byte> Write(int length)
        {
            Verify();

            RequestSpace(length);

            var span = Span.Slice(Position, length);
            Position += length;
            return span;
        }

        /// <summary>
        /// Returns a writable memory block asynchronously. Use inside async methods to avoid blocking.
        /// </summary>
        public async Task<Memory<byte>> WriteAsync(int length, CancellationToken cancellationToken = default)
        {
            Verify();

            await RequestSpaceAsync(length, cancellationToken);

            var mem = Memory.Slice(Position, length);
            Position += length;
            return mem;
        }

        /// <summary>
        /// Writes a single byte to the buffer synchronously.
        /// </summary>
        public void WriteByte(byte value)
        {
            Verify();

            RequestSpace(1);

            Span[Position++] = value;
        }

        /// <summary>
        /// Writes the provided span synchronously. Use when the payload is readily available in memory.
        /// </summary>
        public void Write(ReadOnlySpan<byte> buffer)
        {
            Verify();

            RequestSpace(buffer.Length);
            buffer.CopyTo(Span.Slice(Position, buffer.Length));
            Position += buffer.Length;
        }

        /// <summary>
        /// Writes the provided memory asynchronously. Pick this when the caller is using async I/O.
        /// </summary>
        public async Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Verify();

            await RequestSpaceAsync(buffer.Length, cancellationToken);
            buffer.CopyTo(Memory.Slice(Position, buffer.Length));
            Position += buffer.Length;
        }

        /// <summary>
        /// Moves the current position forward synchronously, reserving <paramref name="length"/> bytes for future writes.
        /// </summary>
        public int Advance(int length)
        {
            Verify();

            RequestSpace(length);
            var position = Position;
            Position += length;
            return position;
        }

        /// <summary>
        /// Moves the current position forward asynchronously. Use this rather than <see cref="Advance"/> when the surrounding code awaits I/O.
        /// </summary>
        public async Task<int> AdvanceAsync(int length, CancellationToken cancellationToken = default)
        {
            Verify();

            await RequestSpaceAsync(length, cancellationToken);
            var position = Position;
            Position += length;
            return position;
        }

        protected override void OnDispose()
        {
            if (Position <= 0) return;

            // Flush unwritten data to stream
            var (streamOffset, streamLength) = GetAndAdvanceStreamPosition(Position);
            if (streamLength < Position) {
                throw new EndOfStreamException();
            }
            stream.Write(streamOffset, Memory.Span[..Position]);
        }
    }
}

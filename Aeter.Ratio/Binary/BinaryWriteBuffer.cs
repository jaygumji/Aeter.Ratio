/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary
{
    public class BinaryWriteBuffer : BinaryBuffer
    {
        private readonly List<BinaryBufferReservation> _reservations = new List<BinaryBufferReservation>();

        private BinaryBufferReservation? _firstReservation;

        public BinaryWriteBuffer(BinaryBufferPool pool, BinaryBufferPool.BinaryMemoryHandle handle, Stream stream)
            : base(pool, handle, stream)
        {
        }

        public BinaryWriteBuffer(int size, Stream stream) : base(new byte[size], stream)
        {
        }

        private bool HasReservations => _firstReservation != null;

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

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            Verify();

            if (Position <= 0) return;

            await Stream.WriteAsync(Memory[..Position], cancellationToken);
            Position = 0;
        }

        public void ApplyUInt32Size(BinaryBufferReservation reservation)
        {
            Verify();

            var value = (uint)(Position - reservation.Position);
            var buffer = BinaryInformation.UInt32.Converter.Convert(value);
            Use(reservation, buffer, 0, buffer.Length);
        }

        public void ApplyInt32Size(BinaryBufferReservation reservation)
        {
            Verify();

            var value = Position - reservation.Position;
            var buffer = BinaryInformation.Int32.Converter.Convert(value);
            Use(reservation, buffer, 0, buffer.Length);
        }

        public void Use(BinaryBufferReservation reservation, byte[] buffer, int offset, int length)
        {
            Verify();

            if (buffer.Length > reservation.Size)
                throw new ArgumentException("The supplied buffer can not exceed the reservation size");

            buffer.AsSpan(offset, buffer.Length).CopyTo(Span.Slice(reservation.Position, buffer.Length));

            if (_firstReservation == reservation) {
                _reservations.RemoveAt(0);
                _firstReservation = _reservations.FirstOrDefault();
                return;
            }

            _reservations.Remove(reservation);
        }

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

        public void WriteByteAsync(byte value)
        {
            Verify();

            if (Position >= Size)
                throw new ArgumentException("Buffer is out of space.");

            Span[Position++] = value;
        }

        public async Task WriteAsync(byte[] buffer, CancellationToken cancellationToken = default)
        {
            await WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        }

        public async Task WriteAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken = default)
        {
            Verify();

            await RequestSpaceAsync(length, cancellationToken);
            buffer.AsSpan(offset, length).CopyTo(Span.Slice(Position, length));
            Position += length;
        }

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
            Stream.Write(Memory[..Position].Span);
        }

    }
}

/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary
{
    public class BinaryReadBuffer : BinaryBuffer
    {

        public int Length { get; private set; }

        public BinaryReadBuffer(BinaryBufferPool poolHandle, BinaryBufferPool.BinaryMemoryHandle handle, Stream stream)
            : base(poolHandle, handle, stream)
        {
        }

        public BinaryReadBuffer(int size, Stream stream) : base(new byte[size], stream)
        {
        }

        public bool IsEndOfStream => Position == Length && Length < Size;

        public async Task<byte> ReadByteAsync(CancellationToken cancellationToken = default)
        {
            await RefillBufferAsync(cancellationToken: cancellationToken);
            return Span[Position++];
        }

        private async Task RefillBufferAsync(bool force = false, CancellationToken cancellationToken = default)
        {
            if (!force && Position != Length) {
                return;
            }
            if (Length < Size) {
                throw new EndOfStreamException();
            }

            var sizeLeft = Size - Position;
            var copyOffset = 0;
            if (Position != Length) {
                Span.Slice(Position, sizeLeft).CopyTo(Span);
                copyOffset = sizeLeft;
            }
            Position = 0;

            Length = sizeLeft + await Stream.ReadAsync(Memory[copyOffset..Size], cancellationToken);

            if (Length == 0) {
                throw new EndOfStreamException();
            }
        }

        public async Task RequestSpaceAsync(int length, CancellationToken cancellationToken = default)
        {
            if (Length - Position >= length) {
                return;
            }

            if (length > Size) {
                var sizeLeft = Length - Position;
                var copyOffset = sizeLeft;
                Expand(length, Position, sizeLeft);
                Position = 0;

                Length = sizeLeft + await Stream.ReadAsync(Memory[copyOffset..Size], cancellationToken);
            }
            else {
                await RefillBufferAsync(force: true, cancellationToken);
            }
        }

        public Task<byte> PeekByteAsync(CancellationToken cancellationToken = default) => PeekByteAsync(0, cancellationToken);
        public async Task<byte> PeekByteAsync(int offset, CancellationToken cancellationToken = default)
        {
            var offsetPosition = Position + offset;
            if (offsetPosition >= Length) {
                if (Length < Size) {
                    throw new EndOfStreamException();
                }
                await RefillBufferAsync(force: true, cancellationToken: cancellationToken);
                if (offsetPosition >= Length) {
                    throw new EndOfStreamException();
                }
            }
            return Span[offsetPosition];
        }

        public Task CopyToAsync(byte[] destArr, CancellationToken cancellationToken = default)
        {
            return CopyToAsync(destArr, 0, destArr.Length, cancellationToken);
        }
        public async Task CopyToAsync(byte[] destArr, int destOffset, int length, CancellationToken cancellationToken = default)
        {
            if (destArr.Length < length) throw new System.ArgumentException("Insufficient length of array", nameof(destArr));
            await RequestSpaceAsync(length, cancellationToken);
            Span.Slice(Position, length).CopyTo(destArr.AsSpan(destOffset, length));
            await AdvanceAsync(length, cancellationToken);
        }

        public async Task AdvanceAsync(int length, CancellationToken cancellationToken = default)
        {
            await RefillBufferAsync(cancellationToken: cancellationToken);
            Position += length;
        }

    }
}

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

        public int Length { get; private set; } = -1;

        public BinaryReadBuffer(BinaryBufferPool poolHandle, BinaryBufferPool.BinaryMemoryHandle handle, Stream stream)
            : base(poolHandle, handle, stream)
        {
        }

        public BinaryReadBuffer(int size, Stream stream) : base(new byte[size], stream)
        {
        }

        public bool IsEndOfStream => Position == Length && Length < Size;

        private void EnsureInitialized()
        {
            if (Length < 0) RefillBuffer();
        }
        private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
        {
            if (Length < 0) await RefillBufferAsync(cancellationToken: cancellationToken);
        }

        public byte ReadByte()
        {
            RefillBuffer();
            return Span[Position++];
        }

        public async Task<byte> ReadByteAsync(CancellationToken cancellationToken = default)
        {
            await RefillBufferAsync(cancellationToken: cancellationToken);
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
            Length = sizeLeft + Read(Memory[copyOffset..Size]);

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
            Length = sizeLeft + await ReadAsync(Memory[copyOffset..Size], cancellationToken);

            if (require && Length == 0) {
                throw new EndOfStreamException();
            }
        }

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

                Length = sizeLeft + Read(Memory[copyOffset..Size]);
            }
            else {
                RefillBuffer(require: false);
            }
        }

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

                Length = sizeLeft + await ReadAsync(Memory[copyOffset..Size], cancellationToken);
            }
            else {
                await RefillBufferAsync(require: false, cancellationToken);
            }
        }

        public byte PeekByte() => PeekByte(0);

        public Task<byte> PeekByteAsync(CancellationToken cancellationToken = default) => PeekByteAsync(0, cancellationToken);

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

        public async Task<byte> PeekByteAsync(int offset, CancellationToken cancellationToken = default)
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

        public void CopyTo(byte[] destArr)
        {
            CopyTo(destArr, 0, destArr.Length);
        }

        public Task CopyToAsync(byte[] destArr, CancellationToken cancellationToken = default)
        {
            return CopyToAsync(destArr, 0, destArr.Length, cancellationToken);
        }

        public void CopyTo(byte[] destArr, int destOffset, int length)
        {
            if (destArr.Length < length) throw new ArgumentException("Insufficient length of array", nameof(destArr));
            RequestSpace(length);
            Span.Slice(Position, length).CopyTo(destArr.AsSpan(destOffset, length));
            Advance(length);
        }

        public async Task CopyToAsync(byte[] destArr, int destOffset, int length, CancellationToken cancellationToken = default)
        {
            if (destArr.Length < length) throw new System.ArgumentException("Insufficient length of array", nameof(destArr));
            await RequestSpaceAsync(length, cancellationToken);
            Span.Slice(Position, length).CopyTo(destArr.AsSpan(destOffset, length));
            await AdvanceAsync(length, cancellationToken);
        }

        public void Advance(int length)
        {
            RefillBuffer();
            Position += length;
        }

        public async Task AdvanceAsync(int length, CancellationToken cancellationToken = default)
        {
            await RefillBufferAsync(cancellationToken: cancellationToken);
            Position += length;
        }

    }
}

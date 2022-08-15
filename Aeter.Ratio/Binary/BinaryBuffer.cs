/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.IO;

namespace Aeter.Ratio.Binary
{
    public class BinaryBuffer : IDisposable
    {
        private readonly IBinaryBufferPool? _pool;
        private bool _isDisposed;

        protected Stream Stream { get; }

        public byte[] Buffer { get; private set; }
        public int Position { get; protected set; }
        protected int Size { get; set; }

        public BinaryBuffer(IBinaryBufferPool? pool, byte[] buffer, Stream stream)
        {
            Size = buffer.Length;
            Stream = stream;
            _pool = pool;
            Position = 0;
            Buffer = buffer;
        }

        protected void Expand(int length, int keepPosition, int keepLength)
        {
            Verify();

            var newSize = Math.Max(length, Size * 2);

            if (_pool == null) throw new NotSupportedException("No pool has been specified, can not expand");

            var newBuffer = _pool.AcquireBuffer(newSize);
            System.Buffer.BlockCopy(Buffer, keepPosition, newBuffer, 0, keepLength);
            _pool.Release(Buffer);

            Buffer = newBuffer;
            Size = newBuffer.Length;
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

            _pool?.Release(Buffer);

            Buffer = Array.Empty<byte>();
            Position = -1;
            Size = 0;
        }

        protected virtual void OnDispose()
        {
        }

    }
}
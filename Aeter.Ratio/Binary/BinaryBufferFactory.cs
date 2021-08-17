/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.IO;

namespace Aeter.Ratio.Binary
{
    public class BinaryBufferFactory : IBinaryBufferPool
    {
        private readonly int _size;

        public BinaryBufferFactory()
            : this(1024)
        {
        }

        public BinaryBufferFactory(int size)
        {
            _size = size;
        }

        public BinaryWriteBuffer AcquireWriteBuffer(Stream stream)
        {
            return new BinaryWriteBuffer(this, AcquireBuffer(_size), stream);
        }

        public BinaryReadBuffer AcquireReadBuffer(Stream stream)
        {
            return new BinaryReadBuffer(this, AcquireBuffer(_size), stream);
        }

        public byte[] AcquireBuffer(int minSize)
        {
            return new byte[minSize];
        }

        public void Release(byte[] buffer)
        {
        }
    }
}
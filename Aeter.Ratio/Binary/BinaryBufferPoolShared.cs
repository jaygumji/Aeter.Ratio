/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Buffers;

namespace Aeter.Ratio.Binary
{
    public class BinaryBufferPoolShared : BinaryBufferPool
    {
        public BinaryBufferPoolShared() : base(1024)
        {
        }

        protected override BinaryMemoryHandle OnAcquire(int minSize)
        {
            return new Handle(this, MemoryPool<byte>.Shared.Rent(minSize));
        }

        protected override void OnRelease(BinaryMemoryHandle handle)
        {
            ((Handle)handle).MemoryOwner.Dispose();
        }

        private class Handle : BinaryMemoryHandle
        {
            public IMemoryOwner<byte> MemoryOwner { get; }

            public Handle(BinaryBufferPool owner, IMemoryOwner<byte> memoryOwner) : base(owner)
            {
                MemoryOwner = memoryOwner;
            }

            public override Memory<byte> Memory => MemoryOwner.Memory;
        }
    }
}
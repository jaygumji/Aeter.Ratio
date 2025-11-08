/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Binary
{
    public class BinaryMemoryProvider : BinaryBufferPool
    {
        private readonly Memory<byte> single;

        public BinaryMemoryProvider(int minSize = 1024) : base(minSize)
        {
            this.single = Array.Empty<byte>();
        }
        private BinaryMemoryProvider(Memory<byte> single) : base(single.Length)
        {
            this.single = single;
        }

        public static BinaryMemoryProvider Single(Memory<byte> single, out BinaryMemoryHandle handle)
        {
            var provider = new BinaryMemoryProvider(single.Length);
            handle = provider.Acquire(single.Length);
            return provider;
        }

        protected override BinaryMemoryHandle OnAcquire(int minSize)
        {
            return new Handle(this, single.Length > 0 ? single : new byte[minSize].AsMemory());
        }

        protected override void OnRelease(BinaryMemoryHandle handle)
        {
        }

        private class Handle : BinaryMemoryHandle
        {
            private readonly Memory<byte> memory;

            public Handle(BinaryBufferPool owner, Memory<byte> memory) : base(owner)
            {
                this.memory = memory;
            }

            public override Memory<byte> Memory => memory;
        }
    }
}
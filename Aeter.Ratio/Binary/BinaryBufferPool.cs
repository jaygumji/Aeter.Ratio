/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.IO;
using System;
using System.IO;

namespace Aeter.Ratio.Binary
{
    /// <summary>
    /// A binary buffer pool to efficiently use binary memory
    /// </summary>
    public abstract class BinaryBufferPool
    {
        public static BinaryBufferPool Default { get; } = new BinaryBufferPoolShared();
        public static BinaryMemoryHandle InvalidHandle { get; } = new InvalidHandleInstance(null!);

        protected int MinSize { get; }

        protected BinaryBufferPool(int minSize)
        {
            MinSize = minSize;
        }

        /// <summary>
        /// Acquires the buffer for the requested stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="streamOffset">The offset in stream where we start writing</param>
        /// <param name="streamLength">The length of data in stream we are allowed to write to</param>
        /// <returns>BinaryBuffer.</returns>
        public BinaryWriteBuffer AcquireWriteBuffer(IBinaryWriteStream stream, long streamOffset = 0, int streamLength = int.MaxValue)
        {
            var handle = Acquire(MinSize);
            return new BinaryWriteBuffer(this, handle, stream, streamOffset, streamLength);
        }

        /// <summary>
        /// Acquires the buffer for the requested stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="streamOffset">The offset in stream where we start writing</param>
        /// <param name="streamLength">The length of data in stream we are allowed to write to</param>
        /// <returns>BinaryBuffer.</returns>
        public BinaryWriteBuffer AcquireWriteBuffer(Stream stream, long streamOffset = 0, int streamLength = int.MaxValue)
            => AcquireWriteBuffer(BinaryStream.Passthrough(stream), streamOffset, streamLength);

        /// <summary>
        /// Acquires the buffer for the requested stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="streamOffset">The offset in stream where we start reading</param>
        /// <param name="streamLength">The length of data in stream we are allowed to read</param>
        /// <returns>BinaryBuffer.</returns>
        public BinaryReadBuffer AcquireReadBuffer(IBinaryReadStream stream, long streamOffset = 0, int streamLength = int.MaxValue)
        {
            var handle = Acquire(MinSize);
            return new BinaryReadBuffer(this, handle, stream, streamOffset, streamLength);
        }

        /// <summary>
        /// Acquires the buffer for the requested stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="streamOffset">The offset in stream where we start reading</param>
        /// <param name="streamLength">The length of data in stream we are allowed to read</param>
        /// <returns>BinaryBuffer.</returns>
        public BinaryReadBuffer AcquireReadBuffer(Stream stream, long streamOffset = 0, int streamLength = int.MaxValue)
            => AcquireReadBuffer(BinaryStream.Passthrough(stream), streamOffset, streamLength);

        /// <summary>
        /// Get a binary memory to use.
        /// </summary>
        /// <param name="minSize">The minimum size of the new binary memory</param>
        /// <returns>Binary memory handle.</returns>
        public BinaryMemoryHandle Acquire(int minSize)
        {
            return OnAcquire(minSize);
        }

        protected abstract BinaryMemoryHandle OnAcquire(int minSize);

        /// <summary>
        /// Releases the specified binary memory handle.
        /// </summary>
        /// <param name="handle">The binary memory handle.</param>
        private void Release(BinaryMemoryHandle handle)
        {
            OnRelease(handle);
        }

        protected abstract void OnRelease(BinaryMemoryHandle handle);

        public abstract class BinaryMemoryHandle : IDisposable
        {
            private readonly BinaryBufferPool owner;

            public BinaryMemoryHandle(BinaryBufferPool owner)
            {
                this.owner = owner;
            }

            public abstract Memory<byte> Memory { get; }

            public void Dispose()
            {
                OnDispose(owner);
                owner.Release(this);
            }

            protected virtual void OnDispose(BinaryBufferPool owner) { }
        }
        private class InvalidHandleInstance : BinaryMemoryHandle
        {
            public InvalidHandleInstance(BinaryBufferPool owner) : base(owner)
            {
            }

            public override Memory<byte> Memory => throw new InvalidOperationException("Handle is no longer available");

            protected override void OnDispose(BinaryBufferPool owner)
            {
                throw new InvalidOperationException("Handle is no longer available");
            }
        }
    }
}
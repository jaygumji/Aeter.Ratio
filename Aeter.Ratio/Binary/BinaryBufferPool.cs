/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Scheduling;
using System;
using System.IO;
using System.Threading;

namespace Aeter.Ratio.Binary
{
    public class BinaryBufferPool : IBinaryBufferPool, IDisposable
    {
        private static readonly Lazy<IBinaryBufferPool> LazySingleton
            = new Lazy<IBinaryBufferPool>(() => new BinaryBufferPool(1024, TimeSpan.FromMinutes(20)));
        public static IBinaryBufferPool Instance => LazySingleton.Value;

        private readonly int _size;
        private readonly TimeSpan _slidingExpiration;
        private readonly BufferLevel[] _bufferLevels;
        private readonly SemaphoreSlim _sem = new SemaphoreSlim(1);
        private readonly SemaphoreSlim _semTimer = new SemaphoreSlim(1);
        private readonly DateTimeQueue<BufferLevel> _slidingQueue;
        private Timer _timer;
        private DateTime _timerDueAt;
        private int _upperIndex;
        private readonly int _maxIndex;

        private readonly int _size1;
        private readonly int _size2;
        private readonly int _size3;
        private readonly int _size4;
        private readonly int _size5;
        private readonly int _size6;
        private readonly int _size7;
        private readonly int _size8;
        private readonly int _size9;
        private readonly int _size10;
        private readonly int _size11;
        private readonly int _size12;
        private readonly int _size13;
        private readonly int _size14;
        private readonly int _size15;
        private readonly int _size16;
        private readonly int _size17;
        private readonly int _size18;
        private readonly int _size19;
        private readonly int _size20;

        /// <summary>
        /// Creates a new instance of <see cref="BinaryBufferPool"/>
        /// with size of new buffers set to 1024.
        /// </summary>
        public BinaryBufferPool()
            : this(1024, TimeSpan.Zero)
        {
        }

        public BinaryBufferPool(int size)
            : this(size, TimeSpan.Zero)
        {
        }

        public BinaryBufferPool(int size, TimeSpan slidingExpiration)
        {
            if (size < 1024)
            {
                throw new ArgumentException("Size may not be under 1024.");
            }
            _size = size;
            _slidingExpiration = slidingExpiration;
            _bufferLevels = new BufferLevel[20];
            _bufferLevels[0] = new BufferLevel(size);
            _slidingQueue = new DateTimeQueue<BufferLevel>();

            _maxIndex = (int)Math.Log(int.MaxValue / _size, 2);
            _size1 = _size * 2;
            _size2 = _size1 * 2;
            _size3 = _size2 * 2;
            _size4 = _size3 * 2;
            _size5 = _size4 * 2;
            _size6 = _size5 * 2;
            _size7 = _size6 * 2;
            _size8 = _size7 * 2;
            _size9 = _size8 * 2;
            _size10 = _size9 * 2;
            _size11 = _size10 * 2;
            _size12 = _size11 * 2;
            _size13 = _size12 * 2;
            _size14 = _size13 * 2;
            _size15 = _size14 * 2;
            _size16 = _size15 * 2;
            _size17 = _size16 * 2;
            _size18 = _size17 * 2;
            _size19 = _size18 * 2;
            _size20 = _size19 * 2;
        }

        private int GetIndexOf(int size)
        {
            if (size == _size) return 0;
            if (size == _size1) return 1;
            if (size == _size2) return 2;
            if (size == _size3) return 3;
            if (size == _size4) return 4;
            if (size == _size5) return 5;
            if (size == _size6) return 6;
            if (size == _size7) return 7;
            if (size == _size8) return 8;
            if (size == _size9) return 9;
            if (size == _size10) return 10;
            if (size == _size11) return 11;
            if (size == _size12) return 12;
            if (size == _size13) return 13;
            if (size == _size14) return 14;
            if (size == _size15) return 15;
            if (size == _size16) return 16;
            if (size == _size17) return 17;
            if (size == _size18) return 18;
            if (size == _size19) return 19;
            if (size == _size20) return 20;

            throw new ArgumentException("Invalid size " + size);
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
            var rest = minSize % _size;
            if (rest > 0)
            {
                // Makes all sized divisable by _size
                // So that we have less buffer levels
                minSize += _size - rest;
            }
            var sizeIndex = GetIndexOf(minSize);
            BufferLevel level = null;

            _sem.Wait();
            try
            {
                for (var i = _upperIndex; i >= sizeIndex; i--)
                {
                    level = _bufferLevels[i];
                    if (i == sizeIndex)
                    {
                        break;
                    }
                    if (level == null) continue;

                    if (level.TryPop(out var buffer))
                    {
                        return buffer;
                    }
                }
            }
            finally
            {
                _sem.Release();
            }
            return level == null
                ? new byte[minSize]
                : level.PopOrCreate();
        }

        public void Release(byte[] buffer)
        {
            var size = buffer.Length;
            var sizeIndex = GetIndexOf(size);

            BufferLevel level;
            _sem.Wait();
            try
            {
                level = _bufferLevels[sizeIndex];
                if (level == null)
                {
                    level = new BufferLevel(size);
                    _bufferLevels[sizeIndex] = level;
                    if (_upperIndex < sizeIndex)
                    {
                        _upperIndex = sizeIndex;
                    }
                }
            }
            finally
            {
                _sem.Release();
            }
            level.Release(buffer);
            if (_slidingExpiration <= TimeSpan.Zero) return;
            _slidingQueue.Enqueue(DateTime.Now.Add(_slidingExpiration), level);
            SetupTimer();
        }

        private void SetupTimer()
        {
            var now = DateTime.Now;
            if (_timerDueAt > now)
            {
                return;
            }
            _semTimer.Wait();
            try
            {
                if (_timerDueAt > now)
                {
                    return;
                }
                if (!_slidingQueue.TryPeekNextEntryAt(out _timerDueAt))
                {
                    return;
                }
                var timeToNext = _timerDueAt.Subtract(now);
                if (_timer == null)
                {
                    _timer = new Timer(TimerCallback_DiscardUnused, null, timeToNext, Timeout.InfiniteTimeSpan);
                }
                else
                {
                    _timer.Change(timeToNext, Timeout.InfiniteTimeSpan);
                }
            }
            finally
            {
                _semTimer.Release();
            }
        }

        private void TimerCallback_DiscardUnused(object state)
        {
            while (_slidingQueue.TryDequeue(out var levels))
            {
                var minTimeStamp = DateTime.Now.Subtract(_slidingExpiration);
                foreach (var level in levels)
                {
                    level.RemoveUnused(minTimeStamp);
                }
            }
            SetupTimer();
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Threading
{
    public class Lock<T>
    {
        private readonly SemaphoreSlim _locksSemaphore;
        private readonly Dictionary<T, LockEntry> _locks;

        public Lock() : this(EqualityComparer<T>.Default) { }
        public Lock(IEqualityComparer<T> comparer)
        {
            _locks = new Dictionary<T, LockEntry>(comparer);
            _locksSemaphore = new SemaphoreSlim(1);
        }

        public async Task<LockHandle> EnterAsync(T value) => (await TryEnterAsync(value, Times.Infinite, CancellationToken.None))!;
        public async Task<LockHandle> EnterAsync(T value, CancellationToken cancellationToken) => (await TryEnterAsync(value, Times.Infinite, cancellationToken))!;
        public async Task<LockHandle?> TryEnterAsync(T value, TimeSpan timeout) => await TryEnterAsync(value, timeout, CancellationToken.None);
        public async Task<LockHandle?> TryEnterAsync(T value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            TimeSpan curTimeout;
            var sw = Stopwatch.StartNew();
            if (_locks.TryGetValue(value, out var entry)) {
                entry.Increment();
                if (entry.IsDisposing) {
                    entry = null;
                }
            }

            if (entry is null) {
                curTimeout = timeout == Times.Infinite ? timeout : timeout - sw.Elapsed;
                if (timeout != Times.Infinite && curTimeout < TimeSpan.Zero) return null;

                if (!await _locksSemaphore.WaitAsync(curTimeout, cancellationToken)) {
                    return null;
                }
                try {
                    if (_locks.TryGetValue(value, out entry)) {
                        entry.Increment();
                        try {
                            if (entry.IsDisposing) {
                                entry = new LockEntry(value);
                                _locks[value] = entry;
                            }
                        }
                        catch (Exception) {
                            entry.Exit();
                            throw;
                        }
                    }
                    else {
                        entry = new LockEntry(value);
                        _locks.Add(value, entry);
                    }
                }
                finally {
                    _locksSemaphore.Release();
                }
            }

            curTimeout = timeout == Times.Infinite ? timeout : timeout - sw.Elapsed;
            if (timeout != Times.Infinite && curTimeout < TimeSpan.Zero) {
                entry.Exit();
                await DisposeEntryAsync(entry);
                return null;
            }

            try {
                var handle = await entry.WaitAndEnterAsync(this, curTimeout, cancellationToken);
                if (handle is null) {
                    entry.Exit();
                    await DisposeEntryAsync(entry);
                }
                return handle;
            }
            catch (Exception) {
                entry.Exit();
                await DisposeEntryAsync(entry);
                throw;
            }
        }

        private async Task ReleaseAsync(LockHandle handle)
        {
            if (!_locks.TryGetValue(handle.Value, out var entry) || !entry.IsBoundTo(handle)) {
                throw new ArgumentException("The handle is not bound to this lock");
            }

            entry.Exit();
            entry.Semaphore.Release();
            await DisposeEntryAsync(entry);
        }

        private async Task DisposeEntryAsync(LockEntry entry)
        {
            if (!entry.IsDisposing) {
                return;
            }
            entry.Semaphore.Dispose();
            await _locksSemaphore.WaitAsync();
            try {
                if (_locks.TryGetValue(entry.Value, out var x) && ReferenceEquals(entry, x)) {
                    _locks.Remove(entry.Value);
                }
            }
            finally {
                _locksSemaphore.Release();
            }
        }

        private class LockEntry
        {
            private int count;

            public LockEntry(T value)
            {
                Semaphore = new SemaphoreSlim(1);
                Value = value;
                count = 1;
            }

            public SemaphoreSlim Semaphore { get; }
            private LockHandle? Handle { get; set; }
            public T Value { get; }

            public int Count => count;
            public bool IsDisposing => count == 0;

            public bool Increment()
            {
                int current;
                do {
                    current = Volatile.Read(ref count); // read current value
                    if (current <= 0)
                        return false; // don't increment if <= 0
                }
                while (Interlocked.CompareExchange(ref count, current + 1, current) != current);
                return true; // successfully incremented
            }

            public void Exit()
            {
                Interlocked.Decrement(ref count);
            }

            public async Task<LockHandle?> WaitAndEnterAsync(Lock<T> owner, TimeSpan timeout, CancellationToken cancellationToken)
            {
                if (!await Semaphore.WaitAsync(timeout, cancellationToken)) {
                    return null;
                }
                Handle = new LockHandle(owner, Value);
                return Handle;
            }

            public bool IsBoundTo(LockHandle? handle) => ReferenceEquals(Handle, handle);
        }

        public sealed class LockHandle
        {
            private readonly Lock<T> owner;
            private bool isReleased = false;

            public LockHandle(Lock<T> owner, T value)
            {
                this.owner = owner;
                Value = value;
            }

            public T Value { get; }

            public async Task ReleaseAsync()
            {
                if (isReleased) throw new InvalidOperationException("Handle is already released");
                isReleased = true;
                await owner.ReleaseAsync(this);
            }
        }
    }
}

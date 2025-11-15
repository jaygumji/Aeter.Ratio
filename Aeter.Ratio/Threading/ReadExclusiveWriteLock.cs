/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Threading
{
    /// <summary>
    /// Provides keyed reader/writer locking semantics: unlimited concurrent readers per key
    /// and a single exclusive writer that waits for existing readers to finish.
    /// </summary>
    /// <typeparam name="TKey">Lock key type.</typeparam>
    public sealed class ReadExclusiveWriteLock<TKey> : IDisposable
        where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, Entry> entries = new();
        private volatile bool disposed;

        public async Task<ReadHandle> EnterReadAsync(TKey key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);
            ThrowIfDisposed();

            var entry = RentEntry(key);
            try {
                var handle = await entry.Lock.EnterAsync(cancellationToken).ConfigureAwait(false);
                return new ReadHandle(this, key, entry, handle);
            }
            catch {
                ReleaseEntry(key, entry);
                throw;
            }
        }

        public async Task<WriteHandle> EnterWriteAsync(TKey key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);
            ThrowIfDisposed();

            var entry = RentEntry(key);
            try {
                var handle = await entry.Lock.EnterExclusiveAsync(cancellationToken).ConfigureAwait(false);
                return new WriteHandle(this, key, entry, handle);
            }
            catch {
                ReleaseEntry(key, entry);
                throw;
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var (_, entry) in entries) {
                entry.Lock.Dispose();
            }
            entries.Clear();
        }

        private Entry RentEntry(TKey key)
        {
            while (true) {
                ThrowIfDisposed();
                var entry = entries.GetOrAdd(key, static _ => new Entry());
                Interlocked.Increment(ref entry.RefCount);
                if (!disposed) {
                    return entry;
                }
                ReleaseEntry(key, entry);
            }
        }

        private void ReleaseEntry(TKey key, Entry entry)
        {
            if (Interlocked.Decrement(ref entry.RefCount) == 0) {
                if (entries.TryRemove(new KeyValuePair<TKey, Entry>(key, entry))) {
                    entry.Lock.Dispose();
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed) {
                throw new ObjectDisposedException(nameof(ReadExclusiveWriteLock<TKey>));
            }
        }

        internal sealed class Entry
        {
            public ExclusiveLock Lock { get; } = new ExclusiveLock();
            public int RefCount;
        }

        public struct ReadHandle : IAsyncDisposable, IDisposable
        {
            private readonly ReadExclusiveWriteLock<TKey> owner;
            private readonly TKey key;
            private readonly Entry entry;
            private readonly ExclusiveLock.LockHandle handle;
            private int released;

            internal ReadHandle(ReadExclusiveWriteLock<TKey> owner, TKey key, Entry entry, ExclusiveLock.LockHandle handle)
            {
                this.owner = owner;
                this.key = key;
                this.entry = entry;
                this.handle = handle;
                released = 0;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref released, 1) == 1) return;
                handle.ReleaseAsync().GetAwaiter().GetResult();
                owner.ReleaseEntry(key, entry);
            }

            public async ValueTask DisposeAsync()
            {
                if (Interlocked.Exchange(ref released, 1) == 1) return;
                await handle.ReleaseAsync().ConfigureAwait(false);
                owner.ReleaseEntry(key, entry);
            }
        }

        public struct WriteHandle : IAsyncDisposable, IDisposable
        {
            private readonly ReadExclusiveWriteLock<TKey> owner;
            private readonly TKey key;
            private readonly Entry entry;
            private readonly ExclusiveLock.ExclusiveLockHandle handle;
            private int released;

            internal WriteHandle(ReadExclusiveWriteLock<TKey> owner, TKey key, Entry entry, ExclusiveLock.ExclusiveLockHandle handle)
            {
                this.owner = owner;
                this.key = key;
                this.entry = entry;
                this.handle = handle;
                released = 0;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref released, 1) == 1) return;
                handle.ReleaseAsync().GetAwaiter().GetResult();
                owner.ReleaseEntry(key, entry);
            }

            public async ValueTask DisposeAsync()
            {
                if (Interlocked.Exchange(ref released, 1) == 1) return;
                await handle.ReleaseAsync().ConfigureAwait(false);
                owner.ReleaseEntry(key, entry);
            }
        }
    }
}

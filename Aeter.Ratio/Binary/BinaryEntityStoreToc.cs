using Aeter.Ratio.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary
{
    public sealed class BinaryEntityStoreToc : IDisposable
    {
        private readonly BinaryStore store;
        private readonly Dictionary<Guid, BinaryEntityStoreTocEntry> entries;
        private readonly object syncRoot;

        private BinaryEntityStoreToc(BinaryStore store, Dictionary<Guid, BinaryEntityStoreTocEntry> entries)
        {
            this.store = store;
            this.entries = entries;
            syncRoot = entries;
        }

        public BinaryEntityStoreTocEntry Header {
            get {
                lock (syncRoot) {
                    if (entries.TryGetValue(Guid.Empty, out var entry)) {
                        return entry;
                    }
                }
                throw BinaryEntityStoreEngineInitializationException.TocFailed("Missing header entry");
            }
        }

        public static async Task<(BinaryEntityStoreToc Toc, ScheduledTaskHandle Handle)> CreateAsync(string path, BinaryBufferPool bufferPool, BinaryEntityStore entityStore, Scheduler scheduler, CancellationToken cancellationToken = default)
        {
            var store = new BinaryStore(path, bufferPool);
            if (store.Size == 0) {
                if (entityStore.Size == 0) {
                    var header = new BinaryEntityStoreHeader();
                    using var buffer = await entityStore.WriteAsync(0, header.Size, Guid.Empty, header.Version, cancellationToken);
                    await header.WriteToAsync(buffer, cancellationToken);
                }
                var entries = new Dictionary<Guid, BinaryEntityStoreTocEntry>();
                var initHandle = scheduler.Schedule(async state => {
                    ArgumentNullException.ThrowIfNull(state);
                    var (map, entityStore) = ((Dictionary<Guid, BinaryEntityStoreTocEntry>, BinaryEntityStore))state;

                    await entityStore.ReadAllAsync(a => {
                        var entry = new BinaryEntityStoreTocEntry(a.Offset, a.Header.Size, isFree: !a.Header.IsInUse, a.Header.Metadata.Key);
                        lock (map) {
                            map[entry.Key] = entry;
                        }
                        return Task.CompletedTask;
                    }, cancellationToken: cancellationToken);
                }, (entries, entityStore), cancellationToken);

                return (new BinaryEntityStoreToc(store, entries), initHandle);
            }
            else {
                var entries = new Dictionary<Guid, BinaryEntityStoreTocEntry>();
                var initHandle = scheduler.Schedule(async state => {
                    ArgumentNullException.ThrowIfNull(state);
                    var (map, tocStore) = ((Dictionary<Guid, BinaryEntityStoreTocEntry>, BinaryStore))state;

                    await tocStore.ReadAllAsync(async a => {
                        var keyBytes = await a.Buffer.ReadAsync(16, cancellationToken).ConfigureAwait(false);
                        var key = new Guid(keyBytes.Span);
                        var entry = new BinaryEntityStoreTocEntry(a.Offset, a.Size, a.IsFree, key);
                        lock (map) {
                            map[key] = entry;
                        }
                    }, cancellationToken: cancellationToken);
                }, (entries, store), cancellationToken);
                return (new BinaryEntityStoreToc(store, entries), initHandle);
            }
        }

        public bool TryGetEntry(Guid id, [MaybeNullWhen(false)] out BinaryEntityStoreTocEntry entry)
        {
            lock (syncRoot) {
                return entries.TryGetValue(id, out entry);
            }
        }

        internal void Upsert(Guid key, long offset, int size, bool isFree)
        {
            var entry = new BinaryEntityStoreTocEntry(offset, size, isFree, key);
            lock (syncRoot) {
                entries[key] = entry;
            }
        }

        internal bool Remove(Guid key)
        {
            lock (syncRoot) {
                return entries.Remove(key);
            }
        }

        public void Dispose()
        {
            store.Dispose();
        }
    }
}

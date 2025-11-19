using Aeter.Ratio.Binary.Linq;
using Aeter.Ratio.IO;
using Aeter.Ratio.Scheduling;
using Aeter.Ratio.Serialization;
using Aeter.Ratio.Serialization.Bson;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary.EntityStore
{
    public class BinaryEntityStoreEngine : IDisposable, IAsyncDisposable
    {
        private BinaryEntityStoreHeader? header;
        private IEntitySerializer? serializer;
        private readonly BinaryEntityStoreFileSystem fileSystem;
        private readonly BinaryBufferPool bufferPool;
        private readonly BinaryEntityStoreLockManager lockManager;
        private readonly BinaryEntityStore store;
        private readonly Scheduler scheduler;
        private readonly BinaryEntityStoreToc toc;
        private readonly Task initializationTask;
        private readonly EntityEngineEventsManager events = new();
        private bool disposed;
        private BinaryEntityStoreIndexEngine? indexEngine;
        private BinaryEntityStoreQueryProvider? queryProvider;

        public BinaryEntityStoreEngine(BinaryEntityStoreFileSystem fileSystem, BinaryEntityStore store, BinaryBufferPool bufferPool, Scheduler scheduler, BinaryEntityStoreToc toc, BinaryEntityStoreLockManager lockManager, ScheduledTaskHandle initHandle)
        {
            this.fileSystem = fileSystem;
            this.store = store;
            this.scheduler = scheduler;
            this.toc = toc;
            this.bufferPool = bufferPool;
            this.lockManager = lockManager;
            initializationTask = InitializeAsync(initHandle, bufferPool);
        }

        public static async Task<BinaryEntityStoreEngine> CreateAsync(string path, BinaryBufferPool bufferPool, CancellationToken cancellationToken = default)
        {
            var fileSystem = new BinaryEntityStoreFileSystem(path);
            var store = new BinaryEntityStore(fileSystem.Store.Path, bufferPool);
            var scheduler = new Scheduler();
            var (toc, initHandle) = await BinaryEntityStoreToc.CreateAsync(fileSystem.TableOfContent.Path, bufferPool, store, scheduler, cancellationToken);
            var lockManager = new BinaryEntityStoreLockManager();

            return new BinaryEntityStoreEngine(fileSystem, store, bufferPool, scheduler, toc, lockManager, initHandle);
        }

        public async Task<Guid> AddAsync(object entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var id = Guid.NewGuid();
            await AddAsync(id, entity, cancellationToken).ConfigureAwait(false);
            return id;
        }

        public async Task AddAsync(Guid id, object entity, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentNullException(nameof(id));
            ArgumentNullException.ThrowIfNull(entity);
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            await using var writeHandle = await lockManager.EnterEntityWriteAsync(id, cancellationToken).ConfigureAwait(false);
            if (TryGetActiveEntry(id, out _)) {
                throw new InvalidOperationException($"Entity with id '{id}' already exists in the store.");
            }
            await WriteEntityAsync(id, entity, EntityChangeType.Added, cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateAsync(Guid id, object entity, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentNullException(nameof(id));
            ArgumentNullException.ThrowIfNull(entity);
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            await using var writeHandle = await lockManager.EnterEntityWriteAsync(id, cancellationToken).ConfigureAwait(false);
            if (!TryGetActiveEntry(id, out var existing)) {
                throw new InvalidOperationException($"Entity with id '{id}' does not exist.");
            }

            await store.MarkAsNotUsedAsync(existing.Offset, cancellationToken).ConfigureAwait(false);
            toc.Remove(id);
            await WriteEntityAsync(id, entity, EntityChangeType.Updated, cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty) throw new ArgumentNullException(nameof(id));
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            await using var writeHandle = await lockManager.EnterEntityWriteAsync(id, cancellationToken).ConfigureAwait(false);
            if (!TryGetActiveEntry(id, out var existing)) {
                return;
            }

            await store.MarkAsNotUsedAsync(existing.Offset, cancellationToken).ConfigureAwait(false);
            toc.Remove(id);
            await events.RaiseEntityChangedAsync(new EntityEngineEventsChangedArgs(id, null, ReadOnlyMemory<byte>.Empty, EntityChangeType.Deleted));
        }

        public async Task<T?> GetAsync<T>(Guid id, CancellationToken cancellationToken = default)
            where T : class
            => (T?)await GetAsync(id, typeof(T), cancellationToken);

        public async Task<object?> GetAsync(Guid id, Type type, CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            await using var readHandle = await lockManager.EnterEntityReadAsync(id, cancellationToken).ConfigureAwait(false);
            if (TryGetActiveEntry(id, out var entry)) {
                using var record = await store.ReadAsync(entry.Offset, cancellationToken).ConfigureAwait(false);
                return serializer!.Deserialize(type, record.Buffer);
            }
            return default;
        }

        public IQueryable<T> Query<T>(CancellationToken cancellationToken = default)
        {
            EnsureInitializedAsync(cancellationToken).GetAwaiter().GetResult();
            return new BinaryEntityStoreQueryable<T>(EnsureQueryProvider());
        }

        public IQueryable Query(Type entityType, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entityType);
            EnsureInitializedAsync(cancellationToken).GetAwaiter().GetResult();
            return EnsureQueryProvider().CreateQuery(entityType);
        }

        public void Dispose()
        {
            DisposeAsyncCore().GetAwaiter().GetResult();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsyncCore()
        {
            if (disposed) {
                return;
            }
            disposed = true;
            try {
                await initializationTask.ConfigureAwait(false);
            }
            catch {
                // Ignore initialization failures during dispose.
            }

            store.Dispose();
            indexEngine?.Dispose();
            toc.Dispose();
            lockManager.Dispose();
            await scheduler.DisposeAsync().ConfigureAwait(false);
        }

        private async Task InitializeAsync(ScheduledTaskHandle initHandle, BinaryBufferPool bufferPool)
        {
            try {
                await initHandle.Completion.ConfigureAwait(false);
                using var entry = await store.ReadAsync(toc.Header.Offset).ConfigureAwait(false);
                header = await BinaryEntityStoreHeader.ReadFromAsync(entry.Buffer).ConfigureAwait(false) ?? throw BinaryEntityStoreEngineInitializationException.HeaderFailed();
                if (header.SerializerType == BsonSerializer.ARID) {
                    serializer = new BsonSerializer(bufferPool);
                }
                else {
                    throw new ArgumentOutOfRangeException(nameof(header.SerializerType), header.SerializerType, null);
                }
                var (IndexEngine, InitHandle) = await BinaryEntityStoreIndexEngine.CreateAsync(fileSystem, store, events, lockManager, toc, scheduler, serializer);
                indexEngine = IndexEngine;
                await InitHandle.Completion.ConfigureAwait(false);
            }
            finally {
                await initHandle.DisposeAsync().ConfigureAwait(false);
            }
        }

        private Task EnsureInitializedAsync(CancellationToken cancellationToken)
            => initializationTask.WaitAsync(cancellationToken);

        private BinaryEntityStoreQueryProvider EnsureQueryProvider()
        {
            var serializerInstance = serializer ?? throw new InvalidOperationException("Binary entity store engine has not been initialized yet.");
            return queryProvider ??= new BinaryEntityStoreQueryProvider(new BinaryEntityStoreQueryContext(store, toc, lockManager, serializerInstance, indexEngine));
        }

        private bool TryGetActiveEntry(Guid id, [MaybeNullWhen(false)] out BinaryEntityStoreTocEntry entry)
        {
            if (toc.TryGetEntry(id, out entry) && entry is not null && !entry.IsFree) {
                return true;
            }
            entry = null;
            return false;
        }

        private async Task WriteEntityAsync(Guid id, object entity, EntityChangeType changeType, CancellationToken cancellationToken)
        {
            var payload = SerializeEntity(entity);
            var metadata = new BinaryEntityStoreRecordMetadata(id, header!.Version);
            long offset;
            await using (var appendHandle = await lockManager.EnterAppendAsync(cancellationToken).ConfigureAwait(false)) {
                offset = store.Size;
                using (var buffer = await store.WriteAsync(offset, payload.Length, metadata, cancellationToken).ConfigureAwait(false)) {
                    await buffer.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
                }
            }
            using var written = await store.ReadAsync(offset, cancellationToken).ConfigureAwait(false);
            toc.Upsert(id, offset, written.Header.Size, isFree: false);
            await events.RaiseEntityChangedAsync(new EntityEngineEventsChangedArgs(id, entity, payload, changeType));
        }

        private byte[] SerializeEntity(object entity)
        {
            var serializerInstance = serializer ?? throw new InvalidOperationException("Binary entity store engine has not been initialized yet.");
            using var memoryStream = new MemoryStream();
            using var binaryStream = BinaryStream.MemoryStream(memoryStream);
            using (var buffer = bufferPool.AcquireWriteBuffer(binaryStream)) {
                serializerInstance.Serialize(buffer, entity);
            }
            return memoryStream.ToArray();
        }

        public async Task ShrinkAsync(CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            await using (var appendHandle = await lockManager.EnterAppendAsync(cancellationToken).ConfigureAwait(false)) {
                var entries = toc.SnapshotEntries();
                if (entries.Length == 0) {
                    return;
                }

                Array.Sort(entries, static (x, y) => x.Offset.CompareTo(y.Offset));

                var header = toc.Header;
                var nextOffset = header.Offset + header.Size;

                foreach (var entry in entries) {
                    if (entry.Key == Guid.Empty) {
                        nextOffset = entry.Offset + entry.Size;
                        continue;
                    }

                    if (entry.IsFree) {
                        continue;
                    }

                    if (entry.Offset == nextOffset) {
                        nextOffset += entry.Size;
                        continue;
                    }

                    await using var writeHandle = await lockManager.EnterEntityWriteAsync(entry.Key, cancellationToken).ConfigureAwait(false);
                    using var record = await store.ReadAsync(entry.Offset, cancellationToken).ConfigureAwait(false);
                    var payloadLength = record.Header.PayloadLength;

                    using (var buffer = await store.WriteAsync(nextOffset, payloadLength, record.Header.Metadata, cancellationToken).ConfigureAwait(false)) {
                        await record.Buffer.CopyToAsync(buffer, payloadLength, cancellationToken).ConfigureAwait(false);
                    }

                    await store.MarkAsNotUsedAsync(entry.Offset, entry.Size, record.Header.Metadata, cancellationToken).ConfigureAwait(false);
                    toc.Upsert(entry.Key, nextOffset, entry.Size, isFree: false);
                    nextOffset += entry.Size;
                }
            }
        }
    }
}

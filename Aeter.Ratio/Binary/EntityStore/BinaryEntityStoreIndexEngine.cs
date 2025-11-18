/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.IO;
using Aeter.Ratio.Scheduling;
using Aeter.Ratio.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary.EntityStore
{
    /// <summary>
    /// Coordinates creation, initialization, and querying of binary entity store indexes.
    /// </summary>
    public sealed class BinaryEntityStoreIndexEngine : IDisposable
    {
        private readonly BinaryEntityStoreFileSystem fileSystem;
        private readonly BinaryEntityStore entityStore;
        private readonly EntityEngineEvents events;
        private readonly BinaryEntityStoreLockManager lockManager;
        private readonly BinaryEntityStoreToc toc;
        private readonly IEntitySerializer entitySerializer;
        private readonly Dictionary<string, BinaryEntityStoreIndex> indexes = new(StringComparer.OrdinalIgnoreCase);
        private readonly AsyncEventDelegate<EntityEngineEventsChangedArgs> entityChangedHandler;
        private readonly object syncRoot = new();
        private Task initializationTask = Task.CompletedTask;
        private bool disposed;

        private BinaryEntityStoreIndexEngine(BinaryEntityStoreFileSystem fileSystem,
            BinaryEntityStore entityStore,
            EntityEngineEvents events,
            BinaryEntityStoreLockManager lockManager,
            BinaryEntityStoreToc toc,
            IEntitySerializer entitySerializer)
        {
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this.entityStore = entityStore ?? throw new ArgumentNullException(nameof(entityStore));
            this.events = events ?? throw new ArgumentNullException(nameof(events));
            this.lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));
            this.toc = toc ?? throw new ArgumentNullException(nameof(toc));
            this.entitySerializer = entitySerializer ?? throw new ArgumentNullException(nameof(entitySerializer));

            entityChangedHandler = HandleEntityChangedAsync;
            this.events.Register(entityChangedHandler);
        }

        /// <summary>
        /// Creates an index engine and schedules the asynchronous initialization of every on-disk index.
        /// </summary>
        /// <param name="fileSystem">Index file system abstraction.</param>
        /// <param name="entityStore">Backing entity store for reading payloads.</param>
        /// <param name="events">Event source used to observe entity mutations.</param>
        /// <param name="lockManager">Lock manager guarding entity reads during index population.</param>
        /// <param name="toc">Table of contents used to iterate persisted entities.</param>
        /// <param name="scheduler">Scheduler that runs the initialization task.</param>
        /// <param name="entitySerializer">Serializer capable of materializing indexed values.</param>
        /// <returns>
        /// Tuple containing the created engine and the scheduled initialization handle which callers can await before issuing queries.
        /// </returns>
        public static Task<(BinaryEntityStoreIndexEngine IndexEngine, ScheduledTaskHandle InitHandle)> CreateAsync(BinaryEntityStoreFileSystem fileSystem,
            BinaryEntityStore entityStore, EntityEngineEvents events, BinaryEntityStoreLockManager lockManager, BinaryEntityStoreToc toc, Scheduler scheduler, IEntitySerializer entitySerializer)
        {
            var engine = new BinaryEntityStoreIndexEngine(fileSystem, entityStore, events, lockManager, toc, entitySerializer);
            var handle = scheduler.Schedule(engine.InitializeAsync, state: null, CancellationToken.None);
            engine.initializationTask = handle.Completion;
            return Task.FromResult((engine, handle));
        }

        /// <summary>
        /// Gets the paths of the indexes that were discovered on disk.
        /// </summary>
        public IReadOnlyList<string> Paths {
            get {
                lock (syncRoot) {
                    return indexes.Keys.ToArray();
                }
            }
        }

        /// <summary>
        /// Waits until the background initialization (discovery + population) of every index completed.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the wait.</param>
        public Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
            => initializationTask.WaitAsync(cancellationToken);

        /// <summary>
        /// Executes an equality search against an index that advertises the <see cref="BinaryEntityStoreIndexCapabilities.BinarySearch"/> capability.
        /// Prefer calling this method before <see cref="FullTextSearchAsync"/> whenever you know the exact value you are matching
        /// (even for strings), because the lookup is deterministic and does not incur fuzzy matching overhead.
        /// </summary>
        /// <param name="path">Index path inside the entity.</param>
        /// <param name="value">Value to match.</param>
        /// <param name="cancellationToken">Token used to cancel the query.</param>
        /// <returns>Entity identifiers ordered by the index.</returns>
        public async Task<IReadOnlyList<Guid>> BinarySearchAsync(string path, object value, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentNullException.ThrowIfNull(value);

            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            if (!TryGetIndex(path, out var index) || !index.SupportsBinarySearch) {
                return Array.Empty<Guid>();
            }

            return index.Seek(value);
        }

        /// <summary>
        /// Runs a fuzzy full-text search against a string index that advertises the <see cref="BinaryEntityStoreIndexCapabilities.FullText"/> capability.
        /// Use this when <see cref="BinarySearchAsync"/> returned no results or when you intentionally need tolerant matching on textual data.
        /// </summary>
        /// <param name="path">Index path inside the entity.</param>
        /// <param name="query">Text query.</param>
        /// <param name="tolerance">Maximum tolerated edit distance before a result is discarded.</param>
        /// <param name="cancellationToken">Token used to cancel the query.</param>
        /// <returns>Matching entity identifiers, or an empty list if the path does not support full-text indexing.</returns>
        public async Task<IReadOnlyList<Guid>> FullTextSearchAsync(string path, string query, int tolerance = 1, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            ArgumentNullException.ThrowIfNull(query);

            await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            if (!TryGetIndex(path, out var index) || !index.SupportsFullText) {
                return Array.Empty<Guid>();
            }

            return index.FullTextSearch(query, tolerance);
        }

        /// <summary>
        /// Releases unmanaged resources and unsubscribes from entity change notifications.
        /// </summary>
        public void Dispose()
        {
            if (disposed) {
                return;
            }

            disposed = true;
            events.Unregister(entityChangedHandler);
            BinaryEntityStoreIndex[] snapshot;
            lock (syncRoot) {
                snapshot = indexes.Values.ToArray();
                indexes.Clear();
            }
            foreach (var index in snapshot) {
                index.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        private async Task InitializeAsync(object? state, CancellationToken cancellationToken)
        {
            foreach (var indexFile in fileSystem.Indexes) {
                var index = new BinaryEntityStoreIndex(indexFile);
                await index.InitializeAsync(cancellationToken).ConfigureAwait(false);

                if (!index.Metadata.Capabilities.HasFlag(BinaryEntityStoreIndexCapabilities.BinarySearch) &&
                    !index.Metadata.Capabilities.HasFlag(BinaryEntityStoreIndexCapabilities.FullText)) {
                    continue;
                }

                lock (syncRoot) {
                    indexes[index.Path] = index;
                }

                if (!index.HasValues) {
                    await PopulateIndexAsync(index, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task HandleEntityChangedAsync(object sender, EntityEngineEventsChangedArgs args)
        {
            await EnsureInitializedAsync(CancellationToken.None).ConfigureAwait(false);
            var snapshot = SnapshotIndexes();
            if (snapshot.Length == 0) {
                return;
            }

            switch (args.ChangeType) {
                case EntityChangeType.Deleted:
                    foreach (var index in snapshot) {
                        await index.RemoveAsync(args.EntityId, CancellationToken.None).ConfigureAwait(false);
                    }
                    break;

                default:
                    if (args.Payload.IsEmpty) {
                        return;
                    }

                    foreach (var index in snapshot) {
                        using var payload = new EntityPayloadReader(args.Payload);
                        var values = entitySerializer.ReadValue(payload.Buffer, index.Path, index.ValueType);
                        if (values.Count == 0) {
                            await index.RemoveAsync(args.EntityId, CancellationToken.None).ConfigureAwait(false);
                            continue;
                        }

                        await index.UpsertAsync(args.EntityId, values, CancellationToken.None).ConfigureAwait(false);
                    }
                    break;
            }
        }

        private BinaryEntityStoreIndex[] SnapshotIndexes()
        {
            lock (syncRoot) {
                return indexes.Values.ToArray();
            }
        }

        private bool TryGetIndex(string path, out BinaryEntityStoreIndex index)
        {
            lock (syncRoot) {
                return indexes.TryGetValue(path, out index!);
            }
        }

        private async Task PopulateIndexAsync(BinaryEntityStoreIndex index, CancellationToken cancellationToken)
        {
            var snapshot = toc.SnapshotEntries();
            foreach (var entry in snapshot) {
                if (entry.Key == Guid.Empty || entry.IsFree) {
                    continue;
                }

                await using var readHandle = await lockManager.EnterEntityReadAsync(entry.Key, cancellationToken).ConfigureAwait(false);
                using var record = await entityStore.ReadAsync(entry.Offset, cancellationToken).ConfigureAwait(false);
                var values = entitySerializer.ReadValue(record.Buffer, index.Path, index.ValueType);
                if (values.Count == 0) {
                    continue;
                }

                await index.UpsertAsync(entry.Key, values, cancellationToken).ConfigureAwait(false);
            }
        }

        private readonly struct EntityPayloadReader : IDisposable
        {
            private readonly IBinaryWriteStream stream;
            public BinaryReadBuffer Buffer { get; }

            public EntityPayloadReader(ReadOnlyMemory<byte> payload)
            {
                if (!MemoryMarshal.TryGetArray(payload, out var segment)) {
                    segment = new ArraySegment<byte>(payload.ToArray());
                }

                var memoryStream = new MemoryStream(segment.Array!, segment.Offset, segment.Count, writable: false, publiclyVisible: true);
                stream = BinaryStream.MemoryStream(memoryStream);
                var size = Math.Max(segment.Count, 1024);
                Buffer = new BinaryReadBuffer(size, stream, 0, segment.Count);
            }

            public void Dispose()
            {
                Buffer.Dispose();
                stream.Dispose();
            }
        }
    }
}

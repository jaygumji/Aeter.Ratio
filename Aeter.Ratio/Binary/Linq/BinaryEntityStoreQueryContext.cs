/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary.EntityStore;
using Aeter.Ratio.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aeter.Ratio.Binary.Linq
{
    /// <summary>
    /// Coordinates how LINQ queries retrieve entities from the binary store.
    /// Decides whether to use indexes, iterate the file sequentially, or materialize the dataset in memory.
    /// </summary>
    internal sealed class BinaryEntityStoreQueryContext
    {
        private const int DefaultInMemoryThreshold = 256;

        private readonly BinaryEntityStore store;
        private readonly BinaryEntityStoreToc toc;
        private readonly BinaryEntityStoreLockManager lockManager;
        private readonly IEntitySerializer serializer;
        private readonly BinaryEntityStoreIndexEngine? indexEngine;
        private readonly int inMemoryThreshold;

        public BinaryEntityStoreQueryContext(BinaryEntityStore store,
            BinaryEntityStoreToc toc,
            BinaryEntityStoreLockManager lockManager,
            IEntitySerializer serializer,
            BinaryEntityStoreIndexEngine? indexEngine,
            int inMemoryThreshold = DefaultInMemoryThreshold)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.toc = toc ?? throw new ArgumentNullException(nameof(toc));
            this.lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.indexEngine = indexEngine;
            this.inMemoryThreshold = inMemoryThreshold > 0 ? inMemoryThreshold : DefaultInMemoryThreshold;
        }

        /// <summary>
        /// Builds the IEnumerable that will back a query for the specified entity type.
        /// </summary>
        internal IEnumerable<T> CreateEnumerableCore<T>(BinaryEntityStoreQuerySpecification specification)
        {
            ArgumentNullException.ThrowIfNull(specification);

            var predicate = specification.CreatePredicate<T>();
            var indexedMatches = TryResolveIndexedMatches(specification);
            if (indexedMatches is not null) {
                if (indexedMatches.Count == 0) {
                    return Enumerable.Empty<T>();
                }
                return EnumerateIndexed(indexedMatches, predicate);
            }

            var activeEntries = FilterActiveEntries(toc.SnapshotEntries());
            if (activeEntries.Length == 0) {
                return Enumerable.Empty<T>();
            }

            if (activeEntries.Length <= inMemoryThreshold) {
                return Materialize(activeEntries, predicate);
            }

            return Enumerate(activeEntries, predicate);
        }

        private IReadOnlyList<Guid>? TryResolveIndexedMatches(BinaryEntityStoreQuerySpecification specification)
        {
            if (indexEngine is null || specification.IndexFilters.Count == 0) {
                return null;
            }

            var availablePaths = indexEngine.Paths;
            foreach (var filter in specification.IndexFilters) {
                if (!ContainsPath(availablePaths, filter.Path)) {
                    continue;
                }

                return indexEngine.BinarySearchAsync(filter.Path, filter.Value).GetAwaiter().GetResult();
            }

            return null;
        }

        private static bool ContainsPath(IReadOnlyList<string> availablePaths, string path)
        {
            if (availablePaths.Count == 0) {
                return false;
            }

            foreach (var candidate in availablePaths) {
                if (string.Equals(candidate, path, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }

        private static BinaryEntityStoreTocEntry[] FilterActiveEntries(BinaryEntityStoreTocEntry[] entries)
        {
            if (entries.Length == 0) {
                return Array.Empty<BinaryEntityStoreTocEntry>();
            }

            var buffer = new List<BinaryEntityStoreTocEntry>(entries.Length);
            foreach (var entry in entries) {
                if (entry.IsFree || entry.Key == Guid.Empty) {
                    continue;
                }
                buffer.Add(entry);
            }
            return buffer.ToArray();
        }

        private IEnumerable<T> EnumerateIndexed<T>(IReadOnlyList<Guid> matches, Func<T, bool>? predicate)
        {
            if (matches.Count == 0) {
                return Enumerable.Empty<T>();
            }

            return EnumerateIndexedCore(matches, predicate);
        }

        private IEnumerable<T> EnumerateIndexedCore<T>(IReadOnlyList<Guid> matches, Func<T, bool>? predicate)
        {
            var seen = new HashSet<Guid>();
            foreach (var id in matches) {
                if (!seen.Add(id)) {
                    continue;
                }

                if (!toc.TryGetEntry(id, out var entry) || entry is null || entry.IsFree) {
                    continue;
                }

                var entity = TryReadEntity<T>(entry);
                if (entity is null) {
                    continue;
                }

                if (predicate is null || predicate(entity)) {
                    yield return entity;
                }
            }
        }

        private IEnumerable<T> Materialize<T>(IReadOnlyList<BinaryEntityStoreTocEntry> entries, Func<T, bool>? predicate)
        {
            var results = new List<T>();
            foreach (var entry in entries) {
                var entity = TryReadEntity<T>(entry);
                if (entity is null) {
                    continue;
                }

                if (predicate is null || predicate(entity)) {
                    results.Add(entity);
                }
            }
            return results;
        }

        private IEnumerable<T> Enumerate<T>(IReadOnlyList<BinaryEntityStoreTocEntry> entries, Func<T, bool>? predicate)
        {
            foreach (var entry in entries) {
                var entity = TryReadEntity<T>(entry);
                if (entity is null) {
                    continue;
                }

                if (predicate is null || predicate(entity)) {
                    yield return entity;
                }
            }
        }

        private T? TryReadEntity<T>(BinaryEntityStoreTocEntry entry)
        {
            if (entry.IsFree) {
                return default;
            }

            using var readHandle = lockManager.EnterEntityReadAsync(entry.Key).GetAwaiter().GetResult();
            using var record = store.ReadAsync(entry.Offset).GetAwaiter().GetResult();
            return serializer.Deserialize<T>(record.Buffer);
        }
    }
}

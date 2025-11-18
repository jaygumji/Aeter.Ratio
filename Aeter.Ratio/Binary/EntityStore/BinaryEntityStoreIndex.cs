/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio;
using Aeter.Ratio.Binary;
using Aeter.Ratio.Binary.Algorithm;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Binary.EntityStore
{
    /// <summary>
    /// Represents the on-disk ordered index corresponding to a specific entity path in the binary store.
    /// Handles persistence (mutation log) and offers binary and full-text lookups when enabled.
    /// </summary>
    internal sealed class BinaryEntityStoreIndex : IDisposable
    {
        private const int BinaryStoreHeaderLength = 5;

        private readonly BinaryStore store;
        private readonly SemaphoreSlim gate = new(1, 1);
        private readonly List<BinaryEntityStoreIndexValue> entries = new();
        private readonly Dictionary<Guid, List<EntityValueReference>> entityValues = new();
        private readonly Dictionary<string, HashSet<Guid>> stringOwners = new(StringComparer.Ordinal);
        private readonly HashSet<string> stringCatalog = new(StringComparer.Ordinal);
        private readonly IndexValueComparer entryComparer;
        private readonly ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;

        private StringStorage? stringStorage;
        private FuzzyStringSearcher? stringSearcher;
        private BinaryEntityStoreIndexMetadata? metadata;
        private IComparer<object?>? valueComparer;
        private bool disposed;

        /// <summary>
        /// Creates a new index wrapper around the specified on-disk file.
        /// </summary>
        /// <param name="file">Binary store file containing the serialized index entries.</param>
        public BinaryEntityStoreIndex(BinaryEntityStoreFile file)
        {
            ArgumentNullException.ThrowIfNull(file);
            store = new BinaryStore(file.Path, BinaryBufferPool.Default);
            entryComparer = new IndexValueComparer(this);
        }

        /// <summary>
        /// Gets the metadata describing this index. Requires <see cref="InitializeAsync"/> to run first.
        /// </summary>
        public BinaryEntityStoreIndexMetadata Metadata => metadata ?? throw new InvalidOperationException("Index metadata has not been initialized.");
        /// <summary>
        /// Gets the entity path (ex: "Address.ZipCode") that was indexed.
        /// </summary>
        public string Path => Metadata.Path;
        /// <summary>
        /// Gets the .NET type stored by each entry inside this index.
        /// </summary>
        public Type ValueType => Metadata.ValueType;
        /// <summary>
        /// Gets the set of capabilities that the stored values expose.
        /// </summary>
        public BinaryEntityStoreIndexCapabilities Capabilities => Metadata.Capabilities;
        /// <summary>
        /// Gets a value indicating whether ordered equality searches are available.
        /// </summary>
        public bool SupportsBinarySearch => Capabilities.HasFlag(BinaryEntityStoreIndexCapabilities.BinarySearch);
        /// <summary>
        /// Gets a value indicating whether string fuzzy searches are available.
        /// </summary>
        public bool SupportsFullText => Capabilities.HasFlag(BinaryEntityStoreIndexCapabilities.FullText) && ValueType == typeof(string);
        /// <summary>
        /// Gets a value indicating whether this index already contains materialized values.
        /// </summary>
        internal bool HasValues {
            get {
                lock (entries) {
                    return entries.Count > 0;
                }
            }
        }

        /// <summary>
        /// Reads every record from the backing file, reconstructing metadata and in-memory state.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel initialization.</param>
        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await store.ReadAllAsync(async args =>
            {
                var payloadLength = args.Size - BinaryStoreHeaderLength;
                if (payloadLength <= 0) {
                    return;
                }

                if (args.IsFree) {
                    await args.Buffer.SkipAsync(payloadLength, cancellationToken).ConfigureAwait(false);
                    return;
                }

                var payload = await args.Buffer.ReadAsync(payloadLength, cancellationToken).ConfigureAwait(false);
                var bufferLength = Math.Max(payload.Length, 1);
                var rented = arrayPool.Rent(bufferLength);
                try {
                    payload.Span.CopyTo(rented);
                    using var stream = new MemoryStream(rented, 0, payload.Length, writable: false, publiclyVisible: true);
                    using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);
                    var entryKind = (BinaryEntityStoreIndexEntryKind)reader.ReadByte();
                    switch (entryKind) {
                        case BinaryEntityStoreIndexEntryKind.Metadata:
                            metadata = ReadMetadata(reader);
                            valueComparer = CreateValueComparer(ValueType);
                            EnsureFullTextInfrastructure();
                            break;

                        case BinaryEntityStoreIndexEntryKind.Mutation:
                            EnsureMetadata();
                            var mutation = ReadMutation(reader);
                            ApplyMutation(mutation);
                            break;
                    }
                }
                finally {
                    arrayPool.Return(rented);
                }
            }, cancellationToken: cancellationToken).ConfigureAwait(false);

            EnsureMetadata();
        }

        /// <summary>
        /// Performs an equality lookup against the ordered entries of this index.
        /// Prefer this over <see cref="FullTextSearch(string, int)"/> whenever the caller can supply the exact typed value.
        /// </summary>
        /// <param name="value">Value to match.</param>
        /// <returns>Collection of matching entity ids sorted by the order of the index.</returns>
        public IReadOnlyList<Guid> Seek(object value)
        {
            if (!SupportsBinarySearch) {
                return Array.Empty<Guid>();
            }

            EnsureMetadata();
            var typedValue = ValueConverter.ChangeType(value, ValueType);
            if (typedValue is null && ValueType.IsValueType && Nullable.GetUnderlyingType(ValueType) is null) {
                return Array.Empty<Guid>();
            }

            lock (entries) {
                if (entries.Count == 0) {
                    return Array.Empty<Guid>();
                }

                var searchEntry = new BinaryEntityStoreIndexValue(Guid.Empty, typedValue, ValueConverter.Text(typedValue));
                var index = entries.BinarySearch(searchEntry, entryComparer);
                if (index < 0) {
                    return Array.Empty<Guid>();
                }

                var results = new List<Guid>();
                CollectMatches(index, typedValue, results);
                return results;
            }
        }

        /// <summary>
        /// Performs a fuzzy lookup using the string catalog built by this index.
        /// Use this when <see cref="SupportsFullText"/> is true and either you want tolerant string matching
        /// or <see cref="Seek(object)"/> returned no matches because the search term was approximate.
        /// </summary>
        /// <param name="query">Text to search for.</param>
        /// <param name="tolerance">Allowed edit distance.</param>
        /// <returns>Unique set of entity ids that contained the matching text.</returns>
        public IReadOnlyList<Guid> FullTextSearch(string query, int tolerance = 1)
        {
            if (!SupportsFullText || stringSearcher is null || stringStorage is null) {
                return Array.Empty<Guid>();
            }

            if (string.IsNullOrWhiteSpace(query)) {
                return Array.Empty<Guid>();
            }

            EnsureMetadata();

            var matches = new HashSet<Guid>();
            foreach (var result in stringSearcher.Search(query, tolerance)) {
                if (stringOwners.TryGetValue(result.Source, out var owners) && owners.Count > 0) {
                    matches.UnionWith(owners);
                }
            }

            return matches.ToArray();
        }

        /// <summary>
        /// Replaces (or inserts) the values for the specified entity and persists the mutations to disk.
        /// </summary>
        /// <param name="entityId">Entity identifier.</param>
        /// <param name="values">Values to index.</param>
        /// <param name="cancellationToken">Token used to cancel the upsert.</param>
        internal async Task UpsertAsync(Guid entityId, IReadOnlyList<object?> values, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(values);
            EnsureMetadata();

            var normalized = NormalizeValues(entityId, values);

            await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try {
                var mutations = new List<BinaryEntityStoreIndexMutation>();
                if (entityValues.ContainsKey(entityId)) {
                    RemoveEntityInternal(entityId);
                    mutations.Add(BinaryEntityStoreIndexMutation.Remove(entityId));
                }

                foreach (var entry in normalized) {
                    AddEntryInternal(entry);
                    mutations.Add(BinaryEntityStoreIndexMutation.Add(entry.EntityId, entry.Text));
                }

                foreach (var mutation in mutations) {
                    await AppendMutationAsync(mutation, cancellationToken).ConfigureAwait(false);
                }
            }
            finally {
                gate.Release();
            }
        }

        /// <summary>
        /// Removes every indexed value for the specified entity identifier.
        /// </summary>
        /// <param name="entityId">Entity identifier.</param>
        /// <param name="cancellationToken">Token used to cancel the removal.</param>
        internal async Task RemoveAsync(Guid entityId, CancellationToken cancellationToken)
        {
            EnsureMetadata();

            await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try {
                if (!entityValues.ContainsKey(entityId)) {
                    return;
                }

                RemoveEntityInternal(entityId);
                await AppendMutationAsync(BinaryEntityStoreIndexMutation.Remove(entityId), cancellationToken).ConfigureAwait(false);
            }
            finally {
                gate.Release();
            }
        }

        /// <summary>
        /// Releases the index resources (locks and open file handlers).
        /// </summary>
        public void Dispose()
        {
            if (disposed) {
                return;
            }

            disposed = true;
            gate.Dispose();
            store.Dispose();
            GC.SuppressFinalize(this);
        }

        private void CollectMatches(int index, object? typedValue, List<Guid> buffer)
        {
            var comparerInstance = valueComparer ?? throw new InvalidOperationException("Index not initialized.");
            var current = index;
            while (current >= 0 && comparerInstance.Compare(entries[current].Value, typedValue) == 0) {
                buffer.Add(entries[current].EntityId);
                current--;
            }

            current = index + 1;
            while (current < entries.Count && comparerInstance.Compare(entries[current].Value, typedValue) == 0) {
                buffer.Add(entries[current].EntityId);
                current++;
            }
        }

        private void ApplyMutation(BinaryEntityStoreIndexMutation mutation)
        {
            switch (mutation.Type) {
                case BinaryEntityStoreIndexMutationType.Remove:
                    RemoveEntityInternal(mutation.EntityId);
                    break;

                case BinaryEntityStoreIndexMutationType.Add:
                    if (mutation.Value is null) {
                        break;
                    }

                    var typed = ValueConverter.ChangeType(mutation.Value, ValueType);
                    if (typed is null && ValueType.IsValueType && Nullable.GetUnderlyingType(ValueType) is null) {
                        break;
                    }

                    var entry = new BinaryEntityStoreIndexValue(mutation.EntityId, typed, mutation.Value);
                    AddEntryInternal(entry);
                    break;
            }
        }

        private void AddEntryInternal(BinaryEntityStoreIndexValue entry)
        {
            lock (entries) {
                var index = entries.BinarySearch(entry, entryComparer);
                if (index < 0) {
                    index = ~index;
                }
                entries.Insert(index, entry);
            }

            if (!entityValues.TryGetValue(entry.EntityId, out var existing)) {
                existing = new List<EntityValueReference>();
                entityValues[entry.EntityId] = existing;
            }
            existing.Add(new EntityValueReference(entry.Value, entry.Text));

            if (SupportsFullText) {
                AddToFullText(entry.Text, entry.EntityId);
            }
        }

        private List<BinaryEntityStoreIndexValue> NormalizeValues(Guid entityId, IReadOnlyList<object?> source)
        {
            var normalized = new List<BinaryEntityStoreIndexValue>(source.Count);
            foreach (var value in source) {
                if (value is null) {
                    continue;
                }

                var typedValue = ValueConverter.ChangeType(value, ValueType);
                if (typedValue is null && ValueType.IsValueType && Nullable.GetUnderlyingType(ValueType) is null) {
                    continue;
                }

                var text = ValueConverter.Text(typedValue);
                normalized.Add(new BinaryEntityStoreIndexValue(entityId, typedValue, text));
            }
            return normalized;
        }

        private void RemoveEntityInternal(Guid entityId)
        {
            if (!entityValues.TryGetValue(entityId, out var tracked)) {
                return;
            }

            foreach (var reference in tracked) {
                RemoveValueInternal(entityId, reference);
            }

            entityValues.Remove(entityId);
        }

        private void RemoveValueInternal(Guid entityId, EntityValueReference reference)
        {
            lock (entries) {
                var searchEntry = new BinaryEntityStoreIndexValue(entityId, reference.Value, reference.Text);
                var index = entries.BinarySearch(searchEntry, entryComparer);
                if (index < 0) {
                    index = ~index;
                }

                for (var i = index; i < entries.Count; i++) {
                    if (valueComparer!.Compare(entries[i].Value, reference.Value) != 0) {
                        break;
                    }

                    if (entries[i].EntityId == entityId) {
                        entries.RemoveAt(i);
                        break;
                    }
                }
            }

            if (SupportsFullText) {
                RemoveFromFullText(reference.Text, entityId);
            }
        }

        private void EnsureFullTextInfrastructure()
        {
            if (!SupportsFullText) {
                return;
            }

            stringStorage ??= new StringStorage();
            stringSearcher ??= new FuzzyStringSearcher(stringStorage);
        }

        private void AddToFullText(string text, Guid entityId)
        {
            if (!SupportsFullText || stringStorage is null) {
                return;
            }

            if (stringCatalog.Add(text)) {
                stringStorage.Add(text);
            }

            if (!stringOwners.TryGetValue(text, out var owners)) {
                owners = new HashSet<Guid>();
                stringOwners[text] = owners;
            }
            owners.Add(entityId);
        }

        private void RemoveFromFullText(string text, Guid entityId)
        {
            if (!SupportsFullText) {
                return;
            }

            if (!stringOwners.TryGetValue(text, out var owners)) {
                return;
            }

            owners.Remove(entityId);
            if (owners.Count == 0) {
                stringOwners.Remove(text);
            }
        }

        private void EnsureMetadata()
        {
            if (metadata is null) {
                throw new InvalidOperationException("Index metadata is required before performing this operation.");
            }
        }

        private async Task AppendMutationAsync(BinaryEntityStoreIndexMutation mutation, CancellationToken cancellationToken)
        {
            var payload = SerializeMutation(mutation);
            var offset = store.Size;
            using var buffer = await store.WriteAsync(offset, payload.Length, cancellationToken).ConfigureAwait(false);
            await buffer.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
        }

        private static BinaryEntityStoreIndexMetadata ReadMetadata(BinaryReader reader)
        {
            var version = reader.ReadByte();
            var path = reader.ReadString();
            var typeName = reader.ReadString();
            var capabilities = (BinaryEntityStoreIndexCapabilities)reader.ReadByte();
            return new BinaryEntityStoreIndexMetadata(path, typeName, capabilities, version);
        }

        private static BinaryEntityStoreIndexMutation ReadMutation(BinaryReader reader)
        {
            var type = (BinaryEntityStoreIndexMutationType)reader.ReadByte();
            var entityBytes = reader.ReadBytes(16);
            var entityId = new Guid(entityBytes);
            if (type == BinaryEntityStoreIndexMutationType.Add) {
                var value = reader.ReadString();
                return BinaryEntityStoreIndexMutation.Add(entityId, value);
            }

            return BinaryEntityStoreIndexMutation.Remove(entityId);
        }

        private static byte[] SerializeMutation(BinaryEntityStoreIndexMutation mutation)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
            writer.Write((byte)BinaryEntityStoreIndexEntryKind.Mutation);
            writer.Write((byte)mutation.Type);
            writer.Write(mutation.EntityId.ToByteArray());
            if (mutation.Type == BinaryEntityStoreIndexMutationType.Add) {
                writer.Write(mutation.Value ?? string.Empty);
            }
            writer.Flush();
            return stream.ToArray();
        }

        private static IComparer<object?> CreateValueComparer(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            var underlying = Nullable.GetUnderlyingType(type) ?? type;

            if (typeof(IComparable).IsAssignableFrom(underlying)) {
                return Comparer<object?>.Create((left, right) =>
                {
                    if (ReferenceEquals(left, right)) {
                        return 0;
                    }

                    if (left is null) {
                        return -1;
                    }

                    if (right is null) {
                        return 1;
                    }

                    var comparable = (IComparable)left;
                    return comparable.CompareTo(right);
                });
            }

            return Comparer<object?>.Create((left, right) =>
            {
                var leftText = ValueConverter.Text(left);
                var rightText = ValueConverter.Text(right);
                return StringComparer.Ordinal.Compare(leftText, rightText);
            });
        }

        private readonly struct BinaryEntityStoreIndexValue
        {
            public BinaryEntityStoreIndexValue(Guid entityId, object? value, string text)
            {
                EntityId = entityId;
                Value = value;
                Text = text;
            }

            public Guid EntityId { get; }
            public object? Value { get; }
            public string Text { get; }
        }

        private readonly struct EntityValueReference
        {
            public EntityValueReference(object? value, string text)
            {
                Value = value;
                Text = text;
            }

            public object? Value { get; }
            public string Text { get; }
        }

        private readonly struct BinaryEntityStoreIndexMutation
        {
            private BinaryEntityStoreIndexMutation(BinaryEntityStoreIndexMutationType type, Guid entityId, string? value)
            {
                Type = type;
                EntityId = entityId;
                Value = value;
            }

            public BinaryEntityStoreIndexMutationType Type { get; }
            public Guid EntityId { get; }
            public string? Value { get; }

            public static BinaryEntityStoreIndexMutation Add(Guid entityId, string value)
                => new(BinaryEntityStoreIndexMutationType.Add, entityId, value);

            public static BinaryEntityStoreIndexMutation Remove(Guid entityId)
                => new(BinaryEntityStoreIndexMutationType.Remove, entityId, null);
        }

        private sealed class IndexValueComparer : IComparer<BinaryEntityStoreIndexValue>
        {
            private readonly BinaryEntityStoreIndex owner;

            public IndexValueComparer(BinaryEntityStoreIndex owner)
            {
                this.owner = owner;
            }

            public int Compare(BinaryEntityStoreIndexValue x, BinaryEntityStoreIndexValue y)
            {
                var comparerInstance = owner.valueComparer ?? throw new InvalidOperationException("Index not initialized.");
                return comparerInstance.Compare(x.Value, y.Value);
            }
        }

        private enum BinaryEntityStoreIndexEntryKind : byte
        {
            Metadata = 1,
            Mutation = 2
        }
    }
}

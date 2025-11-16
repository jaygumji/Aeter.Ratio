/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.Binary.EntityStore;
using Aeter.Ratio.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Aeter.Ratio.Test.Binary
{
    public class BinaryEntityStoreTests
    {
        private static BinaryEntityStore CreateStore()
            => new BinaryEntityStore(BinaryStream.MemoryStream(), BinaryBufferPool.Default);

        [Fact]
        public async Task ReadAsync_ReturnsPayloadAndMetadata()
        {
            using var store = CreateStore();
            var metadata = new BinaryEntityStoreRecordMetadata(Guid.NewGuid(), version: 7);
            var payload = Enumerable.Range(0, 16).Select(i => (byte)(i + 1)).ToArray();

            using (var writeBuffer = await store.WriteAsync(0, payload.Length, metadata)) {
                await writeBuffer.WriteAsync(payload);
            }

            using var result = await store.ReadAsync(0);
            Assert.True(result.Header.IsInUse);
            Assert.Equal(metadata.Key, result.Header.Metadata.Key);
            Assert.Equal(metadata.Version, result.Header.Metadata.Version);

            var readPayload = await result.Buffer.ReadAsync(payload.Length);
            Assert.True(readPayload.Span.SequenceEqual(payload));
        }

        [Fact]
        public async Task ReadAllAsync_EnumeratesAllEntriesAndSkipsPayloads()
        {
            using var store = CreateStore();
            var entries = new[] {
                (Payload: Enumerable.Repeat((byte)0xAA, 6000).ToArray(), Metadata: new BinaryEntityStoreRecordMetadata(Guid.NewGuid(), 1u)),
                (Payload: Enumerable.Repeat((byte)0x55, 64).ToArray(), Metadata: new BinaryEntityStoreRecordMetadata(Guid.NewGuid(), 2u)),
            };

            var appended = new List<(long Offset, BinaryEntityStoreEntryHeader Header)>();
            var offset = 0L;
            foreach (var entry in entries) {
                using (var writeBuffer = await store.WriteAsync(offset, entry.Payload.Length, entry.Metadata)) {
                    await writeBuffer.WriteAsync(entry.Payload);
                }

                using var readForSize = await store.ReadAsync(offset);
                appended.Add((offset, readForSize.Header));
                offset += readForSize.Header.Size;
            }

            var observed = new List<(long Offset, BinaryEntityStoreEntryHeader Header)>();
            await store.ReadAllAsync(args => {
                observed.Add((args.Offset, args.Header));
                return Task.CompletedTask;
            });

            Assert.Equal(appended.Count, observed.Count);
            for (var i = 0; i < appended.Count; i++) {
                Assert.Equal(appended[i].Offset, observed[i].Offset);
                Assert.Equal(appended[i].Header.Metadata.Key, observed[i].Header.Metadata.Key);
                Assert.Equal(appended[i].Header.Metadata.Version, observed[i].Header.Metadata.Version);
                Assert.Equal(appended[i].Header.PayloadLength, observed[i].Header.PayloadLength);
            }
        }

        [Fact]
        public async Task MarkAsNotUsedAsync_UpdatesMarkerAndKeepsMetadata()
        {
            using var store = CreateStore();
            var metadata = new BinaryEntityStoreRecordMetadata(Guid.NewGuid(), version: 11);
            var payload = new byte[] { 1, 2, 3, 4 };

            using (var writeBuffer = await store.WriteAsync(0, payload.Length, metadata)) {
                writeBuffer.Write(payload);
            }

            await store.MarkAsNotUsedAsync(0);

            using var result = await store.ReadAsync(0);
            Assert.False(result.Header.IsInUse);
            Assert.Equal(metadata.Key, result.Header.Metadata.Key);
            Assert.Equal(metadata.Version, result.Header.Metadata.Version);

            var storedPayload = await result.Buffer.ReadAsync(payload.Length);
            Assert.True(storedPayload.Span.SequenceEqual(payload));
        }
    }
}

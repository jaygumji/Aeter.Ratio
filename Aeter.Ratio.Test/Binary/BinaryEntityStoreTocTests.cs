/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.Binary.EntityStore;
using Aeter.Ratio.Scheduling;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Aeter.Ratio.Test.Binary
{
    public sealed class BinaryEntityStoreTocTests : IDisposable
    {
        private readonly string workingDirectory;

        public BinaryEntityStoreTocTests()
        {
            workingDirectory = CreateTempDirectory();
        }

        [Fact]
        public async Task CreateAsync_InitializesHeaderEntry()
        {
            using var scheduler = new Scheduler();
            var tocPath = GetPath("toc.bin");
            using var entityStore = new BinaryEntityStore(GetPath("store.bin"), BinaryBufferPool.Default);

            var (toc, initHandle) = await BinaryEntityStoreToc.CreateAsync(tocPath, BinaryBufferPool.Default, entityStore, scheduler);
            await initHandle.Completion;
            await initHandle.DisposeAsync();

            var header = toc.Header;
            Assert.Equal(Guid.Empty, header.Key);
            Assert.False(header.IsFree);

            toc.Dispose();
        }

        [Fact]
        public async Task UpsertAndRemove_UpdateEntryCollection()
        {
            using var scheduler = new Scheduler();
            var tocPath = GetPath("toc_entries.bin");
            using var entityStore = new BinaryEntityStore(GetPath("store_entries.bin"), BinaryBufferPool.Default);

            var (toc, initHandle) = await BinaryEntityStoreToc.CreateAsync(tocPath, BinaryBufferPool.Default, entityStore, scheduler);
            await initHandle.Completion;
            await initHandle.DisposeAsync();

            var key = Guid.NewGuid();
            toc.Upsert(key, offset: 42, size: 128, isFree: false);

            Assert.True(toc.TryGetEntry(key, out var entry));
            Assert.NotNull(entry);
            Assert.Equal(42, entry!.Offset);
            Assert.Equal(128, entry.Size);
            Assert.False(entry.IsFree);

            Assert.True(toc.Remove(key));
            Assert.False(toc.TryGetEntry(key, out _));

            toc.Dispose();
        }

        private string GetPath(string fileName)
            => Path.Combine(workingDirectory, fileName);

        public void Dispose()
        {
            TryDeleteDirectory(workingDirectory);
        }

        private static string CreateTempDirectory()
        {
            for (var i = 0; i < 3; i++) {
                var path = Path.Combine(Path.GetTempPath(), "aeter_ratio_toc_" + Guid.NewGuid().ToString("N"));
                try {
                    Directory.CreateDirectory(path);
                    return path;
                }
                catch (UnauthorizedAccessException) when (i < 2) {
                    continue;
                }
            }
            throw new InvalidOperationException("Unable to create temporary working directory for BinaryEntityStoreToc tests.");
        }

        private static void TryDeleteDirectory(string path)
        {
            try {
                if (Directory.Exists(path)) {
                    Directory.Delete(path, recursive: true);
                }
            }
            catch (UnauthorizedAccessException) {
                // ignore cleanup failures
            }
        }
    }
}

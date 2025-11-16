/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary;
using Aeter.Ratio.Binary.EntityStore;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Aeter.Ratio.Test.Binary
{
    public sealed class BinaryEntityStoreEngineTests : IDisposable
    {
        private readonly string workingDirectory;

        public BinaryEntityStoreEngineTests()
        {
            workingDirectory = CreateTempDirectory();
        }

        [Fact]
        public async Task AddAndGetEntity_RoundTripsThroughStore()
        {
            var (engine, _) = await CreateEngineAsync();
            await using var scopedEngine = engine;

            var entity = new SampleEntity { Name = "alpha", Value = 7 };
            var id = await scopedEngine.AddAsync(entity);

            var stored = await scopedEngine.GetAsync<SampleEntity>(id);

            Assert.NotNull(stored);
            Assert.Equal(entity.Name, stored!.Name);
            Assert.Equal(entity.Value, stored.Value);
        }

        [Fact]
        public async Task UpdateAsync_ReplacesExistingPayload()
        {
            var (engine, id) = await CreateEngineWithEntityAsync(new SampleEntity { Name = "before", Value = 1 });
            await using var scopedEngine = engine;

            var replacement = new SampleEntity { Name = "after", Value = 2 };
            await scopedEngine.UpdateAsync(id, replacement);

            var stored = await scopedEngine.GetAsync<SampleEntity>(id);
            Assert.NotNull(stored);
            Assert.Equal(replacement.Name, stored!.Name);
            Assert.Equal(replacement.Value, stored.Value);
        }

        [Fact]
        public async Task DeleteAsync_RemovesEntity()
        {
            var (engine, id) = await CreateEngineWithEntityAsync(new SampleEntity { Name = "todelete", Value = 42 });
            await using var scopedEngine = engine;

            await scopedEngine.DeleteAsync(id);

            var stored = await scopedEngine.GetAsync<SampleEntity>(id);
            Assert.Null(stored);
        }

        [Fact]
        public async Task ShrinkAsync_CompactsFreeSpace()
        {
            var engine = await CreateEngineInternalAsync();
            await using var scopedEngine = engine;

            var first = await scopedEngine.AddAsync(new SampleEntity { Name = "first", Value = 1 });
            var second = await scopedEngine.AddAsync(new SampleEntity { Name = "second", Value = 2 });
            var third = await scopedEngine.AddAsync(new SampleEntity { Name = "third", Value = 3 });

            await scopedEngine.DeleteAsync(second);

            var beforeShrink = await scopedEngine.GetAsync<SampleEntity>(third);
            Assert.NotNull(beforeShrink);

            await scopedEngine.ShrinkAsync();

            var afterShrink = await scopedEngine.GetAsync<SampleEntity>(third);
            Assert.NotNull(afterShrink);
            Assert.Equal(3, afterShrink!.Value);

            var missingSecond = await scopedEngine.GetAsync<SampleEntity>(second);
            Assert.Null(missingSecond);
        }

        private async Task<(BinaryEntityStoreEngine Engine, Guid EntityId)> CreateEngineWithEntityAsync(SampleEntity entity)
        {
            var engine = await CreateEngineInternalAsync();
            var id = await engine.AddAsync(entity);
            return (engine, id);
        }

        private async Task<(BinaryEntityStoreEngine Engine, Guid SeedId)> CreateEngineAsync()
        {
            var engine = await CreateEngineInternalAsync();
            return (engine, Guid.Empty);
        }

        private async Task<BinaryEntityStoreEngine> CreateEngineInternalAsync()
        {
            var storePath = Path.Combine(workingDirectory, Guid.NewGuid().ToString("N") + ".store");
            return await BinaryEntityStoreEngine.CreateAsync(storePath, BinaryBufferPool.Default);
        }

        public void Dispose()
        {
            TryDeleteDirectory(workingDirectory);
        }

        public class SampleEntity
        {
            public string Name { get; set; } = string.Empty;
            public int Value { get; set; }
        }

        private static string CreateTempDirectory()
        {
            for (var i = 0; i < 3; i++) {
                var path = Path.Combine(Path.GetTempPath(), "aeter_ratio_engine_" + Guid.NewGuid().ToString("N"));
                try {
                    Directory.CreateDirectory(path);
                    return path;
                }
                catch (UnauthorizedAccessException) when (i < 2) {
                    continue;
                }
            }
            throw new InvalidOperationException("Unable to create temporary working directory for BinaryEntityStoreEngine tests.");
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

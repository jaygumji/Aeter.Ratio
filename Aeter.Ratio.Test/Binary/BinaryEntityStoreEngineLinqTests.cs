/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Binary.EntityStore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SampleEntity = Aeter.Ratio.Test.Binary.BinaryEntityStoreEngineTests.SampleEntity;

namespace Aeter.Ratio.Test.Binary
{
    public sealed class BinaryEntityStoreEngineLinqTests : IDisposable
    {
        private readonly string workingDirectory;

        public BinaryEntityStoreEngineLinqTests()
        {
            workingDirectory = BinaryEntityStoreEngineTestUtilities.CreateTempDirectory();
        }

        [Fact]
        public async Task Query_ReturnsAllEntities()
        {
            await using var engine = await CreateSeededEngineAsync(
                new SampleEntity { Name = "first", Value = 1 },
                new SampleEntity { Name = "second", Value = 2 },
                new SampleEntity { Name = "third", Value = 3 });

            var names = engine.Query<SampleEntity>()
                .Select(e => e.Name)
                .ToArray();

            Assert.Equal(new[] { "first", "second", "third" }, names);
        }

        [Fact]
        public async Task Query_WithWhereFilter_ReturnsMatchingEntities()
        {
            await using var engine = await CreateSeededEngineAsync(
                new SampleEntity { Name = "low", Value = 1 },
                new SampleEntity { Name = "mid", Value = 5 },
                new SampleEntity { Name = "high", Value = 10 });

            var filtered = engine.Query<SampleEntity>()
                .Where(e => e.Value >= 5)
                .OrderBy(e => e.Value)
                .Select(e => e.Name)
                .ToList();

            Assert.Equal(new[] { "mid", "high" }, filtered);
        }

        [Fact]
        public async Task Query_SupportsAggregates()
        {
            await using var engine = await CreateSeededEngineAsync(
                new SampleEntity { Name = "alpha", Value = 2 },
                new SampleEntity { Name = "beta", Value = 4 },
                new SampleEntity { Name = "gamma", Value = 6 });

            var count = engine.Query<SampleEntity>().Count(e => e.Value >= 4);
            Assert.Equal(2, count);

            var first = engine.Query<SampleEntity>().FirstOrDefault(e => e.Name == "beta");
            Assert.NotNull(first);
            Assert.Equal("beta", first!.Name);
        }

        private async Task<BinaryEntityStoreEngine> CreateSeededEngineAsync(params SampleEntity[] entities)
        {
            var engine = await BinaryEntityStoreEngineTestUtilities.CreateEngineAsync(workingDirectory);
            foreach (var entity in entities) {
                await engine.AddAsync(entity);
            }

            return engine;
        }

        public void Dispose()
        {
            BinaryEntityStoreEngineTestUtilities.TryDeleteDirectory(workingDirectory);
        }
    }
}

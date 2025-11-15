/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Threading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Aeter.Ratio.Test.Threading
{
    public class ReadExclusiveWriteLockTests
    {
        [Fact]
        public async Task MultipleReadersPerKey_Succeeds()
        {
            var rw = new ReadExclusiveWriteLock<Guid>();
            var key = Guid.NewGuid();

            var handles = await Task.WhenAll(rw.EnterReadAsync(key), rw.EnterReadAsync(key), rw.EnterReadAsync(key));

            Assert.Equal(3, handles.Length);

            foreach (var handle in handles) {
                await handle.DisposeAsync();
            }
        }

        [Fact]
        public async Task WriterWaitsForReadersAndBlocksOtherWriters()
        {
            var rw = new ReadExclusiveWriteLock<Guid>();
            var key = Guid.NewGuid();

            var readers = await Task.WhenAll(rw.EnterReadAsync(key), rw.EnterReadAsync(key));

            var writerTask = rw.EnterWriteAsync(key);
            await Task.Delay(50);
            Assert.False(writerTask.IsCompleted);

            foreach (var reader in readers) {
                await reader.DisposeAsync();
            }

            var writerHandle = await writerTask;
            var competingWriter = rw.EnterWriteAsync(key);
            await Task.Delay(25);
            Assert.False(competingWriter.IsCompleted);

            await writerHandle.DisposeAsync();
            await (await competingWriter).DisposeAsync();
        }

        [Fact]
        public async Task DifferentKeys_DoNotBlockEachOther()
        {
            var rw = new ReadExclusiveWriteLock<Guid>();
            var keys = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

            var tasks = new List<Task>();
            foreach (var key in keys) {
                tasks.Add(Task.Run(async () => {
                    await using var writer = await rw.EnterWriteAsync(key);
                    await Task.Delay(20);
                }));
            }

            await Task.WhenAll(tasks);
        }
    }
}

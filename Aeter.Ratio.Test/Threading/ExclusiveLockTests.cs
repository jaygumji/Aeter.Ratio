using System;
using System.Threading.Tasks;
using Aeter.Ratio.Threading;
using Xunit;

namespace Aeter.Ratio.Test.Threading
{
    public class ExclusiveLockTests
    {
        [Fact]
        public async Task EnterAsync_AllowsMultipleReaders()
        {
            var lck = new ExclusiveLock();
            var handles = await Task.WhenAll(lck.EnterAsync(), lck.EnterAsync());

            Assert.Equal(2, handles.Length);

            foreach (var handle in handles) {
                await handle.ReleaseAsync();
            }
        }

        [Fact]
        public async Task WriterWaitsForReaders()
        {
            var lck = new ExclusiveLock();
            var reader = await lck.EnterAsync();

            var writerTask = lck.EnterExclusiveAsync();

            await Task.Delay(50);
            Assert.False(writerTask.IsCompleted);

            await reader.ReleaseAsync();

            var writerHandle = await writerTask;
            await writerHandle.ReleaseAsync();
        }

        [Fact]
        public async Task TryEnterExclusiveAsyncTimesOutWhenBusy()
        {
            var lck = new ExclusiveLock();
            var writer = await lck.EnterExclusiveAsync();

            var result = await lck.TryEnterExclusiveAsync(TimeSpan.FromMilliseconds(50));
            Assert.Null(result);

            await writer.ReleaseAsync();
        }
    }
}

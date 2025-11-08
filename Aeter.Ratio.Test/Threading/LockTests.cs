using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Aeter.Ratio.Threading;
using Xunit;

namespace Aeter.Ratio.Test.Threading
{
    public class LockTests
    {
        [Fact]
        public async Task TwoThreadsWithLockOfInt()
        {
            var lck = new Lock<int>();
            var res = new List<int>();

            var handle = await lck.EnterAsync(1);
            var t2 = Task.Run(async () => {
                var handle = await lck.EnterAsync(1);
                try {
                    res.Add(2);
                }
                finally {
                    await handle.ReleaseAsync();
                }
            });

            try {
                await Task.Delay(100);
                res.Add(1);
            }
            finally {
                await handle.ReleaseAsync();
            }

            await t2;
            Assert.Equal(2, res.Count);
            Assert.Equal(1, res[0]);
            Assert.Equal(2, res[1]);
        }

        [Fact]
        public async Task TryEnterAsyncReturnsNullWhenTimedOut()
        {
            var lck = new Lock<int>();
            var initialHandle = await lck.EnterAsync(1);

            Lock<int>.LockHandle? timedOutHandle = null;
            try {
                timedOutHandle = await lck.TryEnterAsync(1, TimeSpan.FromMilliseconds(50));
                Assert.Null(timedOutHandle);
            }
            finally {
                await initialHandle.ReleaseAsync();
            }

            var acquiredAfterRelease = await lck.TryEnterAsync(1, TimeSpan.FromMilliseconds(200));
            Assert.NotNull(acquiredAfterRelease);
            await acquiredAfterRelease!.ReleaseAsync();

            Assert.Equal(0, GetTrackedEntryCount(lck));
        }

        [Fact]
        public async Task TryEnterAsyncCancellationRollsBackState()
        {
            var lck = new Lock<int>();
            var initialHandle = await lck.EnterAsync(42);
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

            await Assert.ThrowsAsync<OperationCanceledException>(() => lck.TryEnterAsync(42, TimeSpan.FromSeconds(5), cts.Token));

            await initialHandle.ReleaseAsync();
            Assert.Equal(0, GetTrackedEntryCount(lck));
        }

        [Fact]
        public async Task DoubleReleaseThrows()
        {
            var lck = new Lock<int>();
            var handle = await lck.EnterAsync(7);

            await handle.ReleaseAsync();
            await Assert.ThrowsAsync<InvalidOperationException>(() => handle.ReleaseAsync());
        }

        [Fact]
        public async Task DifferentKeysDoNotBlockEachOther()
        {
            var lck = new Lock<int>();

            var handle1 = await lck.EnterAsync(1);
            var handle2Task = lck.EnterAsync(2);
            var handle2 = await handle2Task;

            await handle1.ReleaseAsync();
            await handle2.ReleaseAsync();

            Assert.Equal(0, GetTrackedEntryCount(lck));
        }

        private static int GetTrackedEntryCount<T>(Lock<T> lck) where T : notnull
        {
            var field = typeof(Lock<T>).GetField("_locks", BindingFlags.NonPublic | BindingFlags.Instance);
            var dictionary = field!.GetValue(lck)!;
            return (int)dictionary.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance)!
                .GetValue(dictionary)!;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Aeter.Ratio.Test
{
    public class AsyncEventTests
    {
        private sealed class SampleArgs : AsyncEventArgs
        {
        }

        [Fact]
        public async Task RaiseAsync_InvokesDelegatesInRegistrationOrder()
        {
            var asyncEvent = new AsyncEvent<SampleArgs>();
            var order = new List<int>();

            asyncEvent.Register(async (_, _) => {
                await Task.Yield();
                order.Add(1);
            });
            asyncEvent.Register(async (_, _) => {
                await Task.Yield();
                order.Add(2);
            });

            await asyncEvent.RaiseAsync(new SampleArgs());

            Assert.Equal([1, 2], order);
        }

        [Fact]
        public async Task Unregister_RemovesDelegate()
        {
            var asyncEvent = new AsyncEvent<SampleArgs>();
            var wasCalled = false;
            AsyncEventDelegate<SampleArgs> handler = (_, _) => {
                wasCalled = true;
                return Task.CompletedTask;
            };

            asyncEvent.Register(handler);
            asyncEvent.Unregister(handler);

            await asyncEvent.RaiseAsync(new SampleArgs());

            Assert.False(wasCalled);
        }

        [Fact]
        public async Task RaiseAsync_ThrowsWhenArgsAreNull()
        {
            var asyncEvent = new AsyncEvent<SampleArgs>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => asyncEvent.RaiseAsync(null!));
        }
    }
}

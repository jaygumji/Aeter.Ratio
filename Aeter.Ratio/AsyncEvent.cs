using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aeter.Ratio
{
    public class AsyncEvent<T> where T : AsyncEventArgs
    {
        private readonly List<AsyncEventDelegate<T>> delegates = [];
        private readonly object syncRoot = new();

        public async Task RaiseAsync(T args)
        {
            ArgumentNullException.ThrowIfNull(args);

            AsyncEventDelegate<T>[] handlers;
            lock (syncRoot) {
                if (delegates.Count == 0) {
                    return;
                }

                handlers = delegates.ToArray();
            }

            foreach (var handler in handlers) {
                await handler(this, args).ConfigureAwait(false);
            }
        }

        public void Register(AsyncEventDelegate<T> @delegate)
        {
            ArgumentNullException.ThrowIfNull(@delegate);
            lock (syncRoot) {
                delegates.Add(@delegate);
            }
        }

        public void Unregister(AsyncEventDelegate<T> @delegate)
        {
            ArgumentNullException.ThrowIfNull(@delegate);
            lock (syncRoot) {
                delegates.Remove(@delegate);
            }
        }
    }

    public delegate Task AsyncEventDelegate<T>(object sender, T args) where T : AsyncEventArgs;

    public class AsyncEventArgs
    {
    }
}

/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Threading
{
    public static class Invocation
    {
        public static void RunSync(Func<Task> action)
        {
            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }

            RunSyncInternal(action);
        }

        public static void RunSync(Func<object, Task> action, object state)
        {
            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }

            RunSyncInternal(() => action(state));
        }

        public static T RunSync<T>(Func<Task<T>> action)
        {
            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }

            return RunSyncInternal(action);
        }

        public static T RunSync<T, TState>(Func<TState, Task<T>> action, TState state)
        {
            if (action == null) {
                throw new ArgumentNullException(nameof(action));
            }

            return RunSyncInternal(() => action(state));
        }

        private static void RunSyncInternal(Func<Task> action)
        {
            var currentContext = SynchronizationContext.Current;
            using var exclusiveContext = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(exclusiveContext);

            try {
                exclusiveContext.Post(async _ => {
                    try {
                        await action().ConfigureAwait(true);
                    }
                    catch (Exception ex) {
                        exclusiveContext.SetException(ex);
                    }
                    finally {
                        exclusiveContext.Complete();
                    }
                }, null);

                exclusiveContext.RunOnCurrentThread();
            }
            finally {
                SynchronizationContext.SetSynchronizationContext(currentContext);
            }
        }

        private static T RunSyncInternal<T>(Func<Task<T>> action)
        {
            T result = default;
            RunSyncInternal(async () => {
                result = await action().ConfigureAwait(true);
            });
            return result;
        }

        private sealed class ExclusiveSynchronizationContext : SynchronizationContext, IDisposable
        {
            private readonly AutoResetEvent _workItemsWaiting = new AutoResetEvent(false);
            private readonly Queue<(SendOrPostCallback Callback, object? State)> _items = new Queue<(SendOrPostCallback, object?)>();
            private bool _done;
            private ExceptionDispatchInfo? _exception;

            public override void Post(SendOrPostCallback d, object? state)
            {
                if (d == null) {
                    throw new ArgumentNullException(nameof(d));
                }

                lock (_items) {
                    _items.Enqueue((d, state));
                }

                _workItemsWaiting.Set();
            }

            public override void Send(SendOrPostCallback d, object? state)
            {
                throw new NotSupportedException("Synchronous send is not supported in this context.");
            }

            public void RunOnCurrentThread()
            {
                while (true) {
                    (SendOrPostCallback Callback, object? State)? work = null;

                    lock (_items) {
                        if (_items.Count > 0) {
                            work = _items.Dequeue();
                        }
                    }

                    if (work.HasValue) {
                        work.Value.Callback(work.Value.State);

                        if (_exception != null) {
                            _exception.Throw();
                        }

                        continue;
                    }

                    if (_done) {
                        break;
                    }

                    _workItemsWaiting.WaitOne();
                }

                _exception?.Throw();
            }

            public void Complete()
            {
                _done = true;
                _workItemsWaiting.Set();
            }

            public void SetException(Exception exception)
            {
                _exception = ExceptionDispatchInfo.Capture(exception);
            }

            public override SynchronizationContext CreateCopy() => this;

            public void Dispose()
            {
                _workItemsWaiting.Dispose();
            }
        }
    }
}

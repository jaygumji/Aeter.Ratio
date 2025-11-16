using System;
using System.Threading;
using System.Threading.Tasks;
using Aeter.Ratio.Threading;

namespace Aeter.Ratio.Binary.EntityStore
{
    /// <summary>
    /// Provides keyed read/write locks for entities together with a global append lock.
    /// Share this manager across components to coordinate access to the underlying store.
    /// </summary>
    public sealed class BinaryEntityStoreLockManager : IDisposable
    {
        private readonly ReadExclusiveWriteLock<Guid> entityLocks = new();
        private readonly SemaphoreSlim appendLock = new(1, 1);
        private bool disposed;

        public Task<ReadExclusiveWriteLock<Guid>.ReadHandle> EnterEntityReadAsync(Guid id, CancellationToken cancellationToken = default)
            => entityLocks.EnterReadAsync(id, cancellationToken);

        public Task<ReadExclusiveWriteLock<Guid>.WriteHandle> EnterEntityWriteAsync(Guid id, CancellationToken cancellationToken = default)
            => entityLocks.EnterWriteAsync(id, cancellationToken);

        public async Task<AppendLockHandle> EnterAppendAsync(CancellationToken cancellationToken = default)
        {
            await appendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new AppendLockHandle(appendLock);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            entityLocks.Dispose();
            appendLock.Dispose();
        }

        public struct AppendLockHandle : IAsyncDisposable, IDisposable
        {
            private SemaphoreSlim? semaphore;

            internal AppendLockHandle(SemaphoreSlim semaphore)
            {
                this.semaphore = semaphore;
            }

            public void Dispose()
            {
                var sem = Interlocked.Exchange(ref semaphore, null);
                sem?.Release();
            }

            public ValueTask DisposeAsync()
            {
                Dispose();
                return ValueTask.CompletedTask;
            }
        }
    }
}

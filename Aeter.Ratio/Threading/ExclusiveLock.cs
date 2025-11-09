/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Threading
{
    public class ExclusiveLock : IDisposable, IAsyncDisposable
    {
        private readonly SemaphoreSlim _writerSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _readerMutex = new SemaphoreSlim(1, 1);
        private volatile bool _disposed;
        private int _readerCount;

        public async Task<LockHandle> EnterAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            await _readerMutex.WaitAsync(cancellationToken).ConfigureAwait(false);
            var writerTaken = false;
            try {
                if (_readerCount == 0) {
                    await _writerSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    writerTaken = true;
                }
                _readerCount++;
            }
            catch {
                if (writerTaken) {
                    _writerSemaphore.Release();
                }
                throw;
            }
            finally {
                _readerMutex.Release();
            }

            return new LockHandle(this);
        }

        public Task<LockHandle?> TryEnterAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
            => TryEnterAsyncInternal(timeout, cancellationToken);

        public async Task<ExclusiveLockHandle> EnterExclusiveAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            await _writerSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new ExclusiveLockHandle(this);
        }

        public async Task<ExclusiveLockHandle?> TryEnterExclusiveAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            var entered = await _writerSemaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);
            return entered ? new ExclusiveLockHandle(this) : null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            Dispose(true);
            return ValueTask.CompletedTask;
        }

        private async Task<LockHandle?> TryEnterAsyncInternal(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var stopwatch = timeout == Timeout.InfiniteTimeSpan ? null : Stopwatch.StartNew();
            if (!await _readerMutex.WaitAsync(timeout, cancellationToken).ConfigureAwait(false)) {
                return null;
            }

            var writerTaken = false;
            try {
                if (_readerCount == 0) {
                    var remaining = GetRemaining(timeout, stopwatch);
                    if (!await _writerSemaphore.WaitAsync(remaining, cancellationToken).ConfigureAwait(false)) {
                        return null;
                    }
                    writerTaken = true;
                }

                _readerCount++;
                return new LockHandle(this);
            }
            catch {
                if (writerTaken) {
                    _writerSemaphore.Release();
                }
                throw;
            }
            finally {
                _readerMutex.Release();
            }
        }

        private async Task ReleaseReaderAsync()
        {
            ThrowIfDisposed();

            await _readerMutex.WaitAsync().ConfigureAwait(false);
            var shouldReleaseWriter = false;
            try {
                if (_readerCount <= 0) {
                    throw new InvalidOperationException("No read locks held.");
                }

                _readerCount--;
                shouldReleaseWriter = _readerCount == 0;
            }
            finally {
                _readerMutex.Release();
            }

            if (shouldReleaseWriter) {
                _writerSemaphore.Release();
            }
        }

        private Task ReleaseExclusiveAsync()
        {
            ThrowIfDisposed();
            _writerSemaphore.Release();
            return Task.CompletedTask;
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed) {
                return;
            }

            _disposed = true;
            _readerMutex.Dispose();
            _writerSemaphore.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) {
                throw new ObjectDisposedException(nameof(ExclusiveLock));
            }
        }

        private static TimeSpan GetRemaining(TimeSpan timeout, Stopwatch? stopwatch)
        {
            if (timeout == Timeout.InfiniteTimeSpan || stopwatch is null) {
                return timeout;
            }

            var remaining = timeout - stopwatch.Elapsed;
            return remaining <= TimeSpan.Zero ? TimeSpan.Zero : remaining;
        }

        public sealed class LockHandle
        {
            private ExclusiveLock? owner;

            internal LockHandle(ExclusiveLock owner)
            {
                this.owner = owner;
            }

            public Task ReleaseAsync()
            {
                var current = Interlocked.Exchange(ref owner, null);
                if (current is null) {
                    throw new InvalidOperationException("Handle is already released.");
                }

                return current.ReleaseReaderAsync();
            }

        }

        public sealed class ExclusiveLockHandle
        {
            private ExclusiveLock? owner;

            internal ExclusiveLockHandle(ExclusiveLock owner)
            {
                this.owner = owner;
            }

            public Task ReleaseAsync()
            {
                var current = Interlocked.Exchange(ref owner, null);
                if (current is null) {
                    throw new InvalidOperationException("Handle is already released.");
                }

                return current.ReleaseExclusiveAsync();
            }

        }
    }
}

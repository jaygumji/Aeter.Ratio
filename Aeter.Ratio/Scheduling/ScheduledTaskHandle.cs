/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Scheduling
{
    /// <summary>
    /// Represents a scheduled task.
    /// </summary>
    public sealed class ScheduledTaskHandle : IAsyncDisposable, IDisposable
    {
        private readonly Scheduler _owner;
        private readonly CancellationTokenSource _cancellationSource;
        private readonly Task _execution;
        private readonly bool _isRecurring;
        private int _disposed;

        internal ScheduledTaskHandle(Scheduler owner, CancellationTokenSource cancellationSource, Task execution, bool isRecurring)
        {
            _owner = owner;
            _cancellationSource = cancellationSource;
            _execution = execution;
            _isRecurring = isRecurring;
        }

        /// <summary>
        /// Indicates if the handle represents a recurring task.
        /// </summary>
        public bool IsRecurring => _isRecurring;

        /// <summary>
        /// Gets the task representing the scheduled execution loop.
        /// </summary>
        public Task Completion => _execution;

        /// <summary>
        /// Requests cancellation of the scheduled task.
        /// </summary>
        public void Cancel()
        {
            if (Volatile.Read(ref _disposed) != 0) {
                return;
            }

            _cancellationSource.Cancel();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) {
                return;
            }

            _cancellationSource.Cancel();
            _cancellationSource.Dispose();
            _owner.Unregister(this);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) {
                await _execution.ConfigureAwait(false);
                return;
            }

            _cancellationSource.Cancel();
            try {
                await _execution.ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (_execution.IsCanceled) {
            }
            finally {
                _cancellationSource.Dispose();
                _owner.Unregister(this);
            }
        }
    }
}

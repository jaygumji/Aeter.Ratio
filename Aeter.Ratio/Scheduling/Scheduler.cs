/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Scheduling
{
    /// <summary>
    /// Provides basic scheduling capabilities for immediate and recurring tasks.
    /// </summary>
    /// <remarks>
    /// The returned <see cref="ScheduledTaskHandle"/> deregisters automatically when the delegate completes, so storing it
    /// is only needed when you plan to cancel or await the execution. Recurring entries keep running until you dispose or cancel
    /// their handle, or until the owning <see cref="Scheduler"/> is disposed. Fire-and-forget usage is therefore as simple as
    /// calling one of the <see cref="Schedule(ScheduledTaskDelegate,object?,CancellationToken)"/> overloads without capturing
    /// the handle—the task will run to completion on its own.
    /// </remarks>
    public sealed class Scheduler : IDisposable, IAsyncDisposable
    {
        private readonly TimeProvider _timeProvider;
        private readonly CancellationTokenSource _shutdown = new();
        private readonly HashSet<ScheduledTaskHandle> _registrations = new();
        private readonly object _sync = new();
        private bool _disposed;

        public Scheduler()
            : this(TimeProvider.System)
        {
        }

        public Scheduler(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        /// <summary>
        /// Schedules a task to run immediately.
        /// </summary>
        public ScheduledTaskHandle Schedule(ScheduledTaskDelegate task, object? state = null, CancellationToken cancellationToken = default)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            ThrowIfDisposed();

            var linked = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token, cancellationToken);
            ScheduledTaskHandle handle;
            try {
                var execution = Task.Run(() => task(state, linked.Token), linked.Token);
                handle = new ScheduledTaskHandle(this, linked, execution, isRecurring: false);
                Register(handle);
                AttachCleanup(handle);
            }
            catch {
                linked.Dispose();
                throw;
            }

            return handle;
        }

        /// <summary>
        /// Schedules a task to run immediately.
        /// </summary>
        public ScheduledTaskHandle Schedule(Func<object?, Task> task, object? state = null, CancellationToken cancellationToken = default)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            return Schedule((s, _) => task(s), state, cancellationToken);
        }

        /// <summary>
        /// Schedules a task to run on a recurring basis.
        /// </summary>
        public ScheduledTaskHandle ScheduleRecurring(ScheduledTaskDelegate task, RecurringSchedule schedule, object? state = null, CancellationToken cancellationToken = default)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (schedule == null) throw new ArgumentNullException(nameof(schedule));

            ThrowIfDisposed();

            var linked = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token, cancellationToken);
            ScheduledTaskHandle handle;
            try {
                var execution = Task.Run(() => RunRecurringAsync(schedule, task, state, linked.Token), linked.Token);
                handle = new ScheduledTaskHandle(this, linked, execution, isRecurring: true);
                Register(handle);
                AttachCleanup(handle);
            }
            catch {
                linked.Dispose();
                throw;
            }

            return handle;
        }

        /// <summary>
        /// Schedules a task to run on a recurring basis.
        /// </summary>
        public ScheduledTaskHandle ScheduleRecurring(Func<object?, Task> task, RecurringSchedule schedule, object? state = null, CancellationToken cancellationToken = default)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            return ScheduleRecurring((s, _) => task(s), schedule, state, cancellationToken);
        }

        public void Dispose()
        {
            var handles = BeginDispose();
            if (handles is null) {
                return;
            }

            foreach (var handle in handles) {
                handle.Dispose();
            }

            _shutdown.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            var handles = BeginDispose();
            if (handles is null) {
                GC.SuppressFinalize(this);
                return;
            }

            foreach (var handle in handles) {
                await handle.DisposeAsync().ConfigureAwait(false);
            }

            _shutdown.Dispose();
            GC.SuppressFinalize(this);
        }

        internal void Unregister(ScheduledTaskHandle handle)
        {
            lock (_sync) {
                _registrations.Remove(handle);
            }
        }

        private async Task RunRecurringAsync(RecurringSchedule schedule, ScheduledTaskDelegate task, object? state, CancellationToken cancellationToken)
        {
            DateTimeOffset? lastExecution = null;

            while (true) {
                cancellationToken.ThrowIfCancellationRequested();
                var now = _timeProvider.GetLocalNow();
                var delay = schedule.GetNextDelay(now, lastExecution);
                if (delay > TimeSpan.Zero) {
                    await _timeProvider.DelayAsync(delay, cancellationToken).ConfigureAwait(false);
                }

                await task(state, cancellationToken).ConfigureAwait(false);
                lastExecution = _timeProvider.GetLocalNow();
            }
        }

        private void AttachCleanup(ScheduledTaskHandle handle)
        {
            _ = handle.Completion.ContinueWith(static (_, state) =>
            {
                var (scheduler, scheduledHandle) = ((Scheduler, ScheduledTaskHandle))state!;
                scheduler.Unregister(scheduledHandle);
            }, (this, handle), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private void Register(ScheduledTaskHandle handle)
        {
            lock (_sync) {
                if (_disposed) {
                    throw new ObjectDisposedException(nameof(Scheduler));
                }

                _registrations.Add(handle);
            }
        }

        private List<ScheduledTaskHandle>? BeginDispose()
        {
            lock (_sync) {
                if (_disposed) {
                    return null;
                }

                _disposed = true;
                var handles = new List<ScheduledTaskHandle>(_registrations);
                _registrations.Clear();
                _shutdown.Cancel();
                return handles;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) {
                throw new ObjectDisposedException(nameof(Scheduler));
            }
        }
    }
}

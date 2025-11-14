/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aeter.Ratio.Scheduling
{
    internal static class TimeProviderExtensions
    {
        public static Task DelayAsync(this TimeProvider provider, TimeSpan delay, CancellationToken cancellationToken = default)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            if (delay <= TimeSpan.Zero) {
                return Task.CompletedTask;
            }

            if (ReferenceEquals(provider, TimeProvider.System)) {
                return Task.Delay(delay, cancellationToken);
            }

            var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            ITimer? timer = null;
            CancellationTokenRegistration registration = default;
            var completed = 0;

            void Cleanup()
            {
                timer?.Dispose();
                registration.Dispose();
            }

            void CompleteSuccessfully()
            {
                if (Interlocked.Exchange(ref completed, 1) == 1) {
                    return;
                }

                Cleanup();
                source.TrySetResult();
            }

            void CompleteCanceled()
            {
                if (Interlocked.Exchange(ref completed, 1) == 1) {
                    return;
                }

                Cleanup();
                source.TrySetCanceled(cancellationToken);
            }

            if (cancellationToken.CanBeCanceled) {
                if (cancellationToken.IsCancellationRequested) {
                    CompleteCanceled();
                    return source.Task;
                }

                registration = cancellationToken.Register(CompleteCanceled);
            }

            timer = provider.CreateTimer(static state =>
            {
                var callback = (Action)state!;
                callback();
            }, (Action)CompleteSuccessfully, delay, Timeout.InfiniteTimeSpan);

            if (source.Task.IsCompleted) {
                timer.Dispose();
            }

            return source.Task;
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Aeter.Ratio.Scheduling;
using Xunit;

namespace Aeter.Ratio.Test.Scheduling
{
    public class SchedulerTests
    {
        [Fact]
        public async Task Schedule_RunsTaskWithProvidedState()
        {
            await using var scheduler = new Scheduler();
            var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var state = "payload";

            var handle = scheduler.Schedule(async (s, _) =>
            {
                tcs.TrySetResult((string?)s);
                await Task.CompletedTask;
            }, state);

            var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(1)));
            Assert.Same(tcs.Task, completed);
            Assert.Equal(state, await tcs.Task);

            await handle.DisposeAsync();
        }

        [Fact]
        public async Task ScheduleRecurring_InvokesDelegateMultipleTimes()
        {
            await using var scheduler = new Scheduler();
            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var count = 0;

            var handle = scheduler.ScheduleRecurring(async (_, _) =>
            {
                var invocation = Interlocked.Increment(ref count);
                if (invocation == 3) {
                    tcs.TrySetResult(invocation);
                }

                await Task.CompletedTask;
            }, RecurringSchedule.Interval(TimeSpan.FromMilliseconds(20)));

            var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(2)));
            Assert.Same(tcs.Task, completed);
            Assert.Equal(3, await tcs.Task);

            await handle.DisposeAsync();
        }

        [Fact]
        public async Task CancelRecurringTask_StopsFurtherInvocations()
        {
            await using var scheduler = new Scheduler();
            var firstInvocation = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var count = 0;

            var handle = scheduler.ScheduleRecurring(async (_, token) =>
            {
                Interlocked.Increment(ref count);
                firstInvocation.TrySetResult();

                try {
                    await gate.Task.WaitAsync(token);
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested) {
                }
            }, RecurringSchedule.Interval(TimeSpan.FromMilliseconds(10)));

            await firstInvocation.Task.WaitAsync(TimeSpan.FromSeconds(1));

            handle.Cancel();

            var completed = await Task.WhenAny(handle.Completion, Task.Delay(TimeSpan.FromSeconds(1)));
            Assert.Same(handle.Completion, completed);
            Assert.True(handle.Completion.IsCanceled);
            Assert.Equal(1, Volatile.Read(ref count));

            await handle.DisposeAsync();
        }
    }
}

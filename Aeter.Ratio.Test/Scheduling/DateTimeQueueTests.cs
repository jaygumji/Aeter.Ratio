using System;
using System.Linq;
using System.Threading.Tasks;
using Aeter.Ratio.Scheduling;
using Xunit;

namespace Aeter.Ratio.Test.Scheduling
{
    public class DateTimeQueueTests
    {
        [Fact]
        public void TryDequeue_ReturnsAllDueEntries()
        {
            var queue = new DateTimeQueue<string>();
            var due = DateTime.Now.AddMilliseconds(-50);
            queue.Enqueue(due, "first");
            queue.Enqueue(due, "second");

            var result = queue.TryDequeue(out var values);

            Assert.True(result);
            Assert.Equal(new[] { "first", "second" }, values);
            Assert.True(queue.IsEmpty);
        }

        [Fact]
        public void TryDequeue_ReturnsFalseForFutureEntries()
        {
            var queue = new DateTimeQueue<int>();
            var future = DateTime.Now.AddMinutes(1);
            queue.Enqueue(future, 42);

            var result = queue.TryDequeue(out var values);

            Assert.False(result);
            Assert.Null(values);
            Assert.True(queue.TryPeekNextEntryAt(out var next));
            Assert.Equal(future, next);
        }

        [Fact]
        public void TryPeekNextEntryAt_ReturnsEarliestTimestamp()
        {
            var queue = new DateTimeQueue<int>();
            var earlier = DateTime.Now.AddMinutes(-1);
            var later = DateTime.Now.AddMinutes(1);
            queue.Enqueue(later, 1);
            queue.Enqueue(earlier, 2);

            var result = queue.TryPeekNextEntryAt(out var next);

            Assert.True(result);
            Assert.Equal(earlier, next);
        }

        [Fact]
        public async Task Enqueue_FromMultipleThreads_IsThreadSafe()
        {
            var queue = new DateTimeQueue<int>();
            var due = DateTime.Now.AddMilliseconds(-10);
            var tasks = Enumerable.Range(0, 4)
                                  .Select(worker => Task.Run(() =>
                                  {
                                      for (var i = 0; i < 50; i++) {
                                          queue.Enqueue(due, worker * 100 + i);
                                      }
                                  }));

            await Task.WhenAll(tasks);

            var result = queue.TryDequeue(out var values);
            Assert.True(result);
            var materialized = values!.ToList();
            Assert.Equal(200, materialized.Count);
            Assert.True(queue.IsEmpty);
        }
    }
}

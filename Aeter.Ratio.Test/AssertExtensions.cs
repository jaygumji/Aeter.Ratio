using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Aeter.Ratio.Test
{
    public static class AssertExtensions
    {
        private static void Fail(string source, string reason)
        {
            Assert.True(false, string.Concat(source, ": ", reason));
        }

        public static void Equal<T>(IEnumerable<T>? left, IEnumerable<T>? right)
        {
            if (left == null && right == null) return;
            Assert.NotNull(left);
            Assert.NotNull(right);
#pragma warning disable CS8602, CS8604 // Dereference of a possibly null reference.
            var leftCount = left.Count();
            var rightCount = right.Count();
            if (leftCount != rightCount) Fail("Collection", "Count differ");
            if (!left.SequenceEqual(right)) Fail("Collection", "Sequence differ");
#pragma warning restore CS8602, CS8604 // Dereference of a possibly null reference.
        }
    }
}

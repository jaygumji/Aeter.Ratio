using System;
using System.Buffers;

namespace Aeter.Ratio.Binary.Algorithm
{
    internal static class LevenshteinDistance
    {
        public static int Compute(ReadOnlySpan<char> source, ReadOnlySpan<char> target, int maxDistance)
        {
            if (maxDistance < 0) throw new ArgumentOutOfRangeException(nameof(maxDistance));
            if (source.Length == 0) {
                return target.Length <= maxDistance ? target.Length : maxDistance + 1;
            }
            if (target.Length == 0) {
                return source.Length <= maxDistance ? source.Length : maxDistance + 1;
            }
            if (Math.Abs(source.Length - target.Length) > maxDistance) {
                return maxDistance + 1;
            }

            var buffer = ArrayPool<int>.Shared.Rent((target.Length + 1) * 3);
            try {
                var prevPrev = new Span<int>(buffer, 0, target.Length + 1);
                var previous = new Span<int>(buffer, target.Length + 1, target.Length + 1);
                var current = new Span<int>(buffer, (target.Length + 1) * 2, target.Length + 1);

                for (var j = 0; j <= target.Length; j++) {
                    previous[j] = j;
                    current[j] = 0;
                    prevPrev[j] = 0;
                }

                for (var i = 1; i <= source.Length; i++) {
                    current[0] = i;
                    var minInRow = current[0];

                    for (var j = 1; j <= target.Length; j++) {
                        var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                        var deletion = previous[j] + 1;
                        var insertion = current[j - 1] + 1;
                        var substitution = previous[j - 1] + cost;
                        var distance = Math.Min(Math.Min(deletion, insertion), substitution);

                        if (i > 1 && j > 1 && source[i - 1] == target[j - 2] && source[i - 2] == target[j - 1]) {
                            distance = Math.Min(distance, prevPrev[j - 2] + cost);
                        }

                        current[j] = distance;
                        if (distance < minInRow) {
                            minInRow = distance;
                        }
                    }

                    if (minInRow > maxDistance) {
                        return maxDistance + 1;
                    }

                    var temp = prevPrev;
                    prevPrev = previous;
                    previous = current;
                    current = temp;
                }

                var result = previous[target.Length];
                return result <= maxDistance ? result : maxDistance + 1;
            }
            finally {
                ArrayPool<int>.Shared.Return(buffer);
            }
        }
    }
}

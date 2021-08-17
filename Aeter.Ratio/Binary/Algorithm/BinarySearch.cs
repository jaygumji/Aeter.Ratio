/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;

namespace Aeter.Ratio.Binary.Algorithm
{
    /// <summary>
    /// Binary search algorithm
    /// </summary>
    public static class BinarySearch
    {
        public static int Search<T>(IList<T> list, T value)
        {
            return Search<T>(list, 0, list.Count, value, Comparer<T>.Default);
        }

        public static int Search<T>(IList<T> list, int index, int length, T value, IComparer<T> comparer)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            if (index < 0)
                throw new ArgumentOutOfRangeException();

            if (length < 0 || list.Count - index < length)
                throw new ArgumentException("Invalid length, must be a valid length between index and the size of the list");

            if (comparer == null) comparer = Comparer<T>.Default;

            return Search<T>(idx => list[idx], index, length, value, comparer);
        }

        public static int Search<T>(Func<int, T> valueAccessor, int index, int length, T value, IComparer<T> comparer)
        {
            if (valueAccessor == null) throw new ArgumentNullException(nameof(valueAccessor));

            if (index < 0)
                throw new ArgumentOutOfRangeException();

            if (comparer == null) comparer = Comparer<T>.Default;

            int left = index;
            int right = index + length - 1;
            while (left <= right)
            {
                int median = left + (right - left >> 1);
                var compareResult = comparer.Compare(valueAccessor.Invoke(median), value);
                if (compareResult == 0) return median;

                if (compareResult < 0)
                    left = median + 1;
                else
                    right = median - 1;
            }

            return ~left;
        }
    }

}

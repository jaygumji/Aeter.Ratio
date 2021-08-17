/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections.Generic;
using System.Linq;

namespace Aeter.Ratio
{
    public sealed class ListComparer<TList, TElement> : IComparer<TList>
        where TList : IReadOnlyList<TElement>
    {
        public static readonly ListComparer<TList, TElement> Default
            = new ListComparer<TList, TElement>();

        private readonly IComparer<TElement> _elementComparer;

        public ListComparer() : this(Comparer<TElement>.Default)
        {
        }

        public ListComparer(IComparer<TElement> elementComparer)
        {
            _elementComparer = elementComparer;
        }

        public int Compare(TList first, TList second)
        {
            if (ReferenceEquals(first, second)) {
                return 0;
            }
            if (first == null) {
                return -1;
            }
            if (second == null) {
                return 1;
            }
            if (first.Count != second.Count) {
                return first.Count - second.Count;
            }
            return (
                from cmp in first.Select((t, i) => _elementComparer.Compare(t, second[i]))
                where cmp != 0
                select cmp > 0 ? 1 : -1
            ).FirstOrDefault();
        }
    }
}
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections.Generic;
using System.Linq;

namespace Aeter.Ratio
{
    public sealed class ListEqualityComparer<TList, TElement> : IEqualityComparer<TList>
        where TList : IReadOnlyList<TElement>
    {

        public static readonly ListEqualityComparer<TList, TElement> Default
            = new ListEqualityComparer<TList, TElement>();

        private readonly IEqualityComparer<TElement> _elementEqualityComparer;

        public ListEqualityComparer()
            : this(EqualityComparer<TElement>.Default)
        {
        }

        public ListEqualityComparer(IEqualityComparer<TElement> elementEqualityComparer)
        {
            _elementEqualityComparer = elementEqualityComparer;
        }

        public bool Equals(TList? first, TList? second)
        {
            if (ReferenceEquals(first, second)) {
                return true;
            }
            if (first == null || second == null) {
                return false;
            }
            if (first.Count != second.Count) {
                return false;
            }
            return !first.Where((t, i) => !_elementEqualityComparer.Equals(t, second[i])).Any();
        }

        public int GetHashCode(TList list)
        {
            unchecked {
                if (list == null) {
                    return 0;
                }
                var hash = 17;
                for (var i = 0; i < list.Count; i++) {
                    hash = hash * 31 + _elementEqualityComparer.GetHashCode(list[i]!);
                }
                return hash;
            }
        }
    }
}
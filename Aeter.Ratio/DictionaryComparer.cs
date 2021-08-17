/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections.Generic;
using System.Linq;

namespace Aeter.Ratio
{
    public sealed class DictionaryComparer<TDictionary, TKey, TValue> : IComparer<TDictionary>
        where TDictionary : IReadOnlyDictionary<TKey, TValue>
    {
        public static readonly DictionaryComparer<TDictionary, TKey, TValue> Default
            = new DictionaryComparer<TDictionary, TKey, TValue>();

        private readonly IComparer<TValue> _valueComparer;

        public DictionaryComparer() : this(Comparer<TValue>.Default)
        {
        }

        public DictionaryComparer(IComparer<TValue> valueComparer)
        {
            _valueComparer = valueComparer;
        }

        public int Compare(TDictionary first, TDictionary second)
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
                from cmp in first.Select(t => _valueComparer.Compare(t.Value, second[t.Key]))
                where cmp != 0
                select cmp > 0 ? 1 : -1
            ).FirstOrDefault();
        }
    }
}
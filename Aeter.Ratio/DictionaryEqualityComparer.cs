/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections.Generic;
using System.Linq;

namespace Aeter.Ratio
{
    public sealed class DictionaryEqualityComparer<TDictionary, TKey, TValue> : IEqualityComparer<TDictionary>
        where TDictionary : IReadOnlyDictionary<TKey, TValue>
    {

        public static readonly DictionaryEqualityComparer<TDictionary, TKey, TValue> Default
            = new DictionaryEqualityComparer<TDictionary, TKey, TValue>();

        private readonly IEqualityComparer<TValue> _valueEqualityComparer;

        public DictionaryEqualityComparer()
            : this(EqualityComparer<TValue>.Default)
        {
        }

        public DictionaryEqualityComparer(IEqualityComparer<TValue> valueEqualityComparer)
        {
            _valueEqualityComparer = valueEqualityComparer;
        }

        public bool Equals(TDictionary first, TDictionary second)
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
            return first.All(t => _valueEqualityComparer.Equals(t.Value, second[t.Key]));
        }

        public int GetHashCode(TDictionary dictionary)
        {
            return dictionary.GetHashCode();
        }
    }
}
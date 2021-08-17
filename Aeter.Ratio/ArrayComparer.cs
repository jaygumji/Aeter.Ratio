/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections.Generic;

namespace Aeter.Ratio
{
    public sealed class ArrayComparer<T> : IComparer<T[]>
    {
        public static readonly ArrayComparer<T> Default = new ArrayComparer<T>();

        private readonly ListComparer<T[], T> _listComparer;

        public ArrayComparer() : this(Comparer<T>.Default)
        {
        }

        public ArrayComparer(IComparer<T> elementComparer)
        {
            _listComparer = new ListComparer<T[], T>(elementComparer);
        }

        public int Compare(T[] first, T[] second)
        {
            return _listComparer.Compare(first, second);
        }
    }
}
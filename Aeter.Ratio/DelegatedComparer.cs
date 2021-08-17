/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Collections;
using System.Collections.Generic;

namespace Aeter.Ratio
{
    public delegate int CompareHandler<in T>(T x, T y);

    public delegate int HashCodeHandler<in T>(T x);

    public class DelegatedComparer<T> : IComparer<T>, IComparer, IEqualityComparer<T>, IEqualityComparer
    {
        private readonly CompareHandler<T> _compareCallback;
        private readonly HashCodeHandler<T> _hashCodeHandler;

        public DelegatedComparer(CompareHandler<T> compareCallback)
        {
            _compareCallback = compareCallback;
        }

        public DelegatedComparer(CompareHandler<T> compareCallback, HashCodeHandler<T> hashCodeHandler)
        {
            _compareCallback = compareCallback;
            _hashCodeHandler = hashCodeHandler;
        }

        public int Compare(T x, T y)
        {
            return _compareCallback.Invoke(x, y);
        }

        int IComparer.Compare(object x, object y)
        {
            if (!(x is T)) {
                return -1;
            }
            if (!(y is T)) {
                return 1;
            }
            return Compare((T) x, (T) y);
        }

        public bool Equals(T x, T y)
        {
            return _compareCallback.Invoke(x, y) == 0;
        }

        public int GetHashCode(T obj)
        {
            return _hashCodeHandler?.Invoke(obj) ?? obj.GetHashCode();
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            if (!(x is T)) {
                return false;
            }
            if (!(y is T)) {
                return false;
            }
            return Equals((T)x, (T)y);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            if (!(obj is T)) {
                return obj.GetHashCode();
            }
            return GetHashCode((T) obj);
        }
    }
}

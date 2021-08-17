/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Linq;

namespace Aeter.Ratio.Collections
{
    public interface IQueryableSet : IQueryable
    {
    }

    public interface IQueryableSet<T> : IQueryable<T>, IQueryableSet
    {
        void Add(T entity);
        void Remove(T entity);

        T Get(object key);
        bool TryGet(object key, out T entity);
    }
}

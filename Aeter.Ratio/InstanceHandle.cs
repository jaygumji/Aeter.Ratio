/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio
{
    public class InstanceHandle<T> : IDisposable
        where T : class
    {
        private readonly InstancePool<T> _pool;
        public T Instance { get; }

        public InstanceHandle(InstancePool<T> pool, T instance)
        {
            _pool = pool;
            Instance = instance;
        }

        public void Dispose()
        {
            _pool.Release(this);
        }
    }
}
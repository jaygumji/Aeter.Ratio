/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Concurrent;
namespace Aeter.Ratio.Threading
{
    public class Lock : ILock
    {
        public ILockHandle Enter()
        {
            System.Threading.Monitor.Enter(this);
            return new LockHandle(this);
        }

        public bool TryEnter()
        {
            return System.Threading.Monitor.TryEnter(this);
        }

        public bool TryEnter(TimeSpan timeLimit)
        {
            return System.Threading.Monitor.TryEnter(this, timeLimit);
        }

        public void Exit()
        {
            System.Threading.Monitor.Exit(this);
        }
    }

    public class Lock<TKey> : ILock<TKey>
        where TKey : notnull
    {

        private readonly ConcurrentDictionary<TKey, ILock> _locks;

        public Lock()
        {
            _locks = new ConcurrentDictionary<TKey, ILock>();
        }

        public ILockHandle Enter(TKey key)
        {
            return _locks.GetOrAdd(key, k => new Lock()).Enter();
        }

        public bool TryEnter(TKey key)
        {
            return _locks.GetOrAdd(key, k => new Lock()).TryEnter();
        }

        public bool TryEnter(TKey key, TimeSpan timeLimit)
        {
            return _locks.GetOrAdd(key, k => new Lock()).TryEnter(timeLimit);
        }

        public void Exit(TKey key)
        {
            if (_locks.TryGetValue(key, out var keyLock))
                keyLock.Exit();
        }
    }
}

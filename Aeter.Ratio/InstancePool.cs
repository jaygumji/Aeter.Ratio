/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;

namespace Aeter.Ratio
{
    public delegate T InstanceFactoryHandler<out T>(object? state);

    public class InstancePool<T>
        where T : class
    {
        private readonly InstanceFactoryHandler<T> _factory;
        private readonly LinkedList<T> _free;
        private readonly Dictionary<T, LinkedListNode<T>> _nodeLookup;
        private readonly HashSet<T> _inUse;

        public InstancePool(InstanceFactoryHandler<T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _free = new LinkedList<T>();
            _nodeLookup = new Dictionary<T, LinkedListNode<T>>();
            _inUse = new HashSet<T>();
        }

        public InstanceHandle<T> Get()
        {
            return Get(null);
        }

        public InstanceHandle<T> Get(object? state)
        {
            T instance;
            lock (_free)
            {
                if (_free.Count > 0)
                {
                    instance = _free.Last!.Value;
                    _free.RemoveLast();
                    _nodeLookup.Remove(instance);
                }
                else
                {
                    instance = _factory.Invoke(state);
                }
            }

            var handle = new InstanceHandle<T>(this, instance);
            lock (_inUse)
            {
                _inUse.Add(instance);
            }
            return handle;
        }

        public void Release(InstanceHandle<T> handle)
        {
            var instance = handle.Instance;
            lock (_inUse)
            {
                if (!_inUse.Remove(instance))
                {
                    throw new ArgumentException("The handle is not available to release from this instance pool.");
                }
            }

            lock (_free)
            {
                var node = _free.AddLast(instance);
                _nodeLookup.Add(instance, node);
            }
        }

        public bool TryDiscard(T instance)
        {
            lock (_free)
            {
                if (!_nodeLookup.TryGetValue(instance, out var node))
                {
                    return false;
                }
                _free.Remove(node);
                _nodeLookup.Remove(instance);
                return true;
            }
        }
    }
}

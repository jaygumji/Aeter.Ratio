/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Aeter.Ratio.Scheduling;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Aeter.Ratio.Caching
{
    public class MemoryCache<TKey, TValue> : ICache<TKey, TValue>, IDisposable
        where TKey : notnull
        where TValue : notnull
    {
        private static readonly TimeSpan Disabled = new TimeSpan(0, 0, 0, 0, -1);

        private readonly ICachePolicy _policy;
        private readonly Timer _timer;
        private readonly DateTimeQueue<TKey> _queue;
        private readonly Dictionary<TKey, CacheContent<TKey, TValue>> _contents;
        private DateTime _nextTimerExecution;
        private readonly ReaderWriterLockSlim _rwLock;

        public event EventHandler<ExpiredEventArgs<TKey, TValue>>? Expired;

        public MemoryCache(ICachePolicy policy) : this(policy, EqualityComparer<TKey>.Default)
        {
        }

        public MemoryCache(ICachePolicy policy, IEqualityComparer<TKey> keyComparer)
        {
            _policy = policy;
            _timer = new Timer(CacheValidationCallback, null, Disabled, Disabled);
            _queue = new DateTimeQueue<TKey>();
            _contents = new Dictionary<TKey, CacheContent<TKey, TValue>>(keyComparer);
            _nextTimerExecution = DateTime.MaxValue;
            _rwLock = new ReaderWriterLockSlim();
        }

        private void CacheValidationCallback(object? state)
        {
            _rwLock.EnterWriteLock();
            try
            {
                while (_queue.TryDequeue(out var keys))
                {
                    foreach (var key in keys)
                    {
                        if (!_contents.TryGetValue(key, out var content)) continue;

                        if (content.Validate())
                        {
                            var expiresAt = content.ExpiresAt;
                            if (expiresAt != DateTime.MaxValue)
                                _queue.Enqueue(content.ExpiresAt, key);

                            continue;
                        }

                        if (Expired != null)
                        {
                            var args = new ExpiredEventArgs<TKey, TValue>(content);
                            Expired.Invoke(this, args);
                            if (args.CancelEviction)
                            {
                                content.Refresh();
                                continue;
                            }
                        }
                        _contents.Remove(key);
                    }
                }
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }

            if (_queue.TryPeekNextEntryAt(out var nextAt))
            {
                SetTimer(DateTime.MaxValue, nextAt);
            }
        }

        private void SetTimer(DateTime currentAt, DateTime expiresAt)
        {
            if (currentAt <= expiresAt) return;
            lock (_timer)
            {
                if (currentAt <= expiresAt) return;

                _nextTimerExecution = expiresAt;
                _timer.Change(expiresAt.Subtract(DateTime.Now), Disabled);
            }
        }

        private void UpdateExpiryControl(DateTime expiresAt, TKey key)
        {
            if (expiresAt == DateTime.MaxValue) return;
            _queue.Enqueue(expiresAt, key);
            SetTimer(_nextTimerExecution, expiresAt);
        }

        public void Dispose()
        {
            _timer.Dispose();
            _rwLock.Dispose();
            Expired = null;
        }

        void ICache.Set(object key, object content)
        {
            ((ICache)this).Set(key, content, _policy);
        }

        void ICache.Set(object key, object content, ICachePolicy policy)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (content == null) throw new ArgumentNullException("content");

            if (!(key is TKey))
                throw new ArgumentException("The parameter key must be of type " + typeof(TKey).FullName);

            if (!(content is TValue))
                throw new ArgumentException("The parameter value must be of type " + typeof(TValue).FullName);

            Set((TKey)key, (TValue)content, policy);
        }

        object ICache.TrySet(object key, Func<object, object> contentGetter)
        {
            return ((ICache)this).TrySet(key, contentGetter, _policy);
        }

        object ICache.TrySet(object key, Func<object, object> contentGetter, ICachePolicy policy)
        {
            if (key is not TKey tkey)
                throw new ArgumentException("The parameter key must be of type " + typeof(TKey).FullName);

            return TrySet(tkey, k => {
                var content = contentGetter(key);

                if (content is not TValue value)
                    throw new ArgumentException("The value must be of type " + typeof(TValue).FullName);

                return value;
            });
        }

        object ICache.Get(object key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (key is not TKey tkey)
                throw new ArgumentException("The parameter key must be of type " + typeof(TKey).FullName);

            return Get(tkey);
        }

        bool ICache.TryGet(object key, [MaybeNullWhen(false)] out object content)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (key is not TKey tkey)
                throw new ArgumentException("The parameter key must be of type " + typeof(TKey).FullName);

            var res = TryGet(tkey, out var typedContent);

            content = typedContent;
            return res;
        }

        public void Set(TKey key, TValue value)
        {
            Set(key, value, _policy);
        }

        public void Set(TKey key, TValue value, ICachePolicy policy)
        {
            _rwLock.EnterWriteLock();
            try
            {
                var cached = new CacheContent<TKey, TValue>(key, value, policy);
                if (_contents.ContainsKey(key))
                {
                    _contents[key] = cached;
                }
                else
                {
                    _contents.Add(key, cached);
                }
                UpdateExpiryControl(cached.ExpiresAt, key);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public TValue TrySet(TKey key, Func<TKey, TValue> valueGetter)
        {
            return TrySet(key, valueGetter, _policy);
        }

        public TValue TrySet(TKey key, Func<TKey, TValue> valueGetter, ICachePolicy policy)
        {
            _rwLock.EnterReadLock();
            try
            {
                if (_contents.TryGetValue(key, out var cached))
                {
                    return cached.Value;
                }
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            _rwLock.EnterWriteLock();
            try
            {
                if (_contents.TryGetValue(key, out var cached))
                {
                    return cached.Value;
                }

                cached = new CacheContent<TKey, TValue>(key, valueGetter(key), policy);
                UpdateExpiryControl(cached.ExpiresAt, key);
                return cached.Value;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public TValue Get(TKey key)
        {
            _rwLock.EnterReadLock();
            try
            {
                if (!_contents.TryGetValue(key, out var cached))
                    throw new KeyNotFoundException("The given key was not found in the cache");

                cached.Touch();
                return cached.Value;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue content)
        {
            _rwLock.EnterReadLock();
            try
            {
                if (!_contents.TryGetValue(key, out var cached)) {
                    content = default;
                    return false;
                }
                cached.Touch();
                content = cached.Value;
                return true;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

    }
}

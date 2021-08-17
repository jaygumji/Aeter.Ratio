/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Caching
{
    internal class CacheContent<TKey, TContent> : ICacheContent<TKey, TContent>
    {
        private readonly ICachePolicy _policy;

        public CacheContent(TKey key, TContent value, ICachePolicy policy)
        {
            Key = key;
            Value = value;
            _policy = policy;
            CreatedAt = DateTime.Now;
            LastAccessedAt = DateTime.Now;
        }

        public DateTime CreatedAt { get; private set; }

        public DateTime LastAccessedAt { get; private set; }

        public TKey Key { get; }
        public TContent Value { get; }
        public DateTime ExpiresAt => _policy.CalculateExpiration(CreatedAt, LastAccessedAt);

        public bool Validate()
        {
            var expiresAt = _policy.CalculateExpiration(CreatedAt, LastAccessedAt);
            return expiresAt >= DateTime.Now;
        }

        public void Touch()
        {
            LastAccessedAt = DateTime.Now;
        }

        public void Refresh()
        {
            CreatedAt = DateTime.Now;
            LastAccessedAt = DateTime.Now;
        }
    }
}
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Caching
{
    public class CachePolicy : ICachePolicy
    {

        public static readonly ICachePolicy Infinite = new InfiniteCachePolicy();

        public CacheMode Mode { get; set; }
        public TimeSpan Expiration { get; set; }

        public DateTime CalculateExpiration(DateTime createdAt, DateTime lastAccessedAt)
        {
            return Mode == CacheMode.Absolute
                ? createdAt.Add(Expiration)
                : lastAccessedAt.Add(Expiration);
        }

        public static CachePolicy Absolute(TimeSpan expiration)
        {
            return new CachePolicy {Mode = CacheMode.Absolute, Expiration = expiration};
        }

        public static CachePolicy Sliding(TimeSpan expiration)
        {
            return new CachePolicy {Mode = CacheMode.Sliding, Expiration = expiration};
        }

    }
}
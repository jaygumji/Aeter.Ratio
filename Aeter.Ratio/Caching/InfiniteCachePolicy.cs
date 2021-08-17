/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Caching
{
    internal class InfiniteCachePolicy : ICachePolicy
    {
        public DateTime CalculateExpiration(DateTime createdAt, DateTime lastAccessedAt)
        {
            return DateTime.MaxValue;
        }
    }
}
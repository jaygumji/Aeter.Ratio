/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Caching
{
    public interface ICacheContent<out TKey, out TContent>
    {
        DateTime CreatedAt { get; }
        DateTime LastAccessedAt { get; }
        TKey Key { get; }
        TContent Value { get; }
        DateTime ExpiresAt { get; }
    }
}
/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Caching
{
    /// <summary>
    /// Thrown when requesting a content with a key that didn't exist in the cache
    /// </summary>
    public class CachedContentNotFoundException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="CachedContentNotFoundException"/>
        /// </summary>
        /// <param name="key">The requested cache key</param>
        public CachedContentNotFoundException(object key) : base(string.Format("Value was not found, supplied key was {0}", key)) { }
    }
}
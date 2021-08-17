/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
namespace Aeter.Ratio.Caching
{
    public class ExpiredEventArgs<TKey, TContent>
    {
        public ICacheContent<TKey, TContent> Content { get; }
        public bool CancelEviction { get; set; }

        public ExpiredEventArgs(ICacheContent<TKey, TContent> content)
        {
            Content = content;
        }
    }
}
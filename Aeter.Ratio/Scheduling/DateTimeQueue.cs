/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Aeter.Ratio.Scheduling
{
    public class DateTimeQueue<T>
    {

        private volatile int _count;
        private readonly SortedDictionary<DateTime, List<T>> _entries; 

        public DateTimeQueue()
        {
            _entries = new SortedDictionary<DateTime, List<T>>();
            _count = 0;
        }

        public bool IsEmpty => _count == 0;

        public void Enqueue(DateTime when, T value)
        {
            if (!_entries.TryGetValue(when, out var list)) {
                lock (_entries) {
                    if (!_entries.TryGetValue(when, out list)) {
                        list = new List<T>();
                        _entries.Add(when, list);
                    }
                }
            }
            lock (list) {
                list.Add(value);
            }
            _count++;
        }

        public bool TryDequeue([MaybeNullWhen(false)] out IEnumerable<T> values)
        {
            if (_count == 0) {
                values = null;
                return false;
            }

            var now = DateTime.Now;
            KeyValuePair<DateTime, List<T>> kv;
            lock (_entries) {
                if (_entries.Count == 0) {
                    values = null;
                    return false;
                }

                kv = _entries.First();
                if (kv.Key > now) {
                    values = null;
                    return false;
                }
                _entries.Remove(kv.Key);
            }

            values = kv.Value;
            _count -= kv.Value.Count;
            return true;
        }

        public bool TryPeekNextEntryAt(out DateTime nextAt)
        {
            if (_count == 0) {
                nextAt = default(DateTime);
                return false;
            }

            KeyValuePair<DateTime, List<T>> kv;
            lock (_entries) {
                if (_entries.Count == 0) {
                    nextAt = default(DateTime);
                    return false;
                }

                kv = _entries.First();
            }

            nextAt = kv.Key;
            return true;
        }

    }
}

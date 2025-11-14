/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Aeter.Ratio.Scheduling
{
    public class DateTimeQueue<T>
    {
        private readonly object _sync = new();
        private readonly SortedDictionary<DateTime, Queue<T>> _entries = new();
        private int _count;

        public bool IsEmpty => Volatile.Read(ref _count) == 0;

        public void Enqueue(DateTime when, T value)
        {
            lock (_sync) {
                if (!_entries.TryGetValue(when, out var queue)) {
                    queue = new Queue<T>();
                    _entries.Add(when, queue);
                }
                queue.Enqueue(value);
                Interlocked.Increment(ref _count);
            }
        }

        public bool TryDequeue([MaybeNullWhen(false)] out IEnumerable<T> values)
        {
            values = null;
            if (IsEmpty) {
                return false;
            }

            Queue<T> queue;
            int removedCount;
            var now = DateTime.Now;
            lock (_sync) {
                if (!TryGetFirstEntry(out var entry)) {
                    return false;
                }

                if (entry.Key > now) {
                    return false;
                }

                queue = entry.Value;
                removedCount = queue.Count;
                _entries.Remove(entry.Key);
            }

            values = queue;
            Interlocked.Add(ref _count, -removedCount);
            return true;
        }

        public bool TryPeekNextEntryAt(out DateTime nextAt)
        {
            nextAt = default;
            if (IsEmpty) {
                return false;
            }

            lock (_sync) {
                if (!TryGetFirstEntry(out var entry)) {
                    return false;
                }

                nextAt = entry.Key;
                return true;
            }
        }

        private bool TryGetFirstEntry(out KeyValuePair<DateTime, Queue<T>> entry)
        {
            var enumerator = _entries.GetEnumerator();
            if (!enumerator.MoveNext()) {
                entry = default;
                return false;
            }

            entry = enumerator.Current;
            return true;
        }
    }
}

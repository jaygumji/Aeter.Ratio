/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Aeter.Ratio.Scheduling
{
    public class ScheduleQueue
    {

        private readonly DateTimeQueue<IScheduledEntry> _entries;

        public ScheduleQueue()
        {
            _entries = new DateTimeQueue<IScheduledEntry>();
        }

        public void Enqueue(IScheduledEntry entry)
        {
            _entries.Enqueue(entry.When.GetNext(DateTime.Now), entry);
        }

        public bool TryDequeue([MaybeNullWhen(false)] out IEnumerable<IScheduledEntry> entries)
        {
            return _entries.TryDequeue(out entries);
        }

        public bool TryPeekNextEntryAt(out DateTime nextAt)
        {
            return _entries.TryPeekNextEntryAt(out nextAt);
        }

    }
}

/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Scheduling
{
    public class IntervalConfiguration : IDateTimeConfiguration
    {
        public IntervalConfiguration(IDateConfiguration date, TimeSpan interval)
        {
            Date = date;
            Interval = interval;
        }

        public IDateConfiguration Date { get; }
        public TimeSpan Interval { get; }

        public DateTime GetNext(DateTime @from)
        {
            var nextAt = from.Add(Interval);
            var nextDate = Date.NextAt(nextAt);
            return nextDate.Date == nextAt.Date
                ? nextAt
                : nextDate.Date;
        }
    }
}
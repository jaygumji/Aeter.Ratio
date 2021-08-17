/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Scheduling
{
    public class DailyConfiguration : IDateConfiguration
    {

        public DayOfWeekOption DayOfWeek { get; set; }

        DateTime IDateConfiguration.NextAt(DateTime @from)
        {
            if (DayOfWeek == DayOfWeekOption.None)
                throw new InvalidSchedulerConfigurationException("None is not valid configuration");

            if (DayOfWeek == DayOfWeekOption.AllDays)
                return from;

            if (DayOfWeek.Contains(from.DayOfWeek))
                return from;

            var dt = from;
            for (var i = 0; i < 6; i++) {
                dt = dt.AddDays(1);
                if (DayOfWeek.Contains(dt.DayOfWeek))
                    return dt.Date;
            }

            throw new InvalidSchedulerConfigurationException("Could not find the next valid date of this daily configuration");
        }
    }
}
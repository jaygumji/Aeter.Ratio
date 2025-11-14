/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;

namespace Aeter.Ratio.Scheduling
{
    /// <summary>
    /// Describes when a recurring task should be executed.
    /// </summary>
    public abstract class RecurringSchedule
    {
        /// <summary>
        /// Schedules a task to run every day at the specified time of day.
        /// </summary>
        public static RecurringSchedule Daily(TimeOnly timeOfDay)
            => new DailyRecurringSchedule(timeOfDay);

        /// <summary>
        /// Schedules a task to run every week on the specified day and time.
        /// </summary>
        public static RecurringSchedule Weekly(DayOfWeek dayOfWeek, TimeOnly timeOfDay)
            => new WeeklyRecurringSchedule(dayOfWeek, timeOfDay);

        /// <summary>
        /// Schedules a task to run every month on the specified day and time.
        /// </summary>
        public static RecurringSchedule Monthly(int dayOfMonth, TimeOnly timeOfDay)
            => new MonthlyRecurringSchedule(dayOfMonth, timeOfDay);

        /// <summary>
        /// Schedules a task to run at a fixed interval.
        /// </summary>
        public static RecurringSchedule Interval(TimeSpan interval)
            => new IntervalRecurringSchedule(interval);

        internal abstract TimeSpan GetNextDelay(DateTimeOffset now, DateTimeOffset? lastExecution);

        private sealed class IntervalRecurringSchedule : RecurringSchedule
        {
            private readonly TimeSpan _interval;

            public IntervalRecurringSchedule(TimeSpan interval)
            {
                if (interval <= TimeSpan.Zero) {
                    throw new ArgumentOutOfRangeException(nameof(interval), interval, "Interval must be greater than zero.");
                }

                _interval = interval;
            }

            internal override TimeSpan GetNextDelay(DateTimeOffset now, DateTimeOffset? lastExecution)
            {
                var anchor = lastExecution ?? now;
                var next = anchor + _interval;
                var delay = next - now;
                return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
            }
        }

        private abstract class CalendarRecurringSchedule : RecurringSchedule
        {
            protected CalendarRecurringSchedule(TimeOnly timeOfDay)
            {
                TimeOfDay = timeOfDay;
            }

            protected TimeOnly TimeOfDay { get; }

            protected DateTimeOffset BuildOccurrence(DateTimeOffset reference)
            {
                var span = TimeOfDay.ToTimeSpan();
                var midnight = new DateTimeOffset(reference.Year, reference.Month, reference.Day, 0, 0, 0, reference.Offset);
                return midnight + span;
            }
        }

        private sealed class DailyRecurringSchedule : CalendarRecurringSchedule
        {
            public DailyRecurringSchedule(TimeOnly timeOfDay)
                : base(timeOfDay)
            {
            }

            internal override TimeSpan GetNextDelay(DateTimeOffset now, DateTimeOffset? lastExecution)
            {
                var candidate = BuildOccurrence(now);
                if (candidate <= now) {
                    candidate = candidate.AddDays(1);
                }

                return candidate - now;
            }
        }

        private sealed class WeeklyRecurringSchedule : CalendarRecurringSchedule
        {
            private readonly DayOfWeek _dayOfWeek;

            public WeeklyRecurringSchedule(DayOfWeek dayOfWeek, TimeOnly timeOfDay)
                : base(timeOfDay)
            {
                _dayOfWeek = dayOfWeek;
            }

            internal override TimeSpan GetNextDelay(DateTimeOffset now, DateTimeOffset? lastExecution)
            {
                var candidate = BuildOccurrence(now);
                var daysToAdd = ((int)_dayOfWeek - (int)now.DayOfWeek + 7) % 7;
                candidate = candidate.AddDays(daysToAdd);

                if (candidate <= now) {
                    candidate = candidate.AddDays(7);
                }

                return candidate - now;
            }
        }

        private sealed class MonthlyRecurringSchedule : CalendarRecurringSchedule
        {
            private readonly int _dayOfMonth;

            public MonthlyRecurringSchedule(int dayOfMonth, TimeOnly timeOfDay)
                : base(timeOfDay)
            {
                if (dayOfMonth < 1 || dayOfMonth > 31) {
                    throw new ArgumentOutOfRangeException(nameof(dayOfMonth), dayOfMonth, "Day of month must be between 1 and 31.");
                }

                _dayOfMonth = dayOfMonth;
            }

            internal override TimeSpan GetNextDelay(DateTimeOffset now, DateTimeOffset? lastExecution)
            {
                var candidate = BuildOccurrenceForMonth(now.Year, now.Month, now.Offset);
                if (candidate <= now) {
                    var next = now.AddMonths(1);
                    candidate = BuildOccurrenceForMonth(next.Year, next.Month, next.Offset);
                }

                return candidate - now;
            }

            private DateTimeOffset BuildOccurrenceForMonth(int year, int month, TimeSpan offset)
            {
                var daysInMonth = DateTime.DaysInMonth(year, month);
                var day = Math.Min(_dayOfMonth, daysInMonth);
                var span = TimeOfDay.ToTimeSpan();
                var midnight = new DateTimeOffset(year, month, day, 0, 0, 0, offset);
                return midnight + span;
            }
        }
    }
}

using Pomodoro.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Pomodoro.Models
{
    public static class StatisticUtils
    {
        public static DayStatistic GetDayStatistic(this IEnumerable<PeriodEntry> data, DateTime date)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            var dayEntries = data
                .Where(d => d.StartTime.CompareTo(dayStart) >= 0)
                .Where(d => d.StartTime.CompareTo(dayEnd) < 0);

            return new DayStatistic()
            {
                Date = dayStart,
                StudyDuration = dayEntries.Where(e => e.IsStudying && !e.IsPaused).Select(e => e.Duration.TotalHours).Sum(),
                BreakDuration = dayEntries.Where(e => !e.IsStudying && !e.IsPaused).Select(e => e.Duration.TotalHours).Sum(),
            };
        }

        public static IEnumerable<DayStatistic> GetStatistics(this IEnumerable<PeriodEntry> data)
        {
            // Using the actual max value would lead to an exception as
            // our GetStatistics() function adds a day to ensure that we catch all periods
            // that happened on the last day
            return data.GetStatistics(DateTime.MinValue, DateTime.MaxValue.AddDays(-1));
        }

        public static IEnumerable<DayStatistic> GetStatistics(this IEnumerable<PeriodEntry> data, DateTime start, DateTime end)
        {
            // Determine first and last day of query
            var firstDay = start.Date;
            var lastDay = end.Date;

            // Fully include the last day
            lastDay = lastDay.AddDays(1);

            var filtered = data
                .Where(d => d.StartTime.CompareTo(firstDay) >= 0)
                .Where(d => d.StartTime.CompareTo(lastDay) < 0);

            // Filter data and get statistic for each day
            return data
                .Where(d => d.StartTime.CompareTo(firstDay) >= 0)
                .Where(d => d.StartTime.CompareTo(lastDay) < 0)
                .GroupBy(
                    e => e.StartTime.Date,
                    (dt, entries) => entries.GetDayStatistic(dt)
                );
        }
    }
}

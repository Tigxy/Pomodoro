#undef POPULATE_DATABASE

using Pomodoro.DB;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomodoro.Models
{
    public class StatsModel : BaseModel
    {
        private WpfPlot _plotControl;
        public WpfPlot PlotControl
        {
            get => _plotControl;
            set { _plotControl = value; OnPropertyChanged(nameof(PlotControl)); }
        }

        private static StatsModel _instance;
        /// <summary>
        /// Singleton instance of <see cref="StatsModel"/>
        /// </summary>
        public static StatsModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (new object())
                    {
                        if (_instance == null)
                            _instance = new StatsModel();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="StatsModel"/>
        /// </summary>
        private StatsModel()
        {
#if POPULATE_DATABASE
            // Allow playing around with new data (e.g. while developping)
            PopulateDB();
#endif
        }

        /// <summary>
        /// Refreshes the displayed plot
        /// </summary>
        public void RefreshPlot()
        {
            // Sadly ScottPlot does not support content bindings, therefore we need
            // to break MVVM by instantiating UI controls in model
            // see https://github.com/ScottPlot/ScottPlot/issues/494
            PlotControl = new WpfPlot(new Plot())
            {
                IsManipulationEnabled = false,
                IsEnabled = false
            };

            var data = DBAccess.LoadPeriodEntries(DateTime.Now.AddDays(-7));
            var st = data.GetStatistics();

            // Ensure that current day is encluded in the last 7 days view
            if (!st.Any(s => s.Date.Date.Equals(DateTime.UtcNow.Date)))
                st = st.Concat(new[] { new DayStatistic() { Date = DateTime.UtcNow.Date }});

            GenerateBarPlot(PlotControl.plt, st);
            PlotControl.UpdateLayout();
        }

        /// <summary>
        /// Adds a bar chart to the specified <see cref="Plot"/>
        /// </summary>
        /// <param name="plt">The <see cref="Plot"/> to add a bar chart to</param>
        /// <param name="statistics">The data to use</param>
        private void GenerateBarPlot(Plot plt, IEnumerable<DayStatistic> statistics)
        {
            var latest = statistics.Max(s => s.Date);

            // Order list such that last day of records is at the start
            var sorted = statistics.OrderByDescending(s => s.Date);

            // Calculate the day span of the statistics
            var day_count = latest.Subtract(statistics.Min(s => s.Date)).Days + 1;
            double[] days = Enumerable.Range(1, day_count).Select(i => (double)i).ToArray();
            double[] studyDurations = new double[day_count];
            double[] breakDurations = new double[day_count];

            foreach (var s in sorted)
            {
                studyDurations[latest.Date.Subtract(s.Date).Days] = s.StudyDuration;
                breakDurations[latest.Date.Subtract(s.Date).Days] = s.BreakDuration + s.StudyDuration;
            }

            plt.PlotBar(days, breakDurations, label: "Break duration");
            plt.PlotBar(days, studyDurations, label: "Study duration");

            // tick '0' should not have a day
            var lastDayNames = new[] { "" }.Concat(sorted.Select(s => s.Date.Date.DayOfWeek.ToString())).ToArray();
            lastDayNames[1] = "Today";
            plt.XTicks(lastDayNames);
            plt.YLabel("hours");

            plt.Legend(location: legendLocation.upperRight);
        }

#if POPULATE_DATABASE
        /// <summary>
        /// Populates the data base to have nice data to work with
        /// </summary>
        private void PopulateDB()
        {
            var dayCount = 7;
            var generationProfile = new Profile();
            var start = DateTime.Now.Subtract(TimeSpan.FromDays(dayCount));
            var end = DateTime.Now;
            var currentDay = start.Date;
            var lastDay = end.Date;

            // First collect all entries and then add them
            var entries = new List<PeriodEntry>();

            var rng = new System.Random();
            while (currentDay.CompareTo(lastDay) <= 0)
            {
                // Randomly start our dummy study session
                currentDay = currentDay.Add(new TimeSpan(hours: rng.Next(6, 22), minutes: rng.Next(0, 59), seconds: rng.Next(0, 59)));

                var dayEntryCount = rng.Next(3, 14);
                for (int i = 0; i < dayEntryCount; i++)
                {
                    var duration = TimeSpan.FromMinutes(i % 2 == 0 ? generationProfile.DurationStudying : generationProfile.DurationShortBreak);
                    entries.Add(new PeriodEntry()
                    {
                        StartTime = currentDay,
                        Duration = duration,
                        IsStudying = i % 2 == 0,
                        IsPaused = rng.NextDouble() < 0.1     // additional probability that entry is due to a pause, 
                    });
                    currentDay = currentDay.Add(duration);
                }

                // Reset the time
                currentDay = currentDay.Date;
                // Increase day while resetting the time
                currentDay = currentDay.AddDays(1);
            }

            DBAccess.SavePeriodEntry(entries);
        }
#endif
    }
}

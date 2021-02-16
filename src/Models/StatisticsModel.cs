#undef POPULATE_DATABASE

#if POPULATE_DATABASE
using Pomodoro.DB;
#endif
using ScottPlot;
using ScottPlot.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Pomodoro.Models
{
    public class StatisticsModel : BaseModel
    {
        private WpfPlot _plotControl;
        public WpfPlot PlotControl
        {
            get => _plotControl;
            set { _plotControl = value; OnPropertyChanged(nameof(PlotControl)); }
        }

        private static StatisticsModel _instance;
        /// <summary>
        /// Singleton instance of <see cref="StatisticsModel"/>
        /// </summary>
        public static StatisticsModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (new object())
                    {
                        if (_instance == null)
                            _instance = new StatisticsModel();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="StatisticsModel"/>
        /// </summary>
        private StatisticsModel()
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
            // Order list such that last day of records is at the start
            var sorted = statistics
                .OrderBy(s => s.Date)
                .Where(s => !s.Date.Date.Equals(DateTime.UtcNow.Date));

            // Handle 'today' differently; even if no data exists yet, it should still be displayed
            var todaysData = statistics.Where(s => s.Date.Date == DateTime.UtcNow.Date).FirstOrDefault() 
                ?? new DayStatistic() { Date = DateTime.UtcNow.Date };

            int dayCount = 0;
            if (sorted.Count() > 0)
            {
                // Determine the latest date (besides 'today')
                var latest = sorted.Last().Date;

                // Calculate the day span of the statistics
                dayCount = latest.Subtract(statistics.Min(s => s.Date)).Days + 1;
                double[] days = Enumerable.Range(1, dayCount).Select(i => (double)i).ToArray();
                double[] studyDurations = new double[dayCount];
                double[] breakDurations = new double[dayCount];

                foreach (var s in sorted)
                {
                    studyDurations[latest.Date.Subtract(s.Date).Days] = s.StudyDuration;
                    breakDurations[latest.Date.Subtract(s.Date).Days] = s.BreakDuration + s.StudyDuration;
                }

                plt.PlotBar(days, breakDurations, label: "Break duration", fillColor: LoadColor("BoxplotColorBreak"), outlineWidth: 0);
                plt.PlotBar(days, studyDurations, label: "Study duration", fillColor: LoadColor("BoxplotColorStudying"), outlineWidth: 0);
            }

            plt.PlotBar(new[] { dayCount + 1.0 }, new[] { todaysData.BreakDuration + todaysData.StudyDuration }, 
                fillColor: LoadColor("BoxplotColorBreakToday"), outlineColor: Color.Transparent);
            plt.PlotBar(new[] { dayCount + 1.0 }, new[] { todaysData.StudyDuration }, 
                fillColor: LoadColor("BoxplotColorStudyingToday"), outlineColor: Color.Transparent);

            var maxDuration = statistics.Count() > 0 ? statistics.Max(s => s.StudyDuration + s.BreakDuration) : 0;
            // Set minimum and maximum (otherwise plot looks ugly if only a few minutes of data exist)
            plt.Axis(y1: 0, y2: Math.Max(4, maxDuration * 1.2));

            // We also have to provide a label for x=0
            var dayNames = new List<string> { "", "Today" };
            dayNames.InsertRange(1, sorted.Select(s => s.Date.Date.DayOfWeek.ToString()));

            plt.Grid(enableVertical: false);
            plt.XTicks(dayNames.ToArray());
            plt.YLabel("hours");
            plt.Legend(location: legendLocation.upperRight);
            //plt.Style(figBg: Color.Transparent, dataBg: Color.White, tick: Color.White, title: Color.White, label: Color.White);

            plt.Style(PageManager.Instance.IsDarkThemeUsed ? Style.Gray1 : Style.Light1);
            plt.Colorset(colorset: PageManager.Instance.IsDarkThemeUsed ? Colorset.OneHalfDark : Colorset.OneHalf);
        }

        private Color LoadColor(string resourceKey)
        {
            var d = App.Current.Resources.MergedDictionaries[0][resourceKey];
            if (d is System.Windows.Media.SolidColorBrush brush)
            {
                var color = brush.Color;
                return Color.FromArgb(color.A, color.R, color.G, color.B);
            }
            return Color.Red;
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

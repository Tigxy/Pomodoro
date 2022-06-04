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
        private const int LAST_N_DAYS_TO_DISPLAY = 7;

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
            var today = DateTime.UtcNow.Date;

            // Calculate the day span of the statistics
            double[] days = Enumerable.Range(0, LAST_N_DAYS_TO_DISPLAY).Select(i => (double)i).ToArray();
            double[] studyDurations = new double[LAST_N_DAYS_TO_DISPLAY];
            double[] breakDurations = new double[LAST_N_DAYS_TO_DISPLAY];

            for (int i = 0; i < LAST_N_DAYS_TO_DISPLAY; i++) {
                var d = statistics.Where(ds => ds.Date.Date.Equals(today.AddDays(-i))).FirstOrDefault();
                var cur = ^(i + 1);
                studyDurations[cur] = d?.StudyDuration ?? 0; 
                breakDurations[cur] = studyDurations[cur] + (d?.BreakDuration ?? 0); 
            }

            // Overlay break with study duration
            var allExceptLast = ..^1;
            plt.PlotBar(days[allExceptLast], breakDurations[allExceptLast], label: "Break duration", fillColor: LoadColor("BoxplotColorBreak"), outlineWidth: 0);
            plt.PlotBar(days[allExceptLast], studyDurations[allExceptLast], label: "Study duration", fillColor: LoadColor("BoxplotColorStudying"), outlineWidth: 0);

            var lastDay = ^1..;
            // Plot current day differently to indicate that it may still change
            plt.PlotBar(days[lastDay], breakDurations[lastDay], 
                fillColor: LoadColor("BoxplotColorBreakToday"), outlineColor: Color.Transparent);
            plt.PlotBar(days[lastDay], studyDurations[lastDay], 
                fillColor: LoadColor("BoxplotColorStudyingToday"), outlineColor: Color.Transparent);

            var maxDuration = statistics.Count() > 0 ? breakDurations.Max() : 0;
            // Set minimum and maximum (otherwise plot looks ugly if only a few minutes of data exist)
            plt.Axis(y1: 0, y2: Math.Max(4, maxDuration * 1.2));


            var currentCulture = System.Globalization.CultureInfo.CurrentCulture;
            var dayNames = days[allExceptLast].Select(d => currentCulture.DateTimeFormat.GetDayName(today.AddDays(-d).Date.DayOfWeek))
                .ToList();
            dayNames.Reverse();
            dayNames.Add("Today");

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
        /// Populates the data base to have nice data to test the app with
        /// </summary>
        private void PopulateDB()
        {
            var dayCount = 6;
            var generationProfile = new Profile();
            var start = DateTime.Now.Subtract(TimeSpan.FromDays(dayCount)).Date;
            var end = DateTime.Now.Date;
            var currentDay = start;

            // First collect all entries and then add them
            var entries = new List<PeriodEntry>();

            var rng = new System.Random();
            while (currentDay.CompareTo(end) <= 0)
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

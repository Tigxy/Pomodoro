#undef UseWinFormsNotifications

using System;
using System.ComponentModel;
using System.Timers;
using Timer = System.Timers.Timer;

#if UseWinFormsNotifications
using System.Windows.Forms;
#else
using Notification.Wpf;
#endif

namespace Pomodoro
{
    public class PDService : INotifyPropertyChanged
    {
        private const int TickFrequency = 200;  // in ms, tick faster than what we display to increase responsiveness
        private const int ToolTipTimeout = (int)1e4; // in ms
        private bool _isTimerReset = true;  // initially, timer starts out reset

        public PDStatus Status { get; private set; }
        public PDConfig Config { get; private set; }
        private readonly Timer _ticker = new Timer(interval: TickFrequency);

        private DateTime _periodStartTime;
        private void UpdatePeriodStartTime() => _periodStartTime = DateTime.UtcNow;

#if UseWinFormsNotifications
        readonly NotifyIcon _notifyIcon = new NotifyIcon();
#else
        readonly NotificationManager _notificationMngr = new NotificationManager(App.Current.Dispatcher);
#endif

        public bool IsTimerReset
        {
            get => _isTimerReset;
            set { _isTimerReset = value; OnPropertyChanged(nameof(IsTimerReset)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        /// <summary>
        /// Default constructor
        /// </summary>
        public PDService()
        {
            // Register tick event
            _ticker.Elapsed += Ticker_Elapsed;

            Config = new PDConfig();
            Status = new PDStatus();
            Status.TimeRemaining = Config.GetPeriodTimespan(Status.PeriodType);
        }

        private void Ticker_Elapsed(object sender, ElapsedEventArgs e)
        {
            IsTimerReset = false;
            bool prevCompleteStatus = Status.IsPeriodComplete;
            Status.DecreaseTime(milliseconds: TickFrequency);

            // Only act the first time period was determined complete
            if (Status.IsPeriodComplete &&! prevCompleteStatus)
            {
                if (Status.IsStudying)
                    Status.CyclesDone++;
                if (Config.AutoSwitchModeAfterEnd)
                    ChangePeriod(GetNextPeriodType());
                NotifyPeriodOver(Status.IsTakingBreak);
            }
        }

        public void Start()
        {
            _ticker.Start();
            Status.IsPaused = false;
            LogStatus();
            UpdatePeriodStartTime();
        }

        public void Pause()
        {
            _ticker.Stop();
            Status.IsPaused = true;
            LogStatus();
            UpdatePeriodStartTime();
        }

        public void ToggleStartPause()
        {
            if (Status.IsPaused)
                Start();
            else
                Pause();
        }

        public void Stop()
        {
            _ticker.Stop();
            Status.IsPaused = true;
            Reset();
            UpdatePeriodStartTime();
        }

        public void Reset()
        {
            IsTimerReset = true;
            Status.TimeRemaining = Config.GetPeriodTimespan(Status.PeriodType);
            LogStatus();
            UpdatePeriodStartTime();
        }

        public PDPeriodType GetNextPeriodType()
        {
            if (Status.IsTakingBreak)
                return PDPeriodType.Studying;
            
            // Note that internally we even count breaks as a cycle and as after each study 
            // period, it follows a break period, we get twice the amount of cycles.
            if (Status.CyclesDone % Config.CyclesUntilLongBreak == 0)
                return PDPeriodType.LongBreak;

            return PDPeriodType.ShortBreak;
        }

        public void ChangePeriod(PDPeriodType periodType)
        {
            Status.PeriodType = periodType;
            Status.TimeRemaining = Config.GetPeriodTimespan(periodType);
            LogStatus();
            UpdatePeriodStartTime();
        }

        public void NotifyPeriodOver(bool isBreakPeriod)
        {
            System.Diagnostics.Debug.WriteLine($"Period over! Is break period? {isBreakPeriod}");

            // As the ticker is not using the UI thread but we would like to access some UI properties,
            // we need to dispatch our actions to them
            App.Current.Dispatcher.Invoke(() =>
            {
#if UseWinFormsNotifications
                _notifyIcon.ShowBalloonTip(
                    timeout: ToolTipTimeout, 
                    tipTitle: App.Current.MainWindow.Title, 
                    isBreakPeriod ? "Take a break!" : "Study!", 
                    ToolTipIcon.Info
                    );
#else
                _notificationMngr.Show(
                    title: App.Current.MainWindow.Title,
                    message: isBreakPeriod ? "Take a break!" : "Study!",
                    type: Notification.Wpf.NotificationType.Information,
                    expirationTime: TimeSpan.FromMilliseconds(ToolTipTimeout)
                    );
#endif
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        /// <summary>
        /// Logs the current status
        /// </summary>
        public void LogStatus()
        {
            System.Diagnostics.Debug.WriteLine(Status);

            var timeDifference = _periodStartTime == default ? 0 : DateTime.UtcNow.Subtract(_periodStartTime).TotalSeconds;
            
            // Remove milliseconds as this is useless information for us
            timeDifference = Math.Round(timeDifference);

            DBAccess.SavePeriodEntry(new DB.PeriodEntry()
            {
                StartTime = _periodStartTime == default ? DateTime.UtcNow : _periodStartTime,
                Duration = TimeSpan.FromSeconds(timeDifference),
                IsStudying = Status.IsStudying,
                IsPaused = Status.IsPaused
            });
        }
    }
}

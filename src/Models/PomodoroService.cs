using System;
using System.Timers;
using Notification.Wpf;
using Pomodoro.UI;

namespace Pomodoro.Models
{
    public class PomodoroService : BaseModel
    {
        private const int TickFrequency = 200;          // in ms, tick faster than what we display to increase responsiveness
        private const int ToolTipTimeout = (int)1e4;    // in ms
        private bool _isTimerReset = true;              // initially, timer starts out reset
        private Profile _profile;

        public Status Status { get; private set; }
        public Profile Profile
        {
            get => _profile;
            private set { _profile = value; OnPropertyChanged(nameof(_profile)); }
        }
        
        private readonly Timer _ticker;
        private DateTime _periodStartTime;
        
        private void UpdatePeriodStartTime() => _periodStartTime = DateTime.UtcNow;
        readonly NotificationManager _notificationMngr = new NotificationManager();

        #region UI properties and commands
        public bool IsTimerReset
        {
            get => _isTimerReset;
            set { _isTimerReset = value; OnPropertyChanged(nameof(IsTimerReset)); }
        }

        public RelayCommand ToggleStartPauseCommand { get; }
        public RelayCommand ResetCommand { get; }
        public RelayCommand StopCommand { get; }
        #endregion

        private static PomodoroService _instance;
        /// <summary>
        /// Singleton instance of <see cref="PomodoroService"/>
        /// </summary>
        public static PomodoroService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (new object())
                    {
                        if (_instance == null)
                            _instance = new PomodoroService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PomodoroService"/> class
        /// </summary>
        /// <param name="profile">The proflie to use</param>
        private PomodoroService()
        {
            _ticker = new Timer(interval: TickFrequency);

            // Register tick event
            _ticker.Elapsed += Ticker_Elapsed;

            StopCommand = new RelayCommand((p) => Stop());
            ResetCommand = new RelayCommand((p) => Reset());
            ToggleStartPauseCommand = new RelayCommand((p) => ToggleStartPause());

            Status = new Status();
            Profile = DBAccess.GetProfile("default") ?? new Profile();
            Profile.PropertyChanged += (s, e) => Restart();
            Restart();
        }

        /// <summary>
        /// Changes the profile to the specified one
        /// </summary>
        /// <param name="profile">The profile to change to</param>
        public void ChangeProfile(Profile profile)
        {
            Stop();
            Profile = profile;
            ChangePeriodType(PeriodType.Studying);
        }

        /// <summary>
        /// Callback of the ticker, used to determine how much time is left for the current period
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                if (Profile.AutoSwitchModeAfterEnd)
                    ChangePeriodType(GetNextPeriodType());
                NotifyPeriodOver(Status.IsTakingBreak);
            }
        }

        /// <summary>
        /// Toggles between a running and a paused period
        /// </summary>
        public void ToggleStartPause()
        {
            // Handle ticker accordingly
            if (Status.IsPaused)
                _ticker.Start();
            else
                _ticker.Stop();

            LogStatus();
            UpdatePeriodStartTime();
            Status.IsPaused ^= true;
        }

        /// <summary>
        /// Stops the ticker and period
        /// </summary>
        public void Stop()
        {
            _ticker.Stop();
            Status.IsPaused = true;
            Reset();
            UpdatePeriodStartTime();
        }

        /// <summary>
        /// Resets the time of the current period
        /// </summary>
        public void Reset()
        {
            IsTimerReset = true;
            Status.TimeRemaining = GetPeriodTimespan(Status.PeriodType);
        }

        /// <summary>
        /// Restarts the whole service
        /// </summary>
        private void Restart()
        {
            Stop(); 
            ChangePeriodType(PeriodType.Studying);
        }

        /// <summary>
        /// Determines what the next periods' type will be. 
        /// In general: after a pause, study period begins and vice versa
        /// </summary>
        /// <returns>The type the next period is expected to have</returns>
        public PeriodType GetNextPeriodType()
        {
            if (Status.IsTakingBreak)
                return PeriodType.Studying;
            
            if (Status.CyclesDone % Profile.CyclesUntilLongBreak == 0)
                return PeriodType.LongBreak;

            return PeriodType.ShortBreak;
        }

        /// <summary>
        /// Returns the corresponding timespan to the specified <see cref="PeriodType"/> based on the currently selected <see cref="Profile"/>
        /// </summary>
        /// <param name="periodType">The <see cref="PeriodType"/> to get the <see cref="TimeSpan"/> for</param>
        /// <returns>The to the <see cref="PeriodType"/> corresponding <see cref="TimeSpan"/></returns>
        public TimeSpan GetPeriodTimespan(PeriodType periodType)
        {
            int duration = periodType == PeriodType.Studying
                ? Profile.DurationStudying
                : periodType == PeriodType.ShortBreak
                ? Profile.DurationShortBreak : Profile.DurationLongBreak;

#if DEBUG
            return TimeSpan.FromSeconds(duration);
#else
            return TimeSpan.FromMinutes(duration);
#endif
        }

        /// <summary>
        /// Changes from the current period type (eg learning, ...) to the specified one
        /// </summary>
        /// <param name="periodType">The period type to change to</param>
        public void ChangePeriodType(PeriodType periodType)
        {
            // Ignore call if current and requested period type match
            if (Status.PeriodType == periodType)
                return;

            // Log current status
            LogStatus();

            // As new period starts, we also need to update the periods start time
            UpdatePeriodStartTime();

            // Update status
            Status.PeriodType = periodType;
            Status.TimeRemaining = GetPeriodTimespan(periodType);
        }

        /// <summary>
        /// Notifies the user that the current period is over (via ToastNotifications)
        /// </summary>
        /// <param name="isBreakPeriod">Whether break or study period is over</param>
        public void NotifyPeriodOver(bool isBreakPeriod)
        {
            // As the ticker is not using the UI thread but we would like to access some UI properties,
            // we need to dispatch our actions to them
            App.Current.Dispatcher.Invoke(() =>
            {
                _notificationMngr.Show(
                    title: App.Current.MainWindow.Title,
                    message: isBreakPeriod ? "Well, done, take a break!" : "Break is over, time to study!",
                    type: Notification.Wpf.NotificationType.Information,
                    expirationTime: TimeSpan.FromMilliseconds(ToolTipTimeout)
                    );
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        /// <summary>
        /// Logs the current status
        /// </summary>
        public void LogStatus()
        {
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
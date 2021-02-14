#define DEBUG_SHORT_TIME

using System;
using System.ComponentModel;
using System.Timers;
using Timer = System.Timers.Timer;
using Notification.Wpf;
using Pomodoro.UI;
using Pomodoro.DB;

namespace Pomodoro
{
    public class PDService : INotifyPropertyChanged
    {
        private const int TickFrequency = 200;          // in ms, tick faster than what we display to increase responsiveness
        private const int ToolTipTimeout = (int)1e4;    // in ms
        private bool _isTimerReset = true;              // initially, timer starts out reset

        public PDStatus Status { get; private set; }
        public Profile Profile { get; } = new Profile();
        
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        private static PDService _instance;
        /// <summary>
        /// Singleton instance of <see cref="PDService"/>
        /// </summary>
        public static PDService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (new object())
                    {
                        if (_instance == null)
                            _instance = new PDService();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PDService"/> class
        /// </summary>
        /// <param name="profile">The proflie to use</param>
        private PDService()
        {
            _ticker = new Timer(interval: TickFrequency);

            // Register tick event
            _ticker.Elapsed += Ticker_Elapsed;

            StopCommand = new RelayCommand((p) => Stop());
            ResetCommand = new RelayCommand((p) => Reset());
            ToggleStartPauseCommand = new RelayCommand((p) => ToggleStartPause());

            Status = new PDStatus();
            Profile.PropertyChanged += (s, e) => { Stop(); ChangePeriodType(PDPeriodType.Studying); };
        }

        /// <summary>
        /// Changes the profile to the specified one
        /// </summary>
        /// <param name="profile">The profile to change to</param>
        public void ChangeProfile(Profile profile)
        {
            Stop();

            // We need to copy all the properties rather than just assigning a new profile to 
            // ensure that the UI bindings are not being broken
            Utils.CopyProperties(profile, Profile);
            ChangePeriodType(PDPeriodType.Studying);
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
        /// Determines what the next periods' type will be. 
        /// In general: after a pause, study period begins and vice versa
        /// </summary>
        /// <returns>The type the next period is expected to have</returns>
        public PDPeriodType GetNextPeriodType()
        {
            if (Status.IsTakingBreak)
                return PDPeriodType.Studying;
            
            if (Status.CyclesDone % Profile.CyclesUntilLongBreak == 0)
                return PDPeriodType.LongBreak;

            return PDPeriodType.ShortBreak;
        }

        /// <summary>
        /// Returns the corresponding timespan to the specified <see cref="PDPeriodType"/> based on the currently selected <see cref="Profile"/>
        /// </summary>
        /// <param name="periodType">The <see cref="PDPeriodType"/> to get the <see cref="TimeSpan"/> for</param>
        /// <returns>The to the <see cref="PDPeriodType"/> corresponding <see cref="TimeSpan"/></returns>
        public TimeSpan GetPeriodTimespan(PDPeriodType periodType)
        {
            int duration = periodType == PDPeriodType.Studying
                ? Profile.DurationStudying
                : periodType == PDPeriodType.ShortBreak
                ? Profile.DurationShortBreak : Profile.DurationLongBreak;

#if DEBUG_SHORT_TIME
            return TimeSpan.FromSeconds(duration);
#else
            return TimeSpan.FromMinutes(duration);
#endif
        }

        /// <summary>
        /// Changes from the current period type (eg learning, ...) to the specified one
        /// </summary>
        /// <param name="periodType">The period type to change to</param>
        public void ChangePeriodType(PDPeriodType periodType)
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
            System.Diagnostics.Debug.WriteLine($"Period over! Is break period? {isBreakPeriod}");

            // As the ticker is not using the UI thread but we would like to access some UI properties,
            // we need to dispatch our actions to them
            App.Current.Dispatcher.Invoke(() =>
            {
                _notificationMngr.Show(
                    title: App.Current.MainWindow.Title,
                    message: isBreakPeriod ? "Take a break!" : "Study!",
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
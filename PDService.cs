using System.ComponentModel;
using System.Timers;

namespace Pomodoro
{
    public class PDService : INotifyPropertyChanged
    {
        private const int TickFrequency = 200;  // in ms, tick faster than what we display to increase responsiveness
        private bool _isTimerReset = true;  // initially, timer starts out reset

        public PDStatus Status { get; private set; }
        public PDConfig Config { get; private set; }
        private readonly Timer _ticker = new Timer(interval: TickFrequency);

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
        }

        public void Pause()
        {
            _ticker.Stop();
            Status.IsPaused = true;
            LogStatus();
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
        }

        public void Reset()
        {
            IsTimerReset = true;
            Status.TimeRemaining = Config.GetPeriodTimespan(Status.PeriodType);
            LogStatus();
        }

        public PDPeriodType GetNextPeriodType()
        {
            if (Status.IsTakingBreak)
                return PDPeriodType.Studying;
            
            // Note that internally we even count breaks as a cycle and as after each study 
            // period, it follows a break period, we get twice the amount of cycles.
            if ((Status.CyclesDone + 1) % (Config.CyclesUntilLongBreak * 2) == 0)
                return PDPeriodType.LongBreak;

            return PDPeriodType.ShortBreak;
        }

        public void ChangePeriod(PDPeriodType periodType)
        {
            Status.PeriodType = periodType;
            Status.TimeRemaining = Config.GetPeriodTimespan(periodType);
            LogStatus();
        }

        public void NotifyPeriodOver(bool isBreakPeriod)
        {
            System.Diagnostics.Debug.WriteLine($"Period over! Is break period? {isBreakPeriod}");
            //App.Current.Dispatcher.Invoke(
            //() =>
            //{
            //    if (IsStudying)
            //        ShowNotification("Good job studying - take a break now", NotificationType.Success);
            //    else
            //        ShowNotification("Break is over, lets study again", NotificationType.Information);
            //});
        }

        /// <summary>
        /// Logs the current status
        /// </summary>
        public void LogStatus()
        {
            System.Diagnostics.Debug.WriteLine(Status);
            // TODO: Implement logging
        }
    }
}

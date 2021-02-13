using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Pomodoro
{
    public class PDStatus : INotifyPropertyChanged
    {
        #region Fields
        private PDPeriodType _periodType = PDPeriodType.Studying;
        private bool _isPaused = true;
        private TimeSpan _timeRemaining;
        private uint _cyclesDone = 0;
        #endregion

        #region Properties
        public PDPeriodType PeriodType
        {
            get => _periodType;
            set { _periodType = value; OnPeriodTypeChanged(); }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set { _isPaused = value; OnPropertyChanged(nameof(IsPaused)); }
        }

        public TimeSpan TimeRemaining
        {
            get => _timeRemaining;
            set { _timeRemaining = value; OnTimeChanged(); }
        }

        public uint CyclesDone
        {
            get => _cyclesDone;
            set { _cyclesDone = value; OnPropertyChanged(nameof(CyclesDone)); }
        }
        #endregion

        #region Helper properties
        public bool IsStudying => PeriodType == PDPeriodType.Studying;
        public bool IsShortBreak => PeriodType == PDPeriodType.ShortBreak;
        public bool IsLongBreak => PeriodType == PDPeriodType.LongBreak;
        public bool IsTakingBreak => !IsStudying;
        public string STimeRemaining => ToTimeString(TimeRemaining);
        public bool IsPeriodComplete => TimeRemaining.TotalSeconds < 0;
        public string CurrentPeriod => IsStudying ? "Studying" : "Taking a break";
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        private void OnPeriodTypeChanged()
        {
            OnPropertyChanged(nameof(PeriodType));
            OnPropertyChanged(nameof(IsStudying));
            OnPropertyChanged(nameof(IsShortBreak));
            OnPropertyChanged(nameof(IsLongBreak));
            OnPropertyChanged(nameof(IsTakingBreak));
            OnPropertyChanged(nameof(CurrentPeriod));
        }

        private void OnTimeChanged()
        {
            OnPropertyChanged(nameof(TimeRemaining));
            OnPropertyChanged(nameof(STimeRemaining));
            OnPropertyChanged(nameof(IsPeriodComplete));
        }

        public void DecreaseOneSecond()
        {
            TimeRemaining = TimeRemaining.Subtract(TimeSpan.FromSeconds(1));
        }

        public void DecreaseTime(int minutes = 0, int seconds = 0, int milliseconds = 0)
        {
            TimeRemaining = TimeRemaining.Subtract(new TimeSpan(days: 0, hours: 0, minutes: minutes, seconds: seconds, milliseconds: milliseconds));
        }

        private string ToTimeString(TimeSpan timer)
        {
            // Ensure that timer was already initialized
            if (timer != null)
                return $"{(timer.TotalSeconds < 0 ? "-" : "")} {Math.Abs(timer.Minutes):D2}:{Math.Abs(timer.Seconds):D2}";
            return "00:00";
        }

        public override string ToString() => $"Action: {PeriodType}, IsPaused: {IsPaused}, TimeRemaining: {TimeRemaining}";
    }
}
